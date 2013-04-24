using System;
using System.Collections.Generic;
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
            var actions = new List<MorphAction>();

            using (var reader = XmlReader.Create(path))
            {
                reader.ReadStartElement("morphactions");

                var expectedAssembly = typeof (MorphAction).Assembly;
                
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        var typeName = reader.LocalName;
                        var qualifiedTypeName = string.Format("{0}.{1}", typeof(MorphAction).Namespace, typeName);
                        var actionType = expectedAssembly.GetType(qualifiedTypeName, throwOnError: false, ignoreCase: true);
                        if (actionType == null)
                        {
                            throw new InvalidOperationException(string.Format("Cannot find type '{0}' in assembly '{1}'.", qualifiedTypeName, expectedAssembly));
                        }
                        var deserializeMethod = MorphAction.GetDeserializeMethod(actionType);
                        actions.Add((MorphAction)deserializeMethod.Invoke(null, new object[] { reader }));
                    }
                }

            }

            return actions.ToArray();
        }
    }
}