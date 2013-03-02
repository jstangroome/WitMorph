using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using WitMorph.Actions;
using WitMorph.Differences;
using WitMorph.Structures;

namespace WitMorph
{
    public class MorphEngine
    {
        public IEnumerable<IMorphAction> GenerateActions(Uri collectionUri, string projectName, string newProcessTemplateName, ProcessTemplateMap processTemplateMap)
        {
            var collection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(collectionUri);

            IEnumerable<WorkItemTypeDefinition> processTemplateWitds;
            using (var downloader = new ProcessTemplateDownloader(collection, newProcessTemplateName))
            {
                var processTemplateReader = new ProcessTemplateReader(downloader.TemplatePath);
                processTemplateWitds = processTemplateReader.WorkItemTypeDefinitions;
            }

            var actionSet = new MorphActionSet();
            var witdCollectionComparer = new WitdCollectionComparer(processTemplateMap, actionSet);
            witdCollectionComparer.Compare(processTemplateWitds, GetTeamProjectWorkItemTypeDefinitions(collection, projectName));

            return actionSet.Combine();
        }

        public IEnumerable<IMorphAction> GenerateActions(IEnumerable<IDifference> differences)
        {
            var actionSet = new MorphActionSet();

            // TODO group differences by work item type?

            foreach (var witdRename in differences.OfType<RenamedWorkItemTypeDefinitionDifference>())
            {
                actionSet.FinaliseWorkItemTypeDefinitions.Add(new RenameWitdMorphAction(witdRename.CurrentTypeName, witdRename.GoalTypeName));
            }

            foreach (var witdRemove in differences.OfType<RemovedWorkItemTypeDefinitionDifference>())
            {
                actionSet.ProcessWorkItemData.Add(new ExportWorkItemDataMorphAction(witdRemove.TypeName, allFields: true));
                actionSet.FinaliseWorkItemTypeDefinitions.Add(new DestroyWitdMorphAction(witdRemove.TypeName));
            }

            var removedFieldGroups = differences
                .OfType<RemovedWorkItemFieldDifference>()
                .GroupBy(d => d.CurrentWorkItemTypeName, StringComparer.OrdinalIgnoreCase);

            foreach (var group in removedFieldGroups)
            {
                var exportDataAction = new ExportWorkItemDataMorphAction(group.Key);
                var finalModifyTypeAction = new ModifyWorkItemTypeDefinitionMorphAction(group.Key);

                foreach (var fieldRemove in group)
                {
                    exportDataAction.AddExportField(fieldRemove.ReferenceFieldName);
                    finalModifyTypeAction.RemoveFieldDefinition(fieldRemove.ReferenceFieldName);
                }

                actionSet.ProcessWorkItemData.Add(exportDataAction);
                actionSet.FinaliseWorkItemTypeDefinitions.Add(finalModifyTypeAction);
            }

            return actionSet.Combine();
        }

        public void Apply(Uri collectionUri, string projectName, IEnumerable<IMorphAction> actions, string outputPath)
        {
            var context = new ExecutionContext(collectionUri, projectName, outputPath);
            foreach (var action in actions)
            {
                action.Execute(context);
            }
        }

        private IEnumerable<WorkItemTypeDefinition> GetTeamProjectWorkItemTypeDefinitions(TfsTeamProjectCollection collection, string projectName)
        {
            var store = collection.GetService<WorkItemStore>();
            var project = store.Projects[projectName];

            var witds = new List<WorkItemTypeDefinition>();
            foreach (WorkItemType wit in project.WorkItemTypes)
            {
                witds.Add(new WorkItemTypeDefinition(wit.Export(false)));
            }
            return witds;
        }
    }
}
