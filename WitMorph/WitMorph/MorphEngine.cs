using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WitMorph.Actions;
using WitMorph.Differences;
using WitMorph.Structures;

namespace WitMorph
{
    public class MorphEngine
    {
        public MorphAction[] GenerateActions(IEnumerable<IDifference> differences)
        {
            differences = differences.ToArray(); // avoid multi-enumeration issues

            var actionSet = new MorphActionSet();

            foreach (var witdAdd in differences.OfType<AddedWorkItemTypeDefinitionDifference>())
            {
                actionSet.PrepareWorkItemTypeDefinitions.Add(new ImportWorkItemTypeDefinitionMorphAction(witdAdd.WorkItemTypeDefinition));
            }

            var workItemTypeDifferences = differences
                .OfType<IWorkItemTypeDifference>()
                .GroupBy(d => d.CurrentWorkItemTypeName, StringComparer.OrdinalIgnoreCase);

            foreach (var workItemTypeGroup in workItemTypeDifferences)
            {
                var modifyTypeAction = new ModifyWorkItemTypeDefinitionMorphAction(workItemTypeGroup.Key);
                var exportDataAction = new ExportWorkItemDataMorphAction(workItemTypeGroup.Key);
                var finalModifyTypeAction = new ModifyWorkItemTypeDefinitionMorphAction(workItemTypeGroup.Key);

                actionSet.PrepareWorkItemTypeDefinitions.Add(modifyTypeAction);
                actionSet.ProcessWorkItemData.Add(exportDataAction);
                actionSet.FinaliseWorkItemTypeDefinitions.Add(finalModifyTypeAction);

                foreach (var fieldAdd in workItemTypeGroup.OfType<AddedWorkItemFieldDifference>())
                {
                    modifyTypeAction.AddFieldDefinition(fieldAdd.GoalField);
                }

                foreach (var fieldRename in workItemTypeGroup.OfType<RenamedWorkItemFieldDifference>())
                {
                    modifyTypeAction.AddFieldDefinition(fieldRename.GoalField);
                    // TODO consolidate data copy for multiple fields into one action
                    actionSet.ProcessWorkItemData.Add(new CopyWorkItemDataMorphAction(fieldRename.CurrentWorkItemTypeName, fieldRename.CurrentFieldReferenceName, fieldRename.GoalFieldReferenceName));
                    finalModifyTypeAction.RemoveFieldDefinition(fieldRename.CurrentFieldReferenceName);
                }

                foreach (var stateRename in workItemTypeGroup.OfType<ConsolidatedWorkItemStateDifference>())
                {
                    // Rename
                    const string defaultReason = "Process Template Change";
                    var removeTransitionAction = modifyTypeAction.RemoveWorkflowTransition(stateRename.CurrentStateName, stateRename.GoalStateName);
                    var addTransitionAction = modifyTypeAction.AddWorkflowTransition(stateRename.CurrentStateName, stateRename.GoalStateName, defaultReason);
                    var modifyStateAction = new ModifyWorkItemStateMorphAction(stateRename.CurrentWorkItemTypeName, stateRename.CurrentStateName, stateRename.GoalStateName);
                    modifyStateAction.LinkedActions.Add(new ActionLink(removeTransitionAction, ActionLinkType.Required));
                    modifyStateAction.LinkedActions.Add(new ActionLink(addTransitionAction, ActionLinkType.Required));
                    actionSet.ProcessWorkItemData.Add(modifyStateAction);
                    finalModifyTypeAction.RemoveWorkflowState(stateRename.CurrentStateName);
                }


                foreach (var stateRename in workItemTypeGroup.OfType<RenamedWorkItemStateDifference>())
                {
                    // Rename
                    const string defaultReason = "Process Template Change";
                    var addStateAction = modifyTypeAction.AddWorkflowState(stateRename.GoalState);
                    var addTransitionAction = modifyTypeAction.AddWorkflowTransition(stateRename.CurrentStateName, stateRename.GoalStateName, defaultReason);
                    var modifyStateAction = new ModifyWorkItemStateMorphAction(stateRename.CurrentWorkItemTypeName, stateRename.CurrentStateName, stateRename.GoalStateName);
                    modifyStateAction.LinkedActions.Add(new ActionLink(addStateAction, ActionLinkType.Required));
                    modifyStateAction.LinkedActions.Add(new ActionLink(addTransitionAction, ActionLinkType.Required));
                    actionSet.ProcessWorkItemData.Add(modifyStateAction);
                    finalModifyTypeAction.RemoveWorkflowState(stateRename.CurrentStateName);
                }

                foreach (var fieldRemove in workItemTypeGroup.OfType<RemovedWorkItemFieldDifference>())
                {
                    exportDataAction.AddExportField(fieldRemove.ReferenceFieldName);
                    finalModifyTypeAction.RemoveFieldDefinition(fieldRemove.ReferenceFieldName);
                }

                foreach (var fieldChange in workItemTypeGroup.OfType<ChangedWorkItemFieldDifference>())
                {
                    finalModifyTypeAction.ReplaceFieldDefinition(fieldChange.GoalField);
                }

                foreach (var formChange in workItemTypeGroup.OfType<ChangedWorkItemFormDifference>())
                {
                    finalModifyTypeAction.ReplaceForm(formChange.FormElement);
                }

                foreach (var formChange in workItemTypeGroup.OfType<ChangedWorkItemWorkflowDifference>())
                {
                    finalModifyTypeAction.ReplaceWorkflow(formChange.WorkflowElement);
                }
            }

            foreach (var witdRename in differences.OfType<RenamedWorkItemTypeDefinitionDifference>())
            {
                actionSet.FinaliseWorkItemTypeDefinitions.Add(new RenameWitdMorphAction(witdRename.CurrentTypeName, witdRename.GoalTypeName));
            }

            foreach (var witdRemove in differences.OfType<RemovedWorkItemTypeDefinitionDifference>())
            {
                var exportAction = new ExportWorkItemDataMorphAction(witdRemove.TypeName, allFields: true);
                var destroyAction = new DestroyWitdMorphAction(witdRemove.TypeName);
                destroyAction.LinkedActions.Add(new ActionLink(exportAction, ActionLinkType.Encouraged));
                actionSet.ProcessWorkItemData.Add(exportAction);
                actionSet.FinaliseWorkItemTypeDefinitions.Add(destroyAction);
            }

            return actionSet.Combine().ToArray();
        }

        public void Apply(Uri collectionUri, string projectName, IEnumerable<MorphAction> actions, string outputPath)
        {
            var context = new ExecutionContext(collectionUri, projectName, outputPath) {TraceLevel = TraceLevel.Verbose};
            foreach (var action in actions)
            {
                action.Execute(context);
            }
        }

    }
}
