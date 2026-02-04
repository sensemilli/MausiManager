using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.PKernelFlow.WrapC;

public class PNT
{
	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int pnt000_(ref float xp, ref float yp, ref int iret);
}
