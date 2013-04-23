using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WitMorph.Model
{
    public class WitdTransition
    {
        private class WitdReason
        {
            private readonly XmlElement _reasonElement;

            public WitdReason(XmlElement reasonElement)
            {
                _reasonElement = reasonElement;
            }

            private bool IsDefaultReason
            {
                get { return _reasonElement.Name == "DEFAULTREASON"; }
            }

            private string Value
            {
                get { return _reasonElement.GetAttribute("value"); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as WitdReason;
                if (other == null) return false;

                if (other.IsDefaultReason != IsDefaultReason) return false;
                if (other.Value != Value) return false;

                return true;
            }

            public override int GetHashCode()
            {
                return IsDefaultReason.GetHashCode() ^ Value.GetHashCode();
            }
        }

        private readonly XmlElement _transitionElement;

        public WitdTransition(XmlElement transitionElement)
        {
            _transitionElement = transitionElement;
        }

        public XmlElement Element
        {
            get { return (XmlElement)_transitionElement.Clone(); }
        }

        public string From
        {
            get { return _transitionElement.GetAttribute("from"); }
        }

        public string To
        {
            get { return _transitionElement.GetAttribute("to"); }
        }

        public string For
        {
            get { return _transitionElement.HasAttribute("for") ? _transitionElement.GetAttribute("for") : string.Empty; }
        }

        public string Not
        {
            get { return _transitionElement.HasAttribute("not") ? _transitionElement.GetAttribute("not") : string.Empty; }
        }

        private ISet<WitdReason> Reasons
        {
            get
            {
                return new HashSet<WitdReason>(
                    _transitionElement.SelectNodes("REASONS/*").OfType<XmlElement>().Select(x => new WitdReason(x))
                    );
            }
        }

        private ISet<WitdFieldReference> Fields
        {
            get
            {
                return new HashSet<WitdFieldReference>(
                    _transitionElement.SelectNodes("FIELDS/*").OfType<XmlElement>().Select(x => new WitdFieldReference(x))
                    );
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as WitdTransition;
            if (other == null) return false;

            if (other.From != From) return false;
            if (other.To != To) return false;
            if (other.For != For) return false;
            if (other.Not != Not) return false;

            if (!other.Reasons.SetEquals(Reasons)) return false;
            if (!other.Fields.SetEquals(Fields)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return From.GetHashCode()
                   ^ To.GetHashCode()
                   ^ For.GetHashCode()
                   ^ Not.GetHashCode()
                   ^ Reasons.Aggregate(0, (seed, x) => seed ^ x.GetHashCode())
                   ^ Fields.Aggregate(0, (seed, x) => seed ^ x.GetHashCode());
        }

    }
}