namespace WitMorph.Differences
{
    public class RemovedWorkItemFieldDifference : IDifference
    {
        private readonly string _currentWorkItemTypeName;
        private readonly string _referenceFieldName;

        public RemovedWorkItemFieldDifference(string currentWorkItemTypeName, string referenceFieldName)
        {
            _currentWorkItemTypeName = currentWorkItemTypeName;
            _referenceFieldName = referenceFieldName;
        }

        public string ReferenceFieldName
        {
            get { return _referenceFieldName; }
        }

        public string CurrentWorkItemTypeName
        {
            get { return _currentWorkItemTypeName; }
        }
    }
}