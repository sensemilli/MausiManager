using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.MachineAndTools.Implementations;
using ViewModelBase = WiCAM.Pn4000.Common.ViewModelBase;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class CrowningViewModel : ViewModelBase
{
	private ICommand? _importCvs;

	private IBendMachine _bendMachine;

	public ObservableCollection<CrowningItemViewModel> Items { get; set; } = new ObservableCollection<CrowningItemViewModel>();

	public ObservableCollection<CrowningColumnViewModel> Columns { get; set; } = new ObservableCollection<CrowningColumnViewModel>();

	public ICommand ImportCvs => _importCvs ?? (_importCvs = new RelayCommand<object>(delegate
	{
		RadOpenFileDialog radOpenFileDialog = new RadOpenFileDialog();
		if (radOpenFileDialog.ShowDialog() == true)
		{
			CrowningTable crowningTable = new CrowningTable();
			crowningTable.ImportCsv(radOpenFileDialog.FileName);
			Columns.Clear();
			Columns.AddRange(crowningTable.Lengths.Select((double len) => new CrowningColumnViewModel
			{
				Length = len
			}));
			Items.Clear();
			Items.AddRange(crowningTable.Entries.Select((ICrowningTableEntry kvp) => new CrowningItemViewModel
			{
				MatGrpId = kvp.MaterialGroupId,
				Thickness = kvp.Thickness,
				Values = kvp.Values.ToArray()
			}));
			this.DataChanged?.Invoke();
		}
	}));

	public event MethodInvoker DataChanged;

	public void Init(IBendMachine bendMachine)
	{
		_bendMachine = bendMachine;
		ICrowningTable crowningTable = _bendMachine.CrowningTable;
		if (crowningTable != null)
		{
			Columns.AddRange(crowningTable.Lengths.Select((double len) => new CrowningColumnViewModel
			{
				Length = len
			}));
			Items.AddRange(crowningTable.Entries.Select((ICrowningTableEntry kvp) => new CrowningItemViewModel
			{
				MatGrpId = kvp.MaterialGroupId,
				Thickness = kvp.Thickness,
				Values = kvp.Values.ToArray()
			}));
		}
		this.DataChanged?.Invoke();
	}

	public void Save()
	{
		IEnumerable<double> lengths = Columns.Select((CrowningColumnViewModel col) => col.Length);
		IEnumerable<CrowningTableEntry> entries = Items.Select((CrowningItemViewModel item) => new CrowningTableEntry
		{
			MaterialGroupId = item.MatGrpId,
			Thickness = item.Thickness,
			Values = item.Values.ToList()
		});
		_bendMachine.CrowningTable.SetData(lengths, entries);
	}
}
