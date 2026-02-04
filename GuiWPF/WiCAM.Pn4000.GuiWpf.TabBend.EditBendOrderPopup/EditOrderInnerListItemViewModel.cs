using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.WpfControls.Extensions;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditBendOrderPopup;

internal class EditOrderInnerListItemViewModel : INotifyPropertyChanged
{
	private readonly ModelColors3DConfig _modelColors3D;

	private string _id;

	private bool _hoveredModel;

	private bool _hoveredBillboard;

	private bool _bent;

	private bool _forceSingle;

	public int BendingZoneId { get; set; }

	public string Id
	{
		get
		{
			return _id;
		}
		set
		{
			if (_id != value)
			{
				_id = value;
				OnPropertyChanged("Id");
			}
		}
	}

	public bool HoveredModel
	{
		get
		{
			return _hoveredModel;
		}
		set
		{
			if (value != _hoveredModel)
			{
				_hoveredModel = value;
				OnPropertyChanged("Color");
				OnPropertyChanged("Highlight");
				OnPropertyChanged("Weight");
				ViewModel.HighlightChanged();
			}
		}
	}

	public bool HoveredBillboard
	{
		get
		{
			return _hoveredBillboard;
		}
		set
		{
			if (value != _hoveredBillboard)
			{
				_hoveredBillboard = value;
				OnPropertyChanged("Color");
				OnPropertyChanged("Highlight");
				OnPropertyChanged("Weight");
				ViewModel.HighlightChanged();
			}
		}
	}

	public bool Highlight
	{
		get
		{
			if (!HoveredBillboard)
			{
				if (HoveredModel)
				{
					return !ViewModel.Items.SelectMany((EditOrderListItemViewModel x) => x.BendZones).Any((EditOrderInnerListItemViewModel x) => x.HoveredBillboard);
				}
				return false;
			}
			return true;
		}
	}

	public SolidColorBrush? Color
	{
		get
		{
			if (!Highlight)
			{
				return new SolidColorBrush(Colors.Black);
			}
			if (Bent)
			{
				return new SolidColorBrush(_modelColors3D.EditOrderHoverAndFullyBentColor.ToWpfColor());
			}
			return new SolidColorBrush(_modelColors3D.EditOrderHoverColor.ToWpfColor());
		}
	}

	public FontWeight? Weight
	{
		get
		{
			if (!Highlight)
			{
				return FontWeights.Normal;
			}
			_ = Bent;
			return FontWeights.Black;
		}
	}

	public Model Model { get; set; }

	public EditOrderViewModel ViewModel { get; set; }

	public EditOrderListItemViewModel OuterViewModel { get; set; }

	public bool Bent
	{
		get
		{
			return _bent;
		}
		set
		{
			if (value != _bent)
			{
				_bent = value;
				OnPropertyChanged("Bent");
			}
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public EditOrderInnerListItemViewModel(ModelColors3DConfig modelColors3D)
	{
		_modelColors3D = modelColors3D;
	}

	public void HighlightChanged()
	{
		OnPropertyChanged("Color");
		OnPropertyChanged("Highlight");
		OnPropertyChanged("Weight");
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
