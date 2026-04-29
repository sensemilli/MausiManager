using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Graph.Models;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager.Services;

namespace WiCAM.Pn4000.JobManager.ViewModels
{
    public class Microsoft365ViewModel : ViewModelBase
    {
        private readonly Microsoft365Service _service;
        private ObservableCollection<DriveItem> _files;
        private DriveItem _selectedFile;
        private string _searchQuery;
        private bool _isLoading;

        public ObservableCollection<DriveItem> Files
        {
            get => _files;
            set
            {
                _files = value;
                NotifyPropertyChanged(nameof(Files));
            }
        }

        public DriveItem SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                NotifyPropertyChanged(nameof(SelectedFile));
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                NotifyPropertyChanged(nameof(SearchQuery));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                NotifyPropertyChanged(nameof(IsLoading));
            }
        }

        public ICommand LoadFilesCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand DownloadCommand { get; }
        public ICommand UploadCommand { get; }

        public Microsoft365ViewModel()
        {
            Files = new ObservableCollection<DriveItem>();

            // Konfiguration aus Datei/Settings laden
            var config = LoadConfiguration();

            // KRITISCH: Validierung BEVOR Service erstellt wird
            if (!ValidateConfiguration(config))
            {
                Logger.Warning("Microsoft 365 Konfiguration ist ungültig oder fehlt. Service wird nicht initialisiert.");
                
                // Commands mit Fehlermeldung erstellen
                LoadFilesCommand = new RelayCommand(_ => ShowConfigurationError());
                SearchCommand = new RelayCommand(_ => ShowConfigurationError(), _ => false);
                DownloadCommand = new RelayCommand(_ => ShowConfigurationError(), _ => false);
                UploadCommand = new RelayCommand(_ => ShowConfigurationError());
                
                return; // Service NICHT initialisieren
            }

            // Nur wenn Konfiguration gültig ist
            _service = new Microsoft365Service(config);

            LoadFilesCommand = new RelayCommand(async _ => await LoadFilesAsync());
            SearchCommand = new RelayCommand(async _ => await SearchFilesAsync(), _ => !string.IsNullOrWhiteSpace(SearchQuery));
            DownloadCommand = new RelayCommand(async _ => await DownloadFileAsync(), _ => SelectedFile != null);
            UploadCommand = new RelayCommand(async _ => await UploadFileAsync());
        }

        private Microsoft365Config LoadConfiguration()
        {
            // Option 1: Aus App.config laden
            return new Microsoft365Config
            {
                ClientId = System.Configuration.ConfigurationManager.AppSettings["AzureClientId"] ?? "IHRE_CLIENT_ID",
                TenantId = System.Configuration.ConfigurationManager.AppSettings["AzureTenantId"] ?? "IHRE_TENANT_ID",
                ClientSecret = System.Configuration.ConfigurationManager.AppSettings["AzureClientSecret"] ?? "IHR_CLIENT_SECRET"
            };
            
            // Option 2: Aus JSON-Datei laden (empfohlen für .NET 9)
            // var json = File.ReadAllText("appsettings.json");
            // return JsonSerializer.Deserialize<Microsoft365Config>(json);
        }

        private bool ValidateConfiguration(Microsoft365Config config)
        {
            if (config == null)
                return false;

            // Prüfe auf Platzhalter-Werte
            var isValid = !string.IsNullOrWhiteSpace(config.ClientId) &&
                          !config.ClientId.Contains("IHRE_") &&
                          !string.IsNullOrWhiteSpace(config.TenantId) &&
                          !config.TenantId.Contains("IHRE_") &&
                          !string.IsNullOrWhiteSpace(config.ClientSecret) &&
                          !config.ClientSecret.Contains("IHR_");

            // Validiere GUID-Format
            if (isValid)
            {
                isValid = Guid.TryParse(config.ClientId, out _) &&
                          Guid.TryParse(config.TenantId, out _);
            }

            return isValid;
        }

        private void ShowConfigurationError()
        {
            MessageBox.Show(
                "Die Microsoft 365 Integration ist nicht konfiguriert.\n\n" +
                "Bitte fügen Sie folgende Werte in Ihre App.config hinzu:\n" +
                "- AzureClientId\n" +
                "- AzureTenantId\n" +
                "- AzureClientSecret\n\n" +
                "Siehe: https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app",
                "Konfiguration erforderlich",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private async Task LoadFilesAsync()
        {
            try
            {
                IsLoading = true;
                var files = await _service.ListFilesInFolderAsync();
                
                Files.Clear();
                foreach (var file in files)
                {
                    Files.Add(file);
                }

                Logger.Info("Dateien geladen: {0}", files.Count);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                MessageBox.Show($"Fehler beim Laden der Dateien: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchFilesAsync()
        {
            try
            {
                IsLoading = true;
                var files = await _service.SearchFilesAsync(SearchQuery);
                
                Files.Clear();
                foreach (var file in files)
                {
                    Files.Add(file);
                }

                Logger.Info("Suche abgeschlossen: {0} Ergebnisse", files.Count);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                MessageBox.Show($"Fehler bei der Suche: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DownloadFileAsync()
        {
            if (SelectedFile == null) return;

            try
            {
                IsLoading = true;
                
                // Datei herunterladen
                using var stream = await _service.ReadFileAsync(SelectedFile.ParentReference?.DriveId, SelectedFile.Id);
                
                // Speicherdialog
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = SelectedFile.Name,
                    Filter = "Alle Dateien (*.*)|*.*"
                };

                if (dialog.ShowDialog() == true)
                {
                    using var fileStream = File.Create(dialog.FileName);
                    await stream.CopyToAsync(fileStream);
                    
                    MessageBox.Show("Datei erfolgreich heruntergeladen!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                    Logger.Info("Datei heruntergeladen: {0}", dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                MessageBox.Show($"Fehler beim Download: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UploadFileAsync()
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Alle Dateien (*.*)|*.*"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    
                    using var fileStream = File.OpenRead(dialog.FileName);
                    var fileName = Path.GetFileName(dialog.FileName);
                    
                    await _service.UploadFileAsync(fileName, fileStream);
                    
                    MessageBox.Show("Datei erfolgreich hochgeladen!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadFilesAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                MessageBox.Show($"Fehler beim Upload: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}