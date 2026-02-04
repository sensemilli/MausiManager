using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.PKernelFlow.Adapters.Data;
using WiCAM.Pn4000.pn4.pn4Services.CADGEO;

namespace WiCAM.Pn4000.pn4.pn4UILib.Popup;

internal interface IQuickTable
{
	string FilterString { get; set; }

	string FilterString1 { get; set; }

	string FilterString2 { get; set; }

	Popup MainPopupWindow { get; set; }

	void SetVisibility(Visibility v);

	void SetFocusable(bool v);

	Visibility CheckVisibility();

	void KeyboardFocus();

	void KeyboardFocusRightFilter();

	void MoveSelectedOneDown();

	void MoveSelectedOneUp();

	void MoveSelectedToEnd();

	void MoveSelectedToStart();

	void PageDown();

	void PageUp();

	void ScrollReset();

	void SendAnswer(string currFilterTabble, string currFilterTabble1, string currFilterTabble2);

	void SetControls(StackPanel filter_panel1, StackPanel filter_panel2, ComboBox filter_combo1, ComboBox filter_combo2, TextBox filter_text, TextBox filter_text1, TextBox filter_text2);

	void SetFilter(string empty1, string empty2, string empty3);

	void Setup(IFactorio factorio, List<PopupLine> lines, PnImageSource imageSource, Action exitCode2, int staticListMode, ComboBox filter_combo, bool updateFilters, Action<bool> updateEnableSortingButtons, int filtersCount, bool is_EditButtonVisible, string popupName);
}
