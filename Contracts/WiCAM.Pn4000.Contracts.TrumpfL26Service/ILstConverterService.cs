using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.TrumpfL26Service;

public interface ILstConverterService
{
	void ReadLst(string path);

	ILstProgramTable GetTable(string tabaleName);

	IEnumerable<string> GetColumn(string columnName, ILstProgramTable table);

	void RenameColumn(string oldName, string newName, string columnToRename);

	void SetColumn(string value, string columnToRename);

	new string ToString();
}
