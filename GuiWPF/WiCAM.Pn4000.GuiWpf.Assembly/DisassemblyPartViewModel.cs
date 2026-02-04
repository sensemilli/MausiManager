using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Telerik.Windows.Data;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Assembly;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiWpf.UiBasic;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.Assembly.PurchasedParts;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.Assembly;

public class DisassemblyPartViewModel : ViewModelBase
{
	private static Brush BrushSelected = Brushes.Yellow;

	private static Brush BrushHover = Brushes.Lime;

	private static Brush BrushDefault = Brushes.White;

	private static WiCAM.Pn4000.BendModel.Base.Color ColorSelected = new WiCAM.Pn4000.BendModel.Base.Color(1f, 1f, 0f, 1f);

	private static WiCAM.Pn4000.BendModel.Base.Color ColorHovered = new WiCAM.Pn4000.BendModel.Base.Color(0.5f, 1f, 0.5f, 1f);

	private static WiCAM.Pn4000.BendModel.Base.Color? ColorDefault = null;

	private int _id;

	private string _nameGeometry;

	private Brush _backgroundCol = BrushDefault;

	private readonly AssemblyViewModel _assemblyViewModel;

	private readonly IPnPathService _pathService;

	private bool _isMouseHovering;

	private bool _isMouseSelected;

	private bool? _visible = true;

	private string _nameAssembly;

	private int _purchasedPartId;

	private bool _purchasedPartIgnoreCollision;

	public int Id
	{
		get
		{
			return _id;
		}
		set
		{
			if (_id != value)
			{
				_id = value;
				NotifyPropertyChanged("Id");
			}
		}
	}

	public string NameDisplayWithCount => PartCount + " x " + NameDisplay;

	public string NameDisplay => NameAssembly;

	public string NameGeometry
	{
		get
		{
			return _nameGeometry;
		}
		set
		{
			if (_nameGeometry != value)
			{
				_nameGeometry = value;
				NotifyPropertyChanged("NameGeometry");
			}
		}
	}

	public Brush BackgroundCol
	{
		get
		{
			return _backgroundCol;
		}
		set
		{
			if (_backgroundCol == value)
			{
				return;
			}
			_backgroundCol = value;
			if (Part.ModelLowTesselation != null)
			{
				if (_backgroundCol == BrushDefault)
				{
					Part.ModelLowTesselation.HighLightModel(ColorDefault, null);
				}
				else if (_backgroundCol == BrushSelected)
				{
					Part.ModelLowTesselation.HighLightModel(ColorSelected, null);
				}
				else if (_backgroundCol == BrushHover)
				{
					Part.ModelLowTesselation.HighLightModel(ColorHovered, null);
				}
			}
			NotifyPropertyChanged("BackgroundCol");
			foreach (DisassemblyNodeViewModel item in NodesVm)
			{
				item.RefreshColor();
			}
		}
	}

	public string Desc => Id + " " + NameGeometry + " " + IsMouseHovering + " | " + IsMouseSelected;

	public DisassemblyPart Part { get; }

	public bool IsMouseHovering
	{
		get
		{
			return _isMouseHovering;
		}
		set
		{
			if (_isMouseHovering == value)
			{
				return;
			}
			_isMouseHovering = value;
			NotifyPropertyChanged("IsMouseHovering");
			NotifyPropertyChanged("Desc");
			foreach (DisassemblyNodeViewModel item in NodesVm)
			{
				item.IsMouseHoveringByCode = value;
			}
			ReCalculateColor();
		}
	}

	public bool IsMouseHoveringByCode
	{
		get
		{
			return _isMouseHovering;
		}
		set
		{
			if (_isMouseHovering != value)
			{
				_isMouseHovering = value;
				NotifyPropertyChanged("IsMouseHoveringByCode");
				NotifyPropertyChanged("IsMouseHovering");
				NotifyPropertyChanged("Desc");
				ReCalculateColor();
			}
		}
	}

	public bool IsMouseSelectedBinding
	{
		get
		{
			return _isMouseSelected;
		}
		set
		{
			IsMouseSelected = value;
			NotifyPropertyChanged("IsMouseSelectedBinding");
		}
	}

	public bool IsMouseSelected
	{
		get
		{
			return _isMouseSelected;
		}
		set
		{
			if (_isMouseSelected == value)
			{
				return;
			}
			_isMouseSelected = value;
			NotifyPropertyChanged("IsMouseSelected");
			NotifyPropertyChanged("Desc");
			NotifyPropertyChanged("IsMouseSelectedBinding");
			foreach (DisassemblyNodeViewModel item in NodesVm)
			{
				item.IsMouseSelectedByCode = value;
			}
			ReCalculateColor();
		}
	}

