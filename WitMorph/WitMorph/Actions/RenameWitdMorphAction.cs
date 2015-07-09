using System.Xml;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WitMorph.Actions
{
    using System.Threading;

    public class RenameWitdMorphAction : MorphAction
    {
        private readonly string _typeName;
        private readonly string _newName;

        public RenameWitdMorphAction(string typeName, string newName)
        {
            _typeName = typeName;
            _newName = newName;
        }

        public string TypeName
        {
            get { return _typeName; }
        }

        public string NewName
        {
            get { return _newName; }
        }

        public override void Execute(ExecutionContext context)
        {
            // most supported implementation would be to run witadmin.exe but that could be tricky with alternate credentials

            var project = context.GetWorkItemProject();

            var workItemType = project.WorkItemTypes[_typeName];

            InternalAdmin.RenameWorkItemType(workItemType, _newName);

            Thread.Sleep(5000);
            project.Store.RefreshCache();
            project.Store.SyncToCache();
        }

        protected override void SerializeCore(XmlWriter writer)
        {
            writer.WriteAttributeString("typename", _typeName);
            writer.WriteAttributeString("newname", _newName);
        }

        public static MorphAction Deserialize(XmlElement element, DeserializationContext context)
        {
            return new RenameWitdMorphAction(element.GetAttribute("typename"), element.GetAttribute("newname"));
        }

        public override string ToString()
        {
            return string.Format("Rename work item type definition from '{0}' to '{1}'", _typeName, _newName);
        }
    }
}