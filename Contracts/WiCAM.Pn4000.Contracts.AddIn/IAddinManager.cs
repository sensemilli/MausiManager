using System;

namespace WiCAM.Pn4000.Contracts.AddIn;

public interface IAddinManager
{
	void UseDi(IServiceProvider provider);
}
