using System.Collections.Generic;

namespace WitMorph.Actions
{
    public interface ILinkableAction
    {
        ICollection<ActionLink> LinkedActions { get; }
    }
}