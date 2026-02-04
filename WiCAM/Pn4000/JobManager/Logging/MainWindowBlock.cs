#region Assembly WiCAM.Pn4000.DependencyInjection, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\u\pn\run\WiCAM.Pn4000.DependencyInjection.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.GuiContracts;

namespace WiCAM.Pn4000.DependencyInjection.Implementations;

internal class MainWindowBlock : IMainWindowBlock
{
    private HashSet<IPnBndDoc> _blockedDocs = new HashSet<IPnBndDoc>();

    private bool _blockAll;

    private HashSet<IPnBndDoc> _waitingDocs = new HashSet<IPnBndDoc>();

    private bool _waitAll;

    public event Action BlockChanged;

    public event Action WaitChanged;

    public void BlockUI_UnblockAll()
    {
        if (BlockUI_IsBlock())
        {
            _blockAll = false;
            _blockedDocs.Clear();
            this.BlockChanged?.Invoke();
        }
    }

    public void BlockUI_Unblock()
    {
        if (_blockAll)
        {
            _blockAll = false;
            this.BlockChanged?.Invoke();
        }
    }

    public void BlockUI_Unblock(IPnBndDoc doc)
    {
        if (_blockedDocs.Remove(doc))
        {
            this.BlockChanged?.Invoke();
        }
    }

    public void BlockUI_Block()
    {
        if (!_blockAll)
        {
            _blockAll = true;
            this.BlockChanged?.Invoke();
        }
    }

    public void BlockUI_Block(IPnBndDoc doc)
    {
        if (_blockedDocs.Add(doc))
        {
            this.BlockChanged?.Invoke();
        }
    }

    public bool BlockUI_IsBlock()
    {
        if (!_blockAll)
        {
            return _blockedDocs.Any();
        }

        return true;
    }

    public bool BlockUI_IsBlock(IPnBndDoc doc)
    {
        if (!_blockAll)
        {
            return _blockedDocs.Contains(doc);
        }

        return true;
    }

    public void CloseWaitAll()
    {
        if (IsWaitCursor())
        {
            _waitAll = false;
            _waitingDocs.Clear();
            this.WaitChanged?.Invoke();
        }
    }

    public void CloseWait()
    {
        if (_waitAll)
        {
            _waitAll = false;
            this.WaitChanged?.Invoke();
        }
    }

    public void CloseWait(IPnBndDoc doc)
    {
        if (_waitingDocs.Remove(doc))
        {
            this.WaitChanged?.Invoke();
        }
    }

    public void InitWait()
    {
        if (!_waitAll)
        {
            _waitAll = true;
            this.WaitChanged?.Invoke();
        }
    }

    public void InitWait(IPnBndDoc doc)
    {
        if (_waitingDocs.Add(doc))
        {
            this.WaitChanged?.Invoke();
        }
    }

    public bool IsWaitCursor()
    {
        if (!_waitAll)
        {
            return _waitingDocs.Any();
        }

        return true;
    }

    public bool IsWaitCursor(IPnBndDoc doc)
    {
        if (!_waitAll)
        {
            return _waitingDocs.Contains(doc);
        }

        return true;
    }
}
#if false // Dekompilierungsprotokoll
361 Elemente im Cache
------------------
Auflösen: System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Runtime.dll
------------------
Auflösen: System.ComponentModel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.ComponentModel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.ComponentModel.dll
------------------
Auflösen: WiCAM.Pn4000.Contracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.Contracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.Contracts.dll
------------------
Auflösen: WiCAM.Services.ConfigProviders.Contracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Services.ConfigProviders.Contracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Services.ConfigProviders.Contracts.dll
------------------
Auflösen: WiCAM.Pn4000.Config, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Pn4000.Config, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.Config.dll
------------------
Auflösen: WiCAM.Pn4000.BendModel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.BendModel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.BendModel.dll
------------------
Auflösen: Microsoft.Extensions.DependencyInjection.Abstractions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
Einzelne Assembly gefunden: Microsoft.Extensions.DependencyInjection.Abstractions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\Microsoft.Extensions.DependencyInjection.Abstractions.dll
------------------
Auflösen: pncommon, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: pncommon, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\pncommon.dll
------------------
Auflösen: Microsoft.Extensions.Options, Version=9.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
Einzelne Assembly gefunden: Microsoft.Extensions.Options, Version=9.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
Laden von: C:\u\pn\run\Microsoft.Extensions.Options.dll
------------------
Auflösen: System.Collections, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Collections, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Collections.dll
------------------
Auflösen: WiCAM.Pn4000.GuiContracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.GuiContracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.GuiContracts.dll
------------------
Auflösen: WiCAM.Pn4000.HinnModuleReader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.HinnModuleReader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.PN3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.PN3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.PN3D.dll
------------------
Auflösen: WiCAM.PN4000.PnPathService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.PN4000.PnPathService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\u\pn\run\WiCAM.PN4000.PnPathService.dll
------------------
Auflösen: WiCAM.Pn4000.Config.Manager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Der Name "WiCAM.Pn4000.Config.Manager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.ToolCalculation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.ToolCalculation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.ToolCalculation.dll
------------------
Auflösen: WiCAM.Pn4000.ToolCalculationMediator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.ToolCalculationMediator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.ToolCalculationMediator.dll
------------------
Auflösen: WiCAM.Pn4000.BendOrderCalculation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.BendOrderCalculation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: BendDataBase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: BendDataBase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\BendDataBase.dll
------------------
Auflösen: BendDataSourceModel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: BendDataSourceModel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\BendDataSourceModel.dll
------------------
Auflösen: WiCAM.Pn4000.BendSimulation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.BendSimulation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.AddInManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.AddInManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Services.ConfigProviders, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Services.ConfigProviders, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\u\pn\run\WiCAM.Services.ConfigProviders.dll
------------------
Auflösen: WiCAM.Services.Loggers.Contracts, Version=1.0.3.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Services.Loggers.Contracts, Version=1.0.3.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Services.Loggers.Contracts.dll
------------------
Auflösen: WiCAM.Services.Loggers, Version=1.0.3.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Services.Loggers, Version=1.0.3.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Services.Loggers.dll
------------------
Auflösen: WiCAM.Pn4000.BendTable, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.BendTable, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.BendTable.dll
------------------
Auflösen: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.ScreenD3D.dll
------------------
Auflösen: WiCAM.Pn4000.Cora, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.Cora, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Services.AutoLopServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Services.AutoLopServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Services.PiplstServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Services.PiplstServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.BendDoc, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.BendDoc, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.MachineAndTools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.MachineAndTools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.BendModel.Services, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.BendModel.Services, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.BendModel.Services.dll
------------------
Auflösen: WiCAM.Pn4000.PKernel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.PKernel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.PN4000.TrumpfL26Service, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.PN4000.TrumpfL26Service, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.Acad2PNWpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Der Name "WiCAM.Pn4000.Acad2PNWpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.Reporting, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.Reporting, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.BendModel.Loader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Pn4000.BendModel.Loader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.BendModel.Loader.dll
------------------
Auflösen: WiCAM.Pn4000.XEvents, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.XEvents, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\u\pn\run\WiCAM.Pn4000.XEvents.dll
------------------
Auflösen: WiCAM.Pn4000.PartsReader, Version=1.0.4.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.PartsReader, Version=1.0.4.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.Popup, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.Popup, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.Popup.dll
------------------
Auflösen: WiCAM.Pn4000.DSTV, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.DSTV, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.Acad2PN, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Der Name "WiCAM.Pn4000.Acad2PN, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.PKernelFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Pn4000.PKernelFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\Users\TBraig\source\repos\sensemilli\MausiManager\BendDoc\bin\Debug\net9.0-windows\WiCAM.Pn4000.PKernelFlow.dll
------------------
Auflösen: System.Threading.Thread, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Threading.Thread, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Threading.Thread.dll
------------------
Auflösen: System.Windows.Forms, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Einzelne Assembly gefunden: System.Windows.Forms, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.Windows.Forms.dll
------------------
Auflösen: System.Threading, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Threading, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Threading.dll
------------------
Auflösen: System.Linq, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Linq, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Linq.dll
------------------
Auflösen: PresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Einzelne Assembly gefunden: PresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\PresentationFramework.dll
------------------
Auflösen: WiCAM.Services.ConfigProviders.Readers, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Der Name "WiCAM.Services.ConfigProviders.Readers, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe" wurde nicht gefunden.
------------------
Auflösen: System.Windows.Forms.Primitives, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Einzelne Assembly gefunden: System.Windows.Forms.Primitives, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.Windows.Forms.Primitives.dll
------------------
Auflösen: System.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Einzelne Assembly gefunden: System.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.Xaml.dll
------------------
Auflösen: WindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Einzelne Assembly gefunden: WindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\WindowsBase.dll
------------------
Auflösen: System.ObjectModel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.ObjectModel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.ObjectModel.dll
------------------
Auflösen: System.Windows.Extensions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Einzelne Assembly gefunden: System.Windows.Extensions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.Windows.Extensions.dll
------------------
Auflösen: System.Security.Permissions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Einzelne Assembly gefunden: System.Security.Permissions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.Security.Permissions.dll
------------------
Auflösen: System.IO.Packaging, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.IO.Packaging, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.IO.Packaging.dll
------------------
Auflösen: System.Drawing.Common, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Einzelne Assembly gefunden: System.Drawing.Common, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Laden von: C:\Users\TBraig\.nuget\packages\system.drawing.common\9.0.8\lib\net9.0\System.Drawing.Common.dll
------------------
Auflösen: System.Security.AccessControl, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Security.AccessControl, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Security.AccessControl.dll
------------------
Auflösen: System.Drawing.Primitives, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Drawing.Primitives, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Drawing.Primitives.dll
------------------
Auflösen: System.Runtime.InteropServices, Version=9.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: System.Runtime.InteropServices, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Runtime.InteropServices.dll
------------------
Auflösen: System.Runtime.CompilerServices.Unsafe, Version=9.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: System.Runtime.CompilerServices.Unsafe, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Runtime.CompilerServices.Unsafe.dll
#endif
