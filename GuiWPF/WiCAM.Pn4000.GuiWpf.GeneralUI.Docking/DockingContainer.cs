using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;

public class DockingContainer : ContentControl, INotifyPropertyChanged
{
	private struct Rect
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	private readonly object _myLock = new object();

	private readonly DockingContainerXaml _containerXaml;

	private IDockingService _dockingService;

	private Window _refWindow;

	private HwndSource _hWndSource;

	private bool _hWndInitialized;

	private DateTime _timeStamp;

	private Vector2d _refWindowPos = Vector2d.Zero;

	private readonly ICollection<Tuple<ToolWindow, Vector2d>> _toolWindowOffsets = new List<Tuple<ToolWindow, Vector2d>>();

	public static readonly DependencyProperty ChildContentProperty = DependencyProperty.Register("ChildContent", typeof(object), typeof(DockingContainer), new PropertyMetadata(null, ChildContentChanged));

	private RadDocking Docking => _containerXaml.DockingContainer;

	public IAddChild ChildContainer => _containerXaml.ChildContainer;

	public object ChildContent
	{
		get
		{
			return GetValue(ChildContentProperty);
		}
		set
		{
			SetValue(ChildContentProperty, value);
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	private static void ChildContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is DockingContainer dockingContainer && e.OldValue != e.NewValue)
		{
			dockingContainer.ChildContainer.AddChild(e.NewValue);
		}
	}

	public DockingContainer()
	{
		_containerXaml = new DockingContainerXaml(this);
		base.Content = _containerXaml;
	}

	public void Initialize(Window refWindow, IDockingService dockingService)
	{
		_refWindow = refWindow;
		_dockingService = dockingService;
		_dockingService.Initialize(Docking);
		Docking.ToolWindowCreated += OnToolWindowCreated;
		_refWindow.Loaded += delegate
		{
			InitializeHWndSource();
		};
		if (_refWindow.IsLoaded)
		{
			InitializeHWndSource();
		}
	}

	private void OnToolWindowCreated(object? sender, ElementCreatedEventArgs e)
	{
		DependencyObject createdElement = e.CreatedElement;
		ToolWindow toolWindow = createdElement as ToolWindow;
		if (toolWindow == null)
		{
			return;
		}
		toolWindow.DataContext = new ToolWindowViewModel();
		toolWindow.GotKeyboardFocus += delegate
		{
			toolWindow.Opacity = 1.0;
		};
		toolWindow.LostKeyboardFocus += delegate(object sender, KeyboardFocusChangedEventArgs args)
		{
			if (sender is ToolWindow { DataContext: ToolWindowViewModel { IsTransparencyEnabled: not false } } toolWindow2 && toolWindow2.WindowState != WindowState.Maximized && (!(args.NewFocus is DependencyObject dependencyObject) || (ParentOfTypeExtensions.ParentOfType<ToolWindow>(dependencyObject) != toolWindow2 && ParentOfTypeExtensions.ParentOfType<System.Windows.Controls.Primitives.Popup>(dependencyObject) == null)))
			{
				Vector2d vector2d = new Vector2d(toolWindow2.Left, toolWindow2.Top) - GetPosition();
				if (!(0.0 - vector2d.X >= toolWindow2.ActualWidth) && !(0.0 - vector2d.Y >= toolWindow2.ActualHeight) && !(vector2d.X >= _refWindow.ActualWidth) && !(vector2d.Y >= _refWindow.ActualHeight))
				{
					toolWindow2.Opacity = 0.5;
				}
			}
		};
	}

	private void OnWindowMoved()
	{
		lock (_myLock)
		{
			if ((DateTime.Now - _timeStamp).TotalMilliseconds > 500.0)
			{
				_timeStamp = DateTime.Now;
				_toolWindowOffsets.Clear();
				foreach (ToolWindow item4 in (from x in Docking.Panes
					select ParentOfTypeExtensions.ParentOfType<ToolWindow>((DependencyObject)x) into x
					where x != null
					select x).Distinct())
				{
					if (item4.WindowState != WindowState.Maximized)
					{
						Vector2d item = new Vector2d(item4.Left, item4.Top) - _refWindowPos;
						if (!(0.0 - item.X >= item4.ActualWidth) && !(0.0 - item.Y >= item4.ActualHeight) && !(item.X >= _refWindow.ActualWidth) && !(item.Y >= _refWindow.ActualHeight))
						{
							_toolWindowOffsets.Add(new Tuple<ToolWindow, Vector2d>(item4, item));
						}
					}
				}
			}
		}
		_refWindowPos = GetPosition();
		foreach (Tuple<ToolWindow, Vector2d> toolWindowOffset in _toolWindowOffsets)
		{
			toolWindowOffset.Deconstruct(out var item2, out var item3);
			ToolWindow toolWindow = item2;
			Vector2d vector2d = item3;
			Vector2d vector2d2 = _refWindowPos + vector2d;
			toolWindow.Left = vector2d2.X;
			toolWindow.Top = vector2d2.Y;
		}
	}

	private void InitializeHWndSource()
	{
		lock (_myLock)
		{
			if (_hWndInitialized)
			{
				return;
			}
			_hWndInitialized = true;
		}
		_hWndSource = HwndSource.FromHwnd(new WindowInteropHelper(_refWindow).Handle);
		_hWndSource.AddHook(WndProc);
		_refWindowPos = GetPosition();
		_timeStamp = DateTime.Now;
	}

	private nint WndProc(nint hWnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		if (msg == 3)
		{
			OnWindowMoved();
		}
		return IntPtr.Zero;
	}

	private Vector2d GetPosition()
	{
		if (!_hWndInitialized)
		{
			throw new InvalidOperationException("hWindSource not initialized!");
		}
		Rect lpRect = default(Rect);
		GetWindowRect(_hWndSource.Handle, ref lpRect);
		return new Vector2d(lpRect.Left, lpRect.Top);
	}

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetWindowRect(nint hWnd, ref Rect lpRect);

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
