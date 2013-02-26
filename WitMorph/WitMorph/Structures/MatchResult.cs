using System.Collections.Generic;

namespace WitMorph.Structures
{
    public class MatchResult<T>
    {
        private readonly IList<T> _goalOnly = new List<T>();
        private readonly IList<T> _currentOnly = new List<T>();
        private readonly IList<CurrentAndGoalPair<T>> _pairs = new List<CurrentAndGoalPair<T>>();

        public IList<T> GoalOnly { get { return _goalOnly; } }
        public IList<T> CurrentOnly { get { return _currentOnly; } }
        public IList<CurrentAndGoalPair<T>> Pairs { get { return _pairs; } }
    }
}