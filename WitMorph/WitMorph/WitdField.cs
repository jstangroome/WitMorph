using System;
using System.Xml;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WitMorph
{
    public class WitdField
    {
        private readonly XmlElement _fieldElement;

        public WitdField(XmlElement fieldElement)
        {
            _fieldElement = fieldElement;
        }

        public XmlElement Element
        {
            get { return (XmlElement)_fieldElement.Clone(); }
        }

        public string Name
        {
            get { return _fieldElement.GetAttribute("name"); }
        }

        public string ReferenceName
        {
            get { return _fieldElement.GetAttribute("refname"); }
        }

        public FieldType Type
        {
            get {
                return (FieldType)Enum.Parse(typeof(FieldType), _fieldElement.GetAttribute("type"), ignoreCase:true);
            }
        }

        public ReportingType Reportable
        {
            get
            {
                return (ReportingType)Enum.Parse(typeof(ReportingType), _fieldElement.GetAttribute("reportable"), ignoreCase: true);
            }
        }

        public string HelpText
        {
            get { 
                var helpTextElement = (XmlElement) _fieldElement.SelectSingleNode("HELPTEXT");
                return helpTextElement == null ? string.Empty : helpTextElement.InnerText;
            }
        }

    }
}