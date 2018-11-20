namespace Rocket.Libraries.Qurious.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class WhereConjuntionBuilder : BuilderBase
    {
        private readonly WhereBuilder _whereBuilder;

        public WhereConjuntionBuilder(WhereBuilder whereBuilder, QBuilder qBuilder)
            : this(qBuilder)
        {
            _whereBuilder = whereBuilder;
        }

        public WhereConjuntionBuilder(QBuilder qBuilder)
            : base(qBuilder)
        {
        }

        public WhereBuilder And()
        {
            _whereBuilder.SetNextConjunction("And");
            return _whereBuilder;
        }

        public WhereBuilder Or()
        {
            _whereBuilder.SetNextConjunction("Or");
            return _whereBuilder;
        }
    }
}
