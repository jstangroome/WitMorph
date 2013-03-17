using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WitMorph.Model
{
    class WitdFieldWhenChanged : WitdFieldNamedChildElement
    {
        public WitdFieldWhenChanged(XmlElement childElement) : base(childElement) { }

        protected string Field { get { return _childElement.GetAttribute("field"); } }

        protected ISet<IWitdFieldChildElement> ChildElements
        {
            get
            {
                return new HashSet<IWitdFieldChildElement>(
                    WitdFieldChildElementFactory.Create(_childElement.ChildNodes.OfType<XmlElement>())
                        .Where(e =>
                        {
                            var named = e as WitdFieldNamedChildElement;
                            return named == null || named.ElementName != "ALLOWEXISTINGVALUE";
                        })
                    );
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as WitdFieldWhenChanged;

            if (other == null) return false;

            if (!base.Equals(other)) return false;
            if (other.Field != Field) return false;

            if (!other.ChildElements.SetEquals(ChildElements)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode()
                   ^ Field.GetHashCode()
                   ^ ChildElements.Aggregate(0, (seed, i) => seed ^ i.GetHashCode());
        }
    }
}