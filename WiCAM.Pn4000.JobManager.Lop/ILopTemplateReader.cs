namespace WiCAM.Pn4000.JobManager.Lop;

public interface ILopTemplateReader
{
	int ReadTemplate(LopTemplateInfo template, string content, int begin);
}
