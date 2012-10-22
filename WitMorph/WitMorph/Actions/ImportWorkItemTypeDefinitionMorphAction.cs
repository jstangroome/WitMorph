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
            _witdElement = (XmlElement)witdElement.Clone();

            if (SelectSingleElement("WORKITEMTYPE") == null)
            {
                throw new ArgumentException("WORKITEMTYPE element missing.");
            }
        }

        private XmlElement SelectSingleElement(string xpath)
        {
            return (XmlElement)_witdElement.SelectSingleNode(xpath);
        }

 /*


        
        public void AddWorkflowState(XmlElement workflowStateElement)
        {
            AppendImportedChild(StatesElement, workflowStateElement);
            _isDirty = true;
        }



*/
        public void Execute(ExecutionContext context)
        {
            //if (!_isDirty)
            //{
            //    return;
            //}
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
                    Debug.WriteLine("IMPORT: " + e.Message);
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
            //if (!_isDirty)
            //{
            //    return string.Format("No action required. {0}", base.ToString());
            //}
            var name = SelectSingleElement("WORKITEMTYPE").GetAttribute("name");
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