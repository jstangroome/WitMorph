using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using WitMorph.Model;

namespace WitMorph
{
    using System.Threading;

    public class ProcessTemplateFactory
    {
        public ProcessTemplate FromCollectionTemplates(Uri collectionUri, string processTemplateName)
        {
            var collection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(collectionUri);

            var processTemplate = new ProcessTemplate();
            using (var downloader = new ProcessTemplateDownloader(collection, processTemplateName))
            {
                var processTemplateReader = new ProcessTemplateReader(downloader.TemplatePath);
                processTemplate.WorkItemTypeDefinitions = processTemplateReader.WorkItemTypeDefinitions.ToList();
            }

            return processTemplate;
        }

        public ProcessTemplate FromActiveTeamProject(Uri collectionUri, string projectName)
        {
            var collection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(collectionUri);

            var processTemplate = new ProcessTemplate
                                  {
                                      WorkItemTypeDefinitions = GetTeamProjectWorkItemTypeDefinitions(collection, projectName)
                                  };

            return processTemplate;
        }

        private IReadOnlyList<WorkItemTypeDefinition> GetTeamProjectWorkItemTypeDefinitions(TfsTeamProjectCollection collection, string projectName)
        {
            var store = collection.GetService<WorkItemStore>();
            store.RefreshCache();
            store.SyncToCache();
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
