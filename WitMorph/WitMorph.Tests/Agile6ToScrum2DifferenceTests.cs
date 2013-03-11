using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WitMorph.Differences;

namespace WitMorph.Tests
{
    [TestClass]
    public class Agile6ToScrum2DifferenceTests : Convert_Agile_6_To_Scrum_2_Scenario
    {
      
        [TestMethod]
        public void Agile6ToScrum2Difference_should_identify_new_Impediment_work_item_type()
        {
            var addedImpediment = Differences
                .OfType<AddedWorkItemTypeDefinitionDifference>()
                .SingleOrDefault(d => d.WorkItemTypeDefinition.Name.Equals("Impediment", StringComparison.InvariantCultureIgnoreCase));

            Assert.IsNotNull(addedImpediment);
        }

        [TestMethod]
        public void Agile6ToScrum2Difference_should_identify_removed_Issue_work_item_type()
        {
            var removedIssue = Differences
                .OfType<RemovedWorkItemTypeDefinitionDifference>()
                .SingleOrDefault(d => d.TypeName.Equals("Issue", StringComparison.InvariantCultureIgnoreCase));

            Assert.IsNotNull(removedIssue);
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
                .SingleOrDefault(d => d.CurrentWorkItemTypeName == "Task"
                    && d.CurrentStateName == "Active"
                    && d.GoalStateName == "In Progress");

            Assert.IsNotNull(stateRename, "Task Active state rename not identified");
        }

    }
}
