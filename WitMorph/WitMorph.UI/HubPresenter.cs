using System;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
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
            _view.SelectOutputActionsFile += SelectOutputActionsFile;
            _view.GenerateActions += GenerateActions;
            _view.SelectInputActionsFile += SelectInputActionsFile;
            _view.ApplyActions += ApplyActions;

            _model = new HubViewModel {Ready = true};
            _view.SetDataSource(_model);
        }

        private void ApplyActions(object sender, EventArgs e)
        {
            var actionSerializer = new ActionSerializer();
            var actions = actionSerializer.Deserialize(_model.InputActionsFile);
            
            var engine = new MorphEngine();
            engine.Apply(new Uri(_model.CurrentCollectionUri), _model.CurrentProjectName, actions, Path.GetTempPath()); 
        }

        private void SelectInputActionsFile(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.CheckFileExists = true;
                openDialog.DefaultExt = ".witact";
                openDialog.DereferenceLinks = true;
                openDialog.Filter = "WitMorph Actions (*.witact)|*.witact|All files (*.*)|*.*";
                var result = openDialog.ShowDialog(_view);
                if (result == DialogResult.OK)
                {
                    _model.InputActionsFile = openDialog.FileName;
                }
            }
        }

        private void GenerateActions(object sender, EventArgs e)
        {
            using (new DisposableAction(() => _model.Ready = true))
            {
                _model.Ready = false;

                // TODO handle exceptions

                var factory = new ProcessTemplateFactory();

                var currentTemplate = GetProcessTemplate(factory, m => m.CurrentCollectionUri, m => m.CurrentProjectName);
                var goalTemplate = GetProcessTemplate(factory, m => m.GoalCollectionUri, m => m.GoalProjectName);

                var map = GetProcessTemplateMap();

                var outputActionsFile = ValidateWritableFile(m => m.OutputActionsFile);

                if (currentTemplate == null || goalTemplate == null || map == null || outputActionsFile == null) return;

                try
                {
                    var diffEngine = new DiffEngine(map);
                    var differences = diffEngine.CompareProcessTemplates(currentTemplate, goalTemplate);

                    var engine = new MorphEngine();

                    var actions = engine.GenerateActions(differences);

                    var actionSerializer = new ActionSerializer();
                    actionSerializer.Serialize(actions, outputActionsFile);

                    _model.ResultMessage = "Successfully generated actions.";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    _model.ResultMessage = string.Format("Error generating actions: {0}", ex.Message);
                }
            }

        }

        private string ValidateWritableFile(Expression<Func<HubViewModel, string>> filePathLamba)
        {
            _model.ClearError(filePathLamba);

            var filePathRawValue = filePathLamba.Compile()(_model);

            if (string.IsNullOrWhiteSpace(filePathRawValue))
            {
                _model.SetError(filePathLamba, "Required.");
                return null;
            }

            try
            {
                var exists = File.Exists(filePathRawValue);
                using (var stream = File.OpenWrite(filePathRawValue))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
                if (!exists) File.Delete(filePathRawValue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                _model.SetError(filePathLamba, ex.Message);
                return null;
            }
            return filePathRawValue;
        }

        private ProcessTemplateMap GetProcessTemplateMap()
        {
            _model.ClearError(m => m.ProcessMapFile);

            if (string.IsNullOrWhiteSpace(_model.ProcessMapFile))
            {
                _model.SetError(m => m.ProcessMapFile, "Required.");
                return null;
            }

            ProcessTemplateMap map;
            try
            {
                if (!File.Exists(_model.ProcessMapFile))
                {
                    _model.SetError(m => m.ProcessMapFile, "File not found.");
                    return null;
                }

                using (var mapStream = File.OpenRead(_model.ProcessMapFile))
                {
                    map = ProcessTemplateMap.Read(mapStream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                _model.SetError(m => m.ProcessMapFile, ex.Message);
                return null;
            }

            return map;
        }

        private ProcessTemplate GetProcessTemplate(ProcessTemplateFactory factory, Expression<Func<HubViewModel, string>> collectionUriLamba, Expression<Func<HubViewModel, string>> projectNameLambda)
        {
            var hasError = false;
            Uri collectionUri = null;

            _model.ClearError(collectionUriLamba);
            _model.ClearError(projectNameLambda);

            var collectionUriRawValue = collectionUriLamba.Compile()(_model);
            var projectNameRawValue = projectNameLambda.Compile()(_model);

            if (string.IsNullOrWhiteSpace(collectionUriRawValue))
            {
                _model.SetError(collectionUriLamba, "Required.");
                hasError = true;
            }
            else
            {
                try
                {
                    collectionUri = new Uri(collectionUriRawValue);
                }
                catch (UriFormatException ex)
                {
                    _model.SetError(collectionUriLamba, ex.Message);
                    hasError = true;
                }
            }

            if (string.IsNullOrWhiteSpace(projectNameRawValue))
            {
                _model.SetError(projectNameLambda ,"Required.");
                hasError = true;
            }

            if (hasError) return null;

            try
            {
                return factory.FromActiveTeamProject(collectionUri, projectNameRawValue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                _model.SetError(collectionUriLamba, ex.Message); // or set error on project name or both or based on exception?
                return null;
            }
        }

        private void SelectOutputActionsFile(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.AddExtension = true;
                saveDialog.AutoUpgradeEnabled = true;
                saveDialog.CheckPathExists = true;
                saveDialog.DefaultExt = ".witact";
                saveDialog.DereferenceLinks = true;
                saveDialog.Filter = "WitMorph Actions (*.witact)|*.witact|All files (*.*)|*.*";
                saveDialog.OverwritePrompt = true;
                var result = saveDialog.ShowDialog(_view);
                if (result == DialogResult.OK)
                {
                    _model.OutputActionsFile = saveDialog.FileName;
                    _model.ClearError(m => m.OutputActionsFile);
                }
            }
        }

        void SelectProcessMap(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.CheckFileExists = true;
                openDialog.DefaultExt = ".witmap";
                openDialog.DereferenceLinks = true;
                openDialog.Filter = "WitMorph Process Map (*.witmap)|*.witmap|All files (*.*)|*.*";
                var result = openDialog.ShowDialog(_view);
                if (result == DialogResult.OK)
                {
                    _model.ProcessMapFile = openDialog.FileName;
                    _model.ClearError(m => m.ProcessMapFile);
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
                    _model.ClearError(m => m.GoalCollectionUri);
                    _model.ClearError(m => m.GoalProjectName);
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
                    _model.ClearError(m => m.CurrentCollectionUri);
                    _model.ClearError(m => m.CurrentProjectName);
                }
            }
        }

    }
}
