namespace WiCAM.Pn4000.Contracts.Tools;

public interface IAdapterProfile : IToolProfile
{
	double? OffsetInX { get; set; }

	InstallationDirection SocketInstallationDirection { get; set; }

	bool? IsHolder { get; set; }

	ISocket Socket { get; set; }
}
