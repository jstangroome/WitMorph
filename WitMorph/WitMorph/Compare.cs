using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using WitMorph.Actions;

namespace WitMorph
{
    public class Compare
    {
        public IEnumerable<IMorphAction> Do(Uri collectionUri, string projectName)
        {
            var actionSet = new MorphActionSet();

            var collection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(collectionUri);
            var wistore = collection.GetService<WorkItemStore>();
            var wiproject = wistore.Projects[projectName];

            var targetWitds = new List<WorkItemTypeDefinition>();
            foreach (WorkItemType wit in wiproject.WorkItemTypes)
            {
                targetWitds.Add(new WorkItemTypeDefinition(wit.Export(false)));
            }


            var processTemplates = collection.GetService<IProcessTemplates>();
            var index = processTemplates.GetTemplateIndex("MSF for Agile Software Development 6.0");
            Debug.WriteLine(index);
            var templateFile = processTemplates.GetTemplateData(index);

            var templatePath = Path.GetTempFileName();
            File.Delete(templatePath);

            System.IO.Compression.ZipFile.ExtractToDirectory(templateFile, templatePath);
            File.Delete(templateFile);

            var xdoc = new XmlDocument();
            xdoc.Load(Path.Combine(templatePath,"ProcessTemplate.xml"));
            var wittasknode = (XmlElement)xdoc.SelectSingleNode("ProcessTemplate/groups/group[@id='WorkItemTracking']/taskList");

            var workitemsdoc = new XmlDocument();
            workitemsdoc.Load(Path.Combine(templatePath, wittasknode.GetAttribute("filename")));
            var witNodes = workitemsdoc.SelectNodes("tasks/task[@id='WITs']/taskXml/WORKITEMTYPES/WORKITEMTYPE");
            var sourceWitds = new List<WorkItemTypeDefinition>();
            foreach (XmlElement witNode in witNodes)
            {
                var doc = new XmlDocument();
                doc.Load(Path.Combine(templatePath, witNode.GetAttribute("fileName")));
                sourceWitds.Add(new WorkItemTypeDefinition(doc));
            }


            var updateWitds = new List<SourceTargetPair<WorkItemTypeDefinition>>();
            var addWitds = new List<WorkItemTypeDefinition>();
            var removeWitds = new List<WorkItemTypeDefinition>();
            foreach (var def in sourceWitds)
            {
                var match = targetWitds.SingleOrDefault(d => string.Equals(d.Name, def.Name, StringComparison.OrdinalIgnoreCase));
                if (match == null)
                {
                    // no match
                    addWitds.Add(def);
                }
                else
                {
                    // exists in target
                    updateWitds.Add(new SourceTargetPair<WorkItemTypeDefinition> { Source = def, Target = match });
                }
                Debug.WriteLine(def.Name);
            }
            foreach (var def in targetWitds)
            {
                var match = sourceWitds.SingleOrDefault(d => string.Equals(d.Name, def.Name, StringComparison.OrdinalIgnoreCase));
                if (match == null)
                {
                    // no match
                    removeWitds.Add(def);
                }
            }

            var knownMatches = new List<SourceTargetPair<string>>();
            knownMatches.Add(new SourceTargetPair<string>{Source = "User Story", Target = "Product Backlog Item"});

            foreach(var enumerator in addWitds.ToArray())
            {
                var addDef = enumerator;
                var match = knownMatches.Where(o => string.Equals(o.Source, addDef.Name, StringComparison.OrdinalIgnoreCase));
                foreach (var pair in match)
                {
                    var removeMatch = removeWitds.SingleOrDefault(d => string.Equals(d.Name, pair.Target, StringComparison.OrdinalIgnoreCase));
                    if (removeMatch != null)
                    {
                        updateWitds.Add(new SourceTargetPair<WorkItemTypeDefinition>{Source = addDef, Target = removeMatch});
                        addWitds.Remove(addDef);
                        removeWitds.Remove(removeMatch);
                    }
                }
            }

            foreach(var def in removeWitds)
            {
                Debug.WriteLine("DEL: " + def.Name);
            }

            foreach(var pair in updateWitds)
            {
                CompareWitds(pair.Source, pair.Target, actionSet);
            }

            Directory.Delete(templatePath, recursive:true);

            return actionSet.Combine();
        }

