using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using WitMorph.Model;

namespace WitMorph.Actions
{
    public abstract class ModifyWorkItemTypeDefinitionSubAction
    {
        protected ModifyWorkItemTypeDefinitionSubAction()
        {
            GetDeserializeMethod(GetType());
        }

        public static MethodInfo GetDeserializeMethod(Type subActionType)
        {
            if (!typeof (ModifyWorkItemTypeDefinitionSubAction).IsAssignableFrom(subActionType))
            {
                throw new InvalidOperationException(string.Format("Type '{0}' must inherit from base type '{1}'", subActionType, typeof (ModifyWorkItemStateMorphAction)));
            }
            var deserializeMethod = subActionType.GetMethod("Deserialize", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(XmlReader) }, null);
            if (deserializeMethod == null || !typeof(ModifyWorkItemTypeDefinitionSubAction).IsAssignableFrom(deserializeMethod.ReturnType))
            {
                throw new InvalidOperationException(string.Format("Type '{0} must implement static method '{1} Deserialize(XmlReader reader)' on .", subActionType.FullName, typeof(ModifyWorkItemTypeDefinitionSubAction)));
            }
            return deserializeMethod;
        }
        
        public abstract void Execute(XmlElement witdElement);

        protected XmlElement SelectSingleElement(XmlElement witdElement, string xpath)
        {
            return (XmlElement)witdElement.SelectSingleNode(xpath);
        }

        protected XmlElement FieldsElement(XmlElement witdElement) { return SelectSingleElement(witdElement, "WORKITEMTYPE/FIELDS"); }
        protected XmlElement StatesElement(XmlElement witdElement) { return SelectSingleElement(witdElement, "WORKITEMTYPE/WORKFLOW/STATES"); }
        protected XmlElement TransitionsElement(XmlElement witdElement) { return SelectSingleElement(witdElement, "WORKITEMTYPE/WORKFLOW/TRANSITIONS"); }

        protected void AppendImportedChild(XmlNode parent, XmlNode child)
        {
            if (parent.OwnerDocument == null)
            {
                throw new ArgumentException("OwnerDocument property value is null.", "parent");
            }
            parent.AppendChild(parent.OwnerDocument.ImportNode(child, deep: true));
        }

        protected void InsertImportedChildAfter(XmlNode parent, XmlNode child, XmlNode refChild)
        {
            parent.InsertAfter(parent.OwnerDocument.ImportNode(child, deep: true), refChild);
        }

        public abstract void Serialize(XmlWriter writer);
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

        public override void Serialize(XmlWriter writer)
        {
            writer.WriteCData(_field.Element.OuterXml);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlReader reader)
        {
            reader.Read();
            if (reader.NodeType != XmlNodeType.CDATA)
            {
                throw new InvalidOperationException(string.Format("Expected CDATA node but was '{0}'", reader.NodeType));
            }
            var doc = new XmlDocument();
            doc.LoadXml(reader.Value);
            reader.Read();

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

        public override void Serialize(XmlWriter writer)
        {
            writer.WriteCData(_field.Element.OuterXml);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlReader reader)
        {
            reader.Read();
            if (reader.NodeType != XmlNodeType.CDATA)
            {
                throw new InvalidOperationException(string.Format("Expected CDATA node but was '{0}'", reader.NodeType));
            }
            var doc = new XmlDocument();
            doc.LoadXml(reader.Value);
            reader.Read();

            var field = new WitdField(doc.DocumentElement);
            return new ReplaceFieldModifyWorkItemTypeDefinitionSubAction(field);
        }

        public override string ToString()
        {
            return string.Format("Replace field '{0}'", ReferenceName);
        }
    }

