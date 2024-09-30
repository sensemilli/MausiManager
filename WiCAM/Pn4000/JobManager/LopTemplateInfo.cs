using System.Collections.Generic;
using System.Diagnostics;
using WiCAM.Pn4000.JobManager.Classes;

namespace WiCAM.Pn4000.JobManager;

[DebuggerDisplay("{Type}   {Company}   {LopType}")]
public class LopTemplateInfo
{
	public string Type { get; set; }

	public string Company { get; set; }

	public string LopType { get; set; }

	public string DestinationPath { get; set; }

	public int NameLength { get; set; }

	public string NameFormat { get; set; }

	public FeedbackFileType TypeOfFeedback { get; set; } = FeedbackFileType.LOP;


	public string Header { get; set; }

	public string Content { get; set; }

	public Dictionary<string, PropertyReference> Mapping { get; set; } = new Dictionary<string, PropertyReference>();

}
