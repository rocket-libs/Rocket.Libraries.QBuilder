namespace Rocket.Libraries.Qurious.Builders.Paging
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;
    using Rocket.Libraries.Qurious.Helpers;

    public class MySqlServerPagingBuilder<TTable> : BuilderBase
    {
        public MySqlServerPagingBuilder(QBuilder qBuilder)
            : base(qBuilder)
        {
        }

        public QBuilder PageBy<TField>(Expression<Func<TTable, TField>> fieldNameDescriber, int page, int pageSize)
        {
            var fieldName = new FieldNameResolver().GetFieldName(fieldNameDescriber);
            var tableName = QBuilder.TableNameAliaser.GetTableAlias<TTable>();
            var range = PageRangeCalculator.GetPageRange(page, pageSize);
            QBuilder.SetSuffix($"Limit {tableName}.{fieldName} {range.Start},{range.End}");
            return QBuilder;
        }
    }
}