using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.Information;
using WiCAM.Pn4000.Popup;

namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

public interface IBendSequenceSortViewModel : ISubViewModel
{
	public interface ISortPrioVm
	{
		BendSequenceSorts? SortType { get; set; }

		string Description { get; }

		string DescriptionLong { get; }

		event PropertyChangedEventHandler PropertyChanged;
	}

	IDoc3d DocTemp { get; }

	ObservableCollection<BendSequenceItem> BendSequence { get; }

	UiModelType CurrentModel { get; }

	BendSequenceItem SelectedBend { get; set; }

	RelayCommand CmdSequenceSelectItem { get; }

	RelayCommand CmdSequenceSave { get; }

	RelayCommand CmdSequenceCancel { get; }

	ISortPrioVm SelectedNewTranslation { get; set; }

	bool GroupParallesInAutoSort { get; set; }

	List<ISortPrioVm> AutoSortListTranslation { get; set; }

	ObservableCollection<ISortPrioVm> AutoSortList { get; set; }

	RelayCommand CmdAutoSortAdd { get; set; }

	RelayCommand CmdAutoSortDo { get; set; }

	RelayCommand CmdAutoSortClear { get; set; }

	RelayCommand CmdAutoSortDoDefault { get; set; }

	RelayCommand CmdDeleteItem { get; set; }

	RelayCommand CmdStartNewBendSequence { get; set; }

	ICommand CloseCommand { get; set; }

	event Action<BendSequenceItem> OnSelectedBendChanged;

	void Init(IDoc3d docTemp, IDoc3d docOriginal, UiModelType model);

	void SetSelectedBend(ICombinedBendDescriptorInternal bend);

	void Dispose();

	void CloseView();

	void ListBoxDoubleClick(object selectedItem);

	void TreeViewSelectedItemChanged(object selectedItem);

	void TreeViewDoubleClick(object selectedItem);

	void OnViewSourceInitialized(PopupBase popupBase);

	void OnViewSourceInitialized();

	void SetListViewColumnHeader(int id, string text);

	void TrySetupLearnButton(PopupModelBase popupModelBase);
}
