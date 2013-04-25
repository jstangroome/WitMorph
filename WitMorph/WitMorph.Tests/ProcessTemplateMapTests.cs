﻿using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WitMorph.Tests
{
    [TestClass]
    public class ProcessTemplateMapTests
    {
        [TestMethod]
        public void ProcessTemplateMap_should_be_readable_from_file()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WitMorph.Tests.ProcessTemplateMaps.Agile6ToScrum2.xml"))
            {
                var map = ProcessTemplateMap.Read(stream);

                Assert.AreEqual("Product Backlog Item", map.WorkItemTypeMap.GetGoalByCurrent("User Story"), "workitemtype");

                Assert.AreEqual("Microsoft.VSTS.Common.BacklogPriority", map.WorkItemFieldMap.GetGoalByCurrent("Microsoft.VSTS.Common.StackRank"), "workitemfield");

                Assert.AreEqual("Approved", map.GetWorkItemStateMap("User Story").GetGoalByCurrent("Active"), "workitemstate");
            }
        }
    }
}