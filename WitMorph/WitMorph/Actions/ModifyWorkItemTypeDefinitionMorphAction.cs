using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;

namespace WitMorph.Actions
{
    public abstract class ModifyWorkItemTypeDefinitionSubAction
    {
        public abstract void Execute(XmlElement witdElement);

        protected XmlElement SelectSingleElement(XmlElement witdElement, string xpath)
        {
            return (XmlElement)witdElement.SelectSingleNode(xpath);
        }

        protected XmlElement FieldsElement(XmlElement witdElement) { return SelectSingleElement(witdElement, "WORKITEMTYPE/FIELDS"); }

        protected void AppendImportedChild(XmlNode parent, XmlElement child)
        {
            if (parent.OwnerDocument == null)
            {
                throw new ArgumentException("OwnerDocument property value is null.", "parent");
            }
            parent.AppendChild(parent.OwnerDocument.ImportNode(child, deep: true));
        }
    }

    public class AnonymousModifyWorkItemTypeDefinitionSubAction : ModifyWorkItemTypeDefinitionSubAction
    {
        private readonly Action<XmlElement> _action;

        public AnonymousModifyWorkItemTypeDefinitionSubAction(Action<XmlElement> action)
        {
            _action = action;
        }

        public override void Execute(XmlElement witdElement)
        {
            _action(witdElement);
        }
    }

    public class AddFieldModifyWorkItemTypeDefinitionSubAction : ModifyWorkItemTypeDefinitionSubAction
    {
        private readonly WitdField _field;

        public AddFieldModifyWorkItemTypeDefinitionSubAction(WitdField field)
        {
            _field = field;
        }

        public string ReferenceName
        {
            get { return _field.ReferenceName; }
        }

        public override void Execute(XmlElement witdElement)
        {
            AppendImportedChild(FieldsElement(witdElement), _field.Element);
        }
    }

    public class RemoveFieldModifyWorkItemTypeDefinitionSubAction : ModifyWorkItemTypeDefinitionSubAction
    {
        private readonly string _referenceName;

        public RemoveFieldModifyWorkItemTypeDefinitionSubAction(string referenceName)
        {
            _referenceName = referenceName;
        }

        public override void Execute(XmlElement witdElement)
        {
            var fieldsElement = FieldsElement(witdElement);
            var originalFieldElement = fieldsElement.SelectSingleNode(string.Format("FIELD[@refname='{0}']", _referenceName));
            if (originalFieldElement == null)
            {
                throw new ArgumentException("Original field not found.");
            }

            fieldsElement.RemoveChild(originalFieldElement);
        }

        public string ReferenceName
        {
            get { return _referenceName; }
        }
    }

    public static class AnonymousModifyWorkItemTypeDefinitionSubActionExtension
    {
        public static void Add(this IList<ModifyWorkItemTypeDefinitionSubAction> actions, Action<XmlElement> action)
        {
            actions.Add(new AnonymousModifyWorkItemTypeDefinitionSubAction(action));
        }
    }

    public class ModifyWorkItemTypeDefinitionMorphAction : IMorphAction
    {
        private readonly string _workItemTypeName;
        private readonly IList<ModifyWorkItemTypeDefinitionSubAction> _actions = new List<ModifyWorkItemTypeDefinitionSubAction>();

        public ModifyWorkItemTypeDefinitionMorphAction(string workItemTypeName)
        {
            _workItemTypeName = workItemTypeName;
        }

        public string WorkItemTypeName
        {
            get { return _workItemTypeName; }
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

        public void AddFieldDefinition(WitdField field)
        {
            _actions.Add(new AddFieldModifyWorkItemTypeDefinitionSubAction(field));
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
            _actions.Add(new RemoveFieldModifyWorkItemTypeDefinitionSubAction(fieldReferenceName));
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
                action.Execute(witdElement);
            }

            var workItemTypeDefinition = new WorkItemTypeDefinition(witdElement, true); // TODO perform actions on WorkItemTypeDefinition instead of witdElement directly

            var importAction = new ImportWorkItemTypeDefinitionMorphAction(workItemTypeDefinition); 
            importAction.Execute(context);
        }

        public IReadOnlyList<ModifyWorkItemTypeDefinitionSubAction> Actions
        {
            get { return new ReadOnlyCollection<ModifyWorkItemTypeDefinitionSubAction>(_actions); }
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