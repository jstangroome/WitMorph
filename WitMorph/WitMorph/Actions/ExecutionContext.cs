using System;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WitMorph.Actions
{
    public class ExecutionContext
    {
        public ExecutionContext(Uri collectionUri, string projectName)
        {
            CollectionUri = collectionUri;
            ProjectName = projectName;
        }

        public Uri CollectionUri { get; set; }
        public string ProjectName { get; set; }

        public Project GetWorkItemProject()
        {
            var collection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(CollectionUri);
            var store = collection.GetService<WorkItemStore>();
            store.RefreshCache();
            return store.Projects[ProjectName];
        }
    }
}