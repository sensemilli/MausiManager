using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.BystronicConfig.Adapter;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.BystronicConfig.Die;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.BystronicConfig.Geometry;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.BystronicConfig.Holder;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.BystronicConfig.Punch;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.HealCadGeo;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.Modifiers;
using Telerik.Windows.Diagrams.Core;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;

public class BystronicXMLImporter : IToolImporter, IAdapterImporter, IHolderImporter
{
	private readonly IUnitConverter _unitConverter;

	private readonly IGlobalToolStorage _toolStorage;

	private readonly IModelFactory _modelFactory;

	private readonly ImportHelper _importHelper;

	private readonly IConfigProvider _configProvider;

	private MachineToolsViewModel _machineTools;

	public BystronicXMLImporter(IUnitConverter unitConverter, IGlobalToolStorage toolStorage, IModelFactory modelFactory, ImportHelper importHelper, IConfigProvider configProvider)
	{
		_unitConverter = unitConverter;
		_toolStorage = toolStorage;
		_modelFactory = modelFactory;
		_importHelper = importHelper;
		_configProvider = configProvider;
	}

	public void Init(MachineToolsViewModel machineTools)
	{
		_machineTools = machineTools;
	}

	public string GetFilterString()
	{
		return Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.BystronicXml") as string;
	}

