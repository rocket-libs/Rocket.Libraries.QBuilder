using Rocket.Libraries.Qurious;
using Rocket.Libraries.QuriousTests.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Rocket.Libraries.QuriousTests
{
    public class QbuilderTests
    {
        [Fact]
        public void DerivedTableJoinsWorkFine()
        {
            var derivedQuery = new QBuilder("derived")
                .UseTableBoundSelector<WorkflowInstanceState>()
                .SelectAggregated(t => t.Created, "Created", "Max")
                .Select(t => t.WorkflowInstanceId)
                .Then()
                .UseTableBoundGrouper<WorkflowInstanceState>()
                .Then();

            var query = new QBuilder("t")
                .UseTableBoundSelector<WorkflowInstance>()
                .Select(t => t.Id)
                .Then()
                .UseSelector()
                .UseDerivedTableSelector(derivedQuery)
                .Select("*")
                .Then()
                .Then()
                .UseJoiner()
                .UseDerivedTableJoiner<WorkflowInstance>()
                .InnerJoinDerivedTable(t => t.Id, derivedQuery, nameof(WorkflowInstanceState.WorkflowInstanceId))
                .InnerJoin<User, WorkflowInstance>(nameof(User.Id), nameof(WorkflowInstance.UserId))
                .Then()
                .Build();
        }

        [Fact]
        public void NonDerivedTableJoinsWorkFine()
        {
            var query = new QBuilder("t")
                .UseTableBoundSelector<WorkflowInstance>()
                .Select(t => t.Id)
                .Then()
                .UseTableBoundSelector<WorkflowInstanceState>()
                .Select(t => t.Created)
                .Then()
                .UseTableBoundJoinBuilder<WorkflowInstance, WorkflowInstanceState>()
                .InnerJoin(l => l.Id, r => r.WorkflowInstanceId)
                .Build();
        }
    }
}