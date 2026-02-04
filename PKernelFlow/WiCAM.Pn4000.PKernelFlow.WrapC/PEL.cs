using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.PKernelFlow.WrapC;

public class PEL
{
	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int pel108_(ref int imode, ref float xpos, ref float ypos, ref int iqnum, ref int iqlift, ref int iqend, ref int iret);
}
