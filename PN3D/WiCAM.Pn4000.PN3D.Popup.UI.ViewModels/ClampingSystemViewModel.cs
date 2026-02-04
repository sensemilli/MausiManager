using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DrawGeometry;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.ClampingSystem;
using Microsoft.Win32;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.PN3D.Tool.Interfaces;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class ClampingSystemViewModel : ToolViewModelBase
{
	private int _selectedIndexProfiles;

	private int _selectedIndexParts;

	private FrameworkElement _imageProfile;

	private FrameworkElement _imagePart;

	private CollectionView _clampingSystemView;

	private ICommand _selectPath;

	private ICommand _selectClick;

	private Type _lastSelectedType;

	private Brush _profileBorderBrush;

	private Brush _partsBorderBrush;

	private new IGlobals _globals;

	private readonly IPnPathService _pnPathService;

	private readonly IConfigProvider _configProvider;

	private ChangedConfigType _changed;

	public Action<ChangedConfigType> DataChanged;

	public int SelectedIndexProfiles
	{
		get
		{
			return this._selectedIndexProfiles;
		}
		set
		{
			this._selectedIndexProfiles = value;
			base.NotifyPropertyChanged("SelectedIndexProfiles");
			if (base.BendMachine.ClampingSystem.ClampingSystemProfiles.Count > 0)
			{
				this.SelectedProfile = ((this._selectedIndexProfiles >= 0) ? base.BendMachine.ClampingSystem.ClampingSystemProfiles[this._selectedIndexProfiles] : null);
				this.ProfileSelectionChanged();
			}
		}
	}

	public int SelectedIndexParts
	{
		get
		{
			return this._selectedIndexParts;
		}
		set
		{
			this._selectedIndexParts = value;
			base.NotifyPropertyChanged("SelectedIndexParts");
			this.SelectedPart = ((this._selectedIndexParts >= 0) ? ((ClampingSystemPart)this._clampingSystemView.GetItemAt(this._selectedIndexParts)) : null);
			this.PartsSelectionChanged();
		}
	}

	public new FrameworkElement ImageProfile
	{
		get
		{
			return this._imageProfile;
		}
		set
		{
			this._imageProfile = value;
			base.NotifyPropertyChanged("ImageProfile");
		}
	}

	public new FrameworkElement ImagePart
	{
		get
		{
			return this._imagePart;
		}
		set
		{
			this._imagePart = value;
			base.NotifyPropertyChanged("ImagePart");
		}
	}

	public ClampingSystemProfile SelectedProfile { get; set; }

	public ClampingSystemPart SelectedPart { get; set; }

	public ICommand SelectClick => this._selectClick ?? (this._selectClick = new RelayCommand(SelectType));

	public ICommand SelectPathCommand => this._selectPath ?? (this._selectPath = new RelayCommand(SelectPath_Click));

	public Type LastSelectedType
	{
		get
		{
			return this._lastSelectedType;
		}
		set
		{
			this._lastSelectedType = value;
			base.NotifyPropertyChanged("LastSelectedType");
			if (this._lastSelectedType == typeof(ClampingSystemPart))
			{
				this.ProfileBorderBrush = new SolidColorBrush(Colors.Transparent);
				this.PartsBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				base.LastSelectedObject = this.SelectedPart;
			}
			else if (this._lastSelectedType == typeof(ClampingSystemProfile))
			{
				this.ProfileBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				this.PartsBorderBrush = new SolidColorBrush(Colors.Transparent);
				base.LastSelectedObject = this.SelectedProfile;
			}
		}
	}

	public Brush ProfileBorderBrush
	{
		get
		{
			return this._profileBorderBrush;
		}
		set
		{
			this._profileBorderBrush = value;
			base.NotifyPropertyChanged("ProfileBorderBrush");
		}
	}

	public Brush PartsBorderBrush
	{
		get
		{
			return this._partsBorderBrush;
		}
		set
		{
			this._partsBorderBrush = value;
			base.NotifyPropertyChanged("PartsBorderBrush");
		}
	}

	public ClampingSystemViewModel(IGlobals globals, IMainWindowDataProvider mainWindowDataProvider, IPnPathService pnPathService, IConfigProvider configProvider, IModelFactory modelFactory)
		: base(globals, mainWindowDataProvider, pnPathService, modelFactory)
	{
		this._globals = globals;
		this._pnPathService = pnPathService;
		this._configProvider = configProvider;
	}

	public void Init(BendMachine bendMachine, ToolConfigModel toolConfigModel, IToolExpert tools = null)
	{
		base.BendMachine = bendMachine;
		base.ToolConfigModel = toolConfigModel;
		base.Tools = tools;
		this.ImageProfile = new Canvas();
		this._clampingSystemView = (CollectionView)CollectionViewSource.GetDefaultView(base.BendMachine.ClampingSystem.ClampingSystemParts);
		this._clampingSystemView.Filter = ClampingSystemFilter;
		this._clampingSystemView.Refresh();
		if (base.BendMachine.ClampingSystem.ClampingSystemProfiles.Count > 0)
		{
			this.SelectedIndexProfiles = 0;
		}
		else
		{
			this.SelectedIndexProfiles = -1;
		}
		base.SelectedType = "ClampingSystems";
	}

	public void DeleteButtonClick()
	{
		if (this.LastSelectedType == typeof(ClampingSystemPart))
		{
			this.Delete_Click(this.SelectedPart);
		}
		else if (this.LastSelectedType == typeof(ClampingSystemProfile))
		{
			this.Delete_Click(this.SelectedProfile);
		}
	}

	public void AddbuttonClick()
	{
		if (this.LastSelectedType == typeof(ClampingSystemPart))
		{
			this.AddPart_Click();
		}
		else if (this.LastSelectedType == typeof(ClampingSystemProfile))
		{
			this.AddProfile_Click();
		}
	}

	private void ProfileSelectionChanged()
	{
		if (this.SelectedIndexProfiles >= 0 && this.SelectedProfile != null && !string.IsNullOrEmpty(this.SelectedProfile.GeometryFile))
		{
			this.SelectType("Profiles");
			string machine3DFile = this._pnPathService.GetMachine3DFile(this.SelectedProfile.GeometryFile);
			CadGeoInfo cadGeoInfo = new CadGeoHelper().ReadCadgeo(machine3DFile);
			((Canvas)this.ImageProfile).Children.Clear();
			((Canvas)this.ImageProfile).Children.Add(DrawGeo.DrawCadGeo(cadGeoInfo, (int)((Canvas)this.ImageProfile).Width, (int)((Canvas)this.ImageProfile).Height, Colors.Red));
			base.NotifyPropertyChanged("ImageProfile");
			this._clampingSystemView.Filter = ClampingSystemFilter;
			this._clampingSystemView.Refresh();
		}
	}

	private void PartsSelectionChanged()
	{
		if (this.SelectedIndexParts >= 0 && this.SelectedPart != null && !string.IsNullOrEmpty(this.SelectedProfile.GeometryFile))
		{
			this.SelectType("Parts");
		}
	}

	private bool ClampingSystemFilter(object item)
	{
		if (this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.ID.ToString()))
		{
			return false;
		}
		return ((ClampingSystemPart)item).ClampingSystemProfileID.ToString().IndexOf(this.SelectedProfile.ID.ToString(), StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public void AddProfile_Click()
	{
		int id = ((base.BendMachine.ClampingSystem.ClampingSystemProfiles.Count > 0) ? (base.BendMachine.ClampingSystem.ClampingSystemProfiles.Max((ClampingSystemProfile p) => p.ID) + 1) : 0);
		base.BendMachine.ClampingSystem.ClampingSystemProfiles.Add(new ClampingSystemProfile(id));
		this.SelectedIndexProfiles = base.BendMachine.ClampingSystem.ClampingSystemProfiles.Count - 1;
		this._changed = ChangedConfigType.ClampingSystem;
	}

	public void AddPart_Click()
	{
		if (this.SelectedProfile != null)
		{
			int id = ((base.BendMachine.ClampingSystem.ClampingSystemParts.Count > 0) ? (base.BendMachine.ClampingSystem.ClampingSystemParts.Max((ClampingSystemPart p) => p.ID) + 1) : 0);
			base.BendMachine.ClampingSystem.ClampingSystemParts.Add(new ClampingSystemPart(id, this.SelectedProfile.ID));
			this.SelectedIndexParts = this._clampingSystemView.Count - 1;
			this._changed = ChangedConfigType.ClampingSystem;
		}
	}

	public void Delete_Click(object param)
	{
		if (param == null)
		{
			return;
		}
		if (param.GetType().ToString().Split('.')
			.Last() == "ClampingSystemProfile")
		{
			int num = base.BendMachine.ClampingSystem.ClampingSystemProfiles.IndexOf((ClampingSystemProfile)param) - 1;
			base.BendMachine.ClampingSystem.ClampingSystemProfiles.Remove((ClampingSystemProfile)param);
			for (int i = 0; i < base.BendMachine.ClampingSystem.ClampingSystemParts.Count; i++)
			{
				ClampingSystemPart clampingSystemPart = base.BendMachine.ClampingSystem.ClampingSystemParts[i];
				if (clampingSystemPart.ClampingSystemProfileID == ((ClampingSystemProfile)param).ID)
				{
					base.BendMachine.ClampingSystem.ClampingSystemParts.Remove(clampingSystemPart);
				}
			}
			if (num < 0 && base.BendMachine.ClampingSystem.ClampingSystemProfiles.Count > 0)
			{
				num = 0;
			}
			this.SelectedIndexProfiles = num;
		}
		else if (param.GetType().ToString().Split('.')
			.Last() == "ClampingSystemPart")
		{
			int num2 = this._clampingSystemView.IndexOf((ClampingSystemPart)param) - 1;
			base.BendMachine.ClampingSystem.ClampingSystemParts.Remove((ClampingSystemPart)param);
			if (num2 < 0 && this._clampingSystemView.Count > 0)
			{
				num2 = 0;
			}
			this.SelectedIndexParts = num2;
		}
		this._changed = ChangedConfigType.ClampingSystem;
	}

	private void SelectPath_Click(object param)
	{
		if (param != null)
		{
			string text = base.BendMachine.MachinePath + base.BendMachine.ClampingSystemGeometry;
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				InitialDirectory = text
			};
			if (openFileDialog.ShowDialog().Value)
			{
				if (Path.GetDirectoryName(openFileDialog.FileName) + "\\" != text)
				{
					File.Copy(openFileDialog.FileName, text + openFileDialog.SafeFileName, overwrite: true);
				}
				((dynamic)param).GeometryFile = openFileDialog.SafeFileName;
			}
		}
		this.SelectedIndexProfiles = this.SelectedIndexProfiles;
	}

	private void SelectType(object param)
	{
		if ((string)param == "Profiles")
		{
			this.LastSelectedType = typeof(ClampingSystemProfile);
		}
		else if ((string)param == "Parts")
		{
			this.LastSelectedType = typeof(ClampingSystemPart);
		}
	}

	public void Save()
	{
		this.DataChanged?.Invoke(this._changed);
	}

	public void Dispose()
	{
		if (this.ImageProfile?.GetType() == typeof(Screen3D))
		{
			((Screen3D)this.ImageProfile).Dispose();
		}
		this.ImageProfile = null;
		if (this.ImagePart?.GetType() == typeof(Screen3D))
		{
			((Screen3D)this.ImagePart).Dispose();
		}
		this.ImagePart = null;
	}
}
