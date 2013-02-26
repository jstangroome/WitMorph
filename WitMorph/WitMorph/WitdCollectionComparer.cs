using System;
using System.Collections.Generic;
using WitMorph.Actions;
using WitMorph.Structures;

namespace WitMorph
{
    public class WitdCollectionComparer
    {
        private readonly ProcessTemplateMap _processTemplateMap;
        private readonly MorphActionSet _actionSet;

        public WitdCollectionComparer(ProcessTemplateMap processTemplateMap, MorphActionSet actionSet)
        {
            _processTemplateMap = processTemplateMap;
            _actionSet = actionSet;
        }

        public void Compare(IEnumerable<WorkItemTypeDefinition> goalWitds, IEnumerable<WorkItemTypeDefinition> currentWitds)
        {
            var mm = new MatchAndMap<WorkItemTypeDefinition, string>(i => i.Name, StringComparer.OrdinalIgnoreCase, _processTemplateMap.WorkItemTypeMap);
            var matchResult = mm.Match(currentWitds, goalWitds);

            foreach (var sourceItem in matchResult.GoalOnly)
            {
                // add the new work item type definitions first
                _actionSet.PrepareWorkItemTypeDefinitions.Add(new ImportWorkItemTypeDefinitionMorphAction(sourceItem));
            }

            foreach (var targetItem in matchResult.CurrentOnly)
            {
                // remove the old work item type definitions last
                _actionSet.ProcessWorkItemData.Add(new ExportWorkItemDataMorphAction(targetItem.Name, allFields: true));
                _actionSet.FinaliseWorkItemTypeDefinitions.Add(new DestroyWitdMorphAction(targetItem.Name));
            }

            var witdComparer = new WorkItemTypeDefinitionComparer(_processTemplateMap, _actionSet);
            foreach (var pair in matchResult.Pairs)
            {
                // generate morph actions to update the matching work item type definitions
                witdComparer.Compare(pair.Goal, pair.Current);
            }
        }
    }
}