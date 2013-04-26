using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using WitMorph.Actions;

namespace WitMorph
{
    public class ActionSerializer {

        public void Serialize(IEnumerable<MorphAction> actions, string path)
        {
            var settings = new XmlWriterSettings {Indent = true};
            using (var writer = XmlWriter.Create(path, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("morphactions");
                foreach (var action in actions)
                {
                    writer.WriteStartElement(action.GetType().Name.ToLowerInvariant());
                    action.Serialize(writer);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public MorphAction[] Deserialize(string path)
        {
            var context = new DeserializationContext();
            var actions = new List<MorphAction>();

            var xdoc = new XmlDocument();
            xdoc.Load(path);

            if (xdoc.DocumentElement == null || xdoc.DocumentElement.Name != "morphactions")
            {
                throw new InvalidOperationException(string.Format("XML document should have '<morphactions>' document element. '{0}'", path));
            }

            foreach (var actionElement in xdoc.DocumentElement.ChildNodes.OfType<XmlElement>())
            {
                var action = MorphAction.Deserialize(actionElement, context);
                actions.Add(action);
            }

            return actions.ToArray();
        }
    }

    public class DeserializationContext
    {
        private IDictionary<string, ILinkableAction> _linkableActions = new Dictionary<string, ILinkableAction>();

        public ILinkableAction GetLinkableAction(string targetId)
        {
            return _linkableActions[targetId];
        }

        public void RegisterLinkableAction(string serializationId, ILinkableAction linkableAction)
        {
            _linkableActions.Add(serializationId, linkableAction);
        }
       
    }
}