using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WitMorph.Actions
{
    public class ModifyWorkItemTypeDefinitionMorphAction : IMorphAction
    {
        private readonly string _workItemTypeName;
        private readonly List<Action<XmlElement>> _actions = new List<Action<XmlElement>>();

        public ModifyWorkItemTypeDefinitionMorphAction(string workItemTypeName)
        {
            _workItemTypeName = workItemTypeName;
        }

        private XmlElement SelectSingleElement(XmlElement witdElement, string xpath)
        {
            return (XmlElement)witdElement.SelectSingleNode(xpath);
        }

        private XmlElement FieldsElement(XmlElement witdElement) { return SelectSingleElement(witdElement, "WORKITEMTYPE/FIELDS"); }
        private XmlElement StatesElement(XmlElement witdElement) { return SelectSingleElement(witdElement, "WORKITEMTYPE/WORKFLOW/STATES"); } 
        private XmlElement TransitionsElement(XmlElement witdElement) { return SelectSingleElement(witdElement, "WORKITEMTYPE/WORKFLOW/TRANSITIONS"); }

        private void AppendImportedChild(XmlNode parent, XmlElement child)
        {
            if (parent.OwnerDocument == null)
            {
                throw new ArgumentException("OwnerDocument property value is null.", "parent");
            }
            parent.AppendChild(parent.OwnerDocument.ImportNode(child, deep: true));
        }

        public void AddFieldDefinition(XmlElement fieldElement)
        {
            _actions.Add(e => AppendImportedChild(FieldsElement(e), fieldElement));
        }

        public void AddWorkflowState(XmlElement workflowStateElement)
        {
            _actions.Add(e => AppendImportedChild(StatesElement(e), workflowStateElement));
        }

        public void AddWorkflowTransition(string fromState, string toState, string defaultReason)
        {
            _actions.Add(e =>
                         {
                             var transitionsElement = TransitionsElement(e);
                             var transitionElement = transitionsElement.OwnerDocument.CreateElement("TRANSITION");
                             transitionElement.SetAttribute("from", fromState);
                             transitionElement.SetAttribute("to", toState);
                             var reasonsElement = transitionsElement.OwnerDocument.CreateElement("REASONS");
                             transitionElement.AppendChild(reasonsElement);
                             var defaultReasonElement = transitionsElement.OwnerDocument.CreateElement("DEFAULTREASON");
                             defaultReasonElement.SetAttribute("value", defaultReason);
                             reasonsElement.AppendChild(defaultReasonElement);

                             transitionsElement.AppendChild(transitionElement);
                         });
        }

        public void RemoveFieldDefinition(string fieldReferenceName)
        {
            _actions.Add(e =>
                         {
                             var fieldsElement = FieldsElement(e);
                             var originalFieldElement = fieldsElement.SelectSingleNode(string.Format("FIELD[@refname='{0}']", fieldReferenceName));
                             if (originalFieldElement == null)
                             {
                                 throw new ArgumentException("Original field not found.");
                             }

                             fieldsElement.RemoveChild(originalFieldElement);
                             
                         });
        }

        public void RemoveWorkflowState(string state)
        {
            _actions.Add(witd =>
                         {
                             var statesElement = StatesElement(witd);
                             var stateElement = statesElement.SelectNodes("STATE").Cast<XmlElement>()
                                 .SingleOrDefault(e => string.Equals(e.GetAttribute("value"), state, StringComparison.OrdinalIgnoreCase));
                             if (stateElement != null)
                             {
                                 statesElement.RemoveChild(stateElement);
                                 //_isDirty = true;
                             }
                             var transitionsElement = TransitionsElement(witd);
                             var transitionElements = transitionsElement.SelectNodes("TRANSITION").Cast<XmlElement>()
                                 .Where(e => string.Equals(e.GetAttribute("from"), state, StringComparison.OrdinalIgnoreCase)
                                             || string.Equals(e.GetAttribute("to"), state, StringComparison.OrdinalIgnoreCase));
                             foreach (var transitionElement in transitionElements)
                             {
                                 transitionsElement.RemoveChild(transitionElement);
                                 //_isDirty = true;
                             }
                         });
        }


        public void ReplaceFieldDefinition(string originalRefName, XmlElement newFieldElement)
        {
            _actions.Add(e =>
                         {
                             var fieldsElement = FieldsElement(e);
                             var originalFieldElement = fieldsElement.SelectSingleNode(string.Format("FIELD[@refname='{0}']", originalRefName));
                             if (originalFieldElement == null)
                             {
                                 throw new ArgumentException("Original field not found.");
                             }

                             var importedFieldElement = fieldsElement.OwnerDocument.ImportNode(newFieldElement, deep: true);
                             fieldsElement.InsertAfter(importedFieldElement, originalFieldElement);
                             fieldsElement.RemoveChild(originalFieldElement);
                         });
        }

        public void ReplaceWorkflow(XmlElement workflowElement)
        {
            _actions.Add(witd =>
                         {
                             var oldElement = SelectSingleElement(witd, "WORKITEMTYPE/WORKFLOW");
                             AppendImportedChild(oldElement.ParentNode, workflowElement);
                             oldElement.ParentNode.RemoveChild(oldElement);
                             //_isDirty = true;
                         });
        }

        public void ReplaceForm(XmlElement formElement)
        {
            _actions.Add(witd =>
                         {
                             var oldElement = SelectSingleElement(witd, "WORKITEMTYPE/FORM");
                             AppendImportedChild(oldElement.ParentNode, formElement);
                             oldElement.ParentNode.RemoveChild(oldElement);
                             //_isDirty = true;
                         });
        }

        public void Execute(ExecutionContext context)
        {
            if (_actions.Count == 0)
            {
                return;
            }

            var project = context.GetWorkItemProject();
            var witdElement = project.WorkItemTypes[_workItemTypeName].Export(false).DocumentElement;

            foreach (var action in _actions)
            {
                action(witdElement);
            }

            var importAction = new ImportWorkItemTypeDefinitionMorphAction(new WorkItemTypeDefinition(witdElement)); // TODO perform actions on WorkItemTypeDefinition instead of witdElement directly
            importAction.Execute(context);
        }

        public override string ToString()
        {
            if (_actions.Count == 0)
            {
                return string.Format("No action required. {0}", base.ToString());
            }
            return string.Format("Import {0} schema change(s) to work item type definition '{1}'", _actions.Count, _workItemTypeName); //TODO list details of changes
        }
    }

}