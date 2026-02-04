using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.PKernelFlow.Adapters.Type;

public class FIntPtrArray
{
	private nint lcData;

	public int this[int key]
	{
		get
		{
			return Marshal.ReadInt32(this.lcData, key * 4);
		}
		set
		{
			Marshal.WriteInt32(this.lcData, key * 4, value);
		}
	}

	public FIntPtrArray(nint _lcData)
	{
		this.lcData = _lcData;
	}
}
