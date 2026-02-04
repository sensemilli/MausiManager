namespace WiCAM.Pn4000.pn4.pn4Services;

public interface IPipLstEntry
{
	int CommandGroup { get; set; }

	string CommandName { get; set; }

	int Id1 { get; set; }

	int Id2 { get; set; }

	string DefaultText { get; set; }

	string CurrentText { get; set; }
}
