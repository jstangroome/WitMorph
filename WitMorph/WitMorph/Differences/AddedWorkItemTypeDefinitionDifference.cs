namespace WitMorph.Differences
{
    public class AddedWorkItemTypeDefinitionDifference : IDifference
    {
        private readonly WorkItemTypeDefinition _witd;

        public AddedWorkItemTypeDefinitionDifference(WorkItemTypeDefinition witd)
        {
            _witd = witd;
        }

        public WorkItemTypeDefinition WorkItemTypeDefinition
        {
            get { return _witd; }
        }
    }
}
