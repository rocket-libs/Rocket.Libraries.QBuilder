using Rocket.Libraries.Qurious;
using Rocket.Libraries.QuriousTests.Models;
using System;
using Xunit;

namespace Rocket.Libraries.QuriousTests
{
    public class SelectBuilderTests
    {
        [Fact]
        public void GroupedSelect()
        {
            var query = new QBuilder()
                .UseSelector()
                .Select<WorkflowInstance>("*")
                .Then()
                .UseJoiner()
                .InnerJoin<WorkflowInstanceState, WorkflowInstance>(nameof(WorkflowInstanceState.WorkflowInstanceStateId), nameof(WorkflowInstance.Id))
                .JoinToDerivedTable("latestState", nameof(WorkflowInstanceState.WorkflowInstanceStateId), nameof(WorkflowInstance.Id))
                .Select<WorkflowInstanceState>(nameof(WorkflowInstanceState.WorkflowInstanceStateId))
                .SelectAggregated<WorkflowInstanceState>("Created", "BiggestCreated", "Max")
                .Then()
                .FinishJoinToDerivedTable()
                .Then()
                .Build();
            Assert.True(true);
        }
    }
}