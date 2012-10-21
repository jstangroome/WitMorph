using System.Collections.Generic;

namespace WitMorph.Structures
{
    public class MatchResult<T>
    {
        private readonly IList<T> _sourceOnly = new List<T>();
        private readonly IList<T> _targetOnly = new List<T>();
        private readonly IList<SourceTargetPair<T>> _pairs = new List<SourceTargetPair<T>>();

        public IList<T> SourceOnly { get { return _sourceOnly; } }
        public IList<T> TargetOnly { get { return _targetOnly; } }
        public IList<SourceTargetPair<T>> Pairs { get { return _pairs; } }
    }
}