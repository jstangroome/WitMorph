using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WitMorph.IntegrationTests
{
    [TestClass]
    public class Baseline_tests
    {
        public readonly static Uri TestCollectionUri = new Uri("http://localhost:8080/tfs/WitMorphTests");

        [TestMethod]
        public void Should_find_no_differences_between_Scrum_project_and_template()
        {
            var factory = new ProcessTemplateFactory();

            var collectionTemplate = factory.FromCollectionTemplates(TestCollectionUri, "Microsoft Visual Studio Scrum 2.1");
            var activeProject = factory.FromActiveTeamProject(TestCollectionUri, "Scrum-2.1");

            var diffEngine = new DiffEngine(ProcessTemplateMap.Empty());

            var differences = diffEngine.CompareProcessTemplates(activeProject, collectionTemplate);

            Assert.AreEqual(0, differences.Count(), "Should be zero differences between new Scrum 2.1 project and the template it was created from.");
        }
    }
}
