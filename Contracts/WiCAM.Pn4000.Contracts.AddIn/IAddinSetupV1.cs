using System;
using Microsoft.Extensions.DependencyInjection;

namespace WiCAM.Pn4000.Contracts.AddIn;

public interface IAddinSetupV1
{
	string Description { get; }

	void RegisterDi(IServiceCollection services);

	void UseDi(IServiceProvider provider);
}
