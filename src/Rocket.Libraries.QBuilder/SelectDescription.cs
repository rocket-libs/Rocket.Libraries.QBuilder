﻿namespace Rocket.Libraries.Qurious
{
    internal class SelectDescription
    {
        public string Table { get; set; }

        public string Field { get; set; }

        public string FieldAlias { get; set; }

        public string AggregateFunction { get; set; }

        public bool TableNameAliasingPrevented { get; set; }
    }
}