namespace Rocket.Libraries.Qurious.Builders.Paging
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;
    using Rocket.Libraries.Qurious.Helpers;

    public class MySqlServerPagingBuilder<TTable> : BuilderBase, IPagingBuilder<TTable>
    {
        public MySqlServerPagingBuilder(QBuilder qBuilder)
            : base(qBuilder)
        {
        }

        public QBuilder PageBy<TField>(Expression<Func<TTable, TField>> fieldNameDescriber, uint page, ushort pageSize)
        {
            var fieldName = new FieldNameResolver().GetFieldName(fieldNameDescriber);
            var tableName = QBuilder.TableNameAliaser.GetTableAlias<TTable>();
            var range = PageRangeCalculator.GetPageRange(0, page, pageSize);
            QBuilder.UseOrdering()
                .OrderBy<TTable>(fieldName);
            QBuilder.SetSuffix($"Limit {range.Start},{range.PageSize}");
            return QBuilder;
        }
    }
}