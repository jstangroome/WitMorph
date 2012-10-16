using System.Collections;
using System.Diagnostics;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WitMorph.Actions
{
    public class ModifyWorkItemStateMorphAction : IMorphAction
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

        public void Execute(ExecutionContext context)
        {
            var project = context.GetWorkItemProject();
            var queryContext = new Hashtable {{"project", project.Name}, {"workitemtypename", _workItemTypeName}, {"fromvalue", _fromValue}};

            const string wiql = @"select [System.Id], [System.State] from WorkItems where [System.TeamProject] = @project and [System.WorkItemType] = @workitemtypename and [System.State] = @fromvalue order by [System.Id]";
            
            var workItems = project.Store.Query(wiql, queryContext);

            foreach (WorkItem workItem in workItems)
            {
                Debug.WriteLine(workItem.Id);
                workItem.State = _toValue;
                workItem.Save();
            }

        }
    }
}