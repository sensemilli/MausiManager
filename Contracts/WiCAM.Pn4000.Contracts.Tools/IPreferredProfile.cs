using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IPreferredProfile
{
	int Id { get; set; }

	int MaterialGroupID { get; set; }

	double Thickness { get; set; }

	double MinRadius { get; set; }

	double MaxRadius { get; set; }

	double MinAngle { get; set; }

	double MaxAngle { get; set; }

	IReadOnlyCollection<IPreferredProfileToolSet> AlternativeTools { get; }

	IPreferredProfileToolSet GetAlternativeProfile(ref int prio);

	IEnumerable<IPreferredProfileToolSet> GetAllAlternativeProfiles();

	void SetAllAlternativeProfiles(IEnumerable<IPreferredProfileToolSet> profiles);

	bool IsValidProfile(int matGroupId, double thickness, double thicknessDelta, double radius, double angle);
}
