using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace WitMorph.Actions
{
    public abstract class ModifyWorkItemTypeDefinitionSubAction : ILinkableAction
    {
        private static readonly Assembly SubActionAssembly = typeof(ModifyWorkItemTypeDefinitionSubAction).Assembly;
        private static int _idCounter;

        protected ModifyWorkItemTypeDefinitionSubAction()
        {
            GetDeserializeMethod(GetType());

            SerializationId = string.Format("mwitdsa{0}", Interlocked.Increment(ref _idCounter));
            LinkedActions = new Collection<ActionLink>();
        }

        private static MethodInfo GetDeserializeMethod(Type subActionType)
        {
            if (!typeof (ModifyWorkItemTypeDefinitionSubAction).IsAssignableFrom(subActionType))
            {
                throw new InvalidOperationException(string.Format("Type '{0}' must inherit from base type '{1}'", subActionType, typeof (ModifyWorkItemStateMorphAction)));
            }
            var deserializeMethod = subActionType.GetMethod("Deserialize", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(XmlElement), typeof(DeserializationContext) }, null);
            if (deserializeMethod == null || !typeof(ModifyWorkItemTypeDefinitionSubAction).IsAssignableFrom(deserializeMethod.ReturnType))
            {
                throw new InvalidOperationException(string.Format("Type '{0}' must implement static method '{1} Deserialize(XmlElement element)' on .", subActionType.FullName, typeof(ModifyWorkItemTypeDefinitionSubAction)));
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

        public void Serialize(XmlWriter writer)
        {
            writer.WriteAttributeString("id", SerializationId);
            SerializeCore(writer);
            foreach (var link in LinkedActions)
            {
                writer.WriteStartElement("linkedaction");
                writer.WriteAttributeString("type", link.Type.ToString());
                writer.WriteAttributeString("target", link.Target.SerializationId);
                writer.WriteEndElement();
            }
        }

        public abstract void SerializeCore(XmlWriter writer);
        public ICollection<ActionLink> LinkedActions { get; private set; }
        public string SerializationId { get; private set; }

        public static ModifyWorkItemTypeDefinitionSubAction Deserialize(XmlElement element, DeserializationContext context)
        {
            var serializationId = element.GetAttribute("id");

            var typeName = element.Name;
            var qualifiedTypeName = string.Format("{0}.{1}", typeof(ModifyWorkItemTypeDefinitionSubAction).Namespace, typeName);
            var subActionType = SubActionAssembly.GetType(qualifiedTypeName, throwOnError: false, ignoreCase: true);
            if (subActionType == null)
            {
                throw new InvalidOperationException(string.Format("Cannot find type '{0}' in assembly '{1}'.", qualifiedTypeName, SubActionAssembly));
            }
            var deserializeMethod = GetDeserializeMethod(subActionType);
            var subAction = (ModifyWorkItemTypeDefinitionSubAction)deserializeMethod.Invoke(null, new object[] { element, context });

            foreach (var linkElement in element.ChildNodes.OfType<XmlElement>().Where(e => e.Name == "linkedaction"))
            {
                var targetid = linkElement.GetAttribute("target");
                var linkType = (ActionLinkType)Enum.Parse(typeof(ActionLinkType), linkElement.GetAttribute("type"), ignoreCase: true);
                subAction.LinkedActions.Add(new ActionLink(context.GetLinkableAction(targetid), linkType));
            }

            context.RegisterLinkableAction(serializationId, subAction);
            return subAction;
        }
    }
}