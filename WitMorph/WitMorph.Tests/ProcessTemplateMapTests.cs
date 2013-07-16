using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WitMorph.Tests
{
    [TestClass]
    public class ProcessTemplateMapTests
    {
        [TestMethod]
        public void ProcessTemplateMap_should_be_readable_from_stream()
        {
            using (var stream = typeof(ProcessTemplateMaps).Assembly.GetManifestResourceStream("WitMorph.ProcessTemplateMaps.Agile6ToScrum2.witmap"))
            {
                var map = ProcessTemplateMap.Read(stream);

                Assert.AreEqual("Product Backlog Item", map.WorkItemTypeMap.GetGoalByCurrent("User Story"), "workitemtype");

                Assert.AreEqual("Microsoft.VSTS.Common.BacklogPriority", map.WorkItemFieldMap.GetGoalByCurrent("Microsoft.VSTS.Common.StackRank"), "workitemfield");

                Assert.AreEqual("Approved", map.GetWorkItemStateMap("User Story").GetGoalByCurrent("Active"), "workitemstate");
            }
        }
    }
}
