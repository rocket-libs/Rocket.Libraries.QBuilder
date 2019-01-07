using System;

namespace Rocket.Libraries.Qurious
{
    public class WhereDescription
    {
        public string Clause { get; set; }

        internal string Conjunction { get; set; }

        internal Guid ParenthesesId {get; set;}
    }
}