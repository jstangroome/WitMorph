namespace WitMorph.Actions
{
    public class ActionLink 
    {
        public ActionLink(ExportWorkItemDataMorphAction target, ActionLinkType linkType)
        {
            Target = target;
            Type = linkType;
        }

        public ExportWorkItemDataMorphAction Target { get; private set; }
        public ActionLinkType Type { get; private set; }
    }
}