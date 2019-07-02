using Rocket.Libraries.Qurious;
using Rocket.Libraries.Qurious.Builders.Paging;
using Rocket.Libraries.Qurious.Models;
using Rocket.Libraries.QuriousTests.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Rocket.Libraries.QuriousTests
{
    public class PagingBuilderTests
    {
        [Fact]
        public void SqlServerPagingIsDoneCorrectly()
        {
            var query = new QBuilder()
                .UseTableBoundSelector<User>()
                .Select(a => a.Id)
                .Then()
                .UseTableBoundSelector<WorkflowInstance>()
                .Select(a => a.UserId)
                .Then()
                .UseTableBoundJoinBuilder<User, WorkflowInstance>()
                .InnerJoin(u => u.Id, w => w.UserId)
                .UseSqlServerPagingBuilder<User>()
                .PageBy(a => a.Name, 1, 4)
                .Build();

            var x = 9;
        }

        [Theory]
        [InlineData(1, 1, 14)]
        [InlineData(2, 15, 28)]
        [InlineData(3, 29, 42)]
        [InlineData(4, 43, 56)]
        [InlineData(5, 57, 70)]
        [InlineData(6, 71, 84)]
        public void PageRangeCalculatedCorrectly(int page, int start, int end)
        {
            const int pageSize = 14;
            var actual = PageRangeCalculator.GetPageRange(page, pageSize);
            var expected = new PageRange
            {
                Start = start,
                End = end
            };

            Assert.Equal(expected, actual);
        }
    }
}