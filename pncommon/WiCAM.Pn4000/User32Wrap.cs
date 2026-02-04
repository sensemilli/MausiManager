using System;
using System.Runtime.InteropServices;

namespace WiCAM.Pn4000;

public static class User32Wrap
{
	public struct POINT
	{
		private long X;

		private long Y;
	}

	public struct NativeMessage
	{
		public nint handle;

		public uint msg;

		public nint wParam;

		public nint lParam;

		public uint time;

		public POINT p;
	}

	[DllImport("user32.dll")]
	public static extern bool LockWindowUpdate(nint hWndLock);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool PeekMessage(out NativeMessage lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

	[DllImport("user32.dll")]
	private static extern bool TranslateMessage([In] ref NativeMessage lpMsg);

	[DllImport("user32.dll")]
	private static extern bool DispatchMessage([In] ref NativeMessage lpMsg);

	[DllImport("user32.dll")]
	public static extern bool WaitMessage();

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern nint SleepEx(uint time, bool flag);

	public static bool PumpMesseges()
	{
		NativeMessage lpMsg = default(NativeMessage);
		if (User32Wrap.PeekMessage(out lpMsg, IntPtr.Zero, 0u, 0u, 1u))
		{
			User32Wrap.TranslateMessage(ref lpMsg);
			User32Wrap.DispatchMessage(ref lpMsg);
			return true;
		}
		return true;
	}
}
