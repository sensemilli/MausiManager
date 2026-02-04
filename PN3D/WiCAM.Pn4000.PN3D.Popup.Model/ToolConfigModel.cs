using System.Collections.ObjectModel;
using System.Linq;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Adapter;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Dies;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Holder;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Punches;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.PN3D.Popup.Model;

public class ToolConfigModel : ViewModelBase
{
	private ObservableCollection<PunchGroupViewModel> _punchGroups = new ObservableCollection<PunchGroupViewModel>();

	private ObservableCollection<PunchProfileViewModel> _punchProfiles = new ObservableCollection<PunchProfileViewModel>();

	private ObservableCollection<PunchPartViewModel> _punchParts = new ObservableCollection<PunchPartViewModel>();

	private ObservableCollection<SensorDiskViewModel> _sensorDisks = new ObservableCollection<SensorDiskViewModel>();

	private ObservableCollection<DieGroupViewModel> _dieGroups = new ObservableCollection<DieGroupViewModel>();

	private ObservableCollection<DieProfileViewModel> _dieProfiles = new ObservableCollection<DieProfileViewModel>();

	private ObservableCollection<DiePartViewModel> _dieParts = new ObservableCollection<DiePartViewModel>();

	private ObservableCollection<HemProfileViewModel> _frontHemProfiles = new ObservableCollection<HemProfileViewModel>();

	private ObservableCollection<HemPartViewModel> _frontHemParts = new ObservableCollection<HemPartViewModel>();

	private ObservableCollection<HemProfileViewModel> _backHemProfiles = new ObservableCollection<HemProfileViewModel>();

	private ObservableCollection<HemPartViewModel> _backHemParts = new ObservableCollection<HemPartViewModel>();

	private ObservableCollection<AdapterProfileViewModel> _upperAdapterProfiles = new ObservableCollection<AdapterProfileViewModel>();

	private ObservableCollection<AdapterPartViewModel> _upperAdapterParts = new ObservableCollection<AdapterPartViewModel>();

	private ObservableCollection<AdapterProfileViewModel> _lowerAdapterProfiles = new ObservableCollection<AdapterProfileViewModel>();

	private ObservableCollection<AdapterPartViewModel> _lowerAdapterParts = new ObservableCollection<AdapterPartViewModel>();

	private ObservableCollection<HolderProfileViewModel> _upperHolderProfiles = new ObservableCollection<HolderProfileViewModel>();

	private ObservableCollection<HolderPartViewModel> _upperHolderParts = new ObservableCollection<HolderPartViewModel>();

	private ObservableCollection<HolderProfileViewModel> _lowerHolderProfiles = new ObservableCollection<HolderProfileViewModel>();

	private ObservableCollection<HolderPartViewModel> _lowerHolderParts = new ObservableCollection<HolderPartViewModel>();

	private ObservableCollection<PreferredProfileViewModel> _preferredProfiles = new ObservableCollection<PreferredProfileViewModel>();

	private ObservableCollection<IPreferredFoldProfileViewModel> _preferredFoldProfiles = new ObservableCollection<IPreferredFoldProfileViewModel>();

	private ObservableCollection<Material3DGroupViewModel> _material3DGroups = new ObservableCollection<Material3DGroupViewModel>();

	public ObservableCollection<PunchGroupViewModel> PunchGroups
	{
		get
		{
			return this._punchGroups;
		}
		set
		{
			this._punchGroups = value;
			base.NotifyPropertyChanged("PunchGroups");
		}
	}

	public ObservableCollection<PunchProfileViewModel> PunchProfiles
	{
		get
		{
			return this._punchProfiles;
		}
		set
		{
			this._punchProfiles = value;
			base.NotifyPropertyChanged("PunchProfiles");
		}
	}

	public ObservableCollection<PunchPartViewModel> PunchParts
	{
		get
		{
			return this._punchParts;
		}
		set
		{
			this._punchParts = value;
			base.NotifyPropertyChanged("PunchParts");
		}
	}

	public ObservableCollection<SensorDiskViewModel> SensorDisks
	{
		get
		{
			return this._sensorDisks;
		}
		set
		{
			this._sensorDisks = value;
			base.NotifyPropertyChanged("SensorDisks");
		}
	}

