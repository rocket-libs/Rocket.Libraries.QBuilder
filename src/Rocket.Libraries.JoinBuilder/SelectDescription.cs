namespace Rocket.Libraries.Qurious
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class SelectDescription
    {
        public string Table { get; set; }

        public string Field { get; set; }

        public string FieldAlias { get; set; }

        public string AggregateFunction { get; set; }
    }
}