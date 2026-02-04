using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.LogCenterServices.Enum;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public partial class ErrorPopup : global::System.Windows.Controls.Primitives.Popup, IErrorPopup, IComponentConnector
{
	private readonly ILogCenterService _logCenterService;

	public Action<string> ToKernelKeyDown;

	public ErrorPopup(ILogCenterService logCenterService)
	{
		this._logCenterService = logCenterService;
		this._logCenterService.OnError += HandleOnError;
		this._logCenterService.OnWarning += HandleOnWarning;
		this.InitializeComponent();
		base.StaysOpen = false;
		base.IsOpen = false;
	}

	private void HandleOnWarning(string obj)
	{
		this.AddErrorInfo(ErrorLevel.Warning, obj);
	}

	private void HandleOnError(string obj)
	{
		this.AddErrorInfo(ErrorLevel.Error, obj);
	}

	public void AddErrorInfo(ErrorLevel errorLevel, string text)
	{
		if (!(text == string.Empty))
		{
			base.Dispatcher.BeginInvoke((Action)delegate
			{
				this.InfoList.Items.Insert(0, text);
			});
		}
	}

	private void Send_Click(object sender, RoutedEventArgs e)
	{
		base.IsOpen = false;
		if (this.ToKernelKeyDown != null)
		{
			this.ToKernelKeyDown("118 1 65");
		}
	}

	private void InfoList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (this.InfoList.SelectedItem != null)
		{
			PnExternalCall.Start($"notepad.exe {this.InfoList.SelectedItem.ToString()}", null);
		}
	}
}
