using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf.TabBend.OrderBillboards;

public interface IOrderBillboardsViewModel
{
	IDoc3d CurrentDoc { get; set; }

	void UpdateOrAddBillboards(bool render);

	void RemoveBillboards(bool render);

	void SetMaxCombinedBend(ICombinedBendDescriptorInternal? combinedBend);

	void SetSelectedAndHoveredCombinedBend(ICombinedBendDescriptorInternal? selected, ICombinedBendDescriptorInternal? hovered);

	IBendDescriptor? GetBend(IBillboard billboard);

	ICombinedBendDescriptorInternal? GetCombinedBend(IBillboard billboard);
}
