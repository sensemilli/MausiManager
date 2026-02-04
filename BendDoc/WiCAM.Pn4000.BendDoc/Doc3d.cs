using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using BendDataSourceModel;
using WiCAM.Pn4000.BendDoc.Services;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.SpatialDataStructures;
using WiCAM.Pn4000.BendModel.BendExceptions;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.BendModel.BendTools.Validations;
using WiCAM.Pn4000.BendModel.Config;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Serialization;
using WiCAM.Pn4000.BendModel.Services.BendTools.PartAnalyzer;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.BendPP;
using WiCAM.Pn4000.Contracts.BendServices;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.ToolCalculation;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.FingerStopCalculationMediator;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.BendSimulation;
using WiCAM.Pn4000.PN3D.CommonBend;
using WiCAM.Pn4000.PN3D.Converter;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Enums;
using WiCAM.Pn4000.PN3D.Extensions;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.PN3D.Tool;
using WiCAM.Pn4000.PN3D.Tool.Interfaces;
using WiCAM.Pn4000.PN3D.Unfold;
using WiCAM.Pn4000.ToolCalculation.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.BendDoc;

public class Doc3d : IDoc3d, IPnBndDoc, IPnBndDocExt
{
	[CompilerGenerated]
	private sealed class _003C_003CUpdateDoc3VisibleFace_003Eg__GetFaceGroups_007C419_0_003Ed : IEnumerable<FaceGroup>, IEnumerable, IEnumerator<FaceGroup>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private FaceGroup _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private FaceGroup fg;

		public FaceGroup _003C_003E3__fg;

		private HashSet<FaceGroup>.Enumerator _003C_003E7__wrap1;

		FaceGroup IEnumerator<FaceGroup>.Current
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
		public _003C_003CUpdateDoc3VisibleFace_003Eg__GetFaceGroups_007C419_0_003Ed(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			this._003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = this._003C_003E1__state;
			if (num == -3 || num == 2)
			{
				try
				{
				}
				finally
				{
					this._003C_003Em__Finally1();
				}
			}
			this._003C_003E7__wrap1 = default(HashSet<FaceGroup>.Enumerator);
			this._003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (this._003C_003E1__state)
				{
				default:
					return false;
				case 0:
					this._003C_003E1__state = -1;
					this._003C_003E2__current = this.fg;
					this._003C_003E1__state = 1;
					return true;
				case 1:
					this._003C_003E1__state = -1;
					this._003C_003E7__wrap1 = this.fg.SubGroups.GetEnumerator();
					this._003C_003E1__state = -3;
					break;
				case 2:
					this._003C_003E1__state = -3;
					break;
				}
				if (this._003C_003E7__wrap1.MoveNext())
				{
					this._003C_003E2__current = this._003C_003E7__wrap1.Current;
					this._003C_003E1__state = 2;
					return true;
				}
				this._003C_003Em__Finally1();
				this._003C_003E7__wrap1 = default(HashSet<FaceGroup>.Enumerator);
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
			((IDisposable)this._003C_003E7__wrap1/*cast due to .constrained prefix*/).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<FaceGroup> IEnumerable<FaceGroup>.GetEnumerator()
		{
			_003C_003CUpdateDoc3VisibleFace_003Eg__GetFaceGroups_007C419_0_003Ed _003C_003CUpdateDoc3VisibleFace_003Eg__GetFaceGroups_007C419_0_003Ed;
			if (this._003C_003E1__state == -2 && this._003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				this._003C_003E1__state = 0;
				_003C_003CUpdateDoc3VisibleFace_003Eg__GetFaceGroups_007C419_0_003Ed = this;
			}
			else
			{
				_003C_003CUpdateDoc3VisibleFace_003Eg__GetFaceGroups_007C419_0_003Ed = new _003C_003CUpdateDoc3VisibleFace_003Eg__GetFaceGroups_007C419_0_003Ed(0);
			}
			_003C_003CUpdateDoc3VisibleFace_003Eg__GetFaceGroups_007C419_0_003Ed.fg = this._003C_003E3__fg;
			return _003C_003CUpdateDoc3VisibleFace_003Eg__GetFaceGroups_007C419_0_003Ed;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<FaceGroup>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
    private sealed class _003CBendsPerPp_003Ed__453 : IEnumerable<(int, List<ICombinedBendDescriptor>)>, IEnumerable, IEnumerator<(int, List<ICombinedBendDescriptor>)>, IEnumerator, IDisposable
    {
        private int _003C_003E1__state;
        private (int, List<ICombinedBendDescriptor>) _003C_003E2__current;
        private int _003C_003El__initialThreadId;
        public Doc3d _003C_003E4__this;
        private IEnumerable<ICombinedBendDescriptor> bends;
        public IEnumerable<ICombinedBendDescriptor> _003C_003E3__bends;
        private int _003CsubNo_003E5__2;
        private IEnumerator<IGrouping<IToolSetups, IBendPositioning>> _003C_003E7__wrap2;

        (int, List<ICombinedBendDescriptor>) IEnumerator<(int, List<ICombinedBendDescriptor>)>.Current
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
        public _003CBendsPerPp_003Ed__453(int _003C_003E1__state)
        {
            this._003C_003E1__state = _003C_003E1__state;
            this._003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
        }

        [DebuggerHidden]
        void IDisposable.Dispose()
        {
            int num = this._003C_003E1__state;
            if (num == -3 || num == 2)
            {
                try
                {
                }
                finally
                {
                    this._003C_003Em__Finally1();
                }
            }
            this._003C_003E7__wrap2 = null;
            this._003C_003E1__state = -2;
        }

        private bool MoveNext()
        {
            try
            {
                int num = this._003C_003E1__state;
                Doc3d doc3d = this._003C_003E4__this;
                switch (num)
                {
                    default:
                        return false;
                    case 0:
                        {
                            this._003C_003E1__state = -1;
                            Doc3d doc3d2 = doc3d;
                            IPostProcessor postProcessor = doc3d2.BendMachine.PostProcessor;
                            if (postProcessor == null)
                            {
                                return false;
                            }
                            this._003CsubNo_003E5__2 = 1;
                            if (postProcessor.SupportMultipleToolRoots)
                            {
                                this._003C_003E2__current = (this._003CsubNo_003E5__2, this.bends.Where((ICombinedBendDescriptor x) => x.IsIncluded).ToList());
                                this._003C_003E1__state = 1;
                                return true;
                            }
                            this._003C_003E7__wrap2 = (from x in (from x in this.bends.Where((ICombinedBendDescriptor x) => x.IsIncluded).Select(doc3d2.ToolsAndBends.GetBend)
                                                                  where x?.Anchor != null
                                                                  select x).ToList()
                                                       group x by x.Anchor.Root into x
                                                       orderby x.Min((IBendPositioning b) => b.Order)
                                                       select x).GetEnumerator();
                            this._003C_003E1__state = -3;
                            goto IL_020e;
                        }
                    case 1:
                        this._003C_003E1__state = -1;
                        break;
                    case 2:
                        {
                            this._003C_003E1__state = -3;
                            goto IL_020e;
                        }
                    IL_020e:
                        if (this._003C_003E7__wrap2.MoveNext())
                        {
                            this._003C_003E2__current = (this._003CsubNo_003E5__2++, (from x in this._003C_003E7__wrap2.Current
                                                                                      orderby x.Order
                                                                                      select x into b
                                                                                      select b.Bend.CombinedBendDescriptor).ToList());
                            this._003C_003E1__state = 2;
                            return true;
                        }
                        this._003C_003Em__Finally1();
                        this._003C_003E7__wrap2 = null;
                        break;
                }
                return false;
            }
            catch
            {
                ((IDisposable)this).Dispose();
                throw;
            }
        }

        bool IEnumerator.MoveNext()
        {
            return this.MoveNext();
        }

        private void _003C_003Em__Finally1()
        {
            this._003C_003E1__state = -1;
            if (this._003C_003E7__wrap2 != null)
            {
                this._003C_003E7__wrap2.Dispose();
            }
        }

        [DebuggerHidden]
        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        [DebuggerHidden]
        IEnumerator<(int, List<ICombinedBendDescriptor>)> IEnumerable<(int, List<ICombinedBendDescriptor>)>.GetEnumerator()
        {
            _003CBendsPerPp_003Ed__453 _003CBendsPerPp_003Ed__;
            if (this._003C_003E1__state == -2 && this._003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
            {
                this._003C_003E1__state = 0;
                _003CBendsPerPp_003Ed__ = this;
            }
            else
            {
                _003CBendsPerPp_003Ed__ = new _003CBendsPerPp_003Ed__453(0)
                {
                    _003C_003E4__this = this._003C_003E4__this
                };
            }
            _003CBendsPerPp_003Ed__.bends = this._003C_003E3__bends;
            return _003CBendsPerPp_003Ed__;
        }

        [DebuggerHidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<(int, List<ICombinedBendDescriptor>)>)this).GetEnumerator();
        }
    }

	private IPreferredProfileStore? _preferredProfileStore;

	private double _thickness;

	private bool _machineFullyLoaded;

	private string? _machinePath;

	private string? _bendTempFolder;

	internal DocData _data;

	private readonly IScopedFactorio _factorio;

	private readonly IDocSerializer _docSerializer;

	private readonly IGlobals _globals;

	private readonly IMachineBendFactory _machineBendFactory;

	private readonly IPartAnalyzer _partAnalyzer;

	private readonly IModelReconstructor _modelReconstructor;

	private readonly IToolFactory _toolFactory;

	private readonly IConfigProvider _configProvider;

	private readonly IPnPathService _pathService;

	private readonly ITranslator _translator;

	private readonly IMaterialManager _materialManager;

	private readonly IMaterial3dFortran _material3dFortran;

	private readonly IImportMaterialMapper _importMaterialMapper;

	private readonly IToolOperator _toolOperator;

	private readonly IGlobalBendTable _globalBendTable;

	private readonly IInternalDocFactory _docFactory;

	private readonly IAutoMode _autoMode;

	private readonly IUndo3dService _undo3dService;

	private ISimulationThread _bendSimulation;

	private List<ValidationResult> _validationResults;

	private bool _bendsSorted;

	internal Dictionary<Tuple<int, int>, (FaceGroup fg, Model model)> _unfoldModelLookup = new Dictionary<Tuple<int, int>, (FaceGroup, Model)>();

	internal Dictionary<Tuple<int, int>, (FaceGroup fg, Model model)> _modifiedModelLookup = new Dictionary<Tuple<int, int>, (FaceGroup, Model)>();

	internal Dictionary<Tuple<int, int>, (FaceGroup fg, Model model)> _bendModelLookup = new Dictionary<Tuple<int, int>, (FaceGroup, Model)>();

	private bool _hasSimulationCalculated;

	private bool _ncFileGenerated;

	private bool _reportGenerated;

	private IToolManager? _toolManager;

	private IBendMachine? _bendMachine;

	private IToolsAndBends? _toolsAndBends;

	private int _recalcSimDirtyFlag;

	public bool IsAssemblyLoading { get; set; }

	public bool IsDevImport { get; private set; }

	public bool FreezeCombinedBendDescriptors
	{
		get
		{
			return this._data.FreezeCombinedBendDescriptors;
		}
		set
		{
			if (this._data.FreezeCombinedBendDescriptors != value)
			{
				this._data.FreezeCombinedBendDescriptors = value;
				this.FreezeCombinedBendDescriptorsChanged?.Invoke(this);
			}
		}
	}

	public int DocAssemblyId { get; set; }

	public bool KFactorWarningsError
	{
		get
		{
			if (!this.KFactorWarningsAcceptedByUser)
			{
				return this.CombinedBendDescriptors.Any((ICombinedBendDescriptorInternal x) => x.IsIncluded && x[0].BendParams.KFactorAlgorithm == BendTableReturnValues.NO_VALUE_FOUND);
			}
			return false;
		}
	}

	public bool KFactorWarningsAcceptedByUser { get; set; }

	public bool PnMaterialByUser
	{
		get
		{
			return this._data.PnMaterialByUser;
		}
		set
		{
			if (this._data.PnMaterialByUser != value)
			{
				this._data.PnMaterialByUser = value;
				if (!this.IsSerialized)
				{
					this.PnMaterialByUserChanged?.Invoke(this);
				}
			}
		}
	}

	public IPostProcessor? PostProcessor => this.BendMachine?.PostProcessor;

	public IDocMetadata MetaData
	{
		get
		{
			IBendMachine bendMachine = this.BendMachine;
			string bendMachineDesc = "-";
			if (bendMachine != null)
			{
				bendMachineDesc = bendMachine.MachineNo + " - " + bendMachine.Name;
			}
			DocState docState = this.State;
			if (docState == DocState.MachineLoaded)
			{
				if (this.HasSimulationCalculated)
				{
					docState = DocState.FingerCalculated;
				}
				else if (this.HasToolSetups)
				{
					docState = DocState.ToolsCalculated;
				}
			}
			List<IToolProfile> list = (from x in this.ToolsAndBends?.ToolSetups.SelectMany((IToolSetups x) => x.AllSections).SelectMany((IToolSection x) => x.MultiToolProfile.ToolProfiles).Distinct()
				orderby x.Name
				select x).ToList();
			Dictionary<Type, int> macros = (from x in global::WiCAM.Pn4000.BendModel.GeometryTools.ModelExtensions.GetAllShells(this.EntryModel3D, false).SelectMany((Shell s) => s.Macros)
				group x by x.GetType()).ToDictionary((IGrouping<Type, Macro> g) => g.Key, (IGrouping<Type, Macro> g) => g.Count());
			DocMetadata obj = new DocMetadata
			{
				BendCount = this.CombinedBendDescriptors.Count,
				Thickness = this.Thickness,
				Dimension = this.EntryModel3D.PartInfo.GeometryType,
				LenX = this.EntryModel3D.PartInfo.Dimensions.X,
				LenY = this.EntryModel3D.PartInfo.Dimensions.Y,
				LenZ = this.EntryModel3D.PartInfo.Dimensions.Z,
				SheetType = this.EntryModel3D.PartInfo.PartType,
				BendMachineDesc = bendMachineDesc,
				BendMachineNo = this.BendMachine?.MachineNo,
				SavedArchivNo = this.SavedArchiveNumber,
				SavedArchivName = this.SavedFileName,
				DocState = docState,
				ValidationGeoErrors = this.ValidationResults?.Count,
				PPName = string.Join(", ", this.NamesPpBase)
			};
			Model unfoldModel3D = this.UnfoldModel3D;
			obj.CutoutsAdded = ((unfoldModel3D != null) ? new int?((from f in global::WiCAM.Pn4000.BendModel.GeometryTools.ModelExtensions.GetAllFaces(unfoldModel3D, false)
				where f.IsTessellated.HasValue
				select Math.Abs(f.IsTessellated.Value)).Distinct().Count()) : ((int?)null));
			obj.Comment = this.Comment;
			obj.KFactorWarnings = this.KFactorWarningsError;
			obj.PnMaterialByUser = this.PnMaterialByUser;
			obj.PnMaterialNo = this.Material?.Number;
			obj.DocAssemblyId = this.DocAssemblyId;
			obj.UserComments = this.UserComments.ToDictionary<KeyValuePair<int, string>, int, string>((KeyValuePair<int, string> x) => x.Key, (KeyValuePair<int, string> x) => x.Value);
			obj.LenUnfoldX = this.UnfoldModel3D?.PartInfo.Dimensions.X ?? double.NaN;
			obj.LenUnfoldY = this.UnfoldModel3D?.PartInfo.Dimensions.Y ?? double.NaN;
			obj.UseBendAid = this.CombinedBendDescriptors.Any((ICombinedBendDescriptorInternal x) => x.UseLiftingAid);
			obj.UpperToolNames = (from x in list?.Where((IToolProfile x) => x.ProfileType.HasFlag(ToolProfileTypes.UpperTool))
				select x.Name).ToList();
			obj.LowerToolNames = (from x in list?.Where((IToolProfile x) => x.ProfileType.HasFlag(ToolProfileTypes.LowerTool))
				select x.Name).ToList();
			obj.UpperAdapterNames = (from x in list?.Where((IToolProfile x) => x.ProfileType.HasFlag(ToolProfileTypes.UpperAdapter))
				select x.Name).ToList();
			obj.LowerAdapterNames = (from x in list?.Where((IToolProfile x) => x.ProfileType.HasFlag(ToolProfileTypes.LowerAdapter))
				select x.Name).ToList();
			obj.CreationUserName = this.CreationUserName;
			obj.LastModifiedUserName = this.LastModifiedUserName;
			obj.CreationDate = this.CreationDate;
			obj.LastModified = this.LastModified;
			obj.AssemblyGuid = this.AssemblyGuid;
			obj.AssemblyName = this.DiskFile.Header.ImportedFilename;
			obj.MacroBlindHole = GetMacroCount<BlindHole>();
			obj.MacroConicBlindHole = GetMacroCount<ConicBlindHole>();
			obj.MacroSphericalBlindHole = GetMacroCount<SphericalBlindHole>();
			obj.MacroBolt = GetMacroCount<Bolt>();
			obj.MacroBorder = 0;
			obj.MacroBridgeLance = GetMacroCount<BridgeLance>();
			obj.MacroChamfer = GetMacroCount<Chamfer>();
			obj.MacroCounterSink = GetMacroCount<CounterSink>();
			obj.MacroStepDrilling = GetMacroCount<StepDrilling>();
			obj.MacroDeepening = GetMacroCount<Deepening>();
			obj.MacroDummy = GetMacroCount<Dummy>();
			obj.MacroEmbossed = GetMacroCount<Embossed>();
			obj.MacroEmbossedCircle = GetMacroCount<EmbossedCircle>();
			obj.MacroEmbossedCounterSink = GetMacroCount<EmbossedCounterSink>();
			obj.MacroEmbossedFreeform = GetMacroCount<EmbossedFreeform>();
			obj.MacroEmbossedLine = GetMacroCount<EmbossedLine>();
			obj.MacroEmbossedRectangle = GetMacroCount<EmbossedRectangle>();
			obj.MacroEmbossedRectangleRounded = GetMacroCount<EmbossedRectangleRounded>();
			obj.MacroEmbossedSquare = GetMacroCount<EmbossedSquare>();
			obj.MacroEmbossedSquareRounded = GetMacroCount<EmbossedSquareRounded>();
			obj.MacroLance = GetMacroCount<Lance>();
			obj.MacroLouver = GetMacroCount<Louver>();
			obj.MacroManufacturingMacro = GetMacroCount<ManufacturingMacro>();
			obj.MacroPressNut = GetMacroCount<PressNut>();
			obj.MacroSimpleHole = GetMacroCount<SimpleHole>();
			obj.MacroThread = GetMacroCount<Thread>();
			obj.MacroTwoSidedCounterSink = GetMacroCount<TwoSidedCounterSink>();
			return obj;
			int GetMacroCount<T>() where T : notnull, Macro
			{
				return macros.GetValueOrDefault(typeof(T), 0);
			}
		}
	}

	public string Id { get; } = Guid.NewGuid().ToString();

	public string AssemblyGuid
	{
		get
		{
			return this._data.AssemblyGuid;
		}
		set
		{
			this._data.AssemblyGuid = value;
		}
	}

	public string CreationUserName
	{
		get
		{
			return this._data.CreationUserName;
		}
		set
		{
			this._data.CreationUserName = value;
		}
	}

	public string LastModifiedUserName
	{
		get
		{
			return this._data.LastModifiedUserName;
		}
		set
		{
			this._data.LastModifiedUserName = value;
		}
	}

	public DateTime CreationDate
	{
		get
		{
			return this._data.CreationDate;
		}
		set
		{
			this._data.CreationDate = value;
		}
	}

	public DateTime LastModified
	{
		get
		{
			return this._data.LastModified;
		}
		set
		{
			this._data.LastModified = value;
		}
	}

	public bool UseDINUnfold
	{
		get
		{
			return this._data.UseDINUnfold;
		}
		set
		{
			if (this._data.UseDINUnfold != value)
			{
				this._data.UseDINUnfold = value;
				if (this.State >= DocState.CombinedDescriptorsCalculated)
				{
					this.UpdateDoc();
				}
			}
		}
	}

	public int MaterialNumber
	{
		get
		{
			return this.Material?.Number ?? (-1);
		}
		set
		{
			this.Material = this._materialManager.MaterialList.FirstOrDefault((IMaterialArt m) => m.Number == value);
		}
	}

	public IMaterialArt Material
	{
		get
		{
			return this._data.Material;
		}
		set
		{
			if (value != null && (value.EModul == 0.0 || value.TensileStrength == 0.0 || value.YieldStrength == 0.0))
			{
				this.MessageDisplay.ShowTranslatedErrorMessage("Material.InvalidBendProperties", value.Number, value.Name);
				return;
			}
			this._data.Material = value;
			if (value != null)
			{
				if (!this.IsAssemblyLoading)
				{
					this._material3dFortran.SetActiveMaterial(value.Number);
				}
				this._data.MaterialSet = true;
			}
			if (this.State > DocState.CombinedDescriptorsCalculated && !this.IsSerialized)
			{
				this.UpdateDoc();
			}
		}
	}

	public Model InputModel3D
	{
		get
		{
			return this._data.InputModel3D;
		}
		set
		{
			if (this._data.InputModel3D == value)
			{
				return;
			}
			Model inputModel3D = this._data.InputModel3D;
			this._data.InputModel3D = value;
			if (!this._data.MaterialSet)
			{
				int matId = this._importMaterialMapper.GetMaterialId(value.OriginalMaterialName);
				if (!this.IsAssemblyLoading)
				{
					if (matId > 0)
					{
						this._material3dFortran.SetActiveMaterial(matId);
						this._data.Material = this._material3dFortran.GetActiveMaterial(isAssembly: false);
					}
					else
					{
						this._material3dFortran.SetActiveMaterial(matId);
						this._data.Material = null;
					}
				}
				else
				{
					this._data.Material = this._materialManager.MaterialList.FirstOrDefault((IMaterialArt m) => m.Number == matId) ?? this._materialManager.MaterialList.FirstOrDefault();
				}
			}
			this.EntryModel3DChanged?.Invoke(inputModel3D, this._data.InputModel3D);
		}
	}

	public Model ReconstructedEntryModel
	{
		get
		{
			return this._data.ReconstructedEntryModel ?? this._data.EntryModel3D ?? this._data.InputModel3D;
		}
		set
		{
			if (this._data.ReconstructedEntryModel != value)
			{
				Model reconstructedEntryModel = this._data.ReconstructedEntryModel;
				this._data.ReconstructedEntryModel = value;
				this.EntryModel3DChanged?.Invoke(reconstructedEntryModel, this._data.ReconstructedEntryModel);
			}
		}
	}

	public Model EntryModel3D
	{
		get
		{
			return this._data.EntryModel3D ?? this._data.InputModel3D;
		}
		set
		{
			if (this._data.EntryModel3D != value)
			{
				Model entryModel3D = this.EntryModel3D;
				this._data.EntryModel3D = value;
				this.EntryModel3DChanged?.Invoke(entryModel3D, this.EntryModel3D);
			}
		}
	}

	public Model ModifiedEntryModel3D => this._data.ModifiedEntryModel3D;

	public Model UnfoldModel3D => this._data.UnfoldModel3D;

	public Model BendModel3D => this._data.BendModel3D;

	public Model View3DModel
	{
		get
		{
			return this._data.View3DModel;
		}
		set
		{
			if (this._data.View3DModel != value)
			{
				Model view3DModel = this._data.View3DModel;
				this._data.View3DModel = value;
				this.View3DModelChanged?.Invoke(view3DModel, this._data.View3DModel);
			}
		}
	}

	public List<SimulationInstance> SimulationInstancesAdditionalParts { get; set; }

	public SimulationInstance? CurrentSimulationInstancesAdditionalPart => this.SimulationInstancesAdditionalParts?.FirstOrDefault();

	public IReadOnlyList<IBendDescriptor> BendDescriptors => this._data.BendDescriptors.AsReadOnly();

	public IReadOnlyList<ICombinedBendDescriptorInternal> CombinedBendDescriptors => this._data.CombinedBendDescriptors.AsReadOnly();

	IBendMachineSimulationBasic? IPnBndDoc.BendMachineConfig => this.BendMachineConfig;

	[Obsolete]
	public IBendMachineSimulation? BendMachineConfig
	{
		get
		{
			return this._data.BendMachineConfig;
		}
		set
		{
			this._toolManager = null;
			this._bendMachine = null;
			_ = this._data.BendMachineConfig;
			this.BendSimulation?.Pause();
			if (!this.IsSerialized)
			{
				this.NamePPBase = null;
				this.NumberPp = null;
			}
			this.HasBendStepsCalculated = false;
			this.HasSimulationCalculated = false;
			this.MachineFullyLoaded = false;
			this._data.BendMachineConfig = value;
			this.MachineFullyLoaded = false;
			if (!this.FreezeCombinedBendDescriptors)
			{
				this._bendsSorted = false;
			}
			if (this.IsSerialized)
			{
				this._bendsSorted = true;
			}
			else
			{
				this.PreferredProfileStore.Clear();
				foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in this.CombinedBendDescriptors)
				{
					combinedBendDescriptor.ResetTools();
					combinedBendDescriptor.ResetMachineSpecificData();
				}
				this.ResetTools();
			}
			if (this.State >= DocState.CombinedDescriptorsCalculated)
			{
				foreach (ICombinedBendDescriptorInternal combinedBendDescriptor2 in this.CombinedBendDescriptors)
				{
					combinedBendDescriptor2.ResetMachineSpecificData();
				}
				value.RestorePrefferedTools();
				this.UpdateDoc();
			}
			this.BendMachineChanged?.Invoke();
		}
	}

