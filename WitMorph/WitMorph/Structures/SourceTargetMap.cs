using System.Collections.Generic;

namespace WitMorph.Structures
{
    public class SourceTargetMap<T>
    {
        private readonly IDictionary<T, T> _sourceKeyedByTarget = new Dictionary<T, T>();

        public SourceTargetMap(IEqualityComparer<T> equalityComparer)
        {
            _sourceKeyedByTarget = new Dictionary<T, T>(equalityComparer);
        }

        public void Add(T sourceItem, T targetItem)
        {
            _sourceKeyedByTarget.Add(targetItem, sourceItem);
        }

        public T GetSourceByTarget(T target)
        {
            return _sourceKeyedByTarget.ContainsKey(target) ? _sourceKeyedByTarget[target] : default(T);
        }
    }
}