namespace WitMorph.Differences
{
    public class RenamedWorkItemTypeDefinitionDifference : IDifference
    {
        private readonly string _currentTypeName;
        private readonly string _goalTypeName;

        public RenamedWorkItemTypeDefinitionDifference(string currentTypeName, string goalTypeName)
        {
            _currentTypeName = currentTypeName;
            _goalTypeName = goalTypeName;
        }

        public string CurrentTypeName
        {
            get { return _currentTypeName; }
        }

        public string GoalTypeName
        {
            get { return _goalTypeName; }
        }
    }
}