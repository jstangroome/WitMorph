using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WitMorph.Actions
{
    public class RenameWitdMorphAction : IMorphAction
    {
        private readonly string _typeName;
        private readonly string _newName;

        public RenameWitdMorphAction(string typeName, string newName)
        {
            _typeName = typeName;
            _newName = newName;
        }

        public void Execute(ExecutionContext context)
        {
            // most supported implementation would be to run witadmin.exe but that could be tricky with alternate credentials

            var project = context.GetWorkItemProject();

            var workItemType = project.WorkItemTypes[_typeName];

            InternalAdmin.RenameWorkItemType(workItemType, _newName);
        }
    }
}