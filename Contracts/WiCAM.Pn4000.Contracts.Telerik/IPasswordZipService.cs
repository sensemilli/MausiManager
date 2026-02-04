namespace WiCAM.Pn4000.Contracts.Telerik;

public interface IPasswordZipService
{
	void ZipDirectory(string sourceDirectory, string targetFile, string password);
}
