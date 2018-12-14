namespace Rocket.Libraries.Qurious.Models
{
    using Rocket.Libraries.Qurious.Builders;

    internal class InnerSelectDescription
    {
        public string DerivedTableName { get; set; }

        public QBuilder QBuilder { get; set; }

        public string Field { get; set; }

        public string InnerField { get; set; }

        public JoinBuilder Parent { get; set; }
    }
}