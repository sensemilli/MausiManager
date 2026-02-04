using System;
using System.Windows.Media;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;

public class BendFaceViewModel : ViewModelBase
{
	private bool _isNotFirstItem;

	private bool _isCombined;

	private bool _isSplitted;

	private bool _changed;

	private Brush _color;

	public Brush Color
	{
		get
		{
			return this._color;
		}
		set
		{
			if (this._color != value)
			{
				this._color = value;
				base.NotifyPropertyChanged("Color");
			}
		}
	}

	public IBendDescriptor BendFace { get; set; }

	public ICombinedBendDescriptorInternal ParentCommonBend { get; set; }

	public bool IsNotFirstItem
	{
		get
		{
			return this._isNotFirstItem;
		}
		set
		{
			this._isNotFirstItem = value;
			base.NotifyPropertyChanged("IsNotFirstItem");
		}
	}

	public bool IsCombined
	{
		get
		{
			return this._isCombined;
		}
		set
		{
			this._isCombined = value;
			if (!this._changed)
			{
				this._changed = true;
				this.IsSplitted = !value;
			}
			this._changed = false;
			base.NotifyPropertyChanged("IsCombined");
		}
	}

	public bool IsSplitted
	{
		get
		{
			return this._isSplitted;
		}
		set
		{
			this._isSplitted = value;
			if (!this._changed)
			{
				this._changed = true;
				this.IsCombined = !value;
			}
			this._changed = false;
			base.NotifyPropertyChanged("IsSplitted");
		}
	}

	public double Radius => this.BendFace.BendParams.FinalRadius;

	public double AngleDeg => this.BendFace.BendParams.Angle * 180.0 / Math.PI;

	public double TotalLength => this.BendFace.BendParams.Length;

	public BendFaceViewModel(IBendDescriptor bendFace, ICombinedBendDescriptorInternal parentcommonBend, bool isNotFirstItem, bool isCombined)
	{
		this.BendFace = bendFace;
		this.ParentCommonBend = parentcommonBend;
		this.IsNotFirstItem = isNotFirstItem;
		this.IsCombined = isCombined;
	}
}
