using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PN3D.Popup.Information;

namespace WiCAM.Pn4000.PN3D.Unfold;

public class ImportMaterialMapper : IImportMaterialMapper
{
	private MaterialDictionaryData _data;

	private IPnPathService _pnPathService;

	private ILogCenterService _logCenterService;

	public ImportMaterialMapper(ILogCenterService logCenterService, IPnPathService pathService)
	{
		this._logCenterService = logCenterService;
		this._pnPathService = pathService;
		this.Load();
	}

	private void Load()
	{
		this._data = MaterialDictionaryData.Load(this._pnPathService);
	}

	private void Save()
	{
		this._data.Save(this._pnPathService, this._logCenterService);
	}

	public int GetMaterialId(string key)
	{
		string key2 = key.Trim().ToUpper();
		if (this._data.Data.ContainsKey(key2))
		{
			return this._data.Data[key2];
		}
		return -1;
	}

	public void AddMaterial(string key, int value)
	{
		string key2 = key.Trim().ToUpper();
		if (!this._data.Data.ContainsKey(key2))
		{
			this._data.Data.Add(key2, value);
			this.Save();
		}
	}

	public ObservableCollection<VisualMaterialAllianceItem> GetMvvm(IMaterialManager materials)
	{
		ObservableCollection<VisualMaterialAllianceItem> observableCollection = new ObservableCollection<VisualMaterialAllianceItem>();
		foreach (KeyValuePair<string, int> datum in this._data.Data)
		{
			VisualMaterialAllianceItem visualMaterialAllianceItem = new VisualMaterialAllianceItem();
			visualMaterialAllianceItem.CadName = datum.Key;
			int id = datum.Value;
			visualMaterialAllianceItem.PnMaterialID = id.ToString();
			IMaterialArt materialArt = materials.MaterialList.FirstOrDefault((IMaterialArt x) => x.Number == id);
			if (materialArt != null)
			{
				visualMaterialAllianceItem.PnMaterialName = materialArt.Name;
				visualMaterialAllianceItem.PnMaterialDesc = materialArt.Description;
			}
			observableCollection.Add(visualMaterialAllianceItem);
		}
		return observableCollection;
	}

	public void SetFromMvvm(IEnumerable<VisualMaterialAllianceItem> items)
	{
		this._data.Clear();
		foreach (VisualMaterialAllianceItem item in items)
		{
			this.AddMaterial(item.CadName, Convert.ToInt32(item.PnMaterialID));
		}
		this.Save();
	}
}
