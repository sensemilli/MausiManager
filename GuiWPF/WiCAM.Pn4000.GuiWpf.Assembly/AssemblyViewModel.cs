using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiWpf.UiBasic;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.Extensions;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;
using WiCAM.Pn4000.PN3D.Unfold;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Assembly;

public class AssemblyViewModel : ViewModelBase, IAssemblyViewModel
{
	private bool _isScreenLoaded;

	private readonly IPN3DDocPipe _pn3DDocPipe;

	private Action<UserControl> _endView;

	private WiCAM.Pn4000.PN3D.Assembly.Assembly _assembly;

	private string _filename;

	private DisassemblyPartViewModel _currentPropertyPart;

	private DisassemblyPartViewModel _currentHighlightedPart;

	private IAssemblyAnalysisManagement _assemblyAnalysisManagement;

	private Thread threadBackgroundCalculation;

	private bool _assemblyLoadingHidden = true;

	private string _assemblyLoadingInfo;

	private IImportArg _importSetting;

	private IMaterialManager _matWicam;

	private readonly IFactorio _factorio;

	private readonly IMessageLogGlobal _logGlobal;

	private readonly IImportMaterialMapper _importMaterialMapper;

	private readonly IConfigProvider _configProvider;

	private readonly IPnPathService _pathService;

	private readonly IMaterial3dFortran _material3dFortran;

	private Action<F2exeReturnCode> _answer;

	private F2exeReturnCode _returnCode = F2exeReturnCode.Undefined;

	private bool _isCancellingByUser;

	private readonly CancellationTokenSource _cancellationImport = new CancellationTokenSource();

	private bool _loadingDoneAlready;

	public IPrefabricatedPartsManager PrefabricatedPartsManager { get; }

	public bool AssemblyLoadingHidden
	{
		get
		{
			return _assemblyLoadingHidden;
		}
		set
		{
			if (_assemblyLoadingHidden != value)
			{
				_assemblyLoadingHidden = value;
				NotifyPropertyChanged("AssemblyLoadingHidden");
			}
		}
	}

	public string AssemblyLoadingInfo
	{
		get
		{
			return _assemblyLoadingInfo;
		}
		private set
		{
			if (_assemblyLoadingInfo != value)
			{
				_assemblyLoadingInfo = value;
				NotifyPropertyChanged("AssemblyLoadingInfo");
			}
		}
	}

	public Screen3D ImageAssembly3D { get; set; }

	public ObservableCollection<DisassemblyPartViewModel> ListParts { get; private set; } = new ObservableCollection<DisassemblyPartViewModel>();

	public ObservableCollection<DisassemblyNodeViewModel> HirarchicParts { get; private set; } = new ObservableCollection<DisassemblyNodeViewModel>();

	public IEnumerable<DisassemblyNodeViewModel> AllHirarchicParts => HirarchicParts.SelectAndManyRecursive((DisassemblyNodeViewModel x) => x.Children);

	public ObservableCollection<DisassemblyNodeViewModel> ListWithPurchasedParts { get; private set; } = new ObservableCollection<DisassemblyNodeViewModel>();

	public DisassemblyPartViewModel CurrentPropertyPart
	{
		get
		{
			return _currentPropertyPart;
		}
		set
		{
			if (_currentPropertyPart != value)
			{
				_currentPropertyPart = value;
				if (_assemblyAnalysisManagement != null)
				{
					_assemblyAnalysisManagement.TopPriority = _currentPropertyPart?.Part;
				}
				NotifyPropertyChanged("CurrentPropertyPart");
				this.OnScrollIntoView?.Invoke(_currentPropertyPart);
			}
		}
	}

	public DisassemblyPartViewModel CurrentHighlightedPart
	{
		get
		{
			return _currentHighlightedPart;
		}
		set
		{
			if (_currentHighlightedPart != value)
			{
				_currentHighlightedPart = value;
				NotifyPropertyChanged("CurrentHighlightedPart");
			}
		}
	}

	public List<IMaterialArt> Materials { get; set; }

	public Dictionary<int, string> PurchasedPartTypes { get; private set; }

