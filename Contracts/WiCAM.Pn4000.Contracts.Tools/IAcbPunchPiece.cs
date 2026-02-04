namespace WiCAM.Pn4000.Contracts.Tools;

public interface IAcbPunchPiece : IToolPiece
{
	ISensorDisk? SensorDisks { get; set; }
}
