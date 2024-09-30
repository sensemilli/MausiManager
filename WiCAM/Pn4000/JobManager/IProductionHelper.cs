using WiCAM.Pn4000.Jobdata.Classes;

namespace WiCAM.Pn4000.JobManager;

public interface IProductionHelper
{
	void WritePartLop(PlatePartProductionInfo part, int amount, LopTemplateInfo template, PlateProductionInfo plateProduction, Plate plateDat);
}
