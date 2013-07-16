using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace WitMorph.UI
{
    public class HubViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _currentCollectionUri;
        private string _currentProjectName;

        private string _goalCollectionUri;
        private string _goalProjectName;
        private string _processMapFile;
        private string _outputActionsFile;
        private string _inputActionsFile;
        private string _outputPath;

        private bool _ready;
        private string _resultMessage;

        private IDictionary<string, string> _errorMessages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string CurrentCollectionUri
        {
            get { return _currentCollectionUri; }
            set { ChangeProperty(ref _currentCollectionUri, value); }
        }

        public string CurrentProjectName
        {
            get { return _currentProjectName; }
            set { ChangeProperty(ref _currentProjectName, value); }
        }

        public string GoalCollectionUri
        {
            get { return _goalCollectionUri; }
            set { ChangeProperty(ref _goalCollectionUri, value); }
        }

        public string GoalProjectName
        {
            get { return _goalProjectName; }
            set { ChangeProperty(ref _goalProjectName, value); }
        }

        public string ProcessMapFile
        {
            get { return _processMapFile; }
            set { ChangeProperty(ref _processMapFile, value); }
        }

        public string OutputActionsFile
        {
            get { return _outputActionsFile; }
            set { ChangeProperty(ref _outputActionsFile, value); }
        }

        public string InputActionsFile
        {
            get { return _inputActionsFile; }
            set { ChangeProperty(ref _inputActionsFile, value); }
        }

        public string OutputPath
        {
            get { return _outputPath; }
            set { ChangeProperty(ref _outputPath, value); }
        }

        public bool Ready
        {
            get { return _ready; }
            set { ChangeProperty(ref _ready, value); }
        }

        public string ResultMessage
        {
            get { return _resultMessage; }
            set { ChangeProperty(ref _resultMessage, value); }
        }

        protected void ChangeProperty<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (Comparer<T>.Default.Compare(backingField, newValue) == 0) return;
            backingField = newValue;
            OnPropertyChanged(propertyName);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private string GetPropertyName<T>(Expression<Func<HubViewModel, T>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null) return string.Empty;
            return memberExpression.Member.Name;
        }

        public void SetError<T>(Expression<Func<HubViewModel, T>> propertyLambda, string errorMessage)
        {
            SetError(GetPropertyName(propertyLambda), errorMessage);
        }

        private void SetError(string propertyName, string errorMessage)
        {
            _errorMessages[propertyName] = errorMessage;
            OnPropertyChanged(propertyName);
        }

        public void ClearError<T>(Expression<Func<HubViewModel, T>> propertyLambda)
        {
            ClearError(GetPropertyName(propertyLambda));
        }

        private void ClearError(string propertyName)
        {
            _errorMessages[propertyName] = string.Empty;
            OnPropertyChanged(propertyName);
        }

        string IDataErrorInfo.Error
        {
            get { return ((IDataErrorInfo)this)[null]; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (_errorMessages.ContainsKey(columnName)) return _errorMessages[columnName];
                return string.Empty;
            }
        }
    }
}