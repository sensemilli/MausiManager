using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Contracts.BendPP;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.Assembly.Doc;

public interface IPnBndDocExt
{
	string AssemblyGuid { get; set; }

	int DocAssemblyId { get; set; }

	string ImportedFilename { get; }

	string SavedFileName { get; }

	int SavedArchiveNumber { get; }

	string Comment { get; set; }

	string Classification { get; set; }

	string DrawingNumber { get; set; }

	string Id { get; }

	bool UseDINUnfold { get; set; }

	int MaterialNumber { get; set; }

	IMaterialArt Material { get; set; }

	double Thickness { get; set; }

	Model InputModel3D { get; set; }

	Model ReconstructedEntryModel { get; set; }

	Model EntryModel3D { get; set; }

	Model ModifiedEntryModel3D { get; }

	Model UnfoldModel3D { get; }

	Model BendModel3D { get; }

	Model View3DModel { get; set; }

	int VisibleFaceGroupId { get; set; }

	int VisibleFaceGroupSide { get; set; }

	bool VisibleFaceIsTopFace { get; }

	IReadOnlyList<IBendDescriptor> BendDescriptors { get; }

	IReadOnlyList<ICombinedBendDescriptor> CombinedBendDescriptors { get; }

	bool HasBendMachine { get; }

	IBendMachine? BendMachine { get; }

	IToolsAndBends? ToolsAndBends { get; set; }

	string NamePP { get; }

	int? NumberPp { get; set; }

	Dictionary<int, string> UserComments { get; set; }

	IPostProcessor? PostProcessor { get; }

	string BendTempFolder { get; }

	bool ZeroBordersAdded2D { get; set; }

	void Close();

	void SetNamesPpBase(IEnumerable<string> subPpNames);
}
