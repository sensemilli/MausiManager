using System.Windows.Media;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Enums;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.Model;

internal interface IStatus3dModelInfoViewModel : IPnStatusViewModel
{
	string Dimensions { get; }

	string ModelType { get; }

	Brush ModelTypeBackground { get; }

	string ComponentCounts { get; }

	IDoc3d? Doc { get; set; }

	ModelViewMode? CurrentViewMode { get; set; }

	WiCAM.Pn4000.BendModel.Model? CurrentModel { get; }

	void ToggleBillboards();
}
