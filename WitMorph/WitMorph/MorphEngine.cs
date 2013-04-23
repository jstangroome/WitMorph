﻿using System;
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
        public IEnumerable<IMorphAction> GenerateActions(Uri collectionUri, string projectName, string newProcessTemplateName, ProcessTemplateMap processTemplateMap)
        {
            var factory = new ProcessTemplateFactory();

            var currentTemplate = factory.FromActiveTeamProject(collectionUri, projectName);
            var goalTemplate = factory.FromCollectionTemplates(collectionUri, newProcessTemplateName);

            var actionSet = new MorphActionSet();
            var witdCollectionComparer = new WitdCollectionComparer(processTemplateMap, actionSet);
            witdCollectionComparer.Compare(currentTemplate.WorkItemTypeDefinitions, goalTemplate.WorkItemTypeDefinitions);

            return actionSet.Combine();
        }

        public IEnumerable<IMorphAction> GenerateActions(IEnumerable<IDifference> differences)
        {
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

                foreach (var stateRename in workItemTypeGroup.OfType<RenamedWorkItemStateDifference>())
                {
                    const string defaultReason = "Process Template Change";
                    modifyTypeAction.AddWorkflowState(stateRename.GoalState);
                    modifyTypeAction.AddWorkflowTransition(stateRename.CurrentStateName, stateRename.GoalStateName, defaultReason);
                    actionSet.ProcessWorkItemData.Add(new ModifyWorkItemStateMorphAction(stateRename.CurrentWorkItemTypeName, stateRename.CurrentStateName, stateRename.GoalStateName));
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
                actionSet.ProcessWorkItemData.Add(new ExportWorkItemDataMorphAction(witdRemove.TypeName, allFields: true));
                actionSet.FinaliseWorkItemTypeDefinitions.Add(new DestroyWitdMorphAction(witdRemove.TypeName));
            }

            return actionSet.Combine();
        }

        public void Apply(Uri collectionUri, string projectName, IEnumerable<IMorphAction> actions, string outputPath)
        {
            var context = new ExecutionContext(collectionUri, projectName, outputPath);
            context.TraceLevel = TraceLevel.Verbose;
            foreach (var action in actions)
            {
                action.Execute(context);
            }
        }

    }
}
