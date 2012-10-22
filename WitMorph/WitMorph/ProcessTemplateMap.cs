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
        private readonly SourceTargetMap<string> _workItemStateMap;

        public ProcessTemplateMap()
        {
            _systemFieldReferenceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "System.Watermark", "System.TeamProject", "System.IterationId", "System.ExternalLinkCount", "System.HyperLinkCount", "System.AttachedFileCount", "System.NodeName", "System.RevisedDate", "System.AreaId", "System.AuthorizedAs", "System.AuthorizedDate", "System.Rev", "System.WorkItemType", "System.Description", "System.RelatedLinkCount", "System.ChangedDate", "System.ChangedBy", "System.CreatedDate", "System.CreatedBy", "System.History" };
            
            // currently only implements map to convert Scrum 2 to Agile 6
            _workItemTypeMap = new SourceTargetMap<string>(StringComparer.OrdinalIgnoreCase);
            _workItemTypeMap.Add("User Story", "Product Backlog Item");

            _workItemStateMap = new SourceTargetMap<string>(StringComparer.OrdinalIgnoreCase);
            _workItemStateMap.Add("New", "Todo");
            _workItemStateMap.Add("Active", "In Progress");
            _workItemStateMap.Add("Closed", "Done");
            // TODO per-work item type state maps

            _workItemFieldMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {{"Microsoft.VSTS.Common.BacklogPriority", "Microsoft.VSTS.Common.StackRank"}, {"Microsoft.VSTS.Scheduling.Effort", "Microsoft.VSTS.Scheduling.StoryPoints"}};
            //TODO consider appending Microsoft.VSTS.Common.AcceptanceCriteria content to System.Description

        }

        public HashSet<string> SystemFieldReferenceNames { get { return _systemFieldReferenceNames; } }

        public SourceTargetMap<string> WorkItemTypeMap { get { return _workItemTypeMap; } }

        public SourceTargetMap<string> WorkItemStateMap { get { return _workItemStateMap; } }

        public IReadOnlyDictionary<string, string> WorkItemFieldMap { get { return _workItemFieldMap; } }
    }
}