using System.Xml;

namespace WitMorph.Differences
{
    public class ChangedWorkItemFormDifference : IWorkItemTypeDifference
    {
        private readonly string _currentWorkItemTypeName;
        private readonly XmlElement _formElement;

        public ChangedWorkItemFormDifference(string currentWorkItemTypeName, XmlElement formElement)
        {
            _currentWorkItemTypeName = currentWorkItemTypeName;
            _formElement = formElement;
        }

        public string CurrentWorkItemTypeName { get { return _currentWorkItemTypeName; } }

        public XmlElement FormElement { get { return _formElement; } }
    }
}