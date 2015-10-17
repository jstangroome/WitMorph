using System.Collections;
using System.Diagnostics;
using System.Xml;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WitMorph.Actions
{
    using Microsoft.TeamFoundation.Client.CommandLine;

    public class CopyWorkItemDataMorphAction : MorphAction
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

        public override void Execute(ExecutionContext context)
        {
            var project = context.GetWorkItemProject();
            var queryContext = new Hashtable { { "project", project.Name }, { "workitemtypename", _workItemTypeName }};

            const string wiqlTemplate = @"select [System.Id], [{0}], [{1}] from WorkItems where [System.TeamProject] = @project and [System.WorkItemType] = @workitemtypename order by [System.Id]";
            var wiql = string.Format(wiqlTemplate, _fromFieldReferenceName, _toFieldReferenceName);

            var workItems = project.Store.Query(wiql, queryContext);

            WorkItemFieldDataConvertor convertor = new WorkItemFieldDataConvertor();
            
            foreach (WorkItem workItem in workItems)
            {
                Debug.WriteLine(workItem.Id);
                var hasFromField = workItem.Fields.Contains(_fromFieldReferenceName);
                var hasToField = workItem.Fields.Contains(_toFieldReferenceName);
                if (hasFromField && hasToField)
                {
                    if (!workItem.IsOpen) workItem.Open();
                    convertor.ConvertFieldData(workItem.Fields[_fromFieldReferenceName], workItem.Fields[_toFieldReferenceName]);
                    workItem.Save();
                }
                else
                {
                    if (!hasFromField)
                    {
                        context.Log(string.Format("Work item '{0}' is missing field '{1}'.", workItem.Id, _fromFieldReferenceName), TraceLevel.Warning);
                    }
                    if (!hasToField)
                    {
                        context.Log(string.Format("Work item '{0}' is missing field '{1}'.", workItem.Id, _toFieldReferenceName), TraceLevel.Warning);
                    }
                }
            }

        }

        protected override void SerializeCore(XmlWriter writer)
        {
            writer.WriteAttributeString("typename", _workItemTypeName);
            writer.WriteAttributeString("fromfieldrefname", _fromFieldReferenceName);
            writer.WriteAttributeString("tofieldrefname", _toFieldReferenceName);
        }

        public static MorphAction Deserialize(XmlElement element, DeserializationContext context)
        {
            return new CopyWorkItemDataMorphAction(
                element.GetAttribute("typename"),
                element.GetAttribute("fromfieldrefname"),
                element.GetAttribute("tofieldrefname")
                );
        }

        public override string ToString()
        {
            return string.Format("Copy data from field '{0}' to field '{1}' for work items of type '{2}'", _fromFieldReferenceName, _toFieldReferenceName, _workItemTypeName);
        }
    }
}