using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WitMorph.Differences;
using WitMorph.Tests.ProcessTemplates;

namespace WitMorph.Tests
{
    [TestClass]
    public class DiffEngineTests
    {
        private static IEnumerable<IDifference> _differences;

        static DiffEngineTests()
        {
            using (var currentTemplate = EmbeddedProcessTemplate.Agile6())
            using (var goalTemplate = EmbeddedProcessTemplate.Scrum2())
            {
                var currentTemplateReader = new ProcessTemplateReader(currentTemplate.TemplatePath);
                var goalTemplateReader = new ProcessTemplateReader(goalTemplate.TemplatePath);

                var currentProcessTemplate = new ProcessTemplate {WorkItemTypeDefinitions = new ReadOnlyCollection<WorkItemTypeDefinition>(currentTemplateReader.WorkItemTypeDefinitions.ToArray())};
                var goalProcessTemplate = new ProcessTemplate {WorkItemTypeDefinitions = new ReadOnlyCollection<WorkItemTypeDefinition>(goalTemplateReader.WorkItemTypeDefinitions.ToArray())};

                var diffEngine = new DiffEngine(new ProcessTemplateMap());
                _differences = diffEngine.CompareProcessTemplates(currentProcessTemplate, goalProcessTemplate);
            }
        }

        [TestMethod]
        public void DiffEngine_should_identify_new_Impediment_work_item_type()
        {
            var addedImpediment = _differences
                .OfType<AddedWorkItemTypeDefinitionDifference>()
                .SingleOrDefault(d => d.WorkItemTypeDefinition.Name.Equals("Impediment", StringComparison.InvariantCultureIgnoreCase));

            Assert.IsNotNull(addedImpediment);
        }

        [TestMethod]
        public void DiffEngine_should_identify_removed_Issue_work_item_type()
        {
            var addedImpediment = _differences
                .OfType<RemovedWorkItemTypeDefinitionDifference>()
                .SingleOrDefault(d => d.TypeName.Equals("Issue", StringComparison.InvariantCultureIgnoreCase));

            Assert.IsNotNull(addedImpediment);
        }

    }
}
