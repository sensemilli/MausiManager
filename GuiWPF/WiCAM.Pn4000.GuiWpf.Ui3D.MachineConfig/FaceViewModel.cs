using System;
using System.Collections.Generic;
using System.Linq;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine.FingerStop;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.ToolCalculationGuiWpf.EditTools.SubViews;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class FaceViewModel : ICustomAutoCompleteBoxViewModel
{
	public IEnumerable<ICustomAutoCompleteBoxViewModel.IItem> CustomAutoCompleteBoxItems { get; }

	public FaceViewModel(Face face, IStopCombinations combinations)
	{
		StopCombinationType mask = face.FaceType switch
		{
			FaceType.Flat => StopCombinationType.FlatFaceMask | StopCombinationType.SupportFaceMask, 
			FaceType.RoundCylindricalConvex => StopCombinationType.CylinderMask, 
			_ => throw new Exception("Invalid face given"), 
		};
		List<FaceNameViewModel> list = (from x in (from x in combinations.Combinations
				select x.Type & mask into x
				where x.Any()
				select x).SelectMany((StopCombinationType x) => x.ToFaceNames()).Distinct()
			orderby x
			select new FaceNameViewModel(x)).ToList();
		list.Insert(0, new FaceNameViewModel(string.Empty, "None"));
		CustomAutoCompleteBoxItems = list;
	}
}
