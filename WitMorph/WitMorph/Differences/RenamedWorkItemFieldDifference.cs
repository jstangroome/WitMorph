namespace WitMorph.Differences
{
    public class RenamedWorkItemFieldDifference : IDifference
    {
        private readonly string _currentWorkItemTypeName;
        private readonly string _currentFieldReferenceName;
        private readonly string _goalFieldReferenceName;

        public RenamedWorkItemFieldDifference(string currentWorkItemTypeName, string currentFieldReferenceName, string goalFieldReferenceName)
        {
            _currentWorkItemTypeName = currentWorkItemTypeName;
            _currentFieldReferenceName = currentFieldReferenceName;
            _goalFieldReferenceName = goalFieldReferenceName;
        }

        public string CurrentWorkItemTypeName
        {
            get { return _currentWorkItemTypeName; }
        }

        public string CurrentFieldReferenceName
        {
            get { return _currentFieldReferenceName; }
        }

        public string GoalFieldReferenceName
        {
            get { return _goalFieldReferenceName; }
        }
    }
}