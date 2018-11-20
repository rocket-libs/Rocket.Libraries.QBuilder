namespace Rocket.Libraries.Qurious
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class WhereDescription
    {
        public string Clause { get; set; }

        internal string Conjunction { get; set; }
    }
}