namespace FodyTools
{
    using System.Collections.Generic;

    internal static class CollectionExtensionMethods
    {
        public static void AddRange<T>(this IList<T> collection, params T[] values)
        {
            AddRange(collection, (IEnumerable<T>)values);
        }

        public static void AddRange<T>(this IList<T> collection, IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                collection.Add(value);
            }
        }
    }
}
