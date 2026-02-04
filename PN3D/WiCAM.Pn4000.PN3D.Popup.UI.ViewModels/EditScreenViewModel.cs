using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class EditScreenViewModel : ViewModelBase
{
	private FrameworkElement _previewImage;

	private FrameworkElement _previewImage3D;

	private ToolItemViewModelBase _selectedItem;

	private ICommand _okCommand;

	private ICommand _applyCommand;

	private ICommand _cancelCommand;

	private ICommand _nextItemCommand;

	private ICommand _previousItemCommand;

	private readonly Action<bool, bool> _closeAction;

	private const double ToolImageHeight = 440.0;

	private const double ToolImageWidth = 256.0;

	private string _geometryFile;

	private readonly BendMachine _bendMachine;

	private readonly IDrawToolProfiles _drawToolProfiles;

	private readonly IConfigProvider _configProvider;

	private readonly ToolConfigModel _toolConfigModel;

	private readonly Type _itemType;

	private readonly bool _isUpper;

	private string _indexOfCount;

	private int _selectedIndex;

	private readonly int _count;

	private Visibility _visible3D;

	private Visibility _visible2D;

	public ICommand OkCommand => this._okCommand ?? (this._okCommand = new RelayCommand((Action)delegate
	{
		this._closeAction?.Invoke(arg1: true, arg2: true);
	}));

	public ICommand ApplyCommand => this._applyCommand ?? (this._applyCommand = new RelayCommand((Action)delegate
	{
		this._closeAction?.Invoke(arg1: true, arg2: false);
	}));

	public ICommand CancelCommand => this._cancelCommand ?? (this._cancelCommand = new RelayCommand((Action)delegate
	{
		this._closeAction?.Invoke(arg1: false, arg2: true);
	}));

	public ICommand NextItemCommand => this._nextItemCommand ?? (this._nextItemCommand = new RelayCommand(NextItem));

	public ICommand PreviousItemCommand => this._previousItemCommand ?? (this._previousItemCommand = new RelayCommand(PreviousItem));

	public FrameworkElement PreviewImage
	{
		get
		{
			return this._previewImage;
		}
		set
		{
			this._previewImage = value;
			base.NotifyPropertyChanged("PreviewImage");
		}
	}

	public FrameworkElement PreviewImage3D
	{
		get
		{
			return this._previewImage3D;
		}
		set
		{
			this._previewImage3D = value;
			base.NotifyPropertyChanged("PreviewImage3D");
		}
	}

	public ToolItemViewModelBase SelectedItem
	{
		get
		{
			return this._selectedItem;
		}
		set
		{
			this._selectedItem = value;
			base.NotifyPropertyChanged("SelectedItem");
		}
	}

	public ToolItemViewModelBase[] Items { get; set; }

	public string IndexOfCount
	{
		get
		{
			return this._indexOfCount;
		}
		set
		{
			this._indexOfCount = value;
			base.NotifyPropertyChanged("IndexOfCount");
		}
	}

	public Visibility Visible3D
	{
		get
		{
			return this._visible3D;
		}
		set
		{
			this._visible3D = value;
			base.NotifyPropertyChanged("Visible3D");
		}
	}

	public Visibility Visible2D
	{
		get
		{
			return this._visible2D;
		}
		set
		{
			this._visible2D = value;
			base.NotifyPropertyChanged("Visible2D");
		}
	}

	public EditScreenViewModel(IEnumerable<ToolItemViewModelBase> items, ToolItemViewModelBase selectedItem, bool isUpper, ToolConfigModel toolConfigModel, BendMachine bendMachine, IConfigProvider configProvider, Action<bool, bool> closeAction)
	{
		this._isUpper = isUpper;
		this._closeAction = closeAction;
		this._bendMachine = bendMachine;
		this._toolConfigModel = toolConfigModel;
		this._configProvider = configProvider;
		this.SelectedItem = selectedItem.Copy();
		this._itemType = this.SelectedItem.GetType();
		this.Items = items.ToArray();
		this._count = this.Items.Length;
		this._selectedIndex = this.Items.IndexOf(selectedItem);
		this.IndexOfCount = $"{this._selectedIndex + 1}/{this._count}";
		this.LoadData();
	}

	private void LoadData()
	{
		string text = (EditScreenViewModel.GetPropValue(this.SelectedItem, "GeometryFile") as GeometryFileDataViewModel)?.Name;
		this._geometryFile = this._bendMachine.MachinePath;
		if (string.IsNullOrEmpty(text))
		{
			if (this._itemType == typeof(DiePartViewModel))
			{
				DiePartViewModel part = this.SelectedItem as DiePartViewModel;
				this._geometryFile = string.Concat(str2: this._toolConfigModel.DieProfiles.First((DieProfileViewModel p) => p.ID == part.DieProfileID).GeometryFile.Name, str0: this._geometryFile, str1: this._bendMachine.LowerToolsGeometry);
			}
			else if (this._itemType == typeof(HemPartViewModel))
			{
				HemPartViewModel part2 = this.SelectedItem as HemPartViewModel;
				this._geometryFile = string.Concat(str2: (this._isUpper ? this._toolConfigModel.FrontHemProfiles.First((HemProfileViewModel p) => p.ID == part2.ProfileID) : this._toolConfigModel.BackHemProfiles.First((HemProfileViewModel p) => p.ID == part2.ProfileID)).GeometryFile.Name, str0: this._geometryFile, str1: this._bendMachine.LowerToolsGeometry);
			}
			else if (this._itemType == typeof(PunchPartViewModel))
			{
				PunchPartViewModel part3 = this.SelectedItem as PunchPartViewModel;
				this._geometryFile = string.Concat(str2: this._toolConfigModel.PunchProfiles.First((PunchProfileViewModel p) => p.ID == part3.PunchProfileID).GeometryFile.Name, str0: this._geometryFile, str1: this._bendMachine.UpperToolsGeometry);
			}
			else if (this._itemType == typeof(AdapterPartViewModel))
			{
				AdapterPartViewModel part4 = this.SelectedItem as AdapterPartViewModel;
				this._geometryFile = string.Concat(str2: (this._isUpper ? this._toolConfigModel.UpperAdapterProfiles.First((AdapterProfileViewModel p) => p.ID == part4.AdapterProfileID) : this._toolConfigModel.LowerAdapterProfiles.First((AdapterProfileViewModel p) => p.ID == part4.AdapterProfileID)).GeometryFile.Name, str0: this._geometryFile, str1: this._bendMachine.AdapterGeometry);
			}
			else if (this._itemType == typeof(HolderPartViewModel))
			{
				HolderPartViewModel part5 = this.SelectedItem as HolderPartViewModel;
				this._geometryFile = string.Concat(str2: (this._isUpper ? this._toolConfigModel.UpperHolderProfiles.First((HolderProfileViewModel p) => p.ID == part5.HolderProfileID) : this._toolConfigModel.LowerHolderProfiles.First((HolderProfileViewModel p) => p.ID == part5.HolderProfileID)).GeometryFile.Name, str0: this._geometryFile, str1: this._bendMachine.HolderGeometry);
			}
			else if (this._itemType == typeof(SensorDiskViewModel))
			{
				this._geometryFile = this._geometryFile + this._bendMachine.UpperToolsGeometry + text;
			}
		}
		else if (this._itemType == typeof(DiePartViewModel))
		{
			this._geometryFile = this._geometryFile + this._bendMachine.LowerToolsGeometry + text;
		}
		else if (this._itemType == typeof(HemPartViewModel))
		{
			this._geometryFile = this._geometryFile + this._bendMachine.LowerToolsGeometry + text;
		}
		else if (this._itemType == typeof(PunchPartViewModel))
		{
			this._geometryFile = this._geometryFile + this._bendMachine.UpperToolsGeometry + text;
		}
		else if (this._itemType == typeof(AdapterPartViewModel))
		{
			this._geometryFile = this._geometryFile + this._bendMachine.AdapterGeometry + text;
		}
		else if (this._itemType == typeof(HolderPartViewModel))
		{
			this._geometryFile = this._geometryFile + this._bendMachine.HolderGeometry + text;
		}
		else if (this._itemType == typeof(SensorDiskViewModel))
		{
			this._geometryFile = this._geometryFile + this._bendMachine.UpperToolsGeometry + text;
		}
		if (string.IsNullOrEmpty(this._geometryFile))
		{
			return;
		}
		if (this._geometryFile.EndsWith(".c3mo") || this._itemType == typeof(DiePartViewModel) || this._itemType == typeof(HemPartViewModel) || this._itemType == typeof(PunchPartViewModel) || this._itemType == typeof(AdapterPartViewModel) || this._itemType == typeof(HolderPartViewModel) || this._itemType == typeof(SensorDiskViewModel))
		{
			this.Visible3D = Visibility.Visible;
			this.Visible2D = Visibility.Collapsed;
			if (this.PreviewImage3D == null)
			{
				this.PreviewImage3D = new Screen3D();
				this.PreviewImage3D.Loaded += Image3DOnLoaded;
			}
			else
			{
				this.Load3DImage();
			}
			return;
		}
		this.Visible3D = Visibility.Collapsed;
		this.Visible2D = Visibility.Visible;
		if (this.PreviewImage == null)
		{
			this.PreviewImage = new Canvas
			{
				Height = 440.0,
				Width = 256.0
			};
			this.PreviewImage.Loaded += Image2DOnLoaded;
		}
		else
		{
			this.Load2DGeomerty();
		}
	}

	private static object GetPropValue(object src, string propName)
	{
		return src.GetType().GetProperty(propName)?.GetValue(src, null);
	}

	private void Image3DOnLoaded(object sender, RoutedEventArgs e)
	{
		this.Load3DImage();
	}

	private void Load3DImage()
	{
		global::WiCAM.Pn4000.BendModel.Model model = null;
		double num = 0.0;
		double num2 = 0.0;
		if (this._itemType == typeof(DiePartViewModel))
		{
			DiePartViewModel part = this.SelectedItem as DiePartViewModel;
			DieProfileViewModel dieProfileViewModel = this._toolConfigModel.DieProfiles.First((DieProfileViewModel p) => p.ID == part.DieProfileID);
			num = part.Length;
			num2 = dieProfileViewModel.WorkingHeight;
		}
		else if (this._itemType == typeof(HemPartViewModel))
		{
			HemPartViewModel part2 = this.SelectedItem as HemPartViewModel;
			HemProfileViewModel obj = (this._isUpper ? this._toolConfigModel.FrontHemProfiles.First((HemProfileViewModel p) => p.ID == part2.ProfileID) : this._toolConfigModel.BackHemProfiles.First((HemProfileViewModel p) => p.ID == part2.ProfileID));
			num = part2.Length;
			num2 = obj.WorkingHeight;
		}
		else if (this._itemType == typeof(PunchPartViewModel))
		{
			PunchPartViewModel part3 = this.SelectedItem as PunchPartViewModel;
			PunchProfileViewModel punchProfileViewModel = this._toolConfigModel.PunchProfiles.First((PunchProfileViewModel p) => p.ID == part3.PunchProfileID);
			num = part3.Length;
			num2 = punchProfileViewModel.WorkingHeight;
		}
		else if (this._itemType == typeof(AdapterPartViewModel))
		{
			AdapterPartViewModel part4 = this.SelectedItem as AdapterPartViewModel;
			AdapterProfileViewModel obj2 = (this._isUpper ? this._toolConfigModel.UpperAdapterProfiles.First((AdapterProfileViewModel p) => p.ID == part4.AdapterProfileID) : this._toolConfigModel.LowerAdapterProfiles.First((AdapterProfileViewModel p) => p.ID == part4.AdapterProfileID));
			num = part4.Length;
			num2 = obj2.WorkingHeight;
		}
		else if (this._itemType == typeof(HolderPartViewModel))
		{
			HolderPartViewModel part5 = this.SelectedItem as HolderPartViewModel;
			HolderProfileViewModel obj3 = (this._isUpper ? this._toolConfigModel.UpperHolderProfiles.First((HolderProfileViewModel p) => p.ID == part5.HolderProfileID) : this._toolConfigModel.LowerHolderProfiles.First((HolderProfileViewModel p) => p.ID == part5.HolderProfileID));
			num = part5.Length;
			num2 = obj3.WorkingHeight;
		}
		else if (this._itemType == typeof(SensorDiskViewModel))
		{
			num = (this.SelectedItem as SensorDiskViewModel).DiskThickness;
		}
		if (this._geometryFile.EndsWith(".c3mo"))
		{
			model = ModelSerializer.Deserialize(this._geometryFile);
			if (model != null)
			{
				model.Transform = Matrix4d.Identity;
				model.Transform *= Matrix4d.RotationX(-Math.PI / 2.0);
				model.Transform *= Matrix4d.RotationZ(-Math.PI / 2.0);
				model.Transform *= Matrix4d.Translation((0.0 - num) / 2.0, 0.0, num2 / 2.0);
			}
		}
		else
		{
			model = CadGeoLoader.LoadCadGeo3D(this._geometryFile, num, 200.0, toolEdges: true, null, null);
			if (model != null)
			{
				model.Transform = Matrix4d.Identity;
				model.Transform *= Matrix4d.RotationZ(-Math.PI / 4.0);
				model.Transform *= Matrix4d.Translation(0.0, 0.0, num2 / 2.0);
			}
		}
		if (this.PreviewImage3D is Screen3D screen3D)
		{
			screen3D.ShowNavigation(show: false);
			screen3D.SetConfigProviderAndApplySettings(this._configProvider);
			screen3D.ScreenD3D?.RemoveModel(null);
			screen3D.ScreenD3D?.RemoveBillboard(null);
		}
		if (model != null)
		{
			((Screen3D)this.PreviewImage3D).ScreenD3D?.AddModel(model, render: false);
			Matrix4d identity = Matrix4d.Identity;
			identity *= Matrix4d.RotationZ(1.5707963705062866);
			identity *= Matrix4d.RotationX(0.39269909262657166);
			((Screen3D)this.PreviewImage3D).ScreenD3D?.SetViewDirectionByMatrix4d(identity, render: false, delegate
			{
				((Screen3D)this.PreviewImage3D).ScreenD3D.ZoomExtend();
			});
		}
		base.NotifyPropertyChanged("PreviewImage3D");
	}

	private void Image2DOnLoaded(object sender, RoutedEventArgs e)
	{
		this.Load2DGeomerty();
	}

	private void Load2DGeomerty()
	{
		if (this._itemType == typeof(DieProfileViewModel))
		{
			this._drawToolProfiles.LoadDiePreview2D((this.SelectedItem as DieProfileViewModel).DieProfile, (Canvas)this.PreviewImage, this._bendMachine);
		}
		else if (this._itemType == typeof(HemProfileViewModel))
		{
			this._drawToolProfiles.LoadDiePreview2D((this.SelectedItem as HemProfileViewModel).HemProfile, (Canvas)this.PreviewImage, this._bendMachine);
		}
		else if (this._itemType == typeof(PunchProfileViewModel))
		{
			this._drawToolProfiles.LoadPunchPreview2D((this.SelectedItem as PunchProfileViewModel).PunchProfile, (Canvas)this.PreviewImage, this._bendMachine);
		}
		else if (this._itemType == typeof(AdapterProfileViewModel))
		{
			this._drawToolProfiles.LoadAdapterPreview2D((this.SelectedItem as AdapterProfileViewModel).AdapterProfile, (Canvas)this.PreviewImage, this._bendMachine);
		}
		else if (this._itemType == typeof(HolderProfileViewModel))
		{
			this._drawToolProfiles.LoadHolderPreview2D((this.SelectedItem as HolderProfileViewModel).HolderProfile, (Canvas)this.PreviewImage, this._bendMachine);
		}
		else if (this._itemType == typeof(PunchPartViewModel))
		{
			PunchPartViewModel part = this.SelectedItem as PunchPartViewModel;
			PunchProfileViewModel punchProfileViewModel = this._toolConfigModel.PunchProfiles.First((PunchProfileViewModel p) => p.ID == part.PunchProfileID);
			this._drawToolProfiles.LoadPunchPreview2D(punchProfileViewModel.PunchProfile, (Canvas)this.PreviewImage, this._bendMachine);
		}
		else if (this._itemType == typeof(DiePartViewModel))
		{
			DiePartViewModel part2 = this.SelectedItem as DiePartViewModel;
			DieProfileViewModel dieProfileViewModel = this._toolConfigModel.DieProfiles.First((DieProfileViewModel p) => p.ID == part2.DieProfileID);
			this._drawToolProfiles.LoadDiePreview2D(dieProfileViewModel.DieProfile, (Canvas)this.PreviewImage, this._bendMachine);
		}
		else if (this._itemType == typeof(AdapterPartViewModel))
		{
			AdapterPartViewModel part3 = this.SelectedItem as AdapterPartViewModel;
			AdapterProfileViewModel obj = (this._isUpper ? this._toolConfigModel.UpperAdapterProfiles.First((AdapterProfileViewModel p) => p.ID == part3.AdapterProfileID) : this._toolConfigModel.LowerAdapterProfiles.First((AdapterProfileViewModel p) => p.ID == part3.AdapterProfileID));
			this._drawToolProfiles.LoadAdapterPreview2D(obj.AdapterProfile, (Canvas)this.PreviewImage, this._bendMachine);
		}
		else if (this._itemType == typeof(HolderPartViewModel))
		{
			HolderPartViewModel part4 = this.SelectedItem as HolderPartViewModel;
			HolderProfileViewModel obj2 = (this._isUpper ? this._toolConfigModel.UpperHolderProfiles.First((HolderProfileViewModel p) => p.ID == part4.HolderProfileID) : this._toolConfigModel.LowerHolderProfiles.First((HolderProfileViewModel p) => p.ID == part4.HolderProfileID));
			this._drawToolProfiles.LoadHolderPreview2D(obj2.HolderProfile, (Canvas)this.PreviewImage, this._bendMachine);
		}
		else
		{
			_ = this._itemType == typeof(SensorDiskViewModel);
		}
		base.NotifyPropertyChanged("PreviewImage");
	}

	private void NextItem()
	{
		if (this._selectedIndex < this._count - 1)
		{
			this._selectedIndex++;
		}
		else
		{
			this._selectedIndex = 0;
		}
		this.SelectedItem = this.Items[this._selectedIndex].Copy();
		this.LoadData();
		this.IndexOfCount = $"{this._selectedIndex + 1}/{this._count}";
	}

	private void PreviousItem()
	{
		if (this._selectedIndex > 0)
		{
			this._selectedIndex--;
		}
		else
		{
			this._selectedIndex = this._count - 1;
		}
		this.SelectedItem = this.Items[this._selectedIndex].Copy();
		this.LoadData();
		this.IndexOfCount = $"{this._selectedIndex + 1}/{this._count}";
	}

	public void Dispose()
	{
		if (this.PreviewImage3D?.GetType() == typeof(Screen3D))
		{
			((Screen3D)this.PreviewImage3D).Dispose();
		}
		this.PreviewImage3D = null;
		this.PreviewImage = null;
	}
}
