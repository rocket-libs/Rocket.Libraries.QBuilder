namespace Rocket.Libraries.Qurious
{
    using Rocket.Libraries.QBuilder;
    using Rocket.Libraries.Qurious.Builders;
    using Rocket.Libraries.Qurious.Builders.Paging;
    using Rocket.Libraries.Qurious.Models;
    using Rocket.Libraries.Validation.Services;
    using System;

    public class QBuilder : IDisposable
    {
        private SelectBuilder _selectBuilder;

        private OrderBuilder _orderBuilder;

        private JoinBuilder _joinBuilder;

        private WhereBuilder _whereBuilder;

        internal string _aliasTableName;

        private GroupBuilder _groupBuilder;

        private string _suffix;

        private BuiltQuery builtQuery;

        public QBuilder(bool parameterize)
            : this("t", parameterize)

        {
        }

        public QBuilder(string aliasTablename, bool parameterize)
            : this(t => t.Name, aliasTablename, parameterize)

        {
        }

        public QBuilder(Func<Type, string> tableNameResolver, string aliasTablename, bool parameterize)
        {
            TableNameResolver = tableNameResolver;
            _aliasTableName = aliasTablename;
            TableNameAliaser = new TableNameAliaser(tableNameResolver);
            _orderBuilder = new OrderBuilder(this);
            _selectBuilder = new SelectBuilder(this, "t");
            _joinBuilder = new JoinBuilder(this);
            _groupBuilder = new GroupBuilder(this);
            if (parameterize)
            {
                builtQuery = new BuiltQuery();
            }
            _whereBuilder = new WhereBuilder(this, builtQuery);
        }

        internal void SetSuffix(string suffix)
        {
            _suffix = suffix;
        }

        public string DerivedTableName => _aliasTableName;

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

        public DerivedTableSelector UseDerivedTableSelector(QBuilder derivedTable)
        {
            return new DerivedTableSelector(derivedTable, _selectBuilder);
        }

        public IPagingBuilder<TTable> UseSqlServerPagingBuilder<TTable>()
        {
            return UsePagingBuilder<TTable, SqlServerPagingBuilder<TTable>>(new SqlServerPagingBuilder<TTable>(this));
        }

        public IPagingBuilder<TTable> UseMySqlServerPagingBuilder<TTable>()
        {
            return UsePagingBuilder<TTable, MySqlServerPagingBuilder<TTable>>(new MySqlServerPagingBuilder<TTable>(this));
        }

        public IPagingBuilder<TTable> UsePagingBuilder<TTable, TBuilderPagingBuilder>(TBuilderPagingBuilder pagingBuilder)
            where TBuilderPagingBuilder : BuilderBase, IPagingBuilder<TTable>
        {
            return pagingBuilder;
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
            DataValidator.EvaluateImmediate(string.IsNullOrEmpty(FirstTableName), "There are no tables queued for data querying. Nothing to return");

            var query = UseSelector().Build()
                + UseJoiner().Build()
                + UseFilter().Build()
                + UseGrouper().Build()
                + UseOrdering().Build();
            var wrappedQuery = GetWrappedInSelectAlias(query);
            var finalQuery = GetWithInnerSelectJoinIfRequired(wrappedQuery);
            var suffixedQuery = finalQuery + " " + _suffix;
            return suffixedQuery.Trim();
        }

        public BuiltQuery BuildWithParameters()
        {
            builtQuery.ParameterizedSql = Build();
            return builtQuery;
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

        public override string ToString()
        {
            var hasAlias = !string.IsNullOrEmpty(_aliasTableName);
            if (hasAlias)
            {
                return _aliasTableName;
            }
            else
            {
                return base.ToString();
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _selectBuilder = null;
                    _orderBuilder = null;
                    _joinBuilder = null;
                    _whereBuilder = null;
                    _aliasTableName = string.Empty;
                    _groupBuilder = null;
                    _suffix = string.Empty;
                    TableNameResolver = null;
                    TableNameAliaser = null;

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~QBuilder() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}