namespace Rocket.Libraries.Qurious.Builders
{
    using Rocket.Libraries.Qurious.Helpers;
    using Rocket.Libraries.Qurious.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class TableBoundWhereBuilder<TTable> : BuilderBase
    {
        private WhereBuilder _whereBuilder;
        private FieldNameResolver _fieldNameResolver;

        public TableBoundWhereBuilder(WhereBuilder whereBuilder, QBuilder qBuilder)
            : base(qBuilder)
        {
            _whereBuilder = whereBuilder;
            _fieldNameResolver = new FieldNameResolver();
        }

        public WhereConjuntionBuilder Where(FilterDescription<TTable> filterDescription)
        {
            return _whereBuilder.Where(filterDescription);
        }

        public WhereConjuntionBuilder Where<TField>(Expression<Func<TTable, TField>> fieldNameDescriptor, string condition)
        {
            var fieldName = _fieldNameResolver.GetFieldName(fieldNameDescriptor);
            return _whereBuilder.Where<TTable>(fieldName, condition);
        }

        public WhereConjuntionBuilder Where<TField>(Expression<Func<TTable, TField>> fieldNameDescriptor, FilterOperator op, object value)
        {
            var fieldName = _fieldNameResolver.GetFieldName(fieldNameDescriptor);
            return _whereBuilder.Where<TTable>(fieldName, op, value);
        }

        public WhereConjuntionBuilder WhereIn<TField, TValueType>(Expression<Func<TTable, TField>> fieldNameDescriptor, List<TValueType> values)
        {
            var fieldName = _fieldNameResolver.GetFieldName(fieldNameDescriptor);
            return _whereBuilder.WhereIn<TTable, TValueType>(fieldName, values);
        }

        public WhereConjuntionBuilder OptionalWhere<TField>(Expression<Func<TTable, TField>> fieldNameDescriptor, Func<string> fnResolveCondition)
        {
            var fieldName = _fieldNameResolver.GetFieldName(fieldNameDescriptor);
            return _whereBuilder.OptionalWhere<TTable>(fieldName, fnResolveCondition);
        }

        public WhereConjuntionBuilder WhereExplicitly(string criteria)
        {
            return _whereBuilder.WhereExplicitly(criteria);
        }

        public TableBoundWhereBuilder<TTable> OpenParentheses()
        {
            _whereBuilder.OpenParentheses();
            return this;
        }

        public TableBoundWhereBuilder<TTable> CloseParentheses()
        {
            _whereBuilder.CloseParentheses();
            return this;
        }

        public WhereConjuntionBuilder WhereEqualTo<TField>(Expression<Func<TTable, TField>> fieldNameDescriptor, object value)
        {
            return Where(fieldNameDescriptor, FilterOperator.EqualTo, value);
        }

        public WhereConjuntionBuilder WhereLessThan<TField>(Expression<Func<TTable, TField>> fieldNameDescriptor, object value)
        {
            return Where(fieldNameDescriptor, FilterOperator.LessThan, value);
        }

        public WhereConjuntionBuilder WhereLessThanOrEqualTo<TField>(Expression<Func<TTable, TField>> fieldNameDescriptor, object value)
        {
            return Where(fieldNameDescriptor, FilterOperator.LessThanOrEqualTo, value);
        }

        public WhereConjuntionBuilder WhereGreaterThan<TField>(Expression<Func<TTable, TField>> fieldNameDescriptor, object value)
        {
            return Where(fieldNameDescriptor, FilterOperator.GreaterThan, value);
        }

        public WhereConjuntionBuilder WhereGreaterThanOrEqualTo<TField>(Expression<Func<TTable, TField>> fieldNameDescriptor, object value)
        {
            return Where(fieldNameDescriptor, FilterOperator.GreaterThanOrEqualTo, value);
        }

        public WhereConjuntionBuilder WhereStartsWith<TField>(Expression<Func<TTable, TField>> fieldNameDescriptor, object value)
        {
            return Where(fieldNameDescriptor, FilterOperator.StartsWith, value);
        }

        public WhereConjuntionBuilder WhereEndsWith<TField>(Expression<Func<TTable, TField>> fieldNameDescriptor, object value)
        {
            return Where(fieldNameDescriptor, FilterOperator.EndsWith, value);
        }

        public WhereConjuntionBuilder WhereNotEqualTo<TField>(Expression<Func<TTable, TField>> fieldNameDescriptor, object value)
        {
            return Where(fieldNameDescriptor, FilterOperator.NotEqualTo, value);
        }
    }
}