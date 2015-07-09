using System;
using System.Diagnostics;
using System.IO;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WitMorph.Actions
{
    using System.Threading;

    public class ExecutionContext
    {
        public ExecutionContext(Uri collectionUri, string projectName, string outputPath)
        {
            CollectionUri = collectionUri;
            ProjectName = projectName;
            OutputPath = outputPath;
            TraceLevel = TraceLevel.Warning;
        }

        public Uri CollectionUri { get; set; }
        public string ProjectName { get; set; }
        public string OutputPath { get; set; }
        public TraceLevel TraceLevel { get; set; }

        public Project GetWorkItemProject()
        {
            var collection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(CollectionUri);
            var store = collection.GetService<WorkItemStore>();

            Thread.Sleep(5000);
            store.RefreshCache();
            store.SyncToCache();
            return store.Projects[ProjectName];
        }

        public void Log(string message, TraceLevel traceLevel)
        {
            if (traceLevel > TraceLevel) return;
            var path = Path.Combine(OutputPath, ProjectName + ".log");
            using (var w = new StreamWriter(path, append: true))
            {
                w.WriteLine(message);
            }
        }
    }
}