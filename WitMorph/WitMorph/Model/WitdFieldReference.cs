using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WitMorph.Model
{
    public class WitdFieldReference // invariant readonly
    {
        private readonly XmlElement _fieldElement;

        public WitdFieldReference(XmlElement fieldElement)
        {
            _fieldElement = fieldElement;
        }
        
        public string ReferenceName
        {
            get { return _fieldElement.GetAttribute("refname"); }
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
            var other = obj as WitdFieldReference;
            if (other == null) return false;
            
            if (other.ReferenceName != ReferenceName) return false;

            if (!other.AllChildElements.SetEquals(AllChildElements)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return ReferenceName.GetHashCode()
                ^ AllChildElements.Aggregate(0, (seed, i) => seed ^ i.GetHashCode());
        }

    }
}