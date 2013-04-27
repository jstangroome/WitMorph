using System;
using System.Windows.Forms;

namespace WitMorph.UI
{
    public partial class HubForm : Form, IHubView
    {
        public HubForm()
        {
            InitializeComponent();
        }

        public void SetDataSource(HubViewModel model)
        {
            modelBindingSource.DataSource = model;
        }

        private void SelectCurrentTeamProject_Click(object sender, EventArgs e)
        {
            var handler = SelectCurrentTeamProject;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler SelectCurrentTeamProject;
        public event EventHandler SelectGoalTeamProject;
        public event EventHandler SelectProcessMap;
        public event EventHandler SelectOutputFile;

        private void SelectGoalTeamProject_Click(object sender, EventArgs e)
        {
            var handler = SelectGoalTeamProject;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void SelectProcessMap_Click(object sender, EventArgs e)
        {
            var handler = SelectProcessMap;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void SelectOutputFile_Click(object sender, EventArgs e)
        {
            var handler = SelectOutputFile;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
