using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.GuiContracts.Popups;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;

internal class DockingService : IDockingService
{
	private enum RegistrationType
	{
		None,
		Singleton,
		Scoped,
		Transient
	}

	private record ViewRegistration(Type ViewModelType, Type ViewType, RegistrationType Type, TmpStartPos? StartPos, string? TitleKey);

	private class PaneWrapper(RadPane pane, ViewRegistration registration)
	{
		public RadPane Pane { get; } = pane;

		public ViewRegistration Registration { get; } = registration;

		public bool Initialized { get; set; }
	}

	private struct Rect
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	private RadDocking? _docking;

	private readonly ObservableCollection<RadPane> _panes = new ObservableCollection<RadPane>();

	private readonly IFactorio _factorio;

	private readonly ITranslator _translator;

	private readonly Dictionary<Type, ViewRegistration> _viewModelToView = new Dictionary<Type, ViewRegistration>();

	private readonly ConditionalWeakTable<object, PaneWrapper> _concreteViewModelToPane = new ConditionalWeakTable<object, PaneWrapper>();

	private readonly ConditionalWeakTable<object, PaneWrapper> _concreteViewToPane = new ConditionalWeakTable<object, PaneWrapper>();

	private object? _activeContext;

	private RadPane _activePane;

	private readonly List<object> _visibleViewModels = new List<object>();

	private event Action<IScopedFactorio> ScopedChanged;

	private event Action ContextChanged;

	public DockingService(IFactorio factorio, ITranslator translator)
	{
		_factorio = factorio;
		_translator = translator;
	}

	public void RegisterViewModelOnly<TViewModel>()
	{
		RegisterView<object, TViewModel>(RegistrationType.None, null, null);
	}

	public void RegisterSingletonView<TView, TViewModel>(TmpStartPos? startPos = null, string? titleKey = null)
	{
		RegisterView<TView, TViewModel>(RegistrationType.Singleton, startPos, titleKey);
	}

	public void RegisterScopedView<TView, TViewModel>(TmpStartPos? startPos = null, string? titleKey = null)
	{
		RegisterView<TView, TViewModel>(RegistrationType.Scoped, startPos, titleKey);
	}

	public void RegisterTransientView<TView, TViewModel>(TmpStartPos? startPos = null, string? titleKey = null)
	{
		RegisterView<TView, TViewModel>(RegistrationType.Transient, startPos, titleKey);
	}

	private void RegisterView<TView, TViewModel>(RegistrationType type, TmpStartPos? startPos, string? titleKey)
	{
		ViewRegistration value = new ViewRegistration(typeof(TViewModel), typeof(TView), type, startPos, titleKey);
		_viewModelToView.Add(typeof(TViewModel), value);
	}

	public void ChangeContext(object? context)
	{
		HideAll();
	}

	public void ChangeScope(IScopedFactorio factorio)
	{
		foreach (var (viewModel, paneWrapper2) in (IEnumerable<KeyValuePair<object, PaneWrapper>>)_concreteViewModelToPane)
		{
			if (paneWrapper2.Registration.Type == RegistrationType.Transient)
			{
				InternalHide(viewModel);
			}
		}
		this.ScopedChanged?.Invoke(factorio);
	}

	private string Save()
	{
		using MemoryStream memoryStream = new MemoryStream();
		_docking.SaveLayout(memoryStream);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return new StreamReader(memoryStream).ReadToEnd();
	}

	private void Load(string? layout)
	{
		if (layout == null)
		{
			return;
		}
		using MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(layout));
		memoryStream.Seek(0L, SeekOrigin.Begin);
		_docking.LoadLayout(memoryStream);
	}

	public void ShowIfExists(object? viewModel)
	{
		CheckInitialized();
		if (viewModel != null && _concreteViewModelToPane.TryGetValue(viewModel, out PaneWrapper wrapper))
		{
			Application.Current.Dispatcher.Invoke(delegate
			{
				ShowPane(viewModel, wrapper);
			});
		}
	}

	public void Show<TViewModel>(TViewModel? viewModel, IScopedFactorio? factorio = null) where TViewModel : class
	{
		CheckInitialized();
		Application.Current.Dispatcher.Invoke(delegate
		{
			PaneWrapper orCreatePane = GetOrCreatePane(ref viewModel, factorio);
			ShowPane(viewModel, orCreatePane);
		});
	}

	private void ShowPane(object? viewModel, PaneWrapper? pane)
	{
		if (viewModel == null || pane == null)
		{
			return;
		}
		_concreteViewModelToPane.AddOrUpdate(viewModel, pane);
		pane.Pane.DataContext = viewModel;
		if (pane.Pane.Header == null && pane.Registration.TitleKey != null)
		{
			pane.Pane.Header = _translator.Translate(pane.Registration.TitleKey);
		}
		pane.Pane.IsHidden = false;
		_docking.ActivePane = pane.Pane;
		try
		{
			typeof(RadPane).GetMethod("WriteFlag", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(pane.Pane, new object[2] { 4194304u, true });
		}
		finally
		{
			pane.Pane.Focus();
		}
	}

	public void Hide(object? viewModel)
	{
		CheckInitialized();
		InternalHide(viewModel);
	}

	private void InternalHide(object? viewModel)
	{
		if (viewModel != null && _concreteViewModelToPane.TryGetValue(viewModel, out PaneWrapper value))
		{
			value.Pane.IsHidden = true;
			if (_docking.ActivePane == value.Pane)
			{
				_docking.ActivePane = null;
			}
			_concreteViewModelToPane.Remove(viewModel);
		}
	}

	public void HideAll()
	{
		CheckInitialized();
		foreach (var (viewModel, _) in (IEnumerable<KeyValuePair<object, PaneWrapper>>)_concreteViewModelToPane)
		{
			InternalHide(viewModel);
		}
	}

	private PaneWrapper? GetOrCreatePane<TViewModel>(ref TViewModel? viewModel, IFactorio? factorio) where TViewModel : class
	{
		if (factorio == null)
		{
			factorio = _factorio;
		}
		if (viewModel == null)
		{
			viewModel = factorio.Resolve<TViewModel>();
		}
		if (viewModel == null)
		{
			throw new Exception("Could not resolve type '" + typeof(TViewModel).Name + "'");
		}
		if (_concreteViewModelToPane.TryGetValue(viewModel, out PaneWrapper wrapper))
		{
			return wrapper;
		}
		if (!_viewModelToView.TryGetValue(typeof(TViewModel), out ViewRegistration value))
		{
			throw new Exception("No view for '" + typeof(TViewModel).Name + "' found.");
		}
		if (value.Type == RegistrationType.None)
		{
			return null;
		}
		Type viewType = value.ViewType;
		object obj = factorio.Resolve(viewType);
		if (obj == null)
		{
			throw new Exception("Could not resolve type '" + viewType.Name + "'");
		}
		if (!_concreteViewToPane.TryGetValue(obj, out wrapper))
		{
			RadPane radPane = new RadPane
			{
				Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentUICulture.IetfLanguageTag),
				Content = obj,
				IsHidden = true,
				CanDockInDocumentHost = false,
				CanUserPin = false,
				ContextMenuTemplate = null
			};
			wrapper = new PaneWrapper(radPane, value);
			radPane.Loaded += delegate
			{
				InitializePane(wrapper);
			};
			_panes.Add(radPane);
			radPane.MakeFloatingDockable();
		}
		TViewModel val = viewModel;
		IPopupViewModel popup = val as IPopupViewModel;
		if (popup != null)
		{
			popup.CloseView += delegate
			{
				InternalHide(popup);
			};
		}
		_concreteViewToPane.AddOrUpdate(obj, wrapper);
		_concreteViewModelToPane.AddOrUpdate(viewModel, wrapper);
		return wrapper;
	}

	private void InitializePane(PaneWrapper wrapper)
	{
		if (wrapper.Initialized)
		{
			return;
		}
		wrapper.Initialized = true;
		ToolWindow toolWindow = ParentOfTypeExtensions.ParentOfType<ToolWindow>((DependencyObject)wrapper.Pane);
		if (toolWindow == null)
		{
			return;
		}
		TmpStartPos startPos = wrapper.Registration.StartPos;
		if (startPos == null)
		{
			return;
		}
		toolWindow.Width = startPos.Width;
		toolWindow.Height = startPos.Height;
		if (_docking.DocumentHost is Grid grid)
		{
			Window window = ParentOfTypeExtensions.ParentOfType<Window>((DependencyObject)grid);
			Vector2d position = GetPosition(window);
			if (startPos.IsCenter)
			{
				toolWindow.Top = position.Y + window.ActualHeight * 0.5 - toolWindow.ActualHeight * 0.5;
				toolWindow.Left = position.X + window.ActualWidth * 0.5 - toolWindow.ActualWidth * 0.5;
			}
			else
			{
				toolWindow.Top = position.Y + window.ActualHeight - toolWindow.ActualHeight - startPos.Offset.Y;
				toolWindow.Left = position.X + startPos.Offset.X;
			}
		}
	}

	public void Initialize(RadDocking docking)
	{
		_docking = docking;
		_docking.PanesSource = _panes;
	}

	private void CheckInitialized()
	{
		if (_docking == null)
		{
			throw new InvalidOperationException("Service not initialized!");
		}
	}

	private static Vector2d GetPosition(Window window)
	{
		HwndSource hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
		Rect lpRect = default(Rect);
		GetWindowRect(hwndSource.Handle, ref lpRect);
		return new Vector2d(lpRect.Left, lpRect.Top);
	}

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetWindowRect(nint hWnd, ref Rect lpRect);
}
