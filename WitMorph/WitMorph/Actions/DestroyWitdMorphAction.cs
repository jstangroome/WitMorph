using System.Xml;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WitMorph.Actions
{
    public class DestroyWitdMorphAction : MorphAction
    {
        private readonly string _typeName;

        public DestroyWitdMorphAction(string typeName)
        {
            _typeName = typeName;
        }

        public string TypeName { get { return _typeName; } }

        public override void Execute(ExecutionContext context)
        {
            // most supported implementation would be to run witadmin.exe but that could be tricky with alternate credentials

            var project = context.GetWorkItemProject();

            var workItemType = project.WorkItemTypes[_typeName];

            InternalAdmin.DestroyWorkItemType(workItemType);
            project.Store.RefreshCache(true);
        }

        public override void Serialize(XmlWriter writer)
        {
            writer.WriteAttributeString("typename", _typeName);
        }

        public static MorphAction Deserialize(XmlReader reader)
        {
            return new DestroyWitdMorphAction(reader.GetAttribute("typename"));
        }

        public override string ToString()
        {
            return string.Format("Destroy work item type '{0}'", _typeName);
        }
    }
}