	public List<WiCAM.Pn4000.GuiWpf.UiBasic.ComboboxEntry<int>> PuchasedParts { get; } = new List<WiCAM.Pn4000.GuiWpf.UiBasic.ComboboxEntry<int>>();

	private Dictionary<int, DisassemblyPartViewModel> _partsById { get; } = new Dictionary<int, DisassemblyPartViewModel>();

	public ICommand CmdCancel => new RelayCommand(CmdCancelExecute);

	public ICommand CmdOpenSelectedParts => new RelayCommand(CmdOpenSelectedPartsExecute, CmdOpenSelectedPartsCanExecute);

	public List<MaterialEntry> MaterialAsignments { get; set; }

	public ObservableCollection<MaterialWicam> WicamMaterialList { get; set; }

	public ITranslator Translator { get; }

	public event Action AnalyzingStatusChanged;

	public event Action<object> OnScrollIntoView;

	public AssemblyViewModel(IPN3DDocPipe pn3DDocPipe, IMaterialManager materials, IFactorio factorio, IMessageLogGlobal logGlobal, ITranslator translator, IPrefabricatedPartsManager prefabricatedPartsManager, IImportMaterialMapper importMaterialMapper, IConfigProvider configProvider, IPnPathService pathService, IMaterial3dFortran material3dFortran)
	{
		PrefabricatedPartsManager = prefabricatedPartsManager;
		_pn3DDocPipe = pn3DDocPipe;
		_matWicam = materials;
		_factorio = factorio;
		_logGlobal = logGlobal;
		Translator = translator;
		_importMaterialMapper = importMaterialMapper;
		_configProvider = configProvider;
		_pathService = pathService;
		_material3dFortran = material3dFortran;
	}

