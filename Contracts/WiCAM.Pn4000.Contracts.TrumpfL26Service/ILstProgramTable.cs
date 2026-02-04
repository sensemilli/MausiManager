using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.TrumpfL26Service;

public interface ILstProgramTable
{
	string Name { get; set; }

	IList<ILstColumnDefinition> Columns { get; set; }

	IList<ILstData> Data { get; set; }
}
