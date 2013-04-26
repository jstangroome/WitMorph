using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WitMorph.Actions;
using WitMorph.Differences;
using WitMorph.Model;
using WitMorph.Tests.ProcessTemplates;

namespace WitMorph.Tests
{
    [TestClass]
    public class MorphEngineTests
    {
        private IDifference[] _differences;
        private MorphAction[] _actions;

        private void SetupDifferences()
        {
            WorkItemTypeDefinition workItemTypeDefinition;
            WitdState state;
            using (var template = EmbeddedProcessTemplate.Scrum2())
            {
                var templateReader = new ProcessTemplateReader(template.TemplatePath);
                workItemTypeDefinition = templateReader.WorkItemTypeDefinitions.First();
                state = workItemTypeDefinition.States.First();
            }

            _differences = new IDifference[]
                           {
                               new RemovedWorkItemTypeDefinitionDifference("Issue"),
                               new RenamedWorkItemStateDifference(workItemTypeDefinition.Name, "not-" + state.Value, state)
                           };

        }

        private void GenerateActions()
        {
            SetupDifferences();
            var morphEngine = new MorphEngine();
            _actions = morphEngine.GenerateActions(_differences);
        }

        [TestMethod]
        public void MorphEngine_should_link_DestroyWitdMorphAction_to_all_fields_ExportWorkItemDataMorphAction_as_encouraged()
        {
            GenerateActions();

            var destroyAction = _actions.OfType<DestroyWitdMorphAction>().First();

            var exportActionLink = destroyAction.LinkedActions
                .Where(l => l.Target is ExportWorkItemDataMorphAction)
                .SingleOrDefault();

            Assert.IsNotNull(exportActionLink, "Destroy action should link to Export action");
            Assert.AreEqual(ActionLinkType.Encouraged, exportActionLink.Type, "Action link should be Type 'Encouraged'");

            var exportAction = (ExportWorkItemDataMorphAction)exportActionLink.Target;
            Assert.AreEqual(true, exportAction.AllFields, "linked Export action should export all fields");

            Assert.IsTrue(_actions.Contains(exportAction), "linked Export Action should be in action list");
        }

        [TestMethod]
        public void MorphEngine_should_link_ModifyWorkItemStateMorphAction_to_AddState_and_AddTransition_subactions_as_required()
        {
            GenerateActions();

            var modifyStateAction = _actions.OfType<ModifyWorkItemStateMorphAction>().FirstOrDefault();

            Assert.IsNotNull(modifyStateAction, "Actions should contain a ModifyWorkItemState action.");

            var addStateLink = modifyStateAction.LinkedActions
                .Where(l => l.Target is AddStateModifyWorkItemTypeDefinitionSubAction)
                .SingleOrDefault();

            Assert.IsNotNull(addStateLink, "Modify State action should link to Add State action");
            Assert.AreEqual(ActionLinkType.Required, addStateLink.Type, "Action link should be Type 'Required'");

            var addStateAction = (AddStateModifyWorkItemTypeDefinitionSubAction)addStateLink.Target;
            Assert.AreEqual(modifyStateAction.ToValue, addStateAction.Name, "Add State action should be for To state");

            var addTransitionLink = modifyStateAction.LinkedActions
                .Where(l => l.Target is AddTransitionModifyWorkItemTypeDefinitionSubAction)
                .SingleOrDefault();

            Assert.IsNotNull(addTransitionLink, "Modify State action should link to Add Transition action");
            Assert.AreEqual(ActionLinkType.Required, addTransitionLink.Type, "Action link should be Type 'Required'");

            var addTransitionAction = (AddTransitionModifyWorkItemTypeDefinitionSubAction)addTransitionLink.Target;
            Assert.AreEqual(modifyStateAction.ToValue, addTransitionAction.ToState, "Add Transaction action ToValue should match Modify State action");
            Assert.AreEqual(modifyStateAction.FromValue, addTransitionAction.FromState, "Add Transaction action FromValue should match Modify State action");
        }

        // TODO remove state should link to modify state
        // TODO remove field should link to export field or export all fields or to copy field
        // TODO copy field should link to add field
        

    }
}
