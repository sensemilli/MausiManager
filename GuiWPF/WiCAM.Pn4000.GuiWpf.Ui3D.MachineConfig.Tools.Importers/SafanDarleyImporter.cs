using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Xml;
using Telerik.Windows.Diagrams.Core;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.PnGeometry;
using WiCAM.Pn4000.BendModel.Loader.Loader;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.UiElements;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;

public class SafanDarleyImporter : IHemImporter, IToolImporter, IHolderImporter, IAdapterImporter
{
	private class ToolStockItem
	{
		public int Length { get; set; }

		public int TotalStockCount { get; set; }

		public int UsedStockCount { get; set; }

		public bool HasStartHeel { get; set; }

		public bool HasEndHeel { get; set; }
	}

	private class ExtensionStockItem
	{
		public int Length { get; set; }

		public int TotalStockCount { get; set; }

		public int UsedStockCount { get; set; }
	}

	protected readonly IUnitConverter _unitConverter;

	protected readonly IGlobalToolStorage _toolStorage;

	protected readonly IModelFactory _modelFactory;

	protected readonly ImportHelper _importHelper;

	protected readonly IConfigProvider _configProvider;

	protected MachineToolsViewModel _machineTools;

	public SafanDarleyImporter(IUnitConverter unitConverter, IGlobalToolStorage toolStorage, IModelFactory modelFactory, ImportHelper importHelper, IConfigProvider configProvider)
	{
		_unitConverter = unitConverter;
		_toolStorage = toolStorage;
		_modelFactory = modelFactory;
		_importHelper = importHelper;
		_configProvider = configProvider;
	}

	public string GetFilterString()
	{
		return Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.SafanDarley") as string;
	}

	public void Init(MachineToolsViewModel machineTools)
	{
		_machineTools = machineTools;
	}

	public void ImportDies(string file)
	{
		Import(file);
	}

	public void ImportPunches(string file)
	{
		Import(file);
	}

	public void ImportHems(string file)
	{
		Import(file);
	}

	public void ImportLowerHolders(string file)
	{
		Import(file);
	}

	public void ImportUpperHolders(string file)
	{
		Import(file);
	}

	public void ImportLowerAdapters(string file)
	{
		Import(file);
	}

	public void ImportUpperAdapters(string file)
	{
		Import(file);
	}

	public bool Import(string file)
	{
		ZipArchive zipArchive = ZipFile.OpenRead(file);
		if (zipArchive == null)
		{
			return false;
		}
		ZipArchiveEntry zipArchiveEntry = zipArchive.Entries.SingleOrDefault((ZipArchiveEntry x) => x.Name == "LocalToolStock.xml");
		if (zipArchiveEntry == null)
		{
			return false;
		}
		XmlDocument xmlDocument = new XmlDocument();
		using (Stream inStream = zipArchiveEntry.Open())
		{
			xmlDocument.Load(inStream);
		}
		IEnumerable<ZipArchiveEntry> enumerable = zipArchive.Entries.Where((ZipArchiveEntry entry) => entry.FullName.Contains("tools") && entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
		SelectItemPopup selectItemPopup = new SelectItemPopup(enumerable.Select((ZipArchiveEntry x) => new SelectableItem
		{
			FilePath = x.FullName,
			FileName = x.Name,
			ToolType = Path.GetFileName(Path.GetDirectoryName(x.FullName))
		}).ToList());
		if (selectItemPopup.ShowDialog() != true)
		{
			return false;
		}
		HashSet<string> hashSet = selectItemPopup.SelectedItems.Select((SelectableItem x) => x.FilePath).ToHashSet();
		foreach (ZipArchiveEntry item in enumerable)
		{
			if (hashSet.Contains(item.FullName))
			{
				using Stream inStream2 = item.Open();
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.Load(inStream2);
				Import(zipArchive, xmlDocument2, item.FullName, xmlDocument);
			}
		}
		return true;
	}

	private bool ImportSections(XmlDocument toolStockXml, string toolName, out List<ToolStockItem> toolStockItems, out List<ExtensionStockItem> extensionStockItems)
	{
		XmlNode xmlNode = toolStockXml.SelectSingleNode("//ToolStockItemGroup[ToolTypeName='" + toolName + "']");
		XmlNode xmlNode2 = toolStockXml.SelectSingleNode("//ExtensionStockItemGroup[ExtensionTypeName='" + toolName + "']");
		toolStockItems = new List<ToolStockItem>();
		extensionStockItems = new List<ExtensionStockItem>();
		if (xmlNode != null)
		{
			foreach (XmlNode item in xmlNode.SelectNodes("Items/ToolStockItem"))
			{
				ToolStockItem toolStockItem = new ToolStockItem();
				toolStockItem.Length = ParseInt(item, "Length").Value;
				toolStockItem.TotalStockCount = ParseInt(item, "TotalStockCount").Value;
				toolStockItem.UsedStockCount = ParseInt(item, "UsedStockCount").Value;
				toolStockItem.HasStartHeel = ParseBool(item, "HasStartHeel").Value;
				toolStockItem.HasEndHeel = ParseBool(item, "HasEndHeel").Value;
				toolStockItems.Add(toolStockItem);
			}
		}
		if (xmlNode2 != null)
		{
			foreach (XmlNode item2 in xmlNode2.SelectNodes("Items/ExtensionStockItem"))
			{
				ExtensionStockItem extensionStockItem = new ExtensionStockItem();
				extensionStockItem.Length = ParseInt(item2, "Length").Value;
				extensionStockItem.TotalStockCount = ParseInt(item2, "TotalStockCount").Value;
				extensionStockItem.UsedStockCount = ParseInt(item2, "UsedStockCount").Value;
				extensionStockItems.Add(extensionStockItem);
			}
		}
		if (xmlNode2 == null)
		{
			return xmlNode != null;
		}
		return true;
	}

	private bool Import(ZipArchive zip, XmlDocument toolXml, string path, XmlDocument toolStockXml)
	{
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(toolXml.NameTable);
		xmlNamespaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
		XmlNode xmlNode = toolXml.SelectSingleNode("PressBrakeToolDescription[@xsi:type='PressBrakeDieDescription']", xmlNamespaceManager);
		XmlNode xmlNode2 = toolXml.SelectSingleNode("PressBrakeToolDescription[@xsi:type='PressBrakePunchDescription']", xmlNamespaceManager);
		XmlNode xmlNode3 = toolXml.SelectSingleNode("PressBrakeToolExtensionDescription[@xsi:type='PressBrakeDieExtensionDescription']", xmlNamespaceManager);
		XmlNode xmlNode4 = toolXml.SelectSingleNode("PressBrakeToolExtensionDescription[@xsi:type='PressBrakePunchExtensionDescription']", xmlNamespaceManager);
		string plugCoupling = "";
		List<ExtensionStockItem> extensionStockItems;
		double maxY;
		if (xmlNode != null)
		{
			double? radius = ParseDouble(xmlNode, "Radius");
			double? vAngle = ParseDouble(xmlNode, "Angle");
			double? vWidth = ParseDouble(xmlNode, "VWidth");
			double? num = ParseDouble(xmlNode, "Height");
			double? num2 = ParseDouble(xmlNode, "MaximumAllowedForce");
			string text = xmlNode.SelectSingleNode("Name")?.InnerText;
			ImportSections(toolStockXml, text, out List<ToolStockItem> toolStockItems, out extensionStockItems);
			MultiToolViewModel multiToolViewModel = _machineTools.CreateMultiTool(null);
			ICadGeo geometry = ParseXmlContour(xmlNode, xmlNamespaceManager, out maxY);
			multiToolViewModel.Name = text;
			multiToolViewModel.SetGeometryData(geometry, Path.GetFileNameWithoutExtension(path));
			multiToolViewModel.PlugCoupling = plugCoupling;
			multiToolViewModel.PlugId = _machineTools.LowerBeamMountTypeId;
			LowerToolGroupViewModel lowerToolGroupViewModel = _machineTools.LowerGroups.FirstOrDefault((LowerToolGroupViewModel x) => Math.Abs((x.Radius - radius).GetValueOrDefault()) < 0.001 && Math.Abs((x.VWidth - vWidth).GetValueOrDefault()) < 0.001 && Math.Abs((x.VAngle - vAngle?.ToRadians()).GetValueOrDefault()) < 0.001 && x.CornerType == CornerType.Default);
			if (lowerToolGroupViewModel == null)
			{
				lowerToolGroupViewModel = _machineTools.CreateLowerToolGroup(Math.Round(vWidth.Value, 3), Math.Round(vAngle.Value.ToRadians(), 3), Math.Round(radius.Value.ToRadians(), 3), $"W{Math.Round(vWidth.Value, 3)} A{Math.Round(vAngle.Value, 3)}");
			}
			lowerToolGroupViewModel.CornerType = CornerType.Default;
			LowerToolViewModel obj = _machineTools.CreateLowerToolProfile(multiToolViewModel, lowerToolGroupViewModel, VWidthTypes.ALvdDelem, text, num2.Value, 0.0, cornerRadius: radius, vAngle: vAngle.Value.ToRadians(), vWidth: vWidth, workingHeight: num.Value);
			obj.FlippingAllowed = AllowedFlippedStates.Both;
			obj.FlippedByDefault = false;
			obj.PlugInstallationDirection = InstallationDirection.Both;
			foreach (ToolStockItem item3 in toolStockItems)
			{
				LowerToolPieceViewModel lowerToolPieceViewModel = _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, multiToolViewModel, _importHelper.MakeValidFileName(""), item3.Length, item3.TotalStockCount);
				lowerToolPieceViewModel.Geometry3D = null;
				lowerToolPieceViewModel.MaxAllowableToolForce = 0.0;
				_machineTools.LowerPieces.Add(lowerToolPieceViewModel);
			}
			return true;
		}
		if (xmlNode2 != null)
		{
			double? radius2 = ParseDouble(xmlNode2, "Radius");
			double? num3 = ParseDouble(xmlNode2, "Angle");
			double? num4 = ParseDouble(xmlNode2, "MaximumAllowedForce");
			string text2 = xmlNode2.SelectSingleNode("Name")?.InnerText;
			double? num5 = ParseDouble(xmlNode2, "Height");
			double? num6 = ParseDouble(xmlNode2, "HeelWidth");
			double? num7 = ParseDouble(xmlNode2, "HeelStartDistanceFromTip");
			double? num8 = ParseDouble(xmlNode2, "HeelEndDistanceFromTip");
			double? num9 = ParseDouble(xmlNode2, "HeelChamferHeightFilletRadius");
			string text3 = xmlNode2.SelectSingleNode("HeelGeometryType")?.InnerText;
			ImportSections(toolStockXml, text2, out List<ToolStockItem> toolStockItems2, out extensionStockItems);
			UpperToolGroupViewModel upperToolGroupViewModel = _machineTools.UpperGroups.FirstOrDefault((UpperToolGroupViewModel x) => Math.Abs((x.Radius - radius2).GetValueOrDefault()) < 0.001);
			MultiToolViewModel multiToolViewModel2 = _machineTools.CreateMultiTool(null);
			double maxY2;
			ICadGeo cadGeo = ParseXmlContour(xmlNode2, xmlNamespaceManager, out maxY2);
			multiToolViewModel2.Name = text2;
			multiToolViewModel2.SetGeometryData(cadGeo, Path.GetFileNameWithoutExtension(path));
			multiToolViewModel2.PlugCoupling = plugCoupling;
			multiToolViewModel2.PlugId = _machineTools.UpperBeamMountTypeId;
			if (upperToolGroupViewModel == null)
			{
				upperToolGroupViewModel = _machineTools.CreateUpperToolGroup($"R{Math.Round(radius2.Value, 3)}", Math.Round(radius2.Value, 3));
			}
			UpperToolViewModel upperToolViewModel = _machineTools.CreateUpperToolProfile(multiToolViewModel2, upperToolGroupViewModel, radius2, num3.Value.ToRadians(), num5.Value, text2, num4.Value, 0.0, 0.0);
			upperToolViewModel.FlippingAllowed = AllowedFlippedStates.Both;
			upperToolViewModel.FlippedByDefault = false;
			upperToolViewModel.PlugInstallationDirection = InstallationDirection.Both;
			foreach (ToolStockItem item4 in toolStockItems2)
			{
				Model resultModel = null;
				if (text3 == "Chamfer" && !item4.HasStartHeel)
				{
					_ = item4.HasEndHeel;
				}
				bool flag = ((text3 == "Fillet" || text3 == "Chamfer") ? true : false);
				if (flag && (item4.HasStartHeel || item4.HasEndHeel))
				{
					ICadGeo heelFront = CreateHeelGeometry(num6.Value, num7.Value, num8.Value, num9.Value, item4.Length, num5.Value, item4.HasStartHeel, item4.HasEndHeel, text3 == "Chamfer", maxY2);
					WzgLoader.GetHeelTool(cadGeo, heelFront, out resultModel, advancedExtrude: true);
				}
				UpperToolPieceViewModel upperToolPieceViewModel = _machineTools.CreateUpperToolPiece(_machineTools.SelectedToolList, multiToolViewModel2, _importHelper.MakeValidFileName((resultModel != null) ? text2 : ""), item4.Length, item4.TotalStockCount);
				upperToolPieceViewModel.Geometry3D = resultModel;
				upperToolPieceViewModel.MaxAllowableToolForce = 0.0;
				upperToolPieceViewModel.HasHeelLeft = item4.HasStartHeel;
				upperToolPieceViewModel.HasHeelRight = item4.HasEndHeel;
				_machineTools.UpperPieces.Add(upperToolPieceViewModel);
			}
			return true;
		}
		if (xmlNode3 != null || xmlNode4 != null)
		{
			XmlNode xmlNode5 = xmlNode3 ?? xmlNode4;
			string text4 = xmlNode5?.SelectSingleNode("Name")?.InnerText;
			ParseDouble(xmlNode5, "MaximumAllowedForce");
			double? num10 = ParseDouble(xmlNode5, "Height");
			if (text4 == null)
			{
				return false;
			}
			ImportSections(toolStockXml, text4, out List<ToolStockItem> _, out List<ExtensionStockItem> extensionStockItems2);
			MultiToolViewModel multiToolViewModel3 = _machineTools.CreateMultiTool(null);
			ICadGeo geometry2 = ParseXmlContour(xmlNode5, xmlNamespaceManager, out maxY);
			multiToolViewModel3.Name = text4;
			multiToolViewModel3.SetGeometryData(geometry2, Path.GetFileNameWithoutExtension(path));
			multiToolViewModel3.PlugCoupling = plugCoupling;
			if (xmlNode3 != null)
			{
				multiToolViewModel3.PlugId = _machineTools.LowerBeamMountTypeId;
				LowerAdapterViewModel lowerAdapterViewModel = new LowerAdapterViewModel(_unitConverter, _toolStorage, multiToolViewModel3)
				{
					Name = text4,
					SocketId = _machineTools.LowerBeamMountTypeId,
					SocketCoupling = text4,
					WorkingHeight = num10.Value,
					AdapterDirection = AdapterDirections.TopDown,
					Implemented = true
				};
				lowerAdapterViewModel.SocketInstallationDirection = InstallationDirection.NotRotated;
				lowerAdapterViewModel.PlugInstallationDirection = InstallationDirection.NotRotated;
				_machineTools.LowerAdapters.Add(lowerAdapterViewModel);
				foreach (ExtensionStockItem item5 in extensionStockItems2)
				{
					LowerToolPieceViewModel item = _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, multiToolViewModel3, _importHelper.MakeValidFileName(""), item5.Length, item5.TotalStockCount);
					_machineTools.LowerPieces.Add(item);
				}
			}
			else if (xmlNode4 != null)
			{
				multiToolViewModel3.PlugId = _machineTools.UpperBeamMountTypeId;
				UpperAdapterViewModel upperAdapterViewModel = new UpperAdapterViewModel(_unitConverter, _toolStorage, multiToolViewModel3)
				{
					Name = text4,
					SocketId = _machineTools.UpperBeamMountTypeId,
					SocketCoupling = text4,
					WorkingHeight = num10.Value,
					Implemented = true
				};
				upperAdapterViewModel.SocketInstallationDirection = InstallationDirection.NotRotated;
				upperAdapterViewModel.PlugInstallationDirection = InstallationDirection.NotRotated;
				_machineTools.UpperAdapters.Add(upperAdapterViewModel);
				foreach (ExtensionStockItem item6 in extensionStockItems2)
				{
					LowerToolPieceViewModel item2 = _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, multiToolViewModel3, _importHelper.MakeValidFileName(""), item6.Length, item6.TotalStockCount);
					_machineTools.LowerPieces.Add(item2);
				}
			}
			return true;
		}
		return false;
	}

	private ICadGeo CreateHeelGeometry(double heelWidth, double heelStartDistanceFromTip, double heelEndDistanceFromTip, double heelChamferHeightFilletRadius, double length, double height, bool heelLeft, bool heelRight, bool chamfer, double maxY)
	{
		List<GeoSegment2D> list = new List<GeoSegment2D>();
		Vector2d vector2d = new Vector2d(0.0, maxY);
		Vector2d vector2d2 = new Vector2d(length, maxY);
		Vector2d vector2d3 = new Vector2d(0.0, 0.0);
		Vector2d vector2d4 = new Vector2d(length, 0.0);
		Vector2d vector2d5 = new Vector2d(0.0, height);
		Vector2d vector2d6 = new Vector2d(length, height);
		if (heelLeft)
		{
			Vector2d vector2d7 = vector2d3 + heelStartDistanceFromTip * Vector2d.UnitY;
			Vector2d vector2d8 = vector2d7 + (heelWidth - heelChamferHeightFilletRadius) * Vector2d.UnitX;
			Vector2d vector2d9 = vector2d8 + heelChamferHeightFilletRadius * Vector2d.UnitY;
			Vector2d vector2d10 = vector2d9 + heelChamferHeightFilletRadius * Vector2d.UnitX;
			Vector2d vector2d11 = vector2d5 - heelEndDistanceFromTip * Vector2d.UnitY;
			Vector2d vector2d12 = vector2d11 + (heelWidth - heelChamferHeightFilletRadius) * Vector2d.UnitX;
			Vector2d vector2d13 = vector2d12 - heelChamferHeightFilletRadius * Vector2d.UnitY;
			Vector2d vector2d14 = vector2d13 + heelChamferHeightFilletRadius * Vector2d.UnitX;
			list.Add(new LineSegment2D(vector2d3, vector2d7));
			list.Add(new LineSegment2D(vector2d7, vector2d8));
			if (chamfer)
			{
				list.Add(new LineSegment2D(vector2d8, vector2d10));
			}
			else
			{
				list.Add(new CircleSegment2D(heelChamferHeightFilletRadius, vector2d9, vector2d8, vector2d10, isCCW: true, CircleSegmentCreationMode.KeepStartAndEndAndRadius));
			}
			list.Add(new LineSegment2D(vector2d10, vector2d14));
			if (chamfer)
			{
				list.Add(new LineSegment2D(vector2d14, vector2d12));
			}
			else
			{
				list.Add(new CircleSegment2D(heelChamferHeightFilletRadius, vector2d13, vector2d14, vector2d12, isCCW: true, CircleSegmentCreationMode.KeepStartAndEndAndRadius));
			}
			list.Add(new LineSegment2D(vector2d12, vector2d11));
			list.Add(new LineSegment2D(vector2d11, vector2d));
		}
		else
		{
			list.Add(new LineSegment2D(vector2d3, vector2d));
		}
		list.Add(new LineSegment2D(vector2d, vector2d2));
		if (heelRight)
		{
			Vector2d vector2d15 = vector2d4 + heelStartDistanceFromTip * Vector2d.UnitY;
			Vector2d vector2d16 = vector2d15 - (heelWidth - heelChamferHeightFilletRadius) * Vector2d.UnitX;
			Vector2d vector2d17 = vector2d16 + heelChamferHeightFilletRadius * Vector2d.UnitY;
			Vector2d vector2d18 = vector2d17 - heelChamferHeightFilletRadius * Vector2d.UnitX;
			Vector2d vector2d19 = vector2d6 - heelEndDistanceFromTip * Vector2d.UnitY;
			Vector2d vector2d20 = vector2d19 - (heelWidth - heelChamferHeightFilletRadius) * Vector2d.UnitX;
			Vector2d vector2d21 = vector2d20 - heelChamferHeightFilletRadius * Vector2d.UnitY;
			Vector2d vector2d22 = vector2d21 - heelChamferHeightFilletRadius * Vector2d.UnitX;
			list.Add(new LineSegment2D(vector2d2, vector2d19));
			list.Add(new LineSegment2D(vector2d19, vector2d20));
			if (chamfer)
			{
				list.Add(new LineSegment2D(vector2d20, vector2d22));
			}
			else
			{
				list.Add(new CircleSegment2D(heelChamferHeightFilletRadius, vector2d21, vector2d20, vector2d22, isCCW: true, CircleSegmentCreationMode.KeepStartAndEndAndRadius));
			}
			list.Add(new LineSegment2D(vector2d22, vector2d18));
			if (chamfer)
			{
				list.Add(new LineSegment2D(vector2d18, vector2d16));
			}
			else
			{
				list.Add(new CircleSegment2D(heelChamferHeightFilletRadius, vector2d17, vector2d18, vector2d16, isCCW: true, CircleSegmentCreationMode.KeepStartAndEndAndRadius));
			}
			list.Add(new LineSegment2D(vector2d16, vector2d15));
			list.Add(new LineSegment2D(vector2d15, vector2d4));
		}
		else
		{
			list.Add(new LineSegment2D(vector2d2, vector2d4));
		}
		list.Add(new LineSegment2D(vector2d4, vector2d3));
		List<CadGeoElement> cadGeoElements = new ContourConverter().ToCadGeoContour(list, 0);
		return ToCadGeoInfoBase(cadGeoElements);
	}

	private Vector2d? ParseVector2d(XmlNode node, string tag)
	{
		XmlNode node2 = node.SelectSingleNode(tag);
		double? num = ParseDouble(node2, "X");
		double? num2 = ParseDouble(node2, "Y");
		if (num.HasValue && num2.HasValue)
		{
			return new Vector2d(num.Value, num2.Value);
		}
		return null;
	}

	private double? ParseDouble(XmlNode node, string tag)
	{
		double? result = null;
		XmlNode xmlNode = node.SelectSingleNode(tag);
		if (xmlNode != null && xmlNode.InnerText != null)
		{
			result = _importHelper.SpecialConvertToDouble(xmlNode.InnerText);
		}
		return result;
	}

	private int? ParseInt(XmlNode node, string tag)
	{
		int? result = null;
		XmlNode xmlNode = node.SelectSingleNode(tag);
		if (xmlNode != null && xmlNode.InnerText != null && int.TryParse(xmlNode?.InnerText, out var result2))
		{
			result = result2;
		}
		return result;
	}

	private bool? ParseBool(XmlNode node, string tag)
	{
		bool? result = null;
		XmlNode xmlNode = node.SelectSingleNode(tag);
		if (xmlNode != null && xmlNode.InnerText != null)
		{
			result = xmlNode?.InnerText == "true";
		}
		return result;
	}

	private ICadGeo ParseXmlContour(XmlNode node, XmlNamespaceManager ns, out double maxY)
	{
		List<GeoSegment2D> list = new List<GeoSegment2D>();
		foreach (XmlNode item in node.SelectNodes(".//ContourItem"))
		{
			string text = item.Attributes["type", ns.LookupNamespace("xsi")]?.Value;
			if (text == "ContourLine")
			{
				Vector2d? vector2d = ParseVector2d(item, "StartPoint");
				Vector2d? vector2d2 = ParseVector2d(item, "EndPoint");
				if (vector2d.HasValue && vector2d2.HasValue)
				{
					list.Add(new LineSegment2D(vector2d.Value, vector2d2.Value));
				}
			}
			else if (text == "ContourArc")
			{
				Vector2d? vector2d3 = ParseVector2d(item, "StartPoint");
				Vector2d? vector2d4 = ParseVector2d(item, "EndPoint");
				Vector2d? vector2d5 = ParseVector2d(item, "CenterPoint");
				string text2 = item.SelectSingleNode("Direction")?.InnerText;
				bool? flag = ((text2 != null) ? new bool?(text2.ToLower() == "cw") : null);
				string text3 = item.SelectSingleNode("IsLargeArc")?.InnerText;
				if (text3 != null)
				{
					_ = text3 == "true";
				}
				double? num = (vector2d3 - vector2d5)?.Length;
				if (vector2d3.HasValue && vector2d4.HasValue && vector2d5.HasValue && flag.HasValue && num.HasValue)
				{
					list.Add(new CircleSegment2D(num.Value, vector2d5.Value, vector2d3.Value, vector2d4.Value, !flag.Value, CircleSegmentCreationMode.KeepStartAndEndAndRadius));
				}
			}
		}
		maxY = (list.Any() ? list.Max((GeoSegment2D x) => Math.Max(x.Start.Y, x.End.Y)) : 0.0);
		List<CadGeoElement> cadGeoElements = new ContourConverter().ToCadGeoContour(list, 0);
		return ToCadGeoInfoBase(cadGeoElements);
	}

	private ICadGeo ToCadGeoInfoBase(List<CadGeoElement> cadGeoElements)
	{
		ICadGeo cadGeo = new CadGeoInfoBase();
		foreach (CadGeoElement cadGeoElement in cadGeoElements)
		{
			if (cadGeoElement is CadGeoLine line)
			{
				AddCadGeoLineElement(cadGeo, line);
			}
			else if (cadGeoElement is CadGeoCircle circle)
			{
				AddCadGeoCircleElement(cadGeo, circle);
			}
		}
		return cadGeo;
	}

	private void AddCadGeoLineElement(ICadGeo cadGeoInfoBase, CadGeoLine line)
	{
		cadGeoInfoBase.AddElement(new GeoLineInfo
		{
			PnColor = 2,
			GroupElementNumber = 0,
			ElementNumber = cadGeoInfoBase.GeoElements.Count,
			ContourType = 1,
			GeoType = GeoElementType.Line,
			X0 = line.StartPoint.X,
			Y0 = line.StartPoint.Y,
			X1 = line.EndPoint.X,
			Y1 = line.EndPoint.Y
		});
	}

	private void AddCadGeoCircleElement(ICadGeo cadGeoInfoBase, CadGeoCircle circle)
	{
		cadGeoInfoBase.AddElement(new GeoArcInfo
		{
			PnColor = 2,
			GroupElementNumber = 0,
			ElementNumber = cadGeoInfoBase.GeoElements.Count,
			ContourType = 1,
			GeoType = GeoElementType.Ellips,
			Direction = circle.Direction,
			BeginAngle = circle.StartAngle,
			EndAngle = circle.EndAngle,
			X0 = circle.Center.X,
			Y0 = circle.Center.Y,
			Radius = circle.Radius,
			Diameter = 2.0 * circle.Radius
		});
	}
}
