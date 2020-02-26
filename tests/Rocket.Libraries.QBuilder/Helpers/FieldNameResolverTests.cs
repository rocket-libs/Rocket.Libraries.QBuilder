using Rocket.Libraries.Qurious.Helpers;
using Rocket.Libraries.QuriousTests.Models;
using System;
using System.Linq.Expressions;
using Xunit;

namespace Rocket.Libraries.QuriousTests.Helpers
{
    public class FieldNameResolverTests
    {
        [Fact]
        public void VerifyFieldNamesAreResolvedCorrectly()
        {
            var fieldName = new Wrapper<WorkflowInstanceState>()
                .GetFieldName(t => t.Created);
            Assert.Equal(nameof(WorkflowInstanceState.Created), fieldName);
        }

        private class Wrapper<TTable>
            where TTable : new()
        {
            public string GetFieldName<TField>(Expression<Func<TTable, TField>> expression)
            {
                var fieldNameResolver = new FieldNameResolver();
                return fieldNameResolver.GetFieldName<TTable, TField>(expression);
            }
        }
    }
}