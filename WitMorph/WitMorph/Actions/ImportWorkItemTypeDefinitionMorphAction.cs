using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Provision;
using WitMorph.Model;

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
            if (context.TraceLevel >= TraceLevel.Verbose)
            {
                string traceFile;
                int count = 0;
                do
                {
                    count++;
                    traceFile = Path.Combine(context.OutputPath, string.Format("{0}-{1}-definition.xml", WorkItemTypeName, count));

                } while (File.Exists(traceFile));
                _typeDefinition.WITDElement.OwnerDocument.Save(traceFile);
            }

            var project = context.GetWorkItemProject();
            var accumulator = new ImportEventArgsAccumulator();
            project.WorkItemTypes.ImportEventHandler += accumulator.Handler;
            try
            {
                project.WorkItemTypes.Import(_typeDefinition.WITDElement);
                project.Store.RefreshCache(true);
            }
            catch (ProvisionValidationException)
            {
                foreach (var e in accumulator.ImportEventArgs)
                {
                    context.Log("IMPORT: " + e.Message, TraceLevel.Error);
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