using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WitMorph.Model
{
    public class WitdState // invariant readonly
    {
        private readonly XmlElement _stateElement;

        public WitdState(XmlElement stateElement)
        {
            _stateElement = stateElement;
        }

        public XmlElement Element
        {
            get { return (XmlElement) _stateElement.Clone(); }
        }

        public string Value
        {
            get { return _stateElement.GetAttribute("value"); }
        }

        private ISet<WitdFieldReference> Fields
        {
            get
            {
                var fields = _stateElement.HasChildNodes
                                 ? _stateElement.FirstChild.ChildNodes.OfType<XmlElement>().Select(x => new WitdFieldReference(x))
                                 : new WitdFieldReference[] { };
                return new HashSet<WitdFieldReference>(fields);
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as WitdState;
            if (other == null) return false;

            if (other.Value != Value) return false;

            if (!other.Fields.SetEquals(Fields)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode() ^ Fields.Aggregate(0, (seed, x) => seed ^ x.GetHashCode());
        }
    }
}