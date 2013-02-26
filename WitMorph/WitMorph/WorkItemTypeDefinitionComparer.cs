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

        public void Compare(WorkItemTypeDefinition source, WorkItemTypeDefinition target)
        {
            // in hindsight, for better separation of concerns, the code for identifying differences should be distinct from the code for creating actions to resolve the differences

            var modifyTypeAction = new ModifyWorkItemTypeDefinitionMorphAction(target.Name);
            _actionSet.PrepareWorkItemTypeDefinitions.Add(modifyTypeAction);

            var finalModifyTypeAction = new ModifyWorkItemTypeDefinitionMorphAction(target.Name);
            _actionSet.FinaliseWorkItemTypeDefinitions.Add(finalModifyTypeAction);

            var exportDataAction = new ExportWorkItemDataMorphAction(target.Name);
            _actionSet.ProcessWorkItemData.Add(exportDataAction);

            foreach (var enumerator in source.Fields)
            {
                var sourceField = enumerator;

                // check if the field already exists
                var targetField = target.Fields.SingleOrDefault(t => string.Equals(t.ReferenceName, sourceField.ReferenceName, StringComparison.OrdinalIgnoreCase));
                if (targetField == null)
                {
                    // the field doesn't exist, add it
                    modifyTypeAction.AddFieldDefinition(sourceField.Element);
                }
                else
                {
                    // the field does exist, ensure it is similar enough to proceed
                    var isNameMatch = string.Equals(targetField.Name, sourceField.Name, StringComparison.OrdinalIgnoreCase);
                    var isTypeMatch = targetField.Type == sourceField.Type;
                    if (!isNameMatch)
                    {
                        Debug.WriteLine("NAME CHANGE: {0}.{1} > {2}.{3}", source.Name, sourceField.Name, target.Name, targetField.Name);
                        // TODO different friendly names. witadmin changefield?
                    }
                    if (!isTypeMatch)
                    {
                        Debug.WriteLine("TYPE CHANGE: {0}.{1}.{2} > {3}.{4}.{5}", source.Name, sourceField.ReferenceName, sourceField.Type, target.Name, targetField.ReferenceName, targetField.Type);
                        // TODO different type. witadmin changefield? data copy? fail?
                    }

                    if (isNameMatch && isTypeMatch)
                    {
                        // update the metadata on the field
                        finalModifyTypeAction.ReplaceFieldDefinition(targetField.ReferenceName, sourceField.Element);
                    }
                }
            }

            foreach (var enumerator in target.Fields)
            {
                var targetField = enumerator;

                // find the matching field in the source with the same name
                var sourceField = source.Fields.SingleOrDefault(s => string.Equals(s.ReferenceName, targetField.ReferenceName, StringComparison.OrdinalIgnoreCase));
                if (sourceField == null)
                {
                    var mappedSourceFieldName = _processTemplateMap.WorkItemFieldMap.GetGoalByCurrent(targetField.ReferenceName);
                    if (mappedSourceFieldName != null)
                    {
                        // find the matching field in the source using the mapped name
                        sourceField = source.Fields.SingleOrDefault(s => string.Equals(s.ReferenceName, mappedSourceFieldName, StringComparison.OrdinalIgnoreCase));
                    }

                    if (sourceField != null)
                    {
                        // the field is mapped to a new field, copy the data then remove the original field
                        _actionSet.ProcessWorkItemData.Add(new CopyWorkItemDataMorphAction(target.Name, targetField.ReferenceName, sourceField.ReferenceName));
                        finalModifyTypeAction.RemoveFieldDefinition(targetField.ReferenceName);
                    }
                    else if (_processTemplateMap.SystemFieldReferenceNames.Contains(targetField.ReferenceName))
                    {
                        // ignore this extra system field
                    }
                    else
                    {
                        // the field does not exist in the source, export the data for backup then remove the original field
                        exportDataAction.AddExportField(targetField.ReferenceName);
                        finalModifyTypeAction.RemoveFieldDefinition(targetField.ReferenceName);
                    }
                }
            }

            foreach (var sourceState in source.States)
            {
                // check for matching states
                var targetState = target.States.SingleOrDefault(t => string.Equals(t.Value, sourceState.Value, StringComparison.OrdinalIgnoreCase));
                if (targetState == null)
                {
                    // the state doesn't exist, add it
                    modifyTypeAction.AddWorkflowState(sourceState.Element);
                }
            }

            foreach (var enumerator in target.States)
            {
                var targetState = enumerator;
                
                // find the matching state
                var sourceState = source.States.SingleOrDefault(s => string.Equals(s.Value, targetState.Value, StringComparison.OrdinalIgnoreCase));
                if (sourceState == null)
                {
                    // no match, check if there is a state map
                    var mappedSourceState = _processTemplateMap.GetWorkItemStateMap(target.Name).GetGoalByCurrent(targetState.Value);
                    if (mappedSourceState != null)
                    {
                        // add a new transition from the current state to the new mapped state
                        const string defaultReason = "Process Template Change";
                        modifyTypeAction.AddWorkflowTransition(targetState.Value, mappedSourceState, defaultReason);

                        // change the current state to new state for existing work items
                        _actionSet.ProcessWorkItemData.Add(new ModifyWorkItemStateMorphAction(target.Name, targetState.Value, mappedSourceState));

                        // remove the obsolete state and the related transitions
                        finalModifyTypeAction.RemoveWorkflowState(targetState.Value); // ReplaceWorkflow below probably makes this irrelevant
                    }
                }
            }

            // replace the workflow (states and transitions)
            finalModifyTypeAction.ReplaceWorkflow(source.WorkflowElement);

            // replace the form layout 
            finalModifyTypeAction.ReplaceForm(source.FormElement);

            // rename the target work item type to the name used by the source 
            if (!string.Equals(source.Name, target.Name, StringComparison.OrdinalIgnoreCase))
            {
                _actionSet.FinaliseWorkItemTypeDefinitions.Add(new RenameWitdMorphAction(target.Name, source.Name));
            }

        }

    }
}