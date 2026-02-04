using System.Diagnostics;
using WiCAM.Pn4000.Contracts.WindowsApi;

namespace WiCAM.Pn4000.Contracts.Threading;

public interface IProcessPool
{
	void Register(Process process);

	void SetCpuGroupAffinity(Kernel32.GROUP_AFFINITY[] groups);
}
