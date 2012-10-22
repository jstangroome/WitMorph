using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WitMorph
{
    public class WorkItemTypeDefinition
    {
        private readonly XmlElement _witdElement;
        private readonly Lazy<WitdField[]> _lazyFields;
        private readonly Lazy<WitdState[]> _lazyStates;

        public WorkItemTypeDefinition(XmlDocument document) : this(document.DocumentElement) {}

        public WorkItemTypeDefinition(XmlElement witdElement)
        {
            if (witdElement.SelectSingleNode("WORKITEMTYPE") == null)
            {
                throw new ArgumentException("Invalid definition document, missing WORKITEMTYPE element.");
            }

            _witdElement = (XmlElement)witdElement.Clone();

            _lazyFields = new Lazy<WitdField[]>(
                () => _witdElement
                          .SelectNodes("WORKITEMTYPE/FIELDS/FIELD")
                          .Cast<XmlElement>()
                          .Select(e => new WitdField(e))
                          .ToArray()
                );

            _lazyStates = new Lazy<WitdState[]>(
                () => _witdElement
                          .SelectNodes("WORKITEMTYPE/WORKFLOW/STATES/STATE")
                          .Cast<XmlElement>()
                          .Select(e => new WitdState(e))
                          .ToArray()
                );
        }

        public string Name
        {
            get { return ((XmlElement)_witdElement.SelectSingleNode("WORKITEMTYPE")).GetAttribute("name"); }
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
            get { return (XmlElement)_witdElement.Clone(); }
        }

        public XmlElement WorkflowElement
        {
            get { return (XmlElement)_witdElement.SelectSingleNode("WORKITEMTYPE/WORKFLOW").Clone(); }
        }

        public XmlElement FormElement
        {
            get { return (XmlElement)_witdElement.SelectSingleNode("WORKITEMTYPE/FORM").Clone(); }
        }
    }
}