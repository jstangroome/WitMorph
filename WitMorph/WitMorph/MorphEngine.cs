using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using WitMorph.Actions;
using WitMorph.Structures;

namespace WitMorph
{
    public class MorphEngine
    {
        public IEnumerable<IMorphAction> GenerateActions(Uri collectionUri, string projectName, string newProcessTemplateName)
        {
            var collection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(collectionUri);

            IEnumerable<WorkItemTypeDefinition> processTemplateWitds;
            using (var downloader = new ProcessTemplateDownloader(collection, newProcessTemplateName))
            {
                var processTemplateReader = new ProcessTemplateReader(downloader.TemplatePath);
                processTemplateWitds = processTemplateReader.WorkItemTypeDefinitions;
            }

            var processTemplateMap = new ProcessTemplateMap();
            var actionSet = new MorphActionSet();
            var witdCollectionComparer = new WitdCollectionComparer(processTemplateMap, actionSet);
            witdCollectionComparer.Compare(processTemplateWitds, GetTeamProjectWorkItemTypeDefinitions(collection, projectName));

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
