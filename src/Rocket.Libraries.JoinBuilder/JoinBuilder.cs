namespace Rocket.Libraries.JoinBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Rocket.Libraries.Validation.Services;

    public class JoinBuilder
    {
        private List<JoinDescription> _joins = new List<JoinDescription>();
        private List<SelectDescription> _selects = new List<SelectDescription>();
        private List<WhereDescription> _wheres = new List<WhereDescription>();
        private TableNameAliaser _tableNameAliaser;
        private bool _distinctAllFields = false;

        private DataValidator DataValidator { get; } = new DataValidator();

        private Func<Type, string> _tableNameResolver;

        public JoinBuilder()
            : this(t => t.Name)

        {
        }

        public JoinBuilder(Func<Type, string> tableNameResolver)
        {
            _tableNameResolver = tableNameResolver;
            _tableNameAliaser = new TableNameAliaser(tableNameResolver);
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

        public JoinBuilder Select<TTable>(string field)
        {
            return Select<TTable>(field, string.Empty);
        }

        public JoinBuilder Select<TTable>(string field, string fieldAlias)
        {
            _selects.Add(new SelectDescription
            {
                Field = field,
                Table = _tableNameResolver(typeof(TTable)),
                FieldAlias = fieldAlias
            });
            return this;
        }

        public JoinBuilder Where<TTable>(string field, string condition)
        {
            _wheres.Add(new WhereDescription
            {
                Clause = $"{_tableNameAliaser.GetTableAlias(_tableNameResolver(typeof(TTable)))}.{field} {condition}"
            });
            return this;
        }

        public JoinBuilder WhereExplicitly(string criteria)
        {
            _wheres.Add(new WhereDescription
            {
                Clause = criteria
            });
            return this;
        }

        public JoinBuilder SelectDistinctAll()
        {
            _distinctAllFields = true;
            return this;
        }

        public string Build()
        {
            DataValidator.EvaluateImmediate(() => _selects.Count == 0, "There are no fields queued for selection. Nothing to return");
            DataValidator.EvaluateImmediate(() => _joins.Count == 0, "There are no tables queued for joining. Nothing to return");

            var query = GetSelectClause()
                + GetJoinClause()
                + GetWhereClause();
            return query;
        }

        private void QueueJoin<TLeftTable, TRightTable>(string leftField, string rightField, string joinType)
        {
            _joins.Add(new JoinDescription
            {
                LeftField = leftField,
                LeftTable = _tableNameResolver(typeof(TLeftTable)),
                RightField = rightField,
                RightTable = _tableNameResolver(typeof(TRightTable)),
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

        private string GetSelectClause()
        {
            var selects = "Select ";
            if (_distinctAllFields)
            {
                selects += $" Distinct {Environment.NewLine}";
            }

            foreach (var selectDescription in _selects)
            {
                DataValidator.EvaluateImmediate(() => TableNotKnown(selectDescription.Table), $"Table '{selectDescription.Table}' has not been queued as a datasource. Cannot show fields from it");
                selects += $"{Environment.NewLine}{_tableNameAliaser.GetTableAlias(selectDescription.Table)}.{selectDescription.Field}";
                var hasAlias = !string.IsNullOrEmpty(selectDescription.FieldAlias);
                if (hasAlias)
                {
                    selects += $" as {selectDescription.FieldAlias}";
                }

                selects += ",";
            }

            selects = selects.Substring(0, selects.Length - 1) + $" From {_joins.First().RightTable} " + _tableNameAliaser.GetTableAlias(_joins.First().RightTable);
            _selects = new List<SelectDescription>();
            return selects + Environment.NewLine;
        }

        private string GetJoinClause()
        {
            var joins = string.Empty;
            foreach (var joinDescription in _joins)
            {
                joins += GetJoinLine(joinDescription);
            }

            _joins = new List<JoinDescription>();
            return joins;
        }

        private string GetJoinLine(JoinDescription joinDescription)
        {
            var joinPrefix = GetJoinPrefix(joinDescription);
            var line = $"{joinPrefix}join {joinDescription.LeftTable} {_tableNameAliaser.GetTableAlias(joinDescription.LeftTable)} on {_tableNameAliaser.GetTableAlias(joinDescription.LeftTable)}.{joinDescription.LeftField}";
            line += $" = {_tableNameAliaser.GetTableAlias(joinDescription.RightTable)}.{joinDescription.RightField}{Environment.NewLine}";
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

        private bool TableNotKnown(string table)
        {
            var occurenceCount = _joins.Count(a => a.LeftTable.Equals(table, StringComparison.CurrentCultureIgnoreCase)
            || a.RightTable.Equals(table, StringComparison.CurrentCultureIgnoreCase));
            return occurenceCount == 0;
        }
    }
}