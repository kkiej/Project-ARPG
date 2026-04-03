using System.Collections.Generic;

namespace vietlabs.fr2
{
    /// <summary>
    /// Extension methods for collection operations to reduce code duplication
    /// </summary>
    internal static class CollectionExtensions
    {
        /// <summary>
        /// Initialize a list if null, or clear it if already initialized
        /// </summary>
        internal static List<T> InitializeOrClear<T>(this List<T> list)
        {
            if (list == null)
            {
                list = new List<T>();
            }
            else
            {
                list.Clear();
            }

            return list;
        }
        
        /// <summary>
        /// Initialize a dictionary if null, or clear it if already initialized
        /// </summary>
        internal static Dictionary<TKey, TValue> InitializeOrClear<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
            {
                dictionary = new Dictionary<TKey, TValue>();
            }
            else
            {
                dictionary.Clear();
            }

            return dictionary;
        }
        
        /// <summary>
        /// Initialize a HashSet if null, or clear it if already initialized
        /// </summary>
        internal static void InitializeOrClear<T>(ref HashSet<T> hashSet)
        {
            if (hashSet == null)
            {
                hashSet = new HashSet<T>();
            }
            else
            {
                hashSet.Clear();
            }
        }
    }
}
