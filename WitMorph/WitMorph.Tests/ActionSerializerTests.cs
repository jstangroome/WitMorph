using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WitMorph.Differences;
using WitMorph.Model;
using WitMorph.Tests.ProcessTemplates;

namespace WitMorph.Tests
{
    [TestClass]
    public class ActionSerializerTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ActionSerializer_Morph_actions_should_be_recordable_for_inspection_and_playback()
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
}
