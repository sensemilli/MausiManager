using System;
using System.Collections.Generic;
using System.Windows;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.View;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.PN3D.Unfold;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D;

public class FrontCallsWpf : IFrontCalls
{
	private readonly IScreen3DMain _screen3d;

	private readonly IMainWindowBlock _windowBlock;

	private readonly IFactorio _factorio;

	private readonly IAutoMode _autoMode;

	private readonly IShowPopupService _showPopupService;

	private readonly ITranslator _translator;

	private int _docCount;

	private readonly ILogCenterService _logCenterService;

	private readonly IConfigProvider _configProvider;

	private readonly IPKernelFlowGlobalDataService _pKernelFlowGlobalData;

	private readonly IPnPathService _pnPathService;

	private readonly IMaterialManager _materials;

	private readonly IMaterial3dFortran _material3dFortran;

	private readonly IImportMaterialMapper _importMaterialMapper;

	private readonly IMainWindowTaskbarItemInfo _mainWindowTaskbar;

	public IMessageDisplay MessageDisplay { get; private set; }

	public FrontCallsWpf(IScreen3DMain screen3d, IMainWindowBlock windowBlock, IFactorio factorio, IAutoMode autoMode, IShowPopupService showPopupService, ILogCenterService logCenterService, IConfigProvider configProvider, IPKernelFlowGlobalDataService pKernelFlowGlobalData, IPnPathService pnPathService, IMaterialManager materials, IMaterial3dFortran material3dFortran, IImportMaterialMapper importMaterialMapper, IMainWindowTaskbarItemInfo mainWindowTaskbar)
	{
		_screen3d = screen3d;
		_windowBlock = windowBlock;
		_factorio = factorio;
		_autoMode = autoMode;
		_showPopupService = showPopupService;
		_logCenterService = logCenterService;
		_configProvider = configProvider;
		_pKernelFlowGlobalData = pKernelFlowGlobalData;
		_pnPathService = pnPathService;
		_materials = materials;
		_material3dFortran = material3dFortran;
		_importMaterialMapper = importMaterialMapper;
		_mainWindowTaskbar = mainWindowTaskbar;
		_translator = factorio.Resolve<ITranslator>();
	}

	public IFrontCalls CreateDocInstance(string docName)
	{
		_docCount++;
		FrontCallsWpf frontCallsWpf = _factorio.CreateNew<FrontCallsWpf>(Array.Empty<object>());
		frontCallsWpf.Initialize(MessageDisplay.CreateSubMessageDisplay(_docCount.ToString("000") + "_" + docName));
		return frontCallsWpf;
	}

	public void Initialize(IMessageDisplay messageDisplay)
	{
		MessageDisplay = messageDisplay;
	}

	public IMaterialArt GetMaterial(IDoc3d doc, IGlobals globals, IMessageDisplay messageDisplay)
	{
		if (_autoMode.PopupsEnabled)
		{
			_windowBlock.InitWait(doc);
			IMaterialArt result = SelectMaterial(doc, globals, messageDisplay);
			_windowBlock.CloseWait(doc);
			return result;
		}
		return _material3dFortran.GetActiveMaterial(isAssembly: false);
	}

	public IMaterialArt GetMaterialAssembly(IDoc3d doc, IGlobals globals, IMessageDisplay messageDisplay)
	{
		return _material3dFortran.GetActiveMaterial(isAssembly: true);
	}

	public void GetDeductionValue(IDoc3d doc, Dictionary<IBendDescriptor, BendTableReturnValues> bendTableResults, IGlobals globals, bool editWithTools)
	{
		if (!_materials.Material3DGroup.ContainsKey(doc.Material.MaterialGroupForBendDeduction))
		{
			MessageDisplay.ShowTranslatedErrorMessage("l_popup.GetDeductionValue.ErrorMatConfig", doc.Material.Name, doc.Material.Number, doc.Material.MaterialGroupForBendDeduction);
		}
		else if (_autoMode.PopupsEnabled)
		{
			throw new NotImplementedException();
		}
	}

	public IWaitCancel ShowWaitWithCancel(IGlobals globals)
	{
		if (_autoMode.HasGui && _factorio.Resolve<IMainWindowDataProvider>() is Window { WindowState: not WindowState.Minimized } window)
		{
			InfoPopupWithCancel infoPopupWithCancel = new InfoPopupWithCancel(_pnPathService, _mainWindowTaskbar, _translator);
			infoPopupWithCancel.Owner = window;
			infoPopupWithCancel.Show();
			return infoPopupWithCancel;
		}
		return null;
	}

	public void CloseWaitWithCancel(IWaitCancel waitCancel)
	{
		if (waitCancel is InfoPopupWithCancel infoPopupWithCancel)
		{
			infoPopupWithCancel.Close();
		}
	}

	public IMaterialArt SelectMaterial(IDoc3d doc, IGlobals globals, IMessageDisplay messageDisplay)
	{
		bool flag = false;
		if (doc?.EntryModel3D != null && doc.EntryModel3D.OriginalMaterialName != string.Empty && _importMaterialMapper.GetMaterialId(doc.EntryModel3D.OriginalMaterialName) == -1)
		{
			messageDisplay.ShowInformationMessage(globals.LanguageDictionary.GetMsg2Int("The material found in the CAD file is") + ": " + doc.EntryModel3D.OriginalMaterialName + "\n\n" + globals.LanguageDictionary.GetMsg2Int("Please select a known material to which this material should be mapped."), globals.LanguageDictionary.GetMsg2Int("Material information"));
			flag = true;
		}
		IMaterialArt materialArt = SelectMaterialWithNewPopup(doc);
		if (flag && materialArt != null)
		{
			_importMaterialMapper.AddMaterial(doc.EntryModel3D.OriginalMaterialName, materialArt.Number);
		}
		return materialArt;
	}

	private IMaterialArt SelectMaterialWithNewPopup(IDoc3d doc)
	{
		if (!_autoMode.PopupsEnabled)
		{
			return _material3dFortran.GetActiveMaterial(isAssembly: false);
		}
		IMaterialArt result = null;
		PopupSelectMaterialView popupSelectMaterialView = _factorio.Resolve<PopupSelectMaterialView>();
		IMainWindowDataProvider mainWindow = _factorio.Resolve<IMainWindowDataProvider>();
		popupSelectMaterialView.Init(doc, delegate(PopupSelectMaterialViewModel sender)
		{
			result = sender.SelectedMaterialOfModel;
			if (result != null)
			{
				mainWindow.AddRecentlyUsedRecord(new RecentlyUsedRecord
				{
					FileName = result.Name,
					ArchiveID = result.Number,
					Type = "Mat3D"
				});
			}
		});
		if (!_windowBlock.BlockUI_IsBlock(doc))
		{
			_windowBlock.BlockUI_Block(doc);
		}
		doc?.SetModelDefaultColors();
		_screen3d.Screen3D.IgnoreMouseMove(ignore: true);
		_windowBlock.CloseWait(doc);
		_showPopupService.Show(popupSelectMaterialView, popupSelectMaterialView.CloseByRightButtonClickOutsideWindow);
		_windowBlock.InitWait(doc);
		_screen3d.Screen3D.IgnoreMouseMove(ignore: false);
		_windowBlock.BlockUI_Unblock(doc);
		return result;
	}

	public bool CreatePostprocessorIfErrorsSimValidation()
	{
		if (_autoMode.PopupsEnabled && MessageBox.Show(_translator.Translate("l_popup.PpView.IgnoreValidationError"), _translator.Translate("General.Error"), MessageBoxButton.YesNo, MessageBoxImage.Hand, MessageBoxResult.No) == MessageBoxResult.Yes)
		{
			return true;
		}
		return false;
	}

	public bool CreatePostprocessorIfMaterialNotByUser()
	{
		if (_autoMode.PopupsEnabled && MessageBox.Show(_translator.Translate("l_popup.PpView.MaterialNotByUser"), _translator.Translate("General.Error"), MessageBoxButton.YesNo, MessageBoxImage.Hand, MessageBoxResult.No) == MessageBoxResult.Yes)
		{
			return true;
		}
		return false;
	}

	public bool CreatePostprocessorIfNoKFactor()
	{
		if (!_configProvider.InjectOrCreate<General3DConfig>().P3D_WarningNoKFactor)
		{
			return true;
		}
		if (_autoMode.PopupsEnabled && MessageBox.Show(_translator.Translate("l_popup.PpView.IgnoreKFactorError"), _translator.Translate("General.Error"), MessageBoxButton.YesNo, MessageBoxImage.Hand, MessageBoxResult.No) == MessageBoxResult.Yes)
		{
			return true;
		}
		return false;
	}

	public string OpenFolderDialog(IPnBndDoc doc, out bool? result, string initialPath)
	{
		if (_autoMode.PopupsEnabled)
		{
			RadOpenFolderDialog radOpenFolderDialog = new RadOpenFolderDialog();
			radOpenFolderDialog.InitialDirectory = initialPath;
			radOpenFolderDialog.Multiselect = false;
			bool num = _windowBlock.IsWaitCursor(doc);
			if (num)
			{
				_windowBlock.CloseWait(doc);
			}
			radOpenFolderDialog.ShowDialog();
			if (num)
			{
				_windowBlock.InitWait(doc);
			}
			result = radOpenFolderDialog.DialogResult == true;
			return radOpenFolderDialog.FileName;
		}
		result = null;
		return initialPath;
	}
}
