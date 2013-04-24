using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WitMorph.Actions;
using WitMorph.Differences;
using WitMorph.Model;
using WitMorph.Tests.ProcessTemplates;

namespace WitMorph.Tests
{
    [TestClass]
    public class Class1
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Morph_actions_should_be_recordable_for_inspection_and_playback()
        {
            WorkItemTypeDefinition workItemTypeDefinition;
            WitdField field;
            WitdState state;
            using (var template = EmbeddedProcessTemplate.Scrum2())
            {
                var templateReader = new ProcessTemplateReader(template.TemplatePath);
                workItemTypeDefinition = templateReader.WorkItemTypeDefinitions.First();
                field = workItemTypeDefinition.Fields.First(f => !f.ReferenceName.StartsWith("System."));
                state = workItemTypeDefinition.States.First();
            }

            var morphEngine = new MorphEngine();
            var differences = new IDifference[]
                              {
                                  new AddedWorkItemTypeDefinitionDifference(workItemTypeDefinition),
                                  new RenamedWorkItemTypeDefinitionDifference("User Story", "Product Backlog Item"),
                                  new RenamedWorkItemFieldDifference(workItemTypeDefinition.Name, field.ReferenceName, field),
                                  new RenamedWorkItemStateDifference(workItemTypeDefinition.Name, state.Value, state),
                                  new ChangedWorkItemFieldDifference(workItemTypeDefinition.Name, field.ReferenceName, field),
                                  new ChangedWorkItemFormDifference(workItemTypeDefinition.Name, workItemTypeDefinition.FormElement),
                                  new ChangedWorkItemWorkflowDifference(workItemTypeDefinition.Name, workItemTypeDefinition.WorkflowElement),
                                  new RemovedWorkItemTypeDefinitionDifference("Issue")
                              };
            var actions = morphEngine.GenerateActions(differences);

            var path = Path.Combine(TestContext.TestRunResultsDirectory, TestContext.TestName + ".actions.xml");

            var actionSerializer = new ActionSerializer();
            actionSerializer.Serialize(actions, path);

            var rehydratedActions = actionSerializer.Deserialize(path);
            
            Assert.AreNotEqual(0, actions.Count(), "No actions to serialize then deserialize.");
            Assert.AreEqual(actions.Count(), rehydratedActions.Length);
        }

    }

    public class ActionSerializer {
        public void Serialize(IEnumerable<IMorphAction> actions, string path)
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

        public IMorphAction[] Deserialize(string path)
        {
            var actions = new List<IMorphAction>();

            using (var reader = XmlReader.Create(path))
            {
                reader.ReadStartElement("morphactions");

                var expectedAssembly = typeof (IMorphAction).Assembly;
                
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        var typeName = reader.LocalName;
                        var qualifiedTypeName = string.Format("{0}.{1}", typeof(IMorphAction).Namespace, typeName);
                        var actionType = expectedAssembly.GetType(qualifiedTypeName, throwOnError: false, ignoreCase: true);
                        if (actionType == null)
                        {
                            throw new InvalidOperationException(string.Format("Cannot find type '{0}' in assembly '{1}'.", qualifiedTypeName, expectedAssembly));
                        }
                        var deserializeMethod = actionType.GetMethod("Deserialize", BindingFlags.Static | BindingFlags.Public, null, new [] {typeof(XmlReader)}, null);
                        if (deserializeMethod == null)
                        {
                            throw new InvalidOperationException(string.Format("Cannot find static method 'Deserialize(XmlReader reader)' on type '{0}'.", actionType.FullName));
                        }
                        if (!typeof (IMorphAction).IsAssignableFrom(deserializeMethod.ReturnType))
                        {
                            throw new InvalidOperationException(string.Format("Deserialize method on type '{0}' must return '{1}'.", actionType.FullName, typeof(IMorphAction)));
                        }
                        actions.Add((IMorphAction)deserializeMethod.Invoke(null, new object[] { reader }));
                    }
                }

            }

            return actions.ToArray();
        }
    }
}
