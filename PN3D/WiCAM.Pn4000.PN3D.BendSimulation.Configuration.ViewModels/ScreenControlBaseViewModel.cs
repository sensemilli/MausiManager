using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes;

namespace WiCAM.Pn4000.PN3D.BendSimulation.Configuration.ViewModels;

public class ScreenControlBaseViewModel : ViewModelBase
{
	protected double OpacityMin = 0.8;

	protected double OpacityMax = 1.0;

	private double _opacity = 0.8;

	private double _left;

	private double _top;

	private double _width = 100.0;

	private double _height = 100.0;

	private double _xOffset = 20.0;

	private double _yOffset = 20.0;

	private bool closed;

	private bool _mouseDownOutside;

	private (object sender, MouseButtonEventArgs e) _mouseEventDown = (sender: null, e: null);

	public Screen3D Screen { get; protected set; }

	public Model Model { get; set; }

	public Vector3d AnchorPoint3d { get; set; }

	public double Left
	{
		get
		{
			return this._left;
		}
		set
		{
			this._left = value;
			base.NotifyPropertyChanged("Left");
		}
	}

	public double Top
	{
		get
		{
			return this._top;
		}
		set
		{
			this._top = value;
			base.NotifyPropertyChanged("Top");
		}
	}

	public double Width
	{
		get
		{
			return this._width;
		}
		set
		{
			this._width = value;
			this.OnViewChanged();
		}
	}

	public double Height
	{
		get
		{
			return this._height;
		}
		set
		{
			this._height = value;
			this.OnViewChanged();
		}
	}

	public double XOffset
	{
		get
		{
			return this._xOffset;
		}
		set
		{
			this._xOffset = value;
			this.OnViewChanged();
		}
	}

	public double YOffset
	{
		get
		{
			return this._yOffset;
		}
		set
		{
			this._yOffset = value;
			this.OnViewChanged();
		}
	}

	public double Opacity
	{
		get
		{
			return this._opacity;
		}
		set
		{
			this._opacity = value;
			base.NotifyPropertyChanged("Opacity");
		}
	}

	public double DialogOpacity => this.OpacityMin;

	public bool IsVisible { get; set; } = true;

	public NavigateInteractionMode InteractionMode { get; private set; }

	public event KeyEventHandler KeyDown;

	public event Action OnClose;

	protected void SetOpacityMin(double min)
	{
		this.OpacityMin = min;
		this._opacity = Math.Min(this.OpacityMax, Math.Max(this.OpacityMin, this._opacity));
	}

	public ScreenControlBaseViewModel(Screen3D screen, Model model, Vector3d anchorPoint3d)
	{
		this.Screen = screen;
		this.Model = model;
		this.AnchorPoint3d = anchorPoint3d;
		this.InteractionMode = this.Screen.InteractionMode as NavigateInteractionMode;
		if (this.InteractionMode != null)
		{
			this.InteractionMode.ViewChanged += OnViewChanged;
		}
		this.Screen.InterActionModeChanged += OnScreenOnInterActionModeChanged;
	}

	private void OnScreenOnInterActionModeChanged(IInteractionsMode oldMode, IInteractionsMode newMode)
	{
		this.InteractionMode = newMode as NavigateInteractionMode;
		if (oldMode is NavigateInteractionMode navigateInteractionMode)
		{
			navigateInteractionMode.ViewChanged -= OnViewChanged;
		}
		if (!this.closed && this.InteractionMode != null)
		{
			this.InteractionMode.ViewChanged += OnViewChanged;
		}
	}

	public virtual bool Close()
	{
		this.InteractionMode.ViewChanged -= OnViewChanged;
		this.Screen.InterActionModeChanged -= OnScreenOnInterActionModeChanged;
		this.closed = true;
		this.OnClose?.Invoke();
		return true;
	}

	public virtual void OnViewChanged()
	{
		Vector3d v = this.AnchorPoint3d;
		Matrix transformToDevice = PresentationSource.FromVisual(this.Screen).CompositionTarget.TransformToDevice;
		this.Model?.WorldMatrix.TransformInPlace(ref v);
		Vector2d screenCoordinates = this.Screen.GetScreenCoordinates(v);
		double num = screenCoordinates.X / transformToDevice.M11;
		double num2 = screenCoordinates.Y / transformToDevice.M22;
		num = Math.Min(Math.Max(0.0, num - (this.Width + this.XOffset)), this.Screen.ActualWidth - this.Width);
		num2 = Math.Min(Math.Max(0.0, num2 - (this.Height + this.YOffset)), this.Screen.ActualHeight - this.Height);
		this.Left = num;
		this.Top = num2;
	}

	public virtual void OnVisibilityChanged(bool isVisible)
	{
		this.IsVisible = isVisible;
	}

	public virtual void OnKeyDown(object sender, KeyEventArgs e)
	{
		this.KeyDown?.Invoke(sender, e);
	}

	public virtual void OnMouseMove(object sender, MouseEventArgs e)
	{
		if (this._mouseDownOutside)
		{
			this.Screen.InteractionMode.ImageHostMouseMove(sender, e);
		}
	}

	public virtual void OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (this._mouseDownOutside)
		{
			this.Screen.InteractionMode.ImageHostMouseUp(sender, e);
			this._mouseDownOutside = false;
		}
		this._mouseEventDown = (sender: null, e: null);
	}

	public virtual void OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		this._mouseDownOutside = false;
		this._mouseEventDown = (sender: sender, e: e);
	}

	public void OnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
	{
		this._mouseDownOutside = mouseEventArgs.LeftButton == MouseButtonState.Pressed || mouseEventArgs.MiddleButton == MouseButtonState.Pressed || mouseEventArgs.RightButton == MouseButtonState.Pressed;
	}

	public void OnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
	{
		if (this._mouseEventDown.e != null && this.Screen.InteractionMode is NavigateInteractionMode navigateInteractionMode)
		{
			navigateInteractionMode.ImageHostMouseDown(this._mouseEventDown.sender, this._mouseEventDown.e);
		}
	}
}
