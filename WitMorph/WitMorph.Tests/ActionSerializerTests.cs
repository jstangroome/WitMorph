using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WitMorph.Actions;
using WitMorph.Differences;
using WitMorph.Model;
using WitMorph.Tests.ProcessTemplates;

namespace WitMorph.Tests
{
    [TestClass]
    public class ActionSerializerTests
    {
        public TestContext TestContext { get; set; }

        private IDifference[] _differences;
        private MorphAction[] _actions;
        private string _serializedActionsPath;
        private MorphAction[] _deserializedActions;

        private void SetupDifferences()
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

            _differences = new IDifference[]
                           {
                               new AddedWorkItemTypeDefinitionDifference(workItemTypeDefinition),
                               new AddedWorkItemStateDifference(workItemTypeDefinition.Name, state),
                               new AddedWorkItemFieldDifference(workItemTypeDefinition.Name, field),
                               new RenamedWorkItemTypeDefinitionDifference("User Story", "Product Backlog Item"),
                               new RenamedWorkItemFieldDifference(workItemTypeDefinition.Name, field.ReferenceName, field),
                               new RenamedWorkItemStateDifference(workItemTypeDefinition.Name, state.Value, state),
                               new ChangedWorkItemFieldDifference(workItemTypeDefinition.Name, field.ReferenceName, field),
                               new ChangedWorkItemStateDifference(workItemTypeDefinition.Name, state.Value, state),
                               new ChangedWorkItemFormDifference(workItemTypeDefinition.Name, workItemTypeDefinition.FormElement),
                               new ChangedWorkItemWorkflowDifference(workItemTypeDefinition.Name, workItemTypeDefinition.WorkflowElement),
                               new RemovedWorkItemTypeDefinitionDifference("Issue"),
                               new RemovedWorkItemFieldDifference(workItemTypeDefinition.Name, field.ReferenceName),
                               new RemovedWorkItemStateDifference(state.Value), //workItemTypeDefinition.Name, 
                           };

        }

        private void GenerateActions()
        {
            SetupDifferences();
            var morphEngine = new MorphEngine();
            _actions = morphEngine.GenerateActions(_differences);
        }

        private void SerializeActions()
        {
            GenerateActions();
            _serializedActionsPath = Path.Combine(TestContext.TestRunResultsDirectory, TestContext.TestName + ".actions.xml");

            var actionSerializer = new ActionSerializer();
            actionSerializer.Serialize(_actions, _serializedActionsPath);
        }

        private void DeserializeActions()
        {
            SerializeActions();

            var actionSerializer = new ActionSerializer();
            _deserializedActions = actionSerializer.Deserialize(_serializedActionsPath);
        }

        [TestMethod]
        public void ActionSerializer_should_have_test_actions_to_serialize()
        {
            GenerateActions();
            Assert.AreNotEqual(0, _actions.Length, "No actions to serialize then deserialize.");
        }

        [TestMethod]
        public void ActionSerializer_should_use_all_IDifference_types_to_test_serializtion()
        {
            SetupDifferences();

            var allDifferenceTypes = typeof(IDifference).Assembly.GetTypes()
                .Where(t => typeof(IDifference).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            var missingDifferenceTypes = allDifferenceTypes.Where(t => !_differences.Select(d => d.GetType()).Contains(t)).ToArray();
            if (missingDifferenceTypes.Any())
            {
                Assert.Fail("IDifference implementation '{0}' not covered by test data.", missingDifferenceTypes.First());
            }
        }

        [TestMethod]
        public void ActionSerializer_should_use_all_MorphAction_types_to_test_serializtion()
        {
            GenerateActions();

            var allMorphActionTypes = typeof(MorphAction).Assembly.GetTypes()
                .Where(t => typeof(MorphAction).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            var missingMorphActionTypes = allMorphActionTypes.Where(t => !_actions.Select(a => a.GetType()).Contains(t)).ToArray();
            if (missingMorphActionTypes.Any())
            {
                Assert.Fail("MorphAction implementation '{0}' not covered by test data.", missingMorphActionTypes.First());
            }
        }

        [TestMethod]
        public void ActionSerializer_should_use_all_ModifyWorkItemTypeDefinitionSubAction_types_to_test_serializtion()
        {
            GenerateActions();

            var allSubActionTypes = typeof(ModifyWorkItemTypeDefinitionSubAction).Assembly.GetTypes()
                .Where(t => typeof(ModifyWorkItemTypeDefinitionSubAction).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            var subActions = _actions.OfType<ModifyWorkItemTypeDefinitionMorphAction>().SelectMany(a => a.Actions);

            var missingSubActionTypes = allSubActionTypes.Where(t => !subActions.Select(a => a.GetType()).Contains(t)).ToArray();
            if (missingSubActionTypes.Any())
            {
                Assert.Fail("MorphAction implementation '{0}' not covered by test data.", missingSubActionTypes.First());
            }
        }

        [TestMethod]
        public void ActionSerializer_Morph_actions_should_be_serializable()
        {
            SerializeActions();
            Assert.IsTrue(File.Exists(_serializedActionsPath), "Missing file '{0}'", _serializedActionsPath);
        }

        [TestMethod]
        public void ActionSerializer_Morph_actions_should_roundtrip_serialization_and_deserialization()
        {
            DeserializeActions();
            Assert.AreEqual(_actions.Length, _deserializedActions.Length);
        }

    }
}
