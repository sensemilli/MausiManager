using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.PN3D.Popup.Information;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;

namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

public class PopupSelectPnColorViewModel : PopupViewModelBase
{
	private PopupPnColor _selectedColor1;

	private PopupPnColor _selectedColor2;

	private PopupPnColor _selectedColor3;

	private PopupPnColor _selectedLineColor;

	private PopupPnColor _selectedDotLineColor;

	private List<Color> _pnColors { get; set; }

	public ObservableCollection<PopupPnColor> Colors1 { get; set; }

	public ObservableCollection<PopupPnColor> Colors2 { get; set; }

	public ObservableCollection<PopupPnColor> Colors3 { get; set; }

	public ObservableCollection<PopupPnColor> LineColors { get; set; }

	public ObservableCollection<PopupPnColor> DotLineColors { get; set; }

	public PopupPnColor LastSelectedColor { get; private set; }

	public PopupPnColor SelectedColor1
	{
		get
		{
			return this._selectedColor1;
		}
		set
		{
			this._selectedColor1 = value;
			if (value != null)
			{
				this.LastSelectedColor = value;
				this.SelectedColor2 = null;
				this.SelectedColor3 = null;
				this.SelectedLineColor = null;
				this.SelectedDotLineColor = null;
			}
			this.OnPropertyChanged("SelectedColor1");
		}
	}

	public PopupPnColor SelectedColor2
	{
		get
		{
			return this._selectedColor2;
		}
		set
		{
			this._selectedColor2 = value;
			if (value != null)
			{
				this.LastSelectedColor = value;
				this.SelectedColor1 = null;
				this.SelectedColor3 = null;
				this.SelectedLineColor = null;
				this.SelectedDotLineColor = null;
			}
			this.OnPropertyChanged("SelectedColor2");
		}
	}

	public PopupPnColor SelectedColor3
	{
		get
		{
			return this._selectedColor3;
		}
		set
		{
			this._selectedColor3 = value;
			if (value != null)
			{
				this.LastSelectedColor = value;
				this.SelectedColor2 = null;
				this.SelectedColor1 = null;
				this.SelectedLineColor = null;
				this.SelectedDotLineColor = null;
			}
			this.OnPropertyChanged("SelectedColor3");
		}
	}

	public PopupPnColor SelectedLineColor
	{
		get
		{
			return this._selectedLineColor;
		}
		set
		{
			this._selectedLineColor = value;
			if (value != null)
			{
				this.LastSelectedColor = value;
				this.SelectedColor2 = null;
				this.SelectedColor3 = null;
				this.SelectedColor1 = null;
				this.SelectedDotLineColor = null;
			}
			this.OnPropertyChanged("SelectedLineColor");
		}
	}

	public PopupPnColor SelectedDotLineColor
	{
		get
		{
			return this._selectedDotLineColor;
		}
		set
		{
			this._selectedDotLineColor = value;
			if (value != null)
			{
				this.LastSelectedColor = value;
				this.SelectedColor2 = null;
				this.SelectedColor3 = null;
				this.SelectedLineColor = null;
				this.SelectedColor1 = null;
			}
			this.OnPropertyChanged("SelectedDotLineColor");
		}
	}

	public PopupSelectPnColorViewModel Init(PopupSelectPnColorModel selectMaterialModel)
	{
		this._pnColors = selectMaterialModel.PnColors;
		base.Button0_DeleteVisibility = Visibility.Collapsed;
		base.Button1_EditVisibility = Visibility.Collapsed;
		base.Button2_AddVisibility = Visibility.Collapsed;
		base.Button3_BackVisibility = Visibility.Collapsed;
		base.Button4_SwapVisibility = Visibility.Collapsed;
		base.Button5_CancelVisibility = Visibility.Visible;
		base.Button6_MarkVisibility = Visibility.Collapsed;
		base.Button7_LearnVisibility = Visibility.Collapsed;
		base.Button8_CopyVisibility = Visibility.Collapsed;
		base.Button9_ListVisibility = Visibility.Collapsed;
		base.Button10_GraphicsVisibility = Visibility.Collapsed;
		base.Button11_LoadingVisibility = Visibility.Collapsed;
		base.Button12_Certain1Visibility = Visibility.Collapsed;
		base.Button13_Certain2Visibility = Visibility.Collapsed;
		base.Button14_Certain3Visibility = Visibility.Collapsed;
		base.Button15_PrintVisibility = Visibility.Collapsed;
		base.Button16_OkVisibility = Visibility.Visible;
		base.Button5_CancelClick = new RelayCommand<object>(CancelClick, CanCancelClick);
		base.Button16_OkClick = new RelayCommand<object>(OkClick, CanOkClick);
		this.Colors1 = new ObservableCollection<PopupPnColor>();
		this.Colors2 = new ObservableCollection<PopupPnColor>();
		this.Colors3 = new ObservableCollection<PopupPnColor>();
		this.LineColors = new ObservableCollection<PopupPnColor>();
		this.DotLineColors = new ObservableCollection<PopupPnColor>();
		for (int i = 0; i < 90; i++)
		{
			if (i < 50)
			{
				if (i < 20)
				{
					this.Colors1.Add(new PopupPnColor(i + 1, this._pnColors[i]));
				}
				else if (i < 40)
				{
					this.Colors2.Add(new PopupPnColor(i + 1, this._pnColors[i]));
				}
				else
				{
					this.Colors3.Add(new PopupPnColor(i + 1, this._pnColors[i]));
				}
			}
			else if (i < 70)
			{
				this.LineColors.Add(new PopupPnColor(i + 1, this._pnColors[i]));
			}
			else
			{
				this.DotLineColors.Add(new PopupPnColor(i + 1, this._pnColors[i]));
			}
		}
		return this;
	}

	private void CancelClick(object obj)
	{
		this.LastSelectedColor = null;
		base.CloseView();
	}

	private void OkClick(object obj)
	{
		this.CloseLikeOK();
	}

	private void CloseLikeOK()
	{
		base.CloseView();
	}

	private static bool CanCancelClick(object obj)
	{
		return true;
	}

	private static bool CanOkClick(object obj)
	{
		return true;
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
		if (reason == EPopupCloseReason.EnterKey || reason == EPopupCloseReason.MouseButton3)
		{
			this.CloseLikeOK();
		}
	}
}
