using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;

namespace WiCAM.Pn4000.PN3D.DeveloperTests;

public class PopupTestModelView : PopupViewModelBase
{
	private PopupTestModel model;

	public ICommand Option1Click { get; set; }

	public ICommand Option2Click { get; set; }

	public ICommand Option3Click { get; set; }

	public ICommand Option4Click { get; set; }

	public PopupTestModelView(PopupTestModel model)
	{
		this.model = model;
		model.SelectedPopupId = 0;
		base.Button0_DeleteVisibility = Visibility.Collapsed;
		base.Button1_EditVisibility = Visibility.Collapsed;
		base.Button2_AddVisibility = Visibility.Collapsed;
		base.Button8_CopyVisibility = Visibility.Collapsed;
		base.Button4_SwapVisibility = Visibility.Collapsed;
		base.Button6_MarkVisibility = Visibility.Collapsed;
		base.Button9_ListVisibility = Visibility.Collapsed;
		base.Button10_GraphicsVisibility = Visibility.Collapsed;
		base.Button11_LoadingVisibility = Visibility.Collapsed;
		base.Button12_Certain1Visibility = Visibility.Collapsed;
		base.Button13_Certain2Visibility = Visibility.Collapsed;
		base.Button14_Certain3Visibility = Visibility.Collapsed;
		base.Button15_PrintVisibility = Visibility.Collapsed;
		base.Button7_LearnVisibility = Visibility.Collapsed;
		base.Button3_BackVisibility = Visibility.Collapsed;
		base.Button16_OkVisibility = Visibility.Collapsed;
		base.Button5_CancelClick = new RelayCommand<object>(CancelClick, CanCancelClick);
		this.Option1Click = new RelayCommand<object>(OnOption1Click, CanOption1Click);
		this.Option2Click = new RelayCommand<object>(OnOption2Click, CanOption2Click);
		this.Option3Click = new RelayCommand<object>(OnOption3Click, CanOption3Click);
		this.Option4Click = new RelayCommand<object>(OnOption4Click, CanOption4Click);
	}

	private bool CanOption1Click(object obj)
	{
		return true;
	}

	private void OnOption1Click(object obj)
	{
		this.model.SelectedPopupId = 1;
		base.CloseView();
	}

	private bool CanOption2Click(object obj)
	{
		return true;
	}

	private void OnOption2Click(object obj)
	{
		this.model.SelectedPopupId = 2;
		base.CloseView();
	}

	private bool CanOption3Click(object obj)
	{
		return true;
	}

	private void OnOption3Click(object obj)
	{
		this.model.SelectedPopupId = 3;
		base.CloseView();
	}

	private bool CanOption4Click(object obj)
	{
		return false;
	}

	private void OnOption4Click(object obj)
	{
	}

	private bool CanCancelClick(object obj)
	{
		return true;
	}

	private void CancelClick(object obj)
	{
		base.CloseView();
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
	}
}
