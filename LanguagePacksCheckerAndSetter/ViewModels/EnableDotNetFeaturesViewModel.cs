using LanguagePacksCheckerAndSetter.Models;
using LanguagePacksCheckerAndSetter.Utilities;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace LanguagePacksCheckerAndSetter.ViewModels
{
    public class EnableDotNetFeaturesViewModel : INotifyPropertyChanged
    {
        #region Fields
        private string _selectedSourcePath = string.Empty;
        private bool _isBusy;
        private ObservableCollection<LogEntry> _operationLog = new();
        #endregion

        #region Getters and Setters
        public string SelectedSourcePath
        {
            get { return _selectedSourcePath; }
            set 
            {
                if (_selectedSourcePath != value)
                {
                    _selectedSourcePath = value;
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
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
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
        public ICommand BrowseSourceCommand { get; }
        public ICommand EnableFeaturesCommand { get; }
        #endregion

        #region Constuctors
        public EnableDotNetFeaturesViewModel()
        {
            BrowseSourceCommand = new RelayCommand(_ => ExecuteBrowseSource(), _ => !IsBusy);
            EnableFeaturesCommand = new RelayCommand(async _ => await ExecuteEnableFeaturesAsync(), _ => !IsBusy && !string.IsNullOrWhiteSpace(SelectedSourcePath));
        }
        #endregion

        #region Functions

        private void ExecuteBrowseSource()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select CAB file for .NET Features",
                Filter = "CAB Files (*.cab)|*.cab",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedSourcePath = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
                OperationLog.Add(new LogEntry
                {
                    Message = $"Selected source path: {SelectedSourcePath}",
                    Foreground = Brushes.SteelBlue
                });
            }
        }

        private async Task ExecuteEnableFeaturesAsync()
        {
            IsBusy = true;
            OperationLog.Clear();

            try
            {
                var features = new[]
                {
                    "NetFx3",
                    "WCF-HTTP-Activation",
                    "WCF-NonHTTP-Activation"
                };

                foreach (var feature in features)
                {
                    var args = $"/Online /Enable-Feature /FeatureName:{feature} /All /LimitAccess /Source:{SelectedSourcePath}";
                    OperationLog.Add(new LogEntry { Message = $"Enabling feature: {feature}", Foreground = Brushes.Gray });

                    var psi = new ProcessStartInfo
                    {
                        FileName = "dism.exe",
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = new Process { StartInfo = psi };
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        OperationLog.Add(new LogEntry { Message = $"Feature {feature} enabled successfully.", Foreground = Brushes.Green });
                    }
                    else
                    {
                        OperationLog.Add(new LogEntry { Message = $"Failed to enable {feature}. Error: {error}", Foreground = Brushes.Red });
                    }
                }

                OperationLog.Add(new LogEntry { Message = "All features processed.", Foreground = Brushes.Green });
            }
            catch (Exception ex)
            {
                OperationLog.Add(new LogEntry { Message = $"Exception: {ex.Message}", Foreground = Brushes.Red });
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
