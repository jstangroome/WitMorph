using System;
using System.Windows.Forms;
using Microsoft.TeamFoundation.Client;

namespace WitMorph.UI
{
    class HubPresenter
    {
        private IHubView _view;
        private HubViewModel _model;

        public HubPresenter(IHubView view)
        {
            _view = view;
            _view.SelectCurrentTeamProject += SelectCurrentTeamProject;
            _view.SelectGoalTeamProject += SelectGoalTeamProject;
            
            _model = new HubViewModel();
            _view.SetDataSource(_model);
        }

        private void SelectGoalTeamProject(object sender, EventArgs e)
        {
            using (var picker = new TeamProjectPicker(TeamProjectPickerMode.SingleProject, disableCollectionChange: false))
            {
                var result = picker.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _model.GoalCollectionUri = picker.SelectedTeamProjectCollection.Uri.ToString();
                    _model.GoalProjectName = picker.SelectedProjects[0].Name;
                }

            }
        }

        void SelectCurrentTeamProject(object sender, EventArgs e)
        {
            using (var picker = new TeamProjectPicker(TeamProjectPickerMode.SingleProject, disableCollectionChange: false))
            {
                var result = picker.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _model.CurrentCollectionUri = picker.SelectedTeamProjectCollection.Uri.ToString();
                    _model.CurrentProjectName = picker.SelectedProjects[0].Name;
                }
                
            }

        }
    }
}
