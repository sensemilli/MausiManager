using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.WpfControls.Extensions;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditBendOrderPopup;

internal class EditOrderListItemViewModel : INotifyPropertyChanged
{
	public enum Status
	{
		Unchanged,
		Merged,
		Split,
		Neither
	}

	private int _order;

	private SolidColorBrush? _currentProgressColor;

	private bool _canMerge;

	private string _orderUi;

	private readonly ModelColors3DConfig _modelColors3D;

	private Status _changeStatus;

	public ObservableCollection<EditOrderInnerListItemViewModel> BendZones { get; set; }

	public int Order
	{
		get
		{
			return _order;
		}
		private set
		{
			_order = value;
		}
	}

	public string OrderUi
	{
		get
		{
			return _orderUi;
		}
		set
		{
			if (!(value == _orderUi))
			{
				_orderUi = value;
				OnPropertyChanged("OrderUi");
			}
		}
	}

	public string DragDesc => (Order + 1).ToString();

	public bool CanMerge
	{
		get
		{
			return _canMerge;
		}
		set
		{
			if (value != _canMerge)
			{
				_canMerge = value;
				OnPropertyChanged("CanMerge");
				OnPropertyChanged("MergeDash");
			}
		}
	}

	public DoubleCollection MergeDash
	{
		get
		{
			if (!CanMerge)
			{
				return new DoubleCollection();
			}
			return new DoubleCollection { 10.0, 30.0 };
		}
	}

	public bool HoveredModel
	{
		get
		{
			return BendZones.All((EditOrderInnerListItemViewModel x) => x.HoveredModel);
		}
		set
		{
			foreach (EditOrderInnerListItemViewModel bendZone in BendZones)
			{
				bendZone.HoveredModel = value;
			}
		}
	}

	public bool HoveredBillboards
	{
		get
		{
			return BendZones.All((EditOrderInnerListItemViewModel x) => x.HoveredBillboard);
		}
		set
		{
			foreach (EditOrderInnerListItemViewModel bendZone in BendZones)
			{
				bendZone.HoveredBillboard = value;
			}
		}
	}

	public Status ChangeStatus
	{
		get
		{
			return _changeStatus;
		}
		set
		{
			if (value != _changeStatus)
			{
				_changeStatus = value;
				OnPropertyChanged("ChangeStatus");
				OnPropertyChanged("BackgroundColor");
			}
		}
	}

	public SolidColorBrush? BackgroundColor => _changeStatus switch
	{
		Status.Unchanged => null, 
		Status.Merged => new SolidColorBrush(_modelColors3D.EditOrderMergeColor.ToWpfColor()), 
		Status.Split => new SolidColorBrush(_modelColors3D.EditOrderSplitColor.ToWpfColor()), 
		Status.Neither => new SolidColorBrush(_modelColors3D.EditOrderAmbiguousColor.ToWpfColor()), 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	public SolidColorBrush? CurrentProgressColor
	{
		get
		{
			return _currentProgressColor;
		}
		set
		{
			if (value != _currentProgressColor)
			{
				_currentProgressColor = value;
				OnPropertyChanged("CurrentProgressColor");
			}
		}
	}

	public string AngleChange { get; set; }

	public string LengthTotalWithGaps { get; set; }

	public string LengthTotalWithoutGaps { get; set; }

	public ICommand CmdSetCurrentBend { get; }

	public ICommand CmdMerge { get; }

	public EditOrderViewModel ViewModel { get; private set; }

	public double Opacity
	{
		get
		{
			if (!ViewModel.Doc.CombinedBendDescriptors[Order].IsIncluded)
			{
				return 0.6;
			}
			return 1.0;
		}
	}

	public bool AngleNegative { get; set; }

	public event PropertyChangedEventHandler? PropertyChanged;

	public EditOrderListItemViewModel(EditOrderViewModel viewModel, int order, string orderUi, ModelColors3DConfig modelColors3D)
	{
		ViewModel = viewModel;
		_orderUi = orderUi;
		_modelColors3D = modelColors3D;
		_order = order;
		CmdSetCurrentBend = new RelayCommand(Execute);
		CmdMerge = new RelayCommand(Merge, () => CanMerge);
	}

	public void Merge()
	{
		ViewModel.Merge(Order);
	}

	private void Execute()
	{
		ViewModel.CurrentBentProgress = Order + 1;
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}
