using Rocket.Libraries.Qurious;
using Rocket.Libraries.Qurious.Helpers;
using Xunit;

namespace Rocket.Libraries.QuriousTests.Helpers
{
    public class JoinDescriptionFlipperTests
    {
        [Fact]
        public void JoinDescriptionFlipIsDoneCorrectly()
        {
            const string leftTable = "leftTable";
            const string rightTable = "RightTable";
            const string leftField = "leftField";
            const string rightField = "rightField";

            var joinDescription = new JoinDescription
            {
                LeftField = rightField,
                LeftTable = rightTable,
                RightField = leftField,
                RightTable = leftTable
            };

            new JoinDescriptionFlipper().Flip(joinDescription);

            Assert.Equal(leftTable, joinDescription.LeftTable);
            Assert.Equal(leftField, joinDescription.LeftField);
            Assert.Equal(rightField, joinDescription.RightField);
            Assert.Equal(rightTable, joinDescription.RightTable);
        }
    }
}