using System.Collections.Generic;
using System.Linq;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

public class PopupMaterialMappingViewModel
{
	public class MappingEntry : ViewModelBase
	{
		private int _matPn;

		public string MatImport { get; }

		public int MatPn
		{
			get
			{
				return this._matPn;
			}
			set
			{
				if (this._matPn != value)
				{
					this._matPn = value;
					base.RaisePropertyChanged("MatPn");
				}
			}
		}

		public int Count { get; }

		public MappingEntry(string matImport, int matPn, int count)
		{
			this.MatImport = matImport;
			this.MatPn = matPn;
			this.Count = count;
		}
	}

	public List<MappingEntry> Mappings { get; }

	public List<KeyValuePair<int, string>> MatMapping { get; }

	public PopupMaterialMappingViewModel(List<(string matImport, int matPn, int count)> listMatsConvert, Dictionary<int, string> material3DGroup, ITranslator translator)
	{
		this.MatMapping = new List<KeyValuePair<int, string>>();
		this.MatMapping.Add(new KeyValuePair<int, string>(-1, translator.Translate("l_popup.PopupMaterialMappingView.SkipEntries")));
		this.MatMapping.AddRange(material3DGroup);
		this.Mappings = listMatsConvert.Select(((string matImport, int matPn, int count) x) => new MappingEntry(x.matImport, x.matPn, x.count)).ToList();
	}

	public Dictionary<string, int> GetResult()
	{
		return this.Mappings.ToDictionary((MappingEntry x) => x.MatImport, (MappingEntry x) => x.MatPn);
	}
}
