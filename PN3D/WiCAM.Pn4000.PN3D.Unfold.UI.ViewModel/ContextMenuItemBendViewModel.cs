using System;
using System.Windows.Input;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;

public class ContextMenuItemBendViewModel : ContextMenuItemBase
{
	private string _description1;

	private string _description2;

	private ICommand _command;

	private ICommand _command2;

	private ICommand _command3;

	private bool _canChangeAngle;

	private bool _canMerge;

	private bool _canSplit;

	private double _value;

	public string Description1
	{
		get
		{
			return this._description1;
		}
		set
		{
			this._description1 = value;
			base.NotifyPropertyChanged("Description1");
		}
	}

	public string Description2
	{
		get
		{
			return this._description2;
		}
		set
		{
			this._description2 = value;
			base.NotifyPropertyChanged("Description2");
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

	public ICommand Command2
	{
		get
		{
			return this._command2;
		}
		set
		{
			this._command2 = value;
			base.NotifyPropertyChanged("Command2");
		}
	}

	public ICommand Command3
	{
		get
		{
			return this._command3;
		}
		set
		{
			this._command3 = value;
			base.NotifyPropertyChanged("Command3");
		}
	}

	public bool CanChangeAngle
	{
		get
		{
			return this._canChangeAngle;
		}
		set
		{
			this._canChangeAngle = value;
			base.NotifyPropertyChanged("CanChangeAngle");
		}
	}

	public bool CanMerge
	{
		get
		{
			return this._canMerge;
		}
		set
		{
			this._canMerge = value;
			base.NotifyPropertyChanged("CanMerge");
		}
	}

	public bool CanSplit
	{
		get
		{
			return this._canSplit;
		}
		set
		{
			this._canSplit = value;
			base.NotifyPropertyChanged("CanSplit");
		}
	}

	public double Value
	{
		get
		{
			return this._value;
		}
		set
		{
			if (this._value != value)
			{
				this._value = value;
				base.NotifyPropertyChanged("Value");
			}
		}
	}

	public ContextMenuItemBendViewModel(Action<double> commandOk, Action command2, Action command3, string imagePath, string description1, double value, string description2)
		: base(imagePath)
	{
		ContextMenuItemBendViewModel contextMenuItemBendViewModel = this;
		this.Value = value;
		this.Command = new RelayCommand(delegate
		{
			commandOk(contextMenuItemBendViewModel.Value);
		});
		this.Command2 = new RelayCommand(delegate
		{
			command2();
		});
		this.Command3 = new RelayCommand(delegate
		{
			command3();
		});
		this.Description1 = description1;
		this.Description2 = description2;
		this.CanChangeAngle = commandOk != null;
		this.CanMerge = command2 != null;
		this.CanSplit = command3 != null;
	}
}
