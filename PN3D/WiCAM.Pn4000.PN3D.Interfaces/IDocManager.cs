using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.Interfaces;

public interface IDocManager
{
	IEnumerable<IDoc3d> Documents { get; }

	IEnumerable<IScopedFactorio> ScopedFactorios { get; }

	IScopedFactorio GetScope(IPnBndDoc doc);

	void CloseAllDocuments();
}
