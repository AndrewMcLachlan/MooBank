using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.BankPlus.Data
{
    public static class ICollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (collection.IsReadOnly) throw new InvalidOperationException("Collection is readonly");

            if (items == null) return;

            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
