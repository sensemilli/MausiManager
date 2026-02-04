using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.PKernelFlow.WrapC;

public class P3L
{
	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int l3clen_(ref int luwrit, ref int luread);
}
