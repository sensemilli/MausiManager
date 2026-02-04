using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using pnpi_common;
using WiCAM.Pn4000.Contracts.LogCenterServices;

namespace WiCAM.Pn4000.pn4.pn4FlowCenter.Plugins;

public class PluginsManager
{
	private readonly Dictionary<string, object> _pluginDictionary = new Dictionary<string, object>();

	private IPnPlugInBasicModel last_ipi_start;

	private readonly MemoryDisk memoryDisk;

	private readonly ILogCenterService _logCenterService;

	private bool _useOnlineHelp;

	public PluginsManager(ILogCenterService logCenterService, MemoryDisk memoryDisk, bool useOnlineHelp = false)
	{
		this.memoryDisk = memoryDisk;
		this._useOnlineHelp = useOnlineHelp;
		this._logCenterService = logCenterService;
	}

	public string OnPlugIn(string plugin, string command, List<string> data)
	{
		try
		{
			string text = plugin.ToLower();
			if (this._pluginDictionary.ContainsKey(text))
			{
				this.last_ipi_start = (IPnPlugInBasicModel)this._pluginDictionary[text];
				this.last_ipi_start.SetHelpLocation(this._useOnlineHelp);
				this.last_ipi_start.CallFunction(command);
			}
			else
			{
				Type[] exportedTypes = Assembly.LoadFile(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\pnpi_" + text + ".dll").GetExportedTypes();
				IPnPlugInBasicModel pnPlugInBasicModel = null;
				try
				{
					Type[] array = exportedTypes;
					foreach (Type type in array)
					{
						if (typeof(IPnPlugInBasicModel).IsAssignableFrom(type))
						{
							pnPlugInBasicModel = (IPnPlugInBasicModel)Activator.CreateInstance(type);
						}
					}
				}
				catch (Exception e)
				{
					this._logCenterService.CatchRaport(e);
				}
				if (pnPlugInBasicModel != null)
				{
					pnPlugInBasicModel.SetMemoryDiskFunctions(PnPlugInGetMemoryDiskData, PnPlugInSetMemoryDiskData);
					pnPlugInBasicModel.SetHelpLocation(this._useOnlineHelp);
					this._pluginDictionary.Add(text, pnPlugInBasicModel);
					this.last_ipi_start = pnPlugInBasicModel;
					pnPlugInBasicModel.CallFunction(command);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show($"PlugIn Error ({plugin},{command}): {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			this._logCenterService.CatchRaport(ex);
		}
		return "0";
	}

	public List<string> PnPlugInGetMemoryDiskData()
	{
		return this.memoryDisk;
	}

	public void PnPlugInSetMemoryDiskData(List<string> disk)
	{
		this.memoryDisk.CopyStringListToMemoryDisk(disk);
	}

	public void ModifyHelpLocation(bool useOnlineHelp)
	{
		this._useOnlineHelp = useOnlineHelp;
	}
}
