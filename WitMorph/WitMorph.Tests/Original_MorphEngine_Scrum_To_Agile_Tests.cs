﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WitMorph.Actions;

namespace WitMorph.Tests
{
    [TestClass]
    public class Original_MorphEngine_Scrum_To_Agile_Tests : Convert_Scrum_2_To_Agile_6_Scenario
    {
        [TestMethod]
        public void ScrumToAgile_should_rename_PBI_to_User_Story()
        {
            var renameAction = GenerateActionsViaDiffEngine()
                    .OfType<RenameWitdMorphAction>()
                    .SingleOrDefault(a => a.TypeName == "Product Backlog Item" && a.NewName == "User Story");

            Assert.IsNotNull(renameAction);
        }

        [TestMethod]
        public void ScrumToAgile_should_export_and_remove_Impediment()
        {
            var actions = GenerateActionsViaDiffEngine();

            var exportIndex = actions.FindIndex(a =>
            {
                var e = a as ExportWorkItemDataMorphAction;
                return e != null && e.WorkItemTypeName == "Impediment" && e.AllFields;
            });

            var destroyIndex = actions.FindIndex(a =>
            {
                var d = a as DestroyWitdMorphAction;
                return d != null && d.TypeName == "Impediment";
            });

            Assert.IsTrue(exportIndex >= 0, "Is exported");
            Assert.IsTrue(destroyIndex > exportIndex, "Is destroyed after exported");
        }

        [TestMethod]
        public void ScrumToAgile_should_export_extra_Bug_fields()
        {
            var actions = GenerateActionsViaDiffEngine();

            var exportIndex = actions.FindIndex(a =>
            {
                var e = a as ExportWorkItemDataMorphAction;
                return e != null && e.WorkItemTypeName == "Bug"
                    && e.FieldReferenceNames.Contains("Microsoft.VSTS.Scheduling.Effort")
                    && e.FieldReferenceNames.Contains("Microsoft.VSTS.Common.AcceptanceCriteria");
            });

            Assert.IsTrue(exportIndex >= 0, "Is exported");
        }

        [TestMethod]
        public void ScrumToAgile_should_export_extra_Task_fields()
        {
            var actions = GenerateActionsViaDiffEngine();

            var exportIndex = actions.FindIndex(a =>
            {
                var e = a as ExportWorkItemDataMorphAction;
                return e != null && e.WorkItemTypeName == "Task"
                    && e.FieldReferenceNames.Contains("Microsoft.VSTS.CMMI.Blocked");
            });

            Assert.IsTrue(exportIndex >= 0, "Is exported");
        }

        [TestMethod]
        public void ScrumToAgile_should_copy_BacklogPriority_to_StackRank_for_Bug_Task_and_PBI()
        {
            var actions = GenerateActionsViaDiffEngine();

            var bugCopyIndex = actions.FindIndex(a =>
            {
                var e = a as CopyWorkItemDataMorphAction;
                return e != null && e.TypeName == "Bug" && e.FromField == "Microsoft.VSTS.Common.BacklogPriority" && e.ToField == "Microsoft.VSTS.Common.StackRank";
            });

            var taskCopyIndex = actions.FindIndex(a =>
            {
                var e = a as CopyWorkItemDataMorphAction;
                return e != null && e.TypeName == "Task" && e.FromField == "Microsoft.VSTS.Common.BacklogPriority" && e.ToField == "Microsoft.VSTS.Common.StackRank";
            });

            var pbiCopyIndex = actions.FindIndex(a =>
            {
                var e = a as CopyWorkItemDataMorphAction;
                return e != null && e.TypeName == "Product Backlog Item" && e.FromField == "Microsoft.VSTS.Common.BacklogPriority" && e.ToField == "Microsoft.VSTS.Common.StackRank";
            });

            Assert.IsTrue(bugCopyIndex >= 0, "Is Bug field copied");
            Assert.IsTrue(taskCopyIndex >= 0, "Is Task field copied");
            Assert.IsTrue(pbiCopyIndex >= 0, "Is PBI field copied");
        }

        [TestMethod]
        public void ScrumToAgile_should_copy_Effort_to_StoryPoints_for_PBI()
        {
            var actions = GenerateActionsViaDiffEngine();

            var pbiCopyIndex = actions.FindIndex(a =>
            {
                var e = a as CopyWorkItemDataMorphAction;
                return e != null && e.TypeName == "Product Backlog Item" && e.FromField == "Microsoft.VSTS.Scheduling.Effort" && e.ToField == "Microsoft.VSTS.Scheduling.StoryPoints";
            });

            Assert.IsTrue(pbiCopyIndex >= 0, "Is PBI field copied");
        }


        [TestMethod]
        public void ScrumToAgile_should_modify_Done_state_to_Closed_for_Task()
        {
            var actions = GenerateActionsViaDiffEngine();

            var taskIndex = actions.FindIndex(a =>
            {
                var e = a as ModifyWorkItemStateMorphAction;
                return e != null && e.TypeName == "Task" && e.FromValue == "Done" && e.ToValue == "Closed";
            });

            Assert.IsTrue(taskIndex >= 0, "Is Task state modified");
        }

        [TestMethod]
        public void ScrumToAgile_should_modify_Done_state_to_Resolved_for_PBI()
        {
            var actions = GenerateActionsViaDiffEngine();

            var pbiIndex = actions.FindIndex(a =>
            {
                var e = a as ModifyWorkItemStateMorphAction;
                return e != null && e.TypeName == "Product Backlog Item" && e.FromValue == "Done" && e.ToValue == "Resolved";
            });

            Assert.IsTrue(pbiIndex >= 0, "Is PBI state modified");
        }

        [TestMethod]
        public void ScrumToAgile_should_modify_Done_state_to_Resolved_for_Bug()
        {
            var actions = GenerateActionsViaDiffEngine();

            var bugIndex = actions.FindIndex(a =>
            {
                var e = a as ModifyWorkItemStateMorphAction;
                return e != null && e.TypeName == "Bug" && e.FromValue == "Done" && e.ToValue == "Resolved";
            });

            Assert.IsTrue(bugIndex >= 0, "Is state modified");
        }

        [TestMethod]
        public void ScrumToAgile_should_modify_InProgress_state_to_Active_for_Task()
        {
            var actions = GenerateActionsViaDiffEngine();

            var taskIndex = actions.FindIndex(a =>
            {
                var e = a as ModifyWorkItemStateMorphAction;
                return e != null && e.TypeName == "Task" && e.FromValue == "In Progress" && e.ToValue == "Active";
            });

            Assert.IsTrue(taskIndex >= 0, "Is Task state modified");
        }

        [TestMethod]
        public void ScrumToAgile_should_modify_ToDo_state_to_New_for_Task()
        {
            var actions = GenerateActionsViaDiffEngine();

            var taskIndex = actions.FindIndex(a =>
            {
                var e = a as ModifyWorkItemStateMorphAction;
                return e != null && e.TypeName == "Task" && e.FromValue == "To Do" && e.ToValue == "New";
            });

            Assert.IsTrue(taskIndex >= 0, "Is Task state modified");
        }

        // TODO verify BusinessValue and AcceptanceCriteria are exported for PBIs
        // TODO verify individual fields are removed after export
        // TODO verify extra states are removed 
        // TODO verify state modifications for all WITDs are done before they're removed
    }
}
