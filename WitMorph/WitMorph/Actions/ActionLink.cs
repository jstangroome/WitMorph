namespace WitMorph.Actions
{
    public class ActionLink 
    {
        public ActionLink(ILinkableAction target, ActionLinkType linkType)
        {
            Target = target;
            Type = linkType;
        }

        public ILinkableAction Target { get; private set; }
        public ActionLinkType Type { get; private set; }
    }
}