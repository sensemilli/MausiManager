using System;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

public class MaterialViewModel : ViewModelBase
{
	private string _materialNameLower;

	private string _materialGroupName;

	private string _materialGroupNameLower;

	private string _materialGroupForBendDeductionName;

	private string _materialGroupForBendDeductionNameLower;

	private string _descriptionLower;

	public IMaterialArt Material { get; set; }

	public string MaterialNameLower
	{
		get
		{
			return this._materialNameLower;
		}
		set
		{
			this._materialNameLower = value;
			base.NotifyPropertyChanged("MaterialNameLower");
		}
	}

	public int Number
	{
		get
		{
			return this.Material.Number;
		}
		set
		{
			this.Material.Number = value;
			base.NotifyPropertyChanged("Number");
		}
	}

	public string Name
	{
		get
		{
			return this.Material.Name;
		}
		set
		{
			this.Material.Name = value;
			base.NotifyPropertyChanged("Name");
		}
	}

	public double Density
	{
		get
		{
			return this.Material.Density;
		}
		set
		{
			this.Material.Density = value;
			base.NotifyPropertyChanged("Density");
		}
	}

	public int Rotation
	{
		get
		{
			return this.Material.Rotation;
		}
		set
		{
			this.Material.Rotation = value;
			base.NotifyPropertyChanged("Rotation");
		}
	}

	public int ReferenceNumber
	{
		get
		{
			return this.Material.ReferenceNumber;
		}
		set
		{
			this.Material.ReferenceNumber = value;
			base.NotifyPropertyChanged("ReferenceNumber");
		}
	}

	public string AlternativeMaterialNumbers
	{
		get
		{
			return this.Material.AlternativeMaterialNumbers;
		}
		set
		{
			this.Material.AlternativeMaterialNumbers = value;
			base.NotifyPropertyChanged("AlternativeMaterialNumbers");
		}
	}

	public int MaterialGroupId
	{
		get
		{
			return this.Material.MaterialGroupId;
		}
		set
		{
			this.Material.MaterialGroupId = value;
			base.NotifyPropertyChanged("MaterialGroupId");
		}
	}

	public string MaterialGroupName
	{
		get
		{
			return this._materialGroupName;
		}
		set
		{
			this._materialGroupName = value;
			base.NotifyPropertyChanged("MaterialGroupName");
		}
	}

	public string MaterialGroupNameLower
	{
		get
		{
			return this._materialGroupNameLower;
		}
		set
		{
			this._materialGroupNameLower = value;
			base.NotifyPropertyChanged("MaterialGroupNameLower");
		}
	}

	public string Description
	{
		get
		{
			return this.Material.Description;
		}
		set
		{
			this.Material.Description = value;
			base.NotifyPropertyChanged("Description");
		}
	}

	public string DescriptionLower
	{
		get
		{
			return this._descriptionLower;
		}
		set
		{
			this._descriptionLower = value;
			base.NotifyPropertyChanged("DescriptionLower");
		}
	}

	public string Remark01
	{
		get
		{
			return this.Material.Remark01;
		}
		set
		{
			this.Material.Remark01 = value;
			base.NotifyPropertyChanged("Remark01");
		}
	}

	public string Remark02
	{
		get
		{
			return this.Material.Remark02;
		}
		set
		{
			this.Material.Remark02 = value;
			base.NotifyPropertyChanged("Remark02");
		}
	}

	public string Remark03
	{
		get
		{
			return this.Material.Remark03;
		}
		set
		{
			this.Material.Remark03 = value;
			base.NotifyPropertyChanged("Remark03");
		}
	}

	public string Remark04
	{
		get
		{
			return this.Material.Remark04;
		}
		set
		{
			this.Material.Remark04 = value;
			base.NotifyPropertyChanged("Remark04");
		}
	}

	public string Remark05
	{
		get
		{
			return this.Material.Remark05;
		}
		set
		{
			this.Material.Remark05 = value;
			base.NotifyPropertyChanged("Remark05");
		}
	}

	public double ThicknessMin
	{
		get
		{
			return this.Material.ThicknessMin;
		}
		set
		{
			this.Material.ThicknessMin = value;
			base.NotifyPropertyChanged("ThicknessMin");
		}
	}

	public double? ThicknessMax => this.Material.ThicknessMax;

	public double YieldStrength
	{
		get
		{
			return this.Material.YieldStrength;
		}
		set
		{
			this.Material.YieldStrength = value;
			base.NotifyPropertyChanged("YieldStrength");
		}
	}

	public double TensileStrength
	{
		get
		{
			return this.Material.TensileStrength;
		}
		set
		{
			this.Material.TensileStrength = value;
			base.NotifyPropertyChanged("TensileStrength");
		}
	}

	public double HeatCapacity
	{
		get
		{
			return this.Material.HeatCapacity;
		}
		set
		{
			this.Material.HeatCapacity = value;
			base.NotifyPropertyChanged("HeatCapacity");
		}
	}

	public double WorkHardeningExponent
	{
		get
		{
			return this.Material.WorkHardeningExponent;
		}
		set
		{
			this.Material.WorkHardeningExponent = value;
			base.NotifyPropertyChanged("WorkHardeningExponent");
		}
	}

	public double EModul
	{
		get
		{
			return this.Material.EModul;
		}
		set
		{
			this.Material.EModul = value;
			base.NotifyPropertyChanged("EModul");
		}
	}

	public int MaterialGroupForBendDeduction
	{
		get
		{
			return this.Material.MaterialGroupForBendDeduction;
		}
		set
		{
			this.Material.MaterialGroupForBendDeduction = value;
			base.NotifyPropertyChanged("MaterialGroupForBendDeduction");
		}
	}

	public string MaterialGroupForBendDeductionName
	{
		get
		{
			return this._materialGroupForBendDeductionName;
		}
		set
		{
			this._materialGroupForBendDeductionName = value;
			base.NotifyPropertyChanged("MaterialGroupForBendDeductionName");
		}
	}

	public string MaterialGroupForBendDeductionNameLower
	{
		get
		{
			return this._materialGroupForBendDeductionNameLower;
		}
		set
		{
			this._materialGroupForBendDeductionNameLower = value;
			base.NotifyPropertyChanged("MaterialGroupForBendDeductionNameLower");
		}
	}

	public DateTime Modified
	{
		get
		{
			return this.Material.Modified;
		}
		set
		{
			this.Material.Modified = value;
			base.NotifyPropertyChanged("Modified");
		}
	}

	public MaterialViewModel(IMaterialArt material, IMaterialManager materials)
	{
		this.Material = material;
		this.MaterialNameLower = this.Material.Name.ToLower();
		this.MaterialGroupName = materials.GetMaterialGroupName(this.Material.MaterialGroupId);
		this.MaterialGroupNameLower = this.MaterialGroupName.ToLower();
		this.MaterialGroupForBendDeductionName = materials.GetMaterial3DGroupName(this.Material.MaterialGroupForBendDeduction);
		this.MaterialGroupForBendDeductionNameLower = this.MaterialGroupForBendDeductionName.ToLower();
		this.DescriptionLower = this.Material.Description.ToLower();
	}
}
