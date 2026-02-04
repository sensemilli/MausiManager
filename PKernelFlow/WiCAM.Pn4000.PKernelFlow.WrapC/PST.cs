using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.PKernelFlow.WrapC;

public class PST
{
	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int pst005_(ref int luwrit, ref int luread);

	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int pst028_(ref int inum, ref int luwrit, ref int luread);

	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int pst034_(ref int iok, ref int luwrit, ref int luread);

	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int pst049_(ref int inum, ref int iakt, ref int izok, ref int iunit);

	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int pst363_(byte[] name1, byte[] chpopn, ref int ipotab, byte[] name2, ref int iret, int name1_len, int chpopn_len, int name2_len);
}
