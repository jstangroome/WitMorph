using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WitMorph.Actions;
using WitMorph.Differences;
using WitMorph.Model;
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

        [Obsolete]
        public void Compare(WorkItemTypeDefinition current, WorkItemTypeDefinition goal)
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
                    modifyTypeAction.AddFieldDefinition(goalField);
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
                        finalModifyTypeAction.ReplaceFieldDefinition(goalField);
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
                    modifyTypeAction.AddWorkflowState(goalState);
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

        public IEnumerable<IDifference> FindDifferences(WorkItemTypeDefinition current, WorkItemTypeDefinition goal)
        {
            var differences = new List<IDifference>();

            if (!current.Name.Equals(goal.Name, StringComparison.OrdinalIgnoreCase))
            {
                differences.Add(new RenamedWorkItemTypeDefinitionDifference(current.Name, goal.Name));
            }

            FindStateDifferences(current.Name, current.States, goal.States, differences);

            FindFieldDifferences(current.Name, current.Fields, goal.Fields, differences);

            if (current.FormElement.OuterXml != goal.FormElement.OuterXml)
            {
                differences.Add(new ChangedWorkItemFormDifference(current.Name, goal.FormElement));
            }

            if (FindTransitionDifferences(current.Name, current.Transitions, goal.Transitions))
            //if (current.WorkflowElement.OuterXml != goal.WorkflowElement.OuterXml)
            {
                differences.Add(new ChangedWorkItemWorkflowDifference(current.Name, goal.WorkflowElement)); 
                // TODO make this obsolete with comprehensive state and transition difference instances
            }

            return differences;
        }

        private void FindFieldDifferences(string currentWorkItemTypeName, IEnumerable<WitdField> currentFields, IEnumerable<WitdField> goalFields, IList<IDifference> differences)
        {
            var fieldMatchAndMap = new MatchAndMap<WitdField, string>(s => s.ReferenceName, StringComparer.OrdinalIgnoreCase, _processTemplateMap.WorkItemFieldMap);
            var fieldMatchResult = fieldMatchAndMap.Match(currentFields, goalFields);

            foreach (var goalField in fieldMatchResult.GoalOnly)
            {
                differences.Add(new AddedWorkItemFieldDifference(currentWorkItemTypeName, goalField));
            }

            foreach (var currentField in fieldMatchResult.CurrentOnly)
            {
                // ignore removal of system fields as they are often omitted from templates
                if (!_processTemplateMap.SystemFieldReferenceNames.Contains(currentField.ReferenceName))
                    differences.Add(new RemovedWorkItemFieldDifference(currentWorkItemTypeName, currentField.ReferenceName));
            }

            foreach (var pair in fieldMatchResult.Pairs)
            {
                if (!string.Equals(pair.Current.ReferenceName, pair.Goal.ReferenceName, StringComparison.OrdinalIgnoreCase))
                {
                    differences.Add(new RenamedWorkItemFieldDifference(currentWorkItemTypeName, pair.Current.ReferenceName, pair.Goal));
                }
                else if (!pair.Current.Equals(pair.Goal))
                {
                    // TODO consider that the decision to exclude system items may not belong here
                    if (!_processTemplateMap.SystemFieldReferenceNames.Contains(pair.Current.ReferenceName))
                    {
                        differences.Add(new ChangedWorkItemFieldDifference(currentWorkItemTypeName, pair.Current.ReferenceName, pair.Goal));
                    }
                }
                // TODO field changes (friendly name, data type, helptext, reporting options, validation, etc)
            }

        }

        private void FindStateDifferences(string currentWorkItemTypeName, IEnumerable<WitdState> currentStates, IEnumerable<WitdState> goalStates, ICollection<IDifference> differences)
        {
            var stateMatchAndMap = new MatchAndMap<WitdState, string>(s => s.Value, StringComparer.OrdinalIgnoreCase, _processTemplateMap.GetWorkItemStateMap(currentWorkItemTypeName));
            var stateMatchResult = stateMatchAndMap.Match(currentStates, goalStates);

            foreach (var goalState in stateMatchResult.GoalOnly)
            {
                differences.Add(new AddedWorkItemStateDifference(currentWorkItemTypeName, goalState));
            }

            foreach (var currentState in stateMatchResult.CurrentOnly)
            {
                differences.Add(new RemovedWorkItemStateDifference(currentState.Value));
            }

            foreach (var pair in stateMatchResult.Pairs)
            {
                if (!string.Equals(pair.Current.Value, pair.Goal.Value, StringComparison.OrdinalIgnoreCase))
                {
                    differences.Add(new RenamedWorkItemStateDifference(currentWorkItemTypeName, pair.Current.Value, pair.Goal));
                } 
                else if (!pair.Current.Equals(pair.Goal))
                {
                    differences.Add(new ChangedWorkItemStateDifference(currentWorkItemTypeName, pair.Current.Value, pair.Goal));
                }
                // TODO state validation changes and transition differences
            }
        }

        private bool FindTransitionDifferences(string currentWorkItemTypeName, ISet<WitdTransition> currentTransitions, ISet<WitdTransition> goalTransitions)
        {
            var map = new TransitionKeyMap(_processTemplateMap.GetWorkItemStateMap(currentWorkItemTypeName));
            var transitionMatchAndMap = new MatchAndMap<WitdTransition, TransitionKey>(t => new TransitionKey(t), EqualityComparer<TransitionKey>.Default, map);
            var result = transitionMatchAndMap.Match(currentTransitions, goalTransitions);

            var isDifferent = result.GoalOnly.Any() || result.CurrentOnly.Any() || result.Pairs.Any(p => !p.Current.Equals(p.Goal));

            return isDifferent;
        }

        class TransitionKey
        {
            private readonly WitdTransition _transition;
            private readonly string _from;
            private readonly string _to;

            public TransitionKey(WitdTransition transition) : this(transition, transition.From, transition.To) {}

            public TransitionKey(TransitionKey key, string from, string to) :this(key._transition, from, to) {}

            TransitionKey(WitdTransition transition, string from, string to)
            {
                _transition = transition;
                _from = from;
                _to = to;
            }

            public string From { get { return _from; } }
            public string To { get { return _to; } }

            public override bool Equals(object obj)
            {
                var other = obj as TransitionKey;
                if (other == null) return false;

                if (other._from != _from) return false;
                if (other._to != _to) return false;
                if (other._transition.For != _transition.For) return false;
                if (other._transition.Not != _transition.Not) return false;

                return true;
            }
        }

        class TransitionKeyMap : ICurrentToGoalMap<TransitionKey>
        {
            private readonly ICurrentToGoalMap<string> _stateMap;

            public TransitionKeyMap(ICurrentToGoalMap<string> stateMap)
            {
                _stateMap = stateMap;
            }

            public TransitionKey GetGoalByCurrent(TransitionKey current)
            {
                return new TransitionKey(current, _stateMap.GetGoalByCurrent(current.From), _stateMap.GetGoalByCurrent(current.To));
            }
        }
    }
}