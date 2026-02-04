using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.PKernelFlow.WrapC;

public class P3H
{
	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int p3h125_(ref int ianz);

	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int p3h145_(ref int i3e, ref int iele, ref int icol, ref float x1, ref float y1, ref float z1, ref float x2, ref float y2, ref float z2, ref float x3, ref float y3, ref float z3, ref int iret);

	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int p3h146_(ref int iret);
}
