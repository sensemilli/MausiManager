using System;

namespace WiCAM.Pn4000.PN3D.Assembly;

public interface IAssemblyFactory
{
	event Action<Assembly> OnCreated;

	Assembly Create();
}
