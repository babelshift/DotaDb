using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DotaDb.Data
{
    public class AppStateService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string headerImageName;
        private string headerText;

        public string HeaderImageName
        {
            get => headerImageName;
            set
            {
                headerImageName = value;
                OnPropertyChanged();
            }
        }

        public string HeaderText
        {
            get => headerText;
            set
            {
                headerText = value;
                OnPropertyChanged();
            }
        }
    }
}