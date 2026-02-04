namespace WiCAM.Pn4000.Contracts.BendDataBase;

public interface IBendTableReader
{
	bool ReadFile(string file, IBendTable db);

	bool ReadFileContent(string fileContent, IBendTable db);
}
