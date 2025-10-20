using LanguagePacksCheckerAndSetter.Models;
using LanguagePacksCheckerAndSetter.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace LanguagePacksCheckerAndSetter.ViewModels
{
    public class CheckViewModel : INotifyPropertyChanged
    {
        private string _selectedLanguageCode = string.Empty;
        private string _operationStatus = string.Empty;
        private bool _isBusy;

        //    public ObservableCollection<string> OperationLog { get; private set; }
        //= new ObservableCollection<string>();
        private ObservableCollection<LogEntry> _operationLog = new ObservableCollection<LogEntry>();

        public ObservableCollection<LogEntry> OperationLog
        {
            get
            {
                return _operationLog;
            }
            set
            {
                if (_operationLog != value)
                {
                    _operationLog = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> SupportedLanguages { get; private set; }
    = new ObservableCollection<string>
{
    "en-US", "de-DE", "fr-FR", "it-IT", "ja-JP", "sv-SE", "zh-CN"
};

        public ObservableCollection<LanguagePackModel> InstalledPackages { get; private set; }
            = new ObservableCollection<LanguagePackModel>();


        public string SelectedLanguageCode
        {
            get { return _selectedLanguageCode; }
            set
            {
                if (_selectedLanguageCode != value)
                {
                    _selectedLanguageCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public string OperationStatus
        {
            get { return _operationStatus; }
            set
            {
                if (_operationStatus != value)
                {
                    _operationStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand CheckCommand { get; }

        // Constructors
        public CheckViewModel()
        {
            SupportedLanguages = new ObservableCollection<string>
            {
                "en-US", "de-DE", "fr-FR", "it-IT", "ja-JP", "sv-SE", "zh-CN"
            };

            InstalledPackages = new ObservableCollection<LanguagePackModel>();
            CheckCommand = new RelayCommand(async _ => await ExecuteCheckAsync(), _ => !IsBusy);
            
        }

        private async Task ExecuteCheckAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedLanguageCode))
            {
                OperationLog.Add(new LogEntry
                {
                    Message = "Please select or enter a language code.",
                    Foreground = Brushes.OrangeRed
                });
                return;
            }

            IsBusy = true;
            OperationLog.Add(new LogEntry
            {
                Message = $"Checking installed packages for {SelectedLanguageCode}...",
                Foreground = Brushes.SteelBlue
            });

            InstalledPackages.Clear();

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "dism.exe",
                    Arguments = "/online /Get-Packages",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync() ?? string.Empty;
                    process.WaitForExit();

                    string[] lines = output.Split('\n');
                    foreach (var raw in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(raw) &&
                            raw.Contains(SelectedLanguageCode, System.StringComparison.OrdinalIgnoreCase))
                        {
                            string cleaned = raw.Trim();
                            if (cleaned.StartsWith("Package Identity", System.StringComparison.OrdinalIgnoreCase))
                            {
                                cleaned = cleaned.Replace("Package Identity : ", "").Trim();
                            }

                            InstalledPackages.Add(new LanguagePackModel { PackageIdentity = cleaned });

                            OperationLog.Add(new LogEntry
                            {
                                Message = $"Found: {cleaned}",
                                Foreground = Brushes.Black
                            });
                        }
                    }
                }

                OperationLog.Add(new LogEntry
                {
                    Message = $"Check completed for {SelectedLanguageCode}.",
                    Foreground = Brushes.Green
                });
            }
            catch
            {
                OperationLog.Add(new LogEntry
                {
                    Message = "Error occurred while checking packages.",
                    Foreground = Brushes.Red
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}