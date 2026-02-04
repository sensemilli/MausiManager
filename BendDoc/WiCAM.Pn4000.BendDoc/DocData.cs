using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.PN3D.BendSimulation;
using WiCAM.Pn4000.PN3D.Tool.Interfaces;

namespace WiCAM.Pn4000.BendDoc;

internal class DocData
{
	public bool UseDINUnfold { get; set; }

	public IMaterialArt Material { get; set; }

	public bool MaterialSet { get; set; }

	public Model? InputModel3D { get; set; }

	public Model? EntryModel3D { get; set; }

	public Model? ReconstructedEntryModel { get; set; }

	public Model? ModifiedEntryModel3D { get; set; }

	public Model? UnfoldModel3D { get; set; }

	public Model? BendModel3D { get; set; }

	public Model View3DModel { get; set; }

	public IBendMachineSimulation? BendMachineConfig { get; set; }

	public IToolExpert Tools { get; set; }

	public List<BendDescriptor> BendDescriptors { get; }

	public List<CombinedBendDescriptor> CombinedBendDescriptors { get; }

	public int AmountInAssembly { get; set; }

	public string Comment { get; set; }

	public string NamePP { get; set; }

	public List<string> NamesPpBase { get; set; }

	public string NamePPSuffix { get; set; } = "";

	public int? NumberPp { get; set; }

	public SetNcTimestampTypes NamePpTimestamps { get; set; }

	public bool PnMaterialByUser { get; set; }

	public bool FreezeCombinedBendDescriptors { get; set; }

	public string AssemblyGuid { get; set; }

	public string CreationUserName { get; set; }

	public string LastModifiedUserName { get; set; }

	public DateTime CreationDate { get; set; }

	public DateTime LastModified { get; set; }

	public string Classification { get; set; }

	public string DrawingNumber { get; set; }

	public DocData()
	{
		this.InputModel3D = new Model();
		this.UnfoldModel3D = new Model();
		this.BendModel3D = new Model();
		this.ModifiedEntryModel3D = new Model();
		this.BendDescriptors = new List<BendDescriptor>();
		this.CombinedBendDescriptors = new List<CombinedBendDescriptor>();
		this.NamesPpBase = new List<string>();
	}

	public DocData(DocData docData, Doc3d newDoc)
	{
		newDoc._data = this;
		this.PnMaterialByUser = docData.PnMaterialByUser;
		this.InputModel3D = docData.InputModel3D;
		this.EntryModel3D = docData.EntryModel3D;
		this.UnfoldModel3D = docData.UnfoldModel3D;
		this.BendModel3D = docData.BendModel3D;
		if (this.BendModel3D != null)
		{
			foreach (Model item in this.BendModel3D.GetAllSubModelsWithSelf())
			{
				item.PartRole = PartRole.BendModel;
			}
		}
		this.ModifiedEntryModel3D = docData.ModifiedEntryModel3D;
		this.BendDescriptors = docData.BendDescriptors.ToList();
		Dictionary<CombinedBendDescriptor, CombinedBendDescriptor> dictCbd = new Dictionary<CombinedBendDescriptor, CombinedBendDescriptor>();
		this.CombinedBendDescriptors = docData.CombinedBendDescriptors.Select(delegate(CombinedBendDescriptor old)
		{
			CombinedBendDescriptor combinedBendDescriptor = new CombinedBendDescriptor(old, old.BendType, newDoc)
			{
				SplitPredecessors = old.SplitPredecessors?.Select((CombinedBendDescriptor o) => dictCbd[o]).ToList()
			};
			dictCbd.Add(old, combinedBendDescriptor);
			return combinedBendDescriptor;
		}).ToList();
		this.NamesPpBase = docData.NamesPpBase.ToList();
	}
}
