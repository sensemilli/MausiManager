using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.PKernelFlow.WrapCommon;

public class XWICOM
{
	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int xwicom_get_iwx();

	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern void xwicom_set_iwx(int value);

	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int xwicom_get_iwy();

	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern void xwicom_set_iwy(int value);

	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int xwicom_get_iwsum();

	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern void xwicom_set_iwsum(int value);
}
