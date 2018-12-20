namespace Rocket.Libraries.Qurious
{
    public class JoinDescription
    {
        public string LeftTable { get; set; }

        public string LeftField { get; set; }

        public string RightField { get; set; }

        public string JoinType { get; set; }

        public string ExplicitLeftTableAlias { get; set; }

        public string DerivedTable { get; set; }

        public string RightTable { get; set; }

        public bool IsDerivedTableJoinDescription =>
            !string.IsNullOrEmpty(ExplicitLeftTableAlias)
            && !string.IsNullOrEmpty(DerivedTable)
            && string.IsNullOrEmpty(LeftTable);
    }
}