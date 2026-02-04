namespace WiCAM.Pn4000.Contracts.TrumpfL26Service;

public interface ICachePart
{
	string ArtikelNr { get; set; }

	string AuftragNrOriginal { get; set; }

	string PositionNr { get; set; }

	string Bemerkung { get; set; }

	int ArchivNr { get; set; }

	string AuftragsNrBoost { get; set; }
}
