using System;
using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Telerik;

public interface IReportCreator
{
	bool Create(string filenameTrdp, string format, ref string filenameOutput, out List<Exception> exceptions);
}
