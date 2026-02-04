using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WiCAM.Pn4000.pn4.pn4Services;

public static class WindowEvents
{
	[Serializable]
	public struct POINT
	{
		public int X;

		public int Y;

		public POINT(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	private class DROPFILES
	{
		public int size;

		public POINT pt;

		public bool fND;

		public bool WIDE;
	}

	public class WindowHandleInfo
	{
		private delegate bool EnumWindowProc(nint hwnd, nint lParam);

		private nint _MainHandle;

		[DllImport("user32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumChildWindows(nint window, EnumWindowProc callback, nint lParam);

		public WindowHandleInfo(nint handle)
		{
			this._MainHandle = handle;
		}

		public List<nint> GetAllChildHandles()
		{
			List<nint> list = new List<nint>();
			GCHandle value = GCHandle.Alloc(list);
			nint lParam = GCHandle.ToIntPtr(value);
			try
			{
				WindowHandleInfo.EnumChildWindows(callback: EnumWindow, window: this._MainHandle, lParam: lParam);
				return list;
			}
			finally
			{
				value.Free();
			}
		}

		private bool EnumWindow(nint hWnd, nint lParam)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(lParam);
			if (gCHandle.Target == null)
			{
				return false;
			}
			(gCHandle.Target as List<nint>).Add(hWnd);
			return true;
		}
	}

	public const int WM_USER = 1024;

	public const int WM_MOUSEWHEEL = 522;

	public const int WM_MOUSEMOVE = 512;

	public const int WM_KEYDOWN = 256;

	public const int WM_DROPFILES = 563;

	public const int WM_ERASEBKGND = 20;

	public const int WM_PAINT = 15;

	public static string parametertransferfile = "parameterstransfer.tmp";

	[DllImport("user32")]
	public static extern bool PostMessage(nint hwnd, int msg, nint wparam, nint lparam);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

	[DllImport("shell32.dll", CharSet = CharSet.Auto)]
	public static extern int DragQueryFile(nint hDrop, uint iFile, StringBuilder lpszFile, int cch);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport("Kernel32.dll", SetLastError = true)]
	private static extern int GlobalLock(nint Handle);

	[DllImport("Kernel32.dll", SetLastError = true)]
	private static extern int GlobalUnlock(nint Handle);

	public static ushort HIWORD(nint dwValue)
	{
		return (ushort)(((long)dwValue >> 16) & 0xFFFF);
	}

	public static ushort HIWORD(uint dwValue)
	{
		return (ushort)(dwValue >> 16);
	}

	public static int GET_WHEEL_DELTA_WPARAM(nint wParam)
	{
		return (short)WindowEvents.HIWORD(wParam);
	}

	public static int GET_WHEEL_DELTA_WPARAM(uint wParam)
	{
		return (short)WindowEvents.HIWORD(wParam);
	}

	public static string GetWindowTextByCall(nint handle)
	{
		StringBuilder stringBuilder = new StringBuilder(1000);
		WindowEvents.GetWindowText(handle, stringBuilder, 1000);
		return stringBuilder.ToString();
	}

	public static List<nint> FindWindowsContainName(string name)
	{
		List<nint> allChildHandles = new WindowHandleInfo(IntPtr.Zero).GetAllChildHandles();
		List<nint> list = new List<nint>();
		foreach (nint item in allChildHandles)
		{
			if (WindowEvents.GetWindowTextByCall(item).Contains(name))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static void DropFile(nint hwnd, string filePath)
	{
		DROPFILES anything = new DROPFILES
		{
			size = 20,
			pt = new POINT(10, 10),
			fND = false,
			WIDE = false
		};
		string text = filePath + "\0";
		int num = Convert.ToInt32(text.Length);
		byte[] array = WindowEvents.RawSerialize(anything);
		int num2 = array.Length;
		nint num3 = Marshal.AllocHGlobal(num2 + num + 1);
		WindowEvents.GlobalLock(num3);
		int num4 = 0;
		for (num4 = 0; num4 < num2; num4++)
		{
			Marshal.WriteByte(num3, num4, array[num4]);
		}
		byte[] bytes = Encoding.ASCII.GetBytes(text);
		for (int i = 0; i < num; i++)
		{
			Marshal.WriteByte(num3, num4, bytes[i]);
			num4++;
		}
		Marshal.WriteByte(num3, num4, 0);
		WindowEvents.GlobalUnlock(num3);
		WindowEvents.PostMessage(hwnd, 563, num3, IntPtr.Zero);
		Marshal.FreeHGlobal(num3);
	}

	public static byte[] RawSerialize(object anything)
	{
		int num = Marshal.SizeOf(anything);
		nint num2 = Marshal.AllocHGlobal(num);
		Marshal.StructureToPtr(anything, num2, fDeleteOld: false);
		byte[] array = new byte[num];
		Marshal.Copy(num2, array, 0, num);
		Marshal.FreeHGlobal(num2);
		return array;
	}
}
