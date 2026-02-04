using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Data;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendServices;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.GuiContracts.Implementations;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf.TabBend.PpRenameProgramNumber;

internal class PpRenameProgramNumberViewModel : ViewModelBase
{
	public class SubProgram : ViewModelBase
	{
		private string _name;

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (_name != value)
				{
					_name = value;
					NotifyPropertyChanged("Name");
				}
			}
		}

		public bool Used { get; set; }

		public string BendNumbers { get; set; }
	}

	private IDoc3d _doc;

	private bool _multiRoots;

	public RadObservableCollection<SubProgram> SubPrograms { get; } = new RadObservableCollection<SubProgram>();

	public int? ProgramNumber { get; set; }

	public string ModelName { get; set; }

	public string NamePPSuffix { get; set; }

	public ICommand CmdNewNumber { get; }

	public ICommand CmdCommit { get; }

	public ICommand CmdCancel { get; }

	public bool MultiRoots
	{
		get
		{
			return _multiRoots;
		}
		set
		{
			if (_multiRoots != value)
			{
				_multiRoots = value;
				NotifyPropertyChanged("MultiRoots");
				NotifyPropertyChanged("MultiRootVisibility");
				NotifyPropertyChanged("SingleRootVisibility");
			}
		}
	}

	public Visibility MultiRootVisibility
	{
		get
		{
			if (!MultiRoots)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public Visibility SingleRootVisibility
	{
		get
		{
			if (!MultiRoots)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public F2exeReturnCode ResultCode { get; private set; } = F2exeReturnCode.Undefined;

	public event Action<F2exeReturnCode> OnResult;

	public PpRenameProgramNumberViewModel()
	{
		CmdNewNumber = new WiCAM.Pn4000.GuiContracts.Implementations.RelayCommand(GenerateNewNumber);
		CmdCommit = new WiCAM.Pn4000.GuiContracts.Implementations.RelayCommand(Commit);
		CmdCancel = new WiCAM.Pn4000.GuiContracts.Implementations.RelayCommand(Cancel);
	}

	public void Init(IDoc3d doc)
	{
		_doc = doc;
		ProgramNumber = doc.NumberPp ?? _doc.BendMachine.GeneratePpNumber();
		ModelName = doc.DiskFile?.Header?.ModelName ?? "";
		NamePPSuffix = doc.NamePPSuffix;
		LoadSubPrograms(doc);
		MultiRoots = SubPrograms.Count > 1;
	}

	private void LoadSubPrograms(IDoc3d doc)
	{
		SubPrograms.Clear();
		List<string> list = doc.NamesPpBase.ToList();
		List<(int, List<ICombinedBendDescriptor>)> list2 = doc.BendsPerPp(doc.CombinedBendDescriptors).ToList();
		foreach (var item3 in list2)
		{
			int item = item3.Item1;
			List<ICombinedBendDescriptor> item2 = item3.Item2;
			string text = ((item <= list.Count) ? list[item - 1] : string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = doc.GetAutoPpName(list2.Count > 1, item, ProgramNumber);
			}
			SubPrograms.Add(new SubProgram
			{
				Used = true,
				BendNumbers = item2.Select((ICombinedBendDescriptor x) => x.Order + 1).ToRangedString(),
				Name = text
			});
		}
		for (int i = SubPrograms.Count; i < list.Count; i++)
		{
			SubPrograms.Add(new SubProgram
			{
				Used = false,
				BendNumbers = "-",
				Name = list[i]
			});
		}
		if (SubPrograms.Count == 0)
		{
			SubPrograms.Add(new SubProgram
			{
				Used = false,
				BendNumbers = "-",
				Name = doc.GetAutoPpName(multi: false, 1, ProgramNumber)
			});
		}
	}

	private void GenerateNewNumber()
	{
		ProgramNumber = _doc.BendMachine.GeneratePpNumber();
		NotifyPropertyChanged("ProgramNumber");
		for (int i = 0; i < SubPrograms.Count; i++)
		{
			SubPrograms[i].Name = _doc.GetAutoPpName(SubPrograms.Count > 1, i + 1, ProgramNumber);
		}
	}

	private void Commit()
	{
		_doc.NumberPp = ProgramNumber;
		_doc.SetNamesPpBase(SubPrograms.Select((SubProgram x) => x.Name));
		ResultCode = F2exeReturnCode.OK;
		this.OnResult?.Invoke(ResultCode);
		_doc.Factorio.Resolve<IUndo3dService>().Save(_doc, _doc.Factorio.Resolve<ITranslator>().Translate("Undo3d.PpNo"));
	}

	private void Cancel()
	{
		ResultCode = F2exeReturnCode.CANCEL_BY_USER;
		this.OnResult?.Invoke(ResultCode);
	}
}
