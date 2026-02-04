using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.Model;

public class ContextMenuStepBendWithoutMachineModel
{
	private readonly IGlobals _globals;

	public FaceGroup SelectedBend { get; }

	public IDoc3d Doc { get; }

	public FaceGroup StepBendParent => this.Doc.EntryModel3D.GetFaceGroupById(this.SelectedBend.BendEntryId);

	public List<FaceGroup> StepBendsInUnfoldModel => (from x in this.Doc.UnfoldModel3D.GetAllRoundFaceGroups()
		where x.BendEntryId == this.StepBendParent.ID || x.SubGroups.Any((FaceGroup y) => y.BendEntryId == this.StepBendParent.ID)
		select x).ToList();

	public ContextMenuStepBendWithoutMachineModel(FaceGroup selectedBend, IDoc3d doc, IGlobals globals)
	{
		this._globals = globals;
		this.SelectedBend = selectedBend;
		this.Doc = doc;
	}

	public void ApplyParameters(int numBends, double radius, double? bendDeductionMiddle, double? bendDeductionStartEnd)
	{
		FaceGroup searchBend = this.SelectedBend.ParentGroup ?? this.SelectedBend;
		ICombinedBendDescriptorInternal elementToFind = this.Doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Enumerable.Any((IBendDescriptor y) => y.BendParams.EntryFaceGroup == searchBend || y.BendParams.ModifiedEntryFaceGroup == searchBend || y.BendParams.UnfoldFaceGroup == searchBend || y.BendParams.BendFaceGroup == searchBend));
		this.Doc.ChangeStepBendProperties(this.Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(elementToFind), numBends, radius, bendDeductionMiddle, bendDeductionStartEnd);
	}

	public void RepealStepBend()
	{
		FaceGroup searchBend = this.SelectedBend.ParentGroup ?? this.SelectedBend;
		ICombinedBendDescriptorInternal elementToFind = this.Doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Enumerable.Any((IBendDescriptor y) => y.BendParams.EntryFaceGroup == searchBend || y.BendParams.ModifiedEntryFaceGroup == searchBend || y.BendParams.UnfoldFaceGroup == searchBend || y.BendParams.BendFaceGroup == searchBend));
		this.Doc.ConvertStepBendToBend(this.Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(elementToFind));
	}
}
