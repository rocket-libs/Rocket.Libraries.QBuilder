namespace Rocket.Libraries.Qurious
{

    using Rocket.Libraries.Qurious.Builders;
    using Rocket.Libraries.Qurious.Builders.Paging;
    using Rocket.Libraries.Qurious.Models;
    using Rocket.Libraries.Validation.Services;
    using System;

    /// <summary>
    /// Provides a fluent interface for building SQL queries with joins, filters, ordering, and grouping.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="QBuilder"/> class with the parameterization option.
        /// </summary>
        /// <param name="parameterize">Determines whether the query should be parameterized for secure parameter binding.</param>
        public QBuilder(bool parameterize)
            : this("t", parameterize)

        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QBuilder"/> class with the specified alias table name and parameterization option.
        /// </summary>
        /// <param name="aliasTablename">The alias for the main table in the query.</param>
        /// <param name="parameterize">Determines whether the query should be parameterized for secure parameter binding.</param>
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

        /// <summary>
        /// Gets the <see cref="SelectBuilder"/> instance for constructing the SELECT clause of the query.
        /// </summary>
        /// <returns>The <see cref="SelectBuilder"/> instance.</returns>
        public SelectBuilder UseSelector()
        {
            return _selectBuilder;
        }


        /// <summary>
        /// Gets the <see cref="TableBoundSelectBuilder{TTable}"/> instance for constructing the SELECT clause of the query for a specific table.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to select from.</typeparam>
        /// <returns>The <see cref="TableBoundSelectBuilder{TTable}"/> instance.</returns>
        public TableBoundSelectBuilder<TTable> UseTableBoundSelector<TTable>()
        {
            return new TableBoundSelectBuilder<TTable>(this, _selectBuilder);
        }

        /// <summary>
        /// Gets the <see cref="TableBoundGroupBuilder{TTable}"/> instance for constructing the GROUP BY clause of the query for a specific table.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to group by.</typeparam>
        /// <returns>The <see cref="TableBoundGroupBuilder{TTable}"/> instance.</returns>
        public TableBoundGroupBuilder<TTable> UseTableBoundGrouper<TTable>()
        {
            return new TableBoundGroupBuilder<TTable>(this);
        }

        /// <summary>
        /// Gets the <see cref="TableBoundJoinBuilder{TLeftTable, TRightTable}"/> instance for constructing a join between two tables.
        /// </summary>
        /// <typeparam name="TLeftTable">The type of the left table in the join.</typeparam>
        /// <typeparam name="TRightTable">The type of the right table in the join.</typeparam>
        /// <returns>The <see cref="TableBoundJoinBuilder{TLeftTable, TRightTable}"/> instance.</returns>
        public TableBoundJoinBuilder<TLeftTable, TRightTable> UseTableBoundJoinBuilder<TLeftTable, TRightTable>()
        {
            return new TableBoundJoinBuilder<TLeftTable, TRightTable>(this);
        }

        /// <summary>
        /// Gets the <see cref="TableBoundWhereBuilder{TTable}"/> instance for constructing the WHERE clause of the query for a specific table.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to filter.</typeparam>
        /// <returns>The <see cref="TableBoundWhereBuilder{TTable}"/> instance.</returns>
        public TableBoundWhereBuilder<TTable> UseTableBoundFilter<TTable>()
        {
            return new TableBoundWhereBuilder<TTable>(_whereBuilder, this);
        }

        /// <summary>
        /// Gets the <see cref="DerivedTableSelector"/> instance for selecting from a derived table.
        /// </summary>
        /// <param name="derivedTable">The <see cref="QBuilder"/> instance representing the derived table.</param>
        /// <returns>The <see cref="DerivedTableSelector"/> instance.</returns>
        public DerivedTableSelector UseDerivedTableSelector(QBuilder derivedTable)
        {
            return new DerivedTableSelector(derivedTable, _selectBuilder);
        }

        /// <summary>
        /// Gets the <see cref="IPagingBuilder{TTable}"/> instance for constructing paging logic using SQL Server syntax.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to apply paging to.</typeparam>
        /// <returns>The <see cref="IPagingBuilder{TTable}"/> instance.</returns>
        public IPagingBuilder<TTable> UseSqlServerPagingBuilder<TTable>()
        {
            return UsePagingBuilder<TTable, SqlServerPagingBuilder<TTable>>(new SqlServerPagingBuilder<TTable>(this));
        }

        /// <summary>
        /// Gets the <see cref="IPagingBuilder{TTable}"/> instance for constructing paging logic using MySQL Server syntax.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to apply paging to.</typeparam>
        /// <returns>The <see cref="IPagingBuilder{TTable}"/> instance.</returns>
        public IPagingBuilder<TTable> UseMySqlServerPagingBuilder<TTable>()
        {
            return UsePagingBuilder<TTable, MySqlServerPagingBuilder<TTable>>(new MySqlServerPagingBuilder<TTable>(this));
        }

        /// <summary>
        /// Gets the <see cref="IPagingBuilder{TTable}"/> instance for constructing paging logic using a custom paging builder.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to apply paging to.</typeparam>
        /// <typeparam name="TBuilderPagingBuilder">The type of the custom paging builder.</typeparam>
        /// <param name="pagingBuilder">The instance of the custom paging builder.</param>
        /// <returns>The <see cref="IPagingBuilder{TTable}"/> instance.</returns>
        public IPagingBuilder<TTable> UsePagingBuilder<TTable, TBuilderPagingBuilder>(TBuilderPagingBuilder pagingBuilder)
            where TBuilderPagingBuilder : BuilderBase, IPagingBuilder<TTable>
        {
            return pagingBuilder;
        }

        /// <summary>
        /// Gets the <see cref="JoinBuilder"/> instance for constructing joins between tables.
        /// </summary>
        /// <returns>The <see cref="JoinBuilder"/> instance.</returns>
        public JoinBuilder UseJoiner()
        {
            return _joinBuilder;
        }

        /// <summary>
        /// Gets the <see cref="WhereBuilder"/> instance for constructing the WHERE clause of the query.
        /// </summary>
        /// <returns>The <see cref="WhereBuilder"/> instance.</returns>
        public WhereBuilder UseFilter()
        {
            return _whereBuilder;
        }

        /// <summary>
        /// Gets the <see cref="OrderBuilder"/> instance for constructing the ORDER BY clause of the query.
        /// </summary>
        /// <returns>The <see cref="OrderBuilder"/> instance.</returns>
        public OrderBuilder UseOrdering()
        {
            return _orderBuilder;
        }

        /// <summary>
        /// Gets the <see cref="GroupBuilder"/> instance for constructing the GROUP BY clause of the query.
        /// </summary>
        /// <returns>The <see cref="GroupBuilder"/> instance.</returns>
        public GroupBuilder UseGrouper()
        {
            return _groupBuilder;
        }

        /// <summary>
        /// Builds the SQL query string based on the configured components (SELECT, JOIN, WHERE, GROUP BY, ORDER BY).
        /// </summary>
        /// <returns>The SQL query string.</returns>
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

        /// <summary>
        /// Builds the parameterized SQL query string based on the configured components (SELECT, JOIN, WHERE, GROUP BY, ORDER BY).
        /// </summary>
        /// <returns>The <see cref="BuiltQuery"/> object containing the parameterized SQL query string.</returns>
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