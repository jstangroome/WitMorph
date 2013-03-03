namespace WitMorph.Differences
{
    public class AddedWorkItemFieldDifference : IWorkItemTypeDifference
    {
        private readonly string _currentWorkItemTypeName;
        private readonly WitdField _goalField;

        public AddedWorkItemFieldDifference(string currentWorkItemTypeName, WitdField goalField)
        {
            _currentWorkItemTypeName = currentWorkItemTypeName;
            _goalField = goalField;
        }

        public string CurrentWorkItemTypeName
        {
            get { return _currentWorkItemTypeName; }
        }

        public string GoalFieldReferenceName
        {
            get { return _goalField.ReferenceName; }
        }

        public WitdField GoalField
        {
            get { return _goalField; }
        }
    }
}