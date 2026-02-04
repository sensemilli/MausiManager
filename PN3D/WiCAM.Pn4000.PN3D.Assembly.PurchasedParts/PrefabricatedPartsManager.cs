using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using WiCAM.Pn4000.Contracts.Assembly;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Assembly.PurchasedParts;

internal class PrefabricatedPartsManager : IPrefabricatedPartsManager
{
	private class PrefabricatedPart : IPrefabricatedPart
	{
		private List<(string propName, string PropertyValues)> _additionalProperties = new List<(string, string)>();

		public string Name { get; set; }

		public int Type { get; set; }

		public bool IsMountedBeforeBending { get; set; }

		public IEnumerable<(string, string)> AdditionalProperties => this._additionalProperties;

		public IPrefabricatedPart.SearchTypes NameSearchType { get; set; }

		public PrefabricatedPart()
		{
		}

		public PrefabricatedPart(string name, bool isMountedBeforeBending, int type, IPrefabricatedPart.SearchTypes searchType, IEnumerable<(string propName, string propValue)> additionalProperties)
		{
			this.Name = name;
			this.Type = type;
			this.IsMountedBeforeBending = isMountedBeforeBending;
			this.NameSearchType = searchType;
			this._additionalProperties = additionalProperties.ToList();
		}

		public void AddProperty(string propName, string propValue)
		{
			this._additionalProperties.Add((propName, propValue));
		}
	}

	private class Data
	{
		public Dictionary<string, IPrefabricatedPart> _parts = new Dictionary<string, IPrefabricatedPart>();

		public Dictionary<int, string> _typeDescriptions = new Dictionary<int, string>();
	}

	private class SerializeData
	{
		public int Version { get; set; } = 1;

		public List<SerializeTPP> Parts { get; set; }

		public List<Tuple<int, string>> TypeDescriptions { get; set; }
	}

	private class SerializeTPP
	{
		public int Type { get; set; }

		public bool IsMountedBeforeBending { get; set; }

		public List<Tuple<string, string>> AdditionalProperties { get; set; }

		public string Name { get; set; }

		public int NameSearchType { get; set; }

		public SerializeTPP()
		{
		}

