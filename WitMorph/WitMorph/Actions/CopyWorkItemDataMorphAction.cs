using System.Collections;
using System.Diagnostics;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WitMorph.Actions
{
    public class CopyWorkItemDataMorphAction : IMorphAction
    {
        private readonly string _workItemTypeName;
        private readonly string _fromFieldReferenceName;
        private readonly string _toFieldReferenceName;

        public CopyWorkItemDataMorphAction(string workItemTypeName, string fromFieldReferenceName, string toFieldReferenceName)
        {
            _workItemTypeName = workItemTypeName;
            _fromFieldReferenceName = fromFieldReferenceName;
            _toFieldReferenceName = toFieldReferenceName;
        }

        public string TypeName { get { return _workItemTypeName; } }
        public string FromField { get { return _fromFieldReferenceName; } }
        public string ToField { get { return _toFieldReferenceName; } }

        public void Execute(ExecutionContext context)
        {
            var project = context.GetWorkItemProject();
            var queryContext = new Hashtable { { "project", project.Name }, { "workitemtypename", _workItemTypeName }};

            const string wiqlTemplate = @"select [System.Id], [{0}], [{1}] from WorkItems where [System.TeamProject] = @project and [System.WorkItemType] = @workitemtypename order by [System.Id]";
            var wiql = string.Format(wiqlTemplate, _fromFieldReferenceName, _toFieldReferenceName);

            var workItems = project.Store.Query(wiql, queryContext);

            foreach (WorkItem workItem in workItems)
            {
                Debug.WriteLine(workItem.Id);
                workItem.Fields[_toFieldReferenceName].Value = workItem.Fields[_fromFieldReferenceName].Value;
                workItem.Save();
            }

        }

        public override string ToString()
        {
            return string.Format("Copy data from field '{0}' to field '{1}' for work items of type '{2}'", _fromFieldReferenceName, _toFieldReferenceName, _workItemTypeName);
        }
    }
}