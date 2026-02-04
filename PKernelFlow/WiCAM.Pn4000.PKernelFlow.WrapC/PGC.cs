using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.PKernelFlow.WrapC;

public class PGC
{
	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int pgc036_(ref float xa1, ref float ya1, ref int iret);
}
