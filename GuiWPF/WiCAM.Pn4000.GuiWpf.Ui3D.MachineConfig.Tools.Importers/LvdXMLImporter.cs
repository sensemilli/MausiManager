using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.LvdConfig;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.LvdConfig.Die;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.LvdConfig.Holder;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.LvdConfig.Punch;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.Modifiers;
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

public class LvdXMLImporter : IToolImporter, IHolderImporter
{
	private readonly IUnitConverter _unitConverter;

	private readonly IGlobalToolStorage _toolStorage;

	private readonly IModelFactory _modelFactory;

	private readonly ImportHelper _importHelper;

	private readonly IConfigProvider _configProvider;

	private MachineToolsViewModel _machineTools;

	public LvdXMLImporter(IUnitConverter unitConverter, IGlobalToolStorage toolStorage, IModelFactory modelFactory, ImportHelper importHelper, IConfigProvider configProvider)
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
		return Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.LvdXml") as string;
	}

	public void ImportDies(string filePath)
	{
		CADMANDie cADMANDie = Xml.DeserializeFromXml<CADMANDie>(filePath);
		List<PBDIESEGMENTTYPE> source = cADMANDie.PBDIESEGMENTTYPE ?? new List<PBDIESEGMENTTYPE>();
		foreach (PBDIETYPE item in cADMANDie.PBDIETYPE ?? new List<PBDIETYPE>())
		{
			LowerToolGroupViewModel matchingLowerGroup = _machineTools.GetMatchingLowerGroup(item.AirbendingBend?.VWidth ?? 0.0, item.AirbendingBend?.Angle ?? 0.0);
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(item.ID));
			ConvertDie(item, multiTool, matchingLowerGroup);
			LvdBase.SegmentListItem[] sEGMENTLIST = item.SEGMENTLIST;
			foreach (LvdBase.SegmentListItem segment in sEGMENTLIST)
			{
				PBDIESEGMENTTYPE seg = source.FirstOrDefault((PBDIESEGMENTTYPE s) => s.ID == segment.SegmentID);
				_machineTools.LowerPieces.Add(ConvertDiePiece(segment, seg, multiTool));
			}
		}
	}

	public void ImportPunches(string filePath)
	{
		CADMANPunch cADMANPunch = Xml.DeserializeFromXml<CADMANPunch>(filePath);
		if (cADMANPunch.PBPUNCHSECTIONFREESHAPE == null)
		{
			new List<PBPUNCHSECTIONFREESHAPE>();
		}
		if (cADMANPunch.PBPUNCHHORNSECTIONFREESHAPE == null)
		{
			new List<PBPUNCHHORNSECTIONFREESHAPE>();
		}
		List<PBPUNCHSEGMENTTYPE> source = cADMANPunch.PBPUNCHSEGMENTTYPE ?? new List<PBPUNCHSEGMENTTYPE>();
		List<PBPUNCHTYPE> obj = cADMANPunch.PBPUNCHTYPE ?? new List<PBPUNCHTYPE>();
		List<PBPUNCHTYPE> list = new List<PBPUNCHTYPE>();
		foreach (PBPUNCHTYPE item in obj)
		{
			UpperToolGroupViewModel matchingUpperGroup = _machineTools.GetMatchingUpperGroup(item.AirbendingBend.Radius);
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(item.ID));
			ConvertPunch(item, multiTool, matchingUpperGroup);
			LvdBase.SegmentListItem[] sEGMENTLIST = item.SEGMENTLIST;
			foreach (LvdBase.SegmentListItem segment in sEGMENTLIST)
			{
				PBPUNCHSEGMENTTYPE seg = source.FirstOrDefault((PBPUNCHSEGMENTTYPE s) => s.ID == segment.SegmentID);
				_machineTools.UpperPieces.Add(ConvertPunchPiece(seg, segment, multiTool));
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Current version not support tools without AirbendingBend.");
		stringBuilder.AppendLine("Follow item(s) have not been converted:");
		foreach (PBPUNCHTYPE item2 in list)
		{
			stringBuilder.AppendLine(item2.ID);
		}
		MessageBox.Show(stringBuilder.ToString());
	}

	public void ImportLowerHolders(string filePath)
	{
		CADMANDieHolder cADMANDieHolder = Xml.DeserializeFromXml<CADMANDieHolder>(filePath);
		PBDIEHOLDERTYPE holderType = cADMANDieHolder.PBDIEHOLDERTYPE;
		if (holderType == null || cADMANDieHolder.PBDIEHOLDERSEGMENTTYPE == null)
		{
			MessageBox.Show("Error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(holderType.ID));
		_machineTools.LowerAdapters.Add(ConvertLowerAdapter(cADMANDieHolder, multiTool));
		foreach (PBDIEHOLDERSEGMENTTYPE item in cADMANDieHolder.PBDIEHOLDERSEGMENTTYPE.Where((PBDIEHOLDERSEGMENTTYPE s) => s.ID.StartsWith(holderType.ID)))
		{
			_machineTools.LowerPieces.Add(ConvertLowerAdapterPiece(item, multiTool));
		}
	}

	public void ImportUpperHolders(string filePath)
	{
		CADMANPunchHolder cADMANPunchHolder = Xml.DeserializeFromXml<CADMANPunchHolder>(filePath);
		PBPUNCHHOLDERTYPE holderType = cADMANPunchHolder.PBPUNCHHOLDERTYPE;
		if (holderType == null || cADMANPunchHolder.PBPUNCHHOLDERSEGMENTTYPE == null)
		{
			MessageBox.Show("Error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(holderType.ID));
		_machineTools.UpperAdapters.Add(ConverUpperAdapter(cADMANPunchHolder, multiTool));
		foreach (PBPUNCHHOLDERSEGMENTTYPE item in cADMANPunchHolder.PBPUNCHHOLDERSEGMENTTYPE.Where((PBPUNCHHOLDERSEGMENTTYPE s) => s.ID.StartsWith(holderType.ID)))
		{
			_machineTools.UpperPieces.Add(ConvertUpperAdapterPiece(item, multiTool));
		}
	}

	private UpperToolPieceViewModel ConvertUpperAdapterPiece(PBPUNCHHOLDERSEGMENTTYPE segment, MultiToolViewModel multiTool)
	{
		return _machineTools.CreateUpperToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(segment.ID), _importHelper.SpecialConvertToDouble(segment.ID.Split('-')[1]), segment.Amount);
	}

	private UpperAdapterViewModel ConverUpperAdapter(CADMANPunchHolder holder, MultiToolViewModel multiTool)
	{
		PBPUNCHHOLDERTYPE pBPUNCHHOLDERTYPE = holder.PBPUNCHHOLDERTYPE;
		UpperAdapterViewModel upperAdapterViewModel = new UpperAdapterViewModel(_unitConverter, _toolStorage, multiTool, pBPUNCHHOLDERTYPE.Height - pBPUNCHHOLDERTYPE.ToothHeight)
		{
			Name = pBPUNCHHOLDERTYPE.ID,
			MaxAllowableToolForcePerLengthUnit = (int)pBPUNCHHOLDERTYPE.MaxForceLength,
			Implemented = true
		};
		if (pBPUNCHHOLDERTYPE.SectionModel.Items.Count() > 0)
		{
			multiTool.Geometry = ConvertLvdToUpperCadGeo(pBPUNCHHOLDERTYPE.SectionModel, upperAdapterViewModel.WorkingHeight);
		}
		return upperAdapterViewModel;
	}

	private LowerToolPieceViewModel ConvertLowerAdapterPiece(PBDIEHOLDERSEGMENTTYPE segment, MultiToolViewModel multiTool)
	{
		return _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(segment.ID), _importHelper.SpecialConvertToDouble(segment.ID.Split('-')[1]), segment.Amount);
	}

	private LowerAdapterViewModel ConvertLowerAdapter(CADMANDieHolder holder, MultiToolViewModel multiTool)
	{
		PBDIEHOLDERTYPE pBDIEHOLDERTYPE = holder.PBDIEHOLDERTYPE;
		holder.PBDIEHOLDERSEGMENTTYPE.ToList();
		LowerAdapterViewModel lowerAdapterViewModel = new LowerAdapterViewModel(_unitConverter, _toolStorage, multiTool, pBDIEHOLDERTYPE.Height - pBDIEHOLDERTYPE.ToothHeight)
		{
			Name = pBDIEHOLDERTYPE.ID,
			MaxAllowableToolForcePerLengthUnit = (int)pBDIEHOLDERTYPE.MaxForceLength,
			Implemented = true
		};
		if (pBDIEHOLDERTYPE.SectionModel.Items.Count() > 0)
		{
			multiTool.Geometry = ConvertLvdToLowerCadGeo(pBDIEHOLDERTYPE.SectionModel, lowerAdapterViewModel.WorkingHeight);
		}
		return lowerAdapterViewModel;
	}

	private UpperToolPieceViewModel ConvertPunchPiece(PBPUNCHSEGMENTTYPE? seg, LvdBase.SegmentListItem? segment, MultiToolViewModel multiTool)
	{
		bool flag = seg.GEOMETRY3D.PBPUNCH3DFROMSECTION.LeftHornRev > -1;
		bool flag2 = seg.GEOMETRY3D.PBPUNCH3DFROMSECTION.RightHornRev > -1;
		UpperToolPieceViewModel upperToolPieceViewModel = _machineTools.CreateUpperToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(segment.SegmentID), _importHelper.SpecialConvertToDouble((flag || flag2) ? segment.SegmentID.Split('-')[segment.SegmentID.Split('-').Length - 2] : segment.SegmentID.Split('-').Last()), seg?.Amount ?? 0);
		upperToolPieceViewModel.HasHeelLeft = flag;
		upperToolPieceViewModel.HasHeelRight = flag2;
		return upperToolPieceViewModel;
	}

	private UpperToolViewModel ConvertPunch(PBPUNCHTYPE? punchType, MultiToolViewModel multiTool, UpperToolGroupViewModel group)
	{
		PBPUNCHTYPEBENDBOTTOMING pBPUNCHTYPEBENDBOTTOMING = punchType.BottomingBends.FirstOrDefault();
		UpperToolViewModel upperToolViewModel = _machineTools.CreateUpperToolProfile(multiTool, group, punchType.AirbendingBend.Radius, punchType.AirbendingBend.Angle, punchType.Height - punchType.ToothHeight, punchType.ID, punchType.MaxForceLength, pBPUNCHTYPEBENDBOTTOMING?.XOffset ?? 0.0, 0.0);
		upperToolViewModel.MultiTool.Geometry = ConvertLvdToUpperCadGeo(punchType.SectionModel, upperToolViewModel.WorkingHeight);
		return upperToolViewModel;
	}

	private LowerToolPieceViewModel ConvertDiePiece(LvdBase.SegmentListItem segment, PBDIESEGMENTTYPE? seg, MultiToolViewModel multiTool)
	{
		return _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(segment.SegmentID), _importHelper.SpecialConvertToDouble(segment.SegmentID.Split('-')[1]), seg?.Amount ?? 0);
	}

	private LowerToolViewModel ConvertDie(PBDIETYPE? dieType, MultiToolViewModel multiTool, LowerToolGroupViewModel group)
	{
		MachineToolsViewModel machineTools = _machineTools;
		string iD = dieType.ID;
		double maxForceLength = dieType.MaxForceLength;
		double? vWidth = dieType.AirbendingBend?.VWidth ?? 0.0;
		double? vAngle = dieType.AirbendingBend?.Angle ?? 0.0;
		double? cornerRadius = dieType.AirbendingBend?.RollRadius ?? 0.0;
		LowerToolViewModel lowerToolViewModel = machineTools.CreateLowerToolProfile(multiTool, group, VWidthTypes.ALvdDelem, iD, maxForceLength, 0.0, dieType.Height, vWidth, vAngle, null, cornerRadius);
		LvdBase.LVDGEOELEM[] items = dieType.SectionModel.Items;
		if (items != null && items.Count() > 0)
		{
			lowerToolViewModel.MultiTool.Geometry = GetLvdLowerGeometry(dieType.SectionModel, dieType.Height, lowerToolViewModel.VAngle.Value, lowerToolViewModel.CornerRadius.Value, lvdToolCorrection: false);
		}
		return lowerToolViewModel;
	}

	private ICadGeo GetLvdLowerGeometry(LvdBase.ModelSection geometryElements, double workingHeight, double vAngle, double cornerRadius, bool lvdToolCorrection)
	{
		if (lvdToolCorrection)
		{
			CorrectLeadInCurve(geometryElements, vAngle, cornerRadius);
		}
		return ConvertLvdToLowerCadGeo(geometryElements, workingHeight);
	}

	private static ICadGeo ConvertLvdToUpperCadGeo(LvdBase.ModelSection geometryElements, double workingHeight)
	{
		ICadGeo cadGeo = new CadGeoInfoBase();
		foreach (LvdBase.LINE item in geometryElements.Items.Where((LvdBase.LVDGEOELEM i) => i is LvdBase.LINE))
		{
			GeoElementInfo gi = new GeoLineInfo
			{
				PnColor = 2,
				GroupElementNumber = 0,
				ElementNumber = cadGeo.GeoElements.Count,
				ContourType = 1,
				GeoType = GeoElementType.Line,
				X0 = item.Start.X,
				Y0 = item.Start.Y + workingHeight,
				X1 = item.End.X,
				Y1 = item.End.Y + workingHeight
			};
			cadGeo.AddElement(gi);
		}
		foreach (LvdBase.ARC item2 in geometryElements.Items.Where((LvdBase.LVDGEOELEM i) => i is LvdBase.ARC))
		{
			Vector3d vector3d = new Vector3d(item2.Center.X, item2.Center.Y, 0.0);
			Vector3d vector3d2 = new Vector3d(item2.Start.X, item2.Start.Y, 0.0);
			Vector3d vector3d3 = new Vector3d(item2.Mid.X, item2.Mid.Y, 0.0);
			Vector3d vector3d4 = new Vector3d(item2.End.X, item2.End.Y, 0.0);
			double length = (vector3d3 - vector3d).Length;
			double num = Math.Atan2(vector3d2.Y - vector3d.Y, vector3d2.X - vector3d.X);
			double num2 = Math.Atan2(vector3d4.Y - vector3d.Y, vector3d4.X - vector3d.X);
			int direction = (((vector3d3 - vector3d).SignedAngle(vector3d2 - vector3d, new Vector3d(0.0, 0.0, 1.0)) < 0.0) ? 1 : (-1));
			GeoElementInfo gi2 = new GeoArcInfo
			{
				PnColor = 2,
				GroupElementNumber = 0,
				ElementNumber = cadGeo.GeoElements.Count,
				ContourType = 1,
				GeoType = GeoElementType.Ellips,
				Direction = direction,
				BeginAngle = num * 180.0 / Math.PI,
				EndAngle = num2 * 180.0 / Math.PI,
				X0 = vector3d.X,
				Y0 = vector3d.Y + workingHeight,
				Radius = length,
				Diameter = 2.0 * length
			};
			cadGeo.AddElement(gi2);
		}
		return cadGeo;
	}

	private static ICadGeo ConvertLvdToLowerCadGeo(LvdBase.ModelSection geometryElements, double workingHeight)
	{
		ICadGeo cadGeo = new CadGeoInfoBase();
		foreach (LvdBase.LINE item in geometryElements.Items.Where((LvdBase.LVDGEOELEM i) => i is LvdBase.LINE))
		{
			GeoElementInfo gi = new GeoLineInfo
			{
				PnColor = 2,
				GroupElementNumber = 0,
				ElementNumber = cadGeo.GeoElements.Count,
				ContourType = 1,
				GeoType = GeoElementType.Line,
				X0 = item.Start.X,
				Y0 = item.Start.Y - workingHeight,
				X1 = item.End.X,
				Y1 = item.End.Y - workingHeight
			};
			cadGeo.AddElement(gi);
		}
		foreach (LvdBase.ARC item2 in geometryElements.Items.Where((LvdBase.LVDGEOELEM i) => i is LvdBase.ARC))
		{
			Vector3d vector3d = new Vector3d(item2.Center.X, item2.Center.Y, 0.0);
			Vector3d vector3d2 = new Vector3d(item2.Start.X, item2.Start.Y, 0.0);
			Vector3d vector3d3 = new Vector3d(item2.Mid.X, item2.Mid.Y, 0.0);
			Vector3d vector3d4 = new Vector3d(item2.End.X, item2.End.Y, 0.0);
			int direction = (((vector3d2.X - vector3d.X) * (vector3d4.Y - vector3d.Y) - (vector3d2.Y - vector3d.Y) * (vector3d4.X - vector3d.X) > 0.0) ? 1 : (-1));
			double length = (vector3d3 - vector3d).Length;
			double num = Math.Atan2(vector3d2.Y - vector3d.Y, vector3d2.X - vector3d.X);
			double num2 = Math.Atan2(vector3d4.Y - vector3d.Y, vector3d4.X - vector3d.X);
			GeoElementInfo gi2 = new GeoArcInfo
			{
				PnColor = 2,
				GroupElementNumber = 0,
				ElementNumber = cadGeo.GeoElements.Count,
				ContourType = 1,
				GeoType = GeoElementType.Ellips,
				Direction = direction,
				BeginAngle = num * 180.0 / Math.PI,
				EndAngle = num2 * 180.0 / Math.PI,
				X0 = vector3d.X,
				Y0 = vector3d.Y - workingHeight,
				Radius = length,
				Diameter = 2.0 * length
			};
			cadGeo.AddElement(gi2);
		}
		return cadGeo;
	}

	private static void CorrectLeadInCurve(LvdBase.ModelSection geometryElements, double vAngle, double cornerRadius)
	{
		int num = -1;
		for (int i = 0; i < geometryElements.Items.Length; i++)
		{
			if (geometryElements.Items[i] is LvdBase.ARC && Math.Abs((geometryElements.Items[i] as LvdBase.ARC).Mid.X) <= 1E-06)
			{
				if (num != -1)
				{
					return;
				}
				num = i;
			}
		}
		List<LvdBase.ARC> list = new List<LvdBase.ARC>();
		List<LvdBase.ARC> list2 = new List<LvdBase.ARC>();
		bool flag = false;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			if (geometryElements.Items[num2] is LvdBase.ARC)
			{
				flag = true;
				list.Add(geometryElements.Items[num2] as LvdBase.ARC);
			}
			else if (flag)
			{
				break;
			}
		}
		flag = false;
		for (int j = num + 1; j < geometryElements.Items.Length; j++)
		{
			if (geometryElements.Items[j] is LvdBase.ARC)
			{
				flag = true;
				list2.Add(geometryElements.Items[j] as LvdBase.ARC);
			}
			else if (flag)
			{
				break;
			}
		}
		if (list.Count != list2.Count || list.Count == 0)
		{
			return;
		}
		LvdBase.Point2D point2D = new LvdBase.Point2D();
		LvdBase.Point2D point2D2 = point2D;
		double x = (point2D.Y = double.MaxValue);
		point2D2.X = x;
		LvdBase.Point2D point2D3 = new LvdBase.Point2D();
		LvdBase.Point2D point2D4 = point2D3;
		x = (point2D3.Y = double.MinValue);
		point2D4.X = x;
		foreach (LvdBase.ARC item in list)
		{
			if (item.Start.X < point2D.X)
			{
				point2D = item.Start;
			}
			if (item.End.X < point2D.X)
			{
				point2D = item.End;
			}
			if (item.Start.X > point2D3.X)
			{
				point2D3 = item.Start;
			}
			if (item.End.X > point2D3.X)
			{
				point2D3 = item.End;
			}
		}
		double num5 = 1.2;
		double wantedPrecision = 0.01;
		double alpha = Math.Atan(num5);
		double num6 = CalcX(cornerRadius, num5, (90.0 - vAngle / 2.0) * (Math.PI / 180.0), alpha);
		double value = CalcY(cornerRadius, num5, (90.0 - vAngle / 2.0) * (Math.PI / 180.0), alpha);
		LvdBase.Point2D point2D5 = new LvdBase.Point2D(double.MinValue, point2D.Y);
		point2D5.X = point2D3.X - Math.Abs(point2D5.Y - point2D3.Y) / Math.Tan((90.0 - vAngle / 2.0) * (Math.PI / 180.0));
		double num7 = point2D5.X + Math.Abs(value) / Math.Tan((90.0 - vAngle / 2.0) * (Math.PI / 180.0));
		double startX = num7 - num6;
		double y = point2D.Y;
		LvdBase.LVDGEOELEM[] array = ReapproximateWithLines(vAngle, cornerRadius, alpha, num5, startX, num7, y, wantedPrecision, geometryElements.Items.ToList(), list, list2);
		if (array != null)
		{
			geometryElements.Items = array;
		}
	}

	private static double CalcX(double piAngle, double m, double phi, double alpha)
	{
		return piAngle / Math.Sqrt(1.0 + Math.Pow(m, 2.0)) * (Math.Pow(Math.E, m * phi) * Math.Sin(phi) * Math.Cos(alpha) + (Math.Pow(Math.E, m * phi) * Math.Cos(phi) - 1.0) * Math.Sin(alpha));
	}

	private static double CalcY(double piAngle, double m, double phi, double alpha)
	{
		return piAngle / Math.Sqrt(1.0 + Math.Pow(m, 2.0)) * ((0.0 - Math.Pow(Math.E, m * phi)) * Math.Sin(phi) * Math.Sin(alpha) + (Math.Pow(Math.E, m * phi) * Math.Cos(phi) - 1.0) * Math.Cos(alpha));
	}

	private static LvdBase.LVDGEOELEM[] ReapproximateWithLines(double betaAngle, double piAngle, double alpha, double m, double startX, double endX, double startY, double wantedPrecision, List<LvdBase.LVDGEOELEM> oldGeometry, List<LvdBase.ARC> leftLeadInCurve, List<LvdBase.ARC> rightLeadInCurve)
	{
		int num = 10000;
		double num2 = (90.0 - betaAngle / 2.0) * (Math.PI / 180.0) / (double)num;
		LinkedList<Vector2d> linkedList = new LinkedList<Vector2d>();
		for (int i = 0; i <= num; i++)
		{
			double phi = num2 * (double)i;
			double x = CalcX(piAngle, m, phi, alpha);
			double y = CalcY(piAngle, m, phi, alpha);
			linkedList.AddLast(new Vector2d(x, y));
		}
		LinkedListNode<Vector2d> linkedListNode = linkedList.First;
		while (linkedListNode != linkedList.Last && linkedListNode.Next != linkedList.Last)
		{
			LinkedListNode<Vector2d> linkedListNode2;
			for (linkedListNode2 = linkedListNode.Next; linkedListNode2 != linkedList.Last; linkedListNode2 = linkedListNode2.Next)
			{
				double num3 = double.MinValue;
				for (LinkedListNode<Vector2d> next = linkedListNode.Next; next != linkedListNode2; next = next.Next)
				{
					num3 = Math.Max(num3, Math.Abs(next.Value.GetSignedDistanceFromLine(linkedListNode.Value, linkedListNode2.Value)));
				}
				if (num3 >= wantedPrecision)
				{
					linkedListNode2 = linkedListNode2.Previous;
					break;
				}
			}
			while (linkedListNode != linkedListNode2 && linkedListNode.Next != linkedListNode2)
			{
				linkedList.Remove(linkedListNode.Next);
			}
			linkedListNode = linkedListNode.Next;
		}
		if (linkedList.Count > 0)
		{
			List<LvdBase.LVDGEOELEM> list = new List<LvdBase.LVDGEOELEM>();
			bool flag = false;
			bool flag2 = false;
			for (int j = 0; j < oldGeometry.Count; j++)
			{
				if (!flag && oldGeometry[j + 1] is LvdBase.ARC)
				{
					LvdBase.ARC item = oldGeometry[j + 1] as LvdBase.ARC;
					if (leftLeadInCurve.Contains(item))
					{
						LvdBase.LINE lINE = new LvdBase.LINE();
						lINE.Start = (oldGeometry[j] as LvdBase.LINE).Start;
						lINE.End = new LvdBase.Point2D(linkedList.First.Value.X + startX, linkedList.First.Value.Y + startY);
						list.Add(lINE);
						do
						{
							j++;
						}
						while (oldGeometry[j] is LvdBase.ARC);
						for (LinkedListNode<Vector2d> linkedListNode3 = linkedList.First; linkedListNode3 != linkedList.Last; linkedListNode3 = linkedListNode3.Next)
						{
							LvdBase.LINE lINE2 = new LvdBase.LINE();
							lINE2.Start = new LvdBase.Point2D(linkedListNode3.Value.X + startX, linkedListNode3.Value.Y + startY);
							lINE2.End = new LvdBase.Point2D(linkedListNode3.Next.Value.X + startX, linkedListNode3.Next.Value.Y + startY);
							list.Add(lINE2);
						}
						LvdBase.LINE lINE3 = new LvdBase.LINE();
						lINE3.Start = new LvdBase.Point2D(linkedList.Last.Value.X + startX, linkedList.Last.Value.Y + startY);
						lINE3.End = (oldGeometry[j] as LvdBase.LINE).End;
						list.Add(lINE3);
						j++;
						flag = true;
					}
				}
				if (!flag2 && oldGeometry[j + 1] is LvdBase.ARC)
				{
					LvdBase.ARC item2 = oldGeometry[j + 1] as LvdBase.ARC;
					if (rightLeadInCurve.Contains(item2))
					{
						LvdBase.LINE lINE4 = new LvdBase.LINE();
						lINE4.Start = (oldGeometry[j] as LvdBase.LINE).Start;
						lINE4.End = new LvdBase.Point2D(0.0 - (linkedList.Last.Value.X + startX), linkedList.Last.Value.Y + startY);
						list.Add(lINE4);
						do
						{
							j++;
						}
						while (oldGeometry[j] is LvdBase.ARC);
						for (LinkedListNode<Vector2d> linkedListNode4 = linkedList.Last; linkedListNode4 != linkedList.First; linkedListNode4 = linkedListNode4.Previous)
						{
							LvdBase.LINE lINE5 = new LvdBase.LINE();
							lINE5.Start = new LvdBase.Point2D(0.0 - (linkedListNode4.Value.X + startX), linkedListNode4.Value.Y + startY);
							lINE5.End = new LvdBase.Point2D(0.0 - (linkedListNode4.Previous.Value.X + startX), linkedListNode4.Previous.Value.Y + startY);
							list.Add(lINE5);
						}
						LvdBase.LINE lINE6 = new LvdBase.LINE();
						lINE6.Start = new LvdBase.Point2D(0.0 - (linkedList.First.Value.X + startX), linkedList.First.Value.Y + startY);
						lINE6.End = (oldGeometry[j] as LvdBase.LINE).End;
						list.Add(lINE6);
						j++;
						flag2 = true;
					}
				}
				if (j < oldGeometry.Count)
				{
					list.Add(oldGeometry[j]);
				}
			}
			if (flag && flag2)
			{
				return list.ToArray();
			}
		}
		return null;
	}
}
