using System.Collections.Generic;
using System.Linq;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.Legend;

internal class LegendViewModel : ILegendViewModel
{
	private class LegendEntry : ILegendViewModel.ILegendEntry
	{
		private string _value;

		public int No { get; }

		public string Desc { get; }

		public string Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (!ReadOnly)
				{
					_value = value;
				}
			}
		}

		public string Type { get; }

		public bool ReadOnly { get; }

		public LegendEntry(int no, string desc, string value, string type, bool readOnly)
		{
			No = no;
			Desc = desc;
			_value = value;
			Type = type;
			ReadOnly = readOnly;
		}
	}

	private readonly ITranslator _translator;

	private const int IdDrawingNumber = -7;

	private const int IdModifiedDate = -6;

	private const int IdModifiedUserName = -5;

	private const int IdCreateDate = -4;

	private const int IdCreateUserName = -3;

	private const int IdClassification = -2;

	private const int IdComment = -1;

	private IPnBndDoc Doc;

	public RadObservableCollection<ILegendViewModel.ILegendEntry> Entries { get; } = new RadObservableCollection<ILegendViewModel.ILegendEntry>();

	private Dictionary<int, string> DescOptional { get; }

	private Dictionary<int, string> DescRequired { get; }

	public LegendViewModel(IConfigProvider configProvider, ITranslator translator)
	{
		_translator = translator;
		UserCommentsConfig userCommentsConfig = configProvider.InjectOrCreate<UserCommentsConfig>();
		DescOptional = userCommentsConfig.UserCommentsOptional.ToDictionary<KeyValuePair<int, string>, int, string>((KeyValuePair<int, string> x) => x.Key, (KeyValuePair<int, string> x) => x.Value);
		DescRequired = userCommentsConfig.UserCommentsRequired.ToDictionary<KeyValuePair<int, string>, int, string>((KeyValuePair<int, string> x) => x.Key, (KeyValuePair<int, string> x) => x.Value);
	}

	public void Init(IPnBndDoc doc)
	{
		Doc = doc;
		string type = _translator.Translate("l_popup.Legend3D.TypeOptional");
		string type2 = _translator.Translate("l_popup.Legend3D.TypeRequired");
		string type3 = _translator.Translate("l_popup.Legend3D.TypeInfo");
		string desc = _translator.Translate("l_popup.Legend3D.Info.Comment");
		string desc2 = _translator.Translate("l_popup.Legend3D.Info.Classification");
		string desc3 = _translator.Translate("l_popup.Legend3D.Info.CreateUserName");
		string desc4 = _translator.Translate("l_popup.Legend3D.Info.CreateDate");
		string desc5 = _translator.Translate("l_popup.Legend3D.Info.ModifiedUserName");
		string desc6 = _translator.Translate("l_popup.Legend3D.Info.ModifiedDate");
		string desc7 = _translator.Translate("l_popup.Legend3D.Info.DrawingNumber");
		Entries.Add(new LegendEntry(-6, desc6, doc.LastModified.ToShortDateString(), type3, readOnly: true));
		Entries.Add(new LegendEntry(-5, desc5, doc.LastModifiedUserName, type3, readOnly: true));
		Entries.Add(new LegendEntry(-4, desc4, doc.CreationDate.ToShortDateString(), type3, readOnly: true));
		Entries.Add(new LegendEntry(-3, desc3, doc.CreationUserName, type3, readOnly: true));
		Entries.Add(new LegendEntry(-1, desc, doc.Comment, type3, readOnly: false));
		Entries.Add(new LegendEntry(-2, desc2, doc.Classification, type3, readOnly: false));
		Entries.Add(new LegendEntry(-7, desc7, doc.DrawingNumber, type3, readOnly: false));
		Dictionary<int, string> userComments = doc.UserComments;
		foreach (KeyValuePair<int, string> item in DescRequired)
		{
			if (!userComments.TryGetValue(item.Key, out var value))
			{
				value = "";
			}
			Entries.Add(new LegendEntry(item.Key, item.Value, value, type2, readOnly: false));
		}
		foreach (KeyValuePair<int, string> item2 in DescOptional)
		{
			if (!userComments.TryGetValue(item2.Key, out var value2))
			{
				value2 = "";
			}
			Entries.Add(new LegendEntry(item2.Key, item2.Value, value2, type, readOnly: false));
		}
	}

	public void Save()
	{
		Dictionary<int, string> dictionary = Entries.ToDictionary((ILegendViewModel.ILegendEntry x) => x.No, (ILegendViewModel.ILegendEntry x) => x.Value);
		Doc.Comment = dictionary[-1];
		Doc.Classification = dictionary[-2];
		Doc.DrawingNumber = dictionary[-7];
		Doc.UserComments.Clear();
		Doc.UserComments.AddRange<KeyValuePair<int, string>>(dictionary.Where((KeyValuePair<int, string> x) => x.Key >= 0));
	}
}