	public IToolExpert Tools
	{
		get
		{
			return this._data.Tools;
		}
		set
		{
			if (this._data.Tools != value)
			{
				_ = this._data.Tools;
				this._data.Tools = value;
			}
		}
	}

	public bool ToolCalculationEnabled { get; set; }

	[Obsolete]
	public IPreferredProfileStore PreferredProfileStore => this._preferredProfileStore ?? (this._preferredProfileStore = this._factorio.Resolve<IPreferredProfileStore>());

	public ISimulationThread BendSimulation
	{
		get
		{
			return this._bendSimulation;
		}
		set
		{
			ISimulationThread bendSimulation = this._bendSimulation;
			this._bendSimulation = value;
			bendSimulation?.Pause();
			this.BendSimulationChanged?.Invoke(bendSimulation, this._bendSimulation);
			this.NcFileGenerated = false;
			this.ReportGenerated = false;
			this.NcDataSend = false;
		}
	}

	public PnBndFile DiskFile { get; set; }

	public double Thickness
	{
		get
		{
			return this._thickness;
		}
		set
		{
			this._thickness = Math.Round(value, 10);
		}
	}

	public int VisibleFaceGroupId { get; set; }

	public int VisibleFaceGroupSide { get; set; }

	public bool VisibleFaceIsTopFace => this.UnfoldModel3D.GetFaceGroupById(this.VisibleFaceGroupId).IsSide0Top == (this.VisibleFaceGroupSide == 0);

	public bool IsReconstructed => this._data.ReconstructedEntryModel != null;

	public bool SimulationValidated
	{
		get
		{
			ISimulationThread bendSimulation = this.BendSimulation;
			if (bendSimulation == null)
			{
				return false;
			}
			return bendSimulation.State?.SimulationCollisionManager?.IsValidated() == true;
		}
	}

	public bool NcFileGenerated
	{
		get
		{
			return this._ncFileGenerated;
		}
		set
		{
			if (this._ncFileGenerated != value)
			{
				this._ncFileGenerated = value;
				this.NcDataSend = false;
			}
		}
	}

	public bool ReportGenerated
	{
		get
		{
			return this._reportGenerated;
		}
		set
		{
			if (this._reportGenerated != value)
			{
				this._reportGenerated = value;
				this.NcDataSend = false;
			}
		}
	}

	public bool NcDataSend { get; set; }

	public List<ValidationResult> ValidationResults
	{
		get
		{
			return this._validationResults;
		}
		set
		{
			if (this._validationResults != value)
			{
				this._validationResults = value;
				this.ValidationResultChanged?.Invoke(this, this._validationResults);
			}
		}
	}

	public string ImportedFilename => this.DiskFile.Header.ImportedFilename;

	public string SavedFileName { get; set; }

	public int SavedArchiveNumber { get; set; } = -1;

	public bool MachinePartiallyLoadedForUnfold => this._bendMachine != null;

	public bool MachineFullyLoaded
	{
		get
		{
			return this._machineFullyLoaded;
		}
		set
		{
			this._machineFullyLoaded = value;
			if (this._machineFullyLoaded)
			{
				if (this.State == DocState.BendModelCreated)
				{
					this.State = DocState.MachineLoaded;
				}
				else if (this.State > DocState.BendModelCreated)
				{
					this.State = DocState.BendModelCreated;
				}
			}
		}
	}

	public string? MachinePath
	{
		get
		{
			return this._machinePath;
		}
		set
		{
			if (this._machinePath != value)
			{
				this._machinePath = value;
				this._bendMachine = null;
				this.MachineFullyLoaded = false;
			}
		}
	}

	public bool IsSerialized { get; set; }

	public bool HasModel => this.EntryModel3D?.Shell != null;

	public bool HasRadiusChangeErrors { get; private set; }

	public bool HasToolSetups
	{
		get
		{
			IToolsAndBends? toolsAndBends = this.ToolsAndBends;
			if (toolsAndBends == null)
			{
				return false;
			}
			return toolsAndBends.ToolSetups.Count > 0;
		}
	}

	public bool HasFingers => this.HasAnyFingers;

	public bool HasAnyFingers
	{
		get
		{
			if (this.CombinedBendDescriptors.Any())
			{
				return this.CombinedBendDescriptors.Where((ICombinedBendDescriptorInternal x) => x.IsIncluded && x.BendType != CombinedBendType.HemBend).Any((ICombinedBendDescriptorInternal x) => x.SelectedStopPointLeft != null && x.SelectedStopPointRight != null);
			}
			return false;
		}
	}

	public bool HasAllFingers
	{
		get
		{
			if (this.CombinedBendDescriptors.Any())
			{
				return this.CombinedBendDescriptors.Where((ICombinedBendDescriptorInternal x) => x.IsIncluded && x.BendType != CombinedBendType.HemBend).All((ICombinedBendDescriptorInternal x) => x.SelectedStopPointLeft != null && x.SelectedStopPointRight != null);
			}
			return false;
		}
	}

	public bool HasBendStepsCalculated { get; set; }

	public bool HasSimulationCalculated
	{
		get
		{
			if (this._hasSimulationCalculated)
			{
				return this.HasFingers;
			}
			return false;
		}
		set
		{
			this._hasSimulationCalculated = value;
		}
	}

	public BdsmDataModel PpModel { get; set; }

	public int AmountInAssembly
	{
		get
		{
			return this._data.AmountInAssembly;
		}
		set
		{
			this._data.AmountInAssembly = value;
		}
	}

	public string NamePP
	{
		get
		{
			throw new NotImplementedException("Doc3d.NamePP is obsolete");
		}
	}

	public string Comment
	{
		get
		{
			return this._data.Comment;
		}
		set
		{
			this._data.Comment = value;
		}
	}

	public string Classification
	{
		get
		{
			return this._data.Classification;
		}
		set
		{
			this._data.Classification = value;
		}
	}

	public string DrawingNumber
	{
		get
		{
			return this._data.DrawingNumber;
		}
		set
		{
			this._data.DrawingNumber = value;
		}
	}

	public string NamePPBase
	{
		get
		{
			return this._data.NamePP;
		}
		set
		{
			this._data.NamePP = value;
		}
	}

	public IReadOnlyList<string> NamesPpBase => this._data.NamesPpBase;

	public string NamePPSuffix
	{
		get
		{
			bool num;
			if (this.NamePpTimestamps != 0)
			{
				num = this.NamePpTimestamps == SetNcTimestampTypes.AddTimestamp;
			}
			else
			{
				IBendMachine? bendMachine = this.BendMachine;
				if (bendMachine == null)
				{
					goto IL_0042;
				}
				num = bendMachine.PressBrakeData.AddDateTimePPName == 1;
			}
			if (num)
			{
				return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
			}
			goto IL_0042;
			IL_0042:
			return this._data.NamePPSuffix;
		}
		set
		{
			this._data.NamePPSuffix = value;
		}
	}

	public int? NumberPp
	{
		get
		{
			return this._data.NumberPp;
		}
		set
		{
			this._data.NumberPp = value;
		}
	}

	public SetNcTimestampTypes NamePpTimestamps
	{
		get
		{
			return this._data.NamePpTimestamps;
		}
		set
		{
			this._data.NamePpTimestamps = value;
		}
	}

	public Dictionary<int, string> UserComments { get; set; } = new Dictionary<int, string>();

	public Assembly Assembly { get; set; }

	public IMessageLogDoc MessageDisplay => this.FrontCalls?.MessageDisplay as IMessageLogDoc;

	public IFrontCalls FrontCalls { get; private set; }

	public string BendTempFolder => this._bendTempFolder ?? (this._bendTempFolder = Path.Combine(this._pathService.GetUserFilePath("bendTemp"), Guid.NewGuid().ToString()));

	public IScopedFactorio Factorio => this._factorio;

	public IConfigProvider ConfigProvider => this._configProvider;

	public DocState State { get; private set; }

	public List<UserProperty> UserProperties { get; set; } = new List<UserProperty>();

	public IToolManager? ToolManager => this.BendMachine?.ToolConfig?.ToolManager;

	public bool HasBendMachine => this._bendMachine != null;

