using System.Xml;

namespace WitMorph
{
    public static class XmlElementExtensions {

        public static string GetAttributeWithDefault(this XmlElement element, string attributeName, string defaultValue)
        {
            return !element.HasAttribute(attributeName) ? defaultValue : element.GetAttribute(attributeName);
        }
    }
}