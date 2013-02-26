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

        private bool MatchFunction(TItem sourceItem, TItem targetItem)
        {
            var sourceKey = _keySelector(sourceItem);
            var targetKey = _keySelector(targetItem);
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

        public MatchResult<TItem> Match(IEnumerable<TItem> sourceItems, IEnumerable<TItem> targetItems)
        {
            var output = new MatchResult<TItem>();

            sourceItems = sourceItems as TItem[] ?? sourceItems.ToArray();
            targetItems = targetItems as TItem[] ?? targetItems.ToArray();

            foreach (var sourceItem in sourceItems)
            {
                var targetItem = targetItems.SingleOrDefault(t => MatchFunction(sourceItem, t));
                if (targetItem == null)
                {
                    output.SourceOnly.Add(sourceItem);
                }
                else
                {
                    output.Pairs.Add(new SourceTargetPair<TItem>(sourceItem, targetItem));
                }
            }

            foreach (var targetItem in targetItems)
            {
                var sourceItem = sourceItems.SingleOrDefault(s => MatchFunction(s, targetItem));
                if (sourceItem == null)
                {
                    output.TargetOnly.Add(targetItem);
                }
            }

            return output;
        }
        
    }
}