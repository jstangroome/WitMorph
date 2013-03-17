using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using WitMorph.Model;

namespace WitMorph
{
    public class ProcessTemplateReader
    {
        private readonly string _processTemplatePath;
        private readonly IList<WorkItemTypeDefinition> _workItemTypeDefinitions = new List<WorkItemTypeDefinition>();

        public ProcessTemplateReader(string processTemplatePath)
        {
            _processTemplatePath = processTemplatePath;

            var xdoc = new XmlDocument();
            xdoc.Load(Path.Combine(_processTemplatePath, "ProcessTemplate.xml"));
            var witTaskNode = (XmlElement)xdoc.SelectSingleNode("ProcessTemplate/groups/group[@id='WorkItemTracking']/taskList");
            if (witTaskNode == null)
            {
                throw new ArgumentException("Process Template does not contain WorkItemTracking details.");
            }

            var workitemsdoc = new XmlDocument();
            workitemsdoc.Load(Path.Combine(_processTemplatePath, witTaskNode.GetAttribute("filename")));
            var witNodes = workitemsdoc.SelectNodes("tasks/task[@id='WITs']/taskXml/WORKITEMTYPES/WORKITEMTYPE");
            if (witNodes == null)
            {
                throw new ArgumentException("Process Template does not contain individual Work Item Type details.");
            }

            foreach (XmlElement witNode in witNodes)
            {
                var doc = new XmlDocument();
                doc.Load(Path.Combine(_processTemplatePath, witNode.GetAttribute("fileName")));
                _workItemTypeDefinitions.Add(new WorkItemTypeDefinition(doc));
            }
        }

        public IEnumerable<WorkItemTypeDefinition> WorkItemTypeDefinitions { get { return _workItemTypeDefinitions; }}
    }
}