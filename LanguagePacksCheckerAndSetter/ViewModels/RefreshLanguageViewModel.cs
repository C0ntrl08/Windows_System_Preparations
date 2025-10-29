using LanguagePacksCheckerAndSetter.Configuration;
using LanguagePacksCheckerAndSetter.Models;
using LanguagePacksCheckerAndSetter.Utilities;
using Microsoft.PowerShell.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace LanguagePacksCheckerAndSetter.ViewModels
{
    public class RefreshLanguageViewModel : INotifyPropertyChanged
    {
        #region Fields
        private ObservableCollection<LogEntry> _operationLog = new();
        private bool _isBusy;
        #endregion

        #region Getters and Setters
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

        public ICommand RefreshLanguageCommand { get; }
        #endregion

        #region Constructors
        public RefreshLanguageViewModel()
        {
            RefreshLanguageCommand = new RelayCommand(async _ => await ExecuteRefreshAsync(), _ => !IsBusy);
        }

        #endregion

        #region Functions
        private async Task ExecuteRefreshAsync()
        {
            IsBusy = true;
            OperationLog.Clear();

            try
            {
                var installedTags = await GetInstalledLanguageTagsAsync();

                if (!installedTags.Any())
                {
                    OperationLog.Add(new LogEntry { Message = "No installed language packs found.", Foreground = Brushes.Red });
                    return;
                }

                foreach (var langTag in installedTags.Intersect(LanguagePackConfig.SupportedLanguages))
                {
                    using PowerShell ps = PowerShell.Create();

                    string script = $@"
$langList = Get-WinUserLanguageList
if (-not ($langList.LanguageTag -contains '{langTag}')) {{
    $langList.Add('{langTag}')
    Set-WinUserLanguageList $langList -Force
    'Added {langTag} to user interface.'
}} else {{
    'Language {langTag} already present.'
}}
";

                    OperationLog.Add(new LogEntry { Message = $"Refreshing UI for {langTag}...", Foreground = Brushes.Gray });

                    var results = await Task.Run(() => ps.AddScript(script).Invoke());
                    foreach (var result in results)
                    {
                        OperationLog.Add(new LogEntry { Message = result.ToString(), Foreground = Brushes.SteelBlue });
                    }

                    if (ps.HadErrors)
                    {
                        foreach (var error in ps.Streams.Error)
                        {
                            OperationLog.Add(new LogEntry { Message = $"PowerShell error: {error}", Foreground = Brushes.Red });
                        }
                    }
                }

                OperationLog.Add(new LogEntry { Message = "Refresh process completed.", Foreground = Brushes.Green });
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

        private async Task<List<string>> GetInstalledLanguageTagsAsync()
        {
            var installedTags = new List<string>();

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dism.exe",
                Arguments = "/online /get-packages",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            // Extract language tags from output
            foreach (var line in output.Split('\n'))
            {
                if (line.Contains("LanguagePack", StringComparison.OrdinalIgnoreCase))
                {
                    // Example line: Package Identity : Microsoft-Windows-Client-LanguagePack-Package~31bf3856ad364e35~amd64~de-DE~10.0.22621.1
                    var parts = line.Split('~');
                    if (parts.Length >= 4)
                    {
                        string langTag = parts[3]; // de-DE
                        installedTags.Add(langTag);
                    }
                }
            }

            return installedTags;
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
