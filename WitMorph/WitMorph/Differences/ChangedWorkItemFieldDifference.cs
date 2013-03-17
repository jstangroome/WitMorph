using WitMorph.Model;

namespace WitMorph.Differences
{
    public class ChangedWorkItemFieldDifference : IWorkItemTypeDifference
    {
        private readonly string _currentWorkItemTypeName;
        private readonly string _currentFieldReferenceName;
        private readonly WitdField _goalField;

        public ChangedWorkItemFieldDifference(string currentWorkItemTypeName, string currentFieldReferenceName, WitdField goalField)
        {
            _currentWorkItemTypeName = currentWorkItemTypeName;
            _currentFieldReferenceName = currentFieldReferenceName;
            _goalField = goalField;
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
            get { return _goalField.ReferenceName; }
        }

        public WitdField GoalField
        {
            get { return _goalField; }
        }
    }
}