using System.Xml;

namespace WitMorph.Model
{
    class WitdFieldWhenValue : WitdFieldWhenChanged
    {
        public WitdFieldWhenValue(XmlElement childElement) : base(childElement) { }

        private string Value { get { return _childElement.GetAttribute("value"); } }

        public override bool Equals(object obj)
        {
            var other = obj as WitdFieldWhenValue;
            if (other == null) return false;

            if (other.Value != Value) return false;

            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode() ^ base.GetHashCode();
        }


    }
}