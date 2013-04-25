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
            using (var template = EmbeddedProcessTemplate.Scrum2())
            {
                var templateReader = new ProcessTemplateReader(template.TemplatePath);
                workItemTypeDefinition = templateReader.WorkItemTypeDefinitions.First();
            }

            _differences = new IDifference[]
                           {
                               new RemovedWorkItemTypeDefinitionDifference("Issue"),
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

            var exportAction = exportActionLink.Target;
            Assert.AreEqual(true, exportAction.AllFields, "linked Export action should export all fields");
        }
    }
}
