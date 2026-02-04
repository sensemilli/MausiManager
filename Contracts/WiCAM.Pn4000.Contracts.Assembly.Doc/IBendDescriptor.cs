namespace WiCAM.Pn4000.Contracts.Assembly.Doc;

public interface IBendDescriptor
{
	BendingType Type { get; }

	IBendParameters BendParams { get; }
}
