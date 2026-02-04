using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PKernelFlow.Adapters.Type;
using WiCAM.Pn4000.PKernelFlow.WrapC;
using WiCAM.Pn4000.PKernelFlow.WrapCommon;

namespace WiCAM.Pn4000.PKernelFlow.Adapters;

public static class GeneralSystemComponentsAdapter
{
    private static readonly ILogger _logger = LoggerFactory.Create(builder => 
        builder.AddConsole()).CreateLogger(typeof(GeneralSystemComponentsAdapter));
        
    public static IPnPathService _pnPathService; // Service für Pfade

    static GeneralSystemComponentsAdapter()
    {
        // Initialisieren Sie _pnPathService über DI oder andere Mechanismen
        // Dies ist ein Beispiel, passen Sie es an Ihre DI-Struktur an
        
    }

    public static float Thickness
    {
        get
        {
            try
            {
                var thickness = GSL3DG.gsl3dg_get_r3dick();
                _logger.LogTrace("Get Thickness: {Thickness}", thickness);
                return thickness;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting thickness");
                throw;
            }
        }
        set
        {
            try
            {
                _logger.LogTrace("Set Thickness: {Thickness}", value);
                GSL3DG.gsl3dg_set_r3dick(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting thickness to {Thickness}", value);
                throw;
            }
        }
    }

    public static float Material
    {
        get
        {
            try
            {
                var material = GSL3DG.gsl3dg_get_r3mtyp();
                _logger.LogTrace("Get Material: {Material}", material);
                return material;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting material");
                throw;
            }
        }
        set
        {
            try
            {
                _logger.LogTrace("Set Material: {Material}", value);
                GSL3DG.gsl3dg_set_r3mtyp(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting material to {Material}", value);
                throw;
            }
        }
    }

    public static void StartPKernel()
    {
        try
        {
            _logger.LogInformation("Starting PKernel");
            PEX.pex002_();
            _logger.LogInformation("PKernel started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting PKernel");
            throw;
        }
    }

    public static int SetApplicationTitle(string title)
    {
        try
        {
            _logger.LogTrace("Setting application title: {Title}", title);
            int iret = 0;
            WIP.wip018_(MarshalString.CreateFString(title, 80), ref iret, 80);
            _logger.LogTrace("Application title set, return code: {ReturnCode}", iret);
            return iret;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting application title: {Title}", title);
            throw;
        }
    }

    public static void UpdateInfoPaneForMaterialAndThickness()
    {
        try
        {
            _logger.LogTrace("Updating info pane for material and thickness");
            int iret = 0;
            WIP.wip036_(ref iret);
            _logger.LogTrace("Info pane updated, return code: {ReturnCode}", iret);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating info pane for material and thickness");
            throw;
        }
    }

    public static void FreeLicense(int lic)
    {
        try
        {
            _logger.LogInformation("Freeing license: {License}", lic);
            int iret = 0;
            WIP.wip103_(ref lic, ref iret);
            _logger.LogTrace("License freed, return code: {ReturnCode}", iret);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error freeing license: {License}", lic);
            throw;
        }
    }

    public static int ReserveLicense(int lic)
    {
        try
        {
            _logger.LogInformation("Reserving license: {License}", lic);
            int iok = 0;
            int iret = 0;
     //       WIP.wip102_(ref lic, ref iok, ref iret);
       //     _logger.LogTrace("License reservation complete - OK: {OK}, return code: {ReturnCode}", iok, iret);
            return iok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving license: {License}", lic);
            throw;
        }
    }
    /// <summary>
    /// WICHTIG
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <param name="archiveNumber"></param>
    /// <returns></returns>
    public static bool Save3DPopup(out string path, out string fileName, ref int archiveNumber)
    {
        path = string.Empty;
        fileName = string.Empty;
        
        try
        {
            _logger.LogTrace("Opening Save3D popup with archive number: {ArchiveNumber}", archiveNumber);
            byte[] array = MarshalString.CreateFString(80);
            byte[] array2 = MarshalString.CreateFString(300);
            int iret = 0;

            if (archiveNumber >= 0)
            {
                _logger.LogTrace("Setting archive for 3D: {ArchiveNumber}", archiveNumber);
                string archivePath = ArchiveAdapter.SetArchiveAndGetPathFor3D(archiveNumber);
                
                _logger.LogTrace("Raw archive path received: {Path}", archivePath);

                if (!string.IsNullOrEmpty(archivePath))
                {
                    // Stelle sicher, dass der Basis-Pfad existiert
                    string fullPath;
                    if (archivePath.StartsWith("\\"))
                    {
                        fullPath = Path.Combine(_pnPathService.PNDRIVE, archivePath.TrimStart('\\'));
                    }
                    else if (!Path.IsPathRooted(archivePath))
                    {
                        // Verwende den korrekten Pfad von ArchiveAdapter anstatt des hartcodierten
                        fullPath = Path.Combine(_pnPathService.PNDRIVE, archivePath);
                    }
                    else
                    {
                        fullPath = archivePath;
                    }

                    // Überprüfe, ob der Pfad existiert
                    if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                    {
                        _logger.LogWarning("Archive directory does not exist: {Path}", fullPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    }

                    _logger.LogTrace("Modified archive path: {Path}", fullPath);
                    array2 = MarshalString.CreateFString(fullPath, 300);
                }
            }

            int result;
            GCHandle handle1 = GCHandle.Alloc(array, GCHandleType.Pinned);
            GCHandle handle2 = GCHandle.Alloc(array2, GCHandleType.Pinned);
            try
            {
                result = WIP.wip104_(array, ref archiveNumber, array2, ref iret, 80, 300);
                _logger.LogTrace("wip104_ called - Result: {Result}, ReturnCode: {ReturnCode}", result, iret);
                
                if (result != 0 || iret != 0)
                {
                    _logger.LogWarning("Save3D popup failed, result code: {ResultCode}, return code: {ReturnCode}", 
                        result, iret);
                    return false;
                }

                path = MarshalString.FString2String(array2);
                fileName = MarshalString.FString2String(array);
            }
            finally
            {
                handle1.Free();
                handle2.Free();
            }

        // Pfadbehandlung nach erfolgreichem Aufruf
        if (!string.IsNullOrEmpty(path))
        {
            if (path.StartsWith("\\"))
            {
                path = Path.Combine(_pnPathService.PNDRIVE, path.TrimStart('\\'));
            }
            else if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(_pnPathService.PNDRIVE, path);
            }
            _logger.LogTrace("Modified return path: {Path}", path);
        }

        _logger.LogInformation("Save3D popup completed - Path: {Path}, FileName: {FileName}", path, fileName);
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in Save3D popup with archive number: {ArchiveNumber}", archiveNumber);
        throw;
    }
}

    public static string GetPopupHelpName(string functionName, string popupName, int popupTab)
    {
        try
        {
            _logger.LogTrace("Getting popup help name - Function: {Function}, Popup: {Popup}, Tab: {Tab}", 
                functionName, popupName, popupTab);
            
            byte[] name = MarshalString.CreateFString(functionName, 80);
            byte[] chpopn = MarshalString.CreateFString(popupName, 80);
            byte[] array = MarshalString.CreateFString(80);
            int iret = 0;

            PST.pst363_(name, chpopn, ref popupTab, array, ref iret, 80, 80, 80);
            var result = MarshalString.FString2String(array);
            
            _logger.LogTrace("Popup help name retrieved: {Result}, return code: {ReturnCode}", result, iret);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popup help name for function: {Function}, popup: {Popup}", 
                functionName, popupName);
            throw;
        }
    }

    public static string GetTextById(int id)
	{
		int iret = 0;
		byte[] array = MarshalString.CreateFString(80);
		P2H.p2h001_(ref id, array, ref iret, 80);
		return MarshalString.FString2String(array);
	}

	public static string GetMFileLine(int menuID, int lineID)
	{
		byte[] array = MarshalString.CreateFString(80);
		int iret = 0;
		PFS.pfs842_(ref menuID, ref lineID, array, ref iret, 80);
		return MarshalString.FString2String(array);
	}

	public static string GetCenterMenuFileName(int iD)
	{
		byte[] array = MarshalString.CreateFString(80);
		int iret = 0;
		P2H.p2h005_(ref iD, array, ref iret, 80);
		return MarshalString.FString2String(array);
	}

	public static string GetContextMenuPath(string name)
	{
		byte[] str = MarshalString.CreateFString(name, 80);
		byte[] array = MarshalString.CreateFString(80);
		int iret = 0;
		P2H.p2h003_(str, array, ref iret, 80, 80);
		return MarshalString.FString2String(array);
	}

	public static void GetCurrentAutoloopName(out string fileName)
	{
		byte[] array = MarshalString.CreateFString(80);
		int iret = 0;
		WIP.wip037_(array, ref iret, 80);
		if (array.All((byte b) => b == 0))
		{
			fileName = null;
		}
		else
		{
			fileName = MarshalString.FString2String(array);
		}
	}

	public static void New3DPartLoaded()
	{
		int iret = 0;
		WIP.wip108_(ref iret);
	}

    public static void WriteToLOPERR(int errId, string err, string filename)
    {
        _logger.LogError("Error ID: {ErrorId}, Message: {Error}, File: {Filename}", errId, err, filename);
    }

    // ... Rest of the methods following the same pattern
}
