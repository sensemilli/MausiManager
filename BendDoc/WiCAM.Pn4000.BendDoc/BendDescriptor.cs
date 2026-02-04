using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.BendDoc;

internal class BendDescriptor : IBendDescriptor
{
	private static int _idCounter;

	public int ID { get; set; }

	public BendingType Type { get; }

	public BendParametersBase BendParamsInternal { get; }

	public IBendParameters BendParams => this.BendParamsInternal;

	public BendDescriptor(BendingType type, BendParametersBase bendParams)
	{
		this.ID = BendDescriptor._idCounter++;
		this.Type = type;
		this.BendParamsInternal = bendParams;
	}

	public BendDescriptor(int id, BendingType type, BendParametersBase bendParams)
	{
		this.ID = id;
		this.Type = type;
		this.BendParamsInternal = bendParams;
	}

	internal BendDescriptor Copy(Doc3d doc)
	{
		return new BendDescriptor(this.ID, this.Type, this.BendParamsInternal.Copy(doc));
	}
}
