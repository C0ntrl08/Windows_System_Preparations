using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LanguagePacksCheckerAndSetter.Models
{
    public class LanguagePackModel : INotifyPropertyChanged
    {
        #region Fields
        private string _packageIdentity = string.Empty;
        private string _filePath = string.Empty;
        private bool _isSelected = false;
        private string _languageCode = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        #region Properties
        public string PackageIdentity
        {
            get { return _packageIdentity; }
            set
            {
                if (_packageIdentity != value)
                {
                    _packageIdentity = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged();
                }

            }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }
        public string LanguageCode
        {
            get { return _languageCode; }
            set
            {
                if (_languageCode != value)
                {
                    _languageCode = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Functions
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion


    }
}
