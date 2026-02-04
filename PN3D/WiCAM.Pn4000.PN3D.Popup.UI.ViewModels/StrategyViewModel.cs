using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class StrategyViewModel : global::Telerik.Windows.Controls.ViewModelBase
{
	private readonly BendSequenceEditorViewModel _mainVm;

	private string _description;

	private int _order;

	private bool _enabled;

	private bool _groupParallels;

	public string Description
	{
		get
		{
			return this._description;
		}
		set
		{
			if (this._description != value)
			{
				this._description = value;
				base.RaisePropertyChanged("Description");
			}
		}
	}

	public Guid Id { get; set; }

	public int Order
	{
		get
		{
			return this._order;
		}
		set
		{
			if (this._order != value)
			{
				this._order = value;
				base.RaisePropertyChanged("Order");
			}
		}
	}

	public bool Enabled
	{
		get
		{
			return this._enabled;
		}
		set
		{
			if (this._enabled != value)
			{
				this._enabled = value;
				base.RaisePropertyChanged("Enabled");
			}
		}
	}

	public bool GroupParallels
	{
		get
		{
			return this._groupParallels;
		}
		set
		{
			if (this._groupParallels != value)
			{
				this._groupParallels = value;
				base.RaisePropertyChanged("GroupParallels");
				base.RaisePropertyChanged("VisibilityGroupParallel");
			}
		}
	}

	public Visibility VisibilityGroupParallel
	{
		get
		{
			if (!this._groupParallels)
			{
				return Visibility.Hidden;
			}
			return Visibility.Visible;
		}
	}

	public RelayCommand CmdDelete { get; }

	public RelayCommand<StrategyItemViewModel> CmdDeleteItem { get; }

	public RelayCommand<StrategyItemViewModel> CmdDeleteGroupItem { get; }

	public RadObservableCollection<StrategyItemViewModel> Items { get; } = new RadObservableCollection<StrategyItemViewModel>();

	public RadObservableCollection<StrategyItemViewModel> GroupItems { get; } = new RadObservableCollection<StrategyItemViewModel>();

	public event Action<StrategyViewModel> Deleting;

	public override string ToString()
	{
		return this.Description;
	}

	public StrategyViewModel(IEnumerable<BendSequenceSorts> elements, BendSequenceEditorViewModel mainVm, string description)
	{
		this._mainVm = mainVm;
		this.Id = Guid.NewGuid();
		this._description = description;
		this._enabled = true;
		this.CmdDelete = new RelayCommand(Delete);
		this.CmdDeleteItem = new RelayCommand<StrategyItemViewModel>(DeleteItem);
		this.CmdDeleteGroupItem = new RelayCommand<StrategyItemViewModel>(DeleteGroupItem);
		this.Items.AddRange(elements.Select((BendSequenceSorts x) => new StrategyItemViewModel(x, this._mainVm)));
	}

	public StrategyViewModel(IBendSequenceOrder strategy, BendSequenceEditorViewModel mainVm)
	{
		this._mainVm = mainVm;
		this.Id = strategy.Id;
		this._description = strategy.Description;
		this._enabled = strategy.Enabled;
		ISequenceGrouping sequenceGrouping = strategy.Groupings?.FirstOrDefault((ISequenceGrouping x) => x.GroupingType == 1);
		this._groupParallels = sequenceGrouping != null;
		this.CmdDelete = new RelayCommand(Delete);
		this.CmdDeleteItem = new RelayCommand<StrategyItemViewModel>(DeleteItem);
		this.CmdDeleteGroupItem = new RelayCommand<StrategyItemViewModel>(DeleteGroupItem);
		this.Items.AddRange(strategy.Sequences.Select((BendSequenceSorts x) => new StrategyItemViewModel(x, this._mainVm)));
		if (sequenceGrouping != null)
		{
			this.GroupItems.AddRange(sequenceGrouping.InnerSortSequences.Select((BendSequenceSorts x) => new StrategyItemViewModel(x, this._mainVm)));
		}
	}

	public void Delete()
	{
		this._mainVm.DeleteStrategy(this);
	}

	public void DeleteItem(StrategyItemViewModel vm)
	{
		if (vm != null)
		{
			this.Items.Remove(vm);
		}
	}

	public void DeleteGroupItem(StrategyItemViewModel vm)
	{
		if (vm != null)
		{
			this.GroupItems.Remove(vm);
		}
	}

	public void AddItem(BendSequenceSorts item)
	{
		this.Items.Add(new StrategyItemViewModel(item, this._mainVm));
	}

	public void AddGroupItem(BendSequenceSorts item)
	{
		this.GroupItems.Add(new StrategyItemViewModel(item, this._mainVm));
	}
}