		public SerializeTPP(IPrefabricatedPart part)
		{
			this.Name = part.Name;
			this.Type = part.Type;
			this.IsMountedBeforeBending = part.IsMountedBeforeBending;
			this.AdditionalProperties = part.AdditionalProperties.Select<(string, string), Tuple<string, string>>(((string propName, string propValue) x) => new Tuple<string, string>(x.propName, x.propValue)).ToList();
			this.NameSearchType = (int)part.NameSearchType;
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetPartTypesOrdered_003Ed__26 : IEnumerable<(int typeId, string typeDesc)>, IEnumerable, IEnumerator<(int typeId, string typeDesc)>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private (int typeId, string typeDesc) _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public PrefabricatedPartsManager _003C_003E4__this;

		private IEnumerator<KeyValuePair<int, string>> _003C_003E7__wrap1;

        [DebuggerHidden]
        (int typeId, string typeDesc) IEnumerator<(int typeId, string typeDesc)>.Current
        {
            get
            {
                return this._003C_003E2__current;
            }
        }

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return this._003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetPartTypesOrdered_003Ed__26(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			this._003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = this._003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					this._003C_003Em__Finally1();
				}
			}
			this._003C_003E7__wrap1 = null;
			this._003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = this._003C_003E1__state;
				PrefabricatedPartsManager prefabricatedPartsManager = this._003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					this._003C_003E1__state = -1;
					this._003C_003E7__wrap1 = prefabricatedPartsManager._typeDescriptions.OrderBy<KeyValuePair<int, string>, string>((KeyValuePair<int, string> x) => x.Value).GetEnumerator();
					this._003C_003E1__state = -3;
					break;
				case 1:
					this._003C_003E1__state = -3;
					break;
				}
				if (this._003C_003E7__wrap1.MoveNext())
				{
					KeyValuePair<int, string> current = this._003C_003E7__wrap1.Current;
					this._003C_003E2__current = (typeId: current.Key, typeDesc: current.Value);
					this._003C_003E1__state = 1;
					return true;
				}
				this._003C_003Em__Finally1();
				this._003C_003E7__wrap1 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			this._003C_003E1__state = -1;
			if (this._003C_003E7__wrap1 != null)
			{
				this._003C_003E7__wrap1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

        [DebuggerHidden]
        IEnumerator<(int typeId, string typeDesc)> IEnumerable<(int typeId, string typeDesc)>.GetEnumerator()
        {
            _003CGetPartTypesOrdered_003Ed__26 result;
            if (this._003C_003E1__state == -2 && this._003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
            {
                this._003C_003E1__state = 0;
                result = this;
            }
            else
            {
                result = new _003CGetPartTypesOrdered_003Ed__26(0)
                {
                    _003C_003E4__this = this._003C_003E4__this
                };
            }
            return result;
        }

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<(int, string)>)this).GetEnumerator();
		}
	}

	private readonly IMessageDisplay _logGlobal;

	private readonly ITranslator _translator;

	private const string GFile = "prefabricatedParts.json";

	private Data _data = new Data();

	private PurchasedPartsCfg _prefabricatedPartsCfg;

	private readonly IConfigProvider _configProvider;

	private IPnPathService _pathService;

	private List<IPrefabricatedPart>? _partsSpecialCompare;

	private List<IPrefabricatedPart> PartsSpecialCompare
	{
		get
		{
			if (this._partsSpecialCompare == null)
			{
				this._partsSpecialCompare = this._parts.Values.Where((IPrefabricatedPart x) => x.NameSearchType != IPrefabricatedPart.SearchTypes.EqualCompare).ToList();
			}
			return this._partsSpecialCompare;
		}
	}

	private Dictionary<string, IPrefabricatedPart> _parts => this._data._parts;

	private Dictionary<int, string> _typeDescriptions => this._data._typeDescriptions;

	public bool IgnoreNonHorizontalPlaneConnectedPp => this._prefabricatedPartsCfg.IgnoreNonHorizontalPlaneConnectedPp;

	public bool IsDetectionPrefabricatedPartsByNameActive => this._prefabricatedPartsCfg.IsDetectionPrefabricatedPartsByNameActive;

	public bool DetectEverythingAsPpInAssembly => this._prefabricatedPartsCfg.DetectEverythingAsPpInAssembly;

	public double MaxDistOfPurchasePartsToSheetMetal => this._prefabricatedPartsCfg.MaxDistOfPurchasePartsToSheetMetal;

	public string TypeToStr(int type)
	{
		if (!this._typeDescriptions.TryGetValue(type, out string value))
		{
			return "";
		}
		return value;
	}

	public PrefabricatedPartsManager(IConfigProvider configProvider, IPnPathService pathService, IMessageLogGlobal logGlobal, ITranslator translator)
	{
		this._logGlobal = logGlobal;
		this._translator = translator;
		this._configProvider = configProvider;
		this._prefabricatedPartsCfg = this._configProvider.InjectOrCreate<PurchasedPartsCfg>();
		this._pathService = pathService;
		this.Load();
	}

	[IteratorStateMachine(typeof(_003CGetPartTypesOrdered_003Ed__26))]
	public IEnumerable<(int typeId, string typeDesc)> GetPartTypesOrdered()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetPartTypesOrdered_003Ed__26(-2)
		{
			_003C_003E4__this = this
		};
	}

	public void SetConfig(bool ignoreNonHorizontalPlaneConnectedPp, bool isDetectionPrefabricatedPartsByNameActive, bool detectEverythingAsPpInAssembly, double maxDistOfPurchasePartsToSheetMetal)
	{
		this._prefabricatedPartsCfg.IgnoreNonHorizontalPlaneConnectedPp = ignoreNonHorizontalPlaneConnectedPp;
		this._prefabricatedPartsCfg.IsDetectionPrefabricatedPartsByNameActive = isDetectionPrefabricatedPartsByNameActive;
		this._prefabricatedPartsCfg.DetectEverythingAsPpInAssembly = detectEverythingAsPpInAssembly;
		this._prefabricatedPartsCfg.MaxDistOfPurchasePartsToSheetMetal = maxDistOfPurchasePartsToSheetMetal;
		this._configProvider.Push(this._prefabricatedPartsCfg);
		this._configProvider.Save<PurchasedPartsCfg>();
	}

	public void SetData(IEnumerable<(int typeId, string typeDesc)> types, IEnumerable<IPrefabricatedPart> parts)
	{
		this._data._typeDescriptions = types.ToDictionary<(int, string), int, string>(((int typeId, string typeDesc) x) => x.typeId, ((int typeId, string typeDesc) x) => x.typeDesc);
		this._data._parts = parts.ToDictionary((IPrefabricatedPart x) => x.Name.ToLowerInvariant(), ClonePart);
		this.Save();
	}

	void IPrefabricatedPartsManager.RemovePart(string partName)
	{
		IPrefabricatedPart prefabricatedPart = this.FindPart(partName, checkDetectionEnabled: false);
		if (prefabricatedPart != null)
		{
			this._parts.Remove(prefabricatedPart.Name);
		}
		this.Save();
	}

	void IPrefabricatedPartsManager.AddPart(IPrefabricatedPart part)
	{
		this.AddPart(this.ClonePart(part));
		this.Save();
	}

	private void AddPart(IPrefabricatedPart part)
	{
		if (!this._parts.TryAdd(part.Name.ToLowerInvariant(), part))
		{
			this._parts[part.Name.ToLowerInvariant()] = part;
		}
	}

	public IEnumerable<IPrefabricatedPart> AllParts()
	{
		return this._parts.Values;
	}

	public IPrefabricatedPart? FindPart(string name, bool checkDetectionEnabled)
	{
		if (checkDetectionEnabled && !this._prefabricatedPartsCfg.IsDetectionPrefabricatedPartsByNameActive)
		{
			return null;
		}
		if (this._parts.TryGetValue(name.ToLowerInvariant(), out IPrefabricatedPart value) && value.NameSearchType == IPrefabricatedPart.SearchTypes.EqualCompare)
		{
			return value;
		}
		foreach (IPrefabricatedPart item in this.PartsSpecialCompare)
		{
			if (item.NameSearchType == IPrefabricatedPart.SearchTypes.RegexCompare && Regex.IsMatch(name, item.Name, RegexOptions.IgnoreCase))
			{
				return item;
			}
		}
		return null;
	}

	public int AddTypeDescription(string desc)
	{
		int num = this._typeDescriptions.Keys.DefaultIfEmpty(0).Max() + 1;
		this._typeDescriptions.Add(num, desc);
		return num;
	}

	private IPrefabricatedPart ClonePart(IPrefabricatedPart part)
	{
		return new PrefabricatedPart(part.Name, part.IsMountedBeforeBending, part.Type, part.NameSearchType, part?.AdditionalProperties ?? new List<(string, string)>());
	}

	private void Save()
	{
		try
		{
			SerializeData serializeData = new SerializeData();
			serializeData.Parts = this._data._parts.Values.Select((IPrefabricatedPart x) => new SerializeTPP(x)).ToList();
			serializeData.TypeDescriptions = this._data._typeDescriptions.Select<KeyValuePair<int, string>, Tuple<int, string>>((KeyValuePair<int, string> x) => new Tuple<int, string>(x.Key, x.Value)).ToList();
			File.WriteAllText(this._pathService.GetFileInGFiles("prefabricatedParts.json"), JsonSerializer.Serialize(serializeData), Encoding.UTF8);
			this._partsSpecialCompare = null;
		}
		catch (Exception)
		{
			this._logGlobal.ShowErrorMessage(this._translator.Translate("l_popup.PrefabricatedParts.SaveError"));
		}
	}

	private void Load()
	{
		try
		{
			string fileInGFiles = this._pathService.GetFileInGFiles("prefabricatedParts.json");
			if (File.Exists(fileInGFiles))
			{
				SerializeData serializeData = JsonSerializer.Deserialize<SerializeData>(File.ReadAllText(fileInGFiles, Encoding.UTF8));
				this._data._typeDescriptions = serializeData.TypeDescriptions.ToDictionary((Tuple<int, string> x) => x.Item1, (Tuple<int, string> x) => x.Item2);
				this._data._parts = serializeData.Parts.Select((SerializeTPP x) => new PrefabricatedPart(x.Name, x.IsMountedBeforeBending, x.Type, (IPrefabricatedPart.SearchTypes)x.NameSearchType, x.AdditionalProperties.Select((Tuple<string, string> x) => (x.Item1, x.Item2)))).ToDictionary((Func<PrefabricatedPart, string>)((PrefabricatedPart x) => x.Name.ToLowerInvariant()), (Func<PrefabricatedPart, IPrefabricatedPart>)((PrefabricatedPart x) => x));
			}
			else
			{
				this._data._typeDescriptions = this._prefabricatedPartsCfg.GetDefaultPartTypes(this._translator).ToDictionary((Tuple<int, string> x) => x.Item1, (Tuple<int, string> x) => x.Item2);
				this._data._parts = new Dictionary<string, IPrefabricatedPart>();
			}
			this._partsSpecialCompare = null;
		}
		catch (Exception)
		{
			this._logGlobal.ShowErrorMessage(this._translator.Translate("l_popup.PrefabricatedParts.LoadError"));
		}
	}

	public void ImportXml(string filename)
	{
		string text = "Filename";
		string text2 = "Description1_DE";
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(filename);
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (KeyValuePair<int, string> typeDescription in this._typeDescriptions)
		{
			dictionary.TryAdd(typeDescription.Value, typeDescription.Key);
		}
		foreach (object childNode in xmlDocument.ChildNodes)
		{
			if (!(childNode is XmlNode xmlNode))
			{
				continue;
			}
			foreach (object childNode2 in xmlNode.ChildNodes)
			{
				if (!(childNode2 is XmlNode xmlNode2))
				{
					continue;
				}
				PrefabricatedPart prefabricatedPart = new PrefabricatedPart
				{
					IsMountedBeforeBending = false,
					NameSearchType = IPrefabricatedPart.SearchTypes.EqualCompare
				};
				foreach (object childNode3 in xmlNode2.ChildNodes)
				{
					if (!(childNode3 is XmlNode xmlNode3))
					{
						continue;
					}
					string name = xmlNode3.Name;
					string innerText = xmlNode3.InnerText;
					if (name == text)
					{
						prefabricatedPart.Name = innerText;
					}
					else if (name == text2)
					{
						if (!dictionary.TryGetValue(innerText, out var value))
						{
							value = this.AddTypeDescription(innerText);
							dictionary.Add(innerText, value);
						}
						prefabricatedPart.Type = value;
					}
					else
					{
						prefabricatedPart.AddProperty(name, innerText);
					}
				}
				IPrefabricatedPart prefabricatedPart2 = this.FindPart(prefabricatedPart.Name, checkDetectionEnabled: false);
				if (prefabricatedPart2 != null)
				{
					prefabricatedPart.IsMountedBeforeBending = prefabricatedPart2.IsMountedBeforeBending;
				}
				if (!string.IsNullOrEmpty(prefabricatedPart.Name))
				{
					this.AddPart(prefabricatedPart);
				}
			}
		}
	}
}
