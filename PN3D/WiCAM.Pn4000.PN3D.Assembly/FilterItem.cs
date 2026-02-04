using WiCAM.Pn4000.BendModel.BendTools;

namespace WiCAM.Pn4000.PN3D.Assembly;

public class FilterItem
{
	private readonly IGlobals _globals;

	public bool IsChecked { get; set; }

	public PartType PartType { get; set; }

	public string ConvertedType => this._globals.LanguageDictionary.GetMsg2Int(this.PartType.ToString());

	public FilterItem(bool isChecked, PartType partType, IGlobals globals)
	{
		this._globals = globals;
		this.IsChecked = isChecked;
		this.PartType = partType;
	}
}
