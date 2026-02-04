
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WiCAM.Pn4000.Contracts.Threading;
using WiCAM.Pn4000.Contracts.WindowsApi;
using WiCAM.Services.Loggers.Contracts;


namespace Sense3D.SenseScreen3D.PN3D.Doc;

public class ProcessPool : IProcessPool
{
	private WiCAM.Pn4000.Contracts.WindowsApi.Kernel32.GROUP_AFFINITY[] _groupAffinities = Array.Empty<WiCAM.Pn4000.Contracts.WindowsApi.Kernel32.GROUP_AFFINITY>();

	private readonly ConcurrentDictionary<int, Process> _processes = new ConcurrentDictionary<int, Process>();

	public ProcessPool(IWiLogger logger)
	{
	}

	public void Register(Process process)
	{
		_processes.TryAdd(process.Id, process);
		SetCpuGroupAffinity(process);
		foreach (KeyValuePair<int, Process> process2 in _processes)
		{
			Process value;
			try
			{
				if (process.HasExited)
				{
					_processes.Remove(process2.Key, out value);
				}
			}
			catch (Exception)
			{
				_processes.Remove(process2.Key, out value);
			}
		}
	}

	public void SetCpuGroupAffinity(Kernel32.GROUP_AFFINITY[] groups)
	{
		_groupAffinities = groups;
		ApplyAffinity();
	}

	private void ApplyAffinity()
	{
		foreach (KeyValuePair<int, Process> process in _processes)
		{
			Process value;
			try
			{
				if (process.Value.HasExited)
				{
					_processes.Remove(process.Key, out value);
				}
			}
			catch (Exception)
			{
				_processes.Remove(process.Key, out value);
			}
			SetCpuGroupAffinity(process.Value);
		}
	}

	private void SetCpuGroupAffinity(Process process)
	{
		try
		{
			if (!_groupAffinities.Any())
			{
				return;
			}
			nint num = Kernel32.OpenProcess(5632u, bInheritHandle: false, process.Id);
			if (num == IntPtr.Zero)
			{
				return;
			}
			try
			{
				Kernel32.SetProcessDefaultCpuSetMasks(num, _groupAffinities, (ushort)_groupAffinities.Length);
			}
			finally
			{
				Kernel32.CloseHandle(num);
			}
		}
		catch (Exception)
		{
		}
	}
}
