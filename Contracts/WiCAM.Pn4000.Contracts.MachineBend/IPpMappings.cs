namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IPpMappings
{
	IToolMappings ToolMapping { get; set; }

	IMaterialMappings MaterialMapping { get; set; }
}
