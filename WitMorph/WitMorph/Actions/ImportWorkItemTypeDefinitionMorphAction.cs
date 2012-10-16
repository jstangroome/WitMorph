using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Provision;

namespace WitMorph.Actions
{
    public class ImportWorkItemTypeDefinitionMorphAction : IMorphAction
    {
        private bool _isDirty;
        private readonly XmlElement _witdElement;

        public ImportWorkItemTypeDefinitionMorphAction(XmlElement witdElement) : this(witdElement, forceImport: false) {}

        public ImportWorkItemTypeDefinitionMorphAction(XmlElement witdElement, bool forceImport)
        {
            _witdElement = (XmlElement)witdElement.Clone();
            _isDirty = forceImport;

            if (SelectSingleElement("WORKITEMTYPE") == null)
            {
                throw new ArgumentException("WORKITEMTYPE element missing.");
            }
        }

        private XmlElement SelectSingleElement(string xpath)
        {
            return (XmlElement)_witdElement.SelectSingleNode(xpath);
        }

        private void AppendImportedChild(XmlNode parent, XmlElement child)
        {
            parent.AppendChild(parent.OwnerDocument.ImportNode(child, deep: true));
        }

        private XmlElement FieldsElement { get { return SelectSingleElement("WORKITEMTYPE/FIELDS"); } }
        private XmlElement StatesElement { get { return SelectSingleElement("WORKITEMTYPE/WORKFLOW/STATES"); } }
        private XmlElement TransitionsElement { get { return SelectSingleElement("WORKITEMTYPE/WORKFLOW/TRANSITIONS"); } }

        public void ReplaceFieldDefinition(string originalRefName, XmlElement newFieldElement)
        {
            var originalFieldElement = FieldsElement.SelectSingleNode(string.Format("FIELD[@refname='{0}']", originalRefName));
            if (originalFieldElement == null)
            {
                throw new ArgumentException("Original field not found.");
            }

            var importedFieldElement = FieldsElement.OwnerDocument.ImportNode(newFieldElement, deep: true);
            FieldsElement.InsertAfter(importedFieldElement, originalFieldElement);
            FieldsElement.RemoveChild(originalFieldElement);

            _isDirty = true;
        }
        
        public void AddFieldDefinition(XmlElement fieldElement)
        {
            AppendImportedChild(FieldsElement, fieldElement);
            _isDirty = true;
        }

        public void RemoveFieldDefinition(string fieldReferenceName)
        {
            var originalFieldElement = FieldsElement.SelectSingleNode(string.Format("FIELD[@refname='{0}']", fieldReferenceName));
            if (originalFieldElement == null)
            {
                throw new ArgumentException("Original field not found.");
            }

            FieldsElement.RemoveChild(originalFieldElement);

            _isDirty = true;
        }

        public void AddWorkflowState(XmlElement workflowStateElement)
        {
            AppendImportedChild(StatesElement, workflowStateElement);
            _isDirty = true;
        }

        public void AddWorkflowTransition(string fromState, string toState, string defaultReason)
        {
            var transitionElement = TransitionsElement.OwnerDocument.CreateElement("TRANSITION");
            transitionElement.SetAttribute("from", fromState);
            transitionElement.SetAttribute("to", toState);
            var reasonsElement = TransitionsElement.OwnerDocument.CreateElement("REASONS");
            transitionElement.AppendChild(reasonsElement);
            var defaultReasonElement = TransitionsElement.OwnerDocument.CreateElement("DEFAULTREASON");
            defaultReasonElement.SetAttribute("value", defaultReason);
            reasonsElement.AppendChild(defaultReasonElement);

            TransitionsElement.AppendChild(transitionElement);
            _isDirty = true;
        }

        public void RemoveWorkflowState(string state)
        {
            var stateElement = StatesElement.SelectNodes("STATE").Cast<XmlElement>()
                .SingleOrDefault(e => string.Equals(e.GetAttribute("value"), state, StringComparison.OrdinalIgnoreCase));
            if (stateElement != null)
            {
                StatesElement.RemoveChild(stateElement);
                _isDirty = true;
            }

            var transitionElements = TransitionsElement.SelectNodes("TRANSITION").Cast<XmlElement>()
                .Where(e => string.Equals(e.GetAttribute("from"), state, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(e.GetAttribute("to"), state, StringComparison.OrdinalIgnoreCase));
            foreach (var transitionElement in transitionElements)
            {
                TransitionsElement.RemoveChild(transitionElement);
                _isDirty = true;
            }
        }

        public void ReplaceWorkflow(XmlElement workflowElement)
        {
            var oldElement = SelectSingleElement("WORKITEMTYPE/WORKFLOW");
            AppendImportedChild(oldElement.ParentNode, workflowElement);
            oldElement.ParentNode.RemoveChild(oldElement);
            _isDirty = true;
        }

        public void ReplaceForm(XmlElement formElement)
        {
            var oldElement = SelectSingleElement("WORKITEMTYPE/FORM");
            AppendImportedChild(oldElement.ParentNode, formElement);
            oldElement.ParentNode.RemoveChild(oldElement);
            _isDirty = true;
        }

        public void Execute(ExecutionContext context)
        {
            if (!_isDirty)
            {
                return;
            }
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