using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WitMorph.Actions
{
    public class ExportWorkItemDataMorphAction : IMorphAction
    {
        private readonly string _workItemTypeName;
        private readonly string _exportPath;
        private readonly List<string> _fieldReferenceNames = new List<string>();

        public ExportWorkItemDataMorphAction(string workItemTypeName, string exportPath)
        {
            _workItemTypeName = workItemTypeName;
            _exportPath = exportPath;
        }

        public void AddExportField(string fieldReferenceName)
        {
            _fieldReferenceNames.Add(fieldReferenceName);
        }

        public void Execute(ExecutionContext context)
        {
            if (_fieldReferenceNames.Count == 0)
            {
                return;
            }

            var project = context.GetWorkItemProject();
            var queryContext = new Hashtable { { "project", project.Name }, { "workitemtypename", _workItemTypeName } };

            const string wiqlTemplate = @"select [System.Id], {0} from WorkItems where [System.TeamProject] = @project and [System.WorkItemType] = @workitemtypename order by [System.Id]";
            var wiqlFieldList = "[" + string.Join("], [", _fieldReferenceNames) + "]";
            var wiql = string.Format(wiqlTemplate, wiqlFieldList);

            var workItems = project.Store.Query(wiql, queryContext);

            var xw = XmlWriter.Create(Path.Combine(_exportPath, string.Format("{0}.xml", _workItemTypeName)));
            xw.WriteStartElement("WorkItemDataExport");
            xw.WriteAttributeString("workitemtypename", _workItemTypeName);
            foreach (WorkItem workItem in workItems)
            {
                Debug.WriteLine(workItem.Id);
                xw.WriteStartElement("WorkItem");
                xw.WriteAttributeString("id", workItem.Id.ToString());
                foreach (var fieldReferenceName in _fieldReferenceNames)
                {
                    xw.WriteElementString(fieldReferenceName, workItem.Fields[fieldReferenceName].Value.ToString());
                }
                xw.WriteEndElement();
            }
            xw.WriteEndElement();

        }
    }
}