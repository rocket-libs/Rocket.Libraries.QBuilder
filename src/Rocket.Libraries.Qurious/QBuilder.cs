namespace Rocket.Libraries.Qurious
{
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
            _aliasTableName = aliasTablename;
            TableNameAliaser = new TableNameAliaser(tableNameResolver);
            _orderBuilder = new OrderBuilder(this);
            _selectBuilder = new SelectBuilder(this, "t");
            _joinBuilder = new JoinBuilder(this);
            _whereBuilder = new WhereBuilder(this);
            _groupBuilder = new GroupBuilder(this);
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

        public SqlServerPagingBuilder<TTable> UseSqlServerPagingBuilder<TTable>()
        {
            return new SqlServerPagingBuilder<TTable>(this);
        }

        public MySqlServerPagingBuilder<TTable> UseMySqlServerPagingBuilder<TTable>()
        {
            return new MySqlServerPagingBuilder<TTable>(this);
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

        [Obsolete("Clumsy. don't use. Instead use the 'UseDerivedTableJoiner' in the Joiner(s)")]
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