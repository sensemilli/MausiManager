using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Encodings;
using WiCAM.Pn4000.pn4.pn4FlowCenter;

namespace WiCAM.Pn4000.pn4.pn4Services;

public class LanguageDictionary : ILanguageDictionary
{
	private readonly IPKernelFlowGlobalDataService _pKernelFlowGlobalData;

	private readonly ILogCenterService _logCenterService;

	private readonly IUnitConverter _unitConverter;

	private readonly IMessageLogGlobal _messageLogGlobal;

	private readonly List<int> _ribbonTabOff;

	private int _inchMode;

	private int _viewer;

	private Dictionary<string, string> _msg2Int;

	private Dictionary<int, string> _pipbas;

	private Dictionary<int, string> _piphlp;

	private Dictionary<int, string> _pophlp;

	private int _currentLanguage;

	public List<PipLstEntry> PipLst { get; set; } = new List<PipLstEntry>();

	public int CurrentLanguage
	{
		get
		{
			return this._currentLanguage;
		}
		internal set
		{
			this._currentLanguage = value;
			this.OnLanguageChanged?.Invoke(value);
		}
	}

	public event Action<int> OnLanguageChanged;

	public LanguageDictionary(IPKernelFlowGlobalDataService pKernelFlowGlobalData, ILogCenterService logCenterService, ITranslator translator, IUnitConverter unitConverter, IMessageLogGlobal messageLogGlobal)
	{
		this._logCenterService = logCenterService;
		this._unitConverter = unitConverter;
		this._messageLogGlobal = messageLogGlobal;
		this._ribbonTabOff = new List<int>();
		this.SetCurrentPnLanguageId();
		this.LoadCurrentLanguageConfiguration();
		this._pKernelFlowGlobalData = pKernelFlowGlobalData;
		translator.SetFallbackTranslation(TryGetMsg2Int);
	}

	public bool ChangeActiveLanguage()
	{
		int currentLanguage = this.CurrentLanguage;
		this.SetCurrentPnLanguageId();
		if (currentLanguage == this.CurrentLanguage)
		{
			return false;
		}
		this.LoadCurrentLanguageConfiguration();
		return true;
	}

	public int GetCurrentLanguage()
	{
		return this.CurrentLanguage;
	}

	public int GetInchMode()
	{
		return this._inchMode;
	}

	public int GetViewer()
	{
		return this._viewer;
	}

	public int SetCurrentPnLanguageId()
	{
		if (this._pKernelFlowGlobalData == null)
		{
			return -1;
		}
		if (!File.Exists("MSGTXT"))
		{
			this.CurrentLanguage = 2;
			return this.CurrentLanguage;
		}
		try
		{
			string[] array = File.ReadAllLines("MSGTXT");
			this.CurrentLanguage = Convert.ToInt32(array[0].Substring(0, 4).Trim());
			this._inchMode = Convert.ToInt32(array[1].Substring(0, 4).Trim());
			this._viewer = Convert.ToInt32(array[2].Substring(0, 4).Trim());
			this._unitConverter.SetLengthUnit((this._inchMode == 1) ? LengthUnits.inch : LengthUnits.mm);
			this._pKernelFlowGlobalData.PnLanguage = this.CurrentLanguage;
			this._pKernelFlowGlobalData.PnViewerMode = this._viewer;
			this._pKernelFlowGlobalData.PnInchMode = this._inchMode;
			this._ribbonTabOff.Clear();
			for (int i = 3; i < array.Count(); i++)
			{
				try
				{
					if (array[i].Substring(5, 6) == "Ribbon")
					{
						int item = Convert.ToInt32(array[i].Substring(0, 4).Trim());
						this._ribbonTabOff.Add(item);
					}
				}
				catch (Exception e)
				{
					this._logCenterService.CatchRaport(e);
				}
			}
		}
		catch (Exception e2)
		{
			this._logCenterService.CatchRaport(e2);
		}
		return this.CurrentLanguage;
	}

