using System.Windows;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.DeveloperTests;

internal class Firma_02
{
	private IDoc3d CurrentDoc;

	public void Execute(IDoc3d CurrentDoc)
	{
		this.CurrentDoc = CurrentDoc;
		if (CurrentDoc == null)
		{
			MessageBox.Show("There is no any loaded model.");
		}
		else if (CurrentDoc.EntryModel3D.Shells.Count < 1 || CurrentDoc.EntryModel3D.Shells[0].Faces.Count == 0)
		{
			MessageBox.Show("There is no faces possible to remove.");
		}
	}
}
