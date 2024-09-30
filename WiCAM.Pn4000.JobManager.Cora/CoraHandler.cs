using System;
using System.Threading.Tasks;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager.Cora;

internal class CoraHandler
{
	private readonly CoraConnectionChecker _checker = new CoraConnectionChecker();

	public bool CheckConnection(string url)
	{
		try
		{
			Task<bool> task = Task.Run(async () => await _checker.IsCoraReachable(url));
			Task.WaitAll(task);
			return task.Result;
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
		}
		return false;
	}
}
