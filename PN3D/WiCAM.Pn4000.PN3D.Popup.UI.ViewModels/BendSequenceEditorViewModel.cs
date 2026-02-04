using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class BendSequenceEditorViewModel : global::Telerik.Windows.Controls.ViewModelBase
{
	public class SequenceTranslation
	{
		public string Desc { get; }

		public BendSequenceSorts Value { get; }

		public SequenceTranslation(string desc, BendSequenceSorts value)
		{
			this.Desc = desc;
			this.Value = value;
		}
	}

	private bool? _bndAutoUseCalcBendOrder;

	public Action<ChangedConfigType> DataChanged;

	private IToolCalculationSettings _setup;

	private readonly IMachineBendFactory _machineFactory;

	private int _bruteforceBendOrderAmount;

	private StrategyViewModel _selectedStrategy;

	private IBendSequenceStrategyFactory _bendSequenceFactory;

	public bool? BndAutoUseCalcBendOrder
	{
		get
		{
			return this._bndAutoUseCalcBendOrder;
		}
		set
		{
			if (this._bndAutoUseCalcBendOrder != value)
			{
				this._bndAutoUseCalcBendOrder = value;
				base.RaisePropertyChanged("BndAutoUseCalcBendOrder");
			}
		}
	}

	public int BruteforceBendOrderAmount
	{
		get
		{
			return this._bruteforceBendOrderAmount;
		}
		set
		{
			if (this._bruteforceBendOrderAmount != value)
			{
				this._bruteforceBendOrderAmount = value;
				base.RaisePropertyChanged("BruteforceBendOrderAmount");
			}
		}
	}

	public int? OrderOptionAmountCalculations { get; set; }

	public double? OrderOptionSubSimStepMaxAngleDeg { get; set; }

	public ITranslator Translator { get; }

	public RadObservableCollection<StrategyViewModel> AllStrategies { get; } = new RadObservableCollection<StrategyViewModel>();

	public RelayCommand CmdNewStrategy { get; set; }

	public Dictionary<BendSequenceSorts, SequenceTranslation> SequenceItemTranslation { get; } = new Dictionary<BendSequenceSorts, SequenceTranslation>();

	public IEnumerable<SequenceTranslation> SequenceItems
	{
		get
		{
			return this.SequenceItemTranslation.Values;
		}
		set
		{
		}
	}

	public SequenceTranslation SelectedNewItem
	{
		get
		{
			return null;
		}
		set
		{
			if (value != null && this.SelectedStrategy != null)
			{
				this.SelectedStrategy.AddItem(value.Value);
				base.RaisePropertyChanged("SelectedNewItem");
			}
		}
	}

	public SequenceTranslation SelectedNewGroupItem
	{
		get
		{
			return null;
		}
		set
		{
			if (value != null && this.SelectedStrategy != null)
			{
				this.SelectedStrategy.AddGroupItem(value.Value);
				base.RaisePropertyChanged("SelectedNewGroupItem");
			}
		}
	}

	public StrategyViewModel SelectedStrategy
	{
		get
		{
			return this._selectedStrategy;
		}
		set
		{
			if (this._selectedStrategy != value)
			{
				this._selectedStrategy = value;
				base.RaisePropertyChanged("SelectedStrategy");
			}
		}
	}

	public BendSequenceEditorViewModel(ITranslator translator, IMachineBendFactory machineFactory, IBendSequenceStrategyFactory bendSequenceFactory)
	{
		this._machineFactory = machineFactory;
		this._bendSequenceFactory = bendSequenceFactory;
		this.Translator = translator;
	}

	public BendSequenceEditorViewModel Init(IToolCalculationSettings setup)
	{
		this._setup = setup;
		foreach (BendSequenceSorts value in global::System.Enum.GetValues(typeof(BendSequenceSorts)))
		{
			this.SequenceItemTranslation.Add(value, new SequenceTranslation(this.Translator.Translate($"l_enum.BendSequence.{value}"), value));
		}
		this.CmdNewStrategy = new RelayCommand(AddStrategy);
		this.LoadList();
		return this;
	}

	public void Save()
	{
		List<IBendSequenceOrder> list = this.AllStrategies.Select(delegate(StrategyViewModel x)
		{
			IBendSequenceOrder bendSequenceOrder = this._bendSequenceFactory.CreateNewSequenceOrder(x.Description, x.Enabled, x.Id);
			bendSequenceOrder.Groupings = new List<ISequenceGrouping>();
			bendSequenceOrder.Sequences = x.Items.Select((StrategyItemViewModel y) => y.Value).ToList();
			if (x.GroupParallels)
			{
				ISequenceGrouping sequenceGrouping = this._bendSequenceFactory.CreateGrouping(null, 1);
				sequenceGrouping.InnerSortSequences = x.GroupItems.Select((StrategyItemViewModel y) => y.Value).ToList();
				bendSequenceOrder.Groupings.Add(sequenceGrouping);
			}
			return bendSequenceOrder;
		}).ToList();
		ChangedConfigType obj = (this.CheckForChanges(list) ? ChangedConfigType.BendSequence : ChangedConfigType.NoChanges);
		this._setup.BruteforceBendOrderAmount = this.BruteforceBendOrderAmount;
		this._setup.BendOrderStrategies = list;
		this._setup.BndAutoCalcBendOrder = this.BndAutoUseCalcBendOrder;
		this._setup.OrderOptionSubSimStepMaxAngle = this.OrderOptionSubSimStepMaxAngleDeg * Math.PI / 180.0;
		this._setup.BendOrderCalcAmountCalculations = this.OrderOptionAmountCalculations;
		this.DataChanged?.Invoke(obj);
	}

	public void DeleteStrategy(StrategyViewModel obj)
	{
		this.AllStrategies.Remove(obj);
		if (this.SelectedStrategy == obj)
		{
			this.SelectedStrategy = null;
		}
	}

	public new void Dispose()
	{
	}

	public void LoadList()
	{
		this.OrderOptionSubSimStepMaxAngleDeg = this._setup.OrderOptionSubSimStepMaxAngle * 180.0 / Math.PI;
		this.OrderOptionAmountCalculations = this._setup.BendOrderCalcAmountCalculations;
		this.BruteforceBendOrderAmount = this._setup.BruteforceBendOrderAmount;
		this.BndAutoUseCalcBendOrder = this._setup.BndAutoCalcBendOrder;
		this.AllStrategies.Clear();
		this.AllStrategies.AddRange(this._setup.BendOrderStrategies.Select((IBendSequenceOrder strategy) => new StrategyViewModel(strategy, this)));
		this.SelectedStrategy = this.AllStrategies.FirstOrDefault();
		this.OnPropertyChanged("OrderOptionSubSimStepMaxAngleDeg");
		this.OnPropertyChanged("OrderOptionAmountCalculations");
	}

	private void AddStrategy()
	{
		StrategyViewModel item = new StrategyViewModel(new BendSequenceSorts[0], this, this.Translator.Translate("l_popup.BendSequenceEditorView.StrategyTemplate", this.AllStrategies.Count + 1));
		this.AllStrategies.Add(item);
	}

	private bool CheckForChanges(List<IBendSequenceOrder> newOrders)
	{
		if (this._setup.BruteforceBendOrderAmount != this.BruteforceBendOrderAmount || this._setup.OrderOptionSubSimStepMaxAngle != this.OrderOptionSubSimStepMaxAngleDeg * Math.PI / 180.0 || this._setup.BendOrderCalcAmountCalculations != this.OrderOptionAmountCalculations)
		{
			return true;
		}
		if (this._setup.BndAutoCalcBendOrder != this.BndAutoUseCalcBendOrder)
		{
			return true;
		}
		if (this._setup.BendOrderStrategies.Count != newOrders.Count)
		{
			return true;
		}
		for (int i = 0; i < newOrders.Count; i++)
		{
			if (!this.Equals(newOrders[i], this._setup.BendOrderStrategies[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool Equals(IBendSequenceOrder a, IBendSequenceOrder b)
	{
		if (a.Description != b.Description)
		{
			return false;
		}
		if (a.Enabled != b.Enabled)
		{
			return false;
		}
		if (!this.Equals(a.Sequences, b.Sequences))
		{
			return false;
		}
		if (a.Groupings.Count != b.Groupings.Count)
		{
			return false;
		}
		for (int i = 0; i < a.Groupings.Count; i++)
		{
			if (!this.Equals(a.Groupings[i], b.Groupings[i]))
			{
				return false;
			}
		}
		return true;
	}

	private bool Equals(ISequenceGrouping a, ISequenceGrouping b)
	{
		if (a.Description != b.Description)
		{
			return false;
		}
		if (a.GroupingType != b.GroupingType)
		{
			return false;
		}
		if (!this.Equals(a.InnerSortSequences, b.InnerSortSequences))
		{
			return false;
		}
		return true;
	}

	private bool Equals(List<BendSequenceSorts> a, List<BendSequenceSorts> b)
	{
		if (a.Count != b.Count)
		{
			return false;
		}
		for (int i = 0; i < a.Count; i++)
		{
			if (a[i] != b[i])
			{
				return false;
			}
		}
		return true;
	}
}
