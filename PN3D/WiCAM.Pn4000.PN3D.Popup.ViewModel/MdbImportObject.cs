namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

public class MdbImportObject : ImportObject
{
	private string _geometrySourceFolder;

	public string GeometrySourceFolder
	{
		get
		{
			return this._geometrySourceFolder;
		}
		set
		{
			this._geometrySourceFolder = value;
			base.NotifyPropertyChanged("GeometrySourceFolder");
		}
	}
}
