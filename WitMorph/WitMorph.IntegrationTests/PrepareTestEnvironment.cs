using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WitMorph.IntegrationTests
{
    [TestClass]
    public class PrepareTestEnvironment
    {
        class CollectionInformation
        {
            public string Name { get; set; }
            public string ConnectionString { get; set; }
            public string VirtualDir { get; set; }
        }

        [Ignore]
        [TestMethod]
        public void Reset_tfs_test_collection()
        {

            /*
             * add user to server admins
             * create new collection WitMorphTests
             * add new team project 'Scrum-2.1' with empty source folder, no SP or reports
             * add new team project 'Agile-6.1'
             * add test projects
             * detach collection and backup db
             */

            const string testServerUri = "http://localhost:8080/tfs";
            const string testCollectionName = "WitMorphTests";
            var databaseBackupFile = @"<path_to_tfs_witmorphtests.bak>";

            var server = new TfsConfigurationServer(new Uri(testServerUri));

            var collectionInfo = DetachCollectionWithInformation(server, testCollectionName)
                ?? BuildCollectionInformation(server, testCollectionName);

            RestoreDatabase(collectionInfo, databaseBackupFile);

            var attachedCollection = AttachCollection(server, collectionInfo);

            Assert.AreEqual(TeamFoundationServiceHostStatus.Started, attachedCollection.State, "Collection not started");
        }

        private static void RestoreDatabase(CollectionInformation collectionInfo, string databaseBackupFile)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(collectionInfo.ConnectionString);

            var serverConnection = new ServerConnection(connectionStringBuilder.DataSource);
            var server = new Server(serverConnection);
            var restore = new Restore();
            restore.Devices.AddDevice(databaseBackupFile, DeviceType.File);
            restore.Database = connectionStringBuilder.InitialCatalog;
            restore.ReplaceDatabase = true;

            server.KillAllProcesses(restore.Database);
            restore.SqlRestore(server);
        }

        private static CollectionInformation BuildCollectionInformation(TfsConfigurationServer server, string collectionName)
        {
            var sqlServer = GetTfsSqlServerInstance(server);

            var connectionStringBuilder = new SqlConnectionStringBuilder
                                          {
                                              DataSource = sqlServer,
                                              IntegratedSecurity = true,
                                              InitialCatalog = "Tfs_" + collectionName
                                          };
            return new CollectionInformation
                   {
                       Name = collectionName,
                       ConnectionString = connectionStringBuilder.ConnectionString,
                       VirtualDir = string.Format("~/{0}/", collectionName)
                   };
        }

        private static TeamProjectCollection AttachCollection(TfsConfigurationServer server, CollectionInformation collectionInfo)
        {
            var collectionService = server.GetService<ITeamProjectCollectionService>();
            var jobDetail = collectionService.QueueAttachCollection(collectionInfo.ConnectionString, null, false, collectionInfo.Name, null, collectionInfo.VirtualDir);
            return collectionService.WaitForCollectionServicingToComplete(jobDetail);
        }

        private static CollectionInformation DetachCollectionWithInformation(TfsConfigurationServer server, string collectionName)
        {
            var collectionService = server.GetService<ITeamProjectCollectionService>();
            var collection = collectionService.GetCollections()
                .FirstOrDefault(c => c.Name.Equals(collectionName, StringComparison.OrdinalIgnoreCase));

            if (collection == null) return null;
            
            string connectionString;
            var jobDetail = collectionService.QueueDetachCollection(collection, null, null, out connectionString);
            var detachedCollection = collectionService.WaitForCollectionServicingToComplete(jobDetail);

            return new CollectionInformation
                   {
                       Name = detachedCollection.Name,
                       ConnectionString = connectionString,
                       VirtualDir = detachedCollection.VirtualDirectory
                   };
        }

        private static string GetTfsSqlServerInstance(TfsConfigurationServer server)
        {
            var catalogService = server.GetService<ICatalogService>();

            var sqlDatabaseInstanceNode = catalogService.QueryRootNode(CatalogTree.Infrastructure)
                .QueryChildren(new[] {CatalogResourceTypes.SqlDatabaseInstance}, recurse: true, queryOptions: CatalogQueryOptions.IncludeParents)
                .FirstOrDefault();

            Assert.IsNotNull(sqlDatabaseInstanceNode, "Could not find SqlDatabaseInstance catalog node.");

            var sqlServerInstance = sqlDatabaseInstanceNode.ParentNode.Resource.Properties["MachineName"];
            var instanceName = sqlDatabaseInstanceNode.Resource.Properties["InstanceName"];
            if (!string.IsNullOrEmpty(instanceName))
            {
                sqlServerInstance += @"\" + instanceName;
            }
            return sqlServerInstance;
        }
    }
}
