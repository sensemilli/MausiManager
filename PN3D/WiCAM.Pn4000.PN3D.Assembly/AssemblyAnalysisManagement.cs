using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.BendModel.Loader.Loader;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Screen;
using WiCAM.Pn4000.PartsReader;
using WiCAM.Pn4000.PartsReader.Contracts;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Pn4000.PN3D.Assembly.DisassemblyWindowHelpers;
using WiCAM.Pn4000.PN3D.Converter;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Doc.Serializer;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Assembly;

public class AssemblyAnalysisManagement : IAssemblyAnalysisManagement
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass45_0
	{
		public DisassemblyPart part;

		internal bool _003CConvertPartToDoc_003Eb__0(IMaterialArt m)
		{
			return m.Number == this.part.PnMaterialID;
		}

		internal bool _003CConvertPartToDoc_003Eb__1(IMaterialArt m)
		{
			return m.Number == this.part.PnMaterialID;
		}
	}

	[CompilerGenerated]
	private sealed class _003CConvertPartToDoc_003Ed__45 : IEnumerable<(DisassemblyPart part, IDoc3d? doc, F2exeReturnCode code)>, IEnumerable, IEnumerator<(DisassemblyPart part, IDoc3d? doc, F2exeReturnCode code)>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private (DisassemblyPart part, IDoc3d? doc, F2exeReturnCode code) _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerable<DisassemblyPart> parts;

		public IEnumerable<DisassemblyPart> _003C_003E3__parts;

		public AssemblyAnalysisManagement _003C_003E4__this;

		private IEnumerator<DisassemblyPart> _003C_003E7__wrap1;

        (DisassemblyPart part, IDoc3d? doc, F2exeReturnCode code) IEnumerator<(DisassemblyPart part, IDoc3d? doc, F2exeReturnCode code)>.Current
        {
            [DebuggerHidden]
            get
            {
                return this._003C_003E2__current;
            }
        }

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return this._003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CConvertPartToDoc_003Ed__45(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			this._003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = this._003C_003E1__state;
			if (num == -3 || (uint)(num - 1) <= 1u)
			{
				try
				{
				}
				finally
				{
					this._003C_003Em__Finally1();
				}
			}
			this._003C_003E7__wrap1 = null;
			this._003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = this._003C_003E1__state;
				AssemblyAnalysisManagement assemblyAnalysisManagement = this._003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					this._003C_003E1__state = -1;
					this._003C_003E7__wrap1 = this.parts.GetEnumerator();
					this._003C_003E1__state = -3;
					break;
				case 1:
					this._003C_003E1__state = -3;
					break;
				case 2:
					this._003C_003E1__state = -3;
					break;
				}
				if (this._003C_003E7__wrap1.MoveNext())
				{
					_003C_003Ec__DisplayClass45_0 CS_0024_003C_003E8__locals22 = new _003C_003Ec__DisplayClass45_0
					{
						part = this._003C_003E7__wrap1.Current
					};
					F2exeReturnCode item = F2exeReturnCode.OK;
					if (CS_0024_003C_003E8__locals22.part == null)
					{
						this._003C_003E2__current = (part: null, doc: null, code: F2exeReturnCode.ERROR_NO_DATA_IMPORT);
						this._003C_003E1__state = 1;
						return true;
					}
					General3DConfig general3DConfig = assemblyAnalysisManagement._configProvider.InjectOrCreate<General3DConfig>();
					string text = ((assemblyAnalysisManagement._assembly.DisassemblyParts.Count == 1) ? ((!string.IsNullOrEmpty(assemblyAnalysisManagement._assembly.FilenameImport)) ? Path.GetFileNameWithoutExtension(assemblyAnalysisManagement._assembly.FilenameImport) : Path.GetFileNameWithoutExtension(assemblyAnalysisManagement._importSetting.Filename)) : ((!general3DConfig.P3D_UseAssemblyName) ? CS_0024_003C_003E8__locals22.part.ModifiedGeometryName : CS_0024_003C_003E8__locals22.part.ModifiedAssemblyName));
					IDoc3d target = CS_0024_003C_003E8__locals22.part.Doc;
					if (target == null)
					{
						CS_0024_003C_003E8__locals22.part.DocTemp?.TryGetTarget(out target);
					}
					if (target != null)
					{
						if (string.IsNullOrEmpty(target.DiskFile.Header.ModelName))
						{
							target.DiskFile.Header.ModelName = text;
						}
						target.DocAssemblyId = CS_0024_003C_003E8__locals22.part.ID;
						if (target.EntryModel3D.PartInfo.PartType.HasFlag(global::WiCAM.Pn4000.BendModel.BendTools.PartType.SmallPart) != CS_0024_003C_003E8__locals22.part.PartTypeSmallPart)
						{
							target.SetSmallPartType(CS_0024_003C_003E8__locals22.part.PartTypeSmallPart);
						}
						IMaterialArt materialArt = assemblyAnalysisManagement._materials.MaterialList.FirstOrDefault((IMaterialArt m) => m.Number == CS_0024_003C_003E8__locals22.part.PnMaterialID);
						if (materialArt != null && materialArt.Number != target.Material?.Number)
						{
							target.Material = materialArt;
						}
						if (CS_0024_003C_003E8__locals22.part.MachineNo.HasValue && CS_0024_003C_003E8__locals22.part.MachineNo != target.MetaData.BendMachineNo)
						{
							item = assemblyAnalysisManagement._bendPipe.SelectBendMachineById(CS_0024_003C_003E8__locals22.part.MachineNo.Value, target);
						}
					}
					else
					{
						string text2 = Path.Combine(assemblyAnalysisManagement._pathService.FolderCad3d2Pn, CS_0024_003C_003E8__locals22.part.ID + ".c3do");
						if (File.Exists(text2))
						{
							IMaterialArt overrideMaterial = assemblyAnalysisManagement._materials.MaterialList.FirstOrDefault((IMaterialArt m) => m.Number == CS_0024_003C_003E8__locals22.part.PnMaterialID);
							assemblyAnalysisManagement._currentDocProvider.CurrentDoc?.BendSimulation?.Pause();
							item = (F2exeReturnCode)assemblyAnalysisManagement._docPipe.OpenFile3dWithParameters(text2, CS_0024_003C_003E8__locals22.part.MachineNo ?? (-1), overrideMaterial, text, out target);
							if (target != null)
							{
								target.DocAssemblyId = CS_0024_003C_003E8__locals22.part.ID;
								if (target.EntryModel3D.PartInfo.PartType.HasFlag(global::WiCAM.Pn4000.BendModel.BendTools.PartType.SmallPart) != CS_0024_003C_003E8__locals22.part.PartTypeSmallPart)
								{
									target.SetSmallPartType(CS_0024_003C_003E8__locals22.part.PartTypeSmallPart);
								}
								General3DConfig general3DConfig2 = assemblyAnalysisManagement._configProvider.InjectOrCreate<General3DConfig>();
								CS_0024_003C_003E8__locals22.part.DocTemp = new WeakReference<IDoc3d>(target);
								if (general3DConfig2.P3D_AnalyzeAssemblyKeepDocs)
								{
									CS_0024_003C_003E8__locals22.part.Doc = target;
								}
							}
						}
					}
					if (target != null)
					{
						target.Assembly = assemblyAnalysisManagement._assembly;
						target.IsAssemblyLoading = false;
						target.PnMaterialByUser = CS_0024_003C_003E8__locals22.part.PnMaterialByUser;
						target?.UpdateGeneralInfo();
					}
					this._003C_003E2__current = (part: CS_0024_003C_003E8__locals22.part, doc: target, code: item);
					this._003C_003E1__state = 2;
					return true;
				}
				this._003C_003Em__Finally1();
				this._003C_003E7__wrap1 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			this._003C_003E1__state = -1;
			if (this._003C_003E7__wrap1 != null)
			{
				this._003C_003E7__wrap1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

        [DebuggerHidden]
        IEnumerator<(DisassemblyPart part, IDoc3d? doc, F2exeReturnCode code)> IEnumerable<(DisassemblyPart part, IDoc3d? doc, F2exeReturnCode code)>.GetEnumerator()
        {
            _003CConvertPartToDoc_003Ed__45 enumerator;
            if (this._003C_003E1__state == -2 && this._003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
            {
                this._003C_003E1__state = 0;
                enumerator = this;
            }
            else
            {
                enumerator = new _003CConvertPartToDoc_003Ed__45(0)
                {
                    _003C_003E4__this = this._003C_003E4__this
                };
            }
            enumerator.parts = this._003C_003E3__parts;
            return enumerator;
        }

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<(DisassemblyPart, IDoc3d, F2exeReturnCode)>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetPartsByPriority_003Ed__27 : IEnumerable<DisassemblyPart>, IEnumerable, IEnumerator<DisassemblyPart>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private DisassemblyPart _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerable<DisassemblyPart> allParts;

		public IEnumerable<DisassemblyPart> _003C_003E3__allParts;

		public AssemblyAnalysisManagement _003C_003E4__this;

		private CancellationToken cancellationToken;

		public CancellationToken _003C_003E3__cancellationToken;

		private HashSet<DisassemblyPart> _003Cparts_003E5__2;

		DisassemblyPart IEnumerator<DisassemblyPart>.Current
		{
			[DebuggerHidden]
			get
			{
				return this._003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return this._003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetPartsByPriority_003Ed__27(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			this._003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			this._003Cparts_003E5__2 = null;
			this._003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = this._003C_003E1__state;
			AssemblyAnalysisManagement assemblyAnalysisManagement = this._003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				this._003C_003E1__state = -1;
				this._003Cparts_003E5__2 = new HashSet<DisassemblyPart>(this.allParts);
				break;
			case 1:
				this._003C_003E1__state = -1;
				break;
			case 2:
				this._003C_003E1__state = -1;
				break;
			}
			if (this._003Cparts_003E5__2.Count > 0 && !this.cancellationToken.IsCancellationRequested)
			{
				DisassemblyPart topPriority = assemblyAnalysisManagement.TopPriority;
				if (topPriority != null && this._003Cparts_003E5__2.Remove(topPriority))
				{
					this._003C_003E2__current = topPriority;
					this._003C_003E1__state = 1;
					return true;
				}
				DisassemblyPart item = this._003Cparts_003E5__2.First();
				this._003Cparts_003E5__2.Remove(item);
				this._003C_003E2__current = item;
				this._003C_003E1__state = 2;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<DisassemblyPart> IEnumerable<DisassemblyPart>.GetEnumerator()
		{
			_003CGetPartsByPriority_003Ed__27 _003CGetPartsByPriority_003Ed__;
			if (this._003C_003E1__state == -2 && this._003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				this._003C_003E1__state = 0;
				_003CGetPartsByPriority_003Ed__ = this;
			}
			else
			{
				_003CGetPartsByPriority_003Ed__ = new _003CGetPartsByPriority_003Ed__27(0)
				{
					_003C_003E4__this = this._003C_003E4__this
				};
			}
			_003CGetPartsByPriority_003Ed__.allParts = this._003C_003E3__allParts;
			_003CGetPartsByPriority_003Ed__.cancellationToken = this._003C_003E3__cancellationToken;
			return _003CGetPartsByPriority_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<DisassemblyPart>)this).GetEnumerator();
		}
	}

	private DisassemblySimpleModel _mainModel;

	private Assembly _assembly;

	private IImportArg _importSetting;

	private int _parallelCount;

	private IGlobals _globals;

	private readonly IPnPathService _pathService;

	private readonly IConfigProvider _configProvider;

	private readonly IPartAnalyzer _partAnalyzer;

	private readonly IPrefabricatedPartsManager _prefabricatedPartsManager;

	private readonly ILogCenterService _logCenterService;

	private readonly ITranslator _translator;

	private readonly IMessageLogGlobal _logGlobal;

	private readonly ICurrentDocProvider _currentDocProvider;

	private readonly IMaterialManager _materials;

	private readonly IPN3DBendPipe _bendPipe;

	private readonly IPN3DDocPipe _docPipe;

	private readonly IScreenshotScreen _screenshotScreen;

	private readonly ISpatialLoader _spatialLoader;

	private readonly ISpatialAssemblyLoader _spatialAssemblyLoader;

	private List<PurchasedPartsMerger.PurchasedPart> _purchasedPartsForAnalyze;

	private List<Parts1dMerger.Part1d> _parts1DForAnalyze;

	private static int PreviewLength = 400;

	private static int PreviewAssemblyLength = 800;

	public DisassemblyPart TopPriority { get; set; }

	public AssemblyAnalysisManagement(IGlobals globals, IPnPathService pathService, IPartAnalyzer partAnalyzer, IPrefabricatedPartsManager prefabricatedPartsManager, ILogCenterService logCenterService, ITranslator translator, IMessageLogGlobal logGlobal, ICurrentDocProvider currentDocProvider, IConfigProvider configProvider, IMaterialManager materials, IPN3DBendPipe bendPipe, IPN3DDocPipe docPipe, IScreenshotScreen screenshotScreen, ISpatialAssemblyLoader spatialAssemblyLoader, ISpatialLoader spatialLoader)
	{
		this._globals = globals;
		this._pathService = pathService;
		this._partAnalyzer = partAnalyzer;
		this._prefabricatedPartsManager = prefabricatedPartsManager;
		this._logCenterService = logCenterService;
		this._translator = translator;
		this._logGlobal = logGlobal;
		this._currentDocProvider = currentDocProvider;
		this._configProvider = configProvider;
		this._materials = materials;
		this._bendPipe = bendPipe;
		this._docPipe = docPipe;
		this._screenshotScreen = screenshotScreen;
		this._spatialAssemblyLoader = spatialAssemblyLoader;
		this._spatialLoader = spatialLoader;
	}

	public void Init(Assembly assembly, IImportArg importSetting)
	{
		this._assembly = assembly;
		GeneralUserSettingsConfig generalUserSettingsConfig = this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		int num = importSetting.ParallelCount ?? generalUserSettingsConfig.P3D_AnalyzeParallelDegreeOfParallelism ?? 1;
		this._parallelCount = ((num == -1) ? num : Math.Max(1, num));
		this._importSetting = importSetting;
	}

	[IteratorStateMachine(typeof(_003CGetPartsByPriority_003Ed__27))]
	private IEnumerable<DisassemblyPart> GetPartsByPriority(IEnumerable<DisassemblyPart> allParts, CancellationToken cancellationToken)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetPartsByPriority_003Ed__27(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__allParts = allParts,
			_003C_003E3__cancellationToken = cancellationToken
		};
	}

	private void SpawnAndWaitForWorkers(HashSet<DisassemblyPart> parts, Action<DisassemblyPart> work, CancellationToken cancellationToken)
	{
		Parallel.ForEach(this.GetPartsByPriority(parts, cancellationToken), new ParallelOptions
		{
			MaxDegreeOfParallelism = this._parallelCount
		}, work);
	}

	public void Step1LoadLowTesselation(CancellationToken cancellationToken)
	{
		if (this._assembly.LoadingStatus >= Assembly.EnumLoadingStatus.LowTesselationLoaded)
		{
			return;
		}
		this.SpawnAndWaitForWorkers(new HashSet<DisassemblyPart>(this._assembly.DisassemblyParts), LoadLowTesselation, cancellationToken);
		if (!cancellationToken.IsCancellationRequested)
		{
			this.ScreenshotAssembly(this._globals, this._assembly.RootNode);
			this._assembly.LoadingStatus = Assembly.EnumLoadingStatus.LowTesselationLoaded;
			if (this._assembly.DisassemblyParts.Count == 0)
			{
				this._assembly.LoadingStatus = Assembly.EnumLoadingStatus.AnalyzedHd;
			}
		}
	}

	private void LoadLowTesselation(DisassemblyPart part)
	{
		if (part != null)
		{
			if (part.ModelLowTesselation == null)
			{
				part.CalcLen(part.ModelLowTesselation = this._spatialAssemblyLoader.LoadModelPart(this._pathService.FolderCad3d2Pn, lowTess: true, part.ID, part.PartInfo.GeometryType, part.OriginalAssemblyName, part.OriginalGeometryName, null));
			}
			string text = Path.Combine(this._pathService.FolderCad3d2Pn, part.ID + ".png");
			if (!File.Exists(text))
			{
				AssemblyAnalysisManagement.TakeScreenshot(text, this._globals, part.ModelLowTesselation, AssemblyAnalysisManagement.PreviewLength, AssemblyAnalysisManagement.PreviewLength);
			}
		}
	}

	private static void TakeScreenshot(string fileScreenshot, IGlobals globals, Model model, int width, int height)
	{
		AssemblyAnalysisManagement.TakeScreenshot(fileScreenshot, globals, new List<Model> { model }, width, height);
	}

	private static void TakeScreenshot(string fileScreenshot, IGlobals globals, List<Model> models, int width, int height)
	{
		if (models.All((Model x) => x == null))
		{
			return;
		}
		global::WiCAM.Pn4000.BendModel.Base.Matrix4d identity = global::WiCAM.Pn4000.BendModel.Base.Matrix4d.Identity;
		identity *= global::WiCAM.Pn4000.BendModel.Base.Matrix4d.RotationZ(0.7853981852531433);
		identity *= global::WiCAM.Pn4000.BendModel.Base.Matrix4d.RotationX(1.0471975803375244);
		int border = 10;
		Color value = new Color(0f, 0f, 0f, 1f);
		float value2 = 3f;
		foreach (Face item in models.SelectMany((Model x) => x.GetAllFaces()))
		{
			foreach (FaceHalfEdge allEdge in item.GetAllEdges())
			{
				allEdge.HighlightWidth = value2;
				allEdge.HighlightColor = value;
			}
			item.HighlightColor = item.ColorInitial;
		}
		globals.ScreenshotScreen.PrintScreen(models, fileScreenshot, identity, border, width, height);
		foreach (Face item2 in models.SelectMany((Model x) => x.GetAllFaces()))
		{
			foreach (FaceHalfEdge allEdge2 in item2.GetAllEdges())
			{
				allEdge2.HighlightWidth = null;
				allEdge2.HighlightColor = null;
			}
			item2.HighlightColor = null;
		}
	}

	private void ScreenshotAssembly(IGlobals globals, DisassemblyPartNode rootNode)
	{
		List<Model> list = new List<Model>();
		foreach (DisassemblyPartNode item in rootNode.Children.SelectAndManyRecursive((DisassemblyPartNode x) => x.Children))
		{
			Model model = item.Part?.ModelLowTesselation;
			if (model != null)
			{
				if (item.ModelLowTesselation == null)
				{
					Model model2 = new Model();
					model2.ReferenceModel.Add(new ModelInstance(model, model2));
					model2.Transform = item.WorldMatrix;
					item.ModelLowTesselation = model2;
				}
				list.Add(item.ModelLowTesselation);
			}
		}
		string text = Path.Combine(this._pathService.FolderCad3d2Pn, "asm.png");
		if (!File.Exists(text))
		{
			AssemblyAnalysisManagement.TakeScreenshot(text, globals, list, AssemblyAnalysisManagement.PreviewAssemblyLength, AssemblyAnalysisManagement.PreviewAssemblyLength);
		}
	}

	public void Step2AnalyzeHd(CancellationToken cancellationToken)
	{
		if (this._assembly.LoadingStatus >= Assembly.EnumLoadingStatus.AnalyzedHd)
		{
			return;
		}
		HashSet<DisassemblyPart> hashSet = new HashSet<DisassemblyPart>();
		HashSet<DisassemblyPart> hashSet2 = new HashSet<DisassemblyPart>();
		foreach (DisassemblyPart disassemblyPart in this._assembly.DisassemblyParts)
		{
			IPrefabricatedPart prefabricatedPart = this._prefabricatedPartsManager.FindPart(disassemblyPart.OriginalAssemblyName, checkDetectionEnabled: true);
			if (disassemblyPart.PartInfo.GeometryType == GeometryType.Contour)
			{
				hashSet.Add(disassemblyPart);
			}
			else if (prefabricatedPart != null)
			{
				disassemblyPart.PartInfo.PurchasedPart = prefabricatedPart.Type;
				disassemblyPart.PartInfo.IgnoreCollision = prefabricatedPart.IgnoreAtCollision;
				disassemblyPart.PartInfo.PartType = global::WiCAM.Pn4000.BendModel.BendTools.PartType.SmallPart;
				hashSet.Add(disassemblyPart);
			}
			else
			{
				hashSet2.Add(disassemblyPart);
			}
		}
		foreach (DisassemblyPart item in hashSet)
		{
			item.IsAdditionalPart = true;
		}
		this._purchasedPartsForAnalyze = new List<PurchasedPartsMerger.PurchasedPart>();
		this._parts1DForAnalyze = new List<Parts1dMerger.Part1d>();
		if (!cancellationToken.IsCancellationRequested)
		{
			this.SpawnAndWaitForWorkers(hashSet, ProcessAdditionalParts, cancellationToken);
		}
		if (!cancellationToken.IsCancellationRequested)
		{
			this.SpawnAndWaitForWorkers(hashSet2, ProcessOtherParts, cancellationToken);
		}
		if (!cancellationToken.IsCancellationRequested)
		{
			this._assembly.LoadingStatus = Assembly.EnumLoadingStatus.AnalyzedHd;
		}
	}

	private void ProcessAdditionalParts(DisassemblyPart part)
	{
		Path.Combine(this._pathService.FolderCad3d2Pn, part.ID + ".c3do");
		this.AnalyzePart(part, out var entryModel, null, null);
		if (entryModel != null)
		{
			if (part.PartInfo.GeometryType == GeometryType.Contour)
			{
				lock (this._parts1DForAnalyze)
				{
					this._parts1DForAnalyze.Add(new Parts1dMerger.Part1d
					{
						EntryModel = entryModel,
						AssemblyName = part.OriginalAssemblyName,
						WorldMatrices = part.Matrixes
					});
				}
			}
			else
			{
				lock (this._purchasedPartsForAnalyze)
				{
					this._purchasedPartsForAnalyze.Add(new PurchasedPartsMerger.PurchasedPart
					{
						EntryModel = entryModel,
						AssemblyName = part.OriginalAssemblyName,
						WorldMatrices = part.Matrixes,
						IgnoreCollision = part.PartInfo.IgnoreCollision,
						UserProperties = part.UserProperties.Select((UserProperty x) => (Name: x.Name, Properties: x.Properties)).ToList()
					});
				}
			}
		}
		AssemblyAnalysisManagement.TakeScreenshot(Path.Combine(this._pathService.FolderCad3d2Pn, part.ID + ".png"), this._globals, entryModel, AssemblyAnalysisManagement.PreviewLength, AssemblyAnalysisManagement.PreviewLength);
		part.LoadingCompleted?.Invoke(part);
	}

	private void ProcessOtherParts(DisassemblyPart part)
	{
		using Activity activity = Activity.Current?.Source.StartActivity("AssemblyAnalysisManagement - ProcessOtherParts");
		activity.AddTag("PartId", part.ID);
		activity.AddTag("PartName", part.ModifiedAssemblyName);
		if (part.Metadata == null)
		{
			try
			{
				part.Metadata = DocMetadata.Load(this._pathService, part.ID);
				if (part.Metadata != null)
				{
					part.LoadDataFromMetadata();
				}
			}
			catch (Exception)
			{
			}
			if (part.Metadata == null)
			{
				part.ImportedFilename = this._assembly.FilenameImport;
				this.AnalyzePart(part, out var entryModel, this._purchasedPartsForAnalyze, this._parts1DForAnalyze);
				AssemblyAnalysisManagement.TakeScreenshot(Path.Combine(this._pathService.FolderCad3d2Pn, part.ID + ".png"), this._globals, entryModel, AssemblyAnalysisManagement.PreviewLength, AssemblyAnalysisManagement.PreviewLength);
			}
		}
		part.LoadingCompleted?.Invoke(part);
	}

	public void AnalyzePart(DisassemblyPart part, out Model entryModel, List<PurchasedPartsMerger.PurchasedPart> purchasedPartsForAnalyze, List<Parts1dMerger.Part1d> parts1DForAnalyze)
	{
		entryModel = null;
		try
		{
			this._partAnalyzer.Analyze(part, this._globals.MessageDisplay, this._assembly.DisassemblyParts.Count, null, isAssembly: true, null, out var _, out var _, out entryModel, purchasedPartsForAnalyze, parts1DForAnalyze, this._importSetting);
			if (entryModel != null)
			{
				part.CalcLen(entryModel);
			}
			IDoc3d target = part.Doc;
			if (target == null)
			{
				part.DocTemp?.TryGetTarget(out target);
			}
			if (target != null)
			{
				target.Assembly = this._assembly;
			}
		}
		catch (Exception ex)
		{
			string message = this._translator.Translate("PartAnalyzer.Error", ex.Message.ToString());
			this._logCenterService.CatchRaport(ex);
			Application.Current.Dispatcher.Invoke(delegate
			{
				this._logGlobal.ShowErrorMessage(message);
			});
			part.PartInfo.OriginalPartType = global::WiCAM.Pn4000.BendModel.BendTools.PartType.Error;
		}
		lock (part)
		{
			part.IsLoaded = true;
		}
	}

	public void Step3ExportData(string assemblyName, IEnumerable<DisassemblyPart> parts)
	{
		bool useAssemblyName = true;
		IPartsReader partsReader = new global::WiCAM.Pn4000.PartsReader.PartsReader();
		partsReader.SerializeAssembly(new global::WiCAM.Pn4000.PartsReader.DataClasses.Assembly
		{
			MajorVersion = partsReader.MajorVersion,
			MinorVersion = partsReader.MinorVersion,
			RootPartName = assemblyName,
			DisassemblyParts = parts.Select(delegate(DisassemblyPart part)
			{
				global::WiCAM.Pn4000.PartsReader.DataClasses.DisassemblyPart convertedPart = part.GetConvertedPart();
				convertedPart.InstanceNumber = part.Matrixes.Count;
				convertedPart.IsAssemblyName = useAssemblyName;
				if (useAssemblyName)
				{
					convertedPart.OriginalName = convertedPart.OriginalAssemblyName;
				}
				else
				{
					convertedPart.OriginalName = convertedPart.OriginalGeometryName;
				}
				convertedPart.Name = convertedPart.OriginalName;
				return convertedPart;
			}).ToList()
		}, Path.Combine(this._pathService.FolderCad3d2Pn, "Parts.xml"));
	}

	public static Assembly LoadTempAssembly(IPnPathService pathService)
	{
		return AsmSerializer.DecompressAndDeserialize(Path.Combine(pathService.FolderCad3d2Pn, "TempAssembly.json"));
	}

	public void SaveAssembly()
	{
		string folderCad3d2Pn = this._pathService.FolderCad3d2Pn;
		foreach (DisassemblyPart item in this._assembly.DisassemblyParts.Where((DisassemblyPart x) => x.Metadata != null))
		{
			item.SaveDataToMetadata();
			item.Metadata.Save(this._pathService);
		}
		AsmSerializer.SerializeAndCompress(Path.Combine(folderCad3d2Pn, "TempAssembly.json"), this._assembly);
	}

	public F2exeReturnCode Step4OpenPartAndSaveAssembly(DisassemblyPart part)
	{
		return this.Step4OpenPartAndSaveAssembly(new DisassemblyPart[1] { part });
	}

	public F2exeReturnCode Step4OpenPartAndSaveAssembly(IEnumerable<DisassemblyPart> parts)
	{
		this.SaveAssembly();
		Stack<F2exeReturnCode> stack = new Stack<F2exeReturnCode>();
		foreach (var (disassemblyPart, doc3d, f2exeReturnCode) in this.ConvertPartToDoc(parts))
		{
			this._assembly.LastOpenedPartId = disassemblyPart?.ID;
			if (doc3d != null)
			{
				if (f2exeReturnCode != 0)
				{
					disassemblyPart.PartInfo.PartType = global::WiCAM.Pn4000.BendModel.BendTools.PartType.Error;
					doc3d.EntryModel3D.PartInfo.PartType = global::WiCAM.Pn4000.BendModel.BendTools.PartType.Error;
				}
				this._currentDocProvider.CurrentDoc = doc3d;
			}
			if (f2exeReturnCode != 0)
			{
				stack.Push(f2exeReturnCode);
			}
			else if ((doc3d?.EntryModel3D?.Shells?.Count).GetValueOrDefault() == 0)
			{
				stack.Push(F2exeReturnCode.ERROR_NO_DATA_IMPORT);
			}
			else if (!doc3d.EntryModel3D.PartInfo.PartType.HasFlag(global::WiCAM.Pn4000.BendModel.BendTools.PartType.FlatSheetMetal) && !doc3d.EntryModel3D.PartInfo.PartType.HasFlag(global::WiCAM.Pn4000.BendModel.BendTools.PartType.UnfoldableSheetMetal) && !doc3d.EntryModel3D.PartInfo.PartType.HasFlag(global::WiCAM.Pn4000.BendModel.BendTools.PartType.Tube) && !doc3d.EntryModel3D.PartInfo.PartType.HasFlag(global::WiCAM.Pn4000.BendModel.BendTools.PartType.RollOffPlate))
			{
				stack.Push(F2exeReturnCode.ERROR_INVALID_GEOMETRY);
			}
			else if (doc3d.HasRadiusChangeErrors)
			{
				stack.Push(F2exeReturnCode.ERROR_RADIUS_ADJUSTMENT_NOT_POSSIBLE);
			}
			else
			{
				stack.Push(F2exeReturnCode.OK);
			}
		}
		F2exeReturnCode num = stack.Distinct().DefaultIfEmpty(F2exeReturnCode.ERROR_NO_DATA_IMPORT).MinBy(delegate(F2exeReturnCode x)
		{
			if (x > F2exeReturnCode.OK)
			{
				return 1;
			}
			return (x < F2exeReturnCode.OK) ? 2 : 3;
		});
		if (num != 0)
		{
			this.Step3ExportData(this._importSetting.Filename, parts.OrderBy((DisassemblyPart x) => x.ID));
		}
		return num;
	}

	[IteratorStateMachine(typeof(_003CConvertPartToDoc_003Ed__45))]
	private IEnumerable<(DisassemblyPart part, IDoc3d? doc, F2exeReturnCode code)> ConvertPartToDoc(IEnumerable<DisassemblyPart> parts)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CConvertPartToDoc_003Ed__45(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__parts = parts
		};
	}

	private void RemoveIgnoredNodes(List<AssemblyNode> nodes, bool countHidden, bool countSuppressed)
	{
		AssemblyNode[] array = nodes.ToArray();
		foreach (AssemblyNode assemblyNode in array)
		{
			if ((!countHidden && assemblyNode.Hidden) || (!countSuppressed && assemblyNode.Suppressed))
			{
				nodes.Remove(assemblyNode);
			}
			else
			{
				this.RemoveIgnoredNodes(assemblyNode.Children, countHidden, countSuppressed);
			}
		}
	}

	public void ImportPartList(Assembly assembly)
	{
		List<AssemblyNode> list = this._spatialAssemblyLoader.LoadSpatialAssemblyFile(Path.Combine(this._pathService.FolderCad3d2Pn, "assemblyInternal.bin"));
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		this.RemoveIgnoredNodes(list, general3DConfig.P3D_ImportPartCountHidden, general3DConfig.P3D_ImportPartCountSuppressed);
		Dictionary<int, DisassemblyPart> dictParts = new Dictionary<int, DisassemblyPart>();
		foreach (AssemblyNode item2 in list.SelectAndManyRecursive((AssemblyNode x) => x.Children))
		{
			foreach (AssemblyBody body in item2.Bodies)
			{
				if (body.BodyId.HasValue && !dictParts.ContainsKey(body.BodyId.Value))
				{
					DisassemblyPart disassemblyPart = new DisassemblyPart(item2, body, assembly.Guid);
					dictParts.Add(body.BodyId.Value, disassemblyPart);
					assembly.DisassemblyParts.Add(disassemblyPart);
				}
			}
		}
		foreach (IGrouping<string, DisassemblyPart> item3 in from x in dictParts.Values
			group x by x.ModifiedAssemblyName into g
			where g.Count() > 1
			select g)
		{
			int num = 1;
			foreach (DisassemblyPart item4 in item3.OrderBy((DisassemblyPart x) => x.ID).Skip(1))
			{
				num++;
				item4.ModifiedAssemblyName += $"[{num}]";
			}
		}
		foreach (IGrouping<string, DisassemblyPart> item5 in from x in dictParts.Values
			group x by x.ModifiedGeometryName into g
			where g.Count() > 1
			select g)
		{
			int num2 = 1;
			foreach (DisassemblyPart item6 in item5.OrderBy((DisassemblyPart x) => x.ID).Skip(1))
			{
				num2++;
				item6.ModifiedGeometryName += $"[{num2}]";
			}
		}
		assembly.RootNode = new DisassemblyPartNode
		{
			Transform = global::WiCAM.Pn4000.BendModel.Base.Matrix4d.Identity,
			HiddenInAssembly = false,
			SuppressedInAssembly = false,
			Parent = null,
			Children = new List<DisassemblyPartNode>(list.Count),
			Part = null
		};
		foreach (AssemblyNode item7 in list)
		{
			assembly.RootNode.Children.Add(CreateNode(item7, assembly.RootNode));
		}
		static void AddUserProperties(DisassemblyPart part, List<UserProperites> userProperties)
		{
			if (userProperties != null)
			{
				foreach (UserProperites prop in userProperties)
				{
					UserProperty userProperty = part.UserProperties.FirstOrDefault((UserProperty x) => x.Name == prop.Name);
					if (userProperty == null)
					{
						userProperty = new UserProperty
						{
							Name = prop.Name
						};
						part.UserProperties.Add(userProperty);
					}
					foreach (global::WiCAM.Pn4000.BendModel.Loader.Loader.UserProperty property in prop.Properties)
					{
						if (!userProperty.Properties.TryAdd(property.Name, property.Value))
						{
							userProperty.Properties[property.Name] = property.Value;
						}
					}
				}
			}
		}
		DisassemblyPartNode CreateNode(AssemblyNode node, DisassemblyPartNode parent)
		{
			global::WiCAM.Pn4000.BendModel.Base.Matrix4d identity = global::WiCAM.Pn4000.BendModel.Base.Matrix4d.Identity;
			identity[0, 0] = node.Transform[0];
			identity[0, 1] = node.Transform[1];
			identity[0, 2] = node.Transform[2];
			identity[0, 3] = node.Transform[3];
			identity[1, 0] = node.Transform[4];
			identity[1, 1] = node.Transform[5];
			identity[1, 2] = node.Transform[6];
			identity[1, 3] = node.Transform[7];
			identity[2, 0] = node.Transform[8];
			identity[2, 1] = node.Transform[9];
			identity[2, 2] = node.Transform[10];
			identity[2, 3] = node.Transform[11];
			identity[3, 0] = node.Transform[12];
			identity[3, 1] = node.Transform[13];
			identity[3, 2] = node.Transform[14];
			identity[3, 3] = node.Transform[15];
			DisassemblyPartNode res = new DisassemblyPartNode
			{
				Parent = parent,
				Transform = identity,
				HiddenInAssembly = node.Hidden,
				SuppressedInAssembly = node.Suppressed,
				Children = new List<DisassemblyPartNode>()
			};
			List<AssemblyBody> list2 = node.Bodies.Where((AssemblyBody x) => x.BodyId.HasValue).ToList();
			if (list2.Count == 1 && node.Children.Count == 0)
			{
				res.Part = dictParts[list2.First().BodyId.Value];
				AddUserProperties(res.Part, node.UserProperties);
			}
			else
			{
				foreach (AssemblyBody item8 in node.Bodies.Where((AssemblyBody x) => x.BodyId.HasValue))
				{
					DisassemblyPart part2 = dictParts[item8.BodyId.Value];
					DisassemblyPartNode item = new DisassemblyPartNode
					{
						Part = part2,
						Parent = res,
						Transform = global::WiCAM.Pn4000.BendModel.Base.Matrix4d.Identity,
						HiddenInAssembly = false,
						SuppressedInAssembly = false,
						Children = new List<DisassemblyPartNode>()
					};
					AddUserProperties(part2, node.UserProperties);
					res.Children.Add(item);
				}
			}
			res.Children.AddRange(node.Children.Select((AssemblyNode x) => CreateNode(x, res)));
			return res;
		}
	}

	public void AnalyzeModel(IDoc3d doc, string fileName, out List<DisassemblyPart> disassemblyParts, bool useBackgroundWorker, Action backgroundCompleted, out Thread threadBackgroundAnalyze, Dictionary<string, int> materialImportToPnMatId = null)
	{
		threadBackgroundAnalyze = null;
		if (File.Exists(Path.Combine(this._pathService.FolderCad3d2Pn, "Parts.xml")))
		{
			disassemblyParts = null;
			return;
		}
		this._mainModel = new DisassemblySimpleModel();
		this._mainModel.Load3DModel("", this._spatialAssemblyLoader, this._globals);
		global::WiCAM.Pn4000.BendModel.Base.Matrix4d identity = global::WiCAM.Pn4000.BendModel.Base.Matrix4d.Identity;
		identity *= global::WiCAM.Pn4000.BendModel.Base.Matrix4d.RotationZ(0.7853981852531433);
		identity *= global::WiCAM.Pn4000.BendModel.Base.Matrix4d.RotationX(1.0471975803375244);
		this._screenshotScreen.PrintScreen(this._mainModel.Assembly, Path.Combine(this._pathService.FolderCad3d2Pn, "asm.png"), identity, 10, 800, 800);
		this._partAnalyzer.AnalyzeParts(this._mainModel, doc, fileName, materialImportToPnMatId, out var disassemblyParts2, useBackgroundWorker, this._screenshotScreen);
		disassemblyParts = disassemblyParts2;
	}

	public Assembly GetParts()
	{
		Assembly assembly = new Assembly();
		if (!File.Exists(Path.Combine(this._pathService.FolderCad3d2Pn, "Parts.xml")))
		{
			return assembly;
		}
		global::WiCAM.Pn4000.PartsReader.DataClasses.Assembly assembly2 = ((IPartsReader)new global::WiCAM.Pn4000.PartsReader.PartsReader()).DeserializeAssembly(Path.Combine(this._pathService.FolderCad3d2Pn, "Parts.xml"));
		if (assembly2 == null)
		{
			return assembly;
		}
		assembly.MajorVersion = assembly2.MajorVersion;
		assembly.MinorVersion = assembly2.MinorVersion;
		assembly.RootPartName = assembly2.RootPartName;
		foreach (global::WiCAM.Pn4000.PartsReader.DataClasses.DisassemblyPart disassemblyPart in assembly2.DisassemblyParts)
		{
			assembly.DisassemblyParts.Add(new DisassemblyPart(disassemblyPart));
		}
		Dictionary<int, Model> dictModels = new Dictionary<int, Model>();
		foreach (DisassemblyPart disassemblyPart2 in assembly.DisassemblyParts)
		{
			if (disassemblyPart2.PartInfo.SimulationInstances == null)
			{
				continue;
			}
			foreach (global::WiCAM.Pn4000.BendModel.BendTools.SimulationInstance simulationInstance in disassemblyPart2.PartInfo.SimulationInstances)
			{
				if (simulationInstance.AdditionalParts == null)
				{
					continue;
				}
				foreach (global::WiCAM.Pn4000.BendModel.BendTools.SimulationInstance.AdditionalGeometry additionalPart in simulationInstance.AdditionalParts)
				{
					additionalPart.Model = GetModel(additionalPart.ModelUid);
				}
			}
		}
		return assembly;
		Model GetModel(int id)
		{
			if (!dictModels.ContainsKey(id))
			{
				dictModels.Add(id, this._spatialLoader.LoadSpatialFile(Path.Combine(this._pathService.FolderCad3d2Pn, id + ".txt"), id + ".txt", cleanAndAnalyze: false, ConvertConfig.GetAnalyzeConfig(this._configProvider, this._pathService)));
			}
			return dictModels[id];
		}
	}

	public static bool IsModelForDisassemblyAtFiles()
	{
		if (!Directory.Exists("cad3d2pn"))
		{
			return false;
		}
		return Directory.GetFiles("cad3d2pn", "*.txt", SearchOption.AllDirectories).Count() > 3;
	}

	public Model GetAssemblyGeometry(IGlobals globals)
	{
		if (this._mainModel != null)
		{
			return this._mainModel?.Assembly;
		}
		this._mainModel = new DisassemblySimpleModel();
		this._mainModel.Load3DModel("", this._spatialAssemblyLoader, globals);
		return this._mainModel?.Assembly;
	}

	public DisassemblySimpleModel.StructureNode GetStructureRootNode()
	{
		if (string.IsNullOrEmpty(this._mainModel.StructureRootNode.Name))
		{
			this._mainModel.StructureRootNode.Name = AssemblyAnalysisManagement.GetFileName();
		}
		return this._mainModel.StructureRootNode;
	}

	private static string GetFileName()
	{
		string path = "cad3d2pn\\name.txt";
		if (!File.Exists(path))
		{
			return "ErrorName";
		}
		return Path.GetFileName(File.ReadAllText(path).Trim());
	}
}
