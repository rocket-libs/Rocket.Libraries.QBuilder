namespace Rocket.Libraries.Qurious.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class DerivedTableJoinDescription
    {
        public string RightTable { get; set; }

        public string LeftField { get; set; }

        public QBuilder QBuilder { get; set; }

        public string RightField { get; set; }

        public string JoinType { get; set; }
    }
}