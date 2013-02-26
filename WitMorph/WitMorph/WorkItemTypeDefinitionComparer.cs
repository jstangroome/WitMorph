using System;
using System.Diagnostics;
using System.Linq;
using WitMorph.Actions;
using WitMorph.Structures;

namespace WitMorph
{
    class WorkItemTypeDefinitionComparer
    {
        private readonly ProcessTemplateMap _processTemplateMap;
        private readonly MorphActionSet _actionSet;

        public WorkItemTypeDefinitionComparer(ProcessTemplateMap processTemplateMap, MorphActionSet actionSet)
        {
            _processTemplateMap = processTemplateMap;
            _actionSet = actionSet;
        }

        public void Compare(WorkItemTypeDefinition goal, WorkItemTypeDefinition current)
        {
            // in hindsight, for better separation of concerns, the code for identifying differences should be distinct from the code for creating actions to resolve the differences

            var modifyTypeAction = new ModifyWorkItemTypeDefinitionMorphAction(current.Name);
            _actionSet.PrepareWorkItemTypeDefinitions.Add(modifyTypeAction);

            var finalModifyTypeAction = new ModifyWorkItemTypeDefinitionMorphAction(current.Name);
            _actionSet.FinaliseWorkItemTypeDefinitions.Add(finalModifyTypeAction);

            var exportDataAction = new ExportWorkItemDataMorphAction(current.Name);
            _actionSet.ProcessWorkItemData.Add(exportDataAction);

            foreach (var enumerator in goal.Fields)
            {
                var goalField = enumerator;

                // check if the field already exists
                var currentField = current.Fields.SingleOrDefault(t => string.Equals(t.ReferenceName, goalField.ReferenceName, StringComparison.OrdinalIgnoreCase));
                if (currentField == null)
                {
                    // the field doesn't exist, add it
                    modifyTypeAction.AddFieldDefinition(goalField.Element);
                }
                else
                {
                    // the field does exist, ensure it is similar enough to proceed
                    var isNameMatch = string.Equals(currentField.Name, goalField.Name, StringComparison.OrdinalIgnoreCase);
                    var isTypeMatch = currentField.Type == goalField.Type;
                    if (!isNameMatch)
                    {
                        Debug.WriteLine("NAME CHANGE: {0}.{1} > {2}.{3}", goal.Name, goalField.Name, current.Name, currentField.Name);
                        // TODO different friendly names. witadmin changefield?
                    }
                    if (!isTypeMatch)
                    {
                        Debug.WriteLine("TYPE CHANGE: {0}.{1}.{2} > {3}.{4}.{5}", goal.Name, goalField.ReferenceName, goalField.Type, current.Name, currentField.ReferenceName, currentField.Type);
                        // TODO different type. witadmin changefield? data copy? fail?
                    }

                    if (isNameMatch && isTypeMatch)
                    {
                        // update the metadata on the field
                        finalModifyTypeAction.ReplaceFieldDefinition(currentField.ReferenceName, goalField.Element);
                    }
                }
            }

            foreach (var enumerator in current.Fields)
            {
                var currentField = enumerator;

                // find the matching field in the goal with the same name
                var goalField = goal.Fields.SingleOrDefault(s => string.Equals(s.ReferenceName, currentField.ReferenceName, StringComparison.OrdinalIgnoreCase));
                if (goalField == null)
                {
                    var mappedGoalFieldName = _processTemplateMap.WorkItemFieldMap.GetGoalByCurrent(currentField.ReferenceName);
                    if (mappedGoalFieldName != null)
                    {
                        // find the matching field in the goal using the mapped name
                        goalField = goal.Fields.SingleOrDefault(s => string.Equals(s.ReferenceName, mappedGoalFieldName, StringComparison.OrdinalIgnoreCase));
                    }

                    if (goalField != null)
                    {
                        // the field is mapped to a new field, copy the data then remove the original field
                        _actionSet.ProcessWorkItemData.Add(new CopyWorkItemDataMorphAction(current.Name, currentField.ReferenceName, goalField.ReferenceName));
                        finalModifyTypeAction.RemoveFieldDefinition(currentField.ReferenceName);
                    }
                    else if (_processTemplateMap.SystemFieldReferenceNames.Contains(currentField.ReferenceName))
                    {
                        // ignore this extra system field
                    }
                    else
                    {
                        // the field does not exist in the goal, export the data for backup then remove the original field
                        exportDataAction.AddExportField(currentField.ReferenceName);
                        finalModifyTypeAction.RemoveFieldDefinition(currentField.ReferenceName);
                    }
                }
            }

            foreach (var goalState in goal.States)
            {
                // check for matching states
                var currentState = current.States.SingleOrDefault(t => string.Equals(t.Value, goalState.Value, StringComparison.OrdinalIgnoreCase));
                if (currentState == null)
                {
                    // the state doesn't exist, add it
                    modifyTypeAction.AddWorkflowState(goalState.Element);
                }
            }

            foreach (var enumerator in current.States)
            {
                var currentState = enumerator;
                
                // find the matching state
                var goalState = goal.States.SingleOrDefault(s => string.Equals(s.Value, currentState.Value, StringComparison.OrdinalIgnoreCase));
                if (goalState == null)
                {
                    // no match, check if there is a state map
                    var mappedGoalState = _processTemplateMap.GetWorkItemStateMap(current.Name).GetGoalByCurrent(currentState.Value);
                    if (mappedGoalState != null)
                    {
                        // add a new transition from the current state to the new mapped state
                        const string defaultReason = "Process Template Change";
                        modifyTypeAction.AddWorkflowTransition(currentState.Value, mappedGoalState, defaultReason);

                        // change the current state to new state for existing work items
                        _actionSet.ProcessWorkItemData.Add(new ModifyWorkItemStateMorphAction(current.Name, currentState.Value, mappedGoalState));

                        // remove the obsolete state and the related transitions
                        finalModifyTypeAction.RemoveWorkflowState(currentState.Value); // ReplaceWorkflow below probably makes this irrelevant
                    }
                }
            }

            // replace the workflow (states and transitions)
            finalModifyTypeAction.ReplaceWorkflow(goal.WorkflowElement);

            // replace the form layout 
            finalModifyTypeAction.ReplaceForm(goal.FormElement);

            // rename the current work item type to the name used by the goal
            if (!string.Equals(goal.Name, current.Name, StringComparison.OrdinalIgnoreCase))
            {
                _actionSet.FinaliseWorkItemTypeDefinitions.Add(new RenameWitdMorphAction(current.Name, goal.Name));
            }

        }

    }
}