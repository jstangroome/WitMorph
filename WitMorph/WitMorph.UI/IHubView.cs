using System;

namespace WitMorph.UI
{
    interface IHubView : IDataBoundView<HubViewModel>
    {
        event EventHandler SelectCurrentTeamProject;
        event EventHandler SelectGoalTeamProject;
    }
}