	public IBendMachine? BendMachine
	{
		get
		{
			if (this._bendMachine == null)
			{
				if (!string.IsNullOrEmpty(this.MachinePath))
				{
					this._bendMachine = this._machineBendFactory.CreateMachine(this);
				}
				if (this._bendMachine == null)
				{
					this.MachineFullyLoaded = false;
					return null;
				}
				IToolsAndBends? toolsAndBends = this.ToolsAndBends;
				if (toolsAndBends != null && toolsAndBends.ToolSetups.Count > 0)
				{
					ImmutableArray<IToolSetups>.Enumerator enumerator = this.ToolsAndBends.ToolSetups.ToImmutableArray().GetEnumerator();
					while (enumerator.MoveNext())
					{
						IToolSetups current = enumerator.Current;
						IToolSetups toolSetups = this._toolFactory.CreateToolSetup(this.ToolsAndBends, this._bendMachine.ToolConfig);
						toolSetups.Desc = current.Desc;
						toolSetups.Number = current.Number;
						ImmutableArray<IToolCluster>.Enumerator enumerator2 = current.Children.ToImmutableArray().GetEnumerator();
						while (enumerator2.MoveNext())
						{
							IToolCluster current2 = enumerator2.Current;
							current2.Parent = toolSetups;
							toolSetups.Children.Add(current2);
							current.Children.Remove(current2);
						}
						ImmutableArray<IToolSection>.Enumerator enumerator3 = current.Sections.ToImmutableArray().GetEnumerator();
						while (enumerator3.MoveNext())
						{
							IToolSection current3 = enumerator3.Current;
							current3.Cluster = toolSetups;
							toolSetups.Sections.Add(current3);
							current.Sections.Remove(current3);
						}
						foreach (IBendPositioning bendPosition in this.ToolsAndBends.BendPositions)
						{
							if (bendPosition.Anchor == current)
							{
								bendPosition.Anchor = toolSetups;
							}
						}
						this._toolOperator.CenterTools(toolSetups);
						this._toolFactory.RemoveToolSetup(this.ToolsAndBends, current);
					}
				}
				this.MachineFullyLoaded = true;
				if (!this.FreezeCombinedBendDescriptors)
				{
					this._bendsSorted = false;
				}
				this.ToolSelectionType = ToolSelectionType.PreferredTools;
				foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in this.CombinedBendDescriptors)
				{
					combinedBendDescriptor.ResetMachineSpecificData();
				}
				if (!this.IsSerialized)
				{
					this.UpdateDoc();
				}
				this.BendMachineChanged?.Invoke();
			}
			return this._bendMachine;
		}
	}

	public IToolsAndBends? ToolsAndBends
	{
		get
		{
			return this._toolsAndBends;
		}
		set
		{
			if (this._toolsAndBends != value)
			{
				IToolsAndBends toolsAndBends = this._toolsAndBends;
				this._toolsAndBends = value;
				if (!this.TrySetCombinedBendDescriptors(toolsAndBends, this._toolsAndBends, this.CombinedBendDescriptors))
				{
					throw new NotImplementedException();
				}
				this.ToolsAndBendsChanged?.Invoke(toolsAndBends, this._toolsAndBends);
			}
		}
	}

	public ToolSelectionType ToolSelectionType { get; set; }

	public bool SafeModeUnfold { get; set; }

	public List<int> SafeModeUnfoldErrorBendOrder { get; set; } = new List<int>();

	public bool ZeroBordersAdded2D { get; set; }

	public event Action<IDoc3d> FreezeCombinedBendDescriptorsChanged;

	public event Action<IDoc3d> PnMaterialByUserChanged;

	public event Action<IPnBndDoc> Closed;

	public event Action<Model, Model> EntryModel3DChanged;

	public event Action<Model, Model> ModifiedEntryModel3DChanged;

	public event Action<Model, Model> UnfoldModel3DChanged;

	public event Action<Model, Model> BendModel3DChanged;

	public event Action<Model, Model> View3DModelChanged;

	public event Action<ISimulationThread, ISimulationThread> BendSimulationChanged;

	public event Action BendMachineChanged;

	public event Action CombinedBendDescriptorsChanged;

	public event Action<IDoc3d, List<ValidationResult>> ValidationResultChanged;

	public event Action<IDoc3d> UpdateBendDataInfoEvent;

	public event Action<IDoc3d> DocUpdated;

	public event Action<IToolsAndBends, IToolsAndBends> ToolsAndBendsChanged;

	public event Action<IPnBndDoc> FingerChanged;

	public event Action<IPnBndDoc> RefreshSimulation;

	public event Action<IDoc3d> UpdateGeneralInfoAutoEvent;

	public event Action<IDoc3d, ModelViewMode> UpdateGeneralInfoEvent;

	public event Action<IDoc3d> UpdateBendMachineInfoEvent;

	public Doc3d(IScopedFactorio factorio, IDocSerializer docSerializer, IGlobals globals, IMachineBendFactory machineBendFactory, 
		IPartAnalyzer partAnalyzer, IModelReconstructor modelReconstructor, IToolFactory toolFactory,
		IConfigProvider configProvider, IPnPathService pathService, ITranslator translator, IMaterialManager materialManager,
		IMaterial3dFortran material3dFortran, IImportMaterialMapper importMaterialMapper, IToolOperator toolOperator,
		IGlobalBendTable globalBendTable, IInternalDocFactory docFactory, IUndo3dService undo3dService)
	{
		this._factorio = factorio;
		this._docSerializer = docSerializer;
		this._globals = globals;
		this._machineBendFactory = machineBendFactory;
		this._partAnalyzer = partAnalyzer;
		this._modelReconstructor = modelReconstructor;
		this._toolFactory = toolFactory;
		this._configProvider = configProvider;
		this._pathService = pathService;
		this._translator = translator;
		this._materialManager = materialManager;
		this._material3dFortran = material3dFortran;
		this._importMaterialMapper = importMaterialMapper;
		this._toolOperator = toolOperator;
		this._globalBendTable = globalBendTable;
		this._docFactory = docFactory;
		this._undo3dService = undo3dService;
		this._autoMode = this._factorio.Resolve<IAutoMode>();
	}

	public void Init(string filename, bool isAssemblyLoading = false, string? assemblyGuid = null, bool isDevImport = false)
	{
		this.IsAssemblyLoading = isAssemblyLoading;
		this.IsDevImport = isDevImport;
		if (string.IsNullOrEmpty(filename))
		{
			filename = "Unknown";
		}
		filename = Path.GetFileName(filename);
		this._data = new DocData();
		if (string.IsNullOrEmpty(assemblyGuid))
		{
			this.AssemblyGuid = Guid.NewGuid().ToString();
		}
		else
		{
			this.AssemblyGuid = assemblyGuid;
		}
		this.FrontCalls = this._globals.FallbackFrontCalls.CreateDocInstance(filename.Replace(".", "_"));
		this.HasSimulationCalculated = false;
		this.HasBendStepsCalculated = false;
		this.ToolCalculationEnabled = false;
		this.DiskFile = new PnBndFile();
		BendToolExpert bendToolExpert = new BendToolExpert(this._globals);
		bendToolExpert.Init(this);
		this.Tools = bendToolExpert;
		this.VisibleFaceGroupId = -1;
		this.VisibleFaceGroupSide = -1;
		this.EntryModel3DChanged += OnEntryModel3DChanged;
		this.CombinedBendDescriptorsChanged += UpdateBends;
		this.UseDINUnfold = this._configProvider.InjectOrCreate<General3DConfig>().P3D_UseDINUnfold;
	}

	public void Close()
	{
		this.Closed?.Invoke(this);
	}

	public void OnToolsChanged()
	{
		if (this.State >= DocState.BendModelCreated && this.MachinePartiallyLoadedForUnfold && this.HasModel)
		{
			this.UpdateDoc();
		}
	}

	public void OnBendDataChanged()
	{
		if (this.State >= DocState.BendModelCreated && this.HasModel)
		{
			this.UpdateBendDataInfoEvent?.Invoke(this);
		}
	}

	public void UpdateBends()
	{
		this._toolFactory.UpdateCbds(this.ToolsAndBends, this.CombinedBendDescriptors);
	}

	public void ResetTools()
	{
		this.ResetFingers();
		this.CreateToolsAndBends();
	}

	public void CreateToolsAndBends()
	{
		if (this.ReconstructedEntryModel.PartInfo.PartType != PartType.Unassigned && !this.ReconstructedEntryModel.PartInfo.PartType.HasFlag(PartType.Tube) && !this.ReconstructedEntryModel.PartInfo.PartType.HasFlag(PartType.Error))
		{
			this.ToolsAndBends = this._toolFactory.CreateToolsAndBends(this.CombinedBendDescriptors, this.BendMachine);
		}
		else
		{
			this.ToolsAndBends = null;
		}
	}

	public void ResetFingers()
	{
		foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in this.CombinedBendDescriptors)
		{
			combinedBendDescriptor.ResetStopPoints();
		}
		this.FingerChanged?.Invoke(this);
	}

	public void ResetLiftingAids()
	{
	}

	public void SetNamesPpBase(IEnumerable<string> subPpNames)
	{
		this._data.NamesPpBase = subPpNames.Reverse().SkipWhile(string.IsNullOrEmpty).Reverse()
			.ToList();
		this.ResetBendTempFolder();
		this.UpdateGeneralInfo();
	}

	private void ResetBendTempFolder()
	{
		string bendTempFolder = this.BendTempFolder;
		if (Directory.Exists(bendTempFolder))
		{
			Directory.Delete(bendTempFolder, recursive: true);
		}
	}

	public void SetFingerPos(ICombinedBendDescriptorInternal? cbd, IFingerStopPointInternal leftFingerPos, IFingerStopPointInternal rightFingerPos, FingerPositioningMode mode)
	{
		cbd.SelectedStopPointLeft = leftFingerPos;
		cbd.SelectedStopPointRight = rightFingerPos;
		cbd.FingerPositioningMode = mode;
		this.FingerChanged?.Invoke(this);
	}

	private void OnEntryModel3DChanged(Model modelOld, Model modelNew)
	{
		this.State = DocState.EntryModelSet;
		this._undo3dService.Reset(this);
		this._data.BendDescriptors.Clear();
		this._data.CombinedBendDescriptors.Clear();
		this._toolsAndBends = null;
		this.Thickness = this.ReconstructedEntryModel.Shell?.Thickness ?? (-1.0);
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		foreach (FaceGroup allRoundFaceGroup in modelNew.GetAllRoundFaceGroups())
		{
			if (!this.ReconstructedEntryModel.PartInfo.PartType.HasFlag(PartType.Tube) && general3DConfig.P3D_MinStepBendRadius <= allRoundFaceGroup.ConcaveAxis.Radius && allRoundFaceGroup.ConcaveAxis.Radius < general3DConfig.P3D_MaxStepBendRadius)
			{
				int num = allRoundFaceGroup.SubBendCount ?? Math.Max(3, (int)Math.Ceiling(allRoundFaceGroup.ConvexAxis.OpeningAngle * allRoundFaceGroup.ConcaveAxis.Radius / (general3DConfig.P3D_StepBendWithoutMachineMinBendLengthFactor * this.Thickness)));
				double radius = allRoundFaceGroup.StepBendRadius ?? (general3DConfig.P3D_StepBendWithoutMachineRadiusFactor * this.Thickness);
				for (int i = 0; i < num; i++)
				{
					BendDescriptor item = new BendDescriptor(BendingType.StepBend, new StepBendParameters(allRoundFaceGroup, i, radius, this));
					this._data.BendDescriptors.Add(item);
				}
			}
			else
			{
				BendDescriptor item2 = new BendDescriptor(BendingType.SimpleBend, new SimpleBendParameters(allRoundFaceGroup, this));
				this._data.BendDescriptors.Add(item2);
			}
		}
		this.State = DocState.BendDescriptorsCalculated;
		this.UpdateDoc();
	}

	public bool ReconstructFromFace(Face entryFace, Model entryFaceModel)
	{
		Model reconstructedEntryModel = this._data.ReconstructedEntryModel;
		this._data.ReconstructedEntryModel = this._modelReconstructor.AnalyzePartReconstructive(entryFace.Shell, entryFaceModel, this.Thickness, out var usedFirstFaceGroupId, out var usedFirstFaceGroupSide, entryFace);
		if (this._data.ReconstructedEntryModel != null)
		{
			this.VisibleFaceGroupId = usedFirstFaceGroupId;
			this.VisibleFaceGroupSide = usedFirstFaceGroupSide;
			this.EntryModel3DChanged?.Invoke(reconstructedEntryModel, this._data.ReconstructedEntryModel);
			return true;
		}
		return false;
	}

	public F2exeReturnCode ReconstructIrregularBends(bool experimental = false)
	{
		global::WiCAM.Pn4000.BendModel.Config.AnalyzeConfig analyzeConfig = ConvertConfig.GetAnalyzeConfig(this._factorio.Resolve<IConfigProvider>(), this._factorio.Resolve<IPnPathService>());
		analyzeConfig.ReconstructBendsConfig.Enabled = true;
		analyzeConfig.ReconstructBendsConfig.UseExperimentalRepairer = experimental;
		Model model = this.InputModel3D.Copy();
		this.ClearData();
		model.Transform = Matrix4d.Identity;
		this._partAnalyzer.AnalyzeParts(model, analyzeConfig);
		this.EntryModel3D = model;
		this._undo3dService.Save(this, this._translator.Translate("Undo3d.Reconstruct"));
		if (this.InputModel3D.PartInfo.PartType.HasFlag(PartType.Unknown))
		{
			return F2exeReturnCode.ERROR_INVALID_GEOMETRY;
		}
		if (this.HasRadiusChangeErrors)
		{
			return F2exeReturnCode.ERROR_RADIUS_ADJUSTMENT_NOT_POSSIBLE;
		}
		return F2exeReturnCode.OK;
	}

	public F2exeReturnCode ExperimentalRepairBends()
	{
		return F2exeReturnCode.OK;
	}

	private void ClearData()
	{
		this._data.BendDescriptors.Clear();
		this._data.CombinedBendDescriptors.Clear();
		this._data.ReconstructedEntryModel = null;
		this._data.ModifiedEntryModel3D = new Model();
		this._data.UnfoldModel3D = new Model();
		this._data.BendModel3D = new Model();
		BendToolExpert bendToolExpert = new BendToolExpert(this._globals);
		bendToolExpert.Init(this);
		this._data.Tools = bendToolExpert;
		this.ToolsAndBends = null;
		this._bendsSorted = false;
		this.VisibleFaceGroupId = -1;
		this.VisibleFaceGroupSide = -1;
	}

	public bool SetTopFace(Model model, Face face, Model faceModel)
	{
		if (face == null)
		{
			return false;
		}
		if (model == this._data.EntryModel3D)
		{
			this.VisibleFaceGroupId = face.FaceGroup.ID;
			if (face.FaceGroup.Side0.Contains(face))
			{
				this.VisibleFaceGroupSide = 0;
				this.MoveVisibleFaceToZero(model, face, faceModel);
				return true;
			}
			if (face.FaceGroup.Side1.Contains(face))
			{
				this.VisibleFaceGroupSide = 1;
				this.MoveVisibleFaceToZero(model, face, faceModel);
				return true;
			}
		}
		else if (model == this._data.UnfoldModel3D || model == this._data.ModifiedEntryModel3D || model == this._data.BendModel3D)
		{
			this.VisibleFaceGroupId = face.FaceGroup.BendEntryId;
			if (face.FaceGroup.Side0.Contains(face))
			{
				this.VisibleFaceGroupSide = 0;
				if (model == this._data.UnfoldModel3D)
				{
					this.CalculateBendAngleSign();
				}
				this.MoveVisibleFaceToZero(model, face, faceModel);
				return true;
			}
			if (face.FaceGroup.Side1.Contains(face))
			{
				this.VisibleFaceGroupSide = 1;
				if (model == this._data.UnfoldModel3D)
				{
					this.CalculateBendAngleSign();
				}
				this.MoveVisibleFaceToZero(model, face, faceModel);
				return true;
			}
		}
		this.MoveVisibleFaceToZero(model, face, faceModel);
		return false;
	}

	private void MoveVisibleFaceToZero(Model model, Face face, Model faceModel)
	{
		Unfold.MoveUnfoldModelToZero(model, face, faceModel);
	}

	public void CombinedBendsAutoSort(IBendSequenceOrder strategy)
	{
		using (Activity.Current?.Source.StartActivity("Doc - CombinedBendsAutoSort"))
		{
			if (strategy == null)
			{
				strategy = this.BendMachine?.ToolCalculationSettings.BendOrderStrategies.FirstOrDefault();
			}
			ISequenceGrouping sequenceGrouping = strategy?.Groupings?.FirstOrDefault();
			CombinedBendsSorter.SortBends(strategy?.Sequences?.ToList() ?? this.BendMachine?.ToolCalculationSettings.BendOrderStrategies?.FirstOrDefault()?.Sequences ?? new List<BendSequenceSorts>
			{
				BendSequenceSorts.CommonBendsFirst,
				BendSequenceSorts.InToOutMainFace,
				BendSequenceSorts.LongToShortWithoutGaps
			}, this._data.CombinedBendDescriptors, sequenceGrouping != null && sequenceGrouping.GroupingType == 1, sequenceGrouping?.InnerSortSequences);
			this.CombinedBendDescriptorsChanged?.Invoke();
		}
	}

	public void CombinedBendsAutoSortBruteforce(int bruteforceIndex)
	{
		CombinedBendsSorter.SortBendsBruteforce(this._data.CombinedBendDescriptors, bruteforceIndex);
		this.CombinedBendDescriptorsChanged?.Invoke();
	}

	public bool MoveBend(int bendIndexOld, int bendIndexNew)
	{
		List<CombinedBendDescriptor> combinedBendDescriptors = this._data.CombinedBendDescriptors;
		int num = combinedBendDescriptors.Count - 1;
		if (bendIndexOld < 0 || bendIndexNew < 0 || bendIndexNew > num || bendIndexOld > num)
		{
			return false;
		}
		if (bendIndexOld == bendIndexNew)
		{
			return true;
		}
		List<ICombinedBendDescriptorInternal> list = new List<ICombinedBendDescriptorInternal>(combinedBendDescriptors) { [bendIndexNew] = combinedBendDescriptors[bendIndexOld] };
		if (bendIndexOld < bendIndexNew)
		{
			for (int i = bendIndexOld; i < bendIndexNew; i++)
			{
				list[i] = combinedBendDescriptors[i + 1];
			}
		}
		else
		{
			for (int j = bendIndexNew + 1; j <= bendIndexOld; j++)
			{
				list[j] = combinedBendDescriptors[j - 1];
			}
		}
		return this.ApplyBendOrder(list);
	}

	public bool ApplyBendOrder(IReadOnlyList<ICombinedBendDescriptorInternal> order, bool autoCombine = true)
	{
		if (!this.CombinedBendDescriptors.ToHashSet().SetEquals(order))
		{
			return false;
		}
		int idx;
		for (idx = 0; idx < order.Count; idx++)
		{
			IReadOnlyList<ICombinedBendDescriptor> splitPredecessors = order[idx].SplitPredecessors;
			if (splitPredecessors != null && splitPredecessors.Any((ICombinedBendDescriptor x) => order.Skip(idx + 1).Any((ICombinedBendDescriptorInternal y) => x == y)))
			{
				return false;
			}
		}
		List<int> list = order.Select((ICombinedBendDescriptorInternal x) => x.Enumerable.Count()).ToList();
		for (int i = 0; i < order.Count; i++)
		{
			((CombinedBendDescriptor)order[i]).Order = i;
		}
		this._data.CombinedBendDescriptors.Sort((CombinedBendDescriptor a, CombinedBendDescriptor b) => a.Order.CompareTo(b.Order));
		foreach (CombinedBendDescriptor combinedBendDescriptor in this._data.CombinedBendDescriptors)
		{
			combinedBendDescriptor.UnfoldBendInUnfoldModel(1.0, relative: false, noGeometryChange: true);
		}
		this.SplitCombinedBends();
		if (autoCombine)
		{
			int num = 0;
			int num2 = this._data.CombinedBendDescriptors.Count - 1;
			for (int j = 0; j < num2; j++)
			{
				if (!this.TryMergeWithNext(this._data.CombinedBendDescriptors[num]))
				{
					num++;
				}
			}
		}
		else
		{
			int num3 = 0;
			for (int k = 0; k < list.Count; k++)
			{
				for (int l = 0; l < list[k] - 1; l++)
				{
					if (!this.TryMergeWithNext(this._data.CombinedBendDescriptors[k + num3]))
					{
						num3++;
					}
				}
			}
		}
		this.AssignNewOrder();
		return true;
	}

	private void AssignNewOrder()
	{
		foreach (CombinedBendDescriptor combinedBendDescriptor2 in this._data.CombinedBendDescriptors)
		{
			combinedBendDescriptor2.UnfoldBendInUnfoldModel(1.0, relative: false, noGeometryChange: true);
		}
		for (int i = 0; i < this._data.CombinedBendDescriptors.Count; i++)
		{
			CombinedBendDescriptor combinedBendDescriptor = this._data.CombinedBendDescriptors[i];
			combinedBendDescriptor.Order = i;
			if (combinedBendDescriptor.ToolPresenceVector == null)
			{
				combinedBendDescriptor.ToolSetupId = null;
				combinedBendDescriptor.LowerAdapterSetId = null;
				combinedBendDescriptor.LowerToolSetId = null;
				combinedBendDescriptor.UpperAdapterSetId = null;
				combinedBendDescriptor.UpperToolSetId = null;
			}
		}
		this.BendSimulation?.State?.SimulationCollisionManager?.Reset();
		this.CombinedBendDescriptorsChanged?.Invoke();
	}

	public List<List<ICombinedBendDescriptorInternal>> GroupCompatibleBends(int step)
	{
		for (int i = 0; i < this._data.CombinedBendDescriptors.Count; i++)
		{
			this._data.CombinedBendDescriptors[i].UnfoldBendInUnfoldModel(1.0, relative: false, noGeometryChange: true);
		}
		for (int j = 0; j < step; j++)
		{
			this._data.CombinedBendDescriptors[j].UnfoldBendInUnfoldModel(0.0, relative: true, noGeometryChange: true);
		}
		List<List<ICombinedBendDescriptorInternal>> result = CombinedBendDescriptorHelper.ComputePotentialBendGroups(this.CombinedBendDescriptors.Skip(step).ToList());
		for (int k = 0; k < this._data.CombinedBendDescriptors.Count; k++)
		{
			this._data.CombinedBendDescriptors[k].UnfoldBendInUnfoldModel(1.0, relative: false, noGeometryChange: true);
		}
		return result;
	}

	public bool TryMergeWithNext(ICombinedBendDescriptorInternal cbdRef)
	{
		int num = this._data.CombinedBendDescriptors.IndexOf(cbdRef);
		if (num == this._data.CombinedBendDescriptors.Count - 1)
		{
			return false;
		}
		int index = num + 1;
		CombinedBendDescriptor combinedBendDescriptor = this._data.CombinedBendDescriptors[num];
		CombinedBendDescriptor combinedBendDescriptor2 = this._data.CombinedBendDescriptors[index];
		if (!this.CanMergeWithNext(combinedBendDescriptor))
		{
			return false;
		}
		combinedBendDescriptor.Merge(combinedBendDescriptor2);
		this._data.CombinedBendDescriptors.Remove(combinedBendDescriptor2);
		this.AssignNewOrder();
		return true;
	}

	public bool CanMergeWithNext(ICombinedBendDescriptorInternal cbd)
	{
		int num = this._data.CombinedBendDescriptors.IndexOf(cbd);
		if (num == this._data.CombinedBendDescriptors.Count - 1)
		{
			return false;
		}
		CombinedBendDescriptor other = this._data.CombinedBendDescriptors[num + 1];
		for (int i = 0; i < this._data.CombinedBendDescriptors.Count; i++)
		{
			this._data.CombinedBendDescriptors[i].UnfoldBendInUnfoldModel(1.0, relative: false, noGeometryChange: true);
		}
		for (int j = 0; j < num; j++)
		{
			this._data.CombinedBendDescriptors[j].UnfoldBendInUnfoldModel(0.0, relative: true, noGeometryChange: true);
		}
		bool result = cbd.IsCompatibleBendUnfoldModel(other);
		for (int k = 0; k < this._data.CombinedBendDescriptors.Count; k++)
		{
			this._data.CombinedBendDescriptors[k].UnfoldBendInUnfoldModel(1.0, relative: false, noGeometryChange: true);
		}
		return result;
	}

	public bool ApplyBendSequence(IPnBndDoc doc3dSource)
	{
		if (doc3dSource is Doc3d doc3d && doc3d != this)
		{
			Dictionary<IBendDescriptor, (int fgEntryId, int? subBendCount, int? subBendIndex)> sourceBd = doc3d.BendDescriptors.ToDictionary((IBendDescriptor x) => x, (IBendDescriptor x) => ((int fgEntryId, int? subBendCount, int? subBendIndex))FaceGroupExtensions.ToFaceGroupIdentifier(x.BendParams.BendFaceGroup));
			Dictionary<(int fgEntryId, int? subBendCount, int? subBendIndex), IBendDescriptor> thisBd = this.BendDescriptors.ToDictionary((IBendDescriptor x) => ((int fgEntryId, int? subBendCount, int? subBendIndex))FaceGroupExtensions.ToFaceGroupIdentifier(x.BendParams.BendFaceGroup));
			if (!sourceBd.Values.ToHashSet().SetEquals(thisBd.Keys))
			{
				return false;
			}
			Dictionary<CombinedBendDescriptor, CombinedBendDescriptor> dictCbd = new Dictionary<CombinedBendDescriptor, CombinedBendDescriptor>();
			this._data.CombinedBendDescriptors.Clear();
			this._data.CombinedBendDescriptors.AddRange(doc3d._data.CombinedBendDescriptors.Select(delegate(CombinedBendDescriptor tmp)
			{
				CombinedBendDescriptor combinedBendDescriptor = new CombinedBendDescriptor(bendDescriptors: tmp.Enumerable.Select((IBendDescriptor x) => thisBd[sourceBd[x]]).ToList(), cbd: tmp, bendType: tmp.BendType, doc: this)
				{
					SplitPredecessors = tmp.SplitPredecessors?.Select((CombinedBendDescriptor o) => dictCbd[o]).ToList()
				};
				dictCbd.Add(tmp, combinedBendDescriptor);
				return combinedBendDescriptor;
			}));
			this.CombinedBendDescriptorsChanged?.Invoke();
			return true;
		}
		return false;
	}

	public bool SplitBend(int bendIndex, double splitValue)
	{
		CombinedBendDescriptor combinedBendDescriptor = this._data.CombinedBendDescriptors[bendIndex];
		CombinedBendDescriptor combinedBendDescriptor2 = new CombinedBendDescriptor(combinedBendDescriptor, combinedBendDescriptor.BendType);
		combinedBendDescriptor2.ProgressStart = (combinedBendDescriptor.ProgressStop = (combinedBendDescriptor.ProgressStart + combinedBendDescriptor.ProgressStop) * splitValue);
		combinedBendDescriptor2.SplitPredecessors = new List<CombinedBendDescriptor> { combinedBendDescriptor };
		foreach (CombinedBendDescriptor splitSuccessor in combinedBendDescriptor.SplitSuccessors)
		{
			splitSuccessor.SplitPredecessors.Remove(combinedBendDescriptor);
			splitSuccessor.SplitPredecessors.Add(combinedBendDescriptor2);
			splitSuccessor.SplitPredecessors = splitSuccessor.SplitPredecessors.Distinct().ToList();
		}
		this._data.CombinedBendDescriptors.Insert(bendIndex + 1, combinedBendDescriptor2);
		CombinedBendDescriptorHelper.SetOrder(this._data.CombinedBendDescriptors);
		this.CombinedBendDescriptorsChanged?.Invoke();
		return true;
	}

	public bool MergeSplitBends(List<int> bendIndices)
	{
		if (!this.CanSplitBendsMerge(bendIndices))
		{
			return false;
		}
		List<CombinedBendDescriptor> list = (from x in bendIndices
			orderby x
			select x into idx
			select this._data.CombinedBendDescriptors[idx]).ToList();
		double progressStart = list.Max((CombinedBendDescriptor x) => x.ProgressStart);
		double progressStop = list.Min((CombinedBendDescriptor x) => x.ProgressStop);
		int index = bendIndices.Min();
		CombinedBendDescriptor item = new CombinedBendDescriptor(list.First())
		{
			ProgressStart = progressStart,
			ProgressStop = progressStop,
			SplitPredecessors = list.First().SplitPredecessors
		};
		foreach (CombinedBendDescriptor splitSuccessor in list.Last().SplitSuccessors)
		{
			splitSuccessor.SplitPredecessors.Remove(list.Last());
			splitSuccessor.SplitPredecessors.Add(item);
			splitSuccessor.SplitPredecessors = splitSuccessor.SplitPredecessors.Distinct().ToList();
		}
		foreach (CombinedBendDescriptor item2 in list)
		{
			this._data.CombinedBendDescriptors.Remove(item2);
		}
		this._data.CombinedBendDescriptors.Insert(index, item);
		CombinedBendDescriptorHelper.SetOrder(this._data.CombinedBendDescriptors);
		this.CombinedBendDescriptorsChanged?.Invoke();
		return true;
	}

	public bool CanSplitBendsMerge(List<int> bendIndices)
	{
		List<CombinedBendDescriptor> list = (from idx in bendIndices.OrderBy((int x) => x).ToList()
			select this._data.CombinedBendDescriptors[idx]).ToList();
		for (int i = 0; i < list.Count - 1; i++)
		{
			CombinedBendDescriptor combinedBendDescriptor = list[i];
			CombinedBendDescriptor combinedBendDescriptor2 = list[i + 1];
			List<CombinedBendDescriptor> splitPredecessors = combinedBendDescriptor2.SplitPredecessors;
			if (splitPredecessors == null || !splitPredecessors.Contains(combinedBendDescriptor))
			{
				return false;
			}
			if (combinedBendDescriptor.Enumerable.Except(combinedBendDescriptor2.Enumerable).Any())
			{
				return false;
			}
		}
		return true;
	}

	public bool ChangeSplitValue(int bendIndex, double splitValue)
	{
		CombinedBendDescriptor cbd0 = this._data.CombinedBendDescriptors[bendIndex];
		cbd0.ProgressStop = splitValue;
		foreach (CombinedBendDescriptor item in this._data.CombinedBendDescriptors.Where((CombinedBendDescriptor x) => x.SplitPredecessors?.Contains(cbd0) ?? false))
		{
			item.ProgressStart = splitValue;
		}
		this.CombinedBendDescriptorsChanged?.Invoke();
		return true;
	}

	public bool ChangeManualRadius(int bendIndex, double radius)
	{
		CombinedBendDescriptor combinedBendDescriptor = this._data.CombinedBendDescriptors[bendIndex];
		if (Math.Abs(combinedBendDescriptor[0].BendParams.FinalRadius - radius) < 1E-06)
		{
			return true;
		}
		foreach (BendDescriptor item in combinedBendDescriptor.BendsInternal)
		{
			item.BendParamsInternal.ManualRadius = radius;
			if (this.PreferredProfileStore.IsEmpty())
			{
				this.PreferredProfileStore.AssignPreferredProfileToCommonBend(null, item.BendParams.EntryFaceGroup.ID, item.BendParams.UnfoldFaceGroup.SubBendIndex.GetValueOrDefault(), ToolSelectionType.NoTools);
			}
		}
		if (this.PreferredProfileStore.IsEmpty())
		{
			IPreferredProfile toolGroupsForBend = this.Tools.GetToolGroupsForBend(combinedBendDescriptor);
			combinedBendDescriptor.ToolSelectionAlgorithm = ToolSelectionType.NoTools;
			foreach (BendDescriptor item2 in combinedBendDescriptor.BendsInternal)
			{
				this.PreferredProfileStore.AssignPreferredProfileToCommonBend(toolGroupsForBend, item2.BendParams.EntryFaceGroup.ID, item2.BendParams.UnfoldFaceGroup.SubBendIndex.GetValueOrDefault(), ToolSelectionType.NoTools);
			}
		}
		if (this.State >= DocState.UnfoldDone)
		{
			this.UpdateDoc();
		}
		return true;
	}

	public bool ChangeManualRadius(List<int> bendIndex, double radius)
	{
		bool flag = false;
		foreach (int item in bendIndex)
		{
			CombinedBendDescriptor combinedBendDescriptor = this._data.CombinedBendDescriptors[item];
			if (Math.Abs(combinedBendDescriptor[0].BendParams.FinalRadius - radius) < 1E-06)
			{
				continue;
			}
			foreach (BendDescriptor item2 in combinedBendDescriptor.BendsInternal)
			{
				item2.BendParamsInternal.ManualRadius = radius;
				if (this.PreferredProfileStore.IsEmpty())
				{
					this.PreferredProfileStore.AssignPreferredProfileToCommonBend(null, item2.BendParams.EntryFaceGroup.ID, item2.BendParams.UnfoldFaceGroup.SubBendIndex.GetValueOrDefault(), ToolSelectionType.NoTools);
				}
			}
			if (this.PreferredProfileStore.IsEmpty())
			{
				IPreferredProfile toolGroupsForBend = this.Tools.GetToolGroupsForBend(combinedBendDescriptor);
				combinedBendDescriptor.ToolSelectionAlgorithm = ToolSelectionType.NoTools;
				foreach (BendDescriptor item3 in combinedBendDescriptor.BendsInternal)
				{
					this.PreferredProfileStore.AssignPreferredProfileToCommonBend(toolGroupsForBend, item3.BendParams.EntryFaceGroup.ID, item3.BendParams.UnfoldFaceGroup.SubBendIndex.GetValueOrDefault(), ToolSelectionType.NoTools);
				}
			}
			flag = true;
		}
		if (flag && this.State >= DocState.UnfoldDone)
		{
			this.UpdateDoc();
		}
		return true;
	}

	public bool ChangeManualBendDeduction(int bendIndex, double bd)
	{
		return this.ChangeManualBendDeduction(new List<int> { bendIndex }, bd);
	}

	public bool ChangeManualBendDeduction(List<int> bendIndex, double bd)
	{
		bool flag = false;
		foreach (int item in bendIndex)
		{
			CombinedBendDescriptor combinedBendDescriptor = this._data.CombinedBendDescriptors[item];
			if (Math.Abs(combinedBendDescriptor[0].BendParams.FinalBendDeduction - bd) < 1E-06)
			{
				continue;
			}
			foreach (BendDescriptor item2 in combinedBendDescriptor.BendsInternal)
			{
				item2.BendParamsInternal.ManualBendDeduction = bd;
				item2.BendParamsInternal.KFactorAlgorithm = BendTableReturnValues.USER_DEFINED;
			}
			flag = true;
		}
		if (flag && this.State >= DocState.UnfoldDone)
		{
			this.UpdateDoc();
		}
		return true;
	}

	public bool ConvertBendToStepBend(int bendIndex)
	{
		if (bendIndex < 0)
		{
			return false;
		}
		HashSet<FaceGroup> entryGroups = this._data.CombinedBendDescriptors[bendIndex].Enumerable.Select((IBendDescriptor x) => x.BendParams.EntryFaceGroup).Distinct().ToHashSet();
		List<BendDescriptor> list = this._data.BendDescriptors.Where((BendDescriptor x) => entryGroups.Contains(x.BendParams.EntryFaceGroup)).ToList();
		CombinedBendDescriptor item = this._data.CombinedBendDescriptors[bendIndex];
		List<BendDescriptor> list2 = this._data.BendDescriptors.Where(list.Contains).ToList();
		List<BendDescriptor> list3 = new List<BendDescriptor>();
		this._data.CombinedBendDescriptors.Remove(item);
		this._data.BendDescriptors.RemoveAll(list2.Contains);
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		int num = Math.Max(3, (int)Math.Ceiling(entryGroups.First().ConvexAxis.OpeningAngle * entryGroups.First().ConcaveAxis.Radius / (general3DConfig.P3D_StepBendWithoutMachineMinBendLengthFactor * this.Thickness)));
		double radius = general3DConfig.P3D_StepBendWithoutMachineRadiusFactor * this.Thickness;
		List<List<BendDescriptor>> list4 = new List<List<BendDescriptor>>();
		for (int i = 0; i < num; i++)
		{
			list4.Add(new List<BendDescriptor>());
		}
		foreach (FaceGroup item3 in entryGroups)
		{
			for (int j = 0; j < num; j++)
			{
				BendDescriptor item2 = new BendDescriptor(BendingType.StepBend, new StepBendParameters(item3, j, radius, this));
				list4[j].Add(item2);
				list3.Add(item2);
				this._data.BendDescriptors.Add(item2);
			}
		}
		try
		{
			this.CreateUnfoldModel();
		}
		catch (UnfoldException)
		{
			this._data.BendDescriptors.RemoveAll(list3.Contains);
			this._data.BendDescriptors.AddRange(list2);
			this._data.CombinedBendDescriptors.Insert(bendIndex, item);
			this.CreateUnfoldModel();
			return false;
		}
		int num2 = 0;
		foreach (List<BendDescriptor> item4 in list4)
		{
			this._data.CombinedBendDescriptors.Insert(bendIndex + num2, new CombinedBendDescriptor(item4, this));
			num2++;
		}
		CombinedBendDescriptorHelper.SetOrder(this._data.CombinedBendDescriptors);
		this.UpdateDoc();
		this.CombinedBendDescriptorsChanged?.Invoke();
		return true;
	}

	public bool ConvertStepBendToBend(int bendIndex)
	{
		this.ConvertStepBendToBend2(this._data.CombinedBendDescriptors[bendIndex]);
		CombinedBendDescriptorHelper.SetOrder(this._data.CombinedBendDescriptors);
		this.UpdateDoc();
		this.CombinedBendDescriptorsChanged?.Invoke();
		return true;
	}

	private void ConvertStepBendToBend2(CombinedBendDescriptor cbd)
	{
		HashSet<FaceGroup> entryGroups = cbd.Enumerable.Select((IBendDescriptor x) => x.BendParams.EntryFaceGroup).Distinct().ToHashSet();
		List<BendDescriptor> list = this._data.BendDescriptors.Where((BendDescriptor x) => entryGroups.Contains(x.BendParams.EntryFaceGroup)).ToList();
		this._data.CombinedBendDescriptors.RemoveAll((CombinedBendDescriptor x) => x != cbd && x.BendsInternal.Any((BendDescriptor bd) => entryGroups.Contains(bd.BendParams.EntryFaceGroup)));
		this._data.BendDescriptors.RemoveAll(list.Contains);
		cbd.BendsInternal.Clear();
		cbd.ResetMachineSpecificData();
		foreach (FaceGroup item2 in entryGroups)
		{
			BendDescriptor item = new BendDescriptor(BendingType.SimpleBend, new SimpleBendParameters(item2, this));
			cbd.BendsInternal.Add(item);
			this._data.BendDescriptors.Add(item);
			item2.StepBendRadius = null;
			item2.SubBendCount = null;
		}
	}

	public bool ChangeStepBendProperties(int bendIndex, int numSteps, double radius, double? bendDeductionMiddle, double? bendDeductionStartEnd)
	{
		HashSet<FaceGroup> entryGroups = this._data.CombinedBendDescriptors[bendIndex].Enumerable.Select((IBendDescriptor x) => x.BendParams.EntryFaceGroup).Distinct().ToHashSet();
		List<BendDescriptor> bends = this._data.BendDescriptors.Where((BendDescriptor x) => entryGroups.Contains(x.BendParams.EntryFaceGroup)).ToList();
		int curNumSteps = bends.Max((BendDescriptor x) => x.BendParams.UnfoldFaceGroup.SubBendIndex.GetValueOrDefault());
		List<BendDescriptor> list = new List<BendDescriptor>();
		List<BendDescriptor> list2 = new List<BendDescriptor>();
		if (curNumSteps != numSteps)
		{
			List<CombinedBendDescriptor> list3 = this._data.CombinedBendDescriptors.Where((CombinedBendDescriptor x) => bends.Any((BendDescriptor y) => x.Enumerable.Contains(y))).ToList();
			this._data.CombinedBendDescriptors.RemoveAll(list3.Contains);
			this._data.BendDescriptors.RemoveAll(bends.Contains);
			bends.Clear();
			foreach (FaceGroup item2 in entryGroups)
			{
				for (int i = 0; i < numSteps; i++)
				{
					BendDescriptor item = new BendDescriptor(BendingType.StepBend, new StepBendParameters(item2, i, radius, this));
					this._data.BendDescriptors.Add(item);
					if (i == 0 || i == numSteps - 1)
					{
						list2.Add(item);
					}
					else
					{
						list.Add(item);
					}
				}
			}
			curNumSteps = numSteps;
		}
		else
		{
			list = bends.Where(delegate(BendDescriptor x)
			{
				FaceGroup unfoldFaceGroup = x.BendParams.UnfoldFaceGroup;
				return unfoldFaceGroup.SubBendIndex > 0 && curNumSteps < unfoldFaceGroup.SubBendIndex;
			}).ToList();
			list2 = bends.Where(delegate(BendDescriptor x)
			{
				FaceGroup unfoldFaceGroup2 = x.BendParams.UnfoldFaceGroup;
				return unfoldFaceGroup2.SubBendIndex == 0 || curNumSteps == unfoldFaceGroup2.SubBendIndex;
			}).ToList();
		}
		foreach (BendDescriptor item3 in list)
		{
			if (Math.Abs(item3.BendParamsInternal.FinalRadius - radius) < 1E-06)
			{
				item3.BendParamsInternal.ManualRadius = radius;
			}
			if (bendDeductionMiddle.HasValue && Math.Abs(item3.BendParamsInternal.FinalBendDeduction - bendDeductionMiddle.Value) > 1E-06)
			{
				item3.BendParamsInternal.ManualBendDeduction = bendDeductionMiddle.Value;
			}
		}
		foreach (BendDescriptor item4 in list2)
		{
			if (Math.Abs(item4.BendParamsInternal.FinalRadius - radius) < 1E-06)
			{
				item4.BendParamsInternal.ManualRadius = radius;
			}
			if (bendDeductionMiddle.HasValue && Math.Abs(item4.BendParamsInternal.FinalBendDeduction - bendDeductionStartEnd.Value) > 1E-06)
			{
				item4.BendParamsInternal.ManualBendDeduction = bendDeductionStartEnd.Value;
			}
		}
		this._data.CombinedBendDescriptors.Clear();
		this.UpdateDoc();
		this.ApplyBendOrder(this._data.CombinedBendDescriptors);
		this.CombinedBendDescriptorsChanged?.Invoke();
		return true;
	}

	public bool MergeCombinedBendsInUnfoldModel(int bendIdx0, int bendIdx1)
	{
		return this.MergeCombinedBends(bendIdx0, bendIdx1, (ICombinedBendDescriptorInternal cbd0, ICombinedBendDescriptorInternal cbd1) => cbd0.IsCompatibleBendUnfoldModel(cbd1));
	}

	public bool MergeCombinedBendsInBendModel(int bendIdx0, int bendIdx1)
	{
		return this.MergeCombinedBends(bendIdx0, bendIdx1, (ICombinedBendDescriptorInternal cbd0, ICombinedBendDescriptorInternal cbd1) => cbd0.IsCompatibleBendBendModel(cbd1));
	}

	private bool MergeCombinedBends(int bendIdx0, int bendIdx1, Func<ICombinedBendDescriptorInternal, ICombinedBendDescriptorInternal, bool> isCompatibleFunc)
	{
		CombinedBendDescriptor combinedBendDescriptor = this._data.CombinedBendDescriptors[bendIdx0];
		CombinedBendDescriptor combinedBendDescriptor2 = this._data.CombinedBendDescriptors[bendIdx1];
		if (!isCompatibleFunc(combinedBendDescriptor, combinedBendDescriptor2))
		{
			return false;
		}
		combinedBendDescriptor.BendsInternal.AddRange(combinedBendDescriptor2.BendsInternal);
		if (combinedBendDescriptor2.SplitPredecessors != null)
		{
			CombinedBendDescriptor combinedBendDescriptor3 = combinedBendDescriptor;
			if (combinedBendDescriptor3.SplitPredecessors == null)
			{
				List<CombinedBendDescriptor> list2 = (combinedBendDescriptor3.SplitPredecessors = new List<CombinedBendDescriptor>());
			}
			combinedBendDescriptor.SplitPredecessors.AddRange(combinedBendDescriptor2.SplitPredecessors);
			combinedBendDescriptor.SplitPredecessors = combinedBendDescriptor.SplitPredecessors.Distinct().ToList();
		}
		foreach (CombinedBendDescriptor splitSuccessor in combinedBendDescriptor2.SplitSuccessors)
		{
			splitSuccessor.SplitPredecessors.Add(combinedBendDescriptor);
			splitSuccessor.SplitPredecessors.Remove(combinedBendDescriptor2);
			splitSuccessor.SplitPredecessors = splitSuccessor.SplitPredecessors.Distinct().ToList();
		}
		combinedBendDescriptor.UpdatePositioningInfo();
		this._data.CombinedBendDescriptors.Remove(combinedBendDescriptor2);
		CombinedBendDescriptorHelper.SetOrder(this._data.CombinedBendDescriptors);
		this.CombinedBendDescriptorsChanged?.Invoke();
		return true;
	}

	public bool SplitCombinedBendsUnfoldModel(int bendIdx0, int splitIndex)
	{
		return this.SplitCombinedBends(bendIdx0, new List<int> { splitIndex }, update: true);
	}

	public bool SplitCombinedBendsUnfoldModel(int bendIdx0, List<int> splitIndices)
	{
		return this.SplitCombinedBends(bendIdx0, splitIndices, update: true);
	}

	public bool SplitCombinedBends(int bendIdx0, int splitIndex)
	{
		return this.SplitCombinedBends(bendIdx0, new List<int> { splitIndex }, update: true);
	}

	public bool SplitCombinedBendsBendModel(int bendIdx0, List<int> splitIndices)
	{
		return this.SplitCombinedBends(bendIdx0, splitIndices, update: true);
	}

	public bool SplitCombinedBends()
	{
		List<CombinedBendDescriptor> list = this._data.CombinedBendDescriptors.Where((CombinedBendDescriptor x) => x.Enumerable.Count() > 1).ToList();
		foreach (CombinedBendDescriptor item in list)
		{
			int i = 0;
			this.SplitCombinedBends(this._data.CombinedBendDescriptors.IndexOf(item), item.Enumerable.Select((IBendDescriptor x) => i++).Skip(1).ToList(), item == list.Last());
		}
		return true;
	}

	public bool SplitCombinedBends(int bendIdx0, List<int> splitIndices, bool update)
	{
		if (splitIndices.Any())
		{
			int num = 0;
			int num2 = 0;
			CombinedBendDescriptor combinedBendDescriptor = this._data.CombinedBendDescriptors[bendIdx0];
			List<CombinedBendDescriptor> splitSuccessors = combinedBendDescriptor.SplitSuccessors;
			foreach (int splitIndex in splitIndices)
			{
				CombinedBendDescriptor cbd = new CombinedBendDescriptor(combinedBendDescriptor.BendsInternal.Skip(num).Take(splitIndex - num).ToList(), this, combinedBendDescriptor.BendType);
				combinedBendDescriptor.CopyMembers(cbd);
				if (combinedBendDescriptor.SplitPredecessors != null)
				{
					List<CombinedBendDescriptor> list = combinedBendDescriptor.SplitPredecessors.Where((CombinedBendDescriptor x) => cbd.Enumerable.Intersect(x.Enumerable).Count() > 0).ToList();
					cbd.SplitPredecessors = (list.Any() ? list : null);
				}
				foreach (CombinedBendDescriptor item in splitSuccessors)
				{
					if (item.Enumerable.Intersect(cbd.Enumerable).Any())
					{
						item.SplitPredecessors.Add(cbd);
						item.SplitPredecessors = item.SplitPredecessors.Distinct().ToList();
					}
				}
				this._data.CombinedBendDescriptors.Insert(bendIdx0 + num2, cbd);
				num = splitIndex;
				num2++;
			}
			CombinedBendDescriptor cbd2 = new CombinedBendDescriptor(combinedBendDescriptor.BendsInternal.Skip(num).Take(combinedBendDescriptor.BendsInternal.Count - num).ToList(), this, combinedBendDescriptor.BendType);
			combinedBendDescriptor.CopyMembers(cbd2);
			if (combinedBendDescriptor.SplitPredecessors != null)
			{
				List<CombinedBendDescriptor> list2 = combinedBendDescriptor.SplitPredecessors.Where((CombinedBendDescriptor x) => cbd2.Enumerable.Intersect(x.Enumerable).Count() > 0).ToList();
				cbd2.SplitPredecessors = (list2.Any() ? list2 : null);
			}
			foreach (CombinedBendDescriptor item2 in splitSuccessors)
			{
				item2.SplitPredecessors.Remove(combinedBendDescriptor);
				if (item2.Enumerable.Intersect(cbd2.Enumerable).Any())
				{
					item2.SplitPredecessors.Add(cbd2);
					item2.SplitPredecessors = item2.SplitPredecessors.Distinct().ToList();
				}
			}
			this._data.CombinedBendDescriptors.Insert(bendIdx0 + num2, cbd2);
			this._data.CombinedBendDescriptors.Remove(combinedBendDescriptor);
			if (update)
			{
				CombinedBendDescriptorHelper.SetOrder(this._data.CombinedBendDescriptors);
				this.CombinedBendDescriptorsChanged?.Invoke();
			}
		}
		return true;
	}

	public void SetPartInsertionDirectionForBend(int bendIdx, MachinePartInsertionDirection dir)
	{
		this._data.CombinedBendDescriptors[bendIdx].MachinePartInsertionDirection = dir;
		this.DisableSimulationCalculated();
	}

	public void DisableSimulationCalculated()
	{
		this._hasSimulationCalculated = false;
		this.CombinedBendDescriptorsChanged?.Invoke();
	}

	public void SetUserDefinedTool(int bendIdx, int? punchId, int? dieId, IPreferredProfile newPP)
	{
		CombinedBendDescriptor combinedBendDescriptor = this._data.CombinedBendDescriptors[bendIdx];
		foreach (IBendDescriptor item in combinedBendDescriptor.Enumerable)
		{
			this.PreferredProfileStore.AssignPreferredProfileToCommonBend(newPP, item.BendParams.EntryFaceGroup.ID, item.BendParams.UnfoldFaceGroup.SubBendIndex.GetValueOrDefault(), ToolSelectionType.UserSelectedTools);
		}
		combinedBendDescriptor.UserForcedUpperToolProfile = punchId;
		combinedBendDescriptor.UserForcedLowerToolProfile = dieId;
		combinedBendDescriptor.ToolSelectionAlgorithm = ToolSelectionType.UserSelectedTools;
	}

	private bool TrySetCombinedBendDescriptors(IToolsAndBends? old, IToolsAndBends? newToolsAndBends, IReadOnlyList<ICombinedBendDescriptorInternal> combinedBendDescriptors)
	{
		if (newToolsAndBends == null || newToolsAndBends.BendPositions.All((IBendPositioning x) => x.Bend.CombinedBendDescriptor != null))
		{
			return true;
		}
		if (old == null)
		{
			return false;
		}
		Dictionary<(int fgEntryId, int? subBendCount, int? subBendIndex), IBendDescriptor> bendDescriptors = this.BendDescriptors.ToDictionary((IBendDescriptor x) => ((int fgEntryId, int? subBendCount, int? subBendIndex))FaceGroupExtensions.ToFaceGroupIdentifier(x.BendParams.UnfoldFaceGroup));
		List<CombinedBendDescriptor> list = new List<CombinedBendDescriptor>();
		foreach (IBendPositioning item in newToolsAndBends.BendPositions.OrderBy((IBendPositioning x) => x.Order))
		{
			IBend bend = item.Bend;
			if (!combinedBendDescriptors.Any(bend.TrySetCombinedBendDescriptor))
			{
				CombinedBendDescriptor combinedBendDescriptor = new CombinedBendDescriptor(bend.FaceGroupIdentifiers.Select(((int bendEntryId, int? subBendCount, int? subBendIndex) x) => bendDescriptors[x]).OfType<BendDescriptor>().ToList(), this, item.Bend.BendType);
				combinedBendDescriptor.UpdatePositioningInfo();
				bend.SetCombinedBendDescriptor(combinedBendDescriptor);
			}
			CombinedBendDescriptor combinedBendDescriptor2 = bend.CombinedBendDescriptor as CombinedBendDescriptor;
			combinedBendDescriptor2.Order = item.Order;
			combinedBendDescriptor2.ProgressStart = 1.0 - bend.StartAngle / combinedBendDescriptor2.Enumerable.First().BendParams.AngleAbs;
			combinedBendDescriptor2.ProgressStop = 1.0 - bend.DestAngle / combinedBendDescriptor2.Enumerable.First().BendParams.AngleAbs;
			combinedBendDescriptor2.MachinePartInsertionDirection = item.MachineInsertDirection;
			list.Add(combinedBendDescriptor2);
		}
		this._data.CombinedBendDescriptors.Clear();
		this._data.CombinedBendDescriptors.AddRange(list);
		this.CombinedBendDescriptorsChanged?.Invoke();
		return true;
	}

	public bool UpdateDoc()
	{
		//IL_0220: Expected O, but got Unknown
		if (this.ReconstructedEntryModel.PartInfo.PartType == PartType.Unassigned || this.ReconstructedEntryModel.PartInfo.PartType.HasFlag(PartType.Error) || this.IsDevImport)
		{
			return false;
		}
		using (Activity.Current?.Source.StartActivity("UpdateDoc"))
		{
			Model unfoldModel3D = this._data.UnfoldModel3D;
			Model modifiedEntryModel3D = this._data.ModifiedEntryModel3D;
			Model bendModel3D = this._data.BendModel3D;
			General3DConfig pnGeneral3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
			this.UpdateDoc1CheckMaterial();
			while (true)
			{
				try
				{
					this.CreateUnfoldModel();
					if (!this.UpdateDoc3VisibleFace())
					{
						return false;
					}
					this.UpdateDoc4CreateCombinedBends();
					this.UpdateDoc5CalculateValuesForBends();
					this.State = DocState.CombinedDescriptorsCalculated;
					Dictionary<FaceGroup, List<RadiusChangeProblem>> radiusChangeProblems;
					List<string> kFactorProblems = this.UpdateDoc6UnfoldWithNewRadiiTools(pnGeneral3DConfig, out radiusChangeProblems);
					using (Activity.Current?.Source.StartActivity("UpdateDoc - Update positioning infos works only after unfold"))
					{
						foreach (CombinedBendDescriptor combinedBendDescriptor3 in this._data.CombinedBendDescriptors)
						{
							combinedBendDescriptor3.PositioningInfo.UpdateBendOffsets(combinedBendDescriptor3.BendsInternal.Select((BendDescriptor x) => x.BendParams.UnfoldFaceGroup).ToList(), this.UnfoldModel3D);
						}
					}
					using (Activity.Current?.Source.StartActivity("UpdateDoc - Move flatten model to z plane"))
					{
						(Face face, Model model) firstFaceOfGroupModel = this.UnfoldModel3D.GetFirstFaceOfGroupModel(this.VisibleFaceGroupId, this.VisibleFaceGroupSide);
						this.SetTopFace(face: firstFaceOfGroupModel.face, faceModel: firstFaceOfGroupModel.model, model: this.UnfoldModel3D);
					}
					this.State = DocState.UnfoldDone;
					if (!this._bendsSorted)
					{
						if (!this.IsSerialized)
						{
							this.CombinedBendsAutoSort(null);
						}
						this._bendsSorted = true;
					}
					this.UpdateDoc6ReportErrorsInChangeRadius(radiusChangeProblems, kFactorProblems, pnGeneral3DConfig);
					this.State = DocState.CombinedBendsSorted;
					this.UpdateDoc7CreateBendModifiedModels();
				}
				catch (UnfoldException val)
				{
					UnfoldException e = val;
					if (this.SafeModeUnfold)
					{
						throw e;
					}
					this.SafeModeUnfold = true;
					this.SafeModeUnfoldErrorBendOrder.Clear();
					if (this._data.CombinedBendDescriptors.Count == 0)
					{
						ImmutableArray<BendDescriptor>.Enumerator enumerator2 = this._data.BendDescriptors.ToImmutableArray().GetEnumerator();
						while (enumerator2.MoveNext())
						{
							BendDescriptor current2 = enumerator2.Current;
							if (current2.BendParams.IsStepBend && current2.BendParams is StepBendParameters stepBendParameters)
							{
								if (stepBendParameters.StepIndex == 0)
								{
									FaceGroup entryFaceGroup = stepBendParameters.EntryFaceGroup;
									BendDescriptor item3 = new BendDescriptor(BendingType.SimpleBend, new SimpleBendParameters(entryFaceGroup, this));
									this._data.BendDescriptors.Add(item3);
									entryFaceGroup.StepBendRadius = null;
									entryFaceGroup.SubBendCount = null;
								}
								this._data.BendDescriptors.Remove(current2);
							}
						}
					}
					else
					{
						bool flag = false;
						CombinedBendDescriptor[] array = this._data.CombinedBendDescriptors.ToArray();
						foreach (CombinedBendDescriptor combinedBendDescriptor in array)
						{
							if (combinedBendDescriptor.Enumerable.FirstOrDefault((IBendDescriptor x) => x.BendParams.IsStepBend) != null && this._data.CombinedBendDescriptors.Contains(combinedBendDescriptor))
							{
								this.ConvertStepBendToBend2(combinedBendDescriptor);
								flag = true;
							}
						}
						if (flag)
						{
							CombinedBendDescriptorHelper.SetOrder(this._data.CombinedBendDescriptors);
						}
						CombinedBendDescriptor combinedBendDescriptor2 = this._data.CombinedBendDescriptors.FirstOrDefault((CombinedBendDescriptor cbd) => cbd.Enumerable.Any((IBendDescriptor b) => b.BendParams.EntryFaceGroup.ID == e.EntryFaceGroupId));
						if (combinedBendDescriptor2 != null)
						{
							this.SafeModeUnfoldErrorBendOrder.Add(combinedBendDescriptor2.Order);
						}
					}
					this.MessageDisplay.ShowWarningMessage(this._translator.Translate("l_popup.PopupUnfoldInfo.UpdateDocWarningSafeModeActivated"), null, notificationStyle: true);
					continue;
				}
				break;
			}
			using (Activity.Current?.Source.StartActivity("UpdateDoc - Notify changes"))
			{
				if (this.ToolsAndBends == null)
				{
					this.CreateToolsAndBends();
				}
				if (!this.IsAssemblyLoading)
				{
					this.ModifiedEntryModel3DChanged?.Invoke(modifiedEntryModel3D, this._data.ModifiedEntryModel3D);
					this.UnfoldModel3DChanged?.Invoke(unfoldModel3D, this._data.UnfoldModel3D);
					this.BendModel3DChanged?.Invoke(bendModel3D, this._data.BendModel3D);
					this.UpdateGeneralInfo();
				}
				this.State = DocState.BendModelCreated;
				if (this.MachineFullyLoaded)
				{
					this.State = DocState.MachineLoaded;
				}
				this.SetModelDefaultColors();
				this.DocUpdated?.Invoke(this);
			}
			return true;
		}
	}

	private void UpdateDoc7CreateBendModifiedModels()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		using (Activity.Current?.Source.StartActivity("UpdateDoc - Create Bend/Modified Models"))
		{
			try
			{
				this._data.BendModel3D = this._data.UnfoldModel3D.CopyStructure();
			}
			catch (Exception ex)
			{
				throw new UnfoldException(ex, (string)null);
			}
			Model model = this._data.EntryModel3D ?? this._data.InputModel3D;
			bool flag = model.PartInfo.PartType.HasFlag(PartType.Tube) && model.PartInfo.TubeType == TubeType.RoundTube;
			if (flag)
			{
				ModelConverter modelConverter = new ModelConverter();
				this._data.ModifiedEntryModel3D = modelConverter.Convert(modelConverter.Convert(model));
			}
			else
			{
				this._data.ModifiedEntryModel3D = this._data.UnfoldModel3D.CopyStructure();
			}
			this._data.ModifiedEntryModel3D.ModelType = ModelType.Part;
			this._data.BendModel3D.ModelType = ModelType.Part;
			this._data.BendModel3D.PartRole = PartRole.BendModel;
			foreach (Model item in this._data.BendModel3D.GetAllSubModelsWithSelf())
			{
				item.PartRole = PartRole.BendModel;
			}
			this._modifiedModelLookup = global::WiCAM.Pn4000.BendModel.GeometryTools.ModelExtensions.GetAllRoundFaceGroupModels(this._data.ModifiedEntryModel3D).ToList().ToDictionary<(FaceGroup, Model), Tuple<int, int>, (FaceGroup, Model)>(((FaceGroup fg, Model model) x) => new Tuple<int, int>(x.fg.BendEntryId, x.fg.SubBendIndex.GetValueOrDefault()), ((FaceGroup fg, Model model) x) => x);
			this._bendModelLookup = global::WiCAM.Pn4000.BendModel.GeometryTools.ModelExtensions.GetAllRoundFaceGroupModels(this._data.BendModel3D).ToList().ToDictionary<(FaceGroup, Model), Tuple<int, int>, (FaceGroup, Model)>(((FaceGroup fg, Model model) x) => new Tuple<int, int>(x.fg.BendEntryId, x.fg.SubBendIndex.GetValueOrDefault()), ((FaceGroup fg, Model model) x) => x);
			if (flag)
			{
				return;
			}
			using (Activity.Current?.Source.StartActivity("Create Bend/Modified Models - Bend ModifiedModel"))
			{
				FaceGroupModelMapping fgMapping = new FaceGroupModelMapping(this.ModifiedEntryModel3D);
				foreach (BendDescriptor bendDescriptor in this._data.BendDescriptors)
				{
					FaceGroup modifiedEntryFaceGroup = bendDescriptor.BendParams.ModifiedEntryFaceGroup;
					double kFactor = bendDescriptor.BendParams.KFactor;
					modifiedEntryFaceGroup.Unfold(fgMapping, (bendDescriptor.BendParams.FinalRadius + this.Thickness * kFactor) * bendDescriptor.BendParams.AngleAbs, modifiedEntryFaceGroup.KFactor, this.Thickness, 0.0);
				}
			}
		}
	}

	private void UpdateDoc6ReportErrorsInChangeRadius(Dictionary<FaceGroup, List<RadiusChangeProblem>> radiusChangeProblems, List<string> kFactorProblems, General3DConfig pnGeneral3DConfig)
	{
		if (!radiusChangeProblems.Any() && !kFactorProblems.Any())
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<FaceGroup, List<RadiusChangeProblem>> kvp2 in radiusChangeProblems.OrderBy<KeyValuePair<FaceGroup, List<RadiusChangeProblem>>, int>((KeyValuePair<FaceGroup, List<RadiusChangeProblem>> kvp) => this.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal c) => c.Enumerable.Any((IBendDescriptor b) => b.BendParams.UnfoldFaceGroup == kvp.Key || b.BendParams.UnfoldFaceGroup.SubGroups.Any((FaceGroup d) => d == kvp.Key)))?.Order ?? 0))
		{
			ICombinedBendDescriptorInternal combinedBendDescriptorInternal = this.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal c) => c.Enumerable.Any((IBendDescriptor b) => b.BendParams.UnfoldFaceGroup == kvp2.Key || b.BendParams.UnfoldFaceGroup.SubGroups.Any((FaceGroup d) => d == kvp2.Key)));
			foreach (IGrouping<UnfoldState, RadiusChangeProblem> item in from tuple in kvp2.Value
				group tuple by tuple.State)
			{
				list.Add($"bend # {((combinedBendDescriptorInternal != null) ? new int?(combinedBendDescriptorInternal.Order + 1) : ((int?)null))}: {item.Key.ToString()}");
			}
		}
		bool flag = false;
		foreach (string kFactorProblem in kFactorProblems)
		{
			flag = true;
			list.Add(kFactorProblem);
		}
		if (!this.IsAssemblyLoading)
		{
			bool notificationStyle = pnGeneral3DConfig.P3D_ReliefInfoAsNotification && !flag;
			this.MessageDisplay?.ShowWarningMessage(this._globals.LanguageDictionary.GetMsg2Int("Unfold Warning") + ": " + Environment.NewLine + string.Join(Environment.NewLine, list), null, notificationStyle);
		}
		else
		{
			this.MessageDisplay?.LogWarningMessage(this._globals.LanguageDictionary.GetMsg2Int("Unfold Warning") + ": " + Environment.NewLine + string.Join(Environment.NewLine, list));
		}
	}

	private List<string> UpdateDoc6UnfoldWithNewRadiiTools(General3DConfig pnGeneral3DConfig, out Dictionary<FaceGroup, List<RadiusChangeProblem>> radiusChangeProblems)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		List<string> kFactorProblems;
		using (Activity.Current?.Source.StartActivity("UpdateDoc - unfold with new radii/tools"))
		{
			kFactorProblems = new List<string>();
			this.HasRadiusChangeErrors = false;
			if (this.SafeModeUnfold)
			{
				radiusChangeProblems = new Dictionary<FaceGroup, List<RadiusChangeProblem>>();
				return kFactorProblems;
			}
			Dictionary<int, UnfoldBendParams> dictionary = this._data.BendDescriptors.ToDictionary((BendDescriptor x) => x.BendParams.UnfoldFaceGroup.ID, delegate(BendDescriptor x)
			{
				List<CombinedBendDescriptor> source = this._data.CombinedBendDescriptors.Where((CombinedBendDescriptor cbd) => cbd.Enumerable.Contains(x)).ToList();
				UnfoldBendParams unfoldBendParams = new UnfoldBendParams
				{
					UseDIN = this.UseDINUnfold,
					Radius = x.BendParams.FinalRadius,
					CutOutWidth = pnGeneral3DConfig.P3D_Default_ReliefWidth
				};
				if (source.Any())
				{
					CombinedBendDescriptor combinedBendDescriptor = source.FirstOrDefault((CombinedBendDescriptor cbd) => cbd.BendType == CombinedBendType.HemBend);
					IBendDescriptor bendDescriptor = combinedBendDescriptor?.Enumerable.FirstOrDefault();
					if (bendDescriptor != null)
					{
						combinedBendDescriptor.SplitPredecessors.First().Enumerable.First();
						unfoldBendParams.UseDIN = true;
						unfoldBendParams.HemRadius = bendDescriptor.BendParams.ManualRadius ?? bendDescriptor.BendParams.OriginalRadius;
						unfoldBendParams.HemStepStart = combinedBendDescriptor.ProgressStart;
						unfoldBendParams.Radius = Math.Max(this.PreferredProfileStore.GetBestPunchDieProfiles(combinedBendDescriptor.SplitPredecessors.FirstOrDefault() ?? combinedBendDescriptor).upperTool?.Radius ?? unfoldBendParams.HemRadius.Value, unfoldBendParams.HemRadius.Value);
					}
				}
				return unfoldBendParams;
			});
			radiusChangeProblems = ModelUnfold.UnfoldAllWithRadiusChange(this._data.UnfoldModel3D, dictionary, new GetKFactorFunc(GetKFactor));
			return kFactorProblems;
		}
		double GetKFactor(int faceGroupId)
		{
			double num = this.BendMachine?.UnfoldConfig?.DefaultKFactor ?? pnGeneral3DConfig.P3D_Default_KFactor;
			BendDescriptor bd = this._data.BendDescriptors.FirstOrDefault((BendDescriptor x) => x.BendParams.UnfoldFaceGroup.ID == faceGroupId || x.BendParams.UnfoldFaceGroup.SubGroups.Any((FaceGroup y) => y.ID == faceGroupId));
			if (this.EntryModel3D.PartInfo.PartType.HasFlag(PartType.Tube))
			{
				num = pnGeneral3DConfig.P3D_Default_Tube_KFactor;
			}
			else if (!(this.BendMachine?.UnfoldConfig?.IgnoreBendTable ?? pnGeneral3DConfig.P3D_IgnoreBendTable))
			{
				num = bd?.BendParams.KFactor ?? num;
			}
			double minimumWidthOfRescaledBendArea = bd.BendParams.UnfoldFaceGroup.MinimumWidthOfRescaledBendArea;
			if ((bd.BendParams.FinalRadius + this.Thickness * num) * bd.BendParams.AngleAbs <= minimumWidthOfRescaledBendArea)
			{
				num = (minimumWidthOfRescaledBendArea / bd.BendParams.AngleAbs - bd.BendParams.FinalRadius) / this.Thickness;
				kFactorProblems.Add($"bend # {(this.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Enumerable.Any((IBendDescriptor desc) => desc.BendParams.UnfoldFaceGroup == bd.BendParams.UnfoldFaceGroup))?.Order ?? (-1)) + 1}: invalid kFactor {bd.BendParams.KFactor:0.###} is changed to {num:0.###} to ensure minimum width of {minimumWidthOfRescaledBendArea} mm of bend zone");
				bd.BendParamsInternal.KFactor = num;
				this.HasRadiusChangeErrors = true;
			}
			return num;
		}
	}

	private void UpdateDoc5CalculateValuesForBends()
	{
		using (Activity.Current?.Source.StartActivity("UpdateDoc - CalculateValuesForBends"))
		{
			int num = 0;
			foreach (CombinedBendDescriptor combinedBendDescriptor in this._data.CombinedBendDescriptors)
			{
				combinedBendDescriptor.Order = num;
				num++;
			}
			foreach (BendDescriptor bendDescriptor in this._data.BendDescriptors)
			{
				var (upperTool, lowerTool, _) = this.PreferredProfileStore.GetBestToolProfiles(this._data.CombinedBendDescriptors.Last((CombinedBendDescriptor x) => x.BendsInternal.Contains(bendDescriptor)));
				bendDescriptor.BendParamsInternal.UpdateData(upperTool, lowerTool);
			}
			Dictionary<IBendDescriptor, BendTableReturnValues> source = this.CombinedBendDescriptors.Where((ICombinedBendDescriptorInternal cbd) => cbd.IsIncluded).SelectMany((ICombinedBendDescriptorInternal x) => x.Enumerable).Distinct()
				.ToDictionary((IBendDescriptor x) => x, (IBendDescriptor x) => x.BendParams.KFactorAlgorithm);
			if (!this.IsSerialized && source.Any((KeyValuePair<IBendDescriptor, BendTableReturnValues> kvp) => kvp.Value == BendTableReturnValues.NO_VALUE_FOUND))
			{
				this.KFactorWarningsAcceptedByUser = false;
			}
		}
	}

	private void UpdateDoc4CreateCombinedBends()
	{
		if (this.CombinedBendDescriptors.Any())
		{
			return;
		}
		using (Activity.Current?.Source.StartActivity("UpdateDoc - CreateCombinedBends"))
		{
			foreach (BendDescriptor bendDescriptor in this._data.BendDescriptors)
			{
				CombinedBendDescriptor item = new CombinedBendDescriptor(new List<BendDescriptor> { bendDescriptor }, this);
				this._data.CombinedBendDescriptors.Add(item);
			}
			foreach (CombinedBendDescriptor item3 in this._data.CombinedBendDescriptors.ToList())
			{
				double hemSplitAngle = this.Tools.GetHemSplitAngle(item3);
				item3.UpdatePositioningInfo();
				if (item3[0].BendParams.AngleAbs - hemSplitAngle > 0.0015)
				{
					item3.ProgressStop = 1.0 - hemSplitAngle / item3[0].BendParams.AngleAbs;
					CombinedBendDescriptor item2 = new CombinedBendDescriptor(item3, CombinedBendType.HemBend)
					{
						SplitPredecessors = new List<CombinedBendDescriptor> { item3 },
						ProgressStart = item3.ProgressStop,
						ProgressStop = 0.0
					};
					this._data.CombinedBendDescriptors.Insert(this._data.CombinedBendDescriptors.IndexOf(item3) + 1, item2);
				}
			}
		}
	}

	private bool UpdateDoc3VisibleFace()
	{
		if (this.UnfoldModel3D.GetFirstFaceOfGroup(this.VisibleFaceGroupId, this.VisibleFaceGroupSide) == null)
		{
			Face face = ((this.ReconstructedEntryModel.PartInfo.TubeType == TubeType.RoundTube) ? this.UnfoldModel3D.GetAllRoundFaceGroups().FirstOrDefault().Side0.FirstOrDefault() : GetBiggestFace(global::WiCAM.Pn4000.BendModel.GeometryTools.ModelExtensions.GetAllSubModels(this.UnfoldModel3D).SelectMany((Model m) => m.Shell.FlatFaceGroups)));
			if (face == null)
			{
				face = GetBiggestFace(global::WiCAM.Pn4000.BendModel.GeometryTools.ModelExtensions.GetAllSubModels(this.UnfoldModel3D).SelectMany((Model x) => x.Shell.RoundFaceGroups));
			}
			if (face == null)
			{
				face = global::WiCAM.Pn4000.BendModel.GeometryTools.ModelExtensions.GetAllFaces(this.UnfoldModel3D, false).FirstOrDefault();
			}
			if (face != null)
			{
				FaceGroup faceGroup = face.FaceGroup;
				this.VisibleFaceGroupId = faceGroup.BendEntryId;
				if (faceGroup.Side0.Contains(face))
				{
					this.VisibleFaceGroupSide = 0;
					if (!faceGroup.Side1.Any())
					{
						return false;
					}
				}
				else
				{
					if (!faceGroup.Side1.Contains(face))
					{
						return false;
					}
					this.VisibleFaceGroupSide = 1;
					if (!faceGroup.Side0.Any())
					{
						return false;
					}
				}
			}
		}
		return true;
		static Face? GetBiggestFace(IEnumerable<FaceGroup> faceGroups)
		{
			FaceGroup faceGroup2 = (from fg in faceGroups.SelectMany(GetFaceGroups)
				where fg.Side0.Count > 0 && fg.Side1.Count > 0
				orderby fg.Side0.Concat(fg.Side1).Sum((Face f) => f.Area) descending, fg.Side0.Concat(fg.Side1).SelectMany((Face f) => f.BoundaryEdgesCcw).Max((FaceHalfEdge e) => e.Vertices.Max()) descending
				select fg).FirstOrDefault();
			return (from f in faceGroup2?.Side0.Concat(faceGroup2.Side1)
				orderby f.BoundaryEdgesCcw.Max((FaceHalfEdge e) => e.Vertices.Max()) descending
				select f).First();
		}
		[IteratorStateMachine(typeof(_003C_003CUpdateDoc3VisibleFace_003Eg__GetFaceGroups_007C419_0_003Ed))]
		static IEnumerable<FaceGroup> GetFaceGroups(FaceGroup fg)
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new _003C_003CUpdateDoc3VisibleFace_003Eg__GetFaceGroups_007C419_0_003Ed(-2)
			{
				_003C_003E3__fg = fg
			};
		}
	}

	private void UpdateDoc1CheckMaterial()
	{
		if (this.Material != null)
		{
			return;
		}
		if (this.IsAssemblyLoading)
		{
			int matId = this._importMaterialMapper.GetMaterialId(this.EntryModel3D.OriginalMaterialName);
			this._data.Material = this._materialManager.MaterialList.FirstOrDefault((IMaterialArt m) => m.Number == matId) ?? this._materialManager.MaterialList.FirstOrDefault();
		}
		else
		{
			this._data.Material = this.FrontCalls.GetMaterial(this, this._globals, this.MessageDisplay) ?? this._materialManager.MaterialList.FirstOrDefault();
		}
	}

	internal void CreateUnfoldModel()
	{
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		using (Activity.Current?.Source.StartActivity("UpdateDoc - create unfold model"))
		{
			this._data.UnfoldModel3D = this.ReconstructedEntryModel.CreateUnfoldModel(this.ReconstructedEntryModel.PartInfo, delegate(int id)
			{
				if (this._data.BendDescriptors.Any())
				{
					List<BendDescriptor> list = this._data.BendDescriptors.Where((BendDescriptor x) => x.BendParams.EntryFaceGroup.ID == id).ToList();
					return new StepBendParams
					{
						IsStepBend = (list.FirstOrDefault()?.BendParams is StepBendParameters),
						StepBendRadius = list.Max((BendDescriptor x) => x.BendParams.FinalRadius),
						StepBendNumber = list.Count
					};
				}
				return new StepBendParams
				{
					IsStepBend = false
				};
			}, this.CurrentSimulationInstancesAdditionalPart, out var _);
			this._data.UnfoldModel3D.ModelType = ModelType.Part;
			List<(FaceGroup, Model)> list2 = global::WiCAM.Pn4000.BendModel.GeometryTools.ModelExtensions.GetAllRoundFaceGroupModels(this._data.UnfoldModel3D).ToList();
			this._unfoldModelLookup = list2.ToDictionary<(FaceGroup, Model), Tuple<int, int>, (FaceGroup, Model)>(((FaceGroup fg, Model model) x) => new Tuple<int, int>(x.fg.BendEntryId, x.fg.SubBendIndex.GetValueOrDefault()), ((FaceGroup fg, Model model) x) => x);
			FaceGroupModelMapping fgMapping = new FaceGroupModelMapping(this._data.UnfoldModel3D);
			foreach (var item2 in list2)
			{
				FaceGroup item = item2.Item1;
				Model fgModel = item2.Item2;
				item.IsNeighbor0Parent = fgModel.NeighborMapping.Neighbors0(item).Any((FaceGroup x) => x.GetModel(fgMapping) == fgModel.Parent);
				foreach (FaceGroup subGroup in item.SubGroups)
				{
					subGroup.IsNeighbor0Parent = item.IsNeighbor0Parent;
				}
			}
			foreach (BendDescriptor bendDescriptor in this._data.BendDescriptors)
			{
				if (bendDescriptor.BendParams.UnfoldFaceGroup == null)
				{
					throw new UnfoldException(bendDescriptor.BendParams.EntryFaceGroup.ID, (Exception)null, (string)null);
				}
			}
		}
	}

	private void CalculateBendAngleSign()
	{
		foreach (BendDescriptor bendDescriptor in this._data.BendDescriptors)
		{
			FaceGroup unfoldFaceGroup = bendDescriptor.BendParams.UnfoldFaceGroup;
			Model unfoldFaceGroupModel = bendDescriptor.BendParams.UnfoldFaceGroupModel;
			bool flag = true;
			Face? face = unfoldFaceGroup.Side1.Concat(unfoldFaceGroup.SubGroups.SelectMany((FaceGroup x) => x.Side1)).FirstOrDefault((Face f) => f.FaceType == FaceType.RoundCylindricalConcave);
			Vector3d v = face.Mesh.First().TriangleNormal;
			face.Shell.GetWorldMatrix(unfoldFaceGroupModel).TransformNormalInPlace(ref v);
			(Face face, Model model) firstFaceOfGroupModel = this.UnfoldModel3D.GetFirstFaceOfGroupModel(this.VisibleFaceGroupId, this.VisibleFaceGroupSide);
			Face item = firstFaceOfGroupModel.face;
			Model item2 = firstFaceOfGroupModel.model;
			Vector3d v2 = item.Mesh.First().TriangleNormal;
			item.Shell.GetWorldMatrix(item2).TransformNormalInPlace(ref v2);
			flag = Math.Round(Math.Abs(Math.Round(v.UnsignedAngle(v2), 5)), 6) < Math.PI / 2.0;
			bendDescriptor.BendParamsInternal.AngleSign = (flag ? 1 : (-1));
		}
	}

	private IDieGroup GetDieGroup(CombinedBendDescriptor bendDesc, out NoToolsFoundReason reason)
	{
		if (this.BendMachine == null)
		{
			reason = new NoToolsFoundReason();
			return null;
		}
		if (this.Tools.GetPreferredTools(bendDesc) != null)
		{
			reason = new NoToolsFoundReason();
			throw new NotImplementedException();
		}
		return this.Tools.GetDieGroupForFaceGroup(bendDesc, out reason);
	}

	private IPunchGroup GetPunchGroup(CombinedBendDescriptor bendDesc, out NoToolsFoundReason reason)
	{
		if (this.BendMachine == null)
		{
			reason = new NoToolsFoundReason();
			return null;
		}
		IPreferredProfile preferredTools = this.Tools.GetPreferredTools(bendDesc);
		reason = new NoToolsFoundReason();
		if (preferredTools != null)
		{
			throw new NotImplementedException();
		}
		return this.Tools.GetPunchGroupForFaceGroup(bendDesc, out reason);
	}

	public void AssignSuggestedTools()
	{
		throw new NotImplementedException();
	}

	public void AssignPreferredTools()
	{
		throw new NotImplementedException();
	}

	public bool ValidateTools(out string message, out bool maxForceOk, out bool toolOverlapOk, out bool toolSavetyDistOk, out bool toolsFitInMachine)
	{
		maxForceOk = true;
		toolOverlapOk = true;
		toolSavetyDistOk = true;
		new StringBuilder("");
		toolsFitInMachine = true;
		message = "";
		return true;
	}

	public void UpdateGeneralInfo()
	{
		this.UpdateGeneralInfoAutoEvent?.Invoke(this);
	}

	public void UpdateGeneralInfo(ModelViewMode viewMode)
	{
		this.UpdateGeneralInfoEvent?.Invoke(this, viewMode);
	}

	public void UpdateBendMachineInfo()
	{
		this.UpdateBendMachineInfoEvent?.Invoke(this);
	}

	public IBendTable GetApplicableBendTable(out bool isMachineSpecific)
	{
		if (this.BendMachine != null)
		{
			isMachineSpecific = true;
			return this.BendMachine.BendTable;
		}
		isMachineSpecific = false;
		return this._globalBendTable.GetGlobalBendTable();
	}

	public bool CheckSelfCollisionUnfoldModel()
	{
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		ICombinedBendDescriptorInternal combinedBendDescriptorInternal = this.CombinedBendDescriptors.FirstOrDefault();
		if (combinedBendDescriptorInternal != null)
		{
			(HashSet<FaceGroup>, HashSet<FaceGroup>) legsOfBendInUnfoldModel = combinedBendDescriptorInternal.GetLegsOfBendInUnfoldModel();
			List<FaceGroup> list = legsOfBendInUnfoldModel.Item1.Concat(legsOfBendInUnfoldModel.Item2).Distinct().ToList();
			FaceGroupModelMapping mapping = new FaceGroupModelMapping(this.UnfoldModel3D);
			for (int i = 0; i < list.Count - 2; i++)
			{
				FaceGroup g = list[i];
				Model model = g.GetModel(mapping);
				for (int j = i + 1; j < list.Count - 1; j++)
				{
					FaceGroup faceGroup = list[j];
					Model model2 = faceGroup.GetModel(mapping);
					if (general3DConfig.P3D_IgnoreSelfCollision)
					{
						HashSet<FaceGroup> hashSet = (from x in model.NeighborMapping.Neighbors0(g)
							where !x.IsBendingZone
							select x).ToHashSet();
						HashSet<FaceGroup> hashSet2 = (from x in model.NeighborMapping.Neighbors1(g)
							where !x.IsBendingZone
							select x).ToHashSet();
						HashSet<FaceGroup> source = model2.NeighborMapping.Neighbors0(faceGroup);
						HashSet<FaceGroup> source2 = model2.NeighborMapping.Neighbors1(faceGroup);
						if (hashSet.Overlaps(source.Where((FaceGroup x) => !x.IsBendingZone)) || hashSet2.Overlaps(source.Where((FaceGroup x) => !x.IsBendingZone)) || hashSet2.Overlaps(source2.Where((FaceGroup x) => !x.IsBendingZone)) || hashSet.Overlaps(source2.Where((FaceGroup x) => !x.IsBendingZone)))
						{
							continue;
						}
					}
					if (!model.NeighborMapping.Neighbors0(g).Contains(faceGroup) && !model.NeighborMapping.Neighbors1(g).Contains(faceGroup))
					{
						ModelCollision obj = new ModelCollision
						{
							ModelA = g.GetModel(mapping),
							ModelB = faceGroup.GetModel(mapping),
							BreakAtFirstCollision = true,
							TreatTouchingFacesAsCollision = false,
							ProccessSubModels = false
						};
						Model parent = obj.ModelA.Parent;
						Model parent2 = obj.ModelB.Parent;
						Matrix4d worldMatrix = obj.ModelA.WorldMatrix;
						Matrix4d worldMatrix2 = obj.ModelB.WorldMatrix;
						Matrix4d transform = obj.ModelA.Transform;
						Matrix4d transform2 = obj.ModelB.Transform;
						AABB<Vector3d> boundingBox = obj.ModelA.Shell.AABBTree.Root.BoundingBox;
						AABB<Vector3d> boundingBox2 = obj.ModelB.Shell.AABBTree.Root.BoundingBox;
						Vector3d vector3d = (boundingBox.Min + boundingBox.Max) * 0.5;
						Vector3d vector3d2 = (boundingBox2.Min + boundingBox2.Max) * 0.5;
						Vector3d vector3d3 = boundingBox.Max - boundingBox.Min;
						Vector3d vector3d4 = boundingBox2.Max - boundingBox2.Min;
						Vector3d vector3d5 = vector3d3 - new Vector3d(0.045, 0.045, 0.045);
						Vector3d vector3d6 = vector3d4 - new Vector3d(0.045, 0.045, 0.045);
						obj.ModelA.Transform = Matrix4d.Translation(vector3d * -1.0) * Matrix4d.Scale((vector3d3.X != 0.0) ? (vector3d5.X / vector3d3.X) : 1.0, (vector3d3.Y != 0.0) ? (vector3d5.Y / vector3d3.Y) : 1.0, (vector3d3.Z != 0.0) ? (vector3d5.Z / vector3d3.Z) : 1.0) * Matrix4d.Translation(vector3d) * worldMatrix;
						obj.ModelB.Transform = Matrix4d.Translation(vector3d2 * -1.0) * Matrix4d.Scale((vector3d4.X != 0.0) ? (vector3d6.X / vector3d4.X) : 1.0, (vector3d4.Y != 0.0) ? (vector3d6.Y / vector3d4.Y) : 1.0, (vector3d4.Z != 0.0) ? (vector3d6.Z / vector3d4.Z) : 1.0) * Matrix4d.Translation(vector3d2) * worldMatrix2;
						obj.ModelA.Parent = null;
						obj.ModelB.Parent = null;
						obj.TestTrianglesFunc = (Triangle triA, Triangle triB, Matrix4d mB2A) => triA.Intersect(triB, mB2A, out var isCoplanar, out var isTouching, null) && !isCoplanar && !isTouching;
						bool flag = obj.UpdateAnyCollisions();
						obj.ModelA.Transform = transform;
						obj.ModelB.Transform = transform2;
						obj.ModelA.Parent = parent;
						obj.ModelB.Parent = parent2;
						if (flag)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public void CalculateFingers()
	{
		this.CalculateFingers((from x in this.CombinedBendDescriptors
			where x.IsIncluded
			select x.Order).ToList());
	}

	public void CalculateFingers(List<int> bendOrders)
	{
		this._factorio.Resolve<IFingerStopCalculationMediator>().CalculateStopPoints(this.BendModel3D, this.Material, this.Thickness, bendOrders, this.CombinedBendDescriptors.ToList(), this.ToolsAndBends.BendPositions, this.BendMachine, this.ToolsAndBends.ToolSetups);
		this.State = DocState.FingerCalculated;
		this.FingerChanged?.Invoke(this);
		this.RecalculateSimulation();
	}

	public void FingerChangedInvoke()
	{
		this.FingerChanged?.Invoke(this);
	}

	public void RecalculateSimulation()
	{
		this._recalcSimDirtyFlag++;
		if (this._recalcSimDirtyFlag == 1)
		{
			Application.Current.Dispatcher.BeginInvoke((Action)delegate
			{
				this._recalcSimDirtyFlag = 0;
				this.RefreshSimulation?.Invoke(this);
			});
		}
	}

	public bool IsUpdateDocNeeded()
	{
		if (this.ReconstructedEntryModel.PartInfo.PartType == PartType.Unassigned || this.ReconstructedEntryModel.PartInfo.PartType.HasFlag(PartType.Error))
		{
			return false;
		}
		if (this.SafeModeUnfold)
		{
			return true;
		}
		foreach (BendDescriptor bendDescriptor in this._data.BendDescriptors)
		{
			BendParametersBase bendParamsInternal = bendDescriptor.BendParamsInternal;
			(IToolProfile? upperTool, IToolProfile? lowerTool, ToolSelectionType tst) bestToolProfiles = this.PreferredProfileStore.GetBestToolProfiles(this._data.CombinedBendDescriptors.Last((CombinedBendDescriptor x) => x.BendsInternal.Contains(bendDescriptor)));
			var (num, num2, bendTableReturnValues, num3) = bendParamsInternal.CalculateData(bestToolProfiles.upperTool, bestToolProfiles.lowerTool);
			if (num != bendParamsInternal.KFactor || num2 != bendParamsInternal.SpringBack || bendTableReturnValues != bendParamsInternal.KFactorAlgorithm || num3 != bendParamsInternal.ToolRadius)
			{
				return true;
			}
		}
		return false;
	}

	public void SetDefautMachine(bool viewStyle)
	{
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		if (this._autoMode.PopupsEnabled && general3DConfig.P3D_UseDefaultMachine && general3DConfig.P3D_Default_MachineId > -1 && !viewStyle)
		{
			this.MachinePath = this._pathService.GetMachine3DFolder(general3DConfig.P3D_Default_MachineId);
			if (!string.IsNullOrEmpty(this.MachinePath))
			{
				this.BendMachineConfig = this._factorio.Resolve<IBendMachineSimulation>().Init(this.MachinePath, null, this, out var _);
				this.ToolSelectionType = ToolSelectionType.PreferredTools;
			}
		}
	}

	public void Save(string filename)
	{
		string fileName = this._pathService.PNDRIVE + "\\u\\pn\\run\\Pkernel.dll";
		string beforeSaveData = "";
		if (new FileInfo(fileName).Exists)
		{
			beforeSaveData = FileVersionInfo.GetVersionInfo(fileName).ProductVersion;
		}
		this.DiskFile.SetBeforeSaveData(beforeSaveData);
		this._docSerializer.SerializeAndCompress(filename, this);
	}

	public void CenterModel(Model model)
	{
		(Face face, Model model) firstFaceOfGroupModel = model.GetFirstFaceOfGroupModel(this.VisibleFaceGroupId, this.VisibleFaceGroupSide);
		this.MoveVisibleFaceToZero(model, firstFaceOfGroupModel.face, firstFaceOfGroupModel.model);
	}

	public void ChangeStartFaceGroupAndSide(int newStartFaceGroupId, int side)
	{
		this.VisibleFaceGroupId = newStartFaceGroupId;
		this.VisibleFaceGroupSide = side;
		this.SetModelDefaultColors();
	}

	public void SetModelDefaultColors()
	{
		this.SetModelDefaultColors(this.EntryModel3D);
		if (this.UnfoldModel3D != null)
		{
			this.SetModelDefaultColors(this.UnfoldModel3D);
		}
		if (this.ModifiedEntryModel3D != null && this.ModifiedEntryModel3D.SubModels.Any())
		{
			this.SetModelDefaultColors(this.ModifiedEntryModel3D);
		}
		if (this.BendModel3D != null && this.BendModel3D.SubModels.Any())
		{
			this.SetModelDefaultColors(this.BendModel3D);
		}
	}

	public void SetModelDefaultColors(Model model)
	{
		UiModelType modelType = UiModelType.Entry;
		if (model == this.ReconstructedEntryModel)
		{
			modelType = UiModelType.Entry;
		}
		else if (model == this.ModifiedEntryModel3D)
		{
			modelType = UiModelType.ModifiedEntry;
		}
		else if (model == this.UnfoldModel3D)
		{
			modelType = UiModelType.Unfold;
		}
		else if (model == this.BendModel3D)
		{
			modelType = UiModelType.Bend;
		}
		ModelColors3DConfig modelColors3DConfig = this._configProvider.InjectOrCreate<ModelColors3DConfig>();
		global::WiCAM.Pn4000.Config.DataStructures.AnalyzeConfig analyzeConfig = this._configProvider.InjectOrCreate<global::WiCAM.Pn4000.Config.DataStructures.AnalyzeConfig>();
		model.UnHighLightModel();
		model.ColorModel(modelColors3DConfig, analyzeConfig, this.ValidationResults);
		this.ColorPurchasedParts(model, modelColors3DConfig.PurchasedPartFaceColor.ToBendColor());
		this.ColorNoDeductionValueFoundBendZones(model, modelColors3DConfig, modelType);
		Face visFace = model.GetFirstFaceOfGroup(this.VisibleFaceGroupId, this.VisibleFaceGroupSide);
		if (visFace == null)
		{
			return;
		}
		HashSet<Face> hashSet = null;
		if (visFace.FaceGroup.Side1.Contains(visFace))
		{
			hashSet = visFace.FaceGroup.Side1;
		}
		else if (visFace.FaceGroup.Side0.Contains(visFace))
		{
			hashSet = visFace.FaceGroup.Side0;
		}
		if (hashSet != null)
		{
			foreach (Face item in hashSet.Where((Face x) => x.Macro == null))
			{
				item.Color = modelColors3DConfig.VisibleFaceColor.ToBendColor();
			}
		}
		else
		{
			visFace.Color = modelColors3DConfig.VisibleFaceColor.ToBendColor();
		}
		foreach (Shell allShell in global::WiCAM.Pn4000.BendModel.GeometryTools.ModelExtensions.GetAllShells(model, false))
		{
			bool flag = visFace.FaceGroup.Side0.Contains(visFace);
			foreach (FaceGroup item2 in from x in allShell.GetAllFaceGroupsWithSubgroups()
				where x != visFace.FaceGroup && !x.IsBendingZone
				select x)
			{
				if (item2.IsSide0Top.Equals(visFace.FaceGroup.IsSide0Top))
				{
					foreach (Face item3 in (flag ? item2.Side0 : item2.Side1).Where((Face x) => x.Macro == null))
					{
						item3.Color = modelColors3DConfig.VisibleSideColor.ToBendColor();
					}
					continue;
				}
				foreach (Face item4 in (flag ? item2.Side1 : item2.Side0).Where((Face x) => x.Macro == null))
				{
					item4.Color = modelColors3DConfig.VisibleSideColor.ToBendColor();
				}
			}
		}
	}

	public void ColorPurchasedParts(Model model, Color colPp)
	{
		if (model == null)
		{
			return;
		}
		PartInfo partInfo = model.PartInfo;
		if (partInfo != null && partInfo.PurchasedPart > 0)
		{
			model.ColorModel(colPp);
			return;
		}
		foreach (Model subModel in model.SubModels)
		{
			this.ColorPurchasedParts(subModel, colPp);
		}
		foreach (ModelInstance item in model.ReferenceModel)
		{
			this.ColorPurchasedParts(item.Reference, colPp);
		}
	}

	public void ColorNoDeductionValueFoundBendZones(Model model, ModelColors3DConfig modelColors3D, UiModelType modelType)
	{
		if (model == null || (model.Shell == null && model.SubModels.Count == 0))
		{
			return;
		}
		foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in this.CombinedBendDescriptors)
		{
			if (combinedBendDescriptor[0].BendParams.KFactorAlgorithm != BendTableReturnValues.NO_VALUE_FOUND)
			{
				foreach (IBendDescriptor item3 in combinedBendDescriptor.Enumerable)
				{
					FaceGroup item = item3.BendParams.ModelFaceGroup(modelType).fg;
					if (item == null)
					{
						continue;
					}
					foreach (Face item4 in item.Side0.Concat(item.Side1).Concat(item.SubGroups.SelectMany((FaceGroup sg) => sg.Side0.Concat(sg.Side1))))
					{
						item4.Color = (item.InvalidChangedBendRadius.HasValue ? modelColors3D.BendZoneAdjustRadiusErrorColor.ToBendColor() : modelColors3D.BendGroupTopBottomFaceColor.ToBendColor());
					}
					foreach (Face item5 in item.ConnectingFaces.Concat(item.SubGroups.SelectMany((FaceGroup sg) => sg.ConnectingFaces)))
					{
						if (!item5.IsTessellated.HasValue)
						{
							item5.Color = (item.InvalidChangedBendRadius.HasValue ? modelColors3D.BendZoneAdjustRadiusErrorColor.ToBendColor() : modelColors3D.BendGroupConnectingFaceColor.ToBendColor());
						}
					}
				}
				continue;
			}
			foreach (IBendDescriptor item6 in combinedBendDescriptor.Enumerable)
			{
				FaceGroup item2 = item6.BendParams.ModelFaceGroup(modelType).fg;
				if (item2 == null)
				{
					continue;
				}
				foreach (Face allFace in item2.GetAllFaces())
				{
					if (!allFace.IsTessellated.HasValue)
					{
						allFace.Color = modelColors3D.BendGroupNoDeductionValueFound.ToBendColor();
					}
				}
			}
		}
	}

	public IDoc3d GetCloneForBendSequence()
	{
		Doc3d doc3d = this._docFactory.InternalCreateDoc();
		doc3d.VisibleFaceGroupId = this.VisibleFaceGroupId;
		doc3d.VisibleFaceGroupSide = this.VisibleFaceGroupSide;
		new DocData(this._data, doc3d);
		return doc3d;
	}

	public UiModelType GetModelType(Model model)
	{
		UiModelType result = UiModelType.Entry;
		if (model == this.ReconstructedEntryModel)
		{
			result = UiModelType.Entry;
		}
		else if (model == this.ModifiedEntryModel3D)
		{
			result = UiModelType.ModifiedEntry;
		}
		else if (model == this.UnfoldModel3D)
		{
			result = UiModelType.Unfold;
		}
		else if (model == this.BendModel3D)
		{
			result = UiModelType.Bend;
		}
		return result;
	}

	public void SetSmallPartType(bool isSmallPart)
	{
		PartType partType = this.EntryModel3D.PartInfo.PartType | PartType.SmallPart;
		if (!isSmallPart)
		{
			partType ^= PartType.SmallPart;
		}
		this.EntryModel3D.PartInfo.PartType = partType;
		this.ModifiedEntryModel3D.PartInfo.PartType = partType;
		this.UnfoldModel3D.PartInfo.PartType = partType;
		this.BendModel3D.PartInfo.PartType = partType;
	}

	public F2exeReturnCode CheckWarningKFactorMaterialByUser()
	{
		if (!this.PnMaterialByUser && !this.FrontCalls.CreatePostprocessorIfMaterialNotByUser())
		{
			return F2exeReturnCode.ERROR_MATERIAL_NOT_BY_USER;
		}
		if (this.KFactorWarningsError && !this.FrontCalls.CreatePostprocessorIfNoKFactor())
		{
			return F2exeReturnCode.ERROR_NO_K_FACTOR_FOUND;
		}
		return F2exeReturnCode.OK;
	}

	public void ReApplyAdditionalParts()
	{
		foreach (Model item in new Model[6] { this.EntryModel3D, this.UnfoldModel3D, this.BendModel3D, this.ModifiedEntryModel3D, this.InputModel3D, this.ReconstructedEntryModel }.Distinct())
		{
			foreach (Model item2 in item.GetAllSubModelsWithSelf())
			{
				foreach (AuxiliaryShell item3 in item2.AuxiliaryShells.Where((AuxiliaryShell x) => x.Type.HasFlag(AuxiliaryShellType.PurchasedPartWithCollision) || x.Type.HasFlag(AuxiliaryShellType.PurchasedPartIgnoreCollision)).ToList())
				{
					item2.AuxiliaryShells.Remove(item3);
				}
			}
			item.AddAdditionalParts(this.CurrentSimulationInstancesAdditionalPart);
		}
	}

	public bool HasRequiredUserComments()
	{
		foreach (KeyValuePair<int, string> item in this._factorio.Resolve<IConfigProvider>().InjectOrCreate<UserCommentsConfig>().UserCommentsRequired)
		{
			if (!this.UserComments.TryGetValue(item.Key, out string value))
			{
				return false;
			}
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}
		}
		return true;
	}

	public string GetAutoPpName(bool multi, int subNo, int? numberPp)
	{
		if (multi)
		{
			return string.Format(this.BendMachine.PressBrakeData.PpNameFormatMulti ?? this.BendMachine.PressBrakeData.PpNameFormatSingle ?? "{1}.{2}", numberPp ?? this.NumberPp, this.DiskFile?.Header?.ModelName, subNo);
		}
		return string.Format(this.BendMachine.PressBrakeData.PpNameFormatSingle ?? this.BendMachine.PressBrakeData.PpNameFormatMulti ?? "{1}.{2}", numberPp ?? this.NumberPp, this.DiskFile?.Header?.ModelName, 1);
	}

	[IteratorStateMachine(typeof(_003CBendsPerPp_003Ed__453))]
	public IEnumerable<(int subNo, List<ICombinedBendDescriptor> subBends)> BendsPerPp(IEnumerable<ICombinedBendDescriptor> bends)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CBendsPerPp_003Ed__453(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__bends = bends
		};
	}

	public IPnBndDoc Copy(ModelCopyMode entry, ModelCopyMode unfold, ModelCopyMode bend, ModelCopyMode machine)
	{
		return this.Copy(new PnBndDocCopyOptions
		{
			EntryModelCopyMode = entry,
			UnfoldModelCopyMode = unfold,
			BendModelCopyMode = bend,
			MachineModelCopyMode = machine
		});
	}

	public IDoc3d Copy(IPnBndDocCopyOptions options)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		Doc3d doc3d = this._docFactory.InternalCreateDoc();
		doc3d._data = new DocData();
		doc3d.Thickness = this.Thickness;
		stopwatch.Stop();
		stopwatch.Restart();
		DocData data = doc3d._data;
		ModelCopyMode entryModelCopyMode = options.EntryModelCopyMode;
		Model model = default(Model);
		switch (entryModelCopyMode)
		{
		case ModelCopyMode.None:
			model = null;
			break;
		case ModelCopyMode.Reference:
			model = this.EntryModel3D;
			break;
		case ModelCopyMode.Copy:
			model = this.EntryModel3D.CopyStructure();
			break;
		case ModelCopyMode.Simplify:
			model = this.EntryModel3D.Simplify();
			break;
		default:
			ThrowSwitchExpressionException(entryModelCopyMode);
			break;
		}
		data.EntryModel3D = model;
		data = doc3d._data;
		entryModelCopyMode = options.UnfoldModelCopyMode;
		switch (entryModelCopyMode)
		{
		case ModelCopyMode.None:
			model = null;
			break;
		case ModelCopyMode.Reference:
			model = this.UnfoldModel3D;
			break;
		case ModelCopyMode.Copy:
			model = this.UnfoldModel3D.CopyStructure();
			break;
		case ModelCopyMode.Simplify:
			model = this.UnfoldModel3D.Simplify();
			break;
		default:
			ThrowSwitchExpressionException(entryModelCopyMode);
			break;
		}
		data.UnfoldModel3D = model;
		data = doc3d._data;
		entryModelCopyMode = options.BendModelCopyMode;
		switch (entryModelCopyMode)
		{
		case ModelCopyMode.None:
			model = null;
			break;
		case ModelCopyMode.Reference:
			model = this.BendModel3D;
			break;
		case ModelCopyMode.Copy:
			model = this.BendModel3D.CopyStructure();
			break;
		case ModelCopyMode.Simplify:
			model = this.BendModel3D.Simplify();
			break;
		default:
			ThrowSwitchExpressionException(entryModelCopyMode);
			break;
		}
		data.BendModel3D = model;
		stopwatch.Stop();
		stopwatch.Restart();
		doc3d.PreferredProfileStore.CopyFrom(this.PreferredProfileStore);
		Dictionary<ICombinedBendDescriptorInternal, IPreferredProfile> preferredProfilesDict = new Dictionary<ICombinedBendDescriptorInternal, IPreferredProfile>();
		if (this._data.BendMachineConfig != null)
		{
			doc3d._data.BendMachineConfig = this._factorio.Resolve<IBendMachineSimulation>().Init(this.MachinePath, preferredProfilesDict, doc3d, out var _);
		}
		doc3d._bendMachine = this._bendMachine;
		stopwatch.Stop();
		stopwatch.Restart();
		stopwatch.Stop();
		stopwatch.Restart();
		doc3d._data.Tools = new BendToolExpert(this._globals);
		doc3d._data.UseDINUnfold = this._data.UseDINUnfold;
		doc3d._data.Material = this.Material;
		doc3d._data.MaterialSet = this._data.MaterialSet;
		doc3d._data.PnMaterialByUser = this._data.PnMaterialByUser;
		doc3d.HasSimulationCalculated = this.HasSimulationCalculated;
		doc3d.HasBendStepsCalculated = this.HasBendStepsCalculated;
		doc3d.ToolCalculationEnabled = this.ToolCalculationEnabled;
		doc3d.DiskFile = this.DiskFile;
		doc3d.State = this.State;
		stopwatch.Stop();
		stopwatch.Restart();
		BendToolExpert bendToolExpert = new BendToolExpert(this._globals);
		bendToolExpert.Init(this);
		doc3d._data.Tools = bendToolExpert;
		stopwatch.Stop();
		stopwatch.Restart();
		doc3d.VisibleFaceGroupId = this.VisibleFaceGroupId;
		doc3d.VisibleFaceGroupSide = this.VisibleFaceGroupSide;
		Model? unfoldModel3D = doc3d._data.UnfoldModel3D;
		doc3d._unfoldModelLookup = ((unfoldModel3D != null) ? global::WiCAM.Pn4000.BendModel.GeometryTools.ModelExtensions.GetAllRoundFaceGroupModels(unfoldModel3D).ToList() : null)?.ToDictionary<(FaceGroup, Model), Tuple<int, int>, (FaceGroup, Model)>(((FaceGroup fg, Model model) x) => new Tuple<int, int>(x.fg.BendEntryId, x.fg.SubBendIndex.GetValueOrDefault()), ((FaceGroup fg, Model model) x) => x) ?? new Dictionary<Tuple<int, int>, (FaceGroup, Model)>();
		Model? modifiedEntryModel3D = doc3d._data.ModifiedEntryModel3D;
		doc3d._modifiedModelLookup = ((modifiedEntryModel3D != null) ? global::WiCAM.Pn4000.BendModel.GeometryTools.ModelExtensions.GetAllRoundFaceGroupModels(modifiedEntryModel3D).ToList() : null)?.ToDictionary<(FaceGroup, Model), Tuple<int, int>, (FaceGroup, Model)>(((FaceGroup fg, Model model) x) => new Tuple<int, int>(x.fg.BendEntryId, x.fg.SubBendIndex.GetValueOrDefault()), ((FaceGroup fg, Model model) x) => x) ?? new Dictionary<Tuple<int, int>, (FaceGroup, Model)>();
		Model? bendModel3D = doc3d._data.BendModel3D;
		doc3d._bendModelLookup = ((bendModel3D != null) ? global::WiCAM.Pn4000.BendModel.GeometryTools.ModelExtensions.GetAllRoundFaceGroupModels(bendModel3D).ToList() : null)?.ToDictionary<(FaceGroup, Model), Tuple<int, int>, (FaceGroup, Model)>(((FaceGroup fg, Model model) x) => new Tuple<int, int>(x.fg.BendEntryId, x.fg.SubBendIndex.GetValueOrDefault()), ((FaceGroup fg, Model model) x) => x) ?? new Dictionary<Tuple<int, int>, (FaceGroup, Model)>();
		foreach (BendDescriptor bendDescriptor in this._data.BendDescriptors)
		{
			doc3d._data.BendDescriptors.Add(bendDescriptor.Copy(doc3d));
		}
		foreach (CombinedBendDescriptor combinedBendDescriptor in this._data.CombinedBendDescriptors)
		{
			doc3d._data.CombinedBendDescriptors.Add(combinedBendDescriptor.Copy(doc3d));
		}
		stopwatch.Stop();
		stopwatch.Restart();
		IToolsAndBends toolsAndBends = this._factorio.Resolve<IToolFactory>().CreateToolsAndBends(doc3d.CombinedBendDescriptors, doc3d.BendMachine);
		List<IBendPositioning> bendPositionings = toolsAndBends.BendPositions.ToList();
		stopwatch.Stop();
		stopwatch.Restart();
		doc3d.BendMachineConfig?.CalculateBendSteps(doc3d.BendModel3D, bendPositionings, calculateFingerPos: false);
		doc3d._toolsAndBends = toolsAndBends;
		stopwatch.Stop();
		return doc3d;
	}

    private void ThrowSwitchExpressionException(ModelCopyMode entryModelCopyMode)
    {
        throw new NotImplementedException();
    }
}
