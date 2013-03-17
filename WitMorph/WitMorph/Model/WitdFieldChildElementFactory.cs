using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WitMorph.Model
{
    public static class WitdFieldChildElementFactory
    {
        private static readonly Dictionary<string, Func<XmlElement, IWitdFieldChildElement>> Factories = new Dictionary<string, Func<XmlElement, IWitdFieldChildElement>>
                                                                                                         {
                                                                                                             {"WHEN", e => new WitdFieldWhenValue(e)},
                                                                                                             {"WHENNOT", e => new WitdFieldWhenValue(e)},
                                                                                                             {"WHENCHANGED", e => new WitdFieldWhenChanged(e)},
                                                                                                             {"WHENNOTCHANGED", e => new WitdFieldWhenChanged(e)},
                                                                                                             {"ALLOWEDVALUES", e => new WitdFieldPickList(e)},
                                                                                                             {"PROHIBITEDVALUES", e => new WitdFieldPickList(e)},
                                                                                                             {"SUGGESTEDVALUES", e => new WitdFieldPickList(e)},
                                                                                                             {"ALLOWEXISTINGVALUE", e => new WitdFieldNamedChildElement(e)},
                                                                                                         };

        public static IWitdFieldChildElement Create(XmlElement childElement)
        {
            return Factories.ContainsKey(childElement.Name)
                       ? Factories[childElement.Name](childElement)
                       : new WitdFieldUnrecognisedChildElement(childElement);
        }

        public static IEnumerable<IWitdFieldChildElement> Create(IEnumerable<XmlElement> childElements)
        {
            return childElements.Select(Create);
        }
    }
}