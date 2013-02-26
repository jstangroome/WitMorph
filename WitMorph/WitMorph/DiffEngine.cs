using System;
using System.Collections.Generic;
using WitMorph.Differences;
using WitMorph.Structures;

namespace WitMorph
{
    public class DiffEngine
    {
        private readonly ProcessTemplateMap _processTemplateMap;

        public DiffEngine(ProcessTemplateMap processTemplateMap)
        {
            _processTemplateMap = processTemplateMap;
        }

        public IEnumerable<IDifference> CompareProcessTemplates(ProcessTemplate current, ProcessTemplate goal)
        {
            var differences = new List<IDifference>();
            
            var mm = new MatchAndMap<WorkItemTypeDefinition, string>(i => i.Name, StringComparer.OrdinalIgnoreCase, _processTemplateMap.WorkItemTypeMap);
            var matchResult = mm.Match(current.WorkItemTypeDefinitions, goal.WorkItemTypeDefinitions);

            foreach (var goalItem in matchResult.GoalOnly)
            {
               differences.Add(new AddedWorkItemTypeDefinitionDifference(goalItem));
            }

            foreach (var currentItem in matchResult.CurrentOnly)
            {
               differences.Add(new RemovedWorkItemTypeDefinitionDifference(currentItem.Name));
            }

            var witdComparer = new WorkItemTypeDefinitionComparer(_processTemplateMap, null);
            foreach (var pair in matchResult.Pairs)
            {
                differences.AddRange(witdComparer.FindDifferences(pair.Current, pair.Goal));
            }

            return differences;
        }
    }

    public class ProcessTemplate
    {
        public IReadOnlyList<WorkItemTypeDefinition> WorkItemTypeDefinitions { get; set; }
    }
}
