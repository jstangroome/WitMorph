using System;
using System.Collections.Generic;
using WitMorph.Structures;

namespace WitMorph
{
    public class ProcessTemplateMap
    {
        private readonly HashSet<string> _systemFieldReferenceNames;
        private readonly SourceTargetMap<string> _workItemTypeMap;
        private readonly Dictionary<string, string> _workItemFieldMap;
        private readonly Dictionary<string, SourceTargetMap<string>> _workItemStateMaps;

        public ProcessTemplateMap()
        {
            _systemFieldReferenceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "System.Watermark", "System.TeamProject", "System.IterationId", "System.ExternalLinkCount", "System.HyperLinkCount", "System.AttachedFileCount", "System.NodeName", "System.RevisedDate", "System.AreaId", "System.AuthorizedAs", "System.AuthorizedDate", "System.Rev", "System.WorkItemType", "System.Description", "System.RelatedLinkCount", "System.ChangedDate", "System.ChangedBy", "System.CreatedDate", "System.CreatedBy", "System.History" };
            
            // currently only implements map to convert Scrum 2 to Agile 6
            _workItemTypeMap = new SourceTargetMap<string>(StringComparer.OrdinalIgnoreCase);
            _workItemTypeMap.Add("User Story", "Product Backlog Item");

            SourceTargetMap<string> stateMap;
            _workItemStateMaps = new Dictionary<string, SourceTargetMap<string>>(StringComparer.OrdinalIgnoreCase);

            // Agile Task <= Scrum Task
            stateMap = new SourceTargetMap<string>(StringComparer.OrdinalIgnoreCase);
            stateMap.Add("New", "To Do");
            stateMap.Add("Active", "In Progress");
            stateMap.Add("Closed", "Done");
            _workItemStateMaps.Add("Task", stateMap);

            // Agile Bug <= Scrum Bug
            stateMap = new SourceTargetMap<string>(StringComparer.OrdinalIgnoreCase);
            stateMap.Add("Active", "New");
            stateMap.Add("Active", "Approved");
            stateMap.Add("Active", "Committed");
            stateMap.Add("Resolved", "Done");
            stateMap.Add("Closed", "Removed");
            _workItemStateMaps.Add("Bug", stateMap);

            // Agile User Story <= Scrum Product Backlog Item
            stateMap = new SourceTargetMap<string>(StringComparer.OrdinalIgnoreCase);
            stateMap.Add("Active", "Approved");
            stateMap.Add("Active", "Committed");
            stateMap.Add("Resolved", "Done");
            _workItemStateMaps.Add("Product Backlog Item", stateMap);

            _workItemFieldMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Microsoft.VSTS.Common.BacklogPriority", "Microsoft.VSTS.Common.StackRank" }, { "Microsoft.VSTS.Scheduling.Effort", "Microsoft.VSTS.Scheduling.StoryPoints" } };
            //TODO consider appending Microsoft.VSTS.Common.AcceptanceCriteria content to System.Description

        }

        public HashSet<string> SystemFieldReferenceNames { get { return _systemFieldReferenceNames; } }

        public SourceTargetMap<string> WorkItemTypeMap { get { return _workItemTypeMap; } }

        public SourceTargetMap<string> GetWorkItemStateMap(string targetWorkItemTypeName)
        {
            if (_workItemStateMaps.ContainsKey(targetWorkItemTypeName))
            {
                return _workItemStateMaps[targetWorkItemTypeName];
            }
            return new SourceTargetMap<string>(StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyDictionary<string, string> WorkItemFieldMap { get { return _workItemFieldMap; } }
    }
}