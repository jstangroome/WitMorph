using System;
using System.Collections.Generic;
using System.Linq;
using WitMorph.Actions;

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

        public void Compare(IEnumerable<WorkItemTypeDefinition> sourceWitds, IEnumerable<WorkItemTypeDefinition> targetWitds)
        {
            // handle multiple enumeration
            sourceWitds = sourceWitds.ToArray();
            targetWitds = targetWitds.ToArray(); 

            var updateWitds = new List<SourceTargetPair<WorkItemTypeDefinition>>();
            var addWitds = new List<WorkItemTypeDefinition>();
            var removeWitds = new List<WorkItemTypeDefinition>();

            foreach (var sourceWitd in sourceWitds)
            {
                var targetWitd = targetWitds.SingleOrDefault(t => string.Equals(t.Name, sourceWitd.Name, StringComparison.OrdinalIgnoreCase));
                if (targetWitd == null)
                {
                    // no match
                    addWitds.Add(sourceWitd);
                }
                else
                {
                    // exists in target
                    updateWitds.Add(new SourceTargetPair<WorkItemTypeDefinition> { Source = sourceWitd, Target = targetWitd });
                }
            }

            foreach (var targetWitd in targetWitds)
            {
                var sourceWitd = sourceWitds.SingleOrDefault(s => string.Equals(s.Name, targetWitd.Name, StringComparison.OrdinalIgnoreCase));
                if (sourceWitd == null)
                {
                    // no match
                    removeWitds.Add(targetWitd);
                }
            }

            // attempt to map removed Witds to added Witds
            foreach (var enumerator in removeWitds.ToArray())
            {
                var targetWitd = enumerator;

                // is the removed witd mapped to another witd
                if (_processTemplateMap.WorkItemTypeMap.ContainsKey(targetWitd.Name))
                {
                    // is the mapped witd in the added list
                    var sourceWitd = addWitds.SingleOrDefault(s => string.Equals(s.Name, _processTemplateMap.WorkItemTypeMap[targetWitd.Name], StringComparison.OrdinalIgnoreCase));
                    if (sourceWitd != null)
                    {
                        // convert the add and remove to an update instead
                        updateWitds.Add(new SourceTargetPair<WorkItemTypeDefinition> { Source = sourceWitd, Target = targetWitd });
                        addWitds.Remove(sourceWitd);
                        removeWitds.Remove(targetWitd);
                    }
                }
            }

            // add the new work item type definitions first
            foreach (var sourceWitd in addWitds)
            {
                _actionSet.PrepareWorkItemTypeDefinitions.Add(new ImportWorkItemTypeDefinitionMorphAction(sourceWitd.WITDElement, forceImport: true));
            }

            // remove the old work item type definitions last
            foreach (var targetWitd in removeWitds)
            {
                // TODO export data for deleted work items
                _actionSet.FinaliseWorkItemTypeDefinitions.Add(new DestroyWitdMorphAction(targetWitd.Name));
                // TODO consider making destroy optional
            }

            // generate morph actions to update the matching work item type definitions
            var witdComparer = new WorkItemTypeDefinitionComparer(_processTemplateMap, _actionSet);
            foreach (var pair in updateWitds)
            {
                witdComparer.Compare(pair.Source, pair.Target);
            }
        }
    }
}