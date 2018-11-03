namespace Rocket.Libraries.JoinBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class SelectDescription
    {
        public string Table { get; set; }

        public string Field { get; set; }

        public string FieldAlias { get; set; }
    }
}