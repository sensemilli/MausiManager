namespace WiCAM.Pn4000.PN3D;

public interface IWaitCancel
{
	bool IsCancel { get; set; }

	string Message { get; set; }

	double? Progress { get; set; }
}
