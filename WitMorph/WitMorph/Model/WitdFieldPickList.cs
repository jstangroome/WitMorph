using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WitMorph.Model
{
    public class WitdFieldPickList : IWitdFieldChildElement
    {
        private readonly XmlElement _pickListElement;

        public WitdFieldPickList(XmlElement pickListElement)
        {
            _pickListElement = pickListElement;
        }

        public string ElementName
        {
            get { return _pickListElement.Name; }
        }

        public string For
        {
            get { return _pickListElement.GetAttributeWithDefault("for", string.Empty); }
        }

        public string Not
        {
            get { return _pickListElement.GetAttributeWithDefault("not", string.Empty); }
        }

        public bool ExpandItems
        {
            get { return Convert.ToBoolean(_pickListElement.GetAttributeWithDefault("expanditems", "true")); }
        }

        public string FilterItems
        {
            get { return _pickListElement.GetAttributeWithDefault("filteritems", string.Empty); }
        }

        public ISet<string> ListItems
        {
            get
            {
                var set = new HashSet<string>();
                foreach (var itemNode in _pickListElement.SelectNodes("LISTITEM").Cast<XmlElement>())
                {
                    set.Add(itemNode.GetAttribute("value"));
                }
                return set;
            }
        }

        public string GlobalListName
        {
            get { 
                var globalListNode = (XmlElement)_pickListElement.SelectSingleNode("GLOBALLIST");
                if (globalListNode == null) return string.Empty;
                return globalListNode.GetAttribute("name");
            }
        }

        public ISet<string> GlobalListItems
        {
            get { 
                var set = new HashSet<string>();
                var globalListNode = (XmlElement)_pickListElement.SelectSingleNode("GLOBALLIST");
                if (globalListNode == null) return set;
                foreach (var itemNode in globalListNode.SelectNodes("LISTITEM").Cast<XmlElement>())
                {
                    set.Add(itemNode.GetAttribute("value"));
                }
                return set;
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as WitdFieldPickList;
            if (other == null) return false;

            if (other._pickListElement == null && _pickListElement == null) return true;

            if (other.ElementName != ElementName) return false;
            if (other.For != For) return false;
            if (other.Not != Not) return false;
            if (other.ExpandItems != ExpandItems) return false;
            if (other.FilterItems != FilterItems) return false;

            if (!other.ListItems.SetEquals(ListItems)) return false;

            if (other.GlobalListName != GlobalListName) return false;
            if (!other.GlobalListItems.SetEquals(GlobalListItems)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            if (_pickListElement == null) return 0;

            return ElementName.GetHashCode()
                   ^ For.GetHashCode()
                   ^ Not.GetHashCode()
                   ^ ExpandItems.GetHashCode()
                   ^ FilterItems.GetHashCode()
                   ^ ListItems.Aggregate(0, (seed, i) => seed ^ i.GetHashCode())
                   ^ GlobalListName.GetHashCode()
                   ^ GlobalListItems.Aggregate(0, (seed, i) => seed ^ i.GetHashCode());
        }

    }
}