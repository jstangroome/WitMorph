using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using WitMorph.Structures;

namespace WitMorph
{
    public class ProcessTemplateMap
    {
        private readonly HashSet<string> _systemFieldReferenceNames;
        private readonly CurrentToGoalMap<string> _workItemTypeMap;
        private readonly CurrentToGoalMap<string> _workItemFieldMap;
        private readonly Dictionary<string, CurrentToGoalMap<string>> _workItemStateMaps;

        public static ProcessTemplateMap Empty()
        {
            return new ProcessTemplateMap();
        }

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

        private ProcessTemplateMap()
        {
            _systemFieldReferenceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                                         {
                                             "System.Id", "System.Watermark", "System.TeamProject", "System.IterationId",
                                             "System.ExternalLinkCount", "System.HyperLinkCount", "System.AttachedFileCount",
                                             "System.NodeName", "System.RevisedDate", "System.AreaId", "System.AuthorizedAs",
                                             "System.AuthorizedDate", "System.Rev", "System.WorkItemType", "System.Description",
                                             "System.RelatedLinkCount", "System.ChangedDate", "System.ChangedBy",
                                             "System.CreatedDate", "System.CreatedBy", "System.History",
                                             "System.Tags" // tags?
                                         };

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

        public static ProcessTemplateMap Read(Stream stream)
        {
            var map = new ProcessTemplateMap();

            var settings = new XmlReaderSettings {CloseInput = false};
            using (var reader = XmlReader.Create(stream, settings))
            {
                reader.ReadStartElement("processtemplatemap");
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (reader.LocalName)
                            {
                                case "workitemtype":
                                    map._workItemTypeMap.Add(reader.GetAttribute("from"), reader.GetAttribute("to"));
                                    break;
                                case "workitemfield":
                                    map._workItemFieldMap.Add(reader.GetAttribute("from"), reader.GetAttribute("to"));
                                    break;
                                case "workitemstate":
                                    var typeName = reader.GetAttribute("type");
                                    if (string.IsNullOrWhiteSpace(typeName))
                                    {
                                        throw new InvalidOperationException("workitemstate element is missing type attribute.");
                                    }
                                    var stateMap = map.GetWorkItemStateMap(typeName);
                                    if (!map._workItemStateMaps.ContainsKey(typeName))
                                    {
                                        map._workItemStateMaps.Add(typeName, stateMap);
                                    }
                                    stateMap.Add(reader.GetAttribute("from"), reader.GetAttribute("to"));
                                    break;
                            }
                            break;
                    }
                }
            }

            return map;
        }
    }
}