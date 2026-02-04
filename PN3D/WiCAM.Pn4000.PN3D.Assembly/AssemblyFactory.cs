using System;

namespace WiCAM.Pn4000.PN3D.Assembly;

internal class AssemblyFactory : IAssemblyFactory
{
	public event Action<Assembly> OnCreated;

	public Assembly Create()
	{
		Assembly assembly = new Assembly();
		this.OnCreated?.Invoke(assembly);
		return assembly;
	}
}
