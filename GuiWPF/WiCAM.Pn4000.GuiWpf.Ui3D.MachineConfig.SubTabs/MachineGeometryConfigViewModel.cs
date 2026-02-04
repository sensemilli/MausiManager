using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using Microsoft.Win32;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Extensions;
using WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.SubTabs;

public class MachineGeometryConfigViewModel : ViewModelBase
{
	private Screen3D _imageMachine;

	private IMessageDisplay _messageDisplay;

	private readonly IMainWindowBlock _windowBlock;

	private readonly IDocSerializer _docSerializer;

	private readonly IFactorio _factorio;

	private readonly IPnBndDocImporter _docImporter;

	private readonly IConfigProvider _configProvider;

	private readonly IPnPathService _pnPathService;

	private BendMachineAssemblyPartUi _selectedAssemblyPart;

	private ObservableCollection<BendMachineAssemblyPartUi> _assembly;

	private ObservableCollection<PartRole> _roles;

	private ICommand _selectPath;

	public BendMachine BendMachine { get; set; }

	public Screen3D ImageMachine
	{
		get
		{
			return _imageMachine;
		}
		set
		{
			_imageMachine = value;
			NotifyPropertyChanged("ImageMachine");
		}
	}

	public BendMachineAssemblyPartUi SelectedAssemblyPart
	{
		get
		{
			return _selectedAssemblyPart;
		}
		set
		{
			_selectedAssemblyPart = value;
			NotifyPropertyChanged("SelectedAssemblyPart");
		}
	}

	public ObservableCollection<BendMachineAssemblyPartUi> Assembly
	{
		get
		{
			return _assembly;
		}
		set
		{
			_assembly = value;
			NotifyPropertyChanged("Assembly");
		}
	}

	public ObservableCollection<PartRole> Roles
	{
		get
		{
			return _roles;
		}
		set
		{
			_roles = value;
			NotifyPropertyChanged("Roles");
		}
	}

	public ICommand SelectPathCommand => _selectPath ?? (_selectPath = new RelayCommand(SelectPath));

	public MachineGeometryConfigViewModel(IMainWindowBlock windowBlock, IDocSerializer docSerializer, IFactorio factorio, IPnBndDocImporter docImporter, IConfigProvider configProvider, IPnPathService pnPathService)
	{
		_windowBlock = windowBlock;
		_docSerializer = docSerializer;
		_factorio = factorio;
		_docImporter = docImporter;
		_configProvider = configProvider;
		_pnPathService = pnPathService;
	}

	public MachineGeometryConfigViewModel Init(BendMachine bendMachine, IMessageLogDoc messageDisplay)
	{
		_messageDisplay = messageDisplay;
		BendMachine = bendMachine;
		Assembly = new ObservableCollection<BendMachineAssemblyPartUi>();
		foreach (Node node in BendMachine.Node.Nodes)
		{
			Assembly.Add(new BendMachineAssemblyPartUi
			{
				GeoFileName3D = node.Geometry,
				PartRole = node.PartRole
			});
		}
		Roles = new ObservableCollection<PartRole>
		{
			PartRole.None,
			PartRole.MainFrame,
			PartRole.Beam,
			PartRole.UTS_Beam,
			PartRole.LTS_Table
		};
		ImageMachine = new Screen3D();
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		ImageMachine.SetBackground(generalUserSettingsConfig.PreviewColor3D1.ToWpfColor(), generalUserSettingsConfig.PreviewColor3D2.ToWpfColor());
		ImageMachine.MouseWheelInverted = generalUserSettingsConfig.P3D_InvertMouseWheel;
		ImageMachine.ShowNavigation(show: false);
		ImageMachine.Loaded += ImageMachineOnLoaded;
		return this;
	}

	private void ImageMachineOnLoaded(object sender, RoutedEventArgs e)
	{
		ViewModelResourceLoader.LoadArrows(_pnPathService.PNDRIVE, ImageMachine.ScreenD3D);
		ViewModelResourceLoader.LoadLetters(_pnPathService.PNDRIVE, ImageMachine.ScreenD3D);
		LoadMachineParts();
	}

