using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WitMorph.Actions
{
    public class ExportWorkItemDataMorphAction : MorphAction
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

        public string WorkItemTypeName { get { return _workItemTypeName; } }

        public bool AllFields { get { return _allFields; } }

        public IEnumerable<string> FieldReferenceNames { get { return _fieldReferenceNames; } }

        public void AddExportField(string fieldReferenceName)
        {
            if (_allFields)
            {
                throw new InvalidOperationException("All fields are exported");
            }
            _fieldReferenceNames.Add(fieldReferenceName);
        }

        public override void Execute(ExecutionContext context)
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

            using (var xw = XmlWriter.Create(Path.Combine(context.OutputPath, string.Format("{0}-data.xml", _workItemTypeName))))
            {
                xw.WriteStartElement("WorkItemDataExport");
                xw.WriteAttributeString("workitemtypename", _workItemTypeName);
                foreach (WorkItem workItem in workItems)
                {
                    Debug.WriteLine(workItem.Id);
                    xw.WriteStartElement("WorkItem");
                    xw.WriteAttributeString("id", workItem.Id.ToString(CultureInfo.InvariantCulture));
                    foreach (var fieldReferenceName in _fieldReferenceNames)
                    {
                        if (workItem.Fields.Contains(fieldReferenceName))
                        {
                            xw.WriteElementString(fieldReferenceName, Convert.ToString(workItem.Fields[fieldReferenceName].Value));
                        }
                        else
                        {
                            xw.WriteElementString(fieldReferenceName, "[missing]");
                        } 

                    }
                    xw.WriteEndElement();
                }
                xw.WriteEndElement();
            }
        }

        protected override void SerializeCore(XmlWriter writer)
        {
            //TODO skip serialization if not all fields and list is empty
            writer.WriteAttributeString("typename", _workItemTypeName);
            writer.WriteAttributeString("allfields", _allFields.ToString());
            if (!_allFields)
            {
                foreach (var refName in _fieldReferenceNames)
                {
                    writer.WriteStartElement("field");
                    writer.WriteAttributeString("refname", refName);
                    writer.WriteEndElement();
                }
            }
        }

        public static MorphAction Deserialize(XmlElement element, DeserializationContext context)
        {
            var action = new ExportWorkItemDataMorphAction(element.GetAttribute("typename"), Convert.ToBoolean(element.GetAttribute("allfields")));

            foreach (var fieldElement in element.ChildNodes.OfType<XmlElement>().Where(e => e.Name == "field"))
            {
                action.AddExportField(fieldElement.GetAttribute("refname"));
            }

            return action;
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