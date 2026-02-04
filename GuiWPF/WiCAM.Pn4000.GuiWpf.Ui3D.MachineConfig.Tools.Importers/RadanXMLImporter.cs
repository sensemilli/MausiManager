using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.RadanConfig.Adapter;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.RadanConfig.Die;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.RadanConfig.Punch;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.Modifiers;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;

public class RadanXMLImporter : IToolImporter, IAdapterImporter
{
	private readonly IPnPathService _pnPathService;

	private readonly IModelFactory _modelFactory;

	private readonly IUnitConverter _unitConverter;

	private readonly IGlobalToolStorage _toolStorage;

	private readonly IConfigProvider _configProvider;

	private readonly ImportHelper _importHelper;

	private MachineToolsViewModel _machineTools;

	public RadanXMLImporter(IPnPathService pnPathService, IModelFactory modelFactory, IUnitConverter unitConverter, IGlobalToolStorage toolStorage, IConfigProvider configProvider, ImportHelper importHelper)
	{
		_pnPathService = pnPathService;
		_modelFactory = modelFactory;
		_unitConverter = unitConverter;
		_toolStorage = toolStorage;
		_configProvider = configProvider;
		_importHelper = importHelper;
	}

	public void Init(MachineToolsViewModel machineTools)
	{
		_machineTools = machineTools;
	}

	public string GetFilterString()
	{
		return Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.RadanXml") as string;
	}

