using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using WitMorph.Model;

namespace WitMorph.Actions
{
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
            //TODO: JH Check if field already exists

            var fieldsElement = FieldsElement(witdElement);
            var originalFieldElement = fieldsElement.SelectSingleNode(string.Format("FIELD[@refname='{0}']", ReferenceName));
            if (originalFieldElement == null)
            {
                AppendImportedChild(FieldsElement(witdElement), _field.Element);
            }

            
        }

        public override void SerializeCore(XmlWriter writer)
        {
            writer.WriteCData(_field.Element.OuterXml);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlElement element, DeserializationContext context)
        {
            var cdata = element.ChildNodes.OfType<XmlCDataSection>().Single();
            var doc = new XmlDocument();
            doc.LoadXml(cdata.Value);
            var field = new WitdField(doc.DocumentElement);
            return new AddFieldModifyWorkItemTypeDefinitionSubAction(field);
        }

        public override string ToString()
        {
            return string.Format("Add field '{0}'", ReferenceName);
        }
    }

    public class ReplaceFieldModifyWorkItemTypeDefinitionSubAction : ModifyWorkItemTypeDefinitionSubAction
    {
        private readonly WitdField _field;

        public ReplaceFieldModifyWorkItemTypeDefinitionSubAction(WitdField field)
        {
            _field = field;
        }

        public string ReferenceName
        {
            get { return _field.ReferenceName; }
        }

        public override void Execute(XmlElement witdElement)
        {
            var fieldsElement = FieldsElement(witdElement);
            var originalFieldElement = fieldsElement.SelectSingleNode(string.Format("FIELD[@refname='{0}']", _field.ReferenceName));
            if (originalFieldElement == null)
            {
                throw new ArgumentException("Original field not found.");
            }

            var importedFieldElement = fieldsElement.OwnerDocument.ImportNode(_field.Element, deep: true);
            fieldsElement.InsertAfter(importedFieldElement, originalFieldElement);
            fieldsElement.RemoveChild(originalFieldElement);
        }

        public override void SerializeCore(XmlWriter writer)
        {
            writer.WriteCData(_field.Element.OuterXml);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlElement element, DeserializationContext context)
        {
            var cdata = element.ChildNodes.OfType<XmlCDataSection>().Single();
            var doc = new XmlDocument();
            doc.LoadXml(cdata.Value);

            var field = new WitdField(doc.DocumentElement);
            return new ReplaceFieldModifyWorkItemTypeDefinitionSubAction(field);
        }

        public override string ToString()
        {
            return string.Format("Replace field '{0}'", ReferenceName);
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

        public override void SerializeCore(XmlWriter writer)
        {
            writer.WriteAttributeString("refname", _referenceName);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlElement element, DeserializationContext context)
        {
            return new RemoveFieldModifyWorkItemTypeDefinitionSubAction(element.GetAttribute("refname"));
        }

        public string ReferenceName
        {
            get { return _referenceName; }
        }

        public override string ToString()
        {
            return string.Format("Remove field '{0}'", ReferenceName);
        }
    }

    public class AddStateModifyWorkItemTypeDefinitionSubAction : ModifyWorkItemTypeDefinitionSubAction
    {
        private readonly WitdState _state;

        public AddStateModifyWorkItemTypeDefinitionSubAction(WitdState state)
        {
            _state = state;
        }

        public override void Execute(XmlElement witdElement)
        {
            var statesElement = StatesElement(witdElement);
            var originalStateElement = statesElement.SelectSingleNode(string.Format("STATE[@value='{0}']", _state.Value));
            if (originalStateElement == null)
            {
                AppendImportedChild(statesElement, _state.Element);
            }
        }

        public override void SerializeCore(XmlWriter writer)
        {
            writer.WriteCData(_state.Element.OuterXml);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlElement element, DeserializationContext context)
        {
            var cdata = element.ChildNodes.OfType<XmlCDataSection>().Single();
            var doc = new XmlDocument();
            doc.LoadXml(cdata.Value);

            var state = new WitdState(doc.DocumentElement);
            return new AddStateModifyWorkItemTypeDefinitionSubAction(state);
        }

        public string Name
        {
            get { return _state.Value; }
        }

        public override string ToString()
        {
            return string.Format("Add state '{0}'", Name);
        }
    }

    public class RemoveStateModifyWorkItemTypeDefinitionSubAction : ModifyWorkItemTypeDefinitionSubAction
    {
        private readonly string _name;

        public RemoveStateModifyWorkItemTypeDefinitionSubAction(string name)
        {
            _name = name;
        }

        public override void Execute(XmlElement witdElement)
        {
            var statesElement = StatesElement(witdElement);
            var stateElement = statesElement.SelectNodes("STATE").Cast<XmlElement>()
                .SingleOrDefault(e => string.Equals(e.GetAttribute("value"), _name, StringComparison.OrdinalIgnoreCase));
            if (stateElement != null)
            {
                statesElement.RemoveChild(stateElement);
                //_isDirty = true;
            }
            var transitionsElement = TransitionsElement(witdElement);
            var transitionElements = transitionsElement.SelectNodes("TRANSITION").Cast<XmlElement>()
                .Where(e => string.Equals(e.GetAttribute("from"), _name, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(e.GetAttribute("to"), _name, StringComparison.OrdinalIgnoreCase));
            foreach (var transitionElement in transitionElements)
            {
                transitionsElement.RemoveChild(transitionElement);
                //_isDirty = true;
            }
        }

        public override void SerializeCore(XmlWriter writer)
        {
            writer.WriteAttributeString("name", _name);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlElement element, DeserializationContext context)
        {
            return new RemoveStateModifyWorkItemTypeDefinitionSubAction(element.GetAttribute("refname"));
        }

        public string Name
        {
            get { return _name; }
        }

        public override string ToString()
        {
            return string.Format("Remove state '{0}'", Name);
        }
    }

    public class AddTransitionModifyWorkItemTypeDefinitionSubAction : ModifyWorkItemTypeDefinitionSubAction
    {
        private readonly string _fromState;
        private readonly string _toState;
        private readonly string _defaultReason;

        public AddTransitionModifyWorkItemTypeDefinitionSubAction(string fromState, string toState, string defaultReason)
        {
            _fromState = fromState;
            _toState = toState;
            _defaultReason = defaultReason;
        }

        public string FromState
        {
            get { return _fromState; }
        }

        public string ToState
        {
            get { return _toState; }
        }

        public override void Execute(XmlElement witdElement)
        {
            var transitionsElement = TransitionsElement(witdElement);
            var transitionElement = transitionsElement.OwnerDocument.CreateElement("TRANSITION");
            transitionElement.SetAttribute("from", FromState);
            transitionElement.SetAttribute("to", ToState);
            var reasonsElement = transitionsElement.OwnerDocument.CreateElement("REASONS");
            transitionElement.AppendChild(reasonsElement);
            var defaultReasonElement = transitionsElement.OwnerDocument.CreateElement("DEFAULTREASON");
            defaultReasonElement.SetAttribute("value", _defaultReason);
            reasonsElement.AppendChild(defaultReasonElement);

            transitionsElement.AppendChild(transitionElement);
        }

        public override void SerializeCore(XmlWriter writer)
        {
            writer.WriteAttributeString("fromstate", _fromState);
            writer.WriteAttributeString("tostate", _toState);
            writer.WriteAttributeString("defaultreason", _defaultReason);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlElement element, DeserializationContext context)
        {
            return new AddTransitionModifyWorkItemTypeDefinitionSubAction(
                element.GetAttribute("fromstate"),
                element.GetAttribute("tostate"),
                element.GetAttribute("defaultreason")
                );
        }

        public override string ToString()
        {
            return string.Format("Add transition from state '{0}' to '{1}' with reason '{2}'", FromState, ToState, _defaultReason);
        }
    }

    public class ReplaceFormModifyWorkItemTypeDefinitionSubAction : ModifyWorkItemTypeDefinitionSubAction
    {
        private readonly XmlElement _formElement;

        public ReplaceFormModifyWorkItemTypeDefinitionSubAction(XmlElement formElement)
        {
            _formElement = formElement;
        }

        public override void Execute(XmlElement witdElement)
        {
            var oldElement = SelectSingleElement(witdElement, "WORKITEMTYPE/FORM");
            InsertImportedChildAfter(oldElement.ParentNode, _formElement, oldElement);
            oldElement.ParentNode.RemoveChild(oldElement);
        }

        public override void SerializeCore(XmlWriter writer)
        {
            writer.WriteCData(_formElement.OuterXml);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlElement element, DeserializationContext context)
        {
            var cdata = element.ChildNodes.OfType<XmlCDataSection>().Single();
            var doc = new XmlDocument();
            doc.LoadXml(cdata.Value);

            return new ReplaceFormModifyWorkItemTypeDefinitionSubAction(doc.DocumentElement);
        }

    }

    [Obsolete("Update states and transitions directly instead.")]
    public class ReplaceWorkflowModifyWorkItemTypeDefinitionSubAction : ModifyWorkItemTypeDefinitionSubAction
    {
        private readonly XmlElement _workflowElement;

        public ReplaceWorkflowModifyWorkItemTypeDefinitionSubAction(XmlElement workflowElement)
        {
            _workflowElement = workflowElement;
        }

        public override void Execute(XmlElement witdElement)
        {
            var oldElement = SelectSingleElement(witdElement, "WORKITEMTYPE/WORKFLOW");
            InsertImportedChildAfter(oldElement.ParentNode, _workflowElement, oldElement);
            oldElement.ParentNode.RemoveChild(oldElement);
        }

        public override void SerializeCore(XmlWriter writer)
        {
            writer.WriteCData(_workflowElement.OuterXml);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlElement element, DeserializationContext context)
        {
            var cdata = element.ChildNodes.OfType<XmlCDataSection>().Single();
            var doc = new XmlDocument();
            doc.LoadXml(cdata.Value);

            return new ReplaceWorkflowModifyWorkItemTypeDefinitionSubAction(doc.DocumentElement);
        }

    }

    public class ModifyWorkItemTypeDefinitionMorphAction : MorphAction
    {
        private readonly string _workItemTypeName;
        private readonly IList<ModifyWorkItemTypeDefinitionSubAction> _subActions = new List<ModifyWorkItemTypeDefinitionSubAction>();
 
        public ModifyWorkItemTypeDefinitionMorphAction(string workItemTypeName)
        {
            _workItemTypeName = workItemTypeName;
        }

        public string WorkItemTypeName
        {
            get { return _workItemTypeName; }
        }

        public void AddFieldDefinition(WitdField field)
        {
            _subActions.Add(new AddFieldModifyWorkItemTypeDefinitionSubAction(field));
        }

        public ILinkableAction AddWorkflowState(WitdState state)
        {
            var newSubAction = new AddStateModifyWorkItemTypeDefinitionSubAction(state);

            var existingAction = _subActions
                .OfType<AddStateModifyWorkItemTypeDefinitionSubAction>()
                .FirstOrDefault(a => a.Name == newSubAction.Name); // match on more than name?

            if (existingAction != null) return existingAction;

            _subActions.Add(newSubAction);
            return newSubAction;
        }

        public ILinkableAction AddWorkflowTransition(string fromState, string toState, string defaultReason)
        {
            var subAction = new AddTransitionModifyWorkItemTypeDefinitionSubAction(fromState, toState, defaultReason);
            _subActions.Add(subAction);
            return subAction;
        }

        public void RemoveFieldDefinition(string fieldReferenceName)
        {
            _subActions.Add(new RemoveFieldModifyWorkItemTypeDefinitionSubAction(fieldReferenceName));
        }

        public void RemoveWorkflowState(string state)
        {
            _subActions.Add(new RemoveStateModifyWorkItemTypeDefinitionSubAction(state));
        }

        public void ReplaceFieldDefinition(WitdField field)
        {
            _subActions.Add(new ReplaceFieldModifyWorkItemTypeDefinitionSubAction(field));
        }

        public void ReplaceWorkflow(XmlElement workflowElement)
        {
            _subActions.Add(new ReplaceWorkflowModifyWorkItemTypeDefinitionSubAction(workflowElement));
        }

        public void ReplaceForm(XmlElement formElement)
        {
            _subActions.Add(new ReplaceFormModifyWorkItemTypeDefinitionSubAction(formElement));
        }

        public override void Execute(ExecutionContext context)
        {
            if (_subActions.Count == 0)
            {
                return;
            }

            var project = context.GetWorkItemProject();
            project.Store.RefreshCache(true);
            var witdElement = project.WorkItemTypes[_workItemTypeName].Export(false).DocumentElement;

            foreach (var action in _subActions)
            {
                action.Execute(witdElement);
            }

            var workItemTypeDefinition = new WorkItemTypeDefinition(witdElement, true); // TODO perform actions on WorkItemTypeDefinition instead of witdElement directly

            var importAction = new ImportWorkItemTypeDefinitionMorphAction(workItemTypeDefinition); 
            importAction.Execute(context);
        }

        protected override void SerializeCore(XmlWriter writer)
        {
            writer.WriteAttributeString("typename", _workItemTypeName);
            foreach (var action in _subActions)
            {
                writer.WriteStartElement(action.GetType().Name.ToLowerInvariant());
                action.Serialize(writer);
                writer.WriteEndElement();
            }
        }

        public static MorphAction Deserialize(XmlElement element, DeserializationContext context)
        {
            var action = new ModifyWorkItemTypeDefinitionMorphAction(element.GetAttribute("typename"));

            foreach (var subActionElement in element.ChildNodes.OfType<XmlElement>())
            {
                var subAction = ModifyWorkItemTypeDefinitionSubAction.Deserialize(subActionElement, context);
                action._subActions.Add(subAction);
            }

            return action;
        }

        public IReadOnlyList<ModifyWorkItemTypeDefinitionSubAction> SubActions
        {
            get { return new ReadOnlyCollection<ModifyWorkItemTypeDefinitionSubAction>(_subActions); }
        }

        public override string ToString()
        {
            if (_subActions.Count == 0)
            {
                return string.Format("No action required. {0}", base.ToString());
            }
            var builder = new StringBuilder();
            builder.AppendLine(string.Format("Import {0} schema change(s) to work item type definition '{1}':", _subActions.Count, _workItemTypeName));
            foreach (var action in _subActions)
            {
                builder.AppendLine(" " + action);
            }
            return builder.ToString();
        }

        public RemoveTransitionModifyWorkItemTypeDefinitionSubAction RemoveWorkflowTransition(string fromState, string toState)
        {
            var subAction = new RemoveTransitionModifyWorkItemTypeDefinitionSubAction(fromState, toState);
            _subActions.Add(subAction);
            return subAction;
        }
    }

    public class RemoveTransitionModifyWorkItemTypeDefinitionSubAction : ModifyWorkItemTypeDefinitionSubAction
    {
        public RemoveTransitionModifyWorkItemTypeDefinitionSubAction(string fromState, string toState)
        {
            _fromState = fromState;
            _toState = toState;
        }

        private readonly string _fromState;
        private readonly string _toState;
        private readonly string _defaultReason;


        public string FromState
        {
            get { return _fromState; }
        }

        public string ToState
        {
            get { return _toState; }
        }

        public override void Execute(XmlElement witdElement)
        {
            var transitionsElement = TransitionsElement(witdElement);
            var transitionElements = transitionsElement.SelectNodes("TRANSITION").Cast<XmlElement>()
                            .Where(e => string.Equals(e.GetAttribute("from"), _fromState, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(e.GetAttribute("to"), _toState, StringComparison.OrdinalIgnoreCase));

            foreach (var transitionElement in transitionElements)
            {
                transitionsElement.RemoveChild(transitionElement);
                //_isDirty = true;
            }
        }

        public override void SerializeCore(XmlWriter writer)
        {
            writer.WriteAttributeString("fromstate", _fromState);
            writer.WriteAttributeString("tostate", _toState);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlElement element, DeserializationContext context)
        {
            return new RemoveTransitionModifyWorkItemTypeDefinitionSubAction(
                element.GetAttribute("fromstate"),
                element.GetAttribute("tostate")
                );
        }

        public override string ToString()
        {
            return string.Format("Remove transition from state '{0}' to '{1}' with reason '{2}'", FromState, ToState, _defaultReason);
        }
    }
}