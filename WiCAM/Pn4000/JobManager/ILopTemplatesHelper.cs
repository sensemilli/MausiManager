using System.Collections.Generic;

namespace WiCAM.Pn4000.JobManager;

public interface ILopTemplatesHelper
{
	IEnumerable<LopTemplateInfo> FindTemplates(string action);

	IEnumerable<LopTemplateInfo> FindTemplates(string action, string type);

	void PrepareTemplates(string path);
}
