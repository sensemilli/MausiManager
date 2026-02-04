using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.Contracts.BendDataBase;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IPreferredProfileToolSet
{
	int Priority { get; }

	IPreferredProfile PreferredProfiles { get; }

	BendMethod BendMethod { get; }

	IPunchGroup? UpperGroup { get; set; }

	IDieGroup? LowerGroup { get; set; }

	IToolProfile? UpperTool { get; set; }

	IToolProfile? LowerTool { get; set; }

	IEnumerable<IToolProfile> UpperTools { get; }

	IEnumerable<IToolProfile> LowerTools { get; }

	IToolProfile? FoldDieExtension { get; set; }

	double? PrebendAngle { get; }

	IPunchGroup? PrebendUpperGroup { get; set; }

	IDieGroup? PrebendLowerGroup { get; set; }

	IToolProfile? PrebendUpperTool { get; set; }

	IToolProfile? PrebendLowerTool { get; set; }

	IEnumerable<IToolProfile> PrebendUpperTools { get; }

	IEnumerable<IToolProfile> PrebendLowerTools { get; }

	bool IsValid
	{
		get
		{
			if (this.UpperTools.Any() && this.LowerTools.Any() && (this.BendMethod != BendMethod.FoldPrebendAndFoldWithSameDieAndPunchAndExtention || this.FoldDieExtension != null))
			{
				if (this.BendMethod != 0)
				{
					if (this.PrebendUpperTools.Any())
					{
						return this.PrebendLowerTools.Any();
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}
}
