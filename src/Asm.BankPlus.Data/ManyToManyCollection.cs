using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Asm.BankPlus.Data.Entities;

namespace Asm.BankPlus.Data
{
    public class ManyToManyCollection<TManyEntity, TChild, TChildKey> : ICollection<TChild> where TChild : class where TChildKey : IEquatable<TChildKey>, IComparable<TChildKey>
    {
        private Func<TManyEntity, TChildKey> ManyEntityChildKeySelector { get; }

        private Func<TChild, TChildKey> ChildKeySelector { get; }

        private Func<TManyEntity, TChild> ChildSelector { get; }

        private Func<TChild, TManyEntity> ManyEntityAdd { get; }

        private ICollection<TManyEntity> ParentCollection { get; }

        public ManyToManyCollection(ICollection<TManyEntity> manyToManyEntityCollection, Func<TChild, TManyEntity> manyEntityAdd, Func<TManyEntity, TChild> childSelector, Func<TManyEntity, TChildKey> manyEntityChildKeySelector, Func<TChild, TChildKey> childKeySelector)
        {
            ParentCollection = manyToManyEntityCollection;
            ManyEntityChildKeySelector = manyEntityChildKeySelector;
            ChildKeySelector = childKeySelector;
            ChildSelector = childSelector;
            ManyEntityAdd = manyEntityAdd;
        }

        public int Count => ParentCollection.Count;

        public bool IsReadOnly => ParentCollection.IsReadOnly;

        public void Add(TChild item)
        {
            ParentCollection.Add(ManyEntityAdd(item));
        }

        public void Clear() => ParentCollection.Clear();

        public bool Contains(TChild item) => ParentCollection.Any(t => ManyEntityChildKeySelector(t).Equals(ChildKeySelector(item)));
        public void CopyTo(TChild[] array, int arrayIndex) => ParentCollection.Select(t => ChildSelector(t)).ToArray().CopyTo(array, arrayIndex);
        public IEnumerator<TChild> GetEnumerator() => ParentCollection.Select(t => ChildSelector(t)).GetEnumerator();
        public bool Remove(TChild item) => ParentCollection.Remove(ParentCollection.SingleOrDefault(t => ManyEntityChildKeySelector(t).Equals(ChildKeySelector(item))));
        IEnumerator IEnumerable.GetEnumerator() => ParentCollection.Select(t => ChildSelector(t)).GetEnumerator();
    }
}