	private void LoadMachineParts()
	{
		ImageMachine.ScreenD3D.RemoveModel(null, render: false);
		ImageMachine.ScreenD3D.RemoveBillboard(null, render: false);
		foreach (BendMachineAssemblyPartUi item in Assembly)
		{
			string text = BendMachine.MachinePath + "\\Machine\\" + item.GeoFileName3D;
			if (File.Exists(text))
			{
				Model model = ModelSerializer.Deserialize(text);
				ImageMachine.ScreenD3D.AddModel(model, render: false);
			}
		}
		SetViewDirection();
	}

	private void SetViewDirection()
	{
		Matrix4d identity = Matrix4d.Identity;
		identity *= Matrix4d.RotationZ(2.356194496154785);
		identity *= Matrix4d.RotationX(0.39269909262657166);
		ImageMachine.ScreenD3D.SetViewDirectionByMatrix4d(identity, render: false, delegate
		{
			ImageMachine.ScreenD3D.ZoomExtend();
		});
	}

	public void AddPart()
	{
		BendMachineAssemblyPartUi item = new BendMachineAssemblyPartUi();
		Assembly.Add(item);
		SelectedAssemblyPart = Assembly.Last();
	}

	public void DeletePart()
	{
		if (SelectedAssemblyPart != null)
		{
			Assembly.Remove(SelectedAssemblyPart);
			SelectedAssemblyPart = null;
		}
	}

	private void SelectPath(object param)
	{
		if (param == null)
		{
			return;
		}
		string folderBendMachine = _pnPathService.FolderBendMachine;
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			InitialDirectory = folderBendMachine
		};
		bool? flag = openFileDialog.ShowDialog();
		if (!flag.HasValue || !flag.Value)
		{
			return;
		}
		string text = CheckFileFormatAndConvert(openFileDialog.FileName);
		string fileName = Path.GetFileName(text);
		if (text != BendMachine.MachinePath + "\\Machine\\")
		{
			string path = BendMachine.MachinePath + "\\Machine\\";
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			File.Copy(text, BendMachine.MachinePath + "\\Machine\\" + fileName, overwrite: true);
		}
		SelectedAssemblyPart.GeoFileName3D = fileName;
		NotifyPropertyChanged("SelectedAssemblyPart");
		LoadMachineParts();
	}

	private string CheckFileFormatAndConvert(string file)
	{
		if (string.IsNullOrEmpty(file))
		{
			return file;
		}
		string extension = Path.GetExtension(file);
		string text = extension.ToLower();
		if (!(text == ".c3mo"))
		{
			if (text == ".c3do")
			{
				string? directoryName = Path.GetDirectoryName(file);
				string text2 = Path.GetFileName(file).Replace(extension, "") + ".c3mo";
				string text3 = directoryName + "\\" + text2;
				Model data = _docSerializer.DeserializeGeometry(file);
				ModelSerializer.Serialize(text3, data);
				return text3;
			}
			string? directoryName2 = Path.GetDirectoryName(file);
			string text4 = Path.GetFileName(file).Replace(extension, "") + ".c3mo";
			string text5 = directoryName2 + "\\" + text4;
			Model geometryBySpatial = GetGeometryBySpatial(file);
			ModelSerializer.Serialize(text5, geometryBySpatial);
			return text5;
		}
		return file;
	}

	private Model GetGeometryBySpatial(string file)
	{
		(string, string, int)? formatInfo = Import3DTypes.GetFormatInfo(file);
		if (formatInfo.HasValue)
		{
			bool flag = false;
			if (!_windowBlock.BlockUI_IsBlock())
			{
				flag = true;
				_windowBlock.BlockUI_Block();
			}
			F2exeReturnCode code;
			Model result = _docImporter.CreateByImportSpatial(out code, file, formatInfo.Value.Item3, checkLicense: true, viewStyle: false, _factorio, moveToCenter: false, analyze: false)?.EntryModel3D;
			if (flag)
			{
				_windowBlock.BlockUI_Unblock();
			}
			if (code == F2exeReturnCode.OK)
			{
				return result;
			}
			_messageDisplay.ShowErrorMessage(code.ToString());
			return null;
		}
		_messageDisplay.ShowErrorMessage("Unsupported format!");
		return null;
	}

	public void Dispose()
	{
		if (ImageMachine.GetType() == typeof(Screen3D))
		{
			ImageMachine.Dispose();
		}
		ImageMachine = null;
	}
}
