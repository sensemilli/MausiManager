using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Xml;
using BendDataSourceModel.DeepCopy;
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
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;

public class TrumpfBtdImporter : IHemImporter, IToolImporter, IHolderImporter, IAdapterImporter
{
	protected readonly IUnitConverter _unitConverter;

	protected readonly IGlobalToolStorage _toolStorage;

	protected readonly IModelFactory _modelFactory;

	protected readonly ImportHelper _importHelper;

	protected readonly IConfigProvider _configProvider;

	protected MachineToolsViewModel _machineTools;

	public TrumpfBtdImporter(IUnitConverter unitConverter, IGlobalToolStorage toolStorage, IModelFactory modelFactory, ImportHelper importHelper, IConfigProvider configProvider)
	{
		_unitConverter = unitConverter;
		_toolStorage = toolStorage;
		_modelFactory = modelFactory;
		_importHelper = importHelper;
		_configProvider = configProvider;
	}

	public string GetFilterString()
	{
		return Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.TrumpfBtd") as string;
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
		ZipArchiveEntry entry = zipArchive.GetEntry("BendingToolType.xml");
		if (entry == null)
		{
			return false;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(entry.Open());
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
		xmlNamespaceManager.AddNamespace("ns", "http://www.at.trumpf.com/BendingToolDescription");
		XmlNode xmlNode = xmlDocument.SelectSingleNode("//ns:BendingToolType", xmlNamespaceManager);
		XmlNode xmlNode2 = xmlNode.SelectSingleNode("ns:BoundingBox/ns:Minimum", xmlNamespaceManager);
		XmlNode xmlNode3 = xmlNode.SelectSingleNode("ns:BoundingBox/ns:Maximum", xmlNamespaceManager);
		XmlNode xmlNode4 = xmlNode.SelectSingleNode("ns:DisplayTransformationPose/ns:Rotation", xmlNamespaceManager);
		XmlNodeList xmlNodeList = xmlNode.SelectSingleNode("ns:PlugList", xmlNamespaceManager)?.ChildNodes;
		XmlNodeList xmlNodeList2 = xmlNode.SelectSingleNode("ns:SocketList", xmlNamespaceManager)?.ChildNodes;
		XmlNode xmlNode5 = xmlNode.SelectSingleNode("ns:ProfileGeometryList/ns:ProfileGeometry", xmlNamespaceManager) ?? xmlNode.SelectSingleNode("ns:ProfileGeometry", xmlNamespaceManager);
		XmlNodeList xmlNodeList3 = xmlNode.SelectNodes("ns:SegmentList/ns:Segment", xmlNamespaceManager);
		XmlNodeList xmlNodeList4 = xmlNode.SelectNodes("ns:FormingList/ns:DieForming", xmlNamespaceManager);
		XmlNodeList xmlNodeList5 = xmlNode.SelectNodes("ns:FormingList/ns:TractrixDieForming", xmlNamespaceManager);
		XmlNodeList xmlNodeList6 = xmlNode.SelectNodes("ns:FormingList/ns:PunchForming", xmlNamespaceManager);
		XmlNodeList xmlNodeList7 = xmlNode.SelectNodes("ns:FormingList/ns:FoldingForming", xmlNamespaceManager);
		XmlNodeList xmlNodeList8 = xmlNode.SelectNodes("ns:FormingList/ns:RollBendForming", xmlNamespaceManager);
		xmlNode.SelectNodes("ns:FormingList/ns:ZBendingForming", xmlNamespaceManager);
		int.TryParse(xmlNode.SelectSingleNode("ns:TechnologyValuesList/ns:TechnologyValue/ns:FatigueLoad", xmlNamespaceManager)?.InnerText, out var result);
		string value = xmlNode.Attributes["Name"].Value;
		string value2 = xmlNode.Attributes["Classification"].Value;
		double num = _importHelper.SpecialConvertToDouble(xmlNode3.InnerText.Split(' ')[0]);
		double num2 = _importHelper.SpecialConvertToDouble(xmlNode2.InnerText.Split(' ')[0]);
		double num3 = _importHelper.SpecialConvertToDouble(xmlNode2.InnerText.Split(' ')[1]);
		double num4 = _importHelper.SpecialConvertToDouble(xmlNode3.InnerText.Split(' ')[1]);
		double num5 = (num + num2) * 0.5;
		if (value2.Contains("Holder") && xmlNodeList2.Count > 0)
		{
			num4 = _importHelper.SpecialConvertToDouble(xmlNodeList2[0].SelectSingleNode("ns:Position", xmlNamespaceManager).InnerText.Split(" ")[1]);
		}
		double num6 = num4 - num3;
		ICadGeo cadGeo = ReadSvg(zipArchive, xmlNode5.InnerText);
		bool flag = true;
		if (xmlNode4 != null)
		{
			string[] array = xmlNode4.InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (array[0] == "1" && array[3] == "180")
			{
				flag = false;
			}
		}
		else if (value2.Contains("Punch"))
		{
			flag = false;
		}
		if (flag)
		{
			cadGeo.RotateAll(Math.PI);
		}
		if (value2 == "FlatteningBar")
		{
			cadGeo.Mirror();
		}
		cadGeo.MoveAll(0.0, flag ? (0.0 - num4) : num4);
		bool flag2 = false;
		bool flag3 = false;
		string plugName = "";
		bool isPlugFaltingBar = false;
		double? plugAngleRad = null;
		double? plugRadiusMm = null;
		if (xmlNodeList != null)
		{
			foreach (XmlNode item4 in xmlNodeList)
			{
				plugName = item4.Attributes["Coupling"].Value;
				if (plugName == "FLATTENINGBAR")
				{
					isPlugFaltingBar = true;
				}
				if (item4.SelectSingleNode("ns:Rotation", xmlNamespaceManager) != null)
				{
					flag3 = true;
				}
				else
				{
					flag2 = true;
				}
				XmlNode xmlNode7 = item4.SelectSingleNode("ns:Angle", xmlNamespaceManager);
				if (xmlNode7 != null)
				{
					plugAngleRad = _importHelper.SpecialConvertToDouble(xmlNode7.InnerText).ToRadians();
				}
				XmlNode xmlNode8 = item4.SelectSingleNode("ns:Radius", xmlNamespaceManager);
				if (xmlNode8 != null)
				{
					plugRadiusMm = _importHelper.SpecialConvertToDouble(xmlNode8.InnerText);
				}
				_ = item4.Attributes["Orientation"]?.Value == "Rotated";
			}
		}
		AllowedFlippedStates allowedFlippedStates = ((!(flag2 && flag3)) ? ((!flag2) ? AllowedFlippedStates.OnlyFlipped : AllowedFlippedStates.OnlyNotFlipped) : AllowedFlippedStates.Both);
		bool flippedByDefault = flag3 && !flag2;
		MultiToolViewModel multiToolViewModel = _machineTools.CreateMultiTool(null);
		multiToolViewModel.SetGeometryData(cadGeo, Path.GetFileNameWithoutExtension(xmlNode5.InnerText));
		multiToolViewModel.Name = value;
		multiToolViewModel.OffsetFrontMm = num2;
		multiToolViewModel.OffsetBackMm = num;
		multiToolViewModel.PlugCoupling = plugName;
		multiToolViewModel.PlugAngleRad = plugAngleRad;
		multiToolViewModel.PlugRadiusMm = plugRadiusMm;
		string side1 = ((num5 < 0.0) ? "Front" : "Rear");
		string side2 = ((num5 < 0.0) ? "Rear" : "Front");
		MappingViewModel.MappingVm mappingVm = _machineTools.MountIdMappings.FirstOrDefault((MappingViewModel.MappingVm x) => x.PpName == plugName && (!isPlugFaltingBar || x.Profile.Desc == side1));
		if (mappingVm != null)
		{
			multiToolViewModel.PlugId = mappingVm.Profile.Id;
		}
		else
		{
			int num7 = _machineTools.MountIdItemVms.Select((MappingViewModel.ItemVm x) => x.Id).Order().LastOrDefault();
			MappingViewModel.ItemVm itemVm = new MappingViewModel.ItemVm
			{
				Id = num7 + 1
			};
			multiToolViewModel.PlugId = itemVm.Id;
			_machineTools.MountIdItemVms.Add(itemVm);
			_machineTools.MountIdMappings.Add(new MappingViewModel.MappingVm
			{
				Profile = itemVm,
				PpName = plugName
			});
		}
		MultiToolViewModel multiToolViewModel2 = null;
		if (isPlugFaltingBar && allowedFlippedStates == AllowedFlippedStates.Both)
		{
			ICadGeo geometry = cadGeo.Copy();
			multiToolViewModel2 = _machineTools.CreateMultiTool(null);
			multiToolViewModel2.SetGeometryData(geometry, Path.GetFileNameWithoutExtension(xmlNode5.InnerText));
			multiToolViewModel2.Name = value;
			multiToolViewModel2.OffsetFrontMm = 0.0 - num;
			multiToolViewModel2.OffsetBackMm = 0.0 - num2;
			multiToolViewModel2.PlugCoupling = plugName;
			multiToolViewModel2.PlugAngleRad = plugAngleRad;
			multiToolViewModel2.PlugRadiusMm = plugRadiusMm;
			MappingViewModel.MappingVm mappingVm2 = _machineTools.MountIdMappings.FirstOrDefault((MappingViewModel.MappingVm x) => x.PpName == plugName && x.Profile.Desc == side2);
			if (mappingVm2 != null)
			{
				multiToolViewModel2.PlugId = mappingVm2.Profile.Id;
			}
			else
			{
				int num8 = _machineTools.MountIdItemVms.Select((MappingViewModel.ItemVm x) => x.Id).Order().LastOrDefault();
				MappingViewModel.ItemVm itemVm2 = new MappingViewModel.ItemVm
				{
					Id = num8 + 1,
					Desc = side2
				};
				multiToolViewModel2.PlugId = itemVm2.Id;
				_machineTools.MountIdItemVms.Add(itemVm2);
				_machineTools.MountIdMappings.Add(new MappingViewModel.MappingVm
				{
					Profile = itemVm2,
					PpName = plugName
				});
			}
		}
		foreach (XmlNode item5 in xmlNodeList4)
		{
			XmlNode xmlNode10 = item5.SelectSingleNode("ns:Radius", xmlNamespaceManager);
			item5.SelectSingleNode("ns:Radii/ns:RearRadius", xmlNamespaceManager);
			XmlNode xmlNode11 = item5.SelectSingleNode("ns:Radii/ns:FrontRadius", xmlNamespaceManager);
			double rFront = 0.0;
			if (xmlNode11 != null)
			{
				rFront = _importHelper.SpecialConvertToDouble(xmlNode11.InnerText);
			}
			else
			{
				rFront = _importHelper.SpecialConvertToDouble(xmlNode10.InnerText);
			}
			XmlNode xmlNode12 = item5.SelectSingleNode("ns:Angle", xmlNamespaceManager);
			XmlNode xmlNode13 = item5.SelectSingleNode("ns:Angle/ns:FrontAngle", xmlNamespaceManager);
			XmlNode xmlNode14 = item5.SelectSingleNode("ns:Angle/ns:RearAngle", xmlNamespaceManager);
			double vAngle = 0.0;
			if (xmlNode13 != null && xmlNode14 != null)
			{
				double num9 = _importHelper.SpecialConvertToDouble(xmlNode13.InnerText);
				double num10 = _importHelper.SpecialConvertToDouble(xmlNode14.InnerText);
				vAngle = num9 + num10;
			}
			else
			{
				vAngle = _importHelper.SpecialConvertToDouble(xmlNode12.InnerText);
			}
			double vWidth = _importHelper.SpecialConvertToDouble(item5.SelectSingleNode("ns:Width", xmlNamespaceManager).InnerText);
			XmlNode xmlNode15 = item5.SelectSingleNode("ns:GrooveGroundDepth", xmlNamespaceManager);
			double? vDepth = null;
			if (xmlNode15 != null)
			{
				vDepth = _importHelper.SpecialConvertToDouble(xmlNode15.InnerText);
			}
			XmlNode xmlNode16 = item5.SelectSingleNode("ns:GrooveGroundRadius", xmlNamespaceManager);
			double? vRadius = null;
			if (xmlNode16 != null)
			{
				vRadius = _importHelper.SpecialConvertToDouble(xmlNode16.InnerText);
			}
			XmlNode xmlNode17 = item5.SelectSingleNode("ns:GrooveGroundWidth", xmlNamespaceManager);
			double? vGroundWidth = null;
			if (xmlNode17 != null)
			{
				vGroundWidth = _importHelper.SpecialConvertToDouble(xmlNode17.InnerText);
			}
			double workingHeight = _importHelper.SpecialConvertToDouble(item5.SelectSingleNode("ns:Position", xmlNamespaceManager).InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
			_importHelper.SpecialConvertToDouble(item5.SelectSingleNode("ns:Position", xmlNamespaceManager).InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]);
			LowerToolGroupViewModel lowerToolGroupViewModel = _machineTools.LowerGroups.FirstOrDefault((LowerToolGroupViewModel x) => x.Radius == rFront && x.VWidth == vWidth && Math.Abs(x.VAngle - vAngle.ToRadians()) < 1E-06);
			if (lowerToolGroupViewModel == null)
			{
				lowerToolGroupViewModel = _machineTools.CreateLowerToolGroup(vWidth, vAngle.ToRadians(), rFront, $"W{vWidth} A{vAngle}");
			}
			MachineToolsViewModel machineTools = _machineTools;
			LowerToolGroupViewModel lowerGroup = lowerToolGroupViewModel;
			double maxAllowableToolForcePerLengthUnit = result;
			double? cornerRadius = rFront;
			double? vAngle2 = vAngle.ToRadians();
			double? vWidth2 = vWidth;
			LowerToolViewModel lowerToolViewModel = machineTools.CreateLowerToolProfile(multiToolViewModel, lowerGroup, VWidthTypes.BTrumpf, value, maxAllowableToolForcePerLengthUnit, 0.0, workingHeight, vWidth2, vAngle2, null, cornerRadius);
			lowerToolViewModel.FlippingAllowed = allowedFlippedStates;
			lowerToolViewModel.FlippedByDefault = flippedByDefault;
			lowerToolViewModel.VDepth = vDepth;
			lowerToolViewModel.VRadius = vRadius;
			lowerToolViewModel.VGroundWidth = vGroundWidth;
			if (flag3 && flag2)
			{
				lowerToolViewModel.PlugInstallationDirection = InstallationDirection.Both;
			}
			else if (flag3)
			{
				lowerToolViewModel.PlugInstallationDirection = InstallationDirection.Rotated;
			}
			else
			{
				lowerToolViewModel.PlugInstallationDirection = InstallationDirection.NotRotated;
			}
		}
		foreach (XmlNode item6 in xmlNodeList8)
		{
			XmlNode xmlNode19 = item6.SelectSingleNode("ns:Radius", xmlNamespaceManager);
			item6.SelectSingleNode("ns:Radii/ns:RearRadius", xmlNamespaceManager);
			XmlNode xmlNode20 = item6.SelectSingleNode("ns:Radii/ns:FrontRadius", xmlNamespaceManager);
			double rFront2 = 0.0;
			if (xmlNode20 != null)
			{
				rFront2 = _importHelper.SpecialConvertToDouble(xmlNode20.InnerText);
			}
			else
			{
				rFront2 = _importHelper.SpecialConvertToDouble(xmlNode19.InnerText);
			}
			XmlNode xmlNode21 = item6.SelectSingleNode("ns:Angle", xmlNamespaceManager);
			XmlNode xmlNode22 = item6.SelectSingleNode("ns:Angle/ns:FrontAngle", xmlNamespaceManager);
			XmlNode xmlNode23 = item6.SelectSingleNode("ns:Angle/ns:RearAngle", xmlNamespaceManager);
			double vAngle3 = 0.0;
			if (xmlNode22 != null && xmlNode23 != null)
			{
				double num11 = _importHelper.SpecialConvertToDouble(xmlNode22.InnerText);
				double num12 = _importHelper.SpecialConvertToDouble(xmlNode23.InnerText);
				vAngle3 = num11 + num12;
			}
			else
			{
				vAngle3 = _importHelper.SpecialConvertToDouble(xmlNode21.InnerText);
			}
			double vWidth3 = _importHelper.SpecialConvertToDouble(item6.SelectSingleNode("ns:Width", xmlNamespaceManager).InnerText);
			XmlNode xmlNode24 = item6.SelectSingleNode("ns:GrooveGroundDepth", xmlNamespaceManager);
			double? vDepth2 = null;
			if (xmlNode24 != null)
			{
				vDepth2 = _importHelper.SpecialConvertToDouble(xmlNode24.InnerText);
			}
			XmlNode xmlNode25 = item6.SelectSingleNode("ns:GrooveGroundRadius", xmlNamespaceManager);
			double? vRadius2 = null;
			if (xmlNode25 != null)
			{
				vRadius2 = _importHelper.SpecialConvertToDouble(xmlNode25.InnerText);
			}
			XmlNode xmlNode26 = item6.SelectSingleNode("ns:GrooveGroundWidth", xmlNamespaceManager);
			double? vGroundWidth2 = null;
			if (xmlNode26 != null)
			{
				vGroundWidth2 = _importHelper.SpecialConvertToDouble(xmlNode26.InnerText);
			}
			double workingHeight2 = _importHelper.SpecialConvertToDouble(item6.SelectSingleNode("ns:Position", xmlNamespaceManager).InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
			_importHelper.SpecialConvertToDouble(item6.SelectSingleNode("ns:Position", xmlNamespaceManager).InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]);
			LowerToolGroupViewModel lowerToolGroupViewModel2 = _machineTools.LowerGroups.FirstOrDefault((LowerToolGroupViewModel x) => x.Radius == rFront2 && x.VWidth == vWidth3 && Math.Abs(x.VAngle - vAngle3.ToRadians()) < 1E-06);
			if (lowerToolGroupViewModel2 == null)
			{
				lowerToolGroupViewModel2 = _machineTools.CreateLowerToolGroup(vWidth3, vAngle3.ToRadians(), rFront2, $"W{vWidth3} A{vAngle3}");
			}
			MachineToolsViewModel machineTools2 = _machineTools;
			LowerToolGroupViewModel lowerGroup2 = lowerToolGroupViewModel2;
			double maxAllowableToolForcePerLengthUnit2 = result;
			double? vWidth2 = rFront2;
			double? vAngle2 = vAngle3.ToRadians();
			double? cornerRadius = vWidth3;
			LowerToolViewModel lowerToolViewModel2 = machineTools2.CreateLowerToolProfile(multiToolViewModel, lowerGroup2, VWidthTypes.BTrumpf, value, maxAllowableToolForcePerLengthUnit2, 0.0, workingHeight2, cornerRadius, vAngle2, null, vWidth2);
			lowerToolViewModel2.FlippingAllowed = allowedFlippedStates;
			lowerToolViewModel2.FlippedByDefault = flippedByDefault;
			lowerToolViewModel2.VDepth = vDepth2;
			lowerToolViewModel2.VRadius = vRadius2;
			lowerToolViewModel2.VGroundWidth = vGroundWidth2;
			if (flag3 && flag2)
			{
				lowerToolViewModel2.PlugInstallationDirection = InstallationDirection.Both;
			}
			else if (flag3)
			{
				lowerToolViewModel2.PlugInstallationDirection = InstallationDirection.Rotated;
			}
			else
			{
				lowerToolViewModel2.PlugInstallationDirection = InstallationDirection.NotRotated;
			}
		}
		foreach (XmlNode item7 in xmlNodeList5)
		{
			XmlNode xmlNode28 = item7.SelectSingleNode("ns:Radius", xmlNamespaceManager);
			item7.SelectSingleNode("ns:Radii/ns:RearRadius", xmlNamespaceManager);
			XmlNode xmlNode29 = item7.SelectSingleNode("ns:Radii/ns:FrontRadius", xmlNamespaceManager);
			item7.SelectSingleNode("ns:TractrixRadii/ns:RearRadius", xmlNamespaceManager);
			XmlNode xmlNode30 = item7.SelectSingleNode("ns:TractrixRadii/ns:FrontRadius", xmlNamespaceManager);
			item7.SelectSingleNode("ns:TransitionAngles/ns:FrontAngle", xmlNamespaceManager);
			XmlNode xmlNode31 = item7.SelectSingleNode("ns:TransitionAngles/ns:RearAngle", xmlNamespaceManager);
			double rFront3 = 0.0;
			if (xmlNode29 != null)
			{
				rFront3 = _importHelper.SpecialConvertToDouble(xmlNode29.InnerText);
			}
			else
			{
				rFront3 = _importHelper.SpecialConvertToDouble(xmlNode28.InnerText);
			}
			double value3 = _importHelper.SpecialConvertToDouble(xmlNode30.InnerText);
			double value4 = _importHelper.SpecialConvertToDouble(xmlNode31.InnerText);
			XmlNode xmlNode32 = item7.SelectSingleNode("ns:Angle", xmlNamespaceManager);
			XmlNode xmlNode33 = item7.SelectSingleNode("ns:Angle/ns:FrontAngle", xmlNamespaceManager);
			XmlNode xmlNode34 = item7.SelectSingleNode("ns:Angle/ns:RearAngle", xmlNamespaceManager);
			double vAngle4 = 0.0;
			if (xmlNode33 != null && xmlNode34 != null)
			{
				double num13 = _importHelper.SpecialConvertToDouble(xmlNode33.InnerText);
				double num14 = _importHelper.SpecialConvertToDouble(xmlNode34.InnerText);
				vAngle4 = num13 + num14;
			}
			else
			{
				vAngle4 = _importHelper.SpecialConvertToDouble(xmlNode32.InnerText);
			}
			double vWidth4 = _importHelper.SpecialConvertToDouble(item7.SelectSingleNode("ns:Width", xmlNamespaceManager).InnerText);
			XmlNode xmlNode35 = item7.SelectSingleNode("ns:GrooveGroundDepth", xmlNamespaceManager);
			double? vDepth3 = null;
			if (xmlNode35 != null)
			{
				vDepth3 = _importHelper.SpecialConvertToDouble(xmlNode35.InnerText);
			}
			XmlNode xmlNode36 = item7.SelectSingleNode("ns:GrooveGroundRadius", xmlNamespaceManager);
			double? vRadius3 = null;
			if (xmlNode36 != null)
			{
				vRadius3 = _importHelper.SpecialConvertToDouble(xmlNode36.InnerText);
			}
			XmlNode xmlNode37 = item7.SelectSingleNode("ns:GrooveGroundWidth", xmlNamespaceManager);
			double? vGroundWidth3 = null;
			if (xmlNode37 != null)
			{
				vGroundWidth3 = _importHelper.SpecialConvertToDouble(xmlNode37.InnerText);
			}
			double workingHeight3 = _importHelper.SpecialConvertToDouble(item7.SelectSingleNode("ns:Position", xmlNamespaceManager).InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
			_importHelper.SpecialConvertToDouble(item7.SelectSingleNode("ns:Position", xmlNamespaceManager).InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]);
			LowerToolGroupViewModel lowerToolGroupViewModel3 = _machineTools.LowerGroups.FirstOrDefault((LowerToolGroupViewModel x) => x.Radius == rFront3 && x.VWidth == vWidth4 && Math.Abs(x.VAngle - vAngle4.ToRadians()) < 1E-06 && x.CornerType == CornerType.Tractrix);
			if (lowerToolGroupViewModel3 == null)
			{
				lowerToolGroupViewModel3 = _machineTools.CreateLowerToolGroup(vWidth4, vAngle4.ToRadians(), rFront3, $"W{vWidth4} A{vAngle4} Tractrix");
			}
			lowerToolGroupViewModel3.CornerType = CornerType.Tractrix;
			MachineToolsViewModel machineTools3 = _machineTools;
			LowerToolGroupViewModel lowerGroup3 = lowerToolGroupViewModel3;
			double maxAllowableToolForcePerLengthUnit3 = result;
			double? cornerRadius = rFront3;
			double? vAngle2 = vAngle4.ToRadians();
			double? vWidth2 = vWidth4;
			LowerToolViewModel lowerToolViewModel3 = machineTools3.CreateLowerToolProfile(multiToolViewModel, lowerGroup3, VWidthTypes.BTrumpf, value, maxAllowableToolForcePerLengthUnit3, 0.0, workingHeight3, vWidth2, vAngle2, null, cornerRadius);
			lowerToolViewModel3.FlippingAllowed = allowedFlippedStates;
			lowerToolViewModel3.FlippedByDefault = flippedByDefault;
			lowerToolViewModel3.VDepth = vDepth3;
			lowerToolViewModel3.VRadius = vRadius3;
			lowerToolViewModel3.VGroundWidth = vGroundWidth3;
			lowerToolViewModel3.TractrixRadius = value3;
			lowerToolViewModel3.TransitionAngle = value4;
			if (flag3 && flag2)
			{
				lowerToolViewModel3.PlugInstallationDirection = InstallationDirection.Both;
			}
			else if (flag3)
			{
				lowerToolViewModel3.PlugInstallationDirection = InstallationDirection.Rotated;
			}
			else
			{
				lowerToolViewModel3.PlugInstallationDirection = InstallationDirection.NotRotated;
			}
		}
		foreach (XmlNode item8 in xmlNodeList6)
		{
			double radius = _importHelper.SpecialConvertToDouble(item8.SelectSingleNode("ns:Radius", xmlNamespaceManager).InnerText);
			XmlNode xmlNode39 = item8.SelectSingleNode("ns:Angle", xmlNamespaceManager);
			XmlNode xmlNode40 = item8.SelectSingleNode("ns:Angle/ns:FrontAngle", xmlNamespaceManager);
			XmlNode xmlNode41 = item8.SelectSingleNode("ns:Angle/ns:RearAngle", xmlNamespaceManager);
			double num15 = 0.0;
			if (xmlNode40 != null && xmlNode41 != null)
			{
				double num16 = _importHelper.SpecialConvertToDouble(xmlNode40.InnerText);
				double num17 = _importHelper.SpecialConvertToDouble(xmlNode41.InnerText);
				num15 = num16 + num17;
			}
			else
			{
				num15 = _importHelper.SpecialConvertToDouble(xmlNode39.InnerText);
			}
			double workingHeight4 = _importHelper.SpecialConvertToDouble(item8.SelectSingleNode("ns:Position", xmlNamespaceManager).InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
			UpperToolGroupViewModel upperToolGroupViewModel = _machineTools.UpperGroups.FirstOrDefault((UpperToolGroupViewModel x) => x.Radius == radius);
			if (upperToolGroupViewModel == null)
			{
				upperToolGroupViewModel = _machineTools.CreateUpperToolGroup($"R{radius}", radius);
			}
			UpperToolViewModel upperToolViewModel = _machineTools.CreateUpperToolProfile(multiToolViewModel, upperToolGroupViewModel, radius, num15.ToRadians(), workingHeight4, value, result, 0.0, 0.0);
			upperToolViewModel.FlippingAllowed = allowedFlippedStates;
			upperToolViewModel.FlippedByDefault = flippedByDefault;
			if (flag3 && flag2)
			{
				upperToolViewModel.PlugInstallationDirection = InstallationDirection.Both;
			}
			else if (flag3)
			{
				upperToolViewModel.PlugInstallationDirection = InstallationDirection.Rotated;
			}
			else
			{
				upperToolViewModel.PlugInstallationDirection = InstallationDirection.NotRotated;
			}
		}
		foreach (XmlNode item9 in xmlNodeList7)
		{
			double workingHeight5 = _importHelper.SpecialConvertToDouble(item9.SelectSingleNode("ns:Position", xmlNamespaceManager).InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
			double num18 = _importHelper.SpecialConvertToDouble(item9.SelectSingleNode("ns:Position", xmlNamespaceManager).InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]);
			double num19 = _importHelper.SpecialConvertToDouble(item9.SelectSingleNode("ns:Extent", xmlNamespaceManager).InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]);
			bool flag4 = item9.SelectSingleNode("ns:Rotation", xmlNamespaceManager) != null;
			if (value2 == "FlatteningBar")
			{
				LowerToolGroupViewModel group = _machineTools.CreateLowerToolGroup();
				double offsetInXForHemmingMm = num18 + num19 * 0.5;
				double offsetInXForHemmingMm2 = 0.0;
				LowerToolExtensionViewModel item = new LowerToolExtensionViewModel(_unitConverter, _toolStorage, multiToolViewModel, group)
				{
					Name = value + " " + side1,
					WorkingHeight = workingHeight5,
					MaxAllowableToolForcePerLengthUnit = result,
					IsFoldTool = true,
					Implemented = true,
					FlippingAllowed = AllowedFlippedStates.OnlyNotFlipped,
					FlippedByDefault = false,
					OffsetInXForHemmingMm = offsetInXForHemmingMm,
					PlugInstallationDirection = InstallationDirection.NotRotated
				};
				_machineTools.LowerToolsExtensions.Add(item);
				if (allowedFlippedStates == AllowedFlippedStates.Both)
				{
					LowerToolExtensionViewModel item2 = new LowerToolExtensionViewModel(_unitConverter, _toolStorage, multiToolViewModel2, group)
					{
						Name = value + " " + side2,
						WorkingHeight = workingHeight5,
						MaxAllowableToolForcePerLengthUnit = result,
						IsFoldTool = true,
						Implemented = true,
						FlippingAllowed = AllowedFlippedStates.OnlyNotFlipped,
						FlippedByDefault = false,
						OffsetInXForHemmingMm = offsetInXForHemmingMm2,
						PlugInstallationDirection = InstallationDirection.NotRotated
					};
					_machineTools.LowerToolsExtensions.Add(item2);
				}
			}
			else if (xmlNodeList4.Count > 0)
			{
				if (!flag4)
				{
					LowerToolGroupViewModel lowerGroup4 = _machineTools.CreateLowerToolGroup();
					LowerToolViewModel lowerToolViewModel4 = _machineTools.CreateLowerToolProfile(multiToolViewModel, lowerGroup4, VWidthTypes.BTrumpf, value, result, (!flag4) ? (num18 + num19 * 0.5) : (0.0 - num18 - num19 * 0.5), workingHeight5);
					lowerToolViewModel4.IsFoldTool = true;
					lowerToolViewModel4.FlippingAllowed = AllowedFlippedStates.OnlyNotFlipped;
					lowerToolViewModel4.FlippedByDefault = false;
					lowerToolViewModel4.PlugInstallationDirection = InstallationDirection.NotRotated;
				}
			}
			else
			{
				UpperToolGroupViewModel upperGroup = _machineTools.CreateUpperToolGroup();
				UpperToolViewModel upperToolViewModel2 = _machineTools.CreateUpperToolProfile(multiToolViewModel, upperGroup, null, null, workingHeight5, value, result, num18 + num19 * 0.5, 0.0);
				upperToolViewModel2.IsFoldTool = true;
				upperToolViewModel2.WidthHemmingFaceMm = num19;
				upperToolViewModel2.Angle = null;
				upperToolViewModel2.Radius = null;
				upperToolViewModel2.FlippingAllowed = allowedFlippedStates;
				upperToolViewModel2.FlippedByDefault = flippedByDefault;
				if (flag3 && flag2)
				{
					upperToolViewModel2.PlugInstallationDirection = InstallationDirection.Both;
				}
				else if (flag3)
				{
					upperToolViewModel2.PlugInstallationDirection = InstallationDirection.Rotated;
				}
				else
				{
					upperToolViewModel2.PlugInstallationDirection = InstallationDirection.NotRotated;
				}
			}
		}
		if (xmlNodeList2 != null)
		{
			double workingHeight6 = 0.0;
			double socketPosXMm = 0.0;
			string socketName = "";
			bool flag5 = false;
			bool flag6 = false;
			double? socketAngleRad = null;
			double? socketMinRadiusMm = null;
			double? socketMaxRadiusMm = null;
			foreach (XmlNode item10 in xmlNodeList2)
			{
				socketName = item10.Attributes["Coupling"].Value;
				workingHeight6 = _importHelper.SpecialConvertToDouble(item10.SelectSingleNode("ns:Position", xmlNamespaceManager).InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
				socketPosXMm = _importHelper.SpecialConvertToDouble(item10.SelectSingleNode("ns:Position", xmlNamespaceManager).InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]);
				flag5 |= item10.SelectSingleNode("ns:Rotation", xmlNamespaceManager) != null;
				flag6 |= item10.SelectSingleNode("ns:Rotation", xmlNamespaceManager) == null;
				XmlNode xmlNode44 = item10.SelectSingleNode("ns:Angle", xmlNamespaceManager);
				XmlNode xmlNode45 = item10.SelectSingleNode("ns:MinRadius", xmlNamespaceManager);
				XmlNode xmlNode46 = item10.SelectSingleNode("ns:MaxRadius", xmlNamespaceManager);
				if (xmlNode44 != null)
				{
					socketAngleRad = _importHelper.SpecialConvertToDouble(xmlNode44.InnerText).ToRadians();
				}
				if (xmlNode45 != null)
				{
					socketMinRadiusMm = _importHelper.SpecialConvertToDouble(xmlNode45.InnerText);
				}
				if (xmlNode46 != null)
				{
					socketMaxRadiusMm = _importHelper.SpecialConvertToDouble(xmlNode46.InnerText);
				}
			}
			bool value5 = value2.Contains("Holder");
			if (socketName.Contains("FLATTENINGBAR"))
			{
				foreach (XmlNode item11 in xmlNodeList2)
				{
					socketPosXMm = _importHelper.SpecialConvertToDouble(item11.SelectSingleNode("ns:Position", xmlNamespaceManager).InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]);
					flag5 = item11.SelectSingleNode("ns:Rotation", xmlNamespaceManager) != null;
					string side3 = ((socketPosXMm > 0.0) ? "Rear" : "Front");
					MappingViewModel.MappingVm mappingVm3 = _machineTools.MountIdMappings.FirstOrDefault((MappingViewModel.MappingVm x) => x.PpName == socketName && x.Profile.Desc == side3);
					if (mappingVm3 == null)
					{
						int num20 = _machineTools.MountIdItemVms.Select((MappingViewModel.ItemVm x) => x.Id).Order().LastOrDefault();
						MappingViewModel.ItemVm itemVm3 = new MappingViewModel.ItemVm
						{
							Id = num20 + 1,
							Desc = side3
						};
						mappingVm3 = new MappingViewModel.MappingVm
						{
							Profile = itemVm3,
							PpName = socketName
						};
						_machineTools.MountIdItemVms.Add(itemVm3);
						_machineTools.MountIdMappings.Add(mappingVm3);
					}
					LowerAdapterViewModel item3 = new LowerAdapterViewModel(_unitConverter, _toolStorage, multiToolViewModel)
					{
						Name = value,
						SocketId = mappingVm3.Profile.Id,
						SocketCoupling = socketName,
						SocketPosXMm = socketPosXMm,
						SocketInstallationDirection = ((!flag5) ? InstallationDirection.NotRotated : InstallationDirection.Rotated),
						MaxAllowableToolForcePerLengthUnit = result,
						WorkingHeight = workingHeight6,
						AdapterDirection = ((!(socketPosXMm > 0.0)) ? AdapterDirections.Front : AdapterDirections.Back),
						Implemented = true,
						FlippingAllowed = allowedFlippedStates,
						FlippedByDefault = flippedByDefault
					};
					_machineTools.LowerAdapters.Add(item3);
				}
			}
			else
			{
				MappingViewModel.MappingVm mappingVm4 = _machineTools.MountIdMappings.FirstOrDefault((MappingViewModel.MappingVm x) => x.PpName == socketName);
				if (mappingVm4 == null)
				{
					int num21 = _machineTools.MountIdItemVms.Select((MappingViewModel.ItemVm x) => x.Id).Order().LastOrDefault();
					MappingViewModel.ItemVm itemVm4 = new MappingViewModel.ItemVm
					{
						Id = num21 + 1
					};
					mappingVm4 = new MappingViewModel.MappingVm
					{
						Profile = itemVm4,
						PpName = socketName
					};
					_machineTools.MountIdItemVms.Add(itemVm4);
					_machineTools.MountIdMappings.Add(mappingVm4);
				}
				if (flag)
				{
					LowerAdapterViewModel lowerAdapterViewModel = new LowerAdapterViewModel(_unitConverter, _toolStorage, multiToolViewModel)
					{
						Name = value,
						SocketId = mappingVm4.Profile.Id,
						SocketCoupling = socketName,
						SocketPosXMm = socketPosXMm,
						SocketAngleRad = socketAngleRad,
						SocketMinRadiusMm = socketMinRadiusMm,
						SocketMaxRadiusMm = socketMaxRadiusMm,
						MaxAllowableToolForcePerLengthUnit = result,
						WorkingHeight = workingHeight6,
						AdapterDirection = AdapterDirections.TopDown,
						Implemented = true,
						FlippingAllowed = allowedFlippedStates,
						FlippedByDefault = flippedByDefault
					};
					if (flag5 && flag6)
					{
						lowerAdapterViewModel.SocketInstallationDirection = InstallationDirection.Both;
					}
					else if (flag5)
					{
						lowerAdapterViewModel.SocketInstallationDirection = InstallationDirection.Rotated;
					}
					else
					{
						lowerAdapterViewModel.SocketInstallationDirection = InstallationDirection.NotRotated;
					}
					if (flag3 && flag2)
					{
						lowerAdapterViewModel.PlugInstallationDirection = InstallationDirection.Both;
					}
					else if (flag3)
					{
						lowerAdapterViewModel.PlugInstallationDirection = InstallationDirection.Rotated;
					}
					else
					{
						lowerAdapterViewModel.PlugInstallationDirection = InstallationDirection.NotRotated;
					}
					_machineTools.LowerAdapters.Add(lowerAdapterViewModel);
				}
				else
				{
					UpperAdapterViewModel upperAdapterViewModel = new UpperAdapterViewModel(_unitConverter, _toolStorage, multiToolViewModel)
					{
						Name = value,
						SocketId = mappingVm4.Profile.Id,
						SocketCoupling = socketName,
						SocketPosXMm = socketPosXMm,
						SocketAngleRad = socketAngleRad,
						SocketMinRadiusMm = socketMinRadiusMm,
						SocketMaxRadiusMm = socketMaxRadiusMm,
						MaxAllowableToolForcePerLengthUnit = result,
						WorkingHeight = workingHeight6,
						IsHolder = value5,
						Implemented = true,
						FlippingAllowed = allowedFlippedStates,
						FlippedByDefault = flippedByDefault
					};
					if (flag5 && flag6)
					{
						upperAdapterViewModel.SocketInstallationDirection = InstallationDirection.Both;
					}
					else if (flag5)
					{
						upperAdapterViewModel.SocketInstallationDirection = InstallationDirection.Rotated;
					}
					else
					{
						upperAdapterViewModel.SocketInstallationDirection = InstallationDirection.NotRotated;
					}
					if (flag3 && flag2)
					{
						upperAdapterViewModel.PlugInstallationDirection = InstallationDirection.Both;
					}
					else if (flag3)
					{
						upperAdapterViewModel.PlugInstallationDirection = InstallationDirection.Rotated;
					}
					else
					{
						upperAdapterViewModel.PlugInstallationDirection = InstallationDirection.NotRotated;
					}
					_machineTools.UpperAdapters.Add(upperAdapterViewModel);
				}
			}
		}
		foreach (XmlNode item12 in xmlNodeList3)
		{
			int amount = 10;
			double length = double.Parse(item12.Attributes["Length"].Value);
			string text = item12.SelectSingleNode("ns:FrontGeometryList/ns:FrontGeometry", xmlNamespaceManager)?.InnerText ?? item12.SelectSingleNode("ns:FrontGeometry", xmlNamespaceManager)?.InnerText ?? "";
			string text2 = item12.Attributes["Type"]?.Value;
			XmlNode xmlNode49 = item12.SelectSingleNode("ns:MaximumLoad", xmlNamespaceManager);
			double value6 = ((xmlNode49 != null) ? int.Parse(xmlNode49.InnerText) : result);
			Model resultModel = null;
			if (!string.IsNullOrEmpty(text))
			{
				ICadGeo cadGeo2 = ReadSvg(zipArchive, text);
				if (flag)
				{
					cadGeo2.RotateAll(Math.PI);
				}
				WzgLoader.GetElements(cadGeo2, out var elements);
				double num22 = elements.SelectMany((List<Vector3d> x) => x).Min((Vector3d x) => x.X);
				cadGeo2.MoveAll(0.0 - num22, flag ? (0.0 - num6) : num6);
				try
				{
					WzgLoader.GetHeelTool(cadGeo, cadGeo2, out resultModel, advancedExtrude: true);
				}
				catch
				{
					text = "";
				}
			}
			if (!flag)
			{
				UpperToolPieceViewModel upperToolPieceViewModel = _machineTools.CreateUpperToolPiece(_machineTools.SelectedToolList, multiToolViewModel, _importHelper.MakeValidFileName(text), length, amount);
				upperToolPieceViewModel.HasHeelLeft = text2 == "HornLeft";
				upperToolPieceViewModel.HasHeelRight = text2 == "HornRight";
				upperToolPieceViewModel.IsAngleMeasurementTool = text2?.ToUpper().StartsWith("ACB") ?? false;
				upperToolPieceViewModel.Geometry3D = resultModel;
				upperToolPieceViewModel.MaxAllowableToolForce = value6;
				_machineTools.UpperPieces.Add(upperToolPieceViewModel);
			}
			else if (value2 == "FlatteningBar")
			{
				LowerToolExtensionPieceViewModel lowerToolExtensionPieceViewModel = _machineTools.CreateLowerToolExtensionPiece(_machineTools.SelectedToolList, multiToolViewModel, _importHelper.MakeValidFileName(text), length, amount);
				lowerToolExtensionPieceViewModel.MaxAllowableToolForce = value6;
				_machineTools.LowerToolExtentionPieces.Add(lowerToolExtensionPieceViewModel);
				if (multiToolViewModel2 != null)
				{
					LowerToolExtensionPieceViewModel lowerToolExtensionPieceViewModel2 = _machineTools.CreateLowerToolExtensionPiece(_machineTools.SelectedToolList, multiToolViewModel2, _importHelper.MakeValidFileName(text), length, amount);
					lowerToolExtensionPieceViewModel2.MaxAllowableToolForce = value6;
					_machineTools.LowerToolExtentionPieces.Add(lowerToolExtensionPieceViewModel2);
				}
			}
			else
			{
				LowerToolPieceViewModel lowerToolPieceViewModel = _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, multiToolViewModel, _importHelper.MakeValidFileName(text), length, amount);
				lowerToolPieceViewModel.Geometry3D = resultModel;
				lowerToolPieceViewModel.MaxAllowableToolForce = value6;
				_machineTools.LowerPieces.Add(lowerToolPieceViewModel);
			}
		}
		return true;
	}

	private ICadGeo ReadSvg(ZipArchive zip, string path)
	{
		string text = Uri.EscapeDataString(path);
		List<GeoSegment2D> segments = SvgReader.Read((zip.GetEntry("2DGraphics\\" + path) ?? zip.GetEntry("2DGraphics/" + text)).Open());
		List<CadGeoElement> list = new ContourConverter().ToCadGeoContour(segments, 52);
		ICadGeo cadGeo = new CadGeoInfoBase();
		foreach (CadGeoElement item in list)
		{
			if (item is CadGeoLine line)
			{
				AddCadGeoLineElement(cadGeo, line);
			}
			else if (item is CadGeoCircle circle)
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
