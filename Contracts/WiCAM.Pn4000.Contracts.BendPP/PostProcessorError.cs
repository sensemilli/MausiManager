namespace WiCAM.Pn4000.Contracts.BendPP;

public class PostProcessorError
{
	public PostProcessorReturnCodes Code { get; set; }

	public string DetailsMessageKey { get; set; }

	public object? DetailsArg0 { get; set; }

	public object? DetailsArg1 { get; set; }

	public object? DetailsArg2 { get; set; }

	public object? DetailsArg3 { get; set; }

	public object? DetailsArg4 { get; set; }

	public object? DetailsArg5 { get; set; }
}
