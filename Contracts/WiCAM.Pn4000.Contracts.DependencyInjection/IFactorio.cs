using System;

namespace WiCAM.Pn4000.Contracts.DependencyInjection;

public interface IFactorio
{
	object? Resolve(Type t);

	T Resolve<T>();

	T CreateNew<T>(params object[] p);

	bool ResolveAll();
}
