using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PN3D.Popup.Information;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;

namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

public class PopupMaterialAllianceViewModel : PopupViewModelBase
{
	private VisualMaterialAllianceItem _selectedItem;

	private ObservableCollection<VisualMaterialAllianceItem> _items;

	private PopupMaterialAllianceModel popupMaterialAllianceModel;

	public ObservableCollection<VisualMaterialAllianceItem> Items
	{
		get
		{
			return this._items;
		}
		set
		{
			if (this._items != value)
			{
				this._items = value;
				this.OnPropertyChanged("Items");
			}
		}
	}

	public VisualMaterialAllianceItem SelectedItem
	{
		get
		{
			return this._selectedItem;
		}
		set
		{
			this._selectedItem = value;
			this.OnPropertyChanged("SelectedItem");
		}
	}

	public ObservableCollection<Pair<int?, string>> MaterialList { get; set; }

	public PopupMaterialAllianceViewModel(PopupMaterialAllianceModel popupMaterialAllianceModel, ITranslator translator)
	{
		this.popupMaterialAllianceModel = popupMaterialAllianceModel;
		base.Button0_DeleteVisibility = Visibility.Visible;
		base.Button5_CancelVisibility = Visibility.Visible;
		base.Button7_LearnVisibility = Visibility.Visible;
		base.Button16_OkVisibility = Visibility.Visible;
		base.Button2_AddVisibility = Visibility.Visible;
		base.Button7_LearnPath = Application.Current.FindResource("popup.Button_SavePath") as Geometry;
		base.Button7_Learn = Application.Current.FindResource("l_popup.Button7_Save") as string;
		base.Button0_DeleteClick = new RelayCommand<object>(DeleteButtonClick, CanDeleteButtonClick);
		base.Button5_CancelClick = new RelayCommand<object>(CanceButtonClick, CanCancelButtonClick);
		base.Button7_LearnClick = new RelayCommand<object>(SaveButtonClick, CanSaveButtonClick);
		base.Button16_OkClick = new RelayCommand<object>(OkButtonClick, CanButtonClick);
		base.Button2_AddClick = new RelayCommand<object>(AddNewButtonClick, CanAddNewButtonClick);
		this.FillMaterialCombo(translator);
		this.LoadCurrentData();
	}

	private void LoadCurrentData()
	{
		this.Items = this.popupMaterialAllianceModel.ImportMaterialMapper.GetMvvm(this.popupMaterialAllianceModel.materials);
	}

	private void FillMaterialCombo(ITranslator translator)
	{
		this.MaterialList = new ObservableCollection<Pair<int?, string>>();
		this.MaterialList.Add(new Pair<int?, string>(-1, translator.Translate("l_popup.PopupMaterialAssignment.Unassinged")));
		foreach (IMaterialArt material in this.popupMaterialAllianceModel.materials.MaterialList)
		{
			this.MaterialList.Add(new Pair<int?, string>(material.Number, material.Name));
		}
	}

	private bool CanAddNewButtonClick(object obj)
	{
		return true;
	}

	private bool CanButtonClick(object obj)
	{
		return true;
	}

	private void OkButtonClick(object obj)
	{
		this.SaveButtonClick(obj);
		this.CloseLikeOK();
	}

	private void CloseLikeOK()
	{
		this.SaveButtonClick(null);
		base.CloseView();
	}

	private bool CanDeleteButtonClick(object obj)
	{
		return this.SelectedItem != null;
	}

	private void DeleteButtonClick(object obj)
	{
		if (this.SelectedItem != null)
		{
			this.Items.Remove(this.SelectedItem);
		}
	}

	private bool CanCancelButtonClick(object obj)
	{
		return true;
	}

	private void CanceButtonClick(object obj)
	{
		this.popupMaterialAllianceModel.isCancel = true;
		base.CloseView();
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
		if (reason == EPopupCloseReason.EnterKey || reason == EPopupCloseReason.MouseButton3)
		{
			this.CloseLikeOK();
		}
	}

	private bool CanSaveButtonClick(object obj)
	{
		return true;
	}

	private void AddNewButtonClick(object obj)
	{
		VisualMaterialAllianceItem visualMaterialAllianceItem = this.CreateItem();
		if (visualMaterialAllianceItem != null)
		{
			this.Items.Add(visualMaterialAllianceItem);
			this.SelectedItem = this.Items.LastOrDefault();
		}
	}

	private VisualMaterialAllianceItem CreateItem()
	{
		if (this.SelectedItem == null)
		{
			return new VisualMaterialAllianceItem();
		}
		string text = this.SelectedItem.CadName.Trim();
		if (text == string.Empty)
		{
			return new VisualMaterialAllianceItem();
		}
		return new VisualMaterialAllianceItem
		{
			CadName = text,
			PnMaterialID = this.SelectedItem.PnMaterialID,
			PnMaterialName = this.SelectedItem.PnMaterialName
		};
	}

	private void SaveButtonClick(object obj)
	{
		this.popupMaterialAllianceModel.ImportMaterialMapper.SetFromMvvm(this.Items.Where((VisualMaterialAllianceItem i) => !string.IsNullOrEmpty(i.PnMaterialID) && !string.IsNullOrEmpty(i.CadName)));
	}
}