	public void ImportPunches(string filePath)
	{
		foreach (PunchProfile punchprofile in Xml.DeserializeFromXml<PunchExport>(filePath, Encoding.GetEncoding(1252)).punchprofiles)
		{
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(punchprofile.name));
			UpperToolGroupViewModel matchingUpperGroup = _machineTools.GetMatchingUpperGroup(punchprofile.radius);
			UpperToolViewModel upperToolVM = ConvertPunch(punchprofile, multiTool, matchingUpperGroup);
			foreach (PunchPart punchpart in punchprofile.punchparts)
			{
				_machineTools.UpperPieces.Add(ConvertPunchPiece(upperToolVM, punchpart, multiTool));
			}
		}
	}

	public void ImportDies(string filePath)
	{
		foreach (DieProfile dieprofile in Xml.DeserializeFromXml<DieExport>(filePath, Encoding.GetEncoding(1252)).dieprofiles)
		{
			if (!dieprofile.geometryFile.blob.Any())
			{
				continue;
			}
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(dieprofile.name));
			LowerToolGroupViewModel matchingLowerGroup = _machineTools.GetMatchingLowerGroup(dieprofile.vWidth, dieprofile.angle);
			LowerToolViewModel lowerTool = ConvertDie(dieprofile, multiTool, matchingLowerGroup);
			foreach (DiePart diepart in dieprofile.dieparts)
			{
				LowerToolPieceViewModel item = ConvertDiePiece(diepart, lowerTool, multiTool);
				_machineTools.LowerPieces.Add(item);
			}
		}
	}

	public void ImportLowerAdapters(string filePath)
	{
		foreach (AdapterProfile adapterprofile in Xml.DeserializeFromXml<AdapterExport>(filePath, Encoding.GetEncoding(1252)).adapterprofiles)
		{
			MultiToolViewModel multiToolViewModel = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(adapterprofile.Name));
			_machineTools.LowerAdapters.Add(ConverLowerAdapter(adapterprofile, multiToolViewModel));
			foreach (AdapterPart item in adapterprofile.AdapterPart)
			{
				_machineTools.LowerPieces.Add(ConvertLowerAdapterPart(item, multiToolViewModel));
			}
		}
	}

	public void ImportUpperAdapters(string filePath)
	{
		foreach (AdapterProfile adapterprofile in Xml.DeserializeFromXml<AdapterExport>(filePath, Encoding.GetEncoding(1252)).adapterprofiles)
		{
			MultiToolViewModel multiToolViewModel = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(adapterprofile.Name));
			_machineTools.LowerAdapters.Add(ConvertUpperAdapter(adapterprofile, multiToolViewModel));
			foreach (AdapterPart item in adapterprofile.AdapterPart)
			{
				_machineTools.LowerPieces.Add(ConvertUpperAdapterPart(item, multiToolViewModel));
			}
		}
	}

	private LowerToolPieceViewModel ConvertUpperAdapterPart(AdapterPart? dpa, MultiToolViewModel mutliTool)
	{
		LowerToolPieceViewModel lowerToolPieceViewModel = _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, mutliTool, _importHelper.MakeValidFileName(dpa.Name), dpa.Length, dpa.Amount);
		lowerToolPieceViewModel.MaxAllowableToolForce = dpa.MaximumAllowableForceVal;
		string name = dpa.GeometryFile.name;
		if (name != null && name.Any() && dpa.GeometryFile.blob.Any())
		{
			lowerToolPieceViewModel.Geometry3D = Load3DGeometry(dpa.GeometryFile.name);
		}
		return lowerToolPieceViewModel;
	}

	private LowerAdapterViewModel ConvertUpperAdapter(AdapterProfile? ap, MultiToolViewModel multiTool)
	{
		return new LowerAdapterViewModel(_unitConverter, _toolStorage, multiTool, ap.WorkingHeight)
		{
			Name = ap.Name,
			MaxAllowableToolForcePerLengthUnit = ap.MaximumAllowableForcepermeter,
			Implemented = Convert.ToBoolean(ap.Implemented),
			MultiTool = 
			{
				Geometry = _importHelper.Get2DGeometry(ap.GeometryFile.name)
			}
		};
	}

	private LowerToolPieceViewModel ConvertLowerAdapterPart(AdapterPart? dpa, MultiToolViewModel mutliTool)
	{
		LowerToolPieceViewModel lowerToolPieceViewModel = _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, mutliTool, _importHelper.MakeValidFileName(dpa.Name), dpa.Length, dpa.Amount);
		lowerToolPieceViewModel.MaxAllowableToolForce = dpa.MaximumAllowableForceVal;
		string name = dpa.GeometryFile.name;
		if (name != null && name.Any() && dpa.GeometryFile.blob.Any())
		{
			lowerToolPieceViewModel.Geometry3D = Load3DGeometry(dpa.GeometryFile.name);
		}
		return lowerToolPieceViewModel;
	}

	private LowerAdapterViewModel ConverLowerAdapter(AdapterProfile? ap, MultiToolViewModel multiTool)
	{
		return new LowerAdapterViewModel(_unitConverter, _toolStorage, multiTool, ap.WorkingHeight)
		{
			Name = ap.Name,
			MaxAllowableToolForcePerLengthUnit = ap.MaximumAllowableForcepermeter,
			Implemented = Convert.ToBoolean(ap.Implemented),
			MultiTool = 
			{
				Geometry = _importHelper.Get2DGeometry(ap.GeometryFile.name)
			}
		};
	}

	private LowerToolPieceViewModel ConvertDiePiece(DiePart diePart, LowerToolViewModel lowerTool, MultiToolViewModel multiTool)
	{
		if (diePart.maximumAllowableForce == "")
		{
			diePart.maximumAllowableForce = "0.0";
		}
		LowerToolPieceViewModel lowerToolPieceViewModel = _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(diePart.name), _importHelper.SpecialConvertToDouble(diePart.length), diePart.amount);
		lowerToolPieceViewModel.MaxAllowableToolForce = _importHelper.SpecialTryConvertToDouble(diePart.maximumAllowableForce);
		if (Convert.ToBoolean(diePart.implemented))
		{
			lowerTool.Implemented = true;
		}
		string name = diePart.geometryFile.name;
		if (name != null && name.Any() && diePart.geometryFile.blob.Any())
		{
			lowerToolPieceViewModel.Geometry3D = Load3DGeometry(diePart.geometryFile.name);
		}
		return lowerToolPieceViewModel;
	}

	private LowerToolViewModel ConvertDie(DieProfile dieProfile, MultiToolViewModel multiTool, LowerToolGroupViewModel group)
	{
		string name = dieProfile.geometryFile.name;
		if (name.Split('/').Length > 1)
		{
			name = name.Split('/')[1];
		}
		string name2 = dieProfile.geometryFile.name;
		LowerToolViewModel obj = _machineTools.CreateLowerToolProfile(multiTool, group, VWidthTypes.Undefined, dieProfile.name, dieProfile.maximumAllowableForcepermeter, vWidth: dieProfile.vWidth, vAngle: dieProfile.angle, cornerRadius: dieProfile.cornerRadius, offsetInX: dieProfile.offsetInX, workingHeight: dieProfile.workingHeight);
		obj.Implemented = Convert.ToBoolean(dieProfile.implemented);
		obj.StoneFactor = dieProfile.stoneFactor;
		obj.MultiTool.Geometry = _importHelper.Get2DGeometry(name2);
		return obj;
	}

	private UpperToolPieceViewModel ConvertPunchPiece(UpperToolViewModel upperToolVM, PunchPart punchPart, MultiToolViewModel multiTool)
	{
		UpperToolPieceViewModel upperToolPieceViewModel = _machineTools.CreateUpperToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(punchPart.name), punchPart.length, punchPart.amount);
		upperToolPieceViewModel.HasHeelLeft = Convert.ToBoolean(punchPart.boolHeelLeft);
		upperToolPieceViewModel.HasHeelRight = Convert.ToBoolean(punchPart.boolHeelRight);
		upperToolPieceViewModel.IsAngleMeasurementTool = Convert.ToBoolean(punchPart.boolAngleMeasurementPart);
		upperToolPieceViewModel.MaxAllowableToolForce = punchPart.maximumAllowableForce;
		if (Convert.ToBoolean(punchPart.implemented))
		{
			upperToolVM.Implemented = true;
		}
		string name = punchPart.geometryFile.name;
		if (name != null && name.Any() && punchPart.geometryFile.blob.Any())
		{
			upperToolPieceViewModel.Geometry3D = Load3DGeometry(punchPart.geometryFile.name);
		}
		return upperToolPieceViewModel;
	}

	private UpperToolViewModel ConvertPunch(PunchProfile punchProfile, MultiToolViewModel multiTool, UpperToolGroupViewModel upperGroup)
	{
		string name = punchProfile.geometryFile.name;
		if (name.Split('/').Length > 1)
		{
			name = name.Split('/')[1];
		}
		UpperToolViewModel obj = _machineTools.CreateUpperToolProfile(multiTool, upperGroup, workingHeight: punchProfile.workingHeight, angleRad: punchProfile.angle, radius: punchProfile.radius, name: punchProfile.name, maxAllowableToolForcePerLengthUnit: punchProfile.maximumAllowableForcepermeter, hemOffsetX: 0.0, widthHemmingFace: 0.0);
		obj.Implemented = punchProfile.implemented;
		obj.MultiTool.Geometry = _importHelper.Get2DGeometry(punchProfile.geometryFile.name);
		return obj;
	}

	private Model Load3DGeometry(string fileName)
	{
		if (!string.IsNullOrEmpty(fileName))
		{
			switch (Path.GetExtension(fileName).ToLower())
			{
			case ".stl":
			{
				Model model = StlLoader.LoadStl(fileName);
				_importHelper.FixStlOrientation(model, -Math.PI / 2.0, -Math.PI / 2.0);
				_importHelper.CalculateShell0Edges(model, 40.0);
				return model;
			}
			}
		}
		return null;
	}
}
