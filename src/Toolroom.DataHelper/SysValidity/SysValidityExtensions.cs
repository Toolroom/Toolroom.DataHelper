using System;
using System.Linq;

namespace Toolroom.DataHelper
{
    public static class SysValidityExtensions
    {
        public static IQueryable<T> OfModifiedAfter<T>(this IQueryable<T> query, DateTime? changedAfter)
            where T : class, ISysValidityEntity
        {
            if (!changedAfter.HasValue || changedAfter.Value == DateTime.MinValue)
                return query;

            return query.Where(_ => _.SysStartTime > changedAfter.Value);
        }
    }
}