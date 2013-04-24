using System.Xml;

namespace WitMorph.Actions
{
    public interface IMorphAction
    {
        void Execute(ExecutionContext context);
        void Serialize(XmlWriter writer);
    }

}