	public void ImportDies(string filePath)
	{
		BystronicDie bystronicDie = Xml.DeserializeFromXml<BystronicDie>(filePath);
		LowerTool[] array = bystronicDie.LowerTools ?? bystronicDie.LowerToolsAlternative;
		foreach (LowerTool lowerTool in array)
		{
			string text = lowerTool.Name;
			if (text.Split('/').Length > 1)
			{
				text = text.Split('/')[1];
			}
			LowerToolGroupViewModel matchingLowerGroup = _machineTools.GetMatchingLowerGroup(lowerTool.OpeningWidth, lowerTool.MinBendAngle);
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(text));
			ConvertDie(lowerTool, multiTool, matchingLowerGroup);
			if (lowerTool.HemmingAllowed == 1)
			{
				LowerToolGroupViewModel group = _machineTools.CreateLowerToolGroup();
				ConvertHemDie(lowerTool, multiTool, group);
			}
			if (lowerTool.MaxLoad.ToString() == "")
			{
				lowerTool.MaxLoad = 0.0;
			}
			IEnumerable<DieSegment> enumerable = lowerTool.Segments?.Segments;
			foreach (DieSegment item in enumerable ?? Enumerable.Empty<DieSegment>())
			{
				_machineTools.LowerPieces.Add(ConvertDiePiece(item, lowerTool, multiTool));
			}
		}
	}

	public void ImportPunches(string filePath)
	{
		BystronicPunch bystronicPunch = Xml.DeserializeFromXml<BystronicPunch>(filePath);
		UpperTool[] array = bystronicPunch.UpperTools ?? bystronicPunch.UpperToolsAlternative;
		foreach (UpperTool upperTool in array)
		{
			string text = upperTool.Name;
			if (text.Split('/').Length > 1)
			{
				text = text.Split('/')[1];
			}
			UpperToolGroupViewModel matchingUpperGroup = _machineTools.GetMatchingUpperGroup(upperTool.Radius);
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(text));
			ConvertPunch(upperTool, multiTool, matchingUpperGroup);
			if (upperTool.HemmingAllowed == 1)
			{
				UpperToolGroupViewModel upperGroup = _machineTools.CreateUpperToolGroup();
				ConvertHemPunch(upperTool, multiTool, upperGroup);
			}
			IEnumerable<PunchSegment> enumerable = upperTool.Segments?.Segments;
			foreach (PunchSegment item in enumerable ?? Enumerable.Empty<PunchSegment>())
			{
				_machineTools.UpperPieces.Add(ConvertPunchPiece(item, upperTool, multiTool));
			}
		}
	}

	public void ImportLowerAdapters(string filePath)
	{
		AdapterPart[] dieAdapters = Xml.DeserializeFromXml<BystronicAdapter>(filePath).DieAdapters;
		foreach (AdapterPart adapterPart in dieAdapters)
		{
			MultiToolViewModel multiToolViewModel = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(adapterPart.Name));
			_machineTools.LowerAdapters.Add(ConverLowerAdapter(adapterPart, multiToolViewModel));
			multiToolViewModel.Geometry = GetBystronicGeometry(adapterPart.Vertices);
		}
	}

	public void ImportUpperAdapters(string filePath)
	{
		AdapterPart[] punchAdapters = Xml.DeserializeFromXml<BystronicAdapter>(filePath).PunchAdapters;
		foreach (AdapterPart adapterPart in punchAdapters)
		{
			MultiToolViewModel multiToolViewModel = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(adapterPart.Name));
			_machineTools.UpperAdapters.Add(ConvertUpperAdapter(adapterPart, multiToolViewModel));
			multiToolViewModel.Geometry = GetBystronicGeometry(adapterPart.Vertices);
		}
	}

	public void ImportUpperHolders(string filePath)
	{
		PunchHolder[] holders = Xml.DeserializeFromXml<BystronicPunchHolder>(filePath).Holders;
		foreach (PunchHolder punchHolder in holders)
		{
			MultiToolViewModel multiToolViewModel = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(punchHolder.Name));
			_machineTools.UpperAdapters.Add(ConvertUpperHolder(punchHolder, multiToolViewModel));
			multiToolViewModel.Geometry = GetBystronicGeometry(punchHolder.Vertices);
		}
	}

	public void ImportLowerHolders(string filePath)
	{
		DieHolder[] holders = Xml.DeserializeFromXml<BystronicDieHolder>(filePath).Holders;
		foreach (DieHolder dieHolder in holders)
		{
			MultiToolViewModel multiToolViewModel = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(dieHolder.Name));
			_machineTools.LowerAdapters.Add(ConvertLowerHolder(dieHolder, multiToolViewModel));
			multiToolViewModel.Geometry = GetBystronicGeometry(dieHolder.Vertices);
		}
	}

	private LowerAdapterViewModel ConvertLowerHolder(DieHolder? holder, MultiToolViewModel multiTool)
	{
		return new LowerAdapterViewModel(_unitConverter, _toolStorage, multiTool, holder.Height)
		{
			Name = holder.Name,
			Implemented = true
		};
	}

	private UpperAdapterViewModel ConvertUpperHolder(PunchHolder? holder, MultiToolViewModel multiTool)
	{
		return new UpperAdapterViewModel(_unitConverter, _toolStorage, multiTool, holder.Height)
		{
			Name = holder.Name,
			Implemented = true
		};
	}

	private UpperAdapterViewModel ConvertUpperAdapter(AdapterPart adapter, MultiToolViewModel multiTool)
	{
		return new UpperAdapterViewModel(_unitConverter, _toolStorage, multiTool, adapter.Height)
		{
			Name = adapter.Name
		};
	}

	private LowerAdapterViewModel ConverLowerAdapter(AdapterPart adapter, MultiToolViewModel multiTool)
	{
		return new LowerAdapterViewModel(_unitConverter, _toolStorage, multiTool, adapter.Height)
		{
			Name = adapter.Name
		};
	}

	private UpperToolPieceViewModel ConvertPunchPiece(PunchSegment? punchSegment, UpperTool? upperTool, MultiToolViewModel multiTool)
	{
		return _machineTools.CreateUpperToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(upperTool.Name), punchSegment.Length, 1);
	}

	private LowerToolPieceViewModel ConvertDiePiece(DieSegment? dieSegment, LowerTool? lowerTool, MultiToolViewModel multiTool)
	{
		return _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(lowerTool.Name), dieSegment.Length, 1);
	}

	private UpperToolViewModel ConvertPunch(UpperTool? upperTool, MultiToolViewModel multiTool, UpperToolGroupViewModel upperGroup)
	{
		string name = upperTool.Name;
		if (name.Split('/').Length > 1)
		{
			name = name.Split('/')[1];
		}
		UpperToolViewModel upperToolViewModel = _machineTools.CreateUpperToolProfile(multiTool, upperGroup, upperTool.Radius, upperTool.MinBendAngle.ToRadians(), upperTool.Height, upperTool.Name, upperTool.MaxLoad, 0.0, 0.0);
		upperToolViewModel.MultiTool.Geometry = GetBystronicGeometry(upperTool.Vertices);
		return upperToolViewModel;
	}

	private UpperToolViewModel ConvertHemPunch(UpperTool? upperTool, MultiToolViewModel multiTool, UpperToolGroupViewModel upperGroup)
	{
		string name = upperTool.Name;
		if (name.Split('/').Length > 1)
		{
			name = name.Split('/')[1];
		}
		UpperToolViewModel upperToolViewModel = _machineTools.CreateUpperToolProfile(multiTool, upperGroup, null, null, upperTool.HemmingHeight, upperTool.Name, upperTool.MaxHemmingLoad, 0.0, 0.0);
		upperToolViewModel.MultiTool.Geometry = GetBystronicGeometry(upperTool.Vertices);
		upperToolViewModel.IsFoldTool = true;
		Anchor anchor = upperTool.Anchors.FirstOrDefault((Anchor x) => x.AnchorType == 3);
		if (anchor != null)
		{
			upperToolViewModel.HemOffsetXMm = anchor.X;
			Vector2d pHem = new Vector2d(anchor.X, anchor.Y);
			Vertex vertex = upperTool.Vertices.MinBy((Vertex x) => (new Vector2d(x.X, x.Y) - pHem).Length);
			int num = upperTool.Vertices.IndexOf(vertex);
			Vertex vertex2 = upperTool.Vertices[(num + 1) % upperTool.Vertices.Count];
			if (vertex.Y == vertex2.Y && vertex.X > vertex2.X)
			{
				upperToolViewModel.WidthHemmingFaceMm = vertex.X - vertex2.X;
			}
			else
			{
				Vertex vertex3 = upperTool.Vertices[(num - 1 + upperTool.Vertices.Count) % upperTool.Vertices.Count];
				if (vertex.Y == vertex3.Y && vertex.X > vertex3.X)
				{
					upperToolViewModel.WidthHemmingFaceMm = vertex.X - vertex3.X;
				}
			}
		}
		return upperToolViewModel;
	}

	private LowerToolViewModel ConvertDie(LowerTool? lowerTool, MultiToolViewModel multiTool, LowerToolGroupViewModel group)
	{
		string name = lowerTool.Name;
		if (name.Split('/').Length > 1)
		{
			name = name.Split('/')[1];
		}
		LowerToolViewModel obj = _machineTools.CreateLowerToolProfile(multiTool, group, VWidthTypes.ALvdDelem, cornerRadius: lowerTool.OpeningRadius, name: lowerTool.Name, maxAllowableToolForcePerLengthUnit: lowerTool.MaxLoad, workingHeight: lowerTool.Height, vWidth: lowerTool.OpeningWidth, vAngle: lowerTool.OpeningAngle.ToRadians(), offsetInX: lowerTool.OpeningOffset);
		obj.MultiTool.Geometry = GetBystronicGeometry(lowerTool.Vertices);
		return obj;
	}

	private LowerToolViewModel ConvertHemDie(LowerTool? lowerTool, MultiToolViewModel multiTool, LowerToolGroupViewModel group)
	{
		string name = lowerTool.Name;
		if (name.Split('/').Length > 1)
		{
			name = name.Split('/')[1];
		}
		LowerToolViewModel obj = _machineTools.CreateLowerToolProfile(multiTool, group, VWidthTypes.Undefined, lowerTool.Name, lowerTool.MaxHemmingLoad, workingHeight: lowerTool.Height, offsetInX: lowerTool.Anchors.FirstOrDefault((Anchor x) => x.AnchorType == 3)?.X ?? 0.0);
		obj.MultiTool.Geometry = GetBystronicGeometry(lowerTool.Vertices);
		obj.IsFoldTool = true;
		return obj;
	}

	private static ICadGeo GetBystronicGeometry(IReadOnlyList<Vertex> vertices)
	{
		ICadGeo cadGeo = new CadGeoInfoBase();
		for (int i = 0; i < vertices.Count - 1; i++)
		{
			GetCadGeoElement(vertices[i], vertices[i + 1], cadGeo);
		}
		if (vertices.Last().X != vertices.First().X || vertices.Last().Y != vertices.First().Y)
		{
			GetCadGeoElement(vertices.Last(), vertices.First(), cadGeo);
		}
		HealCadGeo_.Heal(cadGeo);
		return cadGeo;
	}

	private static void GetCadGeoElement(Vertex firstVertex, Vertex secondVertex, ICadGeo cadGeoInfoBase)
	{
		if (Math.Abs(firstVertex.B) < 0.0001)
		{
			cadGeoInfoBase.AddElement(new GeoLineInfo
			{
				PnColor = 2,
				GroupElementNumber = 0,
				ElementNumber = cadGeoInfoBase.GeoElements.Count,
				ContourType = 1,
				GeoType = GeoElementType.Line,
				X0 = firstVertex.X,
				Y0 = firstVertex.Y,
				X1 = secondVertex.X,
				Y1 = secondVertex.Y
			});
			return;
		}
		double b = firstVertex.B;
		double length = (new Vector3d(secondVertex.X, secondVertex.Y, 0.0) - new Vector3d(firstVertex.X, firstVertex.Y, 0.0)).Length;
		Vector3d vector3d = new Vector3d((secondVertex.X + firstVertex.X) / 2.0, (secondVertex.Y + firstVertex.Y) / 2.0, 0.0);
		double num = Math.Atan(b) * 4.0;
		double num2 = Math.Abs(length / (2.0 * Math.Sin(num / 2.0)));
		double num3 = Math.Abs(num2 * (1.0 - Math.Cos(num / 2.0)));
		Vector3d vector3d2 = new Vector3d(secondVertex.X, secondVertex.Y, 0.0) - new Vector3d(firstVertex.X, firstVertex.Y, 0.0);
		Vector3d vector3d3 = ((b > 0.0) ? new Vector3d(0.0 - vector3d2.Y, vector3d2.X, 0.0) : new Vector3d(vector3d2.Y, 0.0 - vector3d2.X, 0.0));
		vector3d3.Normalize();
		double num4 = Math.Abs(num2 - num3);
		Vector3d vector3d4 = vector3d + vector3d3 * num4;
		double num5 = Math.Atan2(firstVertex.Y - vector3d4.Y, firstVertex.X - vector3d4.X);
		double num6 = Math.Atan2(secondVertex.Y - vector3d4.Y, secondVertex.X - vector3d4.X);
		int direction = ((!(b < 0.0)) ? 1 : (-1));
		cadGeoInfoBase.AddElement(new GeoArcInfo
		{
			PnColor = 2,
			GroupElementNumber = 0,
			ElementNumber = cadGeoInfoBase.GeoElements.Count,
			ContourType = 1,
			GeoType = GeoElementType.Ellips,
			Direction = direction,
			BeginAngle = num5 * 180.0 / Math.PI,
			EndAngle = num6 * 180.0 / Math.PI,
			X0 = vector3d4.X,
			Y0 = vector3d4.Y,
			Radius = num2,
			Diameter = 2.0 * num2
		});
	}
}
