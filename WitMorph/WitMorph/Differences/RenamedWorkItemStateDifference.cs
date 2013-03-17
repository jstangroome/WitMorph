using WitMorph.Model;

namespace WitMorph.Differences
{
    public class RenamedWorkItemStateDifference : IWorkItemTypeDifference
    {
        private readonly string _currentWorkItemTypeName;
        private readonly string _currentStateName;
        private readonly WitdState _goalState;

        public RenamedWorkItemStateDifference(string currentWorkItemTypeName, string currentStateName, WitdState goalState)
        {
            _currentWorkItemTypeName = currentWorkItemTypeName;
            _currentStateName = currentStateName;
            _goalState = goalState;
        }

        public string CurrentWorkItemTypeName
        {
            get { return _currentWorkItemTypeName; }
        }

        public string CurrentStateName
        {
            get { return _currentStateName; }
        }

        public string GoalStateName
        {
            get { return _goalState.Value; }
        }

        public WitdState GoalState
        {
            get { return _goalState; }
        }
    }
}
