using System;
using System.Collections.Generic;
using System.Linq;

namespace WitMorph.Structures
{
    public class MatchAndMap<TItem, TKey> where TItem:class where TKey:class
    {
        private readonly Func<TItem, TKey> _keySelector;
        private readonly IEqualityComparer<TKey> _keyEqualityComparer;
        private readonly CurrentToGoalMap<TKey> _keyMap;

        public MatchAndMap(Func<TItem, TKey> keySelector, IEqualityComparer<TKey> keyEqualityComparer, CurrentToGoalMap<TKey> keyMap)
        {
            _keySelector = keySelector;
            _keyEqualityComparer = keyEqualityComparer;
            _keyMap = keyMap;
        }

        private bool MatchFunction(TItem currentItem, TItem goalItem)
        {
            var sourceKey = _keySelector(goalItem);
            var targetKey = _keySelector(currentItem);
            var match = _keyEqualityComparer.Equals(sourceKey, targetKey);
            if (!match)
            {
                var mappedTargetKey = _keyMap.GetGoalByCurrent(targetKey);
                if (mappedTargetKey != default(TKey))
                {
                    match = _keyEqualityComparer.Equals(sourceKey, mappedTargetKey);
                }
            }
            return match;
        }

        public MatchResult<TItem> Match(IEnumerable<TItem> currentItems, IEnumerable<TItem> goalItems)
        {
            var output = new MatchResult<TItem>();

            goalItems = goalItems as TItem[] ?? goalItems.ToArray();
            currentItems = currentItems as TItem[] ?? currentItems.ToArray();

            foreach (var goalItem in goalItems)
            {
                var currentItem = currentItems.SingleOrDefault(t => MatchFunction(t, goalItem));
                if (currentItem == null)
                {
                    output.GoalOnly.Add(goalItem);
                }
                else
                {
                    output.Pairs.Add(new CurrentAndGoalPair<TItem>(currentItem, goalItem));
                }
            }

            foreach (var currentItem in currentItems)
            {
                var goalItem = goalItems.SingleOrDefault(s => MatchFunction(currentItem, s));
                if (goalItem == null)
                {
                    output.CurrentOnly.Add(currentItem);
                }
            }

            return output;
        }
        
    }
}