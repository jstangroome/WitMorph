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
            var mm = new MatchAndMap<WorkItemTypeDefinition, string>(i => i.Name, StringComparer.OrdinalIgnoreCase, _processTemplateMap.WorkItemTypeMap);
            var matchResult = mm.Match(current.WorkItemTypeDefinitions, goal.WorkItemTypeDefinitions);

            foreach (var goalItem in matchResult.GoalOnly)
            {
                yield return new AddedWorkItemTypeDefinitionDifference(goalItem);
            }

        }
    }

    public class ProcessTemplate
    {
        public IReadOnlyList<WorkItemTypeDefinition> WorkItemTypeDefinitions { get; set; }
    }
}
