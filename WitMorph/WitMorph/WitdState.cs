using System.Xml;

namespace WitMorph
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
    }
}