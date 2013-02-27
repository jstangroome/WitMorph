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

        public static ProcessTemplateMap ConvertScrum2ToAgile6()
        {
            var map = new ProcessTemplateMap();

            CurrentToGoalMap<string> stateMap;

            // Scrum type => Agile type
            map._workItemTypeMap.Add("Product Backlog Item", "User Story");

            // Scrum Task => Agile Task
            stateMap = new CurrentToGoalMap<string>(StringComparer.OrdinalIgnoreCase);
            stateMap.Add("To Do", "New");
            stateMap.Add("In Progress", "Active");
            stateMap.Add("Done", "Closed");
            map._workItemStateMaps.Add("Task", stateMap);

            // Scrum Bug => Agile Bug
            stateMap = new CurrentToGoalMap<string>(StringComparer.OrdinalIgnoreCase);
            stateMap.Add("New", "Active");
            stateMap.Add("Approved", "Active");
            stateMap.Add("Committed", "Active");
            stateMap.Add("Done", "Resolved");
            stateMap.Add("Removed", "Closed");
            map._workItemStateMaps.Add("Bug", stateMap);

            // Scrum Product Backlog Item => Agile User Story
            stateMap = new CurrentToGoalMap<string>(StringComparer.OrdinalIgnoreCase);
            stateMap.Add("Approved", "Active");
            stateMap.Add("Committed", "Active");
            stateMap.Add("Done", "Resolved");
            map._workItemStateMaps.Add("Product Backlog Item", stateMap);

            // Scrum field => Agile field
            map._workItemFieldMap.Add("Microsoft.VSTS.Common.BacklogPriority", "Microsoft.VSTS.Common.StackRank");
            map._workItemFieldMap.Add("Microsoft.VSTS.Scheduling.Effort", "Microsoft.VSTS.Scheduling.StoryPoints");
            //TODO consider appending Microsoft.VSTS.Common.AcceptanceCriteria content to System.Description

            return map;
        }

        public static ProcessTemplateMap ConvertAgile6ToScrum2()
        {
            var map = new ProcessTemplateMap();

            map._workItemTypeMap.Add("User Story", "Product Backlog Item");

            map._workItemFieldMap.Add("Microsoft.VSTS.Common.StackRank", "Microsoft.VSTS.Common.BacklogPriority");

            // Agile Task => Scrum Task
            var taskStateMap = new CurrentToGoalMap<string>(StringComparer.OrdinalIgnoreCase);
            taskStateMap.Add("Active", "In Progress");
            map._workItemStateMaps.Add("Task", taskStateMap);

            return map;
        }

        private ProcessTemplateMap()
        {
            _systemFieldReferenceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "System.Watermark", "System.TeamProject", "System.IterationId", "System.ExternalLinkCount", "System.HyperLinkCount", "System.AttachedFileCount", "System.NodeName", "System.RevisedDate", "System.AreaId", "System.AuthorizedAs", "System.AuthorizedDate", "System.Rev", "System.WorkItemType", "System.Description", "System.RelatedLinkCount", "System.ChangedDate", "System.ChangedBy", "System.CreatedDate", "System.CreatedBy", "System.History" };

            _workItemTypeMap = new CurrentToGoalMap<string>(StringComparer.OrdinalIgnoreCase);
            _workItemStateMaps = new Dictionary<string, CurrentToGoalMap<string>>(StringComparer.OrdinalIgnoreCase);
            _workItemFieldMap = new CurrentToGoalMap<string>(StringComparer.OrdinalIgnoreCase);
        }

        public HashSet<string> SystemFieldReferenceNames { get { return _systemFieldReferenceNames; } }

        public CurrentToGoalMap<string> WorkItemTypeMap { get { return _workItemTypeMap; } }

        public CurrentToGoalMap<string> GetWorkItemStateMap(string currentWorkItemTypeName)
        {
            if (_workItemStateMaps.ContainsKey(currentWorkItemTypeName))
            {
                return _workItemStateMaps[currentWorkItemTypeName];
            }
            return new CurrentToGoalMap<string>(StringComparer.OrdinalIgnoreCase);
        }

        public CurrentToGoalMap<string> WorkItemFieldMap { get { return _workItemFieldMap; } }
    }
}