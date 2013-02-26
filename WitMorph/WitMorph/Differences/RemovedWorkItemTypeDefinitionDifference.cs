namespace WitMorph.Differences
{
    public class RemovedWorkItemTypeDefinitionDifference : IDifference
    {
        private readonly string _typeName;

        public RemovedWorkItemTypeDefinitionDifference(string typeName)
        {
            _typeName = typeName;
        }

        public string TypeName
        {
            get { return _typeName; }
        }
    }
}
