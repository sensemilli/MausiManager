#region Assembly WiCAM.Pn4000.GuiWpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\u\pn\run\WiCAM.Pn4000.GuiWpf.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using SharpDX.Mathematics.Interop;
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

    private class PaneWrapper
    {
        public RadPane Pane { get; }

        public ViewRegistration Registration { get; }

        public bool Initialized { get; set; }

        public PaneWrapper(RadPane pane, ViewRegistration registration)
        {
            Pane = pane;
            Registration = registration;
           // base._002Ector();
        }
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
        _docking.SaveLayout((Stream)memoryStream);
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
        _docking.LoadLayout((Stream)memoryStream);
    }

    public void ShowIfExists(object? viewModel)
    {
        object viewModel2 = viewModel;
        CheckInitialized();
        if (viewModel2 != null && _concreteViewModelToPane.TryGetValue(viewModel2, out PaneWrapper wrapper))
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                ShowPane(viewModel2, wrapper);
            });
        }
    }

    public void Show<TViewModel>(TViewModel? viewModel, IScopedFactorio? factorio = null) where TViewModel : class
    {
        TViewModel viewModel2 = viewModel;
        IScopedFactorio factorio2 = factorio;
        CheckInitialized();
        Application.Current.Dispatcher.Invoke(delegate
        {
            PaneWrapper orCreatePane = GetOrCreatePane(ref viewModel2, factorio2);
            ShowPane(viewModel2, orCreatePane);
        });
    }

    private void ShowPane(object? viewModel, PaneWrapper? pane)
    {
        if (viewModel == null || pane == null)
        {
            return;
        }

        _concreteViewModelToPane.AddOrUpdate(viewModel, pane);
        ((FrameworkElement)(object)pane.Pane).DataContext = viewModel;
        if (((HeaderedContentControl)(object)pane.Pane).Header == null && pane.Registration.TitleKey != null)
        {
            ((HeaderedContentControl)(object)pane.Pane).Header = _translator.Translate(pane.Registration.TitleKey);
        }

        pane.Pane.IsHidden = false;
        _docking.ActivePane = pane.Pane;
        try
        {
            typeof(RadPane).GetMethod("WriteFlag", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(pane.Pane, new object[2] { 4194304u, true });
        }
        finally
        {
            ((UIElement)(object)pane.Pane).Focus();
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
        //IL_010f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0114: Unknown result type (might be due to invalid IL or missing references)
        //IL_012e: Expected O, but got Unknown
        //IL_012e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0135: Expected O, but got Unknown
        //IL_0135: Unknown result type (might be due to invalid IL or missing references)
        //IL_013c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0143: Unknown result type (might be due to invalid IL or missing references)
        //IL_014a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0153: Expected O, but got Unknown
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
            RadPane val = new RadPane();
            ((FrameworkElement)val).Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentUICulture.IetfLanguageTag);
            ((ContentControl)val).Content = obj;
            val.IsHidden = true;
            val.CanDockInDocumentHost = false;
            val.CanUserPin = false;
            val.ContextMenuTemplate = null;
            RadPane val2 = val;
            wrapper = new PaneWrapper(val2, value);
            ((FrameworkElement)(object)val2).Loaded += delegate
            {
                InitializePane(wrapper);
            };
            _panes.Add(val2);
            val2.MakeFloatingDockable();
        }

        TViewModel val3 = viewModel;
        IPopupViewModel popup = val3 as IPopupViewModel;
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
        ToolWindow val = ((DependencyObject)(object)wrapper.Pane).ParentOfType<ToolWindow>();
        if (val == null)
        {
            return;
        }

        TmpStartPos startPos = wrapper.Registration.StartPos;
        if (startPos == null)
        {
            return;
        }

        ((FrameworkElement)(object)val).Width = startPos.Width;
        ((FrameworkElement)(object)val).Height = startPos.Height;
        if (_docking.DocumentHost is Grid element)
        {
            Window window = element.ParentOfType<Window>();
            Vector2d position = GetPosition(window);
            if (startPos.IsCenter)
            {
                ((WindowBase)val).Top = position.Y + window.ActualHeight * 0.5 - ((FrameworkElement)(object)val).ActualHeight * 0.5;
                ((WindowBase)val).Left = position.X + window.ActualWidth * 0.5 - ((FrameworkElement)(object)val).ActualWidth * 0.5;
            }
            else
            {
                ((WindowBase)val).Top = position.Y + window.ActualHeight - ((FrameworkElement)(object)val).ActualHeight - startPos.Offset.Y;
                ((WindowBase)val).Left = position.X + startPos.Offset.X;
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
#if false // Dekompilierungsprotokoll
362 Elemente im Cache
------------------
Auflösen: System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Runtime.dll
------------------
Auflösen: WindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Einzelne Assembly gefunden: WindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\WindowsBase.dll
------------------
Auflösen: WiCAM.Pn4000.Comon, Version=3.27.1.14, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Pn4000.Comon, Version=3.27.1.14, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.Comon.dll
------------------
Auflösen: PresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Einzelne Assembly gefunden: PresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\PresentationFramework.dll
------------------
Auflösen: System.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Einzelne Assembly gefunden: System.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.Xaml.dll
------------------
Auflösen: Telerik.Windows.Controls.Data, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7
Der Name "Telerik.Windows.Controls.Data, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.Popup, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.Popup, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.Popup.dll
------------------
Auflösen: WiCAM.Pn4000.Contracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.Contracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.Contracts.dll
------------------
Auflösen: WiCAM.Services.ConfigProviders.Contracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Services.ConfigProviders.Contracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Services.ConfigProviders.Contracts.dll
------------------
Auflösen: pncommon, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: pncommon, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\pncommon.dll
------------------
Auflösen: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.ScreenD3D.dll
------------------
Auflösen: System.ObjectModel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.ObjectModel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.ObjectModel.dll
------------------
Auflösen: WiCAM.Pn4000.BendModel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.BendModel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.BendModel.dll
------------------
Auflösen: System.Collections, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Collections, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Collections.dll
------------------
Auflösen: PresentationCore, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Einzelne Assembly gefunden: PresentationCore, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\PresentationCore.dll
------------------
Auflösen: WiCAM.Pn4000.GuiContracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.GuiContracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.GuiContracts.dll
------------------
Auflösen: pn4.uicontrols, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: pn4.uicontrols, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\pn4.uicontrols.dll
------------------
Auflösen: WiCAM.Pn4000.Config, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Pn4000.Config, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.Config.dll
------------------
Auflösen: Microsoft.Extensions.DependencyInjection.Abstractions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
Einzelne Assembly gefunden: Microsoft.Extensions.DependencyInjection.Abstractions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\Microsoft.Extensions.DependencyInjection.Abstractions.dll
------------------
Auflösen: System.ComponentModel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.ComponentModel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.ComponentModel.dll
------------------
Auflösen: SharpDX.Mathematics, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1
Einzelne Assembly gefunden: SharpDX.Mathematics, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1
Laden von: C:\Users\TBraig\.nuget\packages\sharpdx.mathematics\4.2.0\lib\netstandard1.1\SharpDX.Mathematics.dll
------------------
Auflösen: WiCAM.Pn4000.PN3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.PN3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.PN3D.dll
------------------
Auflösen: BendDataBase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: BendDataBase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\BendDataBase.dll
------------------
Auflösen: Telerik.Windows.Controls.GridView, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7
Der Name "Telerik.Windows.Controls.GridView, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7" wurde nicht gefunden.
------------------
Auflösen: System.Threading.Thread, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Threading.Thread, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Threading.Thread.dll
------------------
Auflösen: System.Linq, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Linq, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Linq.dll
------------------
Auflösen: Telerik.Windows.Controls.FileDialogs, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7
Der Name "Telerik.Windows.Controls.FileDialogs, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7" wurde nicht gefunden.
------------------
Auflösen: Telerik.Windows.Controls.Charting, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7
Der Name "Telerik.Windows.Controls.Charting, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7" wurde nicht gefunden.
------------------
Auflösen: Telerik.Windows.Data, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7
Einzelne Assembly gefunden: Telerik.Windows.Data, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\Telerik.Windows.Data.dll
------------------
Auflösen: System.Windows.Forms, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Einzelne Assembly gefunden: System.Windows.Forms, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.Windows.Forms.dll
------------------
Auflösen: WiCAM.Pn4000.MachineAndTools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.MachineAndTools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.ToolCalculationGuiWpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.ToolCalculationGuiWpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: Telerik.Windows.Controls.Input, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7
Der Name "Telerik.Windows.Controls.Input, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.BendTable, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.BendTable, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.BendTable.dll
------------------
Auflösen: System.Collections.Immutable, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Collections.Immutable, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Collections.Immutable.dll
------------------
Auflösen: WiCAM.Pn4000.WpfControls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.WpfControls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: Telerik.Windows.Controls, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7
Einzelne Assembly gefunden: Telerik.Windows.Controls, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\Telerik.Windows.Controls.dll
------------------
Auflösen: Telerik.Windows.Controls.Navigation, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7
Der Name "Telerik.Windows.Controls.Navigation, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7" wurde nicht gefunden.
------------------
Auflösen: System.ComponentModel.Primitives, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.ComponentModel.Primitives, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.ComponentModel.Primitives.dll
------------------
Auflösen: System.IO.Compression, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Einzelne Assembly gefunden: System.IO.Compression, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.IO.Compression.dll
------------------
Auflösen: System.Xml.ReaderWriter, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Xml.ReaderWriter, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Xml.ReaderWriter.dll
------------------
Auflösen: System.Text.RegularExpressions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Text.RegularExpressions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Text.RegularExpressions.dll
------------------
Auflösen: Telerik.Windows.Controls.Chart, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7
Der Name "Telerik.Windows.Controls.Chart, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.ToolCalculation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.ToolCalculation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.ToolCalculation.dll
------------------
Auflösen: System.Threading, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Threading, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Threading.dll
------------------
Auflösen: System.Collections.Concurrent, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Collections.Concurrent, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Collections.Concurrent.dll
------------------
Auflösen: WiCAM.Pn4000.ScreenControls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.ScreenControls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\Librarys\WiCAM.Pn4000.ScreenControls.dll
------------------
Auflösen: WiCAM.Pn4000.BendSimulation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.BendSimulation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.ToolCalculationMediator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.ToolCalculationMediator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.ToolCalculationMediator.dll
------------------
Auflösen: WiCAM.Pn4000.FingerStopCalculationMediator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.FingerStopCalculationMediator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.FingerStopCalculationMediator.dll
------------------
Auflösen: WiCAM.Pn4000.BendModel.Services, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.BendModel.Services, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.BendModel.Services.dll
------------------
Auflösen: Telerik.Windows.Controls.Docking, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7
Der Name "Telerik.Windows.Controls.Docking, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.Screen, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.Screen, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.Screen.dll
------------------
Auflösen: WiCAM.Pn4000.BendModel.Validation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Pn4000.BendModel.Validation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.BendModel.Validation.dll
------------------
Auflösen: WiCAM.Pn4000.WpfDSTV, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.WpfDSTV, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.BendModel.Loader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Pn4000.BendModel.Loader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.BendModel.Loader.dll
------------------
Auflösen: Telerik.Windows.Controls.ScheduleView, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7
Der Name "Telerik.Windows.Controls.ScheduleView, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.WpfBasics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.WpfBasics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: System.Console, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Console, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Console.dll
------------------
Auflösen: WiCAM.Pn4000.MdbImporter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.MdbImporter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: System.Text.Json, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Einzelne Assembly gefunden: System.Text.Json, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Text.Json.dll
------------------
Auflösen: System.Threading.Tasks.Parallel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Threading.Tasks.Parallel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Threading.Tasks.Parallel.dll
------------------
Auflösen: WiCAM.Pn4000.PKernelFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Pn4000.PKernelFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.PKernelFlow.dll
------------------
Auflösen: Telerik.Windows.Diagrams.Core, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7
Der Name "Telerik.Windows.Diagrams.Core, Version=2022.3.912.60, Culture=neutral, PublicKeyToken=5803cfa389c90ce7" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.Encodings, Version=2.0.3.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Pn4000.Encodings, Version=2.0.3.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.Encodings.dll
------------------
Auflösen: System.IO.Compression.ZipFile, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Einzelne Assembly gefunden: System.IO.Compression.ZipFile, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.IO.Compression.ZipFile.dll
------------------
Auflösen: BendDataSourceModel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: BendDataSourceModel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\BendDataSourceModel.dll
------------------
Auflösen: System.Runtime.InteropServices, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Runtime.InteropServices, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Runtime.InteropServices.dll
------------------
Auflösen: System.ComponentModel.TypeConverter, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.ComponentModel.TypeConverter, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.ComponentModel.TypeConverter.dll
------------------
Auflösen: WiCAM.Pn4000.PKernelFlow.Adapters, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.PKernelFlow.Adapters, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.PKernelFlow.Adapters.dll
------------------
Auflösen: System.Xml.XmlSerializer, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Xml.XmlSerializer, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Xml.XmlSerializer.dll
------------------
Auflösen: Microsoft.EntityFrameworkCore.Abstractions, Version=6.0.12.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
Der Name "Microsoft.EntityFrameworkCore.Abstractions, Version=6.0.12.0, Culture=neutral, PublicKeyToken=adb9793829ddae60" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.BendModel.Writer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Pn4000.BendModel.Writer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.BendModel.Writer.dll
------------------
Auflösen: System.IO.Packaging, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.IO.Packaging, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.IO.Packaging.dll
------------------
Auflösen: System.Security.Permissions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Einzelne Assembly gefunden: System.Security.Permissions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.Security.Permissions.dll
------------------
Auflösen: System.Windows.Extensions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Einzelne Assembly gefunden: System.Windows.Extensions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.Windows.Extensions.dll
------------------
Auflösen: System.Windows.Forms.Primitives, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Einzelne Assembly gefunden: System.Windows.Forms.Primitives, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.Windows.Forms.Primitives.dll
------------------
Auflösen: System.Security.AccessControl, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Security.AccessControl, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Security.AccessControl.dll
------------------
Auflösen: System.Drawing.Common, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Einzelne Assembly gefunden: System.Drawing.Common, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Laden von: C:\Users\TBraig\.nuget\packages\system.drawing.common\9.0.8\lib\net9.0\System.Drawing.Common.dll
------------------
Auflösen: System.Drawing.Primitives, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Drawing.Primitives, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Drawing.Primitives.dll
------------------
Auflösen: System.Runtime.CompilerServices.Unsafe, Version=9.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: System.Runtime.CompilerServices.Unsafe, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Runtime.CompilerServices.Unsafe.dll
#endif
