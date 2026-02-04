using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.GuiWpf.UiBasic;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.ScreenD3D.Controls;

namespace WiCAM.Pn4000.GuiWpf.Assembly;

public interface IAssemblyViewModel
{
	bool AssemblyLoadingHidden { get; set; }

	string AssemblyLoadingInfo { get; }

	Screen3D ImageAssembly3D { get; set; }

	ObservableCollection<DisassemblyPartViewModel> ListParts { get; }

	ObservableCollection<DisassemblyNodeViewModel> HirarchicParts { get; }

	IEnumerable<DisassemblyNodeViewModel> AllHirarchicParts { get; }

	ObservableCollection<DisassemblyNodeViewModel> ListWithPurchasedParts { get; }

	DisassemblyPartViewModel CurrentPropertyPart { get; set; }

	DisassemblyPartViewModel CurrentHighlightedPart { get; set; }

	List<IMaterialArt> Materials { get; set; }

	Dictionary<int, string> PurchasedPartTypes { get; }

	List<ComboboxEntry<int>> PuchasedParts { get; }

	ICommand CmdCancel { get; }

	ICommand CmdOpenSelectedParts { get; }

	List<MaterialEntry> MaterialAsignments { get; set; }

	ObservableCollection<MaterialWicam> WicamMaterialList { get; set; }

	event Action AnalyzingStatusChanged;

	event Action<object> OnScrollIntoView;

	bool Init(Action<UserControl> endView, WiCAM.Pn4000.PN3D.Assembly.Assembly assembly, IImportArg importSetting, Action<F2exeReturnCode> answer);
}
