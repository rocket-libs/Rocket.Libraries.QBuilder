namespace Rocket.Libraries.Qurious.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Rocket.Libraries.Qurious.Helpers;

    public class DerivedTableSelector
    {
        public DerivedTableSelector(QBuilder derivedTable, SelectBuilder selectBuilder)
        {
            SelectBuilder = selectBuilder;
            TableName = DerivedTableWrapperNameResolver.GetWrapperName(derivedTable.DerivedTableName);
        }

        private SelectBuilder SelectBuilder { get; }

        private string TableName { get; }

        public DerivedTableSelector Select(string field)
        {
            return Select(field, string.Empty);
        }

        public DerivedTableSelector Select(string field, string fieldAlias)
        {
            return Select(field, fieldAlias, string.Empty);
        }

        public DerivedTableSelector Select(string field, string fieldAlias, string aggregateFunction)
        {
            SelectBuilder.SelectExplicit(TableName, field, fieldAlias, aggregateFunction, true);
            return this;
        }

        public SelectBuilder Then()
        {
            return SelectBuilder;
        }
    }
}