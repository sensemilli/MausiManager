namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IToolMapping
{
	int ID { get; set; }

	int ToolProfileId { get; set; }

	string PPidentifier { get; set; }

	double MutePosition { get; set; }

	int AdapterProfileID { get; set; }

	int Hemming { get; set; }

	int Rotated { get; set; }
}
