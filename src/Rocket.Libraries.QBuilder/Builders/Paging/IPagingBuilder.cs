using System;
using System.Linq.Expressions;

namespace Rocket.Libraries.Qurious.Builders.Paging
{
    public interface IPagingBuilder<TTable>
    {
        QBuilder PageBy<TField>(Expression<Func<TTable, TField>> fieldNameDescriber, uint page, ushort pageSize, bool orderAscending = true);
    }
}