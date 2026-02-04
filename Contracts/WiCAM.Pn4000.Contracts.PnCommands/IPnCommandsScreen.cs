namespace WiCAM.Pn4000.Contracts.PnCommands;

public interface IPnCommandsScreen
{
	void SelectPoint3D(IPnCommandArg arg);

	void Distance3D(IPnCommandArg arg);

	void Angle3D(IPnCommandArg arg);

	void V3D_J3CSHA(IPnCommandArg arg);

	void V3D_J3FO(IPnCommandArg arg);

	void V3D_J3BLCH(IPnCommandArg arg);

	void V3D_WIRESMODE(IPnCommandArg arg);

	void V3D_TRA3D100(IPnCommandArg arg);

	void V3D_TRA3D75(IPnCommandArg arg);

	void V3D_TRA3D50(IPnCommandArg arg);

	void V3D_TRA3D25(IPnCommandArg arg);

	void V3D_ROT3DFREE(IPnCommandArg arg);

	void V3D_ROT3DX(IPnCommandArg arg);

	void V3D_ROT3DY(IPnCommandArg arg);

	void V3D_ROT3DZ(IPnCommandArg arg);
}
