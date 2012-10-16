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

            var processTemplateMap = new ProcessTemplateMap();
            var witdComparer = new WorkItemTypeDefinitionComparer(processTemplateMap, actionSet);
            foreach(var pair in updateWitds)
            {
                witdComparer.Compare(pair.Source, pair.Target);
            }

            Directory.Delete(templatePath, recursive:true);

            return actionSet.Combine();
        }

    }
}