        private void CompareWitds(WorkItemTypeDefinition source, WorkItemTypeDefinition target, MorphActionSet actionSet)
        {
            var importAction = new ImportWorkItemTypeDefinitionMorphAction(target.WITDElement);
            actionSet.PrepareWorkItemTypeDefinitions.Add(importAction);

            var finalImportAction = new LazyImportWorkItemTypeDefinitionMorphAction(target.Name);
            actionSet.FinaliseWorkItemTypeDefinitions.Add(finalImportAction);

            var exportDataAction = new ExportWorkItemDataMorphAction(target.Name, Path.GetTempPath()); //TODO replace temp path with something useful
            actionSet.ProcessWorkItemData.Add(exportDataAction);

            // fields
            foreach (var enumerator in source.Fields)
            {
                var sourceField = enumerator;
                var targetField = target.Fields.SingleOrDefault(t => string.Equals(t.ReferenceName, sourceField.ReferenceName, StringComparison.OrdinalIgnoreCase));
                if (targetField == null)
                {
                    importAction.AddFieldDefinition(sourceField.Element);
                }
                else
                {
                    var isNameMatch = string.Equals(targetField.Name, sourceField.Name, StringComparison.OrdinalIgnoreCase);
                    var isTypeMatch = targetField.Type == sourceField.Type;
                    if (!isNameMatch)
                    {
                        Debug.WriteLine(string.Format("NAME CHANGE: {0}.{1} > {2}.{3}", source.Name, sourceField.Name, target.Name, targetField.Name));
                        // TODO different friendly names. witadmin changefield?
                    }
                    if (!isTypeMatch)
                    {
                        Debug.WriteLine(string.Format("TYPE CHANGE: {0}.{1}.{2} > {3}.{4}.{5}", source.Name, sourceField.ReferenceName, sourceField.Type, target.Name, targetField.ReferenceName, targetField.Type));
                        // TODO different type. witadmin changefield? data copy? fail?
                    }

                    if (isNameMatch && isTypeMatch)
                    {
                        finalImportAction.AddImportStep(i => i.ReplaceFieldDefinition(targetField.ReferenceName, sourceField.Element));
                    }
                }
            }

            var fieldMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);            
            fieldMap.Add("Microsoft.VSTS.Common.BacklogPriority", "Microsoft.VSTS.Common.StackRank");
            fieldMap.Add("Microsoft.VSTS.Scheduling.Effort", "Microsoft.VSTS.Scheduling.StoryPoints");
            //TODO consider appending Microsoft.VSTS.Common.AcceptanceCriteria to System.Description

            var systemFieldReferenceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {"System.Watermark", "System.TeamProject", "System.IterationId", "System.ExternalLinkCount", "System.HyperLinkCount", "System.AttachedFileCount", "System.NodeName", "System.RevisedDate", "System.AreaId", "System.AuthorizedAs", "System.AuthorizedDate", "System.Rev", "System.WorkItemType", "System.Description", "System.RelatedLinkCount", "System.ChangedDate", "System.ChangedBy", "System.CreatedDate", "System.CreatedBy"};

            foreach (var enumerator in target.Fields)
            {
                var targetField = enumerator;
                var sourceField = source.Fields.SingleOrDefault(s => string.Equals(s.ReferenceName, targetField.ReferenceName, StringComparison.OrdinalIgnoreCase));
                if (sourceField == null)
                {
                    if (fieldMap.ContainsKey(targetField.ReferenceName))
                    {
                        sourceField = source.Fields.SingleOrDefault(s => string.Equals(s.ReferenceName, fieldMap[targetField.ReferenceName], StringComparison.OrdinalIgnoreCase));
                    }

                    if (sourceField != null)
                    {
                        actionSet.ProcessWorkItemData.Add(new CopyWorkItemDataMorphAction(target.Name, targetField.ReferenceName, sourceField.ReferenceName));
                        finalImportAction.AddImportStep(i => i.RemoveFieldDefinition(targetField.ReferenceName));
                    }
                    else if (systemFieldReferenceNames.Contains(targetField.ReferenceName))
                    {
                        // ignore this extra system field
                    }
                    else
                    {
                        exportDataAction.AddExportField(targetField.ReferenceName);
                        finalImportAction.AddImportStep(i => i.RemoveFieldDefinition(targetField.ReferenceName));
                    }
                }
            }

            // states
            foreach (var sourceState in source.States)
            {
                var targetState = target.States.SingleOrDefault(t => string.Equals(t.Value, sourceState.Value, StringComparison.OrdinalIgnoreCase));
                if (targetState == null)
                {
                    importAction.AddWorkflowState(sourceState.Element);
                } 
            }

            var stateMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {{"Todo", "New"}, {"In Progress", "Active"}, {"Done", "Closed"}};

            foreach (var enumerator in target.States)
            {
                var targetState = enumerator;
                var sourceState = source.States.SingleOrDefault(s => string.Equals(s.Value, targetState.Value, StringComparison.OrdinalIgnoreCase));
                if (sourceState == null)
                {
                    if (stateMap.ContainsKey(targetState.Value))
                    {
                        string newStateValue = stateMap[targetState.Value];
                        const string defaultReason = "Process Template Change";
                        // add new state
                        importAction.AddWorkflowTransition(targetState.Value, newStateValue, defaultReason);

                        // change old state to new state for existing work items
                        actionSet.ProcessWorkItemData.Add(new ModifyWorkItemStateMorphAction(target.Name, targetState.Value, newStateValue));

                        // remove old state and related transitions
                        finalImportAction.AddImportStep(i => i.RemoveWorkflowState(targetState.Value)); // ReplaceWorkflow below probably makes this irrelevant
                    }
                }
            }

            finalImportAction.AddImportStep(i => i.ReplaceWorkflow(source.WorkflowElement));
            finalImportAction.AddImportStep(i => i.ReplaceForm(source.FormElement));


            if (!string.Equals(source.Name, target.Name, StringComparison.OrdinalIgnoreCase))
            {
                actionSet.FinaliseWorkItemTypeDefinitions.Add(new RenameWitdMorphAction(target.Name, source.Name));
            }

        }
    }
}
