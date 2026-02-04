using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.PKernelFlow.WrapC;

public class PFS
{
	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int pfs842_(ref int inr, ref int index, byte[] str, ref int iret, int string_len);
}
