using System.Threading.Tasks;
using WiCAM.Pn4000.Jobdata.Classes;

namespace WiCAM.Pn4000.Contracts.TrumpfL26Service;

public interface IDatService
{
	Task WriteJobDat(Job job, string jobPath);

	Task WritePartDat(Part part, string partPath);

	Task WritePlateDat(Plate plate, string platePath);
}
