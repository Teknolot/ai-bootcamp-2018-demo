using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace demo.Models
{
    public class UserModel : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        private ImageSource _file;
        public ImageSource File
        {
            get
            {
                return _file;
            }
            set
            {
                _file = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}