using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApp14
{
    public class UserProfile : INotifyPropertyChanged
    {
        private static UserProfile _current = new UserProfile();

        public static UserProfile Current
        {
            get => _current;
            set => _current = value;
        }

        private string _username = "Студент";
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        public int IQ { get; set; } = 100;
        public double CorrectAnswerPercentage { get; set; } = 0.0;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}