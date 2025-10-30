using LanguagePacksCheckerAndSetter.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace LanguagePacksCheckerAndSetter.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private int _selectedTabIndex;
        private bool _isBusy;

        // Add or remove names here as needed (names without ".exe")
        private static readonly HashSet<string> _criticalProcessNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "dism",       // Deployment Image Servicing and Management
            "dismhost",   // Child processes used by DISM
            "lpksetup",   // Legacy language pack installer (may still appear)
            "wusa"        // Windows Update Standalone Installer
            // Add more if your app uses them explicitly
        };


        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
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

        public ICommand RestartCommand { get; }
        public MainWindowViewModel()
        {
            RestartCommand = new RelayCommand(_ => ExecuteRestart(), _ => true);
        }
        //private void ExecuteRestart()
        //{
        //    var result = MessageBox.Show("Are you sure you want to restart the computer?",
        //                                  "Confirm Restart",
        //                                  MessageBoxButton.YesNo,
        //                                  MessageBoxImage.Warning);

        //    if (result == MessageBoxResult.Yes)
        //    {
        //        var psi = new ProcessStartInfo
        //        {
        //            FileName = "shutdown",
        //            Arguments = "/r /t 0",
        //            CreateNoWindow = true,
        //            UseShellExecute = false
        //        };
        //        Process.Start(psi);
        //    }
        //}


        private void ExecuteRestart()
        {
            var isCriticalOpRunning = IsCriticalOperationRunning(out var runningNames);

            string message;
            string title = "Confirm Restart";
            MessageBoxImage icon;

            if (isCriticalOpRunning)
            {
                // Stronger warning if DISM / related operations are still active.
                var details = string.Join(", ", runningNames.Distinct().OrderBy(n => n));
                message =
                    "It looks like a system operation is still running " +
                    "(e.g., DISM / language pack installation / feature enablement).\n\n" +
                    (details.Length > 0 ? $"Detected process(es): {details}\n\n" : string.Empty) +
                    "Restarting now may interrupt it and leave the system in an inconsistent state.\n\n" +
                    "Do you still want to restart the computer?";
                icon = MessageBoxImage.Warning;
            }
            else
            {
                // Standard confirmation path
                message = "Are you sure you want to restart the computer?";
                icon = MessageBoxImage.Question;
            }

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, icon);

            if (result == MessageBoxResult.Yes)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/r /t 0",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                try
                {
                    Process.Start(psi);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to initiate restart.\n\n{ex.Message}",
                        "Restart Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Checks if any critical processes are running.
        /// Returns true if found and outputs which names were detected.
        /// </summary>
        private static bool IsCriticalOperationRunning(out List<string> runningNames)
        {
            runningNames = new List<string>();

            foreach (var name in _criticalProcessNames)
            {
                // GetProcessesByName expects the process name without ".exe"
                var found = Process.GetProcessesByName(name);
                if (found != null && found.Length > 0)
                {
                    runningNames.Add(name);
                }
            }

            return runningNames.Count > 0;
        }



        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
