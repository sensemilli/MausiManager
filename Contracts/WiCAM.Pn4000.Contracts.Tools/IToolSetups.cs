using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolSetups : IToolCluster
{
	string Desc { get; set; }

	int MountTypeIdUpperBeam { get; set; }

	int MountTypeIdLowerBeam { get; set; }

	int MachineNumber { get; set; }

	double LowerBeamXStart { get; }

	double LowerBeamXEnd { get; }

	double UpperBeamXStart { get; }

	double UpperBeamXEnd { get; }

	IEnumerable<IAcbPunchPiece> Acbs => this.AllSections.Where((IToolSection x) => x.IsUpperSection).SelectMany((IToolSection x) => x.Pieces).OfType<IAcbPunchPiece>();

	double SpaceLeft => (from section in EnumerableExtensions.SelectAndManyRecursive<IToolCluster>(EnumerableExtensions.ToIEnumerable<IToolCluster>((IToolCluster)this), (Func<IToolCluster, IEnumerable<IToolCluster>>)((IToolCluster x) => x.Children)).SelectMany((IToolCluster x) => x.Sections.Where((IToolSection s) => s.HasTools))
		select section.XMin.Value - (section.IsUpperSection ? this.LowerBeamXStart : this.UpperBeamXStart)).DefaultIfEmpty(0.0).Min();

	double SpaceRight => (from section in EnumerableExtensions.SelectAndManyRecursive<IToolCluster>(EnumerableExtensions.ToIEnumerable<IToolCluster>((IToolCluster)this), (Func<IToolCluster, IEnumerable<IToolCluster>>)((IToolCluster x) => x.Children)).SelectMany((IToolCluster x) => x.Sections.Where((IToolSection s) => s.HasTools))
		select (section.IsUpperSection ? this.UpperBeamXEnd : this.LowerBeamXEnd) - section.XMax.Value).DefaultIfEmpty(0.0).Min();

	new IEnumerable<IToolSection> AllSections => EnumerableExtensions.SelectAndManyRecursive<IToolCluster>(EnumerableExtensions.ToIEnumerable<IToolCluster>((IToolCluster)this), (Func<IToolCluster, IEnumerable<IToolCluster>>)((IToolCluster x) => x.Children)).SelectMany((IToolCluster x) => x.Sections);

	int MountTypeIdBeam(bool upper)
	{
		if (!upper)
		{
			return this.MountTypeIdLowerBeam;
		}
		return this.MountTypeIdUpperBeam;
	}

	bool HasTools()
	{
		return this.AllSections.Any((IToolSection x) => x.HasTools);
	}
}