	private void LoadCurrentLanguageConfiguration()
	{
		string text = $"{this.CurrentLanguage:00}";
		if (this.CurrentLanguage == 0)
		{
			return;
		}
		try
		{
			this._pophlp = new Dictionary<int, string>();
			this.AddPophlp(Environment.GetEnvironmentVariable("PNDRIVE") + "\\u\\pn\\lfiles\\" + text + "\\POPHLP");
			this.AddPophlp(Environment.GetEnvironmentVariable("PNDRIVE") + "\\u\\pn\\lfiles\\00\\POPHLP");
		}
		catch (Exception e)
		{
			this._logCenterService.CatchRaport(e);
		}
		try
		{
			this._msg2Int = new Dictionary<string, string>();
			this.AddMsgint(Environment.GetEnvironmentVariable("PNDRIVE") + "\\u\\pn\\lfiles\\" + text + "\\MSG2INT");
			this.AddMsgint(Environment.GetEnvironmentVariable("PNDRIVE") + "\\u\\pn\\lfiles\\00\\MSG2INT");
		}
		catch (Exception e2)
		{
			this._logCenterService.CatchRaport(e2);
		}
		try
		{
			this._pipbas = new Dictionary<int, string>();
			this.AddPipbas(Environment.GetEnvironmentVariable("PNDRIVE") + "\\u\\pn\\lfiles\\" + text + "\\PIPBAS");
			this.AddPipbas(Environment.GetEnvironmentVariable("PNDRIVE") + "\\u\\pn\\lfiles\\00\\PIPBAS");
		}
		catch (Exception e3)
		{
			this._logCenterService.CatchRaport(e3);
		}
		try
		{
			this._piphlp = new Dictionary<int, string>();
			this.AddPiphlp(Environment.GetEnvironmentVariable("PNDRIVE") + "\\u\\pn\\lfiles\\" + text + "\\PIPHLP");
			this.AddPiphlp(Environment.GetEnvironmentVariable("PNDRIVE") + "\\u\\pn\\lfiles\\00\\PIPHLP");
		}
		catch (Exception e4)
		{
			this._logCenterService.CatchRaport(e4);
		}
		try
		{
			this.PipLst = new List<PipLstEntry>();
			string[] array = File.ReadAllLines(Environment.GetEnvironmentVariable("PNDRIVE") + "\\u\\pn\\lfiles\\" + text + "\\PIPLST", CurrentEncoding.SystemEncoding);
			foreach (string text2 in array)
			{
				try
				{
					if (text2.Length > 8 && text2[0] != '#')
					{
						PipLstEntry pipLstEntry = new PipLstEntry(text2, this._logCenterService);
						string pipbas = this.GetPipbas(pipLstEntry.Id1);
						if (pipbas != null)
						{
							pipLstEntry.CurrentText = pipbas;
						}
						this.PipLst.Add(pipLstEntry);
					}
				}
				catch (Exception e5)
				{
					this._logCenterService.CatchRaport(e5);
				}
			}
		}
		catch (Exception e6)
		{
			this._logCenterService.CatchRaport(e6);
		}
	}

	private void AddPiphlp(string filename)
	{
		if (!File.Exists(filename))
		{
			return;
		}
		try
		{
			string[] array = File.ReadAllLines(filename, CurrentEncoding.SystemEncoding);
			int num = -1;
			int num2 = 0;
			string[] array2 = array;
			foreach (string text in array2)
			{
				num2++;
				try
				{
					if (text.Trim() == string.Empty && num >= 0)
					{
						this._piphlp[num] = this._piphlp[num] + "\n\n";
					}
					else
					{
						if (text.Length <= 8)
						{
							continue;
						}
						if (text[0] != '#' && text[0] != '=')
						{
							if (text.Substring(0, 7) == "       ")
							{
								if (num >= 0)
								{
									this._piphlp[num] = this._piphlp[num] + text.Substring(8).Trim() + " ";
								}
								continue;
							}
							num = Convert.ToInt32(text.Substring(0, 7).Trim());
							if (this._piphlp.ContainsKey(num))
							{
								this._piphlp.Remove(num);
							}
							this._piphlp.Add(num, text.Substring(8).Trim() + " ");
						}
						else
						{
							num = -1;
						}
						continue;
					}
				}
				catch (Exception e)
				{
					this._logCenterService.CatchRaport(e);
					this._messageLogGlobal.ShowErrorMessage("corrupt file: '" + filename + "'" + Environment.NewLine + $"line {num2}");
				}
			}
		}
		catch (Exception e2)
		{
			this._logCenterService.CatchRaport(e2);
		}
	}

	private void AddPipbas(string filename)
	{
		if (!File.Exists(filename))
		{
			return;
		}
		try
		{
			string[] array = File.ReadAllLines(filename, CurrentEncoding.SystemEncoding);
			foreach (string text in array)
			{
				try
				{
					if (text.Length > 8 && text[0] != '#' && text[0] != '=')
					{
						int key = Convert.ToInt32(text.Substring(0, 7).Trim());
						if (this._pipbas.ContainsKey(key))
						{
							this._pipbas.Remove(key);
						}
						this._pipbas.Add(key, text.Substring(8).Trim());
					}
				}
				catch (Exception e)
				{
					this._logCenterService.CatchRaport(e);
				}
			}
		}
		catch (Exception e2)
		{
			this._logCenterService.CatchRaport(e2);
		}
	}

	private void AddMsgint(string filename)
	{
		if (!File.Exists(filename))
		{
			return;
		}
		try
		{
			string[] array = File.ReadAllLines(filename, CurrentEncoding.SystemEncoding);
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('#');
				if (array2.Length == 2)
				{
					if (this._msg2Int.ContainsKey(array2[0]))
					{
						this._msg2Int.Remove(array2[0]);
					}
					this._msg2Int.Add(array2[0], array2[1]);
				}
			}
		}
		catch (Exception e)
		{
			this._logCenterService.CatchRaport(e);
		}
	}

	private void AddPophlp(string filename)
	{
		if (!File.Exists(filename))
		{
			return;
		}
		try
		{
			string[] array = File.ReadAllLines(filename, CurrentEncoding.SystemEncoding);
			int num = -1;
			string[] array2 = array;
			foreach (string text in array2)
			{
				string text2 = text.Trim();
				if (text2.Length > 0 && text2[0] != '#' && text.Length > 3 && text[2] >= '0' && text[2] <= '9')
				{
					try
					{
						int num2 = text2.IndexOf(' ');
						if (num2 <= 0)
						{
							this._pophlp.Add(Convert.ToInt32(text2), "/n/r");
							continue;
						}
						int num3 = Convert.ToInt32(text2.Substring(0, num2));
						string value = text2.Substring(num2 + 1);
						if (this._pophlp.ContainsKey(num3))
						{
							this._pophlp.Remove(num3);
						}
						this._pophlp.Add(num3, value);
						num = num3;
					}
					catch (Exception e)
					{
						this._logCenterService.CatchRaport(e);
					}
				}
				else
				{
					string text3 = string.Empty;
					if (text2.Length == 0)
					{
						text3 = "\n";
					}
					else if (text2[0] != '#')
					{
						text3 = "\n" + text2;
					}
					if (text3 != string.Empty && num > 0)
					{
						this._pophlp[num] = this._pophlp[num] + text3;
					}
				}
			}
		}
		catch (Exception e2)
		{
			this._logCenterService.CatchRaport(e2);
		}
	}

	public string GetPophlp(int idx)
	{
		if (this._pophlp == null)
		{
			return null;
		}
		if (this._pophlp.ContainsKey(idx))
		{
			return this._pophlp[idx];
		}
		return null;
	}

	public bool TryGetMsg2Int(string key, out string result)
	{
		Dictionary<string, string> msg2Int = this._msg2Int;
		if (msg2Int != null && msg2Int.TryGetValue(key, out result))
		{
			return true;
		}
		result = null;
		return false;
	}

	public string GetMsg2Int(string key)
	{
		if (this._msg2Int == null)
		{
			return key;
		}
		if (this._msg2Int.ContainsKey(key))
		{
			return this._msg2Int[key];
		}
		return key;
	}

	public string GetPipbas(int id)
	{
		if (id != 0 && this._pipbas.ContainsKey(id))
		{
			return this._pipbas[id];
		}
		return null;
	}

	public string GetPiphlp(int id)
	{
		if (id != 0 && this._piphlp.ContainsKey(id))
		{
			return this._piphlp[id];
		}
		return null;
	}

	public bool CheckRibbonTabOff(int idx)
	{
		if (idx == 0)
		{
			return false;
		}
		foreach (int item in this._ribbonTabOff)
		{
			if (item == idx)
			{
				return true;
			}
		}
		return false;
	}

	public string GetCurrentLanguageXmlString()
	{
		return this.CurrentLanguage switch
		{
			1 => "de", 
			2 => "en", 
			3 => "fr", 
			4 => "it", 
			5 => "es", 
			6 => "pl", 
			7 => "ru", 
			8 => "jp", 
			9 => "cn", 
			10 => "tr", 
			_ => "EN", 
		};
	}

	public string GetButtonLabel(int idLabel, string defaultLabel)
	{
		string pipbas = this.GetPipbas(idLabel);
		if (pipbas != null)
		{
			return pipbas;
		}
		return defaultLabel;
	}

	public string GetButtonToolTip(int helpid, int textid, string deflabel)
	{
		string piphlp = this.GetPiphlp(helpid);
		if (piphlp != null)
		{
			return piphlp;
		}
		return this.GetButtonLabel(textid, deflabel);
	}
}
