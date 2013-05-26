﻿using System;
using System.IO;
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
            _view.SelectProcessMap += SelectProcessMap;
            _view.SelectOutputFile += SelectOutputFile;
            _view.GenerateActions += GenerateActions;

            _model = new HubViewModel();
            _view.SetDataSource(_model);
        }

        private void GenerateActions(object sender, EventArgs e)
        {
            // TODO validate inputs, handle exceptions

            var factory = new ProcessTemplateFactory();

            var currentTemplate = factory.FromActiveTeamProject(new Uri(_model.CurrentCollectionUri), _model.CurrentProjectName);
            var goalTemplate = factory.FromActiveTeamProject(new Uri(_model.GoalCollectionUri), _model.GoalProjectName);

            ProcessTemplateMap map;
            using (var mapStream = File.OpenRead(_model.ProcessMapFile))
            {
                map = ProcessTemplateMap.Read(mapStream);
            }

            var diffEngine = new DiffEngine(map);
            var differences = diffEngine.CompareProcessTemplates(currentTemplate, goalTemplate);

            var engine = new MorphEngine();

            var actions = engine.GenerateActions(differences);

            var actionSerializer = new ActionSerializer();
            actionSerializer.Serialize(actions, _model.OutputActionsFile);
        }

        private void SelectOutputFile(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.AddExtension = true;
                saveDialog.AutoUpgradeEnabled = true;
                saveDialog.CheckPathExists = true;
                saveDialog.DefaultExt = ".xml";
                saveDialog.DereferenceLinks = true;
                saveDialog.Filter = "WitMorph Process Map (*.xml)|*.xml|All files (*.*)|*.*";
                saveDialog.OverwritePrompt = true;
                var result = saveDialog.ShowDialog(_view);
                if (result == DialogResult.OK)
                {
                    _model.OutputActionsFile = saveDialog.FileName;
                }
            }
        }

        void SelectProcessMap(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.CheckFileExists = true;
                openDialog.DefaultExt = ".xml";
                openDialog.DereferenceLinks = true;
                openDialog.Filter = "WitMorph Process Map (*.xml)|*.xml|All files (*.*)|*.*";
                var result = openDialog.ShowDialog(_view);
                if (result == DialogResult.OK)
                {
                    _model.ProcessMapFile = openDialog.FileName;
                }
            }
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