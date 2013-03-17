using System.Diagnostics;
using System.Xml;

namespace WitMorph.Model
{
    [DebuggerDisplay("{_childElement.OuterXml}")]
    internal class WitdFieldUnrecognisedChildElement : WitdFieldNamedChildElement
    {
        public WitdFieldUnrecognisedChildElement(XmlElement childElement) : base(childElement) {}

        public override bool Equals(object obj)
        {
            var other = obj as WitdFieldUnrecognisedChildElement;
            if (other == null) return false;

            return other._childElement.OuterXml == _childElement.OuterXml;
        }

        public override int GetHashCode()
        {
            return _childElement.OuterXml.GetHashCode();
        }
    }
}