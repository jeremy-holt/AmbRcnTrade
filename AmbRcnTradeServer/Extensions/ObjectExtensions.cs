using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Extensions
{
    public static class ObjectExtensions
    {
        public static List<T> ToNonNullList<T>(this Dictionary<string, T> dictionary)
        {
            return dictionary.Where(c => c.Value != null).Select(x=>x.Value).ToList();
        }

        public static async Task<List<T>> LoadListFromMultipleIdsAsync<T>(this IAsyncDocumentSession session, IEnumerable<string> multipleIds)
        {
            var dictionary = await session.LoadAsync<T>(multipleIds);
            return dictionary.ToNonNullList();
        }
    }
}