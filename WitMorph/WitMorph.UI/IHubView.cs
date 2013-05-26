using System;
using System.Windows.Forms;

namespace WitMorph.UI
{
    interface IHubView : IDataBoundView<HubViewModel>, IWin32Window
    {
        event EventHandler SelectCurrentTeamProject;
        event EventHandler SelectGoalTeamProject;
        event EventHandler SelectProcessMap;
        event EventHandler SelectOutputFile;
        event EventHandler GenerateActions;
    }
}