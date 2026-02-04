namespace WiCAM.Pn4000.Contracts.Screen;

public interface IRenderTaskBase
{
	long ID { get; }

	void Execute(IRenderer renderer);

	void Skip();
}
