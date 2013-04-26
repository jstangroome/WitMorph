using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WitMorph.UI
{
    public class HubViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _currentCollectionUri;
        private string _currentProjectName;

        private string _goalCollectionUri;
        private string _goalProjectName;

        public string CurrentCollectionUri
        {
            get { return _currentCollectionUri; }
            set
            {
                _currentCollectionUri = value;
                OnPropertyChanged();
            }
        }

        public string CurrentProjectName
        {
            get { return _currentProjectName; }
            set
            {
                _currentProjectName = value;
                OnPropertyChanged();
            }
        }

        public string GoalCollectionUri
        {
            get { return _goalCollectionUri; }
            set
            {
                _goalCollectionUri = value;
                OnPropertyChanged();
            }
        }

        public string GoalProjectName
        {
            get { return _goalProjectName; }
            set
            {
                _goalProjectName = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}