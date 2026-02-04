using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.Contracts.WindowsApi;

public static class Kernel32
{
	public struct SYSTEM_CPU_SET_INFORMATION
	{
		public uint Size;

		public CPU_SET_INFORMATION_TYPE Type;

		public CPU_SET_INFORMATION CpuSet;
	}

	public enum CPU_SET_INFORMATION_TYPE
	{
		CpuSetInformation = 0
	}

	public struct CPU_SET_INFORMATION
	{
		public uint Id;

		public ushort Group;

		public byte LogicalProcessorIndex;

		public byte CoreIndex;

		public byte LastLevelCacheIndex;

		public byte NumaNodeIndex;

		public byte EfficiencyClass;

		public byte Reserved1;

		public byte Allocated;

		public byte AllocatedToTargetProcess;

		public byte RealTime;

		public byte Parked;

		public byte Reserved2;

		public ulong AllocationTag;
	}

	public struct GROUP_AFFINITY
	{
		public ulong Mask;

		public ushort Group;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public ushort[] Reserved;
	}

	public const uint PROCESS_SET_LIMITED_INFORMATION = 4096u;

	public const uint PROCESS_QUERY_INFORMATION = 1024u;

	public const uint PROCESS_SET_INFORMATION = 512u;

	public const uint THREAD_SET_LIMITED_INFORMATION = 1024u;

	public const uint THREAD_QUERY_INFORMATION = 64u;

	public const uint THREAD_SET_INFORMATION = 32u;

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetSystemCpuSetInformation(nint Information, uint BufferLength, ref uint ReturnedLength, nint Process, uint Flags);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetProcessDefaultCpuSetMasks(nint Process, [In] GROUP_AFFINITY[] CpuSetMasks, ushort CpuSetMaskCount);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetThreadSelectedCpuSetMasks(nint Process, [In] GROUP_AFFINITY[] CpuSetMasks, ushort CpuSetMaskCount);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetProcessDefaultCpuSets(nint Process, [In] uint[] CpuSetIds, uint CpuSetIdCount);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetThreadSelectedCpuSets(nint Thread, [In] uint[] CpuSetIds, uint CpuSetIdCount);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern nint OpenProcess(uint processAccess, bool bInheritHandle, int processId);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetProcessDefaultCpuSets(nint Process, [Out] uint[] CpuSetIds, uint CpuSetIdCount, out uint RequiredIdCount);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetProcessDefaultCpuSetMasks(nint Process, [Out] GROUP_AFFINITY[] CpuSetMasks, ushort CpuSetMaskCount, out ushort RequiredMaskCount);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetThreadSelectedCpuSetMasks(nint Thread, [Out] GROUP_AFFINITY[] CpuSetMasks, ushort CpuSetMaskCount, out ushort RequiredMaskCount);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetThreadSelectedCpuSets(nint Thread, [Out] uint[] CpuSetIds, uint CpuSetIdCount, out uint RequiredIdCount);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool ReadProcessMemory(nint hProcess, nint lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern nint OpenThread(uint desiredAccess, bool bInheritHandle, int threadId);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool CloseHandle(nint hObject);
}
