using System.Windows.Media;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.Material;

internal interface IStatus3dMaterialViewModel
{
	Brush HeaderColor { get; }

	string Header { get; set; }

	IMaterialArt? Material { get; }

	string UiThickness { get; set; }

	string DisplayGroupName { get; set; }

	string Display3dGroupName { get; set; }

	string DescMaterial { get; }

	string DescMaterialGroup { get; }

	string DescMaterial3dGroup { get; }

	string DescThickness { get; }

	IDoc3d Doc { get; set; }

	void UpdateMaterial(IMaterialArt? material, double? thickness);
}