	public ObservableCollection<DieGroupViewModel> DieGroups
	{
		get
		{
			return this._dieGroups;
		}
		set
		{
			this._dieGroups = value;
			base.NotifyPropertyChanged("DieGroups");
		}
	}

	public ObservableCollection<DieProfileViewModel> DieProfiles
	{
		get
		{
			return this._dieProfiles;
		}
		set
		{
			this._dieProfiles = value;
			base.NotifyPropertyChanged("DieProfiles");
		}
	}

	public ObservableCollection<DiePartViewModel> DieParts
	{
		get
		{
			return this._dieParts;
		}
		set
		{
			this._dieParts = value;
			base.NotifyPropertyChanged("DieParts");
		}
	}

	public ObservableCollection<HemProfileViewModel> FrontHemProfiles
	{
		get
		{
			return this._frontHemProfiles;
		}
		set
		{
			this._frontHemProfiles = value;
			base.NotifyPropertyChanged("FrontHemProfiles");
		}
	}

	public ObservableCollection<HemPartViewModel> FrontHemParts
	{
		get
		{
			return this._frontHemParts;
		}
		set
		{
			this._frontHemParts = value;
			base.NotifyPropertyChanged("FrontHemParts");
		}
	}

	public ObservableCollection<HemProfileViewModel> BackHemProfiles
	{
		get
		{
			return this._backHemProfiles;
		}
		set
		{
			this._backHemProfiles = value;
			base.NotifyPropertyChanged("BackHemProfiles");
		}
	}

	public ObservableCollection<HemPartViewModel> BackHemParts
	{
		get
		{
			return this._backHemParts;
		}
		set
		{
			this._backHemParts = value;
			base.NotifyPropertyChanged("BackHemParts");
		}
	}

	public ObservableCollection<PreferredProfileViewModel> PreferredProfiles
	{
		get
		{
			return this._preferredProfiles;
		}
		set
		{
			this._preferredProfiles = value;
			base.NotifyPropertyChanged("PreferredProfiles");
		}
	}

	public ObservableCollection<IPreferredFoldProfileViewModel> PreferredFoldProfiles
	{
		get
		{
			return this._preferredFoldProfiles;
		}
		set
		{
			this._preferredFoldProfiles = value;
			base.NotifyPropertyChanged("PreferredFoldProfiles");
		}
	}

	public ObservableCollection<AdapterProfileViewModel> UpperAdapterProfiles
	{
		get
		{
			return this._upperAdapterProfiles;
		}
		set
		{
			this._upperAdapterProfiles = value;
			base.NotifyPropertyChanged("UpperAdapterProfiles");
		}
	}

	public ObservableCollection<AdapterPartViewModel> UpperAdapterParts
	{
		get
		{
			return this._upperAdapterParts;
		}
		set
		{
			this._upperAdapterParts = value;
			base.NotifyPropertyChanged("UpperAdapterParts");
		}
	}

	public ObservableCollection<AdapterProfileViewModel> LowerAdapterProfiles
	{
		get
		{
			return this._lowerAdapterProfiles;
		}
		set
		{
			this._lowerAdapterProfiles = value;
			base.NotifyPropertyChanged("LowerAdapterProfiles");
		}
	}

	public ObservableCollection<AdapterPartViewModel> LowerAdapterParts
	{
		get
		{
			return this._lowerAdapterParts;
		}
		set
		{
			this._lowerAdapterParts = value;
			base.NotifyPropertyChanged("LowerAdapterParts");
		}
	}

	public ObservableCollection<HolderProfileViewModel> UpperHolderProfiles
	{
		get
		{
			return this._upperHolderProfiles;
		}
		set
		{
			this._upperHolderProfiles = value;
			base.NotifyPropertyChanged("UpperHolderProfiles");
		}
	}

	public ObservableCollection<HolderPartViewModel> UpperHolderParts
	{
		get
		{
			return this._upperHolderParts;
		}
		set
		{
			this._upperHolderParts = value;
			base.NotifyPropertyChanged("UpperHolderParts");
		}
	}

	public ObservableCollection<HolderProfileViewModel> LowerHolderProfiles
	{
		get
		{
			return this._lowerHolderProfiles;
		}
		set
		{
			this._lowerHolderProfiles = value;
			base.NotifyPropertyChanged("LowerHolderProfiles");
		}
	}

