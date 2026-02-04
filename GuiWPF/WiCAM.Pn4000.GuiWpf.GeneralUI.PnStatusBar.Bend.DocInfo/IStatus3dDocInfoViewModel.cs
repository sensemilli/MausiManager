using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.DocInfo;

internal interface IStatus3dDocInfoViewModel : IPnStatusViewModel
{
	IDoc3d? Doc { get; set; }

	string ArchiveNumber { get; }

	string ArchiveFile { get; }

	string DescName { get; }

	string NamePp { get; }

	string ModelName { get; }
}
