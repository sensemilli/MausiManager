using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;

namespace WiCAM.Pn4000.PN3D.DeveloperTests;

public class PopupTest3ModelView : PopupViewModelBase
{
	private PopupTest3Model model;

	private List<View3DControlPart> _parts;

	private ObservableCollection<DisassemblyPart> _disassemblyParts;

	private int _selectId;

	private bool _listview = true;

	public List<View3DControlPart> Parts
	{
		get
		{
			return this._parts;
		}
		set
		{
			if (this._parts != value)
			{
				this._parts = value;
				this.OnPropertyChanged("Parts");
			}
		}
	}

	public ObservableCollection<DisassemblyPart> DisassemblyParts
	{
		get
		{
			return this._disassemblyParts;
		}
		set
		{
			if (this._disassemblyParts != value)
			{
				this._disassemblyParts = value;
				this.OnPropertyChanged("DisassemblyParts");
			}
		}
	}

	public int SelectID
	{
		get
		{
			return this._selectId;
		}
		set
		{
			if (this._selectId != value)
			{
				this._selectId = value;
				this.OnPropertyChanged("SelectID");
			}
		}
	}

	public ICommand DisassemblyPartCommand1 { get; set; }

	public PopupTest3ModelView(PopupTest3Model model)
	{
		this.model = model;
		this.Parts = model.Parts;
		this.DisassemblyParts = model.DisassemblyParts;
		base.Button0_DeleteVisibility = Visibility.Collapsed;
		base.Button1_EditVisibility = Visibility.Collapsed;
		base.Button2_AddVisibility = Visibility.Collapsed;
		base.Button8_CopyVisibility = Visibility.Collapsed;
		base.Button4_SwapVisibility = Visibility.Collapsed;
		base.Button6_MarkVisibility = Visibility.Collapsed;
		base.Button13_Certain2Visibility = Visibility.Collapsed;
		base.Button14_Certain3Visibility = Visibility.Collapsed;
		base.Button3_BackVisibility = Visibility.Collapsed;
		base.Button9_ListVisibility = Visibility.Visible;
		base.Button10_GraphicsVisibility = Visibility.Visible;
		base.Button11_LoadingVisibility = Visibility.Visible;
		base.Button12_Certain1Visibility = Visibility.Visible;
		base.Button15_PrintVisibility = Visibility.Visible;
		base.Button7_LearnVisibility = Visibility.Visible;
		base.Button16_OkVisibility = Visibility.Visible;
		base.Button5_CancelVisibility = Visibility.Visible;
		base.Button10_Graphics = Application.Current.FindResource("l_popup.Button_Tree") as string;
		base.Button10_GraphicsPath = Application.Current.FindResource("popup.Button_TreePath") as Geometry;
		base.Button11_Loading = Application.Current.FindResource("l_popup.Button_Name") as string;
		base.Button11_LoadingPath = Application.Current.FindResource("popup.Button_NamePath") as Geometry;
		base.Button12_Certain1 = Application.Current.FindResource("l_popup.Button_Filter") as string;
		base.Button12_Certain1Path = Application.Current.FindResource("popup.Button_FilterPath") as Geometry;
		base.Button7_Learn = Application.Current.FindResource("l_popup.Button_Pdf") as string;
		base.Button7_LearnPath = Application.Current.FindResource("popup.Button_PdfPath") as Geometry;
		base.Button16_OkClick = new RelayCommand<object>(OkClick, CanOkClick);
		base.Button5_CancelClick = new RelayCommand<object>(CancelClick, CanCancelClick);
		base.Button9_ListClick = new RelayCommand<object>(ListClick, CanListClick);
		base.Button10_GraphicsClick = new RelayCommand<object>(TreeClick, CanTreeClick);
		base.Button11_LoadingClick = new RelayCommand<object>(NameClick, CanNameClick);
		base.Button12_Certain1Click = new RelayCommand<object>(FilterClick, CanFilterClick);
		base.Button15_PrintClick = new RelayCommand<object>(PrintClick, CanPrintClick);
		base.Button7_LearnClick = new RelayCommand<object>(PdfClick, CanPdfClick);
		this.DisassemblyPartCommand1 = new RelayCommand<object>(DisassemblyPartClickAction, DisassemblyPartCanClick);
	}

	private void DisassemblyPartClickAction(object obj)
	{
	}

	private bool DisassemblyPartCanClick(object obj)
	{
		return true;
	}

	private bool CanPdfClick(object obj)
	{
		return this._listview;
	}

	private bool CanPrintClick(object obj)
	{
		return this._listview;
	}

	private bool CanFilterClick(object obj)
	{
		return this._listview;
	}

	private bool CanNameClick(object obj)
	{
		return this._listview;
	}

	private bool CanTreeClick(object obj)
	{
		return this._listview;
	}

	private bool CanListClick(object obj)
	{
		return !this._listview;
	}

	private bool CanCancelClick(object obj)
	{
		return true;
	}

	private bool CanOkClick(object obj)
	{
		return true;
	}

	private void PdfClick(object obj)
	{
	}

	private void PrintClick(object obj)
	{
		this.SelectID = 2;
		this.Parts = null;
	}

	private void FilterClick(object obj)
	{
		this.SelectID = 1;
	}

	private void NameClick(object obj)
	{
		this.SelectID = 0;
	}

	private void TreeClick(object obj)
	{
		this._listview = false;
	}

	private void ListClick(object obj)
	{
		this._listview = true;
	}

	private void CancelClick(object obj)
	{
		base.CloseView();
	}

	private void OkClick(object obj)
	{
		base.CloseView();
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
		if ((reason == EPopupCloseReason.System || (uint)(reason - 3) <= 1u) && this.CanOkClick(null))
		{
			this.OkClick(null);
		}
	}
}
