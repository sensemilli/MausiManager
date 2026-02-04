using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Data;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.DocManagement;

public interface IDocumentManagerViewModel
{
	Visibility Visibility { get; set; }

	RadObservableCollection<IDocWrapper> Documents { get; }

	IDocWrapper SelectedDoc { get; set; }

	ICommand CmdTileView { get; }
}
