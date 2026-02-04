using Telerik.Windows.Controls;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class StrategyItemViewModel : ViewModelBase
{
	private string _description;

	public BendSequenceSorts Value { get; }

	public string Description
	{
		get
		{
			return this._description;
		}
		set
		{
			if (this._description != value)
			{
				this._description = value;
				base.RaisePropertyChanged("Description");
			}
		}
	}

	public override string ToString()
	{
		return this.Description;
	}

	public StrategyItemViewModel(BendSequenceSorts value, BendSequenceEditorViewModel mainVm)
	{
		this.Value = value;
		this.Description = mainVm.SequenceItemTranslation[value].Desc;
	}
}