	public ObservableCollection<HolderPartViewModel> LowerHolderParts
	{
		get
		{
			return this._lowerHolderParts;
		}
		set
		{
			this._lowerHolderParts = value;
			base.NotifyPropertyChanged("LowerHolderParts");
		}
	}

	public ObservableCollection<Material3DGroupViewModel> Material3DGroups
	{
		get
		{
			return this._material3DGroups;
		}
		set
		{
			this._material3DGroups = value;
			base.NotifyPropertyChanged("Material3DGroups");
		}
	}

	public ToolConfigModel(BendMachine bendMachine, IMaterialManager materials)
	{
		if (bendMachine == null)
		{
			return;
		}
		this.Material3DGroups.Add(new Material3DGroupViewModel());
		foreach (IMaterialUnf item in materials.Material3DGroups.OrderBy((IMaterialUnf i) => i.Number))
		{
			this.Material3DGroups.Add(new Material3DGroupViewModel(item));
		}
		if (bendMachine.Adapter != null)
		{
			foreach (AdapterProfile item2 in bendMachine.Adapter.UpperAdapterProfiles.OrderBy((AdapterProfile i) => i.Name))
			{
				this.UpperAdapterProfiles.Add(new AdapterProfileViewModel(item2));
			}
			foreach (AdapterPart item3 in bendMachine.Adapter.UpperAdapterParts.OrderBy((AdapterPart i) => i.Name))
			{
				this.UpperAdapterParts.Add(new AdapterPartViewModel(item3));
			}
		}
		if (bendMachine.Adapter != null)
		{
			foreach (AdapterProfile item4 in bendMachine.Adapter.LowerAdapterProfiles.OrderBy((AdapterProfile i) => i.Name))
			{
				this.LowerAdapterProfiles.Add(new AdapterProfileViewModel(item4));
			}
			foreach (AdapterPart item5 in bendMachine.Adapter.LowerAdapterParts.OrderBy((AdapterPart i) => i.Name))
			{
				this.LowerAdapterParts.Add(new AdapterPartViewModel(item5));
			}
		}
		if (bendMachine.Holder != null)
		{
			foreach (HolderProfile item6 in bendMachine.Holder.UpperHolderProfiles.OrderBy((HolderProfile i) => i.Name))
			{
				this.UpperHolderProfiles.Add(new HolderProfileViewModel(item6));
			}
			foreach (HolderPart item7 in bendMachine.Holder.UpperHolderParts.OrderBy((HolderPart i) => i.Name))
			{
				this.UpperHolderParts.Add(new HolderPartViewModel(item7));
			}
		}
		if (bendMachine.Holder != null)
		{
			foreach (HolderProfile item8 in bendMachine.Holder.LowerHolderProfiles.OrderBy((HolderProfile i) => i.Name))
			{
				this.LowerHolderProfiles.Add(new HolderProfileViewModel(item8));
			}
			foreach (HolderPart item9 in bendMachine.Holder.LowerHolderParts.OrderBy((HolderPart i) => i.Name))
			{
				this.LowerHolderParts.Add(new HolderPartViewModel(item9));
			}
		}
		if (bendMachine.Punches?.SensorDisks != null)
		{
			foreach (SensorDisk item10 in bendMachine.Punches.SensorDisks.OrderBy((SensorDisk i) => i.Name))
			{
				this.SensorDisks.Add(new SensorDiskViewModel(item10));
			}
		}
		if (bendMachine.Dies == null)
		{
			return;
		}
		foreach (HemProfile item11 in from p in bendMachine.Dies.HemProfiles
			where p.HemID == 32
			select p into i
			orderby i.Name
			select i)
		{
			this.FrontHemProfiles.Add(new HemProfileViewModel(item11));
		}
		foreach (HemPart item12 in from p in bendMachine.Dies.HemParts
			where this.FrontHemProfiles.Any((HemProfileViewModel y) => y.ID == p.ProfileID)
			select p into i
			orderby i.Name
			select i)
		{
			this.FrontHemParts.Add(new HemPartViewModel(item12));
		}
		foreach (HemProfile item13 in from p in bendMachine.Dies.HemProfiles
			where p.HemID == 33
			select p into i
			orderby i.Name
			select i)
		{
			this.BackHemProfiles.Add(new HemProfileViewModel(item13));
		}
		foreach (HemPart item14 in from p in bendMachine.Dies.HemParts
			where this.BackHemProfiles.Any((HemProfileViewModel y) => y.ID == p.ProfileID)
			select p into i
			orderby i.Name
			select i)
		{
			this.BackHemParts.Add(new HemPartViewModel(item14));
		}
		foreach (PunchGroup item15 in bendMachine.Punches.PunchGroups.OrderBy((PunchGroup i) => i.Name))
		{
			this.PunchGroups.Add(new PunchGroupViewModel(item15));
		}
		foreach (PunchProfile punch in bendMachine.Punches.PunchProfiles.OrderBy((PunchProfile i) => i.Name))
		{
			AdapterProfileViewModel adapter = null;
			if (punch.AdapterID >= 0)
			{
				adapter = this.LowerAdapterProfiles?.FirstOrDefault((AdapterProfileViewModel a) => a.ID == punch.AdapterID);
			}
			HolderProfileViewModel holder = null;
			if (punch.HolderID >= 0)
			{
				holder = this.LowerHolderProfiles?.FirstOrDefault((HolderProfileViewModel h) => h.ID == punch.HolderID);
			}
			this.PunchProfiles.Add(new PunchProfileViewModel(punch, this.PunchGroups.FirstOrDefault((PunchGroupViewModel g) => g.ID == punch.GroupID), adapter, holder));
		}
		foreach (PunchPart part in bendMachine.Punches.PunchParts.OrderBy((PunchPart i) => i.Name))
		{
			this.PunchParts.Add(new PunchPartViewModel(part, this.SensorDisks.FirstOrDefault((SensorDiskViewModel s) => s.ID == part.SensorDiskID)));
		}
		foreach (DieGroup item16 in bendMachine.Dies.DieGroups.OrderBy((DieGroup i) => i.Name))
		{
			this.DieGroups.Add(new DieGroupViewModel(item16));
		}
		foreach (DieProfile die in bendMachine.Dies.DieProfiles.OrderBy((DieProfile i) => i.Name))
		{
			AdapterProfileViewModel adapter2 = null;
			if (die.AdapterID >= 0)
			{
				adapter2 = this.LowerAdapterProfiles?.FirstOrDefault((AdapterProfileViewModel a) => a.ID == die.AdapterID);
			}
			HolderProfileViewModel holder2 = null;
			if (die.HolderID >= 0)
			{
				holder2 = this.LowerHolderProfiles?.FirstOrDefault((HolderProfileViewModel h) => h.ID == die.HolderID);
			}
			HemProfileViewModel frontHemPart = null;
			if (die.FrontHemPartID >= 0)
			{
				frontHemPart = this.FrontHemProfiles?.FirstOrDefault((HemProfileViewModel h) => h.ID == die.FrontHemPartID);
			}
			HemProfileViewModel backHemPart = null;
			if (die.BackHemPartID >= 0)
			{
				backHemPart = this.BackHemProfiles?.FirstOrDefault((HemProfileViewModel h) => h.ID == die.BackHemPartID);
			}
			this.DieProfiles.Add(new DieProfileViewModel(die, this.DieGroups.FirstOrDefault((DieGroupViewModel g) => g.ID == die.GroupID), adapter2, holder2, frontHemPart, backHemPart));
		}
		foreach (DiePart item17 in bendMachine.Dies.DieParts.OrderBy((DiePart i) => i.Name))
		{
			this.DieParts.Add(new DiePartViewModel(item17));
		}
		foreach (PreferredProfileDeprecated profile in from i in bendMachine.PreferredProfiles.Profiles
			orderby i.PunchGroupName, i.DieGroupName
			select i)
		{
			this.PreferredProfiles.Add(new PreferredProfileViewModel(profile, this.PunchGroups.FirstOrDefault((PunchGroupViewModel p) => p.ID == profile.PunchGroupId), this.DieGroups.FirstOrDefault((DieGroupViewModel d) => d.ID == profile.DieGroupId), this.Material3DGroups.FirstOrDefault((Material3DGroupViewModel m) => m.Number == profile.MaterialGroupID)));
		}
	}
}
