using System.Collections;
using System.Diagnostics;
using System.Xml;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WitMorph.Actions
{
    public class ModifyWorkItemStateMorphAction : MorphAction
    {
        private readonly string _workItemTypeName;
        private readonly string _fromValue;
        private readonly string _toValue;

        public ModifyWorkItemStateMorphAction(string workItemTypeName, string fromValue, string toValue)
        {
            _workItemTypeName = workItemTypeName;
            _fromValue = fromValue;
            _toValue = toValue;
        }

        public string TypeName { get { return _workItemTypeName; } }
        public string FromValue { get { return _fromValue; } }
        public string ToValue { get { return _toValue; } }

        public override void Execute(ExecutionContext context)
        {
            var project = context.GetWorkItemProject();
            var queryContext = new Hashtable {{"project", project.Name}, {"workitemtypename", _workItemTypeName}, {"fromvalue", _fromValue}};

            const string wiql = @"select [System.Id], [System.State] from WorkItems where [System.TeamProject] = @project and [System.WorkItemType] = @workitemtypename and [System.State] = @fromvalue order by [System.Id]";
            
            var workItems = project.Store.Query(wiql, queryContext);

            foreach (WorkItem workItem in workItems)
            {
                Debug.WriteLine(workItem.Id);
                if (!workItem.IsOpen) workItem.Open();
                workItem.State = _toValue;
                workItem.Save();
            }

        }

        protected override void SerializeCore(XmlWriter writer)
        {
            writer.WriteAttributeString("typename", _workItemTypeName);
            writer.WriteAttributeString("fromvalue", _fromValue);
            writer.WriteAttributeString("tovalue", _toValue);
        }

        public static MorphAction Deserialize(XmlElement element, DeserializationContext context)
        {
            return new ModifyWorkItemStateMorphAction(
                element.GetAttribute("typename"), 
                element.GetAttribute("fromvalue"), 
                element.GetAttribute("tovalue"));
        }

        public override string ToString()
        {
            return string.Format("Modify state from '{0}' to '{1}' for work items of type '{2}'", _fromValue, _toValue, _workItemTypeName);
        }
    }
}