using System;
using System.Collections.Generic;
using System.IO;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;

namespace WiCAM.Pn4000.pn4.pn4FlowCenter;

public class Shortcuts
{
	private string _moduleName = string.Empty;

	public List<IRFileRecord> List;

	private readonly IPnPathService _pnPathService;

	private readonly ExeFlow _exeFlow;

	private readonly ILogCenterService _logCenterService;

	public void ActivateModule(string moduleName)
	{
		if (_moduleName != moduleName)
		{
			SaveCfg();
		}
		_moduleName = moduleName;
		LoadCfg();
		if (List == null)
		{
			List = new List<IRFileRecord>();
		}
	}

	public Shortcuts(ILogCenterService logCenter, IPnPathService pathService, ExeFlow exeFlow)
	{
		_logCenterService = logCenter;
		_pnPathService = pathService;
		_exeFlow = exeFlow;
	}

	private void LoadCfg()
	{
		List = RFiles.ReadFile(_moduleName, "shortcuts", _pnPathService);
	}

	public void SaveCfg()
	{
		if (_moduleName == null || _moduleName == string.Empty)
		{
			return;
		}
		string path = "pn.rfiles\\" + _moduleName + "\\shortcuts";
		try
		{
			StreamWriter streamWriter = new StreamWriter(path);
			foreach (IRFileRecord item in List)
			{
				streamWriter.WriteLine(item.GetOutputText());
			}
			streamWriter.Close();
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
	}

	public void Call(int id)
	{
		string text = ((char)id).ToString();
		foreach (IRFileRecord item in List)
		{
			if (text == item.LetterShortcut)
			{
				_exeFlow.CallPnCommand(new PnCommand(item.FunctionGroup, item.FunctionName, item.DefaultLabel, item.IdLabel, item.IdHelp));
				break;
			}
		}
	}
}
