using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WitMorph.Model
{
    public class WitdField // invariant readonly
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
                if (!_fieldElement.HasAttribute("reportable")) return ReportingType.None;
                var reportable = _fieldElement.GetAttribute("reportable");
                if (reportable == "detail") return ReportingType.DrillDownField;
                return (ReportingType)Enum.Parse(typeof(ReportingType), reportable, ignoreCase: true);
            }
        }

        private bool SyncNameChanges
        {
            get
            {
                if (Type != FieldType.String) return false;
                if (!_fieldElement.HasAttribute("syncnamechanges")) return false;
                return Convert.ToBoolean(_fieldElement.GetAttribute("syncnamechanges"));
            }
        }

        private string Formula
        {
            get
            {
                if (!_fieldElement.HasAttribute("formula")) return string.Empty;
                return _fieldElement.GetAttribute("formula");
            }
        }

        private string ReportingName
        {
            get
            {
                if (!_fieldElement.HasAttribute("reportingname")) return Name;
                return _fieldElement.GetAttribute("reportingname");
            }
        }

        private string ReportingRefName
        {
            get
            {
                if (!_fieldElement.HasAttribute("reportingrefname")) return ReferenceName;
                return _fieldElement.GetAttribute("reportingrefname");
            }
        }

        public string HelpText // never used, consider removing
        {
            get { 
                var helpTextElement = (XmlElement) _fieldElement.SelectSingleNode("HELPTEXT");
                return helpTextElement == null ? string.Empty : helpTextElement.InnerText;
            }
        }

        private ISet<IWitdFieldChildElement> AllChildElements
        {
            get
            {
                var set = new HashSet<IWitdFieldChildElement>(
                    WitdFieldChildElementFactory.Create(_fieldElement.ChildNodes.OfType<XmlElement>())
                    );
                return set;
            }
        }
        
        public override bool Equals(object obj)
        {
            var other = obj as WitdField;
            if (other == null) return false;
            
            // attributes
            if (other.ReferenceName != ReferenceName) return false;
            if (other.Name != Name) return false;
            if (other.Type != Type) return false;
            if (other.SyncNameChanges != SyncNameChanges) return false;
            if (other.Reportable != Reportable) return false;

            if (Reportable == ReportingType.Measure)
            {
                if (other.Formula != Formula) return false;
            }

            if (Reportable != ReportingType.None)
            {
                if (other.ReportingRefName != ReportingRefName) return false;
                if (other.ReportingName != ReportingName) return false;
            }
            
            // child elements

            if (!other.AllChildElements.SetEquals(AllChildElements)) return false;

            return true;
        }

    }
}