using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.TrumpfConfig;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.TrumpfConfig.Die;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.Unfold.BendTable.MdbImport;
using MdbImporter;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Encodings;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;

public class TrumpfMDBImporter : TrumpfImporter, IToolImporter, IHolderImporter, IAdapterImporter
{
	public TrumpfMDBImporter(IUnitConverter unitConverter, IGlobalToolStorage toolStorage, IModelFactory modelFactory, ImportHelper importHelper, IConfigProvider configProvider)
		: base(unitConverter, toolStorage, modelFactory, importHelper, configProvider)
	{
	}

	public string GetFilterString()
	{
		return Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.TrumpfMdb") as string;
	}

	public void ImportLowerAdapters(string filePath)
	{
		ImportLowerAdapters(GetAdapterProfiles(filePath));
	}

	public void ImportUpperAdapters(string filePath)
	{
		ImportUpperAdapters(GetAdapterProfiles(filePath));
	}

	public void ImportLowerHolders(string filePath)
	{
		ImportLowerHolders(GetHolderProfiles(filePath));
	}

	public void ImportUpperHolders(string filePath)
	{
		ImportUpperHolders(GetHolderProfiles(filePath));
	}

	public void ImportDies(string filePath)
	{
		string directoryName = Path.GetDirectoryName(filePath);
		IEnumerable<object> enumerable = UniversalMdbImporter.ImportTable(filePath, "WZG_MA_GRUPPE", typeof(WZG_MA_GRUPPE));
		IEnumerable<object> source = UniversalMdbImporter.ImportTable(filePath, "WZG_MA", typeof(WZG_MA));
		IEnumerable<object> source2 = UniversalMdbImporter.ImportTable(filePath, "WZG_MA_ATTRIBUTE", typeof(WZG_MA_ATTRIBUTE));
		IEnumerable<object> enumerable2 = UniversalMdbImporter.ImportTable(filePath, "WZG_FALZ", typeof(WZG_FALZ));
		IEnumerable<object> source3 = UniversalMdbImporter.ImportTable(filePath, "WZG_FALZ_ATTRIBUTE", typeof(WZG_FALZ_ATTRIBUTE));
		List<TrumpfToolProfile> list = new List<TrumpfToolProfile>();
		foreach (object groupEntry in enumerable)
		{
			Dictionary<string, string> properties = _importHelper.GetProperties(groupEntry);
			foreach (object profileEntry in source.Where((object e) => ((WZG_MA)e).Biegeart == 2 && ((WZG_MA)e).GruppeID == ((WZG_MA_GRUPPE)groupEntry).WZG_MA_GRUPPE_ID))
			{
				Dictionary<string, string> properties2 = _importHelper.GetProperties(profileEntry);
				List<Dictionary<string, string>> list2 = source2.Where((object e) => ((WZG_MA_ATTRIBUTE)e).ID == ((WZG_MA)profileEntry).ID).Select(_importHelper.GetProperties).ToList();
				List<Dictionary<string, string>> list3 = new List<Dictionary<string, string>>();
				foreach (Dictionary<string, string> item in list2)
				{
					if (!string.IsNullOrEmpty(item["WZG_ZEICHNUNG"]) && File.Exists(directoryName + "\\" + item["WZG_ZEICHNUNG"]))
					{
						Dictionary<string, string> dictionary = new Dictionary<string, string> { 
						{
							"name",
							item["WZG_ZEICHNUNG"]
						} };
						string s = File.ReadAllText(directoryName + "\\" + item["WZG_ZEICHNUNG"]);
						s = BitConverter.ToString(CurrentEncoding.SystemEncoding.GetBytes(s)).Replace("-", "");
						dictionary.Add("data", s);
						list3.Add(dictionary);
					}
				}
				if (File.Exists(directoryName + "\\" + ((WZG_MA)profileEntry).SkizzeDetail))
				{
					Dictionary<string, string> dictionary2 = new Dictionary<string, string> { 
					{
						"name",
						((WZG_MA)profileEntry).Typ
					} };
					string s2 = File.ReadAllText(directoryName + "\\" + ((WZG_MA)profileEntry).SkizzeDetail);
					s2 = BitConverter.ToString(CurrentEncoding.SystemEncoding.GetBytes(s2)).Replace("-", "");
					dictionary2.Add("data", s2);
					list3.Add(dictionary2);
				}
				list.Add(new TrumpfToolProfile(properties, properties2, list2, list3));
			}
		}
		ImportDies(list);
		if (enumerable2 == null)
		{
			return;
		}
		List<TrumpfToolProfile> list4 = new List<TrumpfToolProfile>();
		foreach (object entry in enumerable2)
		{
			Dictionary<string, string> properties3 = _importHelper.GetProperties(entry);
			List<Dictionary<string, string>> list5 = source3.Where((object e) => ((WZG_FALZ_ATTRIBUTE)e).ID == ((WZG_FALZ)entry).ID).Select(_importHelper.GetProperties).ToList();
			List<Dictionary<string, string>> list6 = new List<Dictionary<string, string>>();
			foreach (Dictionary<string, string> item2 in list5)
			{
				if (!string.IsNullOrEmpty(item2["WZG_ZEICHNUNG"]) && File.Exists(directoryName + "\\" + item2["WZG_ZEICHNUNG"]))
				{
					Dictionary<string, string> dictionary3 = new Dictionary<string, string> { 
					{
						"name",
						item2["WZG_ZEICHNUNG"]
					} };
					string s3 = File.ReadAllText(directoryName + "\\" + item2["WZG_ZEICHNUNG"]);
					s3 = BitConverter.ToString(CurrentEncoding.SystemEncoding.GetBytes(s3)).Replace("-", "");
					dictionary3.Add("data", s3);
					list6.Add(dictionary3);
				}
			}
			if (File.Exists(directoryName + "\\" + ((WZG_FALZ)entry).SkizzeDetail))
			{
				Dictionary<string, string> dictionary4 = new Dictionary<string, string> { 
				{
					"name",
					((WZG_FALZ)entry).Typ
				} };
				string s4 = File.ReadAllText(directoryName + "\\" + ((WZG_FALZ)entry).SkizzeDetail);
				s4 = BitConverter.ToString(CurrentEncoding.SystemEncoding.GetBytes(s4)).Replace("-", "");
				dictionary4.Add("data", s4);
				list6.Add(dictionary4);
			}
			list4.Add(new TrumpfToolProfile(null, properties3, list5, list6));
		}
		TrumpfHemPart trumpfHemPart = new TrumpfHemPart(list4);
		ImportHems(trumpfHemPart.HemProfiles);
	}

	public void ImportPunches(string filePath)
	{
		string directoryName = Path.GetDirectoryName(filePath);
		IEnumerable<object> enumerable = UniversalMdbImporter.ImportTable(filePath, "WZG_OW_GRUPPE", typeof(WZG_OW_GRUPPE));
		List<object> list = UniversalMdbImporter.ImportTable(filePath, "WZG_OW", typeof(WZG_OW)).ToList();
		List<object> source = UniversalMdbImporter.ImportTable(filePath, "WZG_OW_ATTRIBUTE", typeof(WZG_OW_ATTRIBUTE)).ToList();
		List<TrumpfToolProfile> list2 = new List<TrumpfToolProfile>();
		foreach (object item in enumerable)
		{
			Dictionary<string, string> properties = _importHelper.GetProperties(item);
			foreach (object profileEntry in list)
			{
				int num = 2;
				if (profileEntry.GetType() != typeof(WZG_FALZ) && profileEntry.GetType() != typeof(WZG_WELLE))
				{
					num = (int)profileEntry.GetType().GetProperty("Biegeart")?.GetValue(profileEntry);
				}
				string text = (string)profileEntry.GetType().GetProperty("GruppeID")?.GetValue(profileEntry);
				string wZG_OW_GRUPPE_ID = ((WZG_OW_GRUPPE)item).WZG_OW_GRUPPE_ID;
				if (num != 2 || text != wZG_OW_GRUPPE_ID)
				{
					continue;
				}
				Dictionary<string, string> properties2 = _importHelper.GetProperties(profileEntry);
				List<Dictionary<string, string>> list3 = source.Where((object e) => (int)e.GetType().GetProperty("ID")?.GetValue(e) == (int)profileEntry.GetType().GetProperty("ID")?.GetValue(profileEntry)).Select(_importHelper.GetProperties).ToList();
				List<Dictionary<string, string>> list4 = new List<Dictionary<string, string>>();
				foreach (Dictionary<string, string> item2 in list3)
				{
					if (!string.IsNullOrEmpty(item2["WZG_ZEICHNUNG"]) && File.Exists(directoryName + "\\" + item2["WZG_ZEICHNUNG"]))
					{
						Dictionary<string, string> dictionary = new Dictionary<string, string> { 
						{
							"name",
							item2["WZG_ZEICHNUNG"]
						} };
						string s = File.ReadAllText(directoryName + "\\" + item2["WZG_ZEICHNUNG"]);
						s = BitConverter.ToString(CurrentEncoding.SystemEncoding.GetBytes(s)).Replace("-", "");
						dictionary.Add("data", s);
						list4.Add(dictionary);
					}
				}
				if (File.Exists(directoryName + "\\" + (string)profileEntry.GetType().GetProperty("SkizzeDetail")?.GetValue(profileEntry)))
				{
					Dictionary<string, string> dictionary2 = new Dictionary<string, string> { 
					{
						"name",
						(string)profileEntry.GetType().GetProperty("Typ")?.GetValue(profileEntry)
					} };
					string s2 = File.ReadAllText(directoryName + "\\" + (string)profileEntry.GetType().GetProperty("SkizzeDetail")?.GetValue(profileEntry));
					s2 = BitConverter.ToString(CurrentEncoding.SystemEncoding.GetBytes(s2)).Replace("-", "");
					dictionary2.Add("data", s2);
					list4.Add(dictionary2);
				}
				list2.Add(new TrumpfToolProfile(properties, properties2, list3, list4));
			}
		}
		ImportPunches(list2);
	}

	private List<TrumpfHolderProfile> GetHolderProfiles(string filePath)
	{
		IEnumerable<object> enumerable = UniversalMdbImporter.ImportTable(filePath, "WZG_HALTER", typeof(WZG_HALTER));
		IEnumerable<object> source = UniversalMdbImporter.ImportTable(filePath, "WZG_HALTER_ATTRIBUTE", typeof(WZG_HALTER_ATTRIBUTE));
		string directoryName = Path.GetDirectoryName(filePath);
		List<TrumpfHolderProfile> list = new List<TrumpfHolderProfile>();
		foreach (object profileEntry in enumerable)
		{
			Dictionary<string, string> properties = _importHelper.GetProperties(profileEntry);
			List<Dictionary<string, string>> list2 = source.Where((object e) => ((WZG_HALTER_ATTRIBUTE)e).ID == ((WZG_HALTER)profileEntry).ID).Select(_importHelper.GetProperties).ToList();
			List<Dictionary<string, string>> list3 = new List<Dictionary<string, string>>();
			foreach (Dictionary<string, string> item in list2)
			{
				if (!string.IsNullOrEmpty(item["WZG_ZEICHNUNG"]) && File.Exists(directoryName + "\\" + item["WZG_ZEICHNUNG"]))
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string> { 
					{
						"name",
						item["WZG_ZEICHNUNG"]
					} };
					string s = File.ReadAllText(directoryName + "\\" + item["WZG_ZEICHNUNG"]);
					s = BitConverter.ToString(CurrentEncoding.SystemEncoding.GetBytes(s)).Replace("-", "");
					dictionary.Add("data", s);
					list3.Add(dictionary);
				}
			}
			if (File.Exists(directoryName + "\\" + ((WZG_HALTER)profileEntry).SkizzeDetail))
			{
				Dictionary<string, string> dictionary2 = new Dictionary<string, string> { 
				{
					"name",
					((WZG_HALTER)profileEntry).Typ
				} };
				string s2 = File.ReadAllText(directoryName + "\\" + ((WZG_HALTER)profileEntry).SkizzeDetail);
				s2 = BitConverter.ToString(CurrentEncoding.SystemEncoding.GetBytes(s2)).Replace("-", "");
				dictionary2.Add("data", s2);
				list3.Add(dictionary2);
			}
			list.Add(new TrumpfHolderProfile(properties, list2, list3));
		}
		return list;
	}

	private List<TrumpfAdapterProfile> GetAdapterProfiles(string filePath)
	{
		IEnumerable<object> enumerable = UniversalMdbImporter.ImportTable(filePath, "WZG_ADAPTER", typeof(WZG_ADAPTER));
		IEnumerable<object> source = UniversalMdbImporter.ImportTable(filePath, "WZG_ADAPTER_ATTRIBUTE", typeof(WZG_ADAPTER_ATTRIBUTE));
		string directoryName = Path.GetDirectoryName(filePath);
		List<TrumpfAdapterProfile> list = new List<TrumpfAdapterProfile>();
		foreach (object profileEntry in enumerable)
		{
			Dictionary<string, string> properties = _importHelper.GetProperties(profileEntry);
			List<Dictionary<string, string>> list2 = source.Where((object e) => ((WZG_ADAPTER_ATTRIBUTE)e).ID == ((WZG_ADAPTER)profileEntry).ID).Select(_importHelper.GetProperties).ToList();
			List<Dictionary<string, string>> list3 = new List<Dictionary<string, string>>();
			foreach (Dictionary<string, string> item in list2)
			{
				if (!string.IsNullOrEmpty(item["WZG_ZEICHNUNG"]) && File.Exists(directoryName + "\\" + item["WZG_ZEICHNUNG"]))
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string> { 
					{
						"name",
						item["WZG_ZEICHNUNG"]
					} };
					string s = File.ReadAllText(directoryName + "\\" + item["WZG_ZEICHNUNG"]);
					s = BitConverter.ToString(CurrentEncoding.SystemEncoding.GetBytes(s)).Replace("-", "");
					dictionary.Add("data", s);
					list3.Add(dictionary);
				}
			}
			if (File.Exists(directoryName + "\\" + ((WZG_ADAPTER)profileEntry).SkizzeDetail))
			{
				Dictionary<string, string> dictionary2 = new Dictionary<string, string> { 
				{
					"name",
					((WZG_ADAPTER)profileEntry).Typ
				} };
				string s2 = File.ReadAllText(directoryName + "\\" + ((WZG_ADAPTER)profileEntry).SkizzeDetail);
				s2 = BitConverter.ToString(CurrentEncoding.SystemEncoding.GetBytes(s2)).Replace("-", "");
				dictionary2.Add("data", s2);
				list3.Add(dictionary2);
			}
			list.Add(new TrumpfAdapterProfile(properties, list2, list3));
		}
		return list;
	}
}
