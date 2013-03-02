namespace WitMorph.Differences
{
    public class RenamedWorkItemStateDifference : IWorkItemTypeDifference
    {
        private readonly string _currentWorkItemTypeName;
        private readonly string _currentStateName;
        private readonly string _goalStateName;

        public RenamedWorkItemStateDifference(string currentWorkItemTypeName, string currentStateName, string goalStateName)
        {
            _currentWorkItemTypeName = currentWorkItemTypeName;
            _currentStateName = currentStateName;
            _goalStateName = goalStateName;
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
            get { return _goalStateName; }
        }
    }
}
