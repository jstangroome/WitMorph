using System;
using System.IO;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WitMorph.IntegrationTests
{
    [TestClass]
    public class Baseline_tests
    {
        public readonly static Uri TestCollectionUri = new Uri("http://localhost:8080/tfs/WitMorphTests");

        public TestContext TestContext { get; set; }

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

        [TestMethod]
        public void Should_find_no_difference_between_original_Scrum_project_and_Agile_project_converted_to_Scrum()
        {
            PrepareTestEnvironment.ResetCollection();

            TfsConnection.ClientSettingsDirectory = Path.Combine(TestContext.TestRunResultsDirectory, "TfsClient");

            var factory = new ProcessTemplateFactory();

            var unconverted = factory.FromActiveTeamProject(TestCollectionUri, "Agile-6.1");
            var goal = factory.FromActiveTeamProject(TestCollectionUri, "Scrum-2.1");

            var agileToScrumDiffEngine = new DiffEngine(ProcessTemplateMap.ConvertAgile6ToScrum2());

            var initialDifferences = agileToScrumDiffEngine.CompareProcessTemplates(unconverted, goal);

            var morphEngine = new MorphEngine();
            var actions = morphEngine.GenerateActions(initialDifferences);

            morphEngine.Apply(TestCollectionUri, "Agile-6.1", actions, TestContext.TestRunResultsDirectory);

            var emptyMapDiffEngine = new DiffEngine(ProcessTemplateMap.Empty());

            var converted = factory.FromActiveTeamProject(TestCollectionUri, "Agile-6.1");

            var finalDifferences = emptyMapDiffEngine.CompareProcessTemplates(converted, goal);

            var reportBuilder = new ReportBuilder();
            var report = reportBuilder.WriteDifferencesToXml(finalDifferences);
            report.Save(Path.Combine(TestContext.TestRunResultsDirectory, TestContext.TestName + ".xml"));

            Assert.AreEqual(0, finalDifferences.Count(), "Should be zero differences between new Scrum 2.1 project and the converted Agile 6.1 project.");
        }
    }
}
