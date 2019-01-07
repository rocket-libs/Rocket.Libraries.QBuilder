namespace Rocket.Libraries.Qurious
{
    using System;

    public class TableNameAliaser
    {
        public TableNameAliaser(Func<Type, string> tableNameResolver)
        {
            TableNameResolver = tableNameResolver;
        }

        public Func<Type, string> TableNameResolver { get; }

        public string GetTableAlias(string tableName, bool shouldAliasTableName = true)
        {
            if (shouldAliasTableName == false)
            {
                return string.Empty;
            }
            else
            {
                return $"t{tableName}";
            }
        }

        public string GetTableAlias<TTable>()
        {
            var tableName = TableNameResolver(typeof(TTable));
            return GetTableAlias(tableName, true);
        }
    }
}