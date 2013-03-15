using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Provision;

namespace WitMorph.Actions
{
    public class ImportWorkItemTypeDefinitionMorphAction : IMorphAction
    {
        private readonly WorkItemTypeDefinition _typeDefinition;

        public ImportWorkItemTypeDefinitionMorphAction(WorkItemTypeDefinition typeDefinition)
        {
            _typeDefinition = typeDefinition;
        }

        public string WorkItemTypeName
        {
            get { return _typeDefinition.Name; }
        }

        public void Execute(ExecutionContext context)
        {
            var project = context.GetWorkItemProject();
            var accumulator = new ImportEventArgsAccumulator();
            project.WorkItemTypes.ImportEventHandler += accumulator.Handler;
            try
            {
                project.WorkItemTypes.Import(_typeDefinition.WITDElement);
            }
            catch (ProvisionValidationException)
            {
                foreach (var e in accumulator.ImportEventArgs)
                {
                    Debug.WriteLine("IMPORT: " + e.Message); // TODO log errors better, perhaps ExecutionContext.Log(...)
                }
                throw;
            }
            finally
            {
                project.WorkItemTypes.ImportEventHandler -= accumulator.Handler;
            }
        }

        public override string ToString()
        {
            return string.Format("Import work item type definition '{0}'", _typeDefinition.Name);
        }

        class ImportEventArgsAccumulator
        {
            public ImportEventArgsAccumulator()
            {
                ImportEventArgs = new List<ImportEventArgs>();
            }
            
            public void Handler(object sender, ImportEventArgs eventArgs)
            {
                ImportEventArgs.Add(eventArgs);
            }

            public List<ImportEventArgs> ImportEventArgs { get; set; }
        }

    }
}