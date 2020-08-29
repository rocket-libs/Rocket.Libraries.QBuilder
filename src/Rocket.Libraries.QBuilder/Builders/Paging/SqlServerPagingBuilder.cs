﻿namespace Rocket.Libraries.Qurious.Builders.Paging
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;
    using Rocket.Libraries.Qurious.Helpers;
    using Rocket.Libraries.Validation.Services;

    public class SqlServerPagingBuilder<TTable> : BuilderBase, IPagingBuilder<TTable>
    {
        public SqlServerPagingBuilder(QBuilder qBuilder)
            : base(qBuilder)
        {
        }

        public byte AbsoluteFirstRecordIndex => 1;

        public QBuilder PageBy<TField>(Expression<Func<TTable, TField>> fieldNameDescriber, uint page, ushort pageSize, bool orderAscending = true)
        {
            new DataValidator()
                .AddFailureCondition(page < 1, $"Database query requested for page '{page}'. Pages must be greater than or equal to 1", false)
                .AddFailureCondition(pageSize < 1, $"Pages must have at least one record. Page size '{pageSize}' is not valid", false)
                .ThrowExceptionOnInvalidRules();
            const string rowNumber = "RowNumber";
            var fieldName = new FieldNameResolver().GetFieldName(fieldNameDescriber);
            var table = QBuilder.TableNameAliaser.GetTableAlias<TTable>();
            var range = PageRangeCalculator.GetPageRange(AbsoluteFirstRecordIndex, page, pageSize);
            QBuilder.UseSelector()
                 .SetSelectPrefix($"ROW_NUMBER() OVER (ORDER BY [{table}].[{fieldName}]) AS {rowNumber},")
                 .Then()
                 .UseFilter();
            string orderSuffix = orderAscending ? "Asc" : "Desc";
            QBuilder.SetSuffix($"Where {rowNumber} >= {range.Start} AND {rowNumber} <= {range.End} ORDER BY {rowNumber} {orderSuffix}");
            return QBuilder;
        }
    }
}