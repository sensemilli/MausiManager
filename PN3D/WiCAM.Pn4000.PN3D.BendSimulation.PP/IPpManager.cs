using System;
using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP;

public interface IPpManager
{
	void ClearBendTempDir();

	F2exeReturnCode CreateNC(string fileName, IDoc3d doc, bool createNc, bool createReport, string reportFormat);

	F2exeReturnCode CreateReport(IDoc3d doc3d, string format);

	int CreateReportConvert(IDoc3d doc3d, string format, out List<Exception> errors);

	F2exeReturnCode GeneratePpName(IDoc3d doc3d);

	F2exeReturnCode SendPPFiles(IDoc3d doc, bool sendNc, bool sendReport);

	void CalculatePpInformation(IDoc3d doc);

	void ResetTempFolder(IDoc3d doc);
}
