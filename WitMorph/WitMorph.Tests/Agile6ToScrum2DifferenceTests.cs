using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WitMorph.Actions;
using WitMorph.Differences;

namespace WitMorph.Tests
{
    [TestClass]
    public class Agile6ToScrum2DifferenceTests : Convert_Agile_6_To_Scrum_2_Scenario
    {
      
        [TestMethod]
        public void Agile6ToScrum2Difference_should_identify_Issue_renamed_to_Impediment_work_item_type()
        {
            var renamedIssue = Differences
                .OfType<RenamedWorkItemTypeDefinitionDifference>()
                .SingleOrDefault(d => d.CurrentTypeName.Equals("Issue", StringComparison.InvariantCultureIgnoreCase) &&
                    d.GoalTypeName.Equals("Impediment", StringComparison.InvariantCultureIgnoreCase));

            Assert.IsNotNull(renamedIssue);
        }

        [TestMethod]
        public void Agile6ToScrum2Difference_should_identify_User_Story_renamed_to_PBI_work_item_type()
        {
            var renamedUserStory = Differences
                .OfType<RenamedWorkItemTypeDefinitionDifference>()
                .SingleOrDefault(d => d.CurrentTypeName.Equals("User Story", StringComparison.InvariantCultureIgnoreCase) &&
                    d.GoalTypeName.Equals("Product Backlog Item", StringComparison.InvariantCultureIgnoreCase));

            Assert.IsNotNull(renamedUserStory);
        }

        [TestMethod]
        public void Agile6ToScrum2Difference_should_identify_new_To_Do_state_for_Task_work_item_type()
        {
            var addedToDo = Differences
                .OfType<AddedWorkItemStateDifference>()
                .SingleOrDefault(
                    d => d.State.Value.Equals("To Do", StringComparison.InvariantCultureIgnoreCase) &&
                        d.CurrentWorkItemTypeName.Equals("Task", StringComparison.InvariantCultureIgnoreCase)
                );

            Assert.IsNotNull(addedToDo);
        }

        [TestMethod]
        public void Agile6ToScrum2Difference_should_identify_StackRank_field_renamed_to_BacklogPriority_field_for_Bug_Task_and_User_Story()
        {
            var bugFieldRename = Differences
                .OfType<RenamedWorkItemFieldDifference>()
                .SingleOrDefault(d => d.CurrentWorkItemTypeName == "Bug" 
                    && d.CurrentFieldReferenceName == "Microsoft.VSTS.Common.StackRank"
                    && d.GoalFieldReferenceName == "Microsoft.VSTS.Common.BacklogPriority");

            var taskFieldRename = Differences
                .OfType<RenamedWorkItemFieldDifference>()
                .SingleOrDefault(d => d.CurrentWorkItemTypeName == "Task"
                    && d.CurrentFieldReferenceName == "Microsoft.VSTS.Common.StackRank"
                    && d.GoalFieldReferenceName == "Microsoft.VSTS.Common.BacklogPriority");
            
            var userStoryFieldRename = Differences
                .OfType<RenamedWorkItemFieldDifference>()
                .SingleOrDefault(d => d.CurrentWorkItemTypeName == "User Story"
                    && d.CurrentFieldReferenceName == "Microsoft.VSTS.Common.StackRank"
                    && d.GoalFieldReferenceName == "Microsoft.VSTS.Common.BacklogPriority");

            Assert.IsNotNull(bugFieldRename, "Bug field rename not identified");
            Assert.IsNotNull(taskFieldRename, "Task field rename not identified");
            Assert.IsNotNull(userStoryFieldRename, "User Story field rename not identified");
        }

        [TestMethod]
        public void Agile6ToScrum2Difference_should_identify_Active_state_renamed_to_InProgress_for_Task()
        {
            var stateRename = Differences
                .OfType<RenamedWorkItemStateDifference>()
                .SingleOrDefault(d => d.CurrentWorkItemTypeName == "User Story"
                    && d.CurrentStateName == "Active"
                    && d.GoalStateName == "Approved");

            Assert.IsNotNull(stateRename, "Task Active state rename not identified");
        }

        [TestMethod]
        public void Agile6ToScrum2Difference_should_identify_Closed_and_Resolved_states_renamed_to_Done_for_User_Story()
        {
            var resolvedStateRename = Differences
                .OfType<RenamedWorkItemStateDifference>()
                .SingleOrDefault(d => d.CurrentWorkItemTypeName == "User Story"
                    && d.CurrentStateName == "Resolved"
                    && d.GoalStateName == "Done");

            var closedStateRename = Differences
                .OfType<ChangedWorkItemStateDifference>()
                .SingleOrDefault(d => d.CurrentWorkItemTypeName == "User Story"
                    && d.CurrentStateName == "Closed"
                    && d.GoalStateName == "Done");
            
            Assert.IsNotNull(closedStateRename, "User Story Closed state rename not identified");
            Assert.IsNotNull(resolvedStateRename, "User Story Resolved state rename not identified");
        }

        [TestMethod]
        public void Agile6ToScrum2_should_add_User_Story_Done_state_once()
        {
            var addDoneStateActions = Actions
                .OfType<ModifyWorkItemTypeDefinitionMorphAction>()
                .Where(a => a.WorkItemTypeName == "User Story")
                .SelectMany(a => a.SubActions
                    .OfType<AddStateModifyWorkItemTypeDefinitionSubAction>()
                    .Where(s => s.Name == "Done")
                );

            Assert.AreEqual(1, addDoneStateActions.Count(), "Should only add Done state once.");
        }

    }
}
