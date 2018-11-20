namespace Rocket.Libraries.Qurious.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Rocket.Libraries.Qurious.Builders;
    using Rocket.Libraries.Qurious.Models;
    using Rocket.Libraries.Validation.Services;

    public class JoinBuilder : BuilderBase
    {
        public JoinBuilder(QBuilder qBuilder)
            : base(qBuilder)
        {
        }

        internal string FirstTableName => Joins.First().RightTable;

        internal bool JoinsExist => Joins.Count > 0;

        internal InnerSelectDescription InnerSelectDescription { get; set; }

        private List<JoinDescription> Joins { get; set; } = new List<JoinDescription>();

        public JoinBuilder InnerJoin<TLeftTable, TRightTable>(string leftField, string rightField)
        {
            QueueJoin<TLeftTable, TRightTable>(leftField, rightField, JoinTypes.Inner);
            return this;
        }

        public JoinBuilder FullJoin<TLeftTable, TRightTable>(string leftField, string rightField)
        {
            QueueJoin<TLeftTable, TRightTable>(leftField, rightField, JoinTypes.Full);
            return this;
        }

        public JoinBuilder LeftJoin<TLeftTable, TRightTable>(string leftField, string rightField)
        {
            QueueJoin<TLeftTable, TRightTable>(leftField, rightField, JoinTypes.LeftJoin);
            return this;
        }

        public JoinBuilder RightJoin<TLeftTable, TRightTable>(string leftField, string rightField)
        {
            QueueJoin<TLeftTable, TRightTable>(leftField, rightField, JoinTypes.RightJoin);
            return this;
        }

        public QBuilder BeginInnerJoinToDerivedTable(string derivedTableName, string innerField, string field)
        {
            InnerSelectDescription = new InnerSelectDescription
            {
                Field = field,
                InnerField = innerField,
                QBuilder = new QBuilder(QBuilder.TableNameResolver, derivedTableName),
                Parent = this,
                DerivedTableName = derivedTableName,
            };
            InnerSelectDescription.QBuilder.InnerSelectDescription = InnerSelectDescription;
            return InnerSelectDescription.QBuilder;
        }

        public override QBuilder Then()
        {
            DataValidator.EvaluateImmediate(() => QBuilder.InnerSelectDescription != null, $"You are currently in a '{nameof(BeginInnerJoinToDerivedTable)}' section. Please call '{nameof(QBuilder.FinishJoinToDerivedTable)}' instead");
            return base.Then();
        }

        internal bool TableNotKnown(string table)
        {
            var occurenceCount = Joins.Count(a => a.LeftTable.Equals(table, StringComparison.CurrentCultureIgnoreCase)
            || a.RightTable.Equals(table, StringComparison.CurrentCultureIgnoreCase));
            return occurenceCount == 0;
        }

        internal string Build()
        {
            var joins = string.Empty;
            foreach (var joinDescription in Joins)
            {
                joins += GetJoinLine(joinDescription);
            }

            Joins = new List<JoinDescription>();
            return joins;
        }

        private void QueueJoin<TLeftTable, TRightTable>(string leftField, string rightField, string joinType)
        {
            Joins.Add(new JoinDescription
            {
                LeftField = leftField,
                LeftTable = QBuilder.TableNameResolver(typeof(TLeftTable)),
                RightField = rightField,
                RightTable = QBuilder.TableNameResolver(typeof(TRightTable)),
                JoinType = joinType,
            });
        }

        private string GetJoinLine(JoinDescription joinDescription)
        {
            var joinPrefix = GetJoinPrefix(joinDescription);
            var line = $"{joinPrefix}join {joinDescription.LeftTable} {QBuilder.TableNameAliaser.GetTableAlias(joinDescription.LeftTable)} on {QBuilder.TableNameAliaser.GetTableAlias(joinDescription.LeftTable)}.{joinDescription.LeftField}";
            line += $" = {QBuilder.TableNameAliaser.GetTableAlias(joinDescription.RightTable)}.{joinDescription.RightField}{Environment.NewLine}";
            return line;
        }

        private string GetJoinPrefix(JoinDescription joinDescription)
        {
            switch (joinDescription.JoinType)
            {
                default:
                    var joinTypeIsUnsupported = true;
                    DataValidator.EvaluateImmediate(() => joinTypeIsUnsupported, $"Unsupported join type '{joinDescription.JoinType}'");
                    break;

                case JoinTypes.Full:
                    return "Full Outer ";

                case JoinTypes.Inner:
                    return string.Empty;
                case JoinTypes.RightJoin:
                    return "Right ";
                case JoinTypes.LeftJoin:
                    return "Left ";
            }
            return string.Empty;
        }
    }
}