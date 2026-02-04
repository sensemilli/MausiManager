using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.PKernelFlow.Adapters;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.BendDoc.Services;

public class CurrentDocProvider : ICurrentDocProvider, ICurrentBusyState
{
	private readonly IDocManager _docManager;

	private readonly IMainWindowBlock _mainWindowBlock;

	private IScopedFactorio? _currentFactorio;

	private bool _blockRibbon;

	private bool _waitCursor;

	private IDoc3d _currentDoc;

	bool ICurrentBusyState.IsBlockingRibbon => this._blockRibbon;

	bool ICurrentBusyState.IsWaitCursor => this._waitCursor;

	public IDoc3d CurrentDoc
	{
		get
		{
			return this._currentDoc;
		}
		set
		{
			if (this._currentDoc != value)
			{
				this._currentFactorio = null;
				IDoc3d currentDoc = this._currentDoc;
				currentDoc?.BendSimulation?.Pause();
				this._currentDoc = value;
				this.CurrentDocCheckBusy();
				this.CurrentDocChanged?.Invoke(currentDoc, this._currentDoc);
				GeneralSystemComponentsAdapter.New3DPartLoaded();
			}
		}
	}

	public IScopedFactorio CurrentFactorio => this._currentFactorio ?? (this._currentFactorio = this.GetCurrentFactorio());

	public event Action? IsBlockingRibbonChanged;

	public event Action? IsWaitCursorChanged;

	public event Action<IDoc3d, IDoc3d> CurrentDocChanged;

	public CurrentDocProvider(IServiceProvider serviceProvider, IDocManager docManager, IMainWindowBlock mainWindowBlock)
	{
		this._docManager = docManager;
		this._mainWindowBlock = mainWindowBlock;
		this._currentFactorio = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IScopedFactorio>();
		this._mainWindowBlock.BlockChanged += CurrentDocCheckBusy;
		this._mainWindowBlock.WaitChanged += CurrentDocCheckBusy;
	}

	private IScopedFactorio GetCurrentFactorio()
	{
		return this._docManager.GetScope(this.CurrentDoc);
	}

	private void CurrentDocCheckBusy()
	{
		Application.Current.Dispatcher.BeginInvoke(new Action(CurrentDocCheckBusyMainThread));
	}

	private void CurrentDocCheckBusyMainThread()
	{
		bool flag = this._mainWindowBlock.BlockUI_IsBlock(this.CurrentDoc);
		if (flag != this._blockRibbon)
		{
			this._blockRibbon = flag;
			this.IsBlockingRibbonChanged?.Invoke();
		}
		bool flag2 = this._mainWindowBlock.IsWaitCursor(this.CurrentDoc);
		if (flag2 != this._waitCursor)
		{
			this._waitCursor = flag2;
			this.IsWaitCursorChanged?.Invoke();
		}
	}
}
