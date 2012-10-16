using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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


            var processTemplateMap = new ProcessTemplateMap();
            var witdCollectionComparer = new WitdCollectionComparer(processTemplateMap, actionSet);
            witdCollectionComparer.Compare(sourceWitds, targetWitds);

            Directory.Delete(templatePath, recursive:true);

            return actionSet.Combine();
        }

    }
}
