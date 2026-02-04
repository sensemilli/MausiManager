using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DxfToCadGeo;
using Microsoft.Win32;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.Helpers.Util;
using WiCAM.Pn4000.PKernelFlow.WrapC;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;

public class ToolViewModelBase(IModelFactory _modelFactory, ITranslator _translator) : ViewModelBase()
{
	private MachineToolsViewModel _toolConfigModel;

	private const double ToolImageHeight = 424.0;

	private const double ToolImageWidth = 413.0;

	private FrameworkElement _imageProfile = new Canvas
	{
		Width = 413.0,
		Height = 424.0
	};

	private FrameworkElement _imagePart = new Screen3D();

	public MachineToolsViewModel ToolConfigModel
	{
		get
		{
			return _toolConfigModel;
		}
		set
		{
			_toolConfigModel = value;
			NotifyPropertyChanged("ToolConfigModel");
		}
	}

	protected object LastSelectedObject { get; set; }

	protected string SelectedType { get; set; }

	public FrameworkElement ImageProfile
	{
		get
		{
			return _imageProfile;
		}
		set
		{
			_imageProfile = value;
			NotifyPropertyChanged("ImageProfile");
		}
	}

	public FrameworkElement ImagePart
	{
		get
		{
			return _imagePart;
		}
		set
		{
			_imagePart = value;
			NotifyPropertyChanged("ImagePart");
		}
	}

	public bool RememberBlockState { get; set; }

	public void Init(MachineToolsViewModel machineTools)
	{
		_toolConfigModel = machineTools;
	}

	public void ImportTools(IToolImporter usedImporter, Action<string> importAction)
	{
		usedImporter.Init(ToolConfigModel);
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Title = _translator.Translate("l_popup.PopupMachineConfig.l_filter.ImportDialogName"),
			Filter = usedImporter.GetFilterString(),
			Multiselect = true
		};
		if (openFileDialog.ShowDialog().Value)
		{
			string[] fileNames = openFileDialog.FileNames;
			foreach (string obj in fileNames)
			{
				importAction(obj);
			}
		}
	}

	public void ImportDXFProfile(MultiToolViewModel target)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Filter = _translator.Translate("l_popup.PopupMachineConfig.l_filter.Dxf"),
			Title = _translator.Translate("l_popup.PopupMachineConfig.l_filter.ImportDialogName"),
			Multiselect = false
		};
		if (openFileDialog.ShowDialog().Value)
		{
			target.Geometry = DxfToCadGeo.Convert(openFileDialog.FileName, _modelFactory);
		}
	}

	protected void MirrorGeometry(string profileFile, IEnumerable<string> pieceFiles)
	{
		if (!File.Exists(profileFile))
		{
			return;
		}
		try
		{
			CadGeoHelper cadGeoHelper = new CadGeoHelper();
			CadGeoInfo cadGeoInfo = cadGeoHelper.ReadCadgeo(profileFile);
			foreach (GeoElementInfo geoElement in cadGeoInfo.GeoElements)
			{
				if (geoElement is GeoLineInfo geoLineInfo)
				{
					geoLineInfo.X0 *= -1.0;
					geoLineInfo.X1 *= -1.0;
				}
				else if (geoElement is GeoArcInfo geoArcInfo)
				{
					geoArcInfo.X0 *= -1.0;
					geoArcInfo.Direction *= -1;
					if (geoArcInfo.BeginAngle <= 180.0)
					{
						geoArcInfo.BeginAngle = 180.0 - geoArcInfo.BeginAngle;
					}
					else
					{
						geoArcInfo.BeginAngle = 360.0 - (geoArcInfo.BeginAngle - 180.0);
					}
					if (geoArcInfo.EndAngle <= 180.0)
					{
						geoArcInfo.EndAngle = 180.0 - geoArcInfo.EndAngle;
					}
					else
					{
						geoArcInfo.EndAngle = 360.0 - (geoArcInfo.EndAngle - 180.0);
					}
				}
			}
			cadGeoHelper.Write(cadGeoInfo, profileFile);
			foreach (string pieceFile in pieceFiles)
			{
				if (!string.IsNullOrWhiteSpace(pieceFile))
				{
					if (File.Exists(pieceFile) && pieceFile.ToLower().EndsWith(".c3mo"))
					{
						Model model = ModelSerializer.Deserialize(pieceFile);
						model.ModifyVertices(Matrix4d.Scale(1.0, -1.0, 1.0), transformSubModels: true);
						ModelSerializer.Serialize(pieceFile, model);
					}
					else
					{
						MessageBox.Show("Error on file '" + pieceFile + "'. Geometry can't be mirrored.", "error", MessageBoxButton.OK, MessageBoxImage.Hand);
					}
				}
			}
		}
		catch (Exception)
		{
			MessageBox.Show("Error occurred. Please load backup.", "error", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	protected void EditGeometry(string profileFile, IEnumerable<string> pieceFiles)
	{
		IPnPathService pnPathService = _modelFactory.Resolve<IPnPathService>();
		string pathAtHome = pnPathService.GetPathAtHome("\\CADGEO");
		string pathAtHome2 = pnPathService.GetPathAtHome("\\CADTXT");
		try
		{
			File.Copy(profileFile, pathAtHome, overwrite: true);
			File.Delete(pathAtHome2);
		}
		catch (IOException e)
		{
			_modelFactory.Resolve<ILogCenterService>().CatchRaport(e);
			return;
		}
		ToolConfigModel.LastEditGeometryPath = profileFile;
		int iret = 0;
		WIP.wip112_(ref iret);
		IMainWindowDataProvider mainWindowDataProvider = _modelFactory.Resolve<IMainWindowDataProvider>();
		IMainWindowBlock mainWindowBlock = _modelFactory.Resolve<IMainWindowBlock>();
		RememberBlockState = true;
		mainWindowBlock.BlockUI_UnblockAll();
		mainWindowDataProvider.Ribbon_ActivateCadTab();
	}

	protected void SaveGeometry(string profileFile, IEnumerable<string> pieceFiles)
	{
		string reference = _modelFactory.Resolve<IPnPathService>().GetPathAtHome("CADGEO");
		string sourceFileName = Path.Join(new ReadOnlySpan<string>(in reference));
		bool flag = true;
		if (ToolConfigModel.LastEditGeometryPath == null)
		{
			string fileName = Path.GetFileName(profileFile);
			flag = MessageBox.Show(_translator.Translate("l_popup.ToolsView.NoGeometryEditDuringSave", fileName), _translator.Translate("l_popup.ToolsView.NoGeometryEditDuringSaveCaption"), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
		}
		else if (ToolConfigModel.LastEditGeometryPath != profileFile)
		{
			string fileName2 = Path.GetFileName(ToolConfigModel.LastEditGeometryPath);
			string fileName3 = Path.GetFileName(profileFile);
			flag = MessageBox.Show(_translator.Translate("l_popup.ToolsView.MismatchGeometryEditDuringSave", fileName3, fileName2), _translator.Translate("l_popup.ToolsView.MismatchGeometryEditDuringSaveCaption"), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
		}
		if (!flag)
		{
			return;
		}
		int iret = 0;
		WIP.wip111_(ref iret);
		try
		{
			File.Copy(sourceFileName, profileFile, overwrite: true);
		}
		catch (IOException e)
		{
			_modelFactory.Resolve<ILogCenterService>().CatchRaport(e);
		}
	}

	private void LoadCadGeo_Click()
	{
		new ArchiveBrowser().Start(delegate
		{
		});
	}
}
