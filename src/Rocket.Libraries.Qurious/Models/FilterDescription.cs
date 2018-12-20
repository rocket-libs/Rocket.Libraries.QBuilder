using System;
using System.Linq.Expressions;
using Rocket.Libraries.Qurious.Helpers;

namespace Rocket.Libraries.Qurious.Models
{
    public class FilterDescription<TTable>
    {
        public string FieldName { get; private set; }
        public string Filter {get; private set; }

        public bool FilterSet => !string.IsNullOrEmpty(FieldName) && !string.IsNullOrEmpty(Filter);
        public FilterDescription<TTable> SetConditionalFilter<TField>(Expression<Func<TTable, TField>> fieldNameDescriptor, Func<string> fnConditionalFilterResolver)
        {
            return SetFilter(fieldNameDescriptor,fnConditionalFilterResolver());
        }

        public FilterDescription<TTable> SetFilter<TField>(Expression<Func<TTable, TField>> fieldNameDescriptor, string filter)
        {
            FieldName = new FieldNameResolver().GetFieldName(fieldNameDescriptor);
            FieldName = FieldName;
            Filter = filter;
            return this;
        }


    }
}