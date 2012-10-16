using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WitMorph
{
    public class WorkItemTypeDefinition
    {
        private readonly XmlDocument _document;
        private readonly XmlNamespaceManager _namespaceManager;
        private readonly XmlElement _workItemTypeElement;
        private readonly Lazy<WitdField[]> _lazyFields;
        private readonly Lazy<WitdState[]> _lazyStates;

        public WorkItemTypeDefinition(XmlDocument document)
        {
            _document = document;

            _namespaceManager = new XmlNamespaceManager(_document.NameTable);
            _namespaceManager.AddNamespace("witd", "http://schemas.microsoft.com/VisualStudio/2008/workitemtracking/typedef");

            _workItemTypeElement = (XmlElement)_document.SelectSingleNode("/witd:WITD/WORKITEMTYPE", _namespaceManager);
            if (_workItemTypeElement == null)
            {
                throw new ArgumentException("Invalid definition document, missing WORKITEMTYPE element.");
            }

            _lazyFields = new Lazy<WitdField[]>(
                () => _workItemTypeElement
                          .SelectNodes("FIELDS/FIELD")
                          .Cast<XmlElement>()
                          .Select(e => new WitdField(e))
                          .ToArray()
                );

            _lazyStates = new Lazy<WitdState[]>(
                () => _workItemTypeElement
                          .SelectNodes("WORKFLOW/STATES/STATE")
                          .Cast<XmlElement>()
                          .Select(e => new WitdState(e))
                          .ToArray()
                );
        }

        public string Name { 
            get { return _workItemTypeElement.GetAttribute("name"); }
        }

        public ICollection<WitdField> Fields {
            get { return _lazyFields.Value; }
        }

        public ICollection<WitdState> States
        {
            get { return _lazyStates.Value; }
        }

        public XmlElement WITDElement
        {
            get { return (XmlElement)_workItemTypeElement.ParentNode.Clone(); }
        }

        public XmlElement WorkflowElement
        {
            get { return (XmlElement)_workItemTypeElement.SelectSingleNode("WORKFLOW").Clone(); }
        }

        public XmlElement FormElement
        {
            get { return (XmlElement)_workItemTypeElement.SelectSingleNode("FORM").Clone(); }
        }

        //public XmlElement Element
        //{
        //    get { return (XmlElement)_workItemTypeElement.Clone(); }
        //}
    }
}