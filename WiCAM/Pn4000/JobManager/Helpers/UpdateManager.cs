using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager.Helpers;

public class UpdateManager
{
    private readonly string _updateSourcePath = @"P:\MausiManager";
    private readonly string _currentProgramPath;

    public UpdateManager()
    {
        _currentProgramPath = AppDomain.CurrentDomain.BaseDirectory;
    }

    /// <summary>
    /// Prüft ob Updates verfügbar sind und führt diese durch
    /// </summary>
    /// <returns>True wenn ein Update durchgeführt wurde, sonst False</returns>
    public bool CheckAndApplyUpdates()
    {
        try
        {
            // Prüfen ob Update-Pfad existiert
            if (!Directory.Exists(_updateSourcePath))
            {
                Logger.Info("Update-Pfad nicht gefunden: {0}", _updateSourcePath);
                return false;
            }

            // Neuere Dateien ermitteln
            var filesToUpdate = GetNewerFiles();

            if (!filesToUpdate.Any())
            {
                Logger.Info("Keine neueren Dateien gefunden.");
                return false;
            }

            // Benutzer informieren
            var fileList = string.Join("\n", filesToUpdate.Take(10).Select(f => "  - " + f.RelativePath));
            if (filesToUpdate.Count > 10)
            {
                fileList += $"\n  ... und {filesToUpdate.Count - 10} weitere";
            }

            var message = $"Es wurden {filesToUpdate.Count} neuere Dateien gefunden:\n\n" +
                         fileList + "\n\n" +
                         "Das Programm wird beendet und die Updates werden installiert.\n\n" +
                         "Möchten Sie fortfahren?";

            var result = MessageBox.Show(message, "Update verfügbar",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                Logger.Info("Update vom Benutzer abgebrochen.");
                return false;
            }

            // Updater-Batch erstellen und ausführen
            CreateAndRunUpdateBatch(filesToUpdate);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Exception(ex);
            MessageBox.Show($"Fehler beim Update-Check: {ex.Message}",
                           "Update-Fehler",
                           MessageBoxButton.OK,
                           MessageBoxImage.Error);
            return false;
        }
    }

    private System.Collections.Generic.List<FileUpdateInfo> GetNewerFiles()
    {
        var filesToUpdate = new System.Collections.Generic.List<FileUpdateInfo>();

        try
        {
            var sourceFiles = Directory.GetFiles(_updateSourcePath, "*.*", SearchOption.AllDirectories);

            foreach (var sourceFile in sourceFiles)
            {
                var relativePath = Path.GetRelativePath(_updateSourcePath, sourceFile);

                // Dateien ausschließen, die nicht aktualisiert werden sollen
                if (ShouldExcludeFile(relativePath))
                {
                    continue;
                }

                var targetFile = Path.Combine(_currentProgramPath, relativePath);

                // Nur wenn Zieldatei existiert, Datum vergleichen
                if (File.Exists(targetFile))
                {
                    var sourceDate = File.GetLastWriteTime(sourceFile);
                    var targetDate = File.GetLastWriteTime(targetFile);

                    if (sourceDate > targetDate)
                    {
                        filesToUpdate.Add(new FileUpdateInfo
                        {
                            SourcePath = sourceFile,
                            TargetPath = targetFile,
                            RelativePath = relativePath
                        });

                        Logger.Info("Neuere Datei gefunden: {0} (Quelle: {1}, Ziel: {2})",
                                   relativePath,
                                   sourceDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                   targetDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                }
                else
                {
                    // Neue Datei, die im Zielpfad noch nicht existiert
                    filesToUpdate.Add(new FileUpdateInfo
                    {
                        SourcePath = sourceFile,
                        TargetPath = targetFile,
                        RelativePath = relativePath
                    });

                    Logger.Info("Neue Datei gefunden: {0}", relativePath);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Exception(ex);
        }

        return filesToUpdate;
    }

    private bool ShouldExcludeFile(string relativePath)
    {
        var fileName = Path.GetFileName(relativePath).ToLowerInvariant();
        var extension = Path.GetExtension(relativePath).ToLowerInvariant();

        // Dateitypen, die nicht aktualisiert werden sollen
        var excludedExtensions = new[] { ".log", ".ini", ".config", ".user", ".suo" };

        // Dateien, die nicht aktualisiert werden sollen
        var excludedFiles = new[] { "appsettings.json", "app.config" };

        // Verzeichnisse, die ausgeschlossen werden sollen
        var excludedFolders = new[] { "logs", "backup", "temp" };

        if (excludedExtensions.Contains(extension))
            return true;

        if (excludedFiles.Contains(fileName))
            return true;

        var pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (pathParts.Any(part => excludedFolders.Contains(part.ToLowerInvariant())))
            return true;

        return false;
    }

    private void CreateAndRunUpdateBatch(System.Collections.Generic.List<FileUpdateInfo> files)
    {
        var batchPath = Path.Combine(Path.GetTempPath(), "MausiManager_Update.bat");
        var exePath = Process.GetCurrentProcess().MainModule?.FileName;
        var processId = Process.GetCurrentProcess().Id;

        using (var writer = new StreamWriter(batchPath, false, System.Text.Encoding.Default))
        {
            writer.WriteLine("@echo off");
            writer.WriteLine("chcp 1252 > nul");
            writer.WriteLine("title MausiManager Update");
            writer.WriteLine("color 0A");
            writer.WriteLine();
            writer.WriteLine("echo ================================================");
            writer.WriteLine("echo   MausiManager Update");
            writer.WriteLine("echo ================================================");
            writer.WriteLine("echo.");
            writer.WriteLine();

            // Warten bis Programm vollständig beendet ist
            writer.WriteLine("echo Warte auf Beendigung des Programms...");
            writer.WriteLine($":WAIT_LOOP");
            writer.WriteLine($"tasklist /FI \"PID eq {processId}\" 2>nul | find \"{processId}\" >nul");
            writer.WriteLine("if not errorlevel 1 (");
            writer.WriteLine("    timeout /t 1 /nobreak > nul");
            writer.WriteLine("    goto WAIT_LOOP");
            writer.WriteLine(")");
            writer.WriteLine("echo Programm wurde beendet.");
            writer.WriteLine("echo.");
            writer.WriteLine();

            // Zusätzliche Sicherheitspause
            writer.WriteLine("timeout /t 2 /nobreak > nul");
            writer.WriteLine();

            // Backup-Ordner erstellen
            var backupPath = Path.Combine(_currentProgramPath, "Backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            writer.WriteLine("echo Erstelle Backup-Ordner...");
            writer.WriteLine($"if not exist \"{backupPath}\" mkdir \"{backupPath}\"");
            writer.WriteLine("echo.");
            writer.WriteLine();

            // Dateien kopieren
            writer.WriteLine("echo Aktualisiere Dateien...");
            writer.WriteLine("echo.");

            int counter = 0;
            foreach (var file in files)
            {
                counter++;
                var backupFile = Path.Combine(backupPath, file.RelativePath);
                var backupDir = Path.GetDirectoryName(backupFile);

                writer.WriteLine($"echo [{counter}/{files.Count}] {file.RelativePath}");

                // Backup erstellen (falls Datei existiert)
                writer.WriteLine($"if exist \"{file.TargetPath}\" (");
                writer.WriteLine($"    if not exist \"{backupDir}\" mkdir \"{backupDir}\"");
                writer.WriteLine($"    copy /Y \"{file.TargetPath}\" \"{backupFile}\" > nul 2>&1");
                writer.WriteLine(")");

                // Zielverzeichnis erstellen falls nötig
                var targetDir = Path.GetDirectoryName(file.TargetPath);
                writer.WriteLine($"if not exist \"{targetDir}\" mkdir \"{targetDir}\"");

                // Datei kopieren mit Fehlerbehandlung
                writer.WriteLine($"copy /Y \"{file.SourcePath}\" \"{file.TargetPath}\" > nul 2>&1");
                writer.WriteLine("if errorlevel 1 (");
                writer.WriteLine($"    echo    FEHLER: Konnte nicht kopiert werden!");
                writer.WriteLine(") else (");
                writer.WriteLine($"    echo    OK");
                writer.WriteLine(")");
                writer.WriteLine();
            }

            writer.WriteLine();
            writer.WriteLine("echo.");
            writer.WriteLine("echo ================================================");
            writer.WriteLine("echo   Update abgeschlossen!");
            writer.WriteLine("echo ================================================");
            writer.WriteLine("echo.");

            // Backup-Verzeichnis anzeigen
            writer.WriteLine($"echo Backup erstellt in:");
            writer.WriteLine($"echo {backupPath}");
            writer.WriteLine("echo.");
            writer.WriteLine();

            // Programm neu starten
            if (!string.IsNullOrEmpty(exePath))
            {
                writer.WriteLine("echo Starte Programm neu...");
                writer.WriteLine("timeout /t 3 /nobreak > nul");
                writer.WriteLine($"start \"\" \"{exePath}\"");
            }

            writer.WriteLine();
            writer.WriteLine("echo.");
            writer.WriteLine("echo Dieses Fenster wird in 5 Sekunden automatisch geschlossen...");
            writer.WriteLine("timeout /t 5 /nobreak > nul");
            writer.WriteLine();

            // Batch-Datei selbst löschen
            writer.WriteLine("del \"%~f0\" & exit");
        }

        // Batch-Datei ausführen
        var processInfo = new ProcessStartInfo
        {
            FileName = batchPath,
            UseShellExecute = true,
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Normal
        };

        Process.Start(processInfo);

        Logger.Info("Update-Batch gestartet: {0}", batchPath);
        Logger.Info("Programm wird beendet für Update...");
    }

    private class FileUpdateInfo
    {
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public string RelativePath { get; set; }
    }
}