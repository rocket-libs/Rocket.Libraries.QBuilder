namespace Rocket.Libraries.Qurious
{
    using System;
    using Rocket.Libraries.Qurious.Builders;
    using Rocket.Libraries.Qurious.Models;
    using Rocket.Libraries.Validation.Services;

    public class QBuilder
    {
        private readonly SelectBuilder _selectBuilder;
        private readonly OrderBuilder _orderBuilder;
        private readonly JoinBuilder _joinBuilder;
        private readonly WhereBuilder _whereBuilder;
        private readonly string _derivedTableName;
        private readonly GroupBuilder _groupBuilder;

        public QBuilder()
            : this("t")

        {
        }

        public QBuilder(string aliasTablename)
            : this(t => t.Name, aliasTablename)

        {
        }

        public QBuilder(Func<Type, string> tableNameResolver, string aliasTablename)
        {
            TableNameResolver = tableNameResolver;
            _derivedTableName = aliasTablename;
            TableNameAliaser = new TableNameAliaser(tableNameResolver);
            _orderBuilder = new OrderBuilder(this);
            _selectBuilder = new SelectBuilder(this, "t");
            _joinBuilder = new JoinBuilder(this);
            _whereBuilder = new WhereBuilder(this);
            _groupBuilder = new GroupBuilder(this);
        }

        public string DerivedTableName => _derivedTableName;

        internal TableNameAliaser TableNameAliaser { get; set; }

        internal InnerSelectDescription InnerSelectDescription { get; set; }

        internal Func<Type, string> TableNameResolver { get; set; }

        internal string FirstTableName
        {
            get
            {
                if (UseJoiner().JoinsExist == false)
                {
                    return UseSelector().FirstTableName;
                }
                else
                {
                    return UseJoiner().FirstTableName;
                }
            }
        }

        private DataValidator DataValidator { get; } = new DataValidator();

        public SelectBuilder UseSelector()
        {
            return _selectBuilder;
        }

        public TableBoundSelectBuilder<TTable> UseTableBoundSelector<TTable>()
        {
            return new TableBoundSelectBuilder<TTable>(this, _selectBuilder);
        }

        public TableBoundGroupBuilder<TTable> UseTableBoundGrouper<TTable>()
        {
            return new TableBoundGroupBuilder<TTable>(this);
        }

        public TableBoundJoinBuilder<TLeftTable, TRightTable> UseTableBoundJoinBuilder<TLeftTable, TRightTable>()
        {
            return new TableBoundJoinBuilder<TLeftTable, TRightTable>(this);
        }

        public TableBoundWhereBuilder<TTable> UseTableBoundFilter<TTable>()
        {
            return new TableBoundWhereBuilder<TTable>(_whereBuilder, this);
        }

        public JoinBuilder UseJoiner()
        {
            return _joinBuilder;
        }

        public WhereBuilder UseFilter()
        {
            return _whereBuilder;
        }

        public OrderBuilder UseOrdering()
        {
            return _orderBuilder;
        }

        public GroupBuilder UseGrouper()
        {
            return _groupBuilder;
        }

        public string Build()
        {
            DataValidator.EvaluateImmediate(() => string.IsNullOrEmpty(FirstTableName), "There are no tables queued for data querying. Nothing to return");

            var query = UseSelector().Build()
                + UseJoiner().Build()
                + UseFilter().Build()
                + UseGrouper().Build()
                + UseOrdering().Build();
            var wrappedQuery = GetWrappedInSelectAlias(query);
            var finalQuery = GetWithInnerSelectJoinIfRequired(wrappedQuery);
            return finalQuery;
        }

        public JoinBuilder FinishJoinToDerivedTable()
        {
            var noInnerDescription = InnerSelectDescription == null;

            if (noInnerDescription)
            {
                throw new Exception($"You are not in a '{nameof(JoinBuilder.BeginInnerJoinToDerivedTable)}' section. Did you mean to call '{nameof(JoinBuilder.Then)}' instead?");
            }

            return InnerSelectDescription.Parent;
        }

        private string GetWrappedInSelectAlias(string query)
        {
            var result = $"Select * from ({query}) as {DerivedTableName}";
            var isDerivedTable = InnerSelectDescription != null;
            if (isDerivedTable)
            {
                result += "INNER";
                result = $"({result}) as {DerivedTableName}";
            }

            result += $"{Environment.NewLine}";
            return result;
        }

        private string GetWithInnerSelectJoinIfRequired(string query)
        {
            var joiner = UseJoiner();
            if (joiner.InnerSelectDescription == null)
            {
                return query;
            }
            else
            {
                var innerSelect = joiner.InnerSelectDescription;
                var derivedTableQuery = innerSelect.QBuilder.Build();
                var resultQuery = $"{query} Join {derivedTableQuery}"
                    + $" on {innerSelect.DerivedTableName}.{innerSelect.InnerField} = {DerivedTableName}.{innerSelect.Field}";
                return resultQuery;
            }
        }
    }
}