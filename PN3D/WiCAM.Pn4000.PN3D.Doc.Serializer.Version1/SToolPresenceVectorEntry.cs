namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SToolPresenceVectorEntry
{
	public SToolPresenceVector Key { get; set; }

	public double Value { get; set; }

	public SToolPresenceVectorEntry(SToolPresenceVector key, double value)
	{
		this.Key = key;
		this.Value = value;
	}
}
