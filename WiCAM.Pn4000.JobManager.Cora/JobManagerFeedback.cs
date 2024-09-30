using WiCAM.Pn4000.Jobdata.Interfaces;

namespace WiCAM.Pn4000.JobManager.Cora;

public class JobManagerFeedback : IPnControlPlateFeedback
{
	public string StartTime { get; set; }

	public string TimeStamp { get; set; }

	public string UserName { get; set; }
}
