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
    public class DeleteViewModel : INotifyPropertyChanged
    {
        private string _selectedLanguageCode = string.Empty;
        private string _operationStatus = string.Empty;
        private bool _isBusy;

        private ObservableCollection<LogEntry> _operationLog = new ObservableCollection<LogEntry>();

        public ObservableCollection<string> SupportedLanguages { get; set; }
        public ObservableCollection<LogEntry> OperationLog
        {
            get => _operationLog;
            set
            {
                if (_operationLog != value)
                {
                    _operationLog = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedLanguageCode
        {
            get => _selectedLanguageCode;
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
            get => _operationStatus;
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
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand DeleteCommand { get; }

        public DeleteViewModel()
        {
            SupportedLanguages = new ObservableCollection<string>
            {
                "en-US", "de-DE", "fr-FR", "it-IT", "ja-JP", "sv-SE", "zh-CN"
            };

            DeleteCommand = new RelayCommand(async _ => await ExecuteDeleteAsync(), _ => !IsBusy);
        }

        private async Task ExecuteDeleteAsync()
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
                Message = $"Deleting packages for {SelectedLanguageCode}...",
                Foreground = Brushes.SteelBlue
            });

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
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line) &&
                            line.Contains(SelectedLanguageCode, StringComparison.OrdinalIgnoreCase) &&
                            line.Contains("Package Identity"))
                        {
                            string packageName = line.Replace("Package Identity : ", "").Trim();

                            // Only target language-related packages
                            if (!(packageName.Contains("LanguagePack-Package") ||
                                  packageName.Contains("LanguageFeatures-Basic") ||
                                  packageName.Contains("LanguageFeatures-Handwriting") ||
                                  packageName.Contains("LanguageFeatures-OCR") ||
                                  packageName.Contains("LanguageFeatures-Speech") ||
                                  packageName.Contains("LanguageFeatures-TextToSpeech")))
                            {
                                continue;
                            }

                            OperationLog.Add(new LogEntry
                            {
                                Message = $"Removing: {packageName}",
                                Foreground = Brushes.DarkOrange
                            });

                            var removeProcessInfo = new ProcessStartInfo
                            {
                                FileName = "dism.exe",
                                Arguments = $"/online /Remove-Package /PackageName:\"{packageName}\"",
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };

                            using (var removeProcess = new Process { StartInfo = removeProcessInfo })
                            {
                                removeProcess.Start();
                                string removeOutput = await removeProcess.StandardOutput.ReadToEndAsync();
                                string removeError = await removeProcess.StandardError.ReadToEndAsync();
                                removeProcess.WaitForExit();

                                if (removeProcess.ExitCode == 0)
                                {
                                    OperationLog.Add(new LogEntry
                                    {
                                        Message = $"Successfully removed {packageName}",
                                        Foreground = Brushes.Green
                                    });
                                }
                                else
                                {
                                    OperationLog.Add(new LogEntry
                                    {
                                        Message = $"Failed to remove {packageName} (ExitCode {removeProcess.ExitCode}). " +
                                                  $"Output: {removeOutput} Error: {removeError}",
                                        Foreground = Brushes.Red
                                    });
                                }
                            }
                        }
                    }
                }

                OperationLog.Add(new LogEntry
                {
                    Message = $"Deletion completed for {SelectedLanguageCode}.",
                    Foreground = Brushes.Green
                });
            }
            catch
            {
                OperationLog.Add(new LogEntry
                {
                    Message = "Error occurred while deleting packages.",
                    Foreground = Brushes.Red
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
