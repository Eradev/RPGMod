using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGMod.Extensions
{
    public static class IEnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> q, Func<T, bool> e)
        {
            return q.Where(e).Random();
        }

        public static T Random<T>(this IEnumerable<T> q)
        {
            var enumerable = q.ToList();

            return !enumerable.Any()
                ? default(T)
                : enumerable.Skip(new Random().Next(enumerable.Count())).FirstOrDefault();
        }
    }
}
