using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Provision;

namespace WitMorph.Actions
{
    public class ImportWorkItemTypeDefinitionMorphAction : IMorphAction
    {
        private readonly XmlElement _witdElement;

        public ImportWorkItemTypeDefinitionMorphAction(XmlElement witdElement)
        {
            if (witdElement.SelectSingleNode("WORKITEMTYPE") == null)
            {
                throw new ArgumentException("WORKITEMTYPE element missing.");
            }

            _witdElement = (XmlElement)witdElement.Clone();
        }

        public void Execute(ExecutionContext context)
        {
            var project = context.GetWorkItemProject();
            var accumulator = new ImportEventArgsAccumulator();
            project.WorkItemTypes.ImportEventHandler += accumulator.Handler;
            try
            {
                project.WorkItemTypes.Import(_witdElement);
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
            var name = ((XmlElement)_witdElement.SelectSingleNode("WORKITEMTYPE")).GetAttribute("name");
            return string.Format("Import work item type definition '{0}'", name);
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