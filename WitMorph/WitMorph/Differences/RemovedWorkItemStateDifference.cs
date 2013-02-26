namespace WitMorph.Differences
{
    public class RemovedWorkItemStateDifference : IDifference
    {
        private readonly string _stateName;

        public RemovedWorkItemStateDifference(string stateName)
        {
            _stateName = stateName;
        }

        public string StateName
        {
            get { return _stateName; }
        }
    }
}