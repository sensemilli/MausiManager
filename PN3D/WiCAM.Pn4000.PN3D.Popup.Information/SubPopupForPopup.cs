using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.Enums;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.View;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.Popup.Enums;
using WiCAM.Services.ConfigProviders.Contracts.DataType;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class SubPopupForPopup : ISubPopupForPopup
{
	private IMainWindowDataProvider _main;

	private readonly IModelFactory _modelFactory;

	private Window _parrent;

	public SubPopupForPopup(IMainWindowDataProvider main, IModelFactory modelFactory)
	{
		this._main = main;
		this._modelFactory = modelFactory;
	}

	public ISubPopupForPopup Init(object parent)
	{
		this._parrent = parent as Window;
		return this;
	}

	public SubPopupForPopup Init(Window parent)
	{
		this._parrent = parent;
		return this;
	}

	public bool SelectBendColor(ref global::WiCAM.Pn4000.BendModel.Base.Color color)
	{
		ColorDialog colorDialog = new ColorDialog
		{
			Color = global::System.Drawing.Color.FromArgb((byte)(color.R * 255f), (byte)(color.G * 255f), (byte)(color.B * 255f)),
			AllowFullOpen = true
		};
		if (colorDialog.ShowDialog(this._main as IWin32Window) == DialogResult.OK)
		{
			color.R = (float)(int)colorDialog.Color.R / 255f;
			color.G = (float)(int)colorDialog.Color.G / 255f;
			color.B = (float)(int)colorDialog.Color.B / 255f;
			return true;
		}
		return false;
	}

	public bool SelectCfgColor(CfgColor color)
	{
		ColorDialog colorDialog = new ColorDialog
		{
			Color = global::System.Drawing.Color.FromArgb((byte)(color.R * 255f), (byte)(color.G * 255f), (byte)(color.B * 255f)),
			AllowFullOpen = true
		};
		if (colorDialog.ShowDialog(this._main as IWin32Window) == DialogResult.OK)
		{
			color.R = (float)(int)colorDialog.Color.R / 255f;
			color.G = (float)(int)colorDialog.Color.G / 255f;
			color.B = (float)(int)colorDialog.Color.B / 255f;
			return true;
		}
		return false;
	}

	public int SelectColorPnIntEdition(int pnColorId)
	{
		PopupSelectPnColorModel popupSelectPnColorModel = this._modelFactory.Resolve<PopupSelectPnColorModel>();
		popupSelectPnColorModel.PnColors = PnColors.Colors;
		PopupSelectPnColorViewModel popupSelectPnColorViewModel = this._modelFactory.Resolve<PopupSelectPnColorViewModel>().Init(popupSelectPnColorModel);
		PopupSelectPnColorView popupSelectPnColorView = this._modelFactory.Resolve<PopupSelectPnColorView>();
		popupSelectPnColorView.DataContext = popupSelectPnColorViewModel;
		popupSelectPnColorView.Owner = this._parrent;
		popupSelectPnColorView.PopupEdition = true;
		popupSelectPnColorView.OnClosingAction = (Action<EPopupCloseReason>)Delegate.Combine(popupSelectPnColorView.OnClosingAction, new Action<EPopupCloseReason>(popupSelectPnColorViewModel.ViewCloseAction));
		popupSelectPnColorView.ShowDialog();
		(this._main as Window).Activate();
		if (popupSelectPnColorViewModel.LastSelectedColor != null)
		{
			return popupSelectPnColorViewModel.LastSelectedColor.PnNumber;
		}
		return pnColorId;
	}

	public ManualFunction ManualFunctionEdit()
	{
		return this.ManualFunctionEdit(null);
	}

	public ManualFunction ManualFunctionEdit(ManualFunction function)
	{
		ManualAddFunction manualAddFunction = new ManualAddFunction();
		if (function != null)
		{
			manualAddFunction.FunctionGroup = function.FunctionGroup;
			manualAddFunction.FunctionName = function.FunctionName;
			manualAddFunction.Label = function.Label;
		}
		manualAddFunction.Owner = this._parrent;
		manualAddFunction.ShowDialog();
		(this._main as Window).Activate();
		if (manualAddFunction.DialogResult.HasValue && manualAddFunction.DialogResult.Value)
		{
			return new ManualFunction
			{
				FunctionGroup = manualAddFunction.FunctionGroup,
				FunctionName = manualAddFunction.FunctionName,
				Label = manualAddFunction.Label
			};
		}
		return null;
	}

	public string[] OpenFile(string searchPath, string extension, bool isMultiselect, string fileFilter)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.DefaultExt = extension;
		openFileDialog.Multiselect = isMultiselect;
		if (!string.IsNullOrEmpty(fileFilter))
		{
			openFileDialog.Filter = fileFilter;
		}
		if (!string.IsNullOrEmpty(searchPath))
		{
			openFileDialog.InitialDirectory = searchPath;
		}
		if (openFileDialog.ShowDialog(this._main as IWin32Window) == DialogResult.OK)
		{
			return openFileDialog.FileNames;
		}
		return null;
	}

	public MessageBoxResult ShowMessageBox(string Message, string Caption, MessageBoxButton Buttons, MessageBoxImage Image)
	{
		return global::System.Windows.MessageBox.Show(this._main as Window, Message, Caption, Buttons, Image);
	}
}
