using System;
using System.Threading.Tasks;

namespace WiCAM.Pn4000.Contracts.Coras;

public interface ICoraService
{
	Uri CoraUrl { get; }

	bool IsConfigured { get; }

	void OpenLoginPage();

	Task Initialize();
}
