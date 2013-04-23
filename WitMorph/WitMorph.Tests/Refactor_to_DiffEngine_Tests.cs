using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WitMorph.Actions;

namespace WitMorph.Tests
{
    [TestClass]
    public class Refactor_to_DiffEngine_Tests : Convert_Scrum_2_To_Agile_6_Scenario
    {
        [TestMethod]
        public void ScrumToAgile_should_replace_IntegrationBuild_field_for_Task()
        {
            var actionsViaDiffEngine = GenerateActionsViaDiffEngine();

            var replaceActionIndex = actionsViaDiffEngine.FindIndex(a =>
            {
                var m = a as ModifyWorkItemTypeDefinitionMorphAction;
                return m != null && m.WorkItemTypeName == "Task" &&
                       m.Actions.OfType<ReplaceFieldModifyWorkItemTypeDefinitionSubAction>().Any(r => r.ReferenceName == "Microsoft.VSTS.Build.IntegrationBuild");
            });

            RelativeAssert.IsGreaterThanOrEqual(0, replaceActionIndex, "Will not replace IntegrationBuild field for Task");
        }
    
        [TestMethod]
        public void ScrumToAgile_should_produce_the_same_types_of_actions_directly_as_via_the_DiffEngine_combined()
        {
            var actionsViaDiffEngine = GenerateActionsViaDiffEngine();

            //ScrumToAgile_should_rename_PBI_to_User_Story

            var renameAction = actionsViaDiffEngine
                .OfType<RenameWitdMorphAction>()
                .SingleOrDefault(a => a.TypeName == "Product Backlog Item" && a.NewName == "User Story");

            Assert.IsNotNull(renameAction, "Will not rename Product Backlog Item work item type to User Story");

            //ScrumToAgile_should_export_and_remove_Impediment

            var exportIndex = actionsViaDiffEngine.FindIndex(a =>
            {
                var e = a as ExportWorkItemDataMorphAction;
                return e != null && e.WorkItemTypeName == "Impediment" && e.AllFields;
            });

            var destroyIndex = actionsViaDiffEngine.FindIndex(a =>
            {
                var d = a as DestroyWitdMorphAction;
                return d != null && d.TypeName == "Impediment";
            });

            Assert.IsTrue(exportIndex >= 0, "Will not export Impediment work items");
            Assert.IsTrue(destroyIndex > exportIndex, "Will destroy Impediment work items before exporting existing data");

            //ScrumToAgile_should_export_extra_Bug_fields

            var bugExportIndex = actionsViaDiffEngine.FindIndex(a =>
            {
                var e = a as ExportWorkItemDataMorphAction;
                return e != null && e.WorkItemTypeName == "Bug"
                    && e.FieldReferenceNames.Contains("Microsoft.VSTS.Scheduling.Effort")
                    && e.FieldReferenceNames.Contains("Microsoft.VSTS.Common.AcceptanceCriteria");
            });

            Assert.IsTrue(bugExportIndex >= 0, "Will not export two removed Bug fields");

            //ScrumToAgile_should_copy_BacklogPriority_to_StackRank_for_Task

            var taskAddFieldIndex = actionsViaDiffEngine.FindIndex(a =>
            {
                var m = a as ModifyWorkItemTypeDefinitionMorphAction;
                return m != null && m.WorkItemTypeName == "Task"
                       && m.Actions.OfType<AddFieldModifyWorkItemTypeDefinitionSubAction>().Any(s => s.ReferenceName == "Microsoft.VSTS.Common.StackRank");
            });

            var taskFieldCopyIndex = actionsViaDiffEngine.FindIndex(a =>
            {
                var e = a as CopyWorkItemDataMorphAction;
                return e != null && e.TypeName == "Task" && e.FromField == "Microsoft.VSTS.Common.BacklogPriority" && e.ToField == "Microsoft.VSTS.Common.StackRank";
            });

            var taskRemoveFieldIndex = actionsViaDiffEngine.FindIndex(a =>
            {
                var m = a as ModifyWorkItemTypeDefinitionMorphAction;
                return m != null && m.WorkItemTypeName == "Task"
                       && m.Actions.OfType<RemoveFieldModifyWorkItemTypeDefinitionSubAction>().Any(s => s.ReferenceName == "Microsoft.VSTS.Common.BacklogPriority");
            });

            Assert.IsTrue(taskAddFieldIndex >= 0, "Will not add Task StackRank field");
            Assert.IsTrue(taskFieldCopyIndex > taskAddFieldIndex, "Will not copy Task BacklogPriority field to StackRank after adding field");
            Assert.IsTrue(taskRemoveFieldIndex > taskFieldCopyIndex, "Will not remove BacklogPriority field after copying data");

            //ScrumToAgile_should_modify_Done_state_to_Closed_for_Task

            var taskAddStateIndex = actionsViaDiffEngine.FindIndex(a =>
            {
                var m = a as ModifyWorkItemTypeDefinitionMorphAction;
                return m != null && m.WorkItemTypeName == "Task"
                       && m.Actions.OfType<AddStateModifyWorkItemTypeDefinitionSubAction>().Any(s => s.Name == "Closed");
            });

            var taskAddTransitionIndex = actionsViaDiffEngine.FindIndex(a =>
            {
                var m = a as ModifyWorkItemTypeDefinitionMorphAction;
                return m != null && m.WorkItemTypeName == "Task"
                       && m.Actions.OfType<AddTransitionModifyWorkItemTypeDefinitionSubAction>().Any(t => t.FromState == "Done" && t.ToState == "Closed");
            });

            var taskStateChangeIndex = actionsViaDiffEngine.FindIndex(a =>
            {
                var e = a as ModifyWorkItemStateMorphAction;
                return e != null && e.TypeName == "Task" && e.FromValue == "Done" && e.ToValue == "Closed";
            });

            var taskRemoveStateIndex = actionsViaDiffEngine.FindIndex(a =>
            {
                var m = a as ModifyWorkItemTypeDefinitionMorphAction;
                return m != null && m.WorkItemTypeName == "Task"
                       && m.Actions.OfType<RemoveStateModifyWorkItemTypeDefinitionSubAction>().Any(s => s.Name == "Done");
            });


            Assert.IsTrue(taskAddStateIndex >= 0, "Will not add Task Closed state");
            Assert.AreEqual(taskAddStateIndex, taskAddTransitionIndex, "Should add Task transition from Done to Closed");
            Assert.IsTrue(taskStateChangeIndex > taskAddStateIndex, "Will not change Task state from Done to Closed after adding state");
            Assert.IsTrue(taskRemoveStateIndex > taskStateChangeIndex, "Should remove Task Done state after changing data");
        }

        [TestMethod]
        public void ScrumToAgile_should_add_OriginalEstimate_field_to_Task()
        {
            var actionsViaDiffEngine = GenerateActionsViaDiffEngine();

            var addFieldIndex = actionsViaDiffEngine.FindIndex(a =>
            {
                var m = a as ModifyWorkItemTypeDefinitionMorphAction;
                return m != null && m.WorkItemTypeName == "Task"
                       && m.Actions.OfType<AddFieldModifyWorkItemTypeDefinitionSubAction>().Any(s => s.ReferenceName == "Microsoft.VSTS.Scheduling.OriginalEstimate");
            });

            RelativeAssert.IsGreaterThanOrEqual(0, addFieldIndex, "Should add field to task.");
        }

        [TestMethod]
        public void ScrumToAgile_should_add_Issue_work_item()
        {
            var actions = GenerateActionsViaDiffEngine();

            var addIssueIndex = actions.FindIndex(a =>
            {
                var m = a as ImportWorkItemTypeDefinitionMorphAction;
                return m != null && m.WorkItemTypeName == "Issue";
            });

            RelativeAssert.IsGreaterThanOrEqual(0, addIssueIndex, "Should add Issue work item.");
        }

        [TestMethod]
        public void ScrumToAgile_should_replace_Task_work_item_form()
        {
            var actions = GenerateActionsViaDiffEngine();

            var replaceFormIndex = actions.FindIndex(a =>
            {
                var m = a as ModifyWorkItemTypeDefinitionMorphAction;
                return m != null && m.WorkItemTypeName == "Task"
                    && m.Actions.OfType<ReplaceFormModifyWorkItemTypeDefinitionSubAction>().Any();
            });

            RelativeAssert.IsGreaterThanOrEqual(0, replaceFormIndex, "Should replace form for Task work item.");
        }

    }

}