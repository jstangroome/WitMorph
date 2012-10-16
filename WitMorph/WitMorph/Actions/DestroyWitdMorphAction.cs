using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WitMorph.Actions
{
    public class DestroyWitdMorphAction : IMorphAction
    {
        private readonly string _typeName;

        public DestroyWitdMorphAction(string typeName)
        {
            _typeName = typeName;
        }

        public void Execute(ExecutionContext context)
        {
            // most supported implementation would be to run witadmin.exe but that could be tricky with alternate credentials

            var project = context.GetWorkItemProject();

            var workItemType = project.WorkItemTypes[_typeName];

            InternalAdmin.DestroyWorkItemType(workItemType);
        }
    }
}