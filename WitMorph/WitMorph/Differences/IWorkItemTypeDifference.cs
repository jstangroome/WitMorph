namespace WitMorph.Differences
{
    public interface IWorkItemTypeDifference : IDifference
    {
        string CurrentWorkItemTypeName { get; }
    }
}