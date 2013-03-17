using WitMorph.Model;

namespace WitMorph.Differences
{
    public class AddedWorkItemStateDifference : IDifference
    {
        private readonly string _currentWorkItemTypeName;
        private readonly WitdState _state;

        public AddedWorkItemStateDifference(string currentWorkItemTypeName, WitdState state)
        {
            _currentWorkItemTypeName = currentWorkItemTypeName;
            _state = state;
        }

        public string CurrentWorkItemTypeName
        {
            get { return _currentWorkItemTypeName; }
        }

        public WitdState State
        {
            get { return _state; }
        }
    }
}
