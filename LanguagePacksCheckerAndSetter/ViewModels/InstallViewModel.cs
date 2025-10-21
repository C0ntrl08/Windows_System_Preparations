using LanguagePacksCheckerAndSetter.Models;
using LanguagePacksCheckerAndSetter.Utilities;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace LanguagePacksCheckerAndSetter.ViewModels
{
    public class InstallViewModel : INotifyPropertyChanged
    {
        #region Fields
        private string _selectedFolder = string.Empty;
        private bool _isBusy;
        private ObservableCollection<LanguagePackModel> _availablePacks = new ObservableCollection<LanguagePackModel>();
        private ObservableCollection<LogEntry> _operationLog = new ObservableCollection<LogEntry>();
        private bool _hasSelectedPacks = false;
        #endregion

        #region Properties
        public ObservableCollection<LanguagePackModel> AvailablePacks
        {
            get { return _availablePacks; }
            set
            {
                if (_availablePacks != value)
                {
                    _availablePacks = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<LogEntry> OperationLog
        {
            get { return _operationLog; }
            set
            {
                if (_operationLog != value)
                {
                    _operationLog = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SelectedFolder
        {
            get { return _selectedFolder; }
            set
            {
                if (_selectedFolder != value)
                {
                    _selectedFolder = value;
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
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        public bool HasSelectedPacks
        {
            get { return _hasSelectedPacks; }
            set
            {
                if (_hasSelectedPacks != value)
                {
                    _hasSelectedPacks = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand BrowseFolderCommand { get; }
        public ICommand InstallSelectedCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }
        #endregion

        #region Constructors
        public InstallViewModel()
        {
            
            BrowseFolderCommand = new RelayCommand(async _ => await ExecuteBrowseFolderAsync(), _ => !IsBusy);
            InstallSelectedCommand = new RelayCommand(async _ => await ExecuteInstallAsync(), _ => !IsBusy && HasSelectedPacks);

            SelectAllCommand = new RelayCommand(_ => SetSelection(true),_ => !IsBusy && AvailablePacks.Count > 0);
            DeselectAllCommand = new RelayCommand(_ => SetSelection(false),_ => !IsBusy && AvailablePacks.Count > 0);
            //SelectAllCommand = new RelayCommand(_ => SetSelection(true), _ => AvailablePacks.Count > 0);
            //DeselectAllCommand = new RelayCommand(_ => SetSelection(false), _ => AvailablePacks.Count > 0);

            AvailablePacks.CollectionChanged += AvailablePacks_CollectionChanged;
        }
        #endregion

        #region Functions
        /// <summary>
        /// Opens a dialog to allow the user to select a folder and loads CAB files from the selected folder.
        /// </summary>
        /// <remarks>This method displays an <see cref="OpenFileDialog"/> configured to select a folder.
        /// The folder path is determined based on the dummy file name provided in the dialog. If a valid folder is
        /// selected, the folder path is assigned to the <c>SelectedFolder</c> property, and CAB files from the folder
        /// are loaded asynchronously.</remarks>
        /// <returns></returns>
        private async Task ExecuteBrowseFolderAsync()
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                FileName = "Select Folder", // Dummy name
                Title = "Select a folder containing CAB files"
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                string? folderPath = Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    SelectedFolder = folderPath;
                    await LoadCabFilesAsync(SelectedFolder);
                }
            }
        }

        private async Task LoadCabFilesAsync(string folderPath)
        {
            AvailablePacks.Clear();

            if (!Directory.Exists(folderPath))
            {
                OperationLog.Add(new LogEntry
                {
                    Message = "Selected folder does not exist.",
                    Foreground = Brushes.Red
                });
                return;
            }

            string[] cabFiles = Directory.GetFiles(folderPath, "*.cab", SearchOption.TopDirectoryOnly);

            foreach (var file in cabFiles)
            {
                string fileName = Path.GetFileName(file);
                string langCode = ExtractLanguageCode(fileName);

                AvailablePacks.Add(new LanguagePackModel
                {
                    PackageIdentity = fileName,
                    FilePath = file,
                    LanguageCode = langCode,
                    IsSelected = false
                });
            }

            // Just sort the collection view, no grouping
            var view = CollectionViewSource.GetDefaultView(AvailablePacks);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(nameof(LanguagePackModel.LanguageCode), ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription(nameof(LanguagePackModel.PackageIdentity), ListSortDirection.Ascending));

            OperationLog.Add(new LogEntry
            {
                Message = $"{cabFiles.Length} CAB files found in {folderPath}.",
                Foreground = Brushes.SteelBlue
            });

            await Task.CompletedTask;
        }


        private string ExtractLanguageCode(string fileName)
        {
            // Very simple heuristic: look for xx-XX pattern
            var parts = fileName.Split('-');
            foreach (var part in parts)
            {
                if (part.Length == 5 && part[2] == '-')
                {
                    return part;
                }
            }
            return "Unknown";
        }
        private async Task ExecuteInstallAsync()
        {
            IsBusy = true;

            try
            {
                foreach (var pack in AvailablePacks)
                {
                    if (!pack.IsSelected) continue;

                    OperationLog.Add(new LogEntry
                    {
                        Message = $"Installing {pack.PackageIdentity}...",
                        Foreground = Brushes.SteelBlue
                    });

                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "dism.exe",
                        Arguments = $"/online /Add-Package /PackagePath:\"{pack.FilePath}\" /Quiet /NoRestart",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = new Process { StartInfo = processStartInfo })
                    {
                        process.Start();
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();
                        process.WaitForExit();

                        if (process.ExitCode == 0)
                        {
                            OperationLog.Add(new LogEntry
                            {
                                Message = $"Successfully installed {pack.PackageIdentity}",
                                Foreground = Brushes.Green
                            });
                        }
                        else
                        {
                            OperationLog.Add(new LogEntry
                            {
                                Message = $"Failed to install {pack.PackageIdentity} (ExitCode {process.ExitCode}). " +
                                          $"Output: {output} Error: {error}",
                                Foreground = Brushes.Red
                            });
                        }
                    }
                }

                OperationLog.Add(new LogEntry
                {
                    Message = "Installation process completed.",
                    Foreground = Brushes.Green
                });
            }
            catch (Exception ex)
            {
                OperationLog.Add(new LogEntry
                {
                    Message = $"Error occurred during installation: {ex.Message}",
                    Foreground = Brushes.Red
                });
            }
            finally
            {
                IsBusy = false;
            }
        }
        private void SetSelection(bool isSelected)
        {
            foreach (var pack in AvailablePacks)
            {
                pack.IsSelected = isSelected;
            }
            HasSelectedPacks = isSelected;
        }

        private void AvailablePacks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (LanguagePackModel pack in e.NewItems)
                {
                    pack.PropertyChanged += Pack_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (LanguagePackModel pack in e.OldItems)
                {
                    pack.PropertyChanged -= Pack_PropertyChanged;
                }
            }

            UpdateHasSelectedPacks();
        }

        private void Pack_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LanguagePackModel.IsSelected))
            {
                UpdateHasSelectedPacks();
            }
        }

        private void UpdateHasSelectedPacks()
        {
            HasSelectedPacks = AvailablePacks.Any(p => p.IsSelected);
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
