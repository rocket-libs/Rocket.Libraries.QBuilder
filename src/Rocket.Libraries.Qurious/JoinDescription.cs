namespace Rocket.Libraries.Qurious
{
    public class JoinDescription
    {
        public string LeftTable { get; set; }

        public string RightTable { get; set; }

        public string LeftField { get; set; }

        public string RightField { get; set; }

        public string JoinType { get; set; }
    }
}