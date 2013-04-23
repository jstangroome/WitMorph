using System.Collections.Generic;

namespace WitMorph.Structures
{
    public class CurrentToGoalMap<T> : ICurrentToGoalMap<T>
    {
        private readonly IDictionary<T, T> _goalKeyedByCurrent = new Dictionary<T, T>();

        public CurrentToGoalMap(IEqualityComparer<T> equalityComparer)
        {
            _goalKeyedByCurrent = new Dictionary<T, T>(equalityComparer);
        }

        public void Add(T currentItem, T goalItem)
        {
            _goalKeyedByCurrent.Add(currentItem, goalItem);
        }

        public T GetGoalByCurrent(T current)
        {
            return _goalKeyedByCurrent.ContainsKey(current) ? _goalKeyedByCurrent[current] : default(T);
        }
    }
}