using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4UILib;
using WiCAM.Pn4000.pn4.pn4UILib.Ribbon;

namespace WiCAM.Pn4000.pn4.pn4Services;

public class ActivateProgramPart
{
	private ILogCenterService _logCenterService;

	private Dictionary<int, string> _moduleNames = new Dictionary<int, string>();

	private IPKernelFlowGlobalDataService _pKernelFlowGlobalData;

	private readonly PnRibbon _ribbon;

	private readonly Toolbars _toolbars;

	private readonly Shortcuts _shortcuts;

	private readonly ExeFlow _exeFlow;

	private readonly CenterMenuHelper _centerMenuHelper;

	private IPnPathService _pnPathService;

	private string _ribbonModuleName;

	private Dictionary<int, string> _standardModuleNames = new Dictionary<int, string>();

	public string ModuleName { get; set; }

	public ActivateProgramPart(ILogCenterService logCenter, IPnPathService pathService, IPKernelFlowGlobalDataService pKernelFlow,
		PnRibbon ribbon, Toolbars toolbars, Shortcuts shortcuts, ExeFlow exeFlow, CenterMenuHelper centerMenuHelper)
	{
		_pKernelFlowGlobalData = pKernelFlow;
		_ribbon = ribbon;
		_toolbars = toolbars;
		_shortcuts = shortcuts;
		_exeFlow = exeFlow;
		_centerMenuHelper = centerMenuHelper;
		_logCenterService = logCenter;
		_pnPathService = pathService;
		LoadModuleNames();
	}

	private void LoadModuleNames()
	{
		FillModuleNames(_moduleNames);
		FillModuleNames(_standardModuleNames);
		string text = "pn.rfiles\\RFILES.TXT";
		if (!File.Exists(text))
		{
			text = _pnPathService.PNDRIVE + "\\u\\pn\\rfiles\\RFILES.TXT";
			if (!File.Exists(text))
			{
				text = string.Empty;
			}
		}
		if (!(text != string.Empty))
		{
			return;
		}
		try
		{
			string[] array = File.ReadAllLines(text);
			foreach (string obj in array)
			{
				int num = Convert.ToInt32(obj.Substring(0, 1));
				string text2 = obj.Substring(5);
				if (_pnPathService.RfileModuleExist(text2))
				{
					if (_moduleNames.Keys.Contains(num))
					{
						_moduleNames.Remove(num);
					}
					_moduleNames.Add(num, text2);
				}
			}
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
	}

	private void FillModuleNames(Dictionary<int, string> mn)
	{
		mn.Clear();
		mn.Add(0, "2D");
		mn.Add(1, "2D");
		mn.Add(2, "2D");
		mn.Add(3, "View");
	}

	public void SetNewRFileSchemaDynamically(string name)
	{
		int activeProgramPart = _pKernelFlowGlobalData.ActiveProgramPart;
		string value = ((!(name == "Default")) ? $"{_standardModuleNames[activeProgramPart]}_{name}" : _standardModuleNames[activeProgramPart]);
		if (_moduleNames.Keys.Contains(activeProgramPart))
		{
			_moduleNames.Remove(activeProgramPart);
		}
		_moduleNames.Add(activeProgramPart, value);
		string[] array = new string[_moduleNames.Count()];
		for (int i = 0; i < _moduleNames.Count(); i++)
		{
			array[i] = $"{i}    {_moduleNames[i]}";
		}
		try
		{
			Directory.CreateDirectory("pn.rfiles");
			Directory.CreateDirectory("pn.rfiles\\" + ModuleName);
			File.WriteAllLines("pn.rfiles\\RFILES.TXT", array);
			LoadModuleNames();
			_ribbonModuleName = _moduleNames[_pKernelFlowGlobalData.ActiveProgramPart];
			ModuleName = _standardModuleNames[_pKernelFlowGlobalData.ActiveProgramPart];
			_ribbon.ActivateModule(ModuleName, _ribbonModuleName);
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
	}

	public List<string> GetRFileSchemaListForCurrentProgramId(out int current)
	{
		List<string> list = new List<string>();
		current = 0;
		string folder = _pnPathService.BuildPath(_pnPathService.PNHOMEDRIVE, _pnPathService.PNHOMEPATH, "pn.local\\rfiles");
		current = FillRFilesProfilesListByFolder(list, folder);
		if (list.Count() == 0)
		{
			folder = _pnPathService.BuildPath(_pnPathService.PNDRIVE, "\\u\\pn\\rfiles\\");
			current = FillRFilesProfilesListByFolder(list, folder);
		}
		list.Insert(0, "Default");
		return list;
	}

	private int FillRFilesProfilesListByFolder(List<string> ret, string folder)
	{
		int result = 0;
		if (Directory.Exists(folder))
		{
			if (!_standardModuleNames.TryGetValue(_pKernelFlowGlobalData.ActiveProgramPart, out var value))
			{
				return result;
			}
			string searchPattern = $"{value}_*";
			string[] directories = Directory.GetDirectories(folder, searchPattern);
			searchPattern = $"{value}_";
			int num = 1;
			string[] array = directories;
			foreach (string text in array)
			{
				ret.Add(text.Substring(text.LastIndexOf(searchPattern) + searchPattern.Length));
				if (text.Substring(text.LastIndexOf(searchPattern)) == _moduleNames[_pKernelFlowGlobalData.ActiveProgramPart])
				{
					result = num;
				}
				num++;
			}
		}
		ret.Sort();
		return result;
	}

	public void ActivateByPKernelSetActiveProgramPart(int id)
	{
		_pKernelFlowGlobalData.ActiveProgramPart = id;
		_ribbonModuleName = _moduleNames[_pKernelFlowGlobalData.ActiveProgramPart];
		ModuleName = _standardModuleNames[_pKernelFlowGlobalData.ActiveProgramPart];
		Directory.CreateDirectory("pn.rfiles");
		Directory.CreateDirectory("pn.rfiles\\" + ModuleName);
		_ribbon.ActivateModule(ModuleName, _ribbonModuleName);
		_toolbars.ActivateModule(ModuleName, _ribbon.Is3D());
		_centerMenuHelper.ActivateModule(ModuleName);
		_shortcuts.ActivateModule(ModuleName);
	//	_exeFlow.setup2D3D.screenAddOnManager.Update();
	}
}