	public bool Init(Action<UserControl> endView, WiCAM.Pn4000.PN3D.Assembly.Assembly assembly, IImportArg importSetting, Action<F2exeReturnCode> answer)
	{
		_importSetting = importSetting;
		_filename = importSetting.Filename;
		_answer = answer;
		_endView = OnClosing;
		_endView = (Action<UserControl>)Delegate.Combine(_endView, endView);
		_assembly = assembly;
		Materials = _matWicam.MaterialList.ToList();
		GeneratePurchasedPartTranslation();
		SetLoadingStatus("Loading Assembly Structure");
		_returnCode = Step1LoadAssemblyStructure();
		SetLoadingStatus("Loading Low Tesselated Models. Create Screenshots.");
		if (_returnCode == F2exeReturnCode.ERROR_CORRECT_SPATIAL_MISSING)
		{
			_logGlobal.ShowTranslatedErrorMessage("PnBndDoc.CreateByImportSpatial.ErrorVersionMissing", "sp2025.1.0.0.12");
		}
		if (_cancellationImport.IsCancellationRequested && _returnCode == F2exeReturnCode.OK)
		{
			_returnCode = F2exeReturnCode.CANCEL_BY_USER;
		}
		if (_returnCode != 0)
		{
			_endView?.Invoke(null);
			_answer?.Invoke(_returnCode);
			return false;
		}
		Step2LoadLowTesselation();
		if (_cancellationImport.IsCancellationRequested)
		{
			_returnCode = F2exeReturnCode.CANCEL_BY_USER;
			_endView?.Invoke(null);
			_answer?.Invoke(_returnCode);
			return false;
		}
		if (!importSetting.OpenPartId.HasValue)
		{
			SetLoadingMessageAnalyze();
		}
		_assemblyAnalysisManagement.SaveAssembly();
		if (_assembly.LastOpenedPartId.HasValue)
		{
			DisassemblyPartViewModel disassemblyPartViewModel = ListParts.FirstOrDefault((DisassemblyPartViewModel x) => x.Id == _assembly.LastOpenedPartId);
			if (disassemblyPartViewModel != null)
			{
				disassemblyPartViewModel.IsMouseSelected = true;
			}
		}
		WicamMaterialList = new ObservableCollection<MaterialWicam>(_matWicam.MaterialList.Select((IMaterialArt x) => new MaterialWicam(x)));
		WicamMaterialList.Insert(0, new MaterialWicam(null));
		List<(string cadMat, List<DisassemblyPartViewModel> list)> list = (from x in ListParts
			group x by x.Part.OriginalMaterialName into g
			select (cadMat: g.Key, list: g.ToList())).ToList();
		MaterialAsignments = new List<MaterialEntry>();
		IImportMaterialMapper importMaterialMapper = _importMaterialMapper;
		foreach (var item in list)
		{
			if (((item.cadMat == null) ? (-1) : importMaterialMapper.GetMaterialId(item.cadMat)) < 0 && !importSetting.MaterialNumber.HasValue)
			{
				MaterialAsignments.Add(new MaterialEntry
				{
					MatImport = item.cadMat,
					MatWicam = null,
					Parts = item.list
				});
			}
		}
		if (_importSetting.NoPopups)
		{
			if (_cancellationImport.IsCancellationRequested)
			{
				_endView?.Invoke(null);
				_answer?.Invoke(_returnCode);
				return false;
			}
			Step3LoadAndAnalyzeHd();
			if (_cancellationImport.IsCancellationRequested)
			{
				_endView?.Invoke(null);
				_answer?.Invoke(_returnCode);
				return false;
			}
			if (importSetting.OpenPartId.HasValue)
			{
				DisassemblyPartViewModel disassemblyPartViewModel2 = ListParts.FirstOrDefault((DisassemblyPartViewModel x) => x.Id == importSetting.OpenPartId);
				if (disassemblyPartViewModel2 != null)
				{
					Step6SaveAssemblyAndOpenPart(disassemblyPartViewModel2);
				}
			}
		}
		else
		{
			GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
			ImageAssembly3D = new Screen3D();
			ImageAssembly3D.Loaded += ImageAssembly3DOnLoaded;
			ImageAssembly3D.SetBackground(generalUserSettingsConfig.PreviewColor3D1.ToWpfColor(), generalUserSettingsConfig.PreviewColor3D2.ToWpfColor());
			ImageAssembly3D.ShowNavigation(show: true);
			ImageAssembly3D.SetConfigProviderAndApplySettings(_configProvider);
			threadBackgroundCalculation = new Thread(Step3LoadAndAnalyzeHd);
			threadBackgroundCalculation.Start();
		}
		return true;
	}

	private void OnClosing(UserControl obj)
	{
		if (ImageAssembly3D != null)
		{
			ImageAssembly3D.MouseEnterTriangle -= MouseEnterTriangle;
			ImageAssembly3D.TriangleSelected -= TriangleSelected;
			ImageAssembly3D.Loaded -= ImageAssembly3DOnLoaded;
		}
	}

	private void GeneratePurchasedPartTranslation()
	{
		PuchasedParts.Add(new WiCAM.Pn4000.GuiWpf.UiBasic.ComboboxEntry<int>(Translator.Translate("l_enum.PrefabricatedPartsEnum.None"), 0));
		PuchasedParts.AddRange(from x in PrefabricatedPartsManager.GetPartTypesOrdered()
			select new WiCAM.Pn4000.GuiWpf.UiBasic.ComboboxEntry<int>(x.typeDesc, x.typeId));
		PurchasedPartTypes = PuchasedParts.ToDictionary((WiCAM.Pn4000.GuiWpf.UiBasic.ComboboxEntry<int> x) => x.Value, (WiCAM.Pn4000.GuiWpf.UiBasic.ComboboxEntry<int> x) => x.Desc);
	}

