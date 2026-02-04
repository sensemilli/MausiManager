using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public static class WindowsServices
{
	private const int WsExTransparent = 32;

	private const int GwlExstyle = -20;

	[DllImport("user32.dll")]
	private static extern int GetWindowLong(nint hwnd, int index);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(nint hwnd, int index, int newStyle);

	public static void SetWindowExTransparent(nint hwnd)
	{
		int windowLong = GetWindowLong(hwnd, -20);
		SetWindowLong(hwnd, -20, windowLong | 0x20);
	}
}
