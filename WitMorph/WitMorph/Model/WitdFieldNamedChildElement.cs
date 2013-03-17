using System.Xml;

namespace WitMorph.Model
{
    class WitdFieldNamedChildElement : IWitdFieldChildElement
    {
        protected readonly XmlElement _childElement;

        public WitdFieldNamedChildElement(XmlElement childElement)
        {
            _childElement = childElement;
        }

        public string ElementName { get { return _childElement.Name; } }

        public override bool Equals(object obj)
        {
            var other = obj as WitdFieldNamedChildElement;
            if (other == null) return false;

            if (other.ElementName != ElementName) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return ElementName.GetHashCode();
        }
    }
}