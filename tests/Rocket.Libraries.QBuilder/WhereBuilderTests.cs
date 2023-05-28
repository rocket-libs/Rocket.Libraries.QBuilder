using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Rocket.Libraries.Qurious.Builders;
using Rocket.Libraries.Qurious;

namespace Rocket.Libraries.QuriousTests
{
    class TestTable
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class WhereBuilderTests
    {
        [Fact]
        public void TestBasicParameterizedWhere()
        {
            var qBuilder = new QBuilder(parameterize: true)
                .UseTableBoundSelector<TestTable>()
                .Select(x => x.Id)
                .Then()
                .UseTableBoundFilter<TestTable>()
                .WhereEqualTo(x => x.Name, "TestName")
                .Then();

            var result = qBuilder.BuildWithParameters();
            var parameterName = "@Name0";
            Assert.Equal($"Select * from (Select \ntTestTable.Id From TestTable tTestTable\nWhere tTestTable.Name  = '{parameterName}'\n) as t", result.ParameterizedSql);
        }

        [Fact]
        public void TestBasicUnParameterizedWhere()
        {
            var qBuilder = new QBuilder(parameterize: false)
                .UseTableBoundSelector<TestTable>()
                .Select(x => x.Id)
                .Then()
                .UseTableBoundFilter<TestTable>()
                .WhereEqualTo(x => x.Name, "TestName")
                .Then();

            var result = qBuilder.Build();
            var value = "TestName";
            Assert.Equal($"Select * from (Select \ntTestTable.Id From TestTable tTestTable\nWhere tTestTable.Name  = '{value}'\n) as t", result);
        }
    }
}