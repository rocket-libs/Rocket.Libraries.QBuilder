namespace Rocket.Libraries.Qurious.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;
    using Rocket.Libraries.Qurious.Models;

    public class DerivedTableJoiner<TOuterTable>
    {
        private JoinBuilder _joinBuilder;

        public DerivedTableJoiner(JoinBuilder joinBuilder)
        {
            _joinBuilder = joinBuilder;
        }

        public JoinBuilder InnerJoinDerivedTable<TOuterField>(Expression<Func<TOuterTable, TOuterField>> outerFieldDescription, QBuilder derivedTable, string derivedFieldName)
        {
            _joinBuilder.InnerJoinDerivedTable(outerFieldDescription, derivedTable, derivedFieldName);
            return _joinBuilder;
        }
    }
}