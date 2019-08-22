namespace Rocket.Libraries.Qurious.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Rocket.Libraries.Validation.Services;

    public class AggregateRowQuerierBuilder<THistoricalTable>
    {
        private const string LatestVersion = "LatestVersion";

        private string _foreignKeyName;

        private string _incrementingFieldName;

        private QBuilder _innerQBuilder;

        private QBuilder _resultQbuilder;

        private Action _doDerivedTableJoin;

        private Action _doFilter;

        private string _aggregationFunction;

        private Func<Type, string> _tableNameResolver;

        private string _aliasTablename;
        private FilterOperator _filterOperator;

        public AggregateRowQuerierBuilder()
            : this("t")

        {
        }

        public AggregateRowQuerierBuilder(string aliasTablename)
            : this(t => t.Name, aliasTablename)

        {
        }
        

        public AggregateRowQuerierBuilder(Func<Type, string> tableNameResolver, string aliasTablename)
        {
            _tableNameResolver = tableNameResolver;
            _aliasTablename = aliasTablename;
        }

        private QBuilder InnerQBuilder
        {
            get
            {
                if (_innerQBuilder == null)
                {
                    _innerQBuilder = new QBuilder(_tableNameResolver, _aliasTablename);
                }

                return _innerQBuilder;
            }
        }

        private QBuilder ResultQbuilder
        {
            get
            {
                if (_resultQbuilder == null)
                {
                    _resultQbuilder = new QBuilder(_tableNameResolver, _aliasTablename);
                }

                return _resultQbuilder;
            }
        }

        public AggregateRowQuerierBuilder<THistoricalTable> SetAggregationFunction(string aggregationFunction)
        {
            _aggregationFunction = aggregationFunction;
            return this;
        }

        

        public AggregateRowQuerierBuilder<THistoricalTable> SetGroupingValue<TValueType>(TValueType groupingValue)
        {
            FailIfFilterAlreadySet();
            _doFilter = () => 
            {
                InnerQBuilder.
                UseFilter()
                    .Where<THistoricalTable>(_foreignKeyName, FilterOperator.EqualTo,groupingValue);
                ResultQbuilder.
                UseFilter()
                    .Where<THistoricalTable>(_foreignKeyName, FilterOperator.EqualTo,groupingValue);
            };
            return this;
        }

        public AggregateRowQuerierBuilder<THistoricalTable> SetGroupingValueAsList<TValueType>(List<TValueType> groupingValues)
        {
            FailIfFilterAlreadySet();
            _doFilter = () => 
            {
                InnerQBuilder.
                UseFilter()
                    .WhereIn<THistoricalTable, TValueType>(_foreignKeyName, groupingValues);
                ResultQbuilder.
                UseFilter()
                    .WhereIn<THistoricalTable, TValueType>(_foreignKeyName, groupingValues);
            };
            return this; 
        }

        private void FailIfFilterAlreadySet()
        {
            new DataValidator()
                .EvaluateImmediate(() => _doFilter != null,"Grouping has already been set. Grouping can only be set once.");
        }

        public AggregateRowQuerierBuilder<THistoricalTable> SetForeignKeyResolver<TField>(Expression<Func<THistoricalTable, TField>> foreignKeyResolver)
        {
            _foreignKeyName = new FieldNameResolver().GetFieldName(foreignKeyResolver);
            return this;
        }

        public AggregateRowQuerierBuilder<THistoricalTable> SetIncrementingFieldName<TField>(Expression<Func<THistoricalTable, TField>> incrementingFieldNameResolver)
        {
            _incrementingFieldName = new FieldNameResolver().GetFieldName(incrementingFieldNameResolver);
            CacheDerivedTableJoinCall(incrementingFieldNameResolver);
            return this;
        }

        public QBuilder Build()
        {
            FailIfInvalid();
            InnerQBuilder
             .UseSelector()
             .Select<THistoricalTable>(_foreignKeyName)
             .SelectAggregated<THistoricalTable>(_incrementingFieldName, LatestVersion, _aggregationFunction)
             .Then()
             .UseGrouper()
             .GroupBy<THistoricalTable>(_foreignKeyName);

            ResultQbuilder
                .UseSelector()
                .Select<THistoricalTable>("*")
                .Then();

            _doDerivedTableJoin();
            _doFilter();
            _doFilter = null;
            _doDerivedTableJoin = null;
            return ResultQbuilder;
        }

        private void FailIfInvalid()
        {
            new DataValidator()
                .AddFailureCondition(() => _doFilter == null, "No grouping value was specified, cannot generate query as can't tell which records qualify for comparison.", false)
                .AddFailureCondition(() => string.IsNullOrEmpty(_foreignKeyName), $"Foreign key was not specified, cannot determine how to identify records to compare.", false)
                .AddFailureCondition(() => string.IsNullOrEmpty(_incrementingFieldName), $"Cannot determine which field is being incremented. No way to tell which record is the latest.", false)
                .AddFailureCondition(() => string.IsNullOrEmpty(_aggregationFunction), $"No aggregate function specified. Cannot query database", false)
                .AddFailureCondition(() => _filterOperator == default,$"Filter operator has not been set",false)
                .ThrowExceptionOnInvalidRules();
        }

        private void CacheDerivedTableJoinCall<TField>(Expression<Func<THistoricalTable, TField>> incrementingFieldNameResolver)
        {
            _doDerivedTableJoin = () =>
            {
                ResultQbuilder
                .UseJoiner()
                .UseDerivedTableJoiner<THistoricalTable>()
                .InnerJoin(incrementingFieldNameResolver, InnerQBuilder, LatestVersion);
            };
        }

        
    }
}