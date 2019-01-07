namespace Rocket.Libraries.Qurious.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Rocket.Libraries.Qurious.Helpers;
    using Rocket.Libraries.Qurious.Models;

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
    }
}