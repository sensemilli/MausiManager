using System;
using System.Collections.Generic;
using System.Windows;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PN3D.Popup.Information;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.View;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Unfold;

public class UnfoldSettingPopupManager
{
	public static void Start(IPnPathService pathService, IConfigProvider configProvider, IScreen3D screen3d, IModelFactory modelFactory)
	{
		PopupUnfoldSettingModel model = new PopupUnfoldSettingModel(configProvider)
		{
			Machines = UnfoldSettingPopupManager.GetMachines(pathService, modelFactory.Resolve<IMachineBendFactory>()).ToObservableCollection()
		};
		PopupUnfoldSettingViewModel popupUnfoldSettingViewModel = modelFactory.Resolve<PopupUnfoldSettingViewModel>();
		popupUnfoldSettingViewModel.Init(pathService.PNDRIVE, model);
		PopupBase popupBase = modelFactory.Resolve<IPopupUnfoldSettingView>() as PopupBase;
		popupUnfoldSettingViewModel.subPopup = modelFactory.Resolve<SubPopupForPopup>().Init(popupBase);
		popupBase.Owner = modelFactory.Resolve<IMainWindowDataProvider>() as Window;
		popupBase.OnClosingAction = (Action<EPopupCloseReason>)Delegate.Combine(popupBase.OnClosingAction, new Action<EPopupCloseReason>(popupUnfoldSettingViewModel.ViewCloseAction));
		popupBase.DataContext = popupUnfoldSettingViewModel;
		screen3d.IgnoreMouseMove(ignore: true);
		modelFactory.Resolve<IShowPopupService>().Show(popupBase, popupBase.CloseByRightButtonClickOutsideWindow);
		screen3d.IgnoreMouseMove(ignore: false);
	}

	private static IEnumerable<IBendMachineSummary> GetMachines(IPnPathService pathService, IMachineBendFactory machineBendFactory)
	{
		return machineBendFactory.GetMachineSummaries();
	}
}
