using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.PnCommand;

internal class PnCommandBasics : IPnCommandBasics
{
	private IMainWindowBlock _windowBlock;

	private readonly IMessageLogGlobal _logGlobal;

	private readonly MainWindowViewModel _mainWindowViewModel;

	private readonly IDocManager _docManager;

	private readonly IDoEvents _doEvents;

	private readonly IConfigProvider _configProvider;

	public PnCommandBasics(IMainWindowBlock windowBlock, IMessageLogGlobal logGlobal, MainWindowViewModel mainWindowViewModel, IDocManager docManager, IDoEvents doEvents, IConfigProvider configProvider)
	{
		_windowBlock = windowBlock;
		_logGlobal = logGlobal;
		_mainWindowViewModel = mainWindowViewModel;
		_docManager = docManager;
		_doEvents = doEvents;
		_configProvider = configProvider;
	}

	private void AddHistory(F2exeReturnCode result, TimeSpan duration, DateTime timeEnd, [CallerMemberName] string CommandKey = "", [CallerFilePath] string CommandFilePath = "")
	{
	}

	public F2exeReturnCode DoCmd(IPnCommandArg arg, Func<IPnCommandArg, F2exeReturnCode> func, bool showWaitCursor = true, bool blockRibbon = true, bool mainThread = true, [CallerMemberName] string CommandKey = "", [CallerFilePath] string CommandFilePath = "")
	{
		IMessageDisplay logger = _logGlobal.WithContext(arg.Doc);
		string logCmdSuffix = string.Empty;
		if (!string.IsNullOrEmpty(arg.CommandParam))
		{
			logCmdSuffix = " '" + arg.CommandParam + "'";
		}
		if (_mainWindowViewModel.IsHelpMode)
		{
			logger.LogDebug($"Calling Help for Command:{arg.CommandGroup}, {arg.CommandStr}{logCmdSuffix}");
			_mainWindowViewModel.ActivateHelpForCommand(arg.CommandStr);
			return F2exeReturnCode.HELP_STARTED;
		}
		if (_windowBlock.BlockUI_IsBlock(arg.Doc))
		{
			logger.LogDebug($"Skip Command:{arg.CommandGroup}, {arg.CommandStr}{logCmdSuffix} because UI is blocked");
			return F2exeReturnCode.ERROR_DOC_BUSY;
		}
		ICurrentCalculation currentCalculation = null;
		if (arg.CalculationArg != null)
		{
			currentCalculation = arg.ScopedFactorio.Resolve<ICurrentCalculation>();
			if (!currentCalculation.TryStartNewCalculation(arg.CalculationArg))
			{
				logger.LogDebug($"Skip Command:{arg.CommandGroup}, {arg.CommandStr}{logCmdSuffix} because UI is blocked");
				return F2exeReturnCode.ERROR_DOC_BUSY;
			}
		}
		else if (blockRibbon)
		{
			_windowBlock.BlockUI_Block(arg.Doc);
		}
		logger.LogDebug($"Start Command:{arg.CommandGroup}, {arg.CommandStr}{logCmdSuffix}");
		if (showWaitCursor)
		{
			_windowBlock.InitWait(arg.Doc);
		}
		Stopwatch sw = new Stopwatch();
		sw.Start();
		F2exeReturnCode result = F2exeReturnCode.ERROR_FATAL;
		if (mainThread || _configProvider.InjectOrCreate<GeneralUserSettingsConfig>().CalculateBackground != true)
		{
			try
			{
				result = func(arg);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				sw.Stop();
				if (showWaitCursor)
				{
					_windowBlock.CloseWait(arg.Doc);
				}
				if (arg.CalculationArg != null)
				{
					currentCalculation.EndCalculation(arg.CalculationArg);
				}
				else if (blockRibbon)
				{
					_windowBlock.BlockUI_Unblock(arg.Doc);
				}
				logger.LogDebug($"CommandFailed:{arg.CommandGroup}, {arg.CommandStr}{logCmdSuffix} in {(double)sw.ElapsedMilliseconds / 1000.0} sec.");
				throw new Exception("CommandFailed " + arg.CommandStr, ex);
			}
		}
		else
		{
			bool waiting = true;
			Exception exception = null;
			Task.Run(delegate
			{
				result = func(arg);
			}).ContinueWith(delegate(Task t)
			{
				if (t.IsFaulted)
				{
					AggregateException exception2 = t.Exception;
					Console.WriteLine(exception2);
					sw.Stop();
					if (showWaitCursor)
					{
						_windowBlock.CloseWait(arg.Doc);
					}
					if (arg.CalculationArg != null)
					{
						currentCalculation.EndCalculation(arg.CalculationArg);
					}
					else if (blockRibbon)
					{
						_windowBlock.BlockUI_Unblock(arg.Doc);
					}
					logger.LogDebug($"CommandFailed:{arg.CommandGroup}, {arg.CommandStr}{logCmdSuffix} in {(double)sw.ElapsedMilliseconds / 1000.0} sec.");
					exception = new Exception("CommandFailed " + arg.CommandStr, exception2);
				}
				waiting = false;
			}, TaskScheduler.FromCurrentSynchronizationContext());
			while (waiting)
			{
				_doEvents.DoEvents(50);
				if (Keyboard.IsKeyDown(Key.Escape))
				{
					arg.CalculationArg?.TryCancelCalculation();
				}
			}
			if (exception != null)
			{
				throw exception;
			}
		}
		sw.Stop();
		if (showWaitCursor)
		{
			_windowBlock.CloseWait(arg.Doc);
		}
		if (arg.CalculationArg != null)
		{
			currentCalculation.EndCalculation(arg.CalculationArg);
		}
		else if (blockRibbon)
		{
			_windowBlock.BlockUI_Unblock(arg.Doc);
		}
		logger.LogDebug($"CommandEnded:{arg.CommandGroup}, {arg.CommandStr}{logCmdSuffix}: {result} in {(double)sw.ElapsedMilliseconds / 1000.0} sec.");
		if (!arg.IsReadOnly)
		{
			AddHistory(result, sw.Elapsed, DateTime.Now, CommandKey, CommandFilePath);
		}
		return result;
	}

	public IPnCommandArg CreateCommandArg(IPnBndDoc doc, string commandStr)
	{
		return new PnCommandArg(doc, _docManager.GetScope(doc))
		{
			CommandStr = commandStr
		};
	}

	public void CmdNotAvailable(IPnCommandArg arg)
	{
		_logGlobal.LogErrorMessage("Command '" + arg.CommandStr + "' not available in Gui mode");
	}
}
