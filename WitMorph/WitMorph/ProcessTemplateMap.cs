using System;
using System.Collections.Generic;
using WitMorph.Structures;

namespace WitMorph
{
    public class ProcessTemplateMap
    {
        private readonly HashSet<string> _systemFieldReferenceNames;
        private readonly CurrentToGoalMap<string> _workItemTypeMap;
        private readonly CurrentToGoalMap<string> _workItemFieldMap;
        private readonly Dictionary<string, CurrentToGoalMap<string>> _workItemStateMaps;

        public ProcessTemplateMap()
        {
            _systemFieldReferenceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "System.Watermark", "System.TeamProject", "System.IterationId", "System.ExternalLinkCount", "System.HyperLinkCount", "System.AttachedFileCount", "System.NodeName", "System.RevisedDate", "System.AreaId", "System.AuthorizedAs", "System.AuthorizedDate", "System.Rev", "System.WorkItemType", "System.Description", "System.RelatedLinkCount", "System.ChangedDate", "System.ChangedBy", "System.CreatedDate", "System.CreatedBy", "System.History" };
            
            // currently only implements map to convert Scrum 2 to Agile 6

            // Agile type <= Scrum type
            _workItemTypeMap = new CurrentToGoalMap<string>(StringComparer.OrdinalIgnoreCase);
            _workItemTypeMap.Add("Product Backlog Item", "User Story");

            CurrentToGoalMap<string> stateMap;
            _workItemStateMaps = new Dictionary<string, CurrentToGoalMap<string>>(StringComparer.OrdinalIgnoreCase);

            // Agile Task <= Scrum Task
            stateMap = new CurrentToGoalMap<string>(StringComparer.OrdinalIgnoreCase);
            stateMap.Add("To Do", "New");
            stateMap.Add("In Progress", "Active");
            stateMap.Add("Done", "Closed");
            _workItemStateMaps.Add("Task", stateMap);

            // Agile Bug <= Scrum Bug
            stateMap = new CurrentToGoalMap<string>(StringComparer.OrdinalIgnoreCase);
            stateMap.Add("New", "Active");
            stateMap.Add("Approved", "Active");
            stateMap.Add("Committed", "Active");
            stateMap.Add("Done", "Resolved");
            stateMap.Add("Removed", "Closed");
            _workItemStateMaps.Add("Bug", stateMap);

            // Agile User Story <= Scrum Product Backlog Item
            stateMap = new CurrentToGoalMap<string>(StringComparer.OrdinalIgnoreCase);
            stateMap.Add("Approved", "Active");
            stateMap.Add("Committed", "Active");
            stateMap.Add("Done", "Resolved");
            _workItemStateMaps.Add("Product Backlog Item", stateMap);

            // Agile field <= Scrum field
            _workItemFieldMap = new CurrentToGoalMap<string>(StringComparer.OrdinalIgnoreCase);
            _workItemFieldMap.Add("Microsoft.VSTS.Common.BacklogPriority", "Microsoft.VSTS.Common.StackRank");
            _workItemFieldMap.Add("Microsoft.VSTS.Scheduling.Effort", "Microsoft.VSTS.Scheduling.StoryPoints");
            //TODO consider appending Microsoft.VSTS.Common.AcceptanceCriteria content to System.Description

        }

        public HashSet<string> SystemFieldReferenceNames { get { return _systemFieldReferenceNames; } }

        public CurrentToGoalMap<string> WorkItemTypeMap { get { return _workItemTypeMap; } }

        public CurrentToGoalMap<string> GetWorkItemStateMap(string targetWorkItemTypeName)
        {
            if (_workItemStateMaps.ContainsKey(targetWorkItemTypeName))
            {
                return _workItemStateMaps[targetWorkItemTypeName];
            }
            return new CurrentToGoalMap<string>(StringComparer.OrdinalIgnoreCase);
        }

        public CurrentToGoalMap<string> WorkItemFieldMap { get { return _workItemFieldMap; } }
    }
}