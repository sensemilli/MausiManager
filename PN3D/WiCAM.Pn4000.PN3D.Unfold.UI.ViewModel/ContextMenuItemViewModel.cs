using System;
using System.Windows.Input;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;

public class ContextMenuItemViewModel : ContextMenuItemBase
{
	private string _buttonContent;

	private ICommand _command;

	public string ButtonContent
	{
		get
		{
			return this._buttonContent;
		}
		set
		{
			this._buttonContent = value;
			base.NotifyPropertyChanged("ButtonContent");
		}
	}

	public ICommand Command
	{
		get
		{
			return this._command;
		}
		set
		{
			this._command = value;
			base.NotifyPropertyChanged("Command");
		}
	}

	public ContextMenuItemViewModel(Action command, string buttonContent, string imagePath)
		: base(imagePath)
	{
		this.Command = new RelayCommand(delegate
		{
			command();
		});
		this.ButtonContent = buttonContent;
	}
}
