using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.PKernelFlow.WrapCommon;

public class ERASED
{
	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int erased_get_erased();

	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern void erased_set_erased(int value);
}
