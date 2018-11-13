namespace Rocket.Libraries.JoinBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Rocket.Libraries.JoinBuilder.Builders;
    using Rocket.Libraries.Validation.Services;

    public class JoinBuilder
    {
        private List<JoinDescription> _joins = new List<JoinDescription>();
        private List<WhereDescription> _wheres = new List<WhereDescription>();
        private TableNameAliaser _tableNameAliaser;
        private bool _distinctAllFields = false;
        private OrderBuilder _orderBuilder;
        private SelectBuilder _selectBuilder;

        private DataValidator DataValidator { get; } = new DataValidator();
        internal TableNameAliaser TableNameAliaser { get => _tableNameAliaser; set => _tableNameAliaser = value; }
        internal Func<Type, string> TableNameResolver { get => _tableNameResolver; set => _tableNameResolver = value; }
        internal List<JoinDescription> Joins { get => _joins; set => _joins = value; }

        private Func<Type, string> _tableNameResolver;

        public JoinBuilder()
            : this(t => t.Name)

        {
        }

        public JoinBuilder(Func<Type, string> tableNameResolver)
        {
            TableNameResolver = tableNameResolver;
            TableNameAliaser = new TableNameAliaser(tableNameResolver);
            _orderBuilder = new OrderBuilder(this);
            _selectBuilder = new SelectBuilder(this);
        }

        public JoinBuilder InnerJoin<TLeftTable, TRightTable>(string leftField, string rightField)
        {
            QueueJoin<TLeftTable, TRightTable>(leftField, rightField, JoinTypes.Inner);
            return this;
        }

        public JoinBuilder OrderByDescending<TTable>(string field)
        {
            return _orderBuilder.OrderByDescending<TTable>(field);
        }

        public SelectBuilder Selector()
        {
            return _selectBuilder;
        }

        public JoinBuilder FullJoin<TLeftTable, TRightTable>(string leftField, string rightField)
        {
            QueueJoin<TLeftTable, TRightTable>(leftField, rightField, JoinTypes.Full);
            return this;
        }

        

        public JoinBuilder Where<TTable>(string field, string condition)
        {
            _wheres.Add(new WhereDescription
            {
                Clause = $"{TableNameAliaser.GetTableAlias(TableNameResolver(typeof(TTable)))}.{field} {condition}"
            });
            return this;
        }

        public JoinBuilder WhereIn<TTable, TValueType>(string field, List<TValueType> values)
        {
            var criteria = GetWhereInSectionArguments(values);

            if (string.IsNullOrEmpty(criteria))
            {
                return this;
            }
            else
            {
                return Where<TTable>(field, $" in {criteria}");
            }
        }

        public JoinBuilder WhereExplicitly(string criteria)
        {
            _wheres.Add(new WhereDescription
            {
                Clause = criteria
            });
            return this;
        }

        public string Build()
        {
            DataValidator.EvaluateImmediate(() => Joins.Count == 0, "There are no tables queued for joining. Nothing to return");

            var query = _selectBuilder.Build()
                + GetJoinClause()
                + GetWhereClause()
                + _orderBuilder.Build();
            return query;
        }

        private void QueueJoin<TLeftTable, TRightTable>(string leftField, string rightField, string joinType)
        {
            Joins.Add(new JoinDescription
            {
                LeftField = leftField,
                LeftTable = TableNameResolver(typeof(TLeftTable)),
                RightField = rightField,
                RightTable = TableNameResolver(typeof(TRightTable)),
                JoinType = joinType,
            });
        }

        private string GetWhereClause()
        {
            var where = string.Empty;
            foreach (var whereDescription in _wheres)
            {
                if (!string.IsNullOrEmpty(where))
                {
                    where += "and ";
                }
                where += $"{whereDescription.Clause}{Environment.NewLine}";
            }

            _wheres = new List<WhereDescription>();
            if (string.IsNullOrEmpty(where))
            {
                return string.Empty;
            }
            else
            {
                return $"Where {where}";
            }
        }

        private string GetJoinClause()
        {
            var joins = string.Empty;
            foreach (var joinDescription in Joins)
            {
                joins += GetJoinLine(joinDescription);
            }

            Joins = new List<JoinDescription>();
            return joins;
        }

        private string GetJoinLine(JoinDescription joinDescription)
        {
            var joinPrefix = GetJoinPrefix(joinDescription);
            var line = $"{joinPrefix}join {joinDescription.LeftTable} {TableNameAliaser.GetTableAlias(joinDescription.LeftTable)} on {TableNameAliaser.GetTableAlias(joinDescription.LeftTable)}.{joinDescription.LeftField}";
            line += $" = {TableNameAliaser.GetTableAlias(joinDescription.RightTable)}.{joinDescription.RightField}{Environment.NewLine}";
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
            }
            return string.Empty;
        }

        private string GetWhereInSectionArguments<TValueType>(List<TValueType> values)
        {
            if (values == null)
            {
                throw new Exception("Cannot build a where clause from an null list of values");
            }
            if (values.Count == 0)
            {
                return string.Empty;
            }
            var args = string.Empty;
            foreach (var value in values)
            {
                args += $",'{value}'";
            }
            args = $"({args.Substring(1)})";
            return args;
        }
    }
}