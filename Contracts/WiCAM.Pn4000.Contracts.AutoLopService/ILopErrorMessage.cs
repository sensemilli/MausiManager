using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.AutoLopService;

public interface ILopErrorMessage
{
	int Id { get; set; }

	string Message { get; set; }

	double? X { get; set; }

	double? Y { get; set; }

	string File { get; set; }

	IEnumerable<string> Comments { get; set; }
}
