using Rocket.Libraries.Qurious.Helpers;
using Rocket.Libraries.Validation.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Rocket.Libraries.Qurious.Builders.Paging
{
    public class SqlServerPagingBuilder<TTable> : BuilderBase
    {
        public SqlServerPagingBuilder(QBuilder qBuilder)
            : base(qBuilder)
        {
        }

        public QBuilder PageBy<TField>(Expression<Func<TTable, TField>> fieldNameDescriber, int page, int pageSize)
        {
            new DataValidator()
                .AddFailureCondition(page < 1, $"Database query requested for page '{page}'. Pages must be greater than or equal to 1", false)
                .AddFailureCondition(pageSize < 1, $"Pages must have at least one record. Page size '{pageSize}' is not valid", false)
                .ThrowExceptionOnInvalidRules();
            const string rowNumber = "RowNumber";
            var fieldName = new FieldNameResolver().GetFieldName(fieldNameDescriber);
            var table = QBuilder.TableNameAliaser.GetTableAlias<TTable>();
            var range = PageRangeCalculator.GetPageRange(page, pageSize);
            QBuilder.UseSelector()
                 .SetSelectPrefix($"ROW_NUMBER() OVER (ORDER BY [{table}].[{fieldName}]) AS {rowNumber},")
                 .Then()
                 .UseFilter();
            QBuilder.SetSuffix($"Where {rowNumber} >= {range.Start} AND {rowNumber} <= {range.End} ORDER BY {rowNumber}");
            return QBuilder;
        }
    }
}