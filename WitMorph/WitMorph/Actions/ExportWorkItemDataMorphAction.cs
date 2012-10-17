using System;
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
        private readonly bool _allFields;
        private readonly List<string> _fieldReferenceNames = new List<string>();

        public ExportWorkItemDataMorphAction(string workItemTypeName) : this (workItemTypeName, allFields:false) {}

        public ExportWorkItemDataMorphAction(string workItemTypeName, bool allFields)
        {
            _workItemTypeName = workItemTypeName;
            _allFields = allFields;
        }

        public void AddExportField(string fieldReferenceName)
        {
            _fieldReferenceNames.Add(fieldReferenceName);
        }

        public void Execute(ExecutionContext context)
        {
            if (!_allFields && _fieldReferenceNames.Count == 0)
            {
                return;
            }

            var project = context.GetWorkItemProject();
            var queryContext = new Hashtable { { "project", project.Name }, { "workitemtypename", _workItemTypeName } };

            if (_allFields)
            {
                _fieldReferenceNames.Clear();
                foreach (FieldDefinition fieldDef in project.WorkItemTypes[_workItemTypeName].FieldDefinitions)
                {
                    if (!fieldDef.ReferenceName.Equals("System.Id", StringComparison.OrdinalIgnoreCase) && !fieldDef.IsComputed)
                    {
                        _fieldReferenceNames.Add(fieldDef.ReferenceName);
                    }
                }
            }

            const string wiqlTemplate = @"select [System.Id], {0} from WorkItems where [System.TeamProject] = @project and [System.WorkItemType] = @workitemtypename order by [System.Id]";
            var wiqlFieldList = BuildWiqlFieldList();
            var wiql = string.Format(wiqlTemplate, wiqlFieldList);

            var workItems = project.Store.Query(wiql, queryContext);

            var xw = XmlWriter.Create(Path.Combine(context.OutputPath, string.Format("{0}.xml", _workItemTypeName)));
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

        private string BuildWiqlFieldList()
        {
            return "[" + string.Join("], [", _fieldReferenceNames) + "]";
        }

        public override string ToString()
        {
            if (_allFields)
            {
                return string.Format("Export data for work items of type '{0}' in all fields", _workItemTypeName);
            }
            if (_fieldReferenceNames.Count == 0)
            {
                return string.Format("No action required. {0}", base.ToString());
            }
            return string.Format("Export data for work items of type '{0}' in fields: {1}", _workItemTypeName, BuildWiqlFieldList());
        }
    }
}