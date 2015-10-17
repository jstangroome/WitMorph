using System;
using System.Collections.Generic;
using System.Linq;
using WitMorph.Differences;
using WitMorph.Model;
using WitMorph.Structures;

namespace WitMorph
{
    class WorkItemTypeDefinitionComparer
    {
        private readonly ProcessTemplateMap _processTemplateMap;

        public WorkItemTypeDefinitionComparer(ProcessTemplateMap processTemplateMap)
        {
            _processTemplateMap = processTemplateMap;
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
                string[] currentStateNames = currentStates.Select(s => s.Value).ToArray();
                string[] goalStateNames = goalStates.Select(s => s.Value).ToArray();
                bool currentExistsInGoal = currentStateNames.Any(name => string.Equals(pair.Goal.Value, name, StringComparison.OrdinalIgnoreCase));
                bool goalExistsInCurrent = goalStateNames.Any(name => string.Equals(pair.Current.Value, name, StringComparison.OrdinalIgnoreCase));
                bool renameRegistered = differences.OfType<RenamedWorkItemStateDifference>().Any(diff => string.Equals(diff.GoalStateName, pair.Goal.Value, StringComparison.OrdinalIgnoreCase));

                if (!currentExistsInGoal && goalExistsInCurrent)
                {
                    differences.Add(new ConsolidatedWorkItemStateDifference(currentWorkItemTypeName, pair.Current.Value, pair.Goal));
                }
                else if (
                    !currentExistsInGoal 
                    && !goalExistsInCurrent)
                {
                    if (!renameRegistered)
                    {
                        differences.Add(new RenamedWorkItemStateDifference(currentWorkItemTypeName, pair.Current.Value, pair.Goal));
                    }
                    else
                    {
                        differences.Add(new ChangedWorkItemStateDifference(currentWorkItemTypeName, pair.Current.Value, pair.Goal));
                    }
                }
                else if (currentExistsInGoal && goalExistsInCurrent)
                {
                    differences.Add(new ChangedWorkItemStateDifference(currentWorkItemTypeName, pair.Current.Value, pair.Goal));
                }
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

            public override int GetHashCode()
            {
                return _from.GetHashCode()
                    ^ _to.GetHashCode()
                    ^ _transition.For.GetHashCode()
                    ^ _transition.Not.GetHashCode();
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