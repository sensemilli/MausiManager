using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.Contracts.ToolCalculation;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts.Implementations;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.ToolCalculation.Interfaces;
using ViewModelBase = Telerik.Windows.Controls.ViewModelBase;

namespace WiCAM.Pn4000.GuiWpf.TabBend.Developer;

internal class WinDebugViewModel : ViewModelBase
{
	[CompilerGenerated]
	private sealed class _003CSplitIntRanges_003Ed__93 : IEnumerable<int>, IEnumerable, IEnumerator<int>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private int _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string text;

		public string _003C_003E3__text;

		private string[] _003C_003E7__wrap1;

		private int _003C_003E7__wrap2;

		private int _003Cend_003E5__4;

		private int _003Ci_003E5__5;

		int IEnumerator<int>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSplitIntRanges_003Ed__93(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (string.IsNullOrWhiteSpace(this.text))
				{
					break;
				}
				_003C_003E7__wrap1 = this.text.Trim().Split(",");
				_003C_003E7__wrap2 = 0;
				goto IL_0124;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__5++;
				goto IL_00df;
			case 2:
				{
					_003C_003E1__state = -1;
					goto IL_0116;
				}
				IL_00df:
				if (_003Ci_003E5__5 <= _003Cend_003E5__4)
				{
					_003C_003E2__current = _003Ci_003E5__5;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0116;
				IL_0124:
				if (_003C_003E7__wrap2 < _003C_003E7__wrap1.Length)
				{
					string text = _003C_003E7__wrap1[_003C_003E7__wrap2];
					int result2;
					if (text.Contains("-"))
					{
						string[] array = text.Split('-');
						if (int.TryParse(array[0].Trim(), out var result) && int.TryParse(array[1].Trim(), out _003Cend_003E5__4))
						{
							_003Ci_003E5__5 = result;
							goto IL_00df;
						}
					}
					else if (int.TryParse(text.Trim(), out result2))
					{
						_003C_003E2__current = result2;
						_003C_003E1__state = 2;
						return true;
					}
					goto IL_0116;
				}
				_003C_003E7__wrap1 = null;
				break;
				IL_0116:
				_003C_003E7__wrap2++;
				goto IL_0124;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<int> IEnumerable<int>.GetEnumerator()
		{
			_003CSplitIntRanges_003Ed__93 _003CSplitIntRanges_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CSplitIntRanges_003Ed__ = this;
			}
			else
			{
				_003CSplitIntRanges_003Ed__ = new _003CSplitIntRanges_003Ed__93(0);
			}
			_003CSplitIntRanges_003Ed__.text = _003C_003E3__text;
			return _003CSplitIntRanges_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<int>)this).GetEnumerator();
		}
	}

	private readonly IFactorio _factorio;

	private readonly IToolOperator _toolOperator;

	private readonly IToolFactory _toolFactory;

	private readonly ICurrentCalculation _currentCalculation;

	private readonly IScreen3DMain _screen3DMain;

	private bool _topMost;

	private int _selectedSetup;

	private string _inputBendsToSelect;

	private IToolCalculationResult _result;

	private ICalculationArg? _toolCalculationOption;

	private CancellationTokenSource _toolCalcCancellationTokenSource;

	private List<ILogStep> _logSteps = new List<ILogStep>();

	private object? _logDetail;

	public IEditToolsMainView EditToolsView { get; }

	public bool TopMost
	{
		get
		{
			return _topMost;
		}
		set
		{
			_topMost = value;
			RaisePropertyChanged("TopMost");
		}
	}

	public RelayCommand CmdRefreshLogList { get; }

	public RelayCommand CmdRefreshVisibility { get; }

	public RelayCommand CmdTest01 { get; }

	public RelayCommand CmdVisualizeResult { get; }

	public RelayCommand CmdCenterResult { get; }

	public RelayCommand CmdNewToolStation { get; }

	public RelayCommand CmdAddToToolStation { get; }

	public RelayCommand CmdCancelToolCalculation { get; }

	public int SelectedSetup
	{
		get
		{
			return _selectedSetup;
		}
		set
		{
			if (_selectedSetup != value)
			{
				_selectedSetup = value;
				RaisePropertyChanged("SelectedSetup");
			}
		}
	}

	public string InputBendsToSelect
	{
		get
		{
			return _inputBendsToSelect;
		}
		set
		{
			if (_inputBendsToSelect != value)
			{
				_inputBendsToSelect = value;
				RaisePropertyChanged("InputBendsToSelect");
				SelectedBends = SplitIntRanges(value).ToList();
				RaisePropertyChanged("SelectedBendsString");
			}
		}
	}

	public List<int> SelectedBends { get; set; } = new List<int>();

	public string SelectedBendsString => string.Join(", ", SelectedBends);

	public string? Status => _toolCalculationOption?.Status;

	public ICalculationArg? ToolCalculationOption
	{
		get
		{
			return _toolCalculationOption;
		}
		set
		{
			if (_toolCalculationOption != null)
			{
				_toolCalculationOption.StatusChanged -= _toolCalculationOption_StatusChanged;
			}
			_toolCalculationOption = value;
			if (_toolCalculationOption != null)
			{
				_toolCalculationOption.StatusChanged += _toolCalculationOption_StatusChanged;
			}
		}
	}

	public ObservableCollection<LogStepViewModel>? LogSteps { get; set; } = new ObservableCollection<LogStepViewModel>();

	public object? LogDetail
	{
		get
		{
			return _logDetail;
		}
		set
		{
			if (_logDetail == value)
			{
				return;
			}
			if (_logDetail is Model model)
			{
				Screen3D.RemoveModel(model);
			}
			_logDetail = value;
			RaisePropertyChanged("LogDetail");
			RaisePropertyChanged("LogDetailEnumerable");
			RaisePropertyChanged("LogDetailEnumerableVisibility");
			RaisePropertyChanged("LogDetailModel");
			RaisePropertyChanged("LogDetailModelVisibility");
			if (!(_logDetail is Model model2))
			{
				return;
			}
			foreach (Model subModel in model2.SubModels)
			{
				subModel.SubModelsVisible = subModel.Visible;
			}
			Screen3D.AddModel(model2);
		}
	}

	public object? LogDetailEnumerable => (LogDetail as EnumerableNester)?.Enumerable;

	public List<Model>? LogDetailModel => (LogDetail as Model)?.SubModels;

	public Visibility LogDetailEnumerableVisibility
	{
		get
		{
			if (LogDetailEnumerable == null)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public Visibility LogDetailModelVisibility
	{
		get
		{
			if (LogDetailModel == null)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public IPnBndDoc Doc { get; private set; }

	public ScreenD3D11 Screen3D => _screen3DMain.ScreenD3D;

	public WinDebugViewModel(IFactorio factorio, IToolOperator toolOperator, IEditToolsMainView editToolsView, IToolFactory toolFactory, ICurrentCalculation currentCalculation, IPnBndDoc doc, IScreen3DMain screen3DMain)
	{
		Doc = doc;
		EditToolsView = editToolsView;
		_factorio = factorio;
		_toolOperator = toolOperator;
		_toolFactory = toolFactory;
		_currentCalculation = currentCalculation;
		_screen3DMain = screen3DMain;
		_currentCalculation.CurrentCalculationChanged += CurrentCalculationCurrentCalculationChanged;
		CmdTest01 = new RelayCommand(Test01);
		CmdVisualizeResult = new RelayCommand(VisualizeResult, HasResult);
		CmdCenterResult = new RelayCommand(CenterResult, HasResult);
		CmdNewToolStation = new RelayCommand(NewToolStation);
		CmdAddToToolStation = new RelayCommand(AddToToolStation);
		CmdCancelToolCalculation = new RelayCommand(CancelCalculation);
		CmdRefreshVisibility = new RelayCommand(RefreshVisibility);
		CmdRefreshLogList = new RelayCommand(RefreshLogList);
	}

	private void Test01()
	{
	}

	private void CurrentCalculationCurrentCalculationChanged(ICalculationArg? obj)
	{
		if (obj == null)
		{
			return;
		}
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			ToolCalculationOption = obj;
			if (ToolCalculationOption?.DebugInfo?.Log != null)
			{
				_logSteps.Add(ToolCalculationOption.DebugInfo.Log);
				RefreshLogList();
			}
		});
	}

	private bool HasResult()
	{
		return _result != null;
	}

	private void VisualizeResult()
	{
		_factorio.Resolve<IToolsToMachineModel>().AddTools(_result.Setup, Doc);
	}

	private void CenterResult()
	{
		_toolOperator.CenterTools(_result.Setup.Root);
	}

	private void _toolCalculationOption_StatusChanged(ICalculationArg status)
	{
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			RaisePropertyChanged("Status");
		});
	}

	private void NewToolStation()
	{
	}

	private void AddToToolStation()
	{
	}

	private void CancelCalculation()
	{
		_toolCalcCancellationTokenSource.Cancel();
	}

	private void RefreshVisibility()
	{
		object logDetail = LogDetail;
		LogDetail = null;
		LogDetail = logDetail;
	}

	private void RefreshLogList()
	{
		LogSteps = new ObservableCollection<LogStepViewModel>(_logSteps.Select((ILogStep x) => new LogStepViewModel(x)));
		RaisePropertyChanged("LogSteps");
	}

	[IteratorStateMachine(typeof(_003CSplitIntRanges_003Ed__93))]
	private IEnumerable<int> SplitIntRanges(string text)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSplitIntRanges_003Ed__93(-2)
		{
			_003C_003E3__text = text
		};
	}
}
