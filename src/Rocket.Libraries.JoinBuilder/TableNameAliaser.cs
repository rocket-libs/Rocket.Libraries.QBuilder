namespace Rocket.Libraries.JoinBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class TableNameAliaser
    {
        public TableNameAliaser(Func<Type, string> tableNameResolver)
        {
            TableNameResolver = tableNameResolver;
        }

        public Func<Type, string> TableNameResolver { get; }

        public string GetTableAlias(string tableName)
        {
            return $"t{tableName}";
        }

        public string GetTableAlias<TTable>()
        {
            var tableName = TableNameResolver(typeof(TTable));
            return GetTableAlias(tableName);
        }
    }
}