	private void ImageAssembly3DOnLoaded(object sender, RoutedEventArgs e)
	{
		if (_isScreenLoaded)
		{
			return;
		}
		ImageAssembly3D.ScreenD3D.RemoveModel(null);
		ImageAssembly3D.ScreenD3D.RemoveBillboard(null);
		foreach (DisassemblyNodeViewModel allHirarchicPart in AllHirarchicParts)
		{
			Model modelLowTesselation = allHirarchicPart.Node.ModelLowTesselation;
			if (modelLowTesselation != null)
			{
				ImageAssembly3D.ScreenD3D.AddModel(modelLowTesselation, render: false);
			}
		}
		ViewModelResourceLoader.LoadArrows(_pathService.PNDRIVE, ImageAssembly3D.ScreenD3D);
		ViewModelResourceLoader.LoadLetters(_pathService.PNDRIVE, ImageAssembly3D.ScreenD3D);
		SetViewDirection();
		ImageAssembly3D.MouseEnterTriangle += MouseEnterTriangle;
		ImageAssembly3D.TriangleSelected += TriangleSelected;
		ImageAssembly3D.SetConfigProviderAndApplySettings(_configProvider);
		_isScreenLoaded = true;
	}

	private void Part_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (!(sender is DisassemblyPartViewModel disassemblyPartViewModel))
		{
			return;
		}
		if (e.PropertyName == "IsMouseSelected")
		{
			if (disassemblyPartViewModel.IsMouseSelected)
			{
				CurrentPropertyPart = disassemblyPartViewModel;
				{
					foreach (DisassemblyPartViewModel listPart in ListParts)
					{
						_ = listPart;
					}
					return;
				}
			}
			if (CurrentPropertyPart == disassemblyPartViewModel)
			{
				CurrentPropertyPart = null;
			}
		}
		else if (e.PropertyName == "IsMouseHovering")
		{
			if (disassemblyPartViewModel.IsMouseHovering)
			{
				CurrentHighlightedPart = disassemblyPartViewModel;
				{
					foreach (DisassemblyPartViewModel listPart2 in ListParts)
					{
						if (listPart2 != disassemblyPartViewModel)
						{
							listPart2.IsMouseHovering = false;
						}
					}
					return;
				}
			}
			if (CurrentHighlightedPart == disassemblyPartViewModel)
			{
				CurrentHighlightedPart = null;
			}
		}
		else if (e.PropertyName == "BackgroundCol")
		{
			ImageAssembly3D?.ScreenD3D?.UpdateAllModelAppearance(render: true);
		}
		else
		{
			if (!(e.PropertyName == "Deleted"))
			{
				return;
			}
			ImageAssembly3D?.ScreenD3D?.UpdateAllModelAppearance(render: true);
			if (disassemblyPartViewModel == CurrentPropertyPart)
			{
				CurrentPropertyPart = null;
			}
			disassemblyPartViewModel.PropertyChanged -= Part_PropertyChanged;
			ListParts.Remove(disassemblyPartViewModel);
			foreach (DisassemblyPartViewModel listPart3 in ListParts)
			{
				listPart3.PurchasedParts.Remove(disassemblyPartViewModel);
			}
		}
	}

	private void MouseEnterTriangle(IScreen3D sender, ITriangleEventArgs e)
	{
		DisassemblyPartViewModel trianglePartVm = GetTrianglePartVm(e);
		if (trianglePartVm != null)
		{
			trianglePartVm.IsMouseHovering = true;
			return;
		}
		foreach (DisassemblyPartViewModel listPart in ListParts)
		{
			listPart.IsMouseHovering = false;
		}
	}

	private void TriangleSelected(IScreen3D sender, ITriangleEventArgs e)
	{
		DisassemblyPartViewModel trianglePartVm = GetTrianglePartVm(e);
		if (trianglePartVm != null)
		{
			trianglePartVm.IsMouseSelected = !trianglePartVm.IsMouseSelected;
			if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
			{
				Step6SaveAssemblyAndOpenPart(trianglePartVm);
			}
			return;
		}
		foreach (DisassemblyPartViewModel listPart in ListParts)
		{
			listPart.IsMouseSelected = false;
		}
	}

	private DisassemblyPartViewModel GetTrianglePartVm(ITriangleEventArgs e)
	{
		Model model = e.Model;
		if (model != null)
		{
			while (model.Parent != null)
			{
				model = model.Parent;
			}
			foreach (DisassemblyPartViewModel listPart in ListParts)
			{
				if (listPart.Part.ModelLowTesselation == model)
				{
					return listPart;
				}
			}
		}
		return null;
	}

	private void SetViewDirection()
	{
		Matrix4d identity = Matrix4d.Identity;
		identity *= Matrix4d.RotationZ(0.7853981852531433);
		identity *= Matrix4d.RotationX(1.0471975803375244);
		ImageAssembly3D.ScreenD3D.SetViewDirectionByMatrix4d(identity, render: false);
		ImageAssembly3D.ScreenD3D.ZoomExtend();
	}

	public void SetLoadingStatus(string message)
	{
		AssemblyLoadingInfo = message;
		AssemblyLoadingHidden = false;
	}

	public bool SetLoadingMessageAnalyze()
	{
		int num = 0;
		int num2 = 0;
		foreach (DisassemblyPartViewModel listPart in ListParts)
		{
			if (listPart.Part.IsLoaded || listPart.Part.Metadata != null)
			{
				num++;
			}
			else
			{
				num2++;
			}
		}
		if (num2 == 0)
		{
			Step5LoadingDone();
			return true;
		}
		SetLoadingStatus($"{num}/{num + num2} Analysiert.");
		return false;
	}

	public F2exeReturnCode Step1LoadAssemblyStructure()
	{
		if (_assembly == null)
		{
			if (_importSetting.LoadLastAssembly)
			{
				_assembly = AssemblyAnalysisManagement.LoadTempAssembly(_pathService);
				if (_assembly != null)
				{
					_assembly.LoadingStatus = WiCAM.Pn4000.PN3D.Assembly.Assembly.EnumLoadingStatus.AssemblyStructureRead;
				}
				else
				{
					_assembly = _pn3DDocPipe.CreateAssemblyAfterSpatial();
				}
			}
			if (_assembly == null)
			{
				WiCAM.Pn4000.PN3D.Assembly.Assembly assembly;
				F2exeReturnCode f2exeReturnCode = _pn3DDocPipe.ImportSpatialAssembly(_filename, _importSetting.CheckLicense, _importSetting.MoveToCenter, useBackground: false, out assembly, !_importSetting.UseHd);
				if (f2exeReturnCode != 0)
				{
					return f2exeReturnCode;
				}
				_assembly = assembly;
			}
			else
			{
				_assembly.LoadingStatus = WiCAM.Pn4000.PN3D.Assembly.Assembly.EnumLoadingStatus.AssemblyStructureRead;
			}
		}
		_assembly.SetDefaultMaterialsCalcPositions(_importSetting, _importMaterialMapper, _material3dFortran.GetActiveMaterial(isAssembly: false)?.Number ?? (-1), _configProvider.InjectOrCreate<General3DConfig>());
		foreach (DisassemblyPartViewModel listPart in ListParts)
		{
			listPart.PropertyChanged -= Part_PropertyChanged;
		}
		ListParts = new ObservableCollectionListSource<DisassemblyPartViewModel>(from part in _assembly.DisassemblyParts
			where !part.Deleted
			select new DisassemblyPartViewModel(part, this, _pathService, _partsById));
		foreach (DisassemblyPartViewModel listPart2 in ListParts)
		{
			IPrefabricatedPart prefabricatedPart = PrefabricatedPartsManager.FindPart(listPart2.Part?.OriginalAssemblyName ?? "", checkDetectionEnabled: true);
			if (prefabricatedPart != null)
			{
				listPart2.PurchasedPartTypeId = prefabricatedPart.Type;
				listPart2.PurchasedPartIgnoreCollision = prefabricatedPart.IgnoreAtCollision;
			}
			listPart2.PropertyChanged += Part_PropertyChanged;
		}
		HirarchicParts.Clear();
		HirarchicParts.Add(new DisassemblyNodeViewModel(_assembly.RootNode));
		Dictionary<DisassemblyPart, DisassemblyPartViewModel> dictionary = ListParts.ToDictionary((DisassemblyPartViewModel x) => x.Part);
		foreach (DisassemblyNodeViewModel allHirarchicPart in AllHirarchicParts)
		{
			DisassemblyPart part2 = allHirarchicPart.Node.Part;
			if (part2 != null && !part2.Deleted)
			{
				DisassemblyPartViewModel disassemblyPartViewModel2 = (allHirarchicPart.PartViewModel = dictionary[allHirarchicPart.Node.Part]);
				disassemblyPartViewModel2.NodesVm.Add(allHirarchicPart);
			}
		}
		foreach (DisassemblyPartViewModel listPart3 in ListParts)
		{
			if (listPart3.Part.Matrixes == null)
			{
				listPart3.Part.Matrixes = new List<Matrix4d>();
			}
		}
		if (_assembly.LoadingStatus < WiCAM.Pn4000.PN3D.Assembly.Assembly.EnumLoadingStatus.AssemblyStructureRead)
		{
			_assembly.LoadingStatus = WiCAM.Pn4000.PN3D.Assembly.Assembly.EnumLoadingStatus.AssemblyStructureRead;
		}
		return F2exeReturnCode.OK;
	}

	public void Step2LoadLowTesselation()
	{
		_assemblyAnalysisManagement = _factorio.Resolve<IAssemblyAnalysisManagement>();
		_assemblyAnalysisManagement.Init(_assembly, _importSetting);
		_assemblyAnalysisManagement.Step1LoadLowTesselation(_cancellationImport.Token);
		if (_cancellationImport.IsCancellationRequested)
		{
			return;
		}
		foreach (DisassemblyPartViewModel listPart in ListParts)
		{
			listPart.LoadImage();
			listPart.RefreshModelVisibility();
		}
		foreach (DisassemblyPart item in _assembly.DisassemblyParts.Where((DisassemblyPart x) => x.Deleted))
		{
			item.ModelLowTesselation.Enabled = false;
		}
	}

	public void Step3LoadAndAnalyzeHd()
	{
		if (_assembly.LoadingStatus >= WiCAM.Pn4000.PN3D.Assembly.Assembly.EnumLoadingStatus.AnalyzedHd)
		{
			foreach (DisassemblyPartViewModel listPart in ListParts)
			{
				listPart.PartHdAnalyzedMainThread(null);
			}
			return;
		}
		foreach (DisassemblyPartViewModel listPart2 in ListParts)
		{
			listPart2.PartAnalyzed += Part_PartAnalyzed;
		}
		_assemblyAnalysisManagement.Step2AnalyzeHd(_cancellationImport.Token);
		if (_importSetting.NoPopups)
		{
			return;
		}
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			if (_cancellationImport.IsCancellationRequested)
			{
				_endView?.Invoke(null);
				_answer?.Invoke(_returnCode);
			}
		});
	}

	private void Part_PartAnalyzed(DisassemblyPartViewModel obj)
	{
		this.AnalyzingStatusChanged?.Invoke();
		if (!SetLoadingMessageAnalyze())
		{
			return;
		}
		foreach (DisassemblyPartViewModel listPart in ListParts)
		{
			listPart.PartAnalyzed -= Part_PartAnalyzed;
		}
		Step4ExportPartsXml();
	}

	public void Step4ExportPartsXml()
	{
		_assemblyAnalysisManagement.Step3ExportData(_filename, from x in ListParts
			orderby x.Part.ID
			select x.Part);
	}

	private void Step5LoadingDone()
	{
		if (!_loadingDoneAlready)
		{
			_loadingDoneAlready = true;
			AssemblyLoadingInfo = "";
			AssemblyLoadingHidden = true;
			DisassemblyPartViewModel autoChoseSinglePart = _assembly.GetAutoChoseSinglePart(_importSetting, ListParts, (DisassemblyPartViewModel x) => x.Part);
			bool flag = false;
			if (autoChoseSinglePart != null)
			{
				flag = Step6SaveAssemblyAndOpenPart(autoChoseSinglePart);
			}
			else if (_importSetting.OpenSingleParts)
			{
				_ = ListParts.Count;
			}
			if (_importSetting.NoPopups && !flag)
			{
				CmdCancelExecute();
			}
		}
	}

	public bool Step6SaveAssemblyAndOpenPart(DisassemblyPartViewModel part)
	{
		WiCAM.Pn4000.PN3D.Assembly.Assembly assembly = _assembly;
		if (assembly != null && assembly.LoadingStatus >= WiCAM.Pn4000.PN3D.Assembly.Assembly.EnumLoadingStatus.AnalyzedHd)
		{
			Step4ExportPartsXml();
			_endView?.Invoke(null);
			F2exeReturnCode returnCode = _assemblyAnalysisManagement.Step4OpenPartAndSaveAssembly(part?.Part);
			if (_returnCode == F2exeReturnCode.OK)
			{
				_returnCode = returnCode;
			}
			_answer?.Invoke(_returnCode);
			return true;
		}
		return false;
	}

	public bool Step6SaveAssemblyAndOpenPart(IEnumerable<DisassemblyPartViewModel> parts)
	{
		WiCAM.Pn4000.PN3D.Assembly.Assembly assembly = _assembly;
		if (assembly != null && assembly.LoadingStatus >= WiCAM.Pn4000.PN3D.Assembly.Assembly.EnumLoadingStatus.AnalyzedHd)
		{
			Step4ExportPartsXml();
			_endView?.Invoke(null);
			F2exeReturnCode returnCode = _assemblyAnalysisManagement.Step4OpenPartAndSaveAssembly(parts.Select((DisassemblyPartViewModel x) => x.Part));
			if (_returnCode == F2exeReturnCode.OK)
			{
				_returnCode = returnCode;
			}
			_answer?.Invoke(_returnCode);
			return true;
		}
		return false;
	}

	public void ApplyMaterialToAll(DisassemblyPartViewModel obj)
	{
		if (obj == null)
		{
			return;
		}
		foreach (DisassemblyPartViewModel listPart in ListParts)
		{
			listPart.MaterialId = obj.MaterialId;
			listPart.PnMaterialByUser = true;
		}
	}

	public void CmdCancelExecute()
	{
		WiCAM.Pn4000.PN3D.Assembly.Assembly assembly = _assembly;
		if (assembly != null && assembly.LoadingStatus >= WiCAM.Pn4000.PN3D.Assembly.Assembly.EnumLoadingStatus.AnalyzedHd)
		{
			Step4ExportPartsXml();
			_assemblyAnalysisManagement.SaveAssembly();
			_endView?.Invoke(null);
			if (_importSetting.NoPopups)
			{
				_answer?.Invoke((_returnCode == F2exeReturnCode.OK) ? F2exeReturnCode.ERROR_TOO_COMPLEX_DATA : _returnCode);
			}
			else
			{
				_answer?.Invoke((_returnCode == F2exeReturnCode.OK) ? F2exeReturnCode.CANCEL_BY_USER : _returnCode);
			}
		}
		else
		{
			WiCAM.Pn4000.PN3D.Assembly.Assembly assembly2 = _assembly;
			if (assembly2 != null && assembly2.LoadingStatus >= WiCAM.Pn4000.PN3D.Assembly.Assembly.EnumLoadingStatus.LowTesselationLoaded)
			{
				_isCancellingByUser = true;
				_cancellationImport.Cancel();
			}
		}
	}

	private bool CmdOpenSelectedPartsCanExecute()
	{
		return OpenPartCanExecute(ListParts.Where((DisassemblyPartViewModel x) => x.IsMouseSelected));
	}

	public void CmdOpenSelectedPartsExecute()
	{
		OpenPartExecute(ListParts.Where((DisassemblyPartViewModel x) => x.IsMouseSelected));
	}

	private bool OpenPartCanExecute(IEnumerable<DisassemblyPartViewModel> parts)
	{
		if (parts.Any())
		{
			return parts.All((DisassemblyPartViewModel x) => x.Part.Metadata != null);
		}
		return false;
	}

	private void OpenPartExecute(IEnumerable<DisassemblyPartViewModel> parts)
	{
		if (OpenPartCanExecute(parts))
		{
			Step6SaveAssemblyAndOpenPart(parts);
		}
	}
}
