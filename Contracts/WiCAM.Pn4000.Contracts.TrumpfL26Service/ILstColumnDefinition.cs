namespace WiCAM.Pn4000.Contracts.TrumpfL26Service;

public interface ILstColumnDefinition
{
	int Id { get; set; }

	string Name { get; set; }

	string Unit { get; set; }

	string Type { get; set; }
}
