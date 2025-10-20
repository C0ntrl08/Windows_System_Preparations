using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace LanguagePacksCheckerAndSetter.Models
{
    public class LogEntry : INotifyPropertyChanged
    {
        private string _message = string.Empty;
        private Brush _foreground = Brushes.Black;

        public string Message
        {
            get => _message;
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged();
                }
            }
        }

        public Brush Foreground
        {
            get => _foreground;
            set
            {
                if (_foreground != value)
                {
                    _foreground = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
