using System;
using System.Collections.Generic;
using System.IO;
using pncommon.WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.pn4.pn4FlowCenter;

namespace WiCAM.Pn4000.pn4.pn4UILib.Ribbon;

public class PnRibbonDatabase
{
	public List<Tuple<RFileRecord, RFileRecord>> SplitbuttonCurrentConfig;

	private readonly IPnPathService _pnPathService;

	private readonly ILogCenterService _logCenterService;

	public PnRibbonNode Root { get; set; }

	public string LatesHeader { get; set; }

	public PnRibbonDatabase(IPnPathService pnPathService, ILogCenterService logCenterService)
	{
		_pnPathService = pnPathService;
		_logCenterService = logCenterService;
	}

	public void ImportDatabase(string module, string ribbonModuleName)
	{
		List<IRFileRecord> rfiledb = RFiles.ReadFile(ribbonModuleName, "start", _pnPathService);
		Root = new PnRibbonNode();
		RFileChildGenerate(Root, rfiledb);
		string path = "pn.rfiles\\" + module + "\\last_ribbon_tab";
		LatesHeader = string.Empty;
		if (File.Exists(path))
		{
			try
			{
				string[] array = File.ReadAllLines(path);
				LatesHeader = array[0];
			}
			catch (Exception e)
			{
				_logCenterService.CatchRaport(e);
			}
		}
		LoadSplitButtonConfig(module);
	}

	private void LoadSplitButtonConfig(string module)
	{
		string path = "pn.rfiles\\" + module + "\\SplitButtonsConfigP4";
		SplitbuttonCurrentConfig = new List<Tuple<RFileRecord, RFileRecord>>();
		if (!File.Exists(path))
		{
			return;
		}
		try
		{
			string[] array = File.ReadAllLines(path);
			for (int i = 0; i < array.Length; i += 2)
			{
				RFileRecord item = RFileRecord.FromString(array[i]);
				RFileRecord item2 = RFileRecord.FromString(array[i + 1]);
				SplitbuttonCurrentConfig.Add(new Tuple<RFileRecord, RFileRecord>(item, item2));
			}
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
	}

	public void RFileChildGenerate(IPnRibbonNode root, List<IRFileRecord> rfiledb)
	{
		foreach (IRFileRecord item in rfiledb)
		{
			PnRibbonNode pnRibbonNode = new PnRibbonNode(item, root);
			if (root.Children == null)
			{
				List<IPnRibbonNode> list2 = (root.Children = new List<IPnRibbonNode>());
			}
			root.Children.Add(pnRibbonNode);
			if (item.SubRecords != null)
			{
				RFileChildGenerate(pnRibbonNode, item.SubRecords);
			}
		}
	}
}
