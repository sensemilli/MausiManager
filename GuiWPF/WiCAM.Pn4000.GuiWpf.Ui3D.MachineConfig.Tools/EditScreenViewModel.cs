using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;

public class EditScreenViewModel : ViewModelBase
{
	private const double ToolImageHeight = 440.0;

	private const double ToolImageWidth = 256.0;

	private FrameworkElement _previewImage;

	private FrameworkElement _previewImage3D;

	private ToolItemViewModel _selectedItem;

	private ICommand _okCommand;

	private ICommand _applyCommand;

	private ICommand _cancelCommand;

	private ICommand _nextItemCommand;

	private ICommand _previousItemCommand;

	private readonly Action<bool, bool> _closeAction;

	private readonly IDrawToolProfiles _drawToolProfiles;

	private readonly IConfigProvider _configProvider;

	private readonly MachineToolsViewModel _toolConfigModel;

	private readonly bool _isUpper;

	private string _indexOfCount;

	private int _selectedIndex;

	private readonly int _count;

	private Visibility _visible3D;

	private Visibility _visible2D;

	public ICommand OkCommand => _okCommand ?? (_okCommand = new RelayCommand((Action)delegate
	{
		_closeAction?.Invoke(arg1: true, arg2: true);
	}));

	public ICommand ApplyCommand => _applyCommand ?? (_applyCommand = new RelayCommand((Action)delegate
	{
		_closeAction?.Invoke(arg1: true, arg2: false);
	}));

	public ICommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand((Action)delegate
	{
		_closeAction?.Invoke(arg1: false, arg2: true);
	}));

	public ICommand NextItemCommand => _nextItemCommand ?? (_nextItemCommand = new RelayCommand(NextItem));

	public ICommand PreviousItemCommand => _previousItemCommand ?? (_previousItemCommand = new RelayCommand(PreviousItem));

	public FrameworkElement PreviewImage
	{
		get
		{
			return _previewImage;
		}
		set
		{
			_previewImage = value;
			NotifyPropertyChanged("PreviewImage");
		}
	}

	public FrameworkElement PreviewImage3D
	{
		get
		{
			return _previewImage3D;
		}
		set
		{
			_previewImage3D = value;
			NotifyPropertyChanged("PreviewImage3D");
		}
	}

	public ToolItemViewModel SelectedItem
	{
		get
		{
			return _selectedItem;
		}
		set
		{
			_selectedItem = value;
			NotifyPropertyChanged("SelectedItem");
		}
	}

	public ToolItemViewModel[] Items { get; set; }

	public string IndexOfCount
	{
		get
		{
			return _indexOfCount;
		}
		set
		{
			_indexOfCount = value;
			NotifyPropertyChanged("IndexOfCount");
		}
	}

	public Visibility Visible3D
	{
		get
		{
			return _visible3D;
		}
		set
		{
			_visible3D = value;
			NotifyPropertyChanged("Visible3D");
		}
	}

	public Visibility Visible2D
	{
		get
		{
			return _visible2D;
		}
		set
		{
			_visible2D = value;
			NotifyPropertyChanged("Visible2D");
		}
	}

	public EditScreenViewModel(IEnumerable<ToolItemViewModel> items, ToolItemViewModel selectedItem, bool isUpper, MachineToolsViewModel toolConfigModel, IConfigProvider configProvider, Action<bool, bool> closeAction, IDrawToolProfiles drawToolProfiles)
	{
		_isUpper = isUpper;
		_closeAction = closeAction;
		_toolConfigModel = toolConfigModel;
		_configProvider = configProvider;
		SelectedItem = selectedItem.Copy();
		Items = items.ToArray();
		_drawToolProfiles = drawToolProfiles;
		_count = Items.Length;
		_selectedIndex = Items.IndexOf<ToolItemViewModel>(selectedItem);
		IndexOfCount = $"{_selectedIndex + 1}/{_count}";
		LoadData();
	}

	private void LoadData()
	{
		ToolItemViewModel selectedItem = SelectedItem;
		ToolViewModel toolViewModel = selectedItem as ToolViewModel;
		if (toolViewModel != null)
		{
			if (PreviewImage == null)
			{
				PreviewImage = new Canvas
				{
					Height = 440.0,
					Width = 256.0
				};
				PreviewImage.Loaded += delegate
				{
					toolViewModel.Load2DPreview((Canvas)PreviewImage, _drawToolProfiles);
				};
			}
			else
			{
				toolViewModel.Load2DPreview((Canvas)PreviewImage, _drawToolProfiles);
			}
			return;
		}
		selectedItem = SelectedItem;
		ToolPieceViewModel toolPieceViewModel = selectedItem as ToolPieceViewModel;
		if (toolPieceViewModel == null)
		{
			return;
		}
		if (PreviewImage3D == null)
		{
			PreviewImage3D = new Screen3D();
			PreviewImage3D.Loaded += delegate
			{
				toolPieceViewModel.Load3DPreview(PreviewImage3D);
			};
		}
		else
		{
			toolPieceViewModel.Load3DPreview(PreviewImage3D);
		}
	}

	private void NextItem()
	{
		if (_selectedIndex < _count - 1)
		{
			_selectedIndex++;
		}
		else
		{
			_selectedIndex = 0;
		}
		SelectedItem = Items[_selectedIndex].Copy();
		LoadData();
		IndexOfCount = $"{_selectedIndex + 1}/{_count}";
	}

	private void PreviousItem()
	{
		if (_selectedIndex > 0)
		{
			_selectedIndex--;
		}
		else
		{
			_selectedIndex = _count - 1;
		}
		SelectedItem = Items[_selectedIndex].Copy();
		LoadData();
		IndexOfCount = $"{_selectedIndex + 1}/{_count}";
	}

	public void Dispose()
	{
		if (PreviewImage3D?.GetType() == typeof(Screen3D))
		{
			((Screen3D)PreviewImage3D).Dispose();
		}
		PreviewImage3D = null;
		PreviewImage = null;
	}
}
