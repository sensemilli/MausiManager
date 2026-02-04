namespace WiCAM.Pn4000.Contracts.Common;

public interface IAutoMode
{
	bool PopupsEnabled { get; }

	bool ImportantPopupsEnabled { get; }

	bool HasGui { get; }

	void PkernelInitialized();

	void DeactivatePopups();

	void ActivatePopups();

	void SetEngineMode(bool active);
}
