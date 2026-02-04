using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.pn4.pn4FlowCenter;

public class MemoryDisk : List<string>
{
	public delegate string Callback_rc_i(int param);

	public delegate int Callback_ri_0();

	public delegate void Callback_r0_0();

	public delegate void Callback_r0_c(nint param);

	public delegate int Callback_ri_i_c(int p1, nint p2);

	public delegate int Callback_ri_c(nint param);

	public delegate void Callback_r0_i(int p1);

	private Callback_rc_i MemoryDiskGetAtCallBack;

	private Callback_ri_0 MemoryDiskCountCallBack;

	private Callback_r0_0 MemoryDiskCleanCallBack;

	private Callback_r0_c MemoryDiskAddCallBack;

	public void Initialize()
	{
		this.MemoryDiskGetAtCallBack = LocalMemoryDiskGetAtCallBack;
		this.MemoryDiskCountCallBack = LocalMemoryDiskCountCallBack;
		this.MemoryDiskCleanCallBack = LocalMemoryDiskCleanCallBack;
		this.MemoryDiskAddCallBack = LocalMemoryDiskAddCallBack;
		MemoryDisk.SetMemoryDiskCallback(this.MemoryDiskGetAtCallBack, this.MemoryDiskCountCallBack, this.MemoryDiskCleanCallBack, this.MemoryDiskAddCallBack);
	}

	[DllImport("pkernel.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private static extern void SetMemoryDiskCallback(Callback_rc_i c1, Callback_ri_0 c2, Callback_r0_0 c3, Callback_r0_c c4);

	private void LocalMemoryDiskAddCallBack(nint param)
	{
		base.Add(Marshal.PtrToStringAnsi(param));
	}

	private void LocalMemoryDiskCleanCallBack()
	{
		base.Clear();
	}

	private int LocalMemoryDiskCountCallBack()
	{
		return base.Count;
	}

	[return: MarshalAs(UnmanagedType.LPStr)]
	private string LocalMemoryDiskGetAtCallBack(int param)
	{
		if (param < 0)
		{
			return string.Empty;
		}
		if (param >= base.Count)
		{
			return string.Empty;
		}
		return base[param];
	}

	public void CopyStringListToMemoryDisk(List<string> strs)
	{
		base.Clear();
		base.AddRange(strs);
	}
}
