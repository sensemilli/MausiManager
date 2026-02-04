namespace WiCAM.Pn4000.Contracts.TrumpfL26Service;

public interface ICachePlate
{
	string Name { get; set; }

	string Batch { get; set; }

	string Bezeichnung { get; set; }

	string ReferenzNr { get; set; }

	string ArchivNr { get; set; }

	string Lagerort { get; set; }
}
