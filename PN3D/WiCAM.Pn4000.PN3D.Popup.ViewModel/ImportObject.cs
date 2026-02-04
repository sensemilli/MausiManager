using System.Collections.ObjectModel;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;

namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

public class ImportObject : ViewModelBase
{
	private SourceType _type;

	private ObservableCollection<string> _dataSourceFiles;

	private string _geometryTargetFolder;

	private string _punchGeometryTargetFolder;

	public SourceType Type
	{
		get
		{
			return this._type;
		}
		set
		{
			this._type = value;
			base.NotifyPropertyChanged("Type");
		}
	}

	public ObservableCollection<string> DataSourceFiles
	{
		get
		{
			return this._dataSourceFiles;
		}
		set
		{
			this._dataSourceFiles = value;
			base.NotifyPropertyChanged("DataSourceFiles");
		}
	}

	public string GeometryTargetFolder
	{
		get
		{
			return this._geometryTargetFolder;
		}
		set
		{
			this._geometryTargetFolder = value;
			base.NotifyPropertyChanged("GeometryTargetFolder");
		}
	}

	public string PunchGeometryTargetFolder
	{
		get
		{
			return this._punchGeometryTargetFolder;
		}
		set
		{
			this._punchGeometryTargetFolder = value;
			base.NotifyPropertyChanged("PunchGeometryTargetFolder");
		}
	}
}
