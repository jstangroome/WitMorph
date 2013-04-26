using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace WitMorph.Actions
{
    public abstract class MorphAction : ILinkableAction
    {
        private static readonly Assembly MorphActionAssembly = typeof(MorphAction).Assembly;
        private static int _idCounter;

        protected MorphAction()
        {
            GetDeserializeMethod(GetType());

            SerializationId = string.Format("ma{0}", Interlocked.Increment(ref _idCounter));
            LinkedActions = new Collection<ActionLink>();
        }

        private static MethodInfo GetDeserializeMethod(Type actionType)
        {
            var baseType = typeof(MorphAction);
            if (!baseType.IsAssignableFrom(actionType))
            {
                throw new InvalidOperationException(string.Format("Type '{0}' must inherit from base type '{1}'", actionType, baseType));
            }
            var deserializeMethod = actionType.GetMethod("Deserialize", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(XmlElement), typeof(DeserializationContext) }, null);
            if (deserializeMethod == null || !baseType.IsAssignableFrom(deserializeMethod.ReturnType))
            {
                throw new InvalidOperationException(string.Format("Type '{0}' must implement static method '{1} Deserialize(XmlElement element)' on .", actionType.FullName, baseType));
            }
            return deserializeMethod;
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

        public static MorphAction Deserialize(XmlElement element, DeserializationContext context)
        {
            var serializationId = element.GetAttribute("id");

            var typeName = element.Name;
            var qualifiedTypeName = string.Format("{0}.{1}", typeof(MorphAction).Namespace, typeName);
            var actionType = MorphActionAssembly.GetType(qualifiedTypeName, throwOnError: false, ignoreCase: true);
            if (actionType == null)
            {
                throw new InvalidOperationException(string.Format("Cannot find type '{0}' in assembly '{1}'.", qualifiedTypeName, MorphActionAssembly));
            }
            var deserializeMethod = GetDeserializeMethod(actionType);
            var action = (MorphAction)deserializeMethod.Invoke(null, new object[] { element, context });

            foreach ( var linkElement in element.ChildNodes.OfType<XmlElement>().Where(e => e.Name == "linkedaction"))
            {
                var targetid = linkElement.GetAttribute("target");
                var linkType = (ActionLinkType)Enum.Parse(typeof(ActionLinkType), linkElement.GetAttribute("type"), ignoreCase: true);
                action.LinkedActions.Add(new ActionLink(context.GetLinkableAction(targetid), linkType));
            }

            context.RegisterLinkableAction(serializationId, action);
            return action;
        }

        public abstract void Execute(ExecutionContext context);
        protected abstract void SerializeCore(XmlWriter writer);
        public ICollection<ActionLink> LinkedActions { get; private set; }
        public string SerializationId { get; private set; }
    }

}