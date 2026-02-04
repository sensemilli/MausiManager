using System;
using System.Collections.Generic;
using System.Windows.Controls;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4Services;

namespace WiCAM.Pn4000.pn4.Interfaces;

public interface IMainWindowDataProvider : IRibbon
{
	int Active_screen_layout { get; set; }

	LikeModalMode GetLikeModalMode();

	void BlockUI_Command(string command);

	void OnScreenInfo_ClearStrings();

	void OnScreenInfo_CalculateLocation(int ID);

	void OnScreenInfo_UpdateString(int ID, string text);

	object Get3DKernel();

	void AddRecentlyUsedRecord(RecentlyUsedRecord rec);

	void DeleteRecentlyUsedRecord(RecentlyUsedRecord rec);

	void DelRecentlyUsedRecordForType(string type);

	void SetAllRecentlyUsedRecordForType(string type, IEnumerable<RecentlyUsedRecord> allRecords);

	IEnumerable<RecentlyUsedRecord> GetRecentlyUsedMachineRecords();

	IEnumerable<RecentlyUsedRecord> GetRecentlyUsedMaterialRecords();

	IEnumerable<RecentlyUsedRecord> GetRecentlyUsedImportRecords();

	void Ribbon_Activate3DViewTab();

	void Ribbon_ActivateBendTab(bool force = false);

	void Ribbon_ActivateUnfoldTab(bool force = false);

	void Ribbon_Activate3DTab(bool force = false);

	void Ribbon_CloseSubMenu();

	void Ribbon_ShowSubMenu(string TabName, string RibbonFileName, int AssignedToTabID);

	void Print3D();

	void SetViewerTab();

	void SetUnfoldTab();

	void SetViewForConfig(object element);

	UserControl GetActualConfig();

	void Set2D();

	bool Is2DAlternativeScreen();

	void Set2DAlternativeScreen(UserControl element, Action alternativeClosingAction);

	void StartHelp(string function_name);
}