	public bool IsMouseSelectedByCode
	{
		get
		{
			return _isMouseSelected;
		}
		set
		{
			if (_isMouseSelected != value)
			{
				_isMouseSelected = value;
				NotifyPropertyChanged("IsMouseSelectedByCode");
				NotifyPropertyChanged("IsMouseSelected");
				NotifyPropertyChanged("IsMouseSelectedBinding");
				NotifyPropertyChanged("Desc");
				ReCalculateColor();
			}
		}
	}

	public bool Deleted
	{
		get
		{
			return Part?.Deleted ?? false;
		}
		set
		{
			DisassemblyPart part = Part;
			if (part == null || part.Deleted != value)
			{
				if (Part != null)
				{
					Part.Deleted = value;
				}
				NotifyPropertyChanged("Deleted");
				RefreshModelVisibility();
			}
		}
	}

	public bool? Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			if (_visible == value)
			{
				return;
			}
			_visible = value;
			NotifyPropertyChanged("Visible");
			float? highlightAlpha = ((!_visible.HasValue) ? new float?(0.3f) : null);
			foreach (Face item in Part.ModelLowTesselation.GetAllSubModelsWithSelf().SelectMany((Model m) => m.Shells).SelectMany((Shell s) => s.Faces))
			{
				item.HighlightAlpha = highlightAlpha;
			}
			RefreshModelVisibility();
		}
	}

	public ObservableCollection<DisassemblyNodeViewModel> NodesVm { get; } = new ObservableCollection<DisassemblyNodeViewModel>();

	public RadObservableCollection<DisassemblyPartViewModel> PurchasedParts { get; } = new RadObservableCollection<DisassemblyPartViewModel>();

	public byte[] ImageData { get; set; }

	public string ImagePath { get; }

	public string NameAssembly
	{
		get
		{
			return _nameAssembly;
		}
		set
		{
			if (_nameAssembly != value)
			{
				_nameAssembly = value;
				NotifyPropertyChanged("NameAssembly");
			}
		}
	}

	public int MaterialId
	{
		get
		{
			return Part.PnMaterialID;
		}
		set
		{
			if (Part.PnMaterialID != value)
			{
				PnMaterialByUser = true;
				Part.PnMaterialID = value;
				NotifyPropertyChanged("MaterialId");
			}
		}
	}

	public int PurchasedPartTypeId
	{
		get
		{
			return _purchasedPartId;
		}
		set
		{
			if (_purchasedPartId != value)
			{
				_purchasedPartId = value;
				NotifyPropertyChanged("PurchasedPartTypeId");
				NotifyPropertyChanged("VisibilityPurchasedPartIgnoreCollision");
				RefreshPartType();
			}
		}
	}

	public bool PurchasedPartIgnoreCollision
	{
		get
		{
			return _purchasedPartIgnoreCollision;
		}
		set
		{
			_purchasedPartIgnoreCollision = value;
			NotifyPropertyChanged("PurchasedPartIgnoreCollision");
		}
	}

	public Visibility VisibilityPurchasedPartIgnoreCollision
	{
		get
		{
			if (PurchasedPartTypeId <= 0)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public string DisplaySize => Part.GetSizeString(isInchMode: false);

	public double? Thickness => MetaInfo?.Thickness;

	public int? BendCount => MetaInfo?.BendCount;

	public string Dimension => MetaInfo?.Dimension.ToString();

	public int PartCount => NodesVm.Count;

	public string PartType { get; set; }

	public bool PartTypeSmallPart
	{
		get
		{
			return Part.PartTypeSmallPart;
		}
		set
		{
			Part.PartTypeSmallPart = value;
			NotifyPropertyChanged("PartTypeSmallPart");
		}
	}

	public string PartTypeOriginal { get; set; }

	public string DescAdditionalElements
	{
		get
		{
			if (PurchasedParts.Count == 0)
			{
				return null;
			}
			return PurchasedParts.Count + " Zusatzteile";
		}
	}

	public IDocMetadata MetaInfo => Part?.Metadata;

	public bool? KFactorWarnings => MetaInfo?.KFactorWarnings;

	public Visibility MaterialFallbackVisibility
	{
		get
		{
			if (!PnMaterialByUser)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public bool PnMaterialByUser
	{
		get
		{
			return Part.PnMaterialByUser;
		}
		set
		{
			if (Part.PnMaterialByUser != value)
			{
				Part.PnMaterialByUser = value;
				NotifyPropertyChanged("PnMaterialByUser");
				NotifyPropertyChanged("MaterialFallbackVisibility");
			}
		}
	}

	public int? PnMaterialNo { get; set; }

	public int? BendMachineNo { get; set; }

	public string BendMachineDesc => MetaInfo?.BendMachineDesc ?? "???";

	public string SavedArchiv
	{
		get
		{
			IDocMetadata metaInfo = MetaInfo;
			if (metaInfo == null)
			{
				return "???";
			}
			if (metaInfo.SavedArchivNo >= 0)
			{
				return metaInfo.SavedArchivNo + " | " + metaInfo.SavedArchivName;
			}
			return "-";
		}
	}

	public string DocState => MetaInfo?.DocState.ToString() ?? "???";

	public string PPName => MetaInfo?.PPName;

	public int? ValidationGeoErrors => MetaInfo?.ValidationGeoErrors;

	public int? NumberCutouts => MetaInfo?.CutoutsAdded;

	public string Comment
	{
		get
		{
			return MetaInfo?.Comment;
		}
		set
		{
			IDoc3d doc3d = Part?.GetDoc(force: false);
			if (doc3d != null)
			{
				doc3d.Comment = value;
			}
			NotifyPropertyChanged("Comment");
		}
	}

	private Dictionary<int, DisassemblyPartViewModel> _partsById { get; }

	public bool IsHdAnalyzed { get; set; }

	public List<IMaterialArt> Materials => _assemblyViewModel.Materials;

	public List<ComboboxEntry<int>> PurchasedPartTypes => _assemblyViewModel.PuchasedParts;

	public ICommand CmdApplyMaterialToAll => new RelayCommand(CmdApplyMaterialToAllExecute);

	public ICommand CmdExportStep => new RelayCommand(CmdExportStepExecute, CmdExportStepCanExecute);

	public ICommand CmdDelete => new RelayCommand(CmdDeleteExecute);

	public ICommand CmdSavePurchasedPartSetting => new RelayCommand(CmdSavePurchasedPartSettingExecute);

	public ICommand CmdOpenPart => new RelayCommand(CmdOpenPartExecute, CmdOpenPartCanExecute);

	public ICommand CmdAdvancedCalcTools => new RelayCommand(CmdAdvancedCalcToolsExecute);

	public event Action<DisassemblyPartViewModel> PartAnalyzed;

	public void RefreshModelVisibility()
	{
		Part.ModelLowTesselation.Enabled = Visible != false && !Deleted;
		NotifyPropertyChanged("BackgroundCol");
	}

	public DisassemblyPartViewModel(DisassemblyPart part, AssemblyViewModel assemblyViewModel, IPnPathService pnPathService, Dictionary<int, DisassemblyPartViewModel> partsById)
	{
		_partsById = partsById;
		Part = part;
		DisassemblyPart part2 = Part;
		part2.LoadingCompleted = (Action<DisassemblyPart>)Delegate.Combine(part2.LoadingCompleted, new Action<DisassemblyPart>(PartHdAnalyzed));
		_assemblyViewModel = assemblyViewModel;
		_pathService = pnPathService;
		_nameAssembly = Part.ModifiedAssemblyName ?? "";
		_nameGeometry = Part.ModifiedGeometryName ?? "";
		_id = Part.ID;
		ImagePath = Path.Combine(pnPathService.FolderCad3d2Pn, _id + ".png");
		_partsById.Add(_id, this);
	}

	private void RefreshPartType()
	{
		if (PurchasedPartTypeId > 0)
		{
			PartType = _assemblyViewModel.Translator.Translate("l_popup.PopupDisassembly.PurchasedPart") + ": " + _assemblyViewModel.PurchasedPartTypes[PurchasedPartTypeId];
		}
		else if (Part.IsLoaded)
		{
			PartType = GetPartTypeTranslation(Part.Metadata?.SheetType ?? WiCAM.Pn4000.BendModel.BendTools.PartType.Error);
		}
		NotifyPropertyChanged("PartType");
	}

	public void PartHdAnalyzed(DisassemblyPart obj)
	{
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			PartHdAnalyzedMainThread(obj);
		});
	}

	public void PartHdAnalyzedMainThread(DisassemblyPart obj)
	{
		AddPurchasedParts();
		LoadImage();
		NotifyPropertyChanged("MetaInfo");
		NotifyPropertyChanged("BendCount");
		NotifyPropertyChanged("Thickness");
		NotifyPropertyChanged("Dimension");
		NotifyPropertyChanged("DisplaySize");
		NotifyPropertyChanged("Part");
		PartTypeSmallPart = (Part.Metadata?.SheetType ?? WiCAM.Pn4000.BendModel.BendTools.PartType.Error).HasFlag(WiCAM.Pn4000.BendModel.BendTools.PartType.SmallPart);
		PartTypeOriginal = GetPartTypeTranslation(Part.PartInfo.OriginalPartType);
		if (PurchasedPartTypeId == 0)
		{
			PartType = GetPartTypeTranslation(Part.Metadata?.SheetType ?? WiCAM.Pn4000.BendModel.BendTools.PartType.Error);
			NotifyPropertyChanged("PartType");
		}
		NotifyPropertyChanged("PartTypeOriginal");
		NotifyPropertyChanged("PartTypeSmallPart");
		NotifyPropertyChanged("DescAdditionalElements");
		NotifyPropertyChanged("BendMachineDesc");
		NotifyPropertyChanged("SavedArchiv");
		NotifyPropertyChanged("DocState");
		NotifyPropertyChanged("PPName");
		NotifyPropertyChanged("ValidationGeoErrors");
		NotifyPropertyChanged("NumberCutouts");
		NotifyPropertyChanged("Comment");
		this.PartAnalyzed?.Invoke(this);
		IsHdAnalyzed = true;
		NotifyPropertyChanged("IsHdAnalyzed");
	}

	private void AddPurchasedParts()
	{
		if (Part.PartInfo.SimulationInstances == null)
		{
			return;
		}
		using List<SimulationInstance>.Enumerator enumerator = Part.PartInfo.SimulationInstances.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return;
		}
		SimulationInstance current = enumerator.Current;
		if (current.AdditionalParts.Count > 0)
		{
			PurchasedParts.AddRange(from x in current.AdditionalParts
				where _partsById.ContainsKey(x.ModelUid)
				select _partsById[x.ModelUid]);
		}
	}

	public void LoadImage()
	{
		if (File.Exists(ImagePath))
		{
			ImageData = File.ReadAllBytes(ImagePath);
			NotifyPropertyChanged("ImageData");
		}
	}

	private void CmdApplyMaterialToAllExecute()
	{
		_assemblyViewModel.ApplyMaterialToAll(this);
	}

	private bool CmdExportStepCanExecute()
	{
		return File.Exists($"cad3d2pn\\{Id}.step");
	}

	private void CmdExportStepExecute()
	{
		string text = $"cad3d2pn\\{Id}.step";
		if (File.Exists(text))
		{
			string fileTypeCurrentPath = _pathService.GetFileTypeCurrentPath("STEP_PART");
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				InitialDirectory = fileTypeCurrentPath,
				Filter = "STEP file (*.step)|*.step",
				FileName = NameDisplay + ".step"
			};
			if (saveFileDialog.ShowDialog() ?? false)
			{
				string fileName = saveFileDialog.FileName;
				string directoryName = Path.GetDirectoryName(fileName);
				File.Copy(text, fileName, overwrite: true);
				_pathService.SetFileTypeCurrentPath("STEP_PART", directoryName);
			}
		}
	}

	private void CmdDeleteExecute()
	{
		Deleted = true;
	}

	private void CmdSavePurchasedPartSettingExecute()
	{
		string originalAssemblyName = Part.OriginalAssemblyName;
		int purchasedPartTypeId = PurchasedPartTypeId;
		bool isMountedBeforeBending = !PurchasedPartIgnoreCollision;
		IPrefabricatedPart prefabricatedPart = _assemblyViewModel.PrefabricatedPartsManager.FindPart(originalAssemblyName, checkDetectionEnabled: false);
		if (prefabricatedPart != null)
		{
			if (purchasedPartTypeId == 0)
			{
				_assemblyViewModel.PrefabricatedPartsManager.RemovePart(prefabricatedPart.Name);
				return;
			}
			_assemblyViewModel.PrefabricatedPartsManager.AddPart(new PurchasedPart
			{
				Name = originalAssemblyName,
				IsMountedBeforeBending = isMountedBeforeBending,
				Type = purchasedPartTypeId,
				AdditionalProperties = prefabricatedPart.AdditionalProperties
			});
		}
		else if (purchasedPartTypeId != 0)
		{
			_assemblyViewModel.PrefabricatedPartsManager.AddPart(new PurchasedPart
			{
				Name = originalAssemblyName,
				IsMountedBeforeBending = isMountedBeforeBending,
				Type = purchasedPartTypeId
			});
		}
	}

	private bool CmdOpenPartCanExecute()
	{
		return Part.Metadata != null;
	}

	private void CmdOpenPartExecute()
	{
		if (CmdOpenPartCanExecute())
		{
			_assemblyViewModel.Step6SaveAssemblyAndOpenPart(this);
		}
	}

	private void CmdAdvancedCalcToolsExecute()
	{
	}

	private void ReCalculateColor()
	{
		if (IsMouseSelected)
		{
			BackgroundCol = BrushSelected;
		}
		else if (IsMouseHovering)
		{
			BackgroundCol = BrushHover;
		}
		else
		{
			BackgroundCol = BrushDefault;
		}
	}

	public static string GetPartTypeTranslation(PartType type)
	{
		string resourceKey = string.Format("l_enum.DisassemblyPartType.{0}", type.ToString().Replace(" ", "").Replace(",", ""));
		string text = Application.Current.TryFindResource(resourceKey) as string;
		if (text == null)
		{
			text = type.ToString();
		}
		return text;
	}
}
