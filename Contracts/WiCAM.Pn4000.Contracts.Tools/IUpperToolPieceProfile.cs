namespace WiCAM.Pn4000.Contracts.Tools;

public interface IUpperToolPieceProfile : IToolPieceProfile
{
	bool IsHeelLeft { get; set; }

	bool IsHeelRight { get; set; }

	bool IsSensorTool { get; set; }

	double? LengthPlug { get; set; }
}
