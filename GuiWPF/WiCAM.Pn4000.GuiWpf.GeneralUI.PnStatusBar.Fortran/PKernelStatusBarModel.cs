using System;
using System.Collections.Generic;
using WiCAM.Pn4000.GuiContracts.PnStatusBar;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Fortran;

public class PKernelStatusBarModel : IPKernelStatusBarModel
{
	private readonly List<IPKernelStatusBarData> StatusBarRawData = new List<IPKernelStatusBarData>();

	public event Action OnStatusChanged;

	public PKernelStatusBarModel()
	{
		for (int i = 0; i < 9; i++)
		{
			StatusBarRawData.Add(new PKernelStatusBarData());
		}
	}

	public void InfoPaneUpdate()
	{
		this.OnStatusChanged?.Invoke();
	}

	public void InfoPaneSet(int id1, int id2, string text)
	{
		IPKernelStatusBarData iPKernelStatusBarData = StatusBarRawData[id1];
		iPKernelStatusBarData.isVisible = true;
		if (id2 == 0)
		{
			iPKernelStatusBarData.MainStatus = text;
			return;
		}
		while (iPKernelStatusBarData.SubStatusList.Count <= id2)
		{
			iPKernelStatusBarData.SubStatusList.Add(string.Empty);
		}
		iPKernelStatusBarData.SubStatusList[id2] = text;
	}

	public void InfoPaneClear(int id1, int id2)
	{
		if (id1 == -1 && id2 == -1)
		{
			foreach (IPKernelStatusBarData statusBarRawDatum in StatusBarRawData)
			{
				statusBarRawDatum.isVisible = false;
			}
			return;
		}
		IPKernelStatusBarData iPKernelStatusBarData = StatusBarRawData[id1];
		if (id2 == -1)
		{
			iPKernelStatusBarData.SubStatusList.Clear();
		}
		iPKernelStatusBarData.isVisible = false;
	}

	private IPKernelStatusBarData GetRawDataOrCreate(int id1)
	{
		IPKernelStatusBarData iPKernelStatusBarData;
		if (StatusBarRawData.Count > id1)
		{
			iPKernelStatusBarData = StatusBarRawData[id1];
		}
		else
		{
			iPKernelStatusBarData = new PKernelStatusBarData();
			while (StatusBarRawData.Count < id1)
			{
				StatusBarRawData.Add(null);
			}
			StatusBarRawData.Add(iPKernelStatusBarData);
		}
		return iPKernelStatusBarData;
	}

	public List<IPKernelStatusBarData> GetData()
	{
		return StatusBarRawData;
	}
}
