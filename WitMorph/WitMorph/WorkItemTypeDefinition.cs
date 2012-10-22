using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WitMorph
{
    public class WorkItemTypeDefinition
    {
        private readonly XmlElement _witdElement;
        private readonly bool _isWritable;
        private readonly WitdField[] _fields;
        private readonly WitdState[] _states;

        public WorkItemTypeDefinition(XmlDocument document) : this(document.DocumentElement, false) {}

        public WorkItemTypeDefinition(XmlElement witdElement, bool isWritable)
        {
            if (witdElement.SelectSingleNode("WORKITEMTYPE") == null)
            {
                throw new ArgumentException("Invalid definition document, missing WORKITEMTYPE element.");
            }

            _witdElement = (XmlElement)witdElement.Clone();
            _isWritable = isWritable;

            if (!_isWritable)
            {
                _fields = _witdElement
                    .SelectNodes("WORKITEMTYPE/FIELDS/FIELD")
                    .Cast<XmlElement>()
                    .Select(e => new WitdField(e))
                    .ToArray();

                _states = _witdElement
                    .SelectNodes("WORKITEMTYPE/WORKFLOW/STATES/STATE")
                    .Cast<XmlElement>()
                    .Select(e => new WitdState(e))
                    .ToArray();
            }
        }

        public string Name
        {
            get { return ((XmlElement)_witdElement.SelectSingleNode("WORKITEMTYPE")).GetAttribute("name"); }
        }

        public ICollection<WitdField> Fields {
            get { return _fields; }
        }

        public ICollection<WitdState> States
        {
            get { return _states; }
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