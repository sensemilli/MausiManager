using System;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.GuiContracts;

namespace WiCAM.Pn4000.GuiWpf.Services;

internal class CurrentCalculation : ICurrentCalculation
{
	private readonly IMainWindowBlock _mainWindowBlock;

	private readonly IPnBndDoc _doc;

	private ICalculationArg? _option;

	private readonly object _lockObj = new object();

	public ICalculationArg? CurrentCalculationOption => _option;

	public event Action CalculationWaiting;

	public event Action<ICalculationArg?> CurrentCalculationChanged;

	public CurrentCalculation(IMainWindowBlock mainWindowBlock, IPnBndDoc doc)
	{
		_mainWindowBlock = mainWindowBlock;
		_doc = doc;
	}

	public bool TryStartNewCalculation(ICalculationArg? option)
	{
		if (option == null)
		{
			return false;
		}
		lock (_lockObj)
		{
			if (_option == null)
			{
				if (_mainWindowBlock.BlockUI_IsBlock(_doc))
				{
					return false;
				}
				_mainWindowBlock.BlockUI_Block(_doc);
				_option = option;
				this.CurrentCalculationChanged?.Invoke(option);
				if (option?.DebugInfo != null)
				{
					option.DebugInfo.StartWaiting += StartWaiting;
				}
				return true;
			}
		}
		return false;
	}

	public void EndCalculation(ICalculationArg option)
	{
		if (option?.DebugInfo != null)
		{
			option.DebugInfo.StartWaiting -= StartWaiting;
		}
		if (_option == option)
		{
			_option = null;
			this.CurrentCalculationChanged?.Invoke(null);
			_mainWindowBlock.BlockUI_Unblock(_doc);
		}
	}

	private void StartWaiting()
	{
		this.CalculationWaiting?.Invoke();
	}
}