    public static class XmlReaderExtension
    {
        public static string ReadCData(this XmlReader reader)
        {
            if (reader.NodeType != XmlNodeType.CDATA)
            {
                throw new InvalidOperationException(string.Format("Expected CDATA node but was '{0}'", reader.NodeType));
            }
            return reader.Value;
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

        public override void Serialize(XmlWriter writer)
        {
            writer.WriteAttributeString("refname", _referenceName);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlReader reader)
        {
            return new RemoveFieldModifyWorkItemTypeDefinitionSubAction(reader.GetAttribute("refname"));
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
            AppendImportedChild(StatesElement(witdElement), _state.Element);
        }

        public override void Serialize(XmlWriter writer)
        {
            writer.WriteCData(_state.Element.OuterXml);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlReader reader)
        {
            reader.Read();
            if (reader.NodeType != XmlNodeType.CDATA)
            {
                throw new InvalidOperationException(string.Format("Expected CDATA node but was '{0}'", reader.NodeType));
            }
            var doc = new XmlDocument();
            doc.LoadXml(reader.Value);
            reader.Read();

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

        public override void Serialize(XmlWriter writer)
        {
            writer.WriteAttributeString("name", _name);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlReader reader)
        {
            return new RemoveStateModifyWorkItemTypeDefinitionSubAction(reader.GetAttribute("refname"));
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

        public override void Serialize(XmlWriter writer)
        {
            writer.WriteAttributeString("fromstate", _fromState);
            writer.WriteAttributeString("tostate", _toState);
            writer.WriteAttributeString("defaultreason", _defaultReason);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlReader reader)
        {
            return new AddTransitionModifyWorkItemTypeDefinitionSubAction(
                reader.GetAttribute("fromstate"),
                reader.GetAttribute("tostate"),
                reader.GetAttribute("defaultreason")
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

        public override void Serialize(XmlWriter writer)
        {
            writer.WriteCData(_formElement.OuterXml);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlReader reader)
        {
            reader.Read();
            if (reader.NodeType != XmlNodeType.CDATA)
            {
                throw new InvalidOperationException(string.Format("Expected CDATA node but was '{0}'", reader.NodeType));
            }
            var doc = new XmlDocument();
            doc.LoadXml(reader.Value);
            reader.Read();

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

        public override void Serialize(XmlWriter writer)
        {
            writer.WriteCData(_workflowElement.OuterXml);
        }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlReader reader)
        {
            reader.Read();
            if (reader.NodeType != XmlNodeType.CDATA)
            {
                throw new InvalidOperationException(string.Format("Expected CDATA node but was '{0}'", reader.NodeType));
            }
            var doc = new XmlDocument();
            doc.LoadXml(reader.Value);
            reader.Read();

            return new ReplaceWorkflowModifyWorkItemTypeDefinitionSubAction(doc.DocumentElement);
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

        public void AddFieldDefinition(WitdField field)
        {
            _actions.Add(new AddFieldModifyWorkItemTypeDefinitionSubAction(field));
        }

        public void AddWorkflowState(WitdState state)
        {
            _actions.Add(new AddStateModifyWorkItemTypeDefinitionSubAction(state));
        }

        public void AddWorkflowTransition(string fromState, string toState, string defaultReason)
        {
            _actions.Add(new AddTransitionModifyWorkItemTypeDefinitionSubAction(fromState, toState, defaultReason));
        }

        public void RemoveFieldDefinition(string fieldReferenceName)
        {
            _actions.Add(new RemoveFieldModifyWorkItemTypeDefinitionSubAction(fieldReferenceName));
        }

        public void RemoveWorkflowState(string state)
        {
            _actions.Add(new RemoveStateModifyWorkItemTypeDefinitionSubAction(state));
        }


        public void ReplaceFieldDefinition(WitdField field)
        {
            _actions.Add(new ReplaceFieldModifyWorkItemTypeDefinitionSubAction(field));
        }

        public void ReplaceWorkflow(XmlElement workflowElement)
        {
            _actions.Add(new ReplaceWorkflowModifyWorkItemTypeDefinitionSubAction(workflowElement));
        }

        public void ReplaceForm(XmlElement formElement)
        {
            _actions.Add(new ReplaceFormModifyWorkItemTypeDefinitionSubAction(formElement));
        }

        public void Execute(ExecutionContext context)
        {
            if (_actions.Count == 0)
            {
                return;
            }

            var project = context.GetWorkItemProject();
            project.Store.RefreshCache(true);
            var witdElement = project.WorkItemTypes[_workItemTypeName].Export(false).DocumentElement;

            foreach (var action in _actions)
            {
                action.Execute(witdElement);
            }

            var workItemTypeDefinition = new WorkItemTypeDefinition(witdElement, true); // TODO perform actions on WorkItemTypeDefinition instead of witdElement directly

            var importAction = new ImportWorkItemTypeDefinitionMorphAction(workItemTypeDefinition); 
            importAction.Execute(context);
        }

        public void Serialize(XmlWriter writer)
        {
            writer.WriteAttributeString("typename", _workItemTypeName);
            foreach (var action in _actions)
            {
                writer.WriteStartElement(action.GetType().Name.ToLowerInvariant());
                action.Serialize(writer);
                writer.WriteEndElement();
            }
        }

        public static IMorphAction Deserialize(XmlReader reader)
        {
            var action = new ModifyWorkItemTypeDefinitionMorphAction(reader.GetAttribute("typename"));
            
            var expectedAssembly = action.GetType().Assembly;

            while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    var typeName = reader.LocalName;
                    var qualifiedTypeName = string.Format("{0}.{1}", action.GetType().Namespace, typeName);
                    var actionType = expectedAssembly.GetType(qualifiedTypeName, throwOnError: false, ignoreCase: true);
                    if (actionType == null)
                    {
                        throw new InvalidOperationException(string.Format("Cannot find type '{0}' in assembly '{1}'.", qualifiedTypeName, expectedAssembly));
                    }
                    var deserializeMethod = ModifyWorkItemTypeDefinitionSubAction.GetDeserializeMethod(actionType);
                    action._actions.Add((ModifyWorkItemTypeDefinitionSubAction)deserializeMethod.Invoke(null, new object[] { reader }));
                }
            }

            return action;
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
            var builder = new StringBuilder();
            builder.AppendLine(string.Format("Import {0} schema change(s) to work item type definition '{1}':", _actions.Count, _workItemTypeName));
            foreach (var action in _actions)
            {
                builder.AppendLine(" " + action);
            }
            return builder.ToString();
        }
    }

}