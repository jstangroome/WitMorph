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
            var matchResult = mm.Match(goal.WorkItemTypeDefinitions, current.WorkItemTypeDefinitions);

            foreach (var goalItem in matchResult.TargetOnly)
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
