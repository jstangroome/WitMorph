using System.Xml;

namespace WitMorph.Differences
{
    public class ChangedWorkItemWorkflowDifference : IWorkItemTypeDifference
    {
        private readonly string _currentWorkItemTypeName;
        private readonly XmlElement _workflowElement;

        public ChangedWorkItemWorkflowDifference(string currentWorkItemTypeName, XmlElement workflowElement)
        {
            _currentWorkItemTypeName = currentWorkItemTypeName;
            _workflowElement = workflowElement;
        }

        public string CurrentWorkItemTypeName { get { return _currentWorkItemTypeName; } }

        public XmlElement WorkflowElement { get { return _workflowElement; } }
    }
}