using System.Windows;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;

namespace WiCAM.Pn4000.PN3D.DeveloperTests;

public class PopupTest1ModelView : PopupViewModelBase
{
	private PopupTest1Model model;

	public string TextForEdit { get; set; } = "INIT TEXT";

	public int Int1Max { get; set; } = 200;

	public int IntForEdit { get; set; } = 123;

	public double Double1ForEdit { get; set; } = 123.456;

	public bool CheckBoxValue1 { get; set; } = true;

	public bool CheckBoxValue2 { get; set; }

	public bool CheckBoxValue3 { get; set; }

	public bool CheckBoxValue4 { get; set; } = true;

	public PopupTest1ModelView(PopupTest1Model model)
	{
		this.model = model;
		model.isBackClick = false;
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
		base.Button3_BackVisibility = Visibility.Visible;
		base.Button16_OkVisibility = Visibility.Visible;
		base.Button5_CancelVisibility = Visibility.Collapsed;
		base.Button16_OkClick = new RelayCommand<object>(OkClick, CanOkClick);
		base.Button3_BackClick = new RelayCommand<object>(BackClick, CanBackClick);
	}

	private bool CanBackClick(object obj)
	{
		return true;
	}

	private void BackClick(object obj)
	{
		this.model.isBackClick = true;
		base.CloseView();
	}

	private bool CanOkClick(object obj)
	{
		return true;
	}

	private void OkClick(object obj)
	{
		base.CloseView();
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
	}
}
