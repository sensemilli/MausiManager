namespace WiCAM.Pn4000.Contracts.Screen;

public interface IRenderTaskResult
{
	bool IsSuccessfull { get; }

	IRenderTaskBase RenderTask { get; }
}
