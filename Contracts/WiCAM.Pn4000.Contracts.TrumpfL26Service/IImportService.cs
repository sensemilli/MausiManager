using System.Threading.Tasks;

namespace WiCAM.Pn4000.Contracts.TrumpfL26Service;

public interface IImportService
{
	void ImportXml(string importPath);

	Task ImportTimes();
}
