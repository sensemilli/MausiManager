using System;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager.Cora;

public interface IBufferedLogger : ILogger
{
	void Write(ILogger logger);

	void Exception(Exception exception);
}
