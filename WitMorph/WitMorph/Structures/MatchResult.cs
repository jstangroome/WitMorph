using System.Collections.Generic;

namespace WitMorph.Structures
{
    public class MatchResult<T>
    {
        private readonly IList<T> _sourceOnly = new List<T>();
        private readonly IList<T> _targetOnly = new List<T>();
        private readonly IList<CurrentAndGoalPair<T>> _pairs = new List<CurrentAndGoalPair<T>>();

        public IList<T> SourceOnly { get { return _sourceOnly; } }
        public IList<T> TargetOnly { get { return _targetOnly; } }
        public IList<CurrentAndGoalPair<T>> Pairs { get { return _pairs; } }
    }
}