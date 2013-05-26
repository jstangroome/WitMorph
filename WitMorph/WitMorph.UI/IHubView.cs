using System;
using System.Windows.Forms;

namespace WitMorph.UI
{
    interface IHubView : IDataBoundView<HubViewModel>, IWin32Window
    {
        event EventHandler SelectCurrentTeamProject;
        event EventHandler SelectGoalTeamProject;
        event EventHandler SelectProcessMap;
        event EventHandler SelectOutputActionsFile;
        event EventHandler GenerateActions;
        event EventHandler ApplyActions;
        event EventHandler SelectInputActionsFile;
    }
}