using System;
using System.IO;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PKernelFlow.Adapters.Type;
using WiCAM.Pn4000.PKernelFlow.WrapC;
using WiCAM.Pn4000.PKernelFlow.WrapCommon;

namespace WiCAM.Pn4000.PKernelFlow.Adapters;

public static class ArchiveAdapter
{
    public static IPnPathService _pnPathService;
    public static void Initialize(IPnPathService pathService)
    {
        _pnPathService = pathService ?? throw new ArgumentNullException(nameof(pathService));

        // Überprüfe und erstelle Basis-Archivstruktur
        string baseArchivePath = Path.Combine(_pnPathService.PNDRIVE, "u", "ar");
        if (!Directory.Exists(baseArchivePath))
        {
            Directory.CreateDirectory(baseArchivePath);
        }
    }

    private static string archivePath;

    public static int ArchiveID
	{
		get
		{
			return ARCHIV.archiv_get_iaktar();
		}
		set
		{
			ARCHIV.archiv_set_iaktar(value);
		}
	}
	/// <summary>
	/// Setzt das Archiv und gibt den Pfad für 3D-Dateien zurück
	/// </summary>
	/// <param name="ID">Die Archiv-ID</param>
	/// <returns>Der vollständige, validierte Pfad zum 3D-Archiv</returns>
	public static string SetArchiveAndGetPathFor3D(int ID)
	{
		try
		{
			if (_pnPathService == null)
			{
				throw new InvalidOperationException("PnPathService wurde nicht initialisiert");
			}

			// Basisverzeichnis für Archive überprüfen/erstellen
			string baseArchivePath = Path.Combine(_pnPathService.PNDRIVE, "u", "ar");
			if (!Directory.Exists(baseArchivePath))
			{
				Directory.CreateDirectory(baseArchivePath);
			}

			// Archivspezifisches Verzeichnis
			string archiveDir = ID > 0 ? $"ar{ID:D4}" : "abasis";
			string n3dDir = "n3d"; // Unterverzeichnis für 3D-Dateien

			// Vollständigen Pfad zusammenbauen
			archivePath = Path.Combine(baseArchivePath, archiveDir, n3dDir);

			// Sicherstellen, dass das Verzeichnis existiert
			if (!Directory.Exists(archivePath))
			{
				Directory.CreateDirectory(archivePath);
			}

			// Native Funktion aufrufen
			int iret = 0;
			byte[] array = MarshalString.CreateFString(300);
			
			// Pfad für den nativen Aufruf vorbereiten (relativ zu PNDRIVE)
			string relativePath = archivePath;
			array = MarshalString.CreateFString(relativePath, 300);
			
			WIP.wip106_(ref ID, array, ref iret, 300);

			if (iret != 0)
			{
//				throw new IOException($"Fehler beim Setzen des Archivpfads. Fehlercode: {iret}");
			}

			// Den tatsächlich gesetzten Pfad zurückgeben
			string setPath = MarshalString.FString2String(array);
			
			// Wenn der zurückgegebene Pfad leer ist, verwenden wir unseren erstellten Pfad
			if (string.IsNullOrEmpty(setPath))
			{
				return archivePath;
			}

			// Absoluten Pfad zurückgeben
			return archivePath;


                //Path.IsPathRooted(setPath) 
				//? setPath 
				//: Path.Combine(_pnPathService.PNDRIVE, setPath.TrimStart('\\'));
		}
		catch (Exception ex)
		{
			throw new IOException($"Fehler beim Erstellen des Archivpfads für ID {ID}", ex);
		}
	}
}
