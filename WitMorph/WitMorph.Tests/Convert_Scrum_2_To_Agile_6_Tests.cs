using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WitMorph.Actions;
using WitMorph.Structures;
using WitMorph.Tests.ProcessTemplates;

namespace WitMorph.Tests
{
    [TestClass]
    public class Convert_Scrum_2_To_Agile_6_Tests
    {
        static readonly IEnumerable<IMorphAction> _actions;

        static Convert_Scrum_2_To_Agile_6_Tests()
        {
            using (var agileTemplate = EmbeddedProcessTemplate.Agile6())
            using (var scrumTemplate = EmbeddedProcessTemplate.Scrum2())
            {
                var agileReader = new ProcessTemplateReader(agileTemplate.TemplatePath);
                var scrumReader = new ProcessTemplateReader(scrumTemplate.TemplatePath);

                var processTemplateMap = ProcessTemplateMap.ConvertScrum2ToAgile6();
                var actionSet = new MorphActionSet();

                var sut = new WitdCollectionComparer(processTemplateMap, actionSet);
                sut.Compare(agileReader.WorkItemTypeDefinitions, scrumReader.WorkItemTypeDefinitions);

                _actions = actionSet.Combine().ToArray();
            }
        }

        private IEnumerable<IMorphAction> Actions
        {
            get { return _actions; }
        }

        [TestMethod]
        public void ScrumToAgile_should_rename_PBI_to_User_Story()
        {
            var renameAction = Actions
                    .OfType<RenameWitdMorphAction>()
                    .SingleOrDefault(a => a.TypeName == "Product Backlog Item" && a.NewName == "User Story");

            Assert.IsNotNull(renameAction);
        }

        [TestMethod]
        public void ScrumToAgile_should_export_and_remove_Impediment()
        {
            var actions = Actions.ToList();

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
            var actions = Actions.ToList();

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
            var actions = Actions.ToList();

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
            var actions = Actions.ToList();

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
            var actions = Actions.ToList();

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
            var actions = Actions.ToList();

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
            var actions = Actions.ToList();

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
            var actions = Actions.ToList();

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
            var actions = Actions.ToList();

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
            var actions = Actions.ToList();

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

        [TestMethod]
        public void ScrumToAgile_should_produce_the_same_actions_directly_as_via_the_DiffEngine()
        {
            List<IMorphAction> actionsViaDiffEngine;
            using (var agileTemplate = EmbeddedProcessTemplate.Agile6())
            using (var scrumTemplate = EmbeddedProcessTemplate.Scrum2())
            {
                var agileReader = new ProcessTemplateReader(agileTemplate.TemplatePath);
                var scrumReader = new ProcessTemplateReader(scrumTemplate.TemplatePath);

                var processTemplateMap = ProcessTemplateMap.ConvertScrum2ToAgile6();

                var currentProcessTemplate = new ProcessTemplate { WorkItemTypeDefinitions = new ReadOnlyCollection<WorkItemTypeDefinition>(scrumReader.WorkItemTypeDefinitions.ToArray()) };
                var goalProcessTemplate = new ProcessTemplate { WorkItemTypeDefinitions = new ReadOnlyCollection<WorkItemTypeDefinition>(agileReader.WorkItemTypeDefinitions.ToArray()) };

                var diffEngine = new DiffEngine(processTemplateMap);
                var differences = diffEngine.CompareProcessTemplates(currentProcessTemplate, goalProcessTemplate);

                var morphEngine = new MorphEngine();
                actionsViaDiffEngine = morphEngine.GenerateActions(differences).ToList();
            }
 
            Assert.AreEqual(46, _actions.Count(), "Baseline for direct actions has changed");

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

    }
}
