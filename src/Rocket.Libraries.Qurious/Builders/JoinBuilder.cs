namespace Rocket.Libraries.Qurious.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Rocket.Libraries.Qurious.Helpers;
    using Rocket.Libraries.Qurious.Models;
    using Rocket.Libraries.Validation.Services;

    public class JoinBuilder : BuilderBase
    {
        private FieldNameResolver _fieldNameResolver = new FieldNameResolver();
        private List<string> _alreadyAliasedTables = new List<string>();

        public JoinBuilder(QBuilder qBuilder)
            : base(qBuilder)
        {
        }

        internal string FirstTableName
        {
            get
            {
                var firstJoin = Joins.FirstOrDefault();
                DataValidator.EvaluateImmediate(() => firstJoin == null, "Could not find a join to use for first table");
                if (firstJoin.IsDerivedTableJoinDescription)
                {
                    return firstJoin.RightTable;
                }
                else
                {
                    return firstJoin.LeftTable;
                }
            }
        }

        internal bool JoinsExist => Joins.Count > 0;

        internal InnerSelectDescription InnerSelectDescription { get; set; }

        private List<JoinDescription> Joins { get; set; } = new List<JoinDescription>();

        public JoinBuilder InnerJoin<TLeftTable, TLeftField, TRightTable, TRightField>(Expression<Func<TLeftTable, TLeftField>> leftFieldNameDescriptor, Expression<Func<TRightTable, TRightField>> rightFieldNameDescriptor, string joinType)
        {
            QueueJoin(leftFieldNameDescriptor, rightFieldNameDescriptor, JoinTypes.Inner);
            return this;
        }

        public DerivedTableJoiner<TOuterTable> UseDerivedTableJoiner<TOuterTable>()
        {
            return new DerivedTableJoiner<TOuterTable>(this);
        }

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

        internal JoinBuilder InnerJoinDerivedTable<TRightTable, TRightField>(Expression<Func<TRightTable, TRightField>> rightFieldNameDescriptor, QBuilder derivedTable, string derivedFieldName)
        {
            var derivedTableJoinDescription = new DerivedTableJoinDescription
            {
                RightField = _fieldNameResolver.GetFieldName(rightFieldNameDescriptor),
                RightTable = QBuilder.TableNameResolver(typeof(TRightTable)),
                LeftField = derivedFieldName,
                QBuilder = derivedTable,
            };
            TranslateToJoinDescription(derivedTableJoinDescription);
            return this;
        }

        internal bool TableNotKnown(string table)
        {
            var match = Joins.FirstOrDefault(a => TableFoundInJoin(table, a));
            return match == null;
        }

        internal bool TableFoundInJoin(string table, JoinDescription joinDescription)
        {
            var leftTable = string.Empty;
            var rightTable = joinDescription.RightTable;
            if (joinDescription.IsDerivedTableJoinDescription)
            {
                leftTable = DerivedTableWrapperNameResolver.GetWrapperName(joinDescription.ExplicitLeftTableAlias);
            }
            else
            {
                leftTable = joinDescription.LeftTable;
            }

            return leftTable.Equals(table, StringComparison.CurrentCultureIgnoreCase)
            || rightTable.Equals(table, StringComparison.CurrentCultureIgnoreCase);
        }

        internal string Build()
        {
            _alreadyAliasedTables.Add(QBuilder.FirstTableName);
            var joins = string.Empty;
            foreach (var joinDescription in Joins)
            {
                joins += GetJoinLine(joinDescription);
            }

            Joins = new List<JoinDescription>();
            return joins;
        }

        private void TranslateToJoinDescription(DerivedTableJoinDescription derivedTableJoinDescription)
        {
            var joinDescription = new JoinDescription
            {
                JoinType = JoinTypes.Inner,
                RightField = derivedTableJoinDescription.RightField,
                RightTable = derivedTableJoinDescription.RightTable,
                LeftField = derivedTableJoinDescription.LeftField,
                ExplicitLeftTableAlias = derivedTableJoinDescription.QBuilder.DerivedTableName,
                DerivedTable = derivedTableJoinDescription.QBuilder.Build(),
            };
            Joins.Add(joinDescription);
        }

        private void QueueJoin<TLeftTable, TLeftField, TRightTable, TRightField>(Expression<Func<TLeftTable, TLeftField>> leftFieldNameDescriptor, Expression<Func<TRightTable, TRightField>> rightFieldNameDescriptor, string joinType)
        {
            var leftField = _fieldNameResolver.GetFieldName(leftFieldNameDescriptor);
            var rightField = _fieldNameResolver.GetFieldName(rightFieldNameDescriptor);
            QueueJoin<TRightField, TLeftTable>(leftField, rightField, joinType);
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

        private string GetLeftTableAlias(JoinDescription joinDescription)
        {
            var leftTableAlias = joinDescription.ExplicitLeftTableAlias;
            if (string.IsNullOrEmpty(leftTableAlias))
            {
                leftTableAlias = QBuilder.TableNameAliaser.GetTableAlias(joinDescription.LeftTable);
            }

            return leftTableAlias;
        }

        private string GetJoinLine(JoinDescription joinDescription)
        {
            if (joinDescription.IsDerivedTableJoinDescription)
            {
                return GetDerivedTableJoin(joinDescription);
            }
            else
            {
                return GetNonDerivedTableJoinLine(joinDescription);
            }
        }

        private string GetDerivedTableJoin(JoinDescription joinDescription)
        {
            var joinPrefix = GetJoinPrefix(joinDescription);
            var leftTableAlias = DerivedTableWrapperNameResolver.GetWrapperName(GetLeftTableAlias(joinDescription));

            var rightTableAlias = QBuilder.TableNameAliaser.GetTableAlias(joinDescription.RightTable);
            var line = $"join ({joinDescription.DerivedTable}) as {leftTableAlias} on {leftTableAlias}.{joinDescription.LeftField} = {rightTableAlias}.{joinDescription.RightField}";
            line += Environment.NewLine;
            return line;
        }

        private string GetNonDerivedTableJoinLine(JoinDescription joinDescription)
        {
            FlipTablesIfLeftTableAlreadyAliased(joinDescription);
            var joinPrefix = GetJoinPrefix(joinDescription);
            var rightTableAlias = QBuilder.TableNameAliaser.GetTableAlias(joinDescription.RightTable);
            var line = $"{joinPrefix}join {joinDescription.LeftTable} {QBuilder.TableNameAliaser.GetTableAlias(joinDescription.LeftTable)} on {QBuilder.TableNameAliaser.GetTableAlias(joinDescription.LeftTable)}.{joinDescription.LeftField}";
            line += $" = {rightTableAlias}.{joinDescription.RightField}{Environment.NewLine}";
            return line;
        }

        private void FlipTablesIfLeftTableAlreadyAliased(JoinDescription joinDescription)
        {
            var searchResult = _alreadyAliasedTables.FirstOrDefault(a => a.Equals(joinDescription.LeftTable, StringComparison.InvariantCultureIgnoreCase));
            var leftTableAlreadyAliased = searchResult != null;
            if (leftTableAlreadyAliased)
            {
                new JoinDescriptionFlipper().Flip(joinDescription);
            }
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