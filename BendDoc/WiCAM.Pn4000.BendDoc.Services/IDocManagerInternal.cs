using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.BendDoc.Services;

public interface IDocManagerInternal : IDocManager
{
	internal IScopedFactorio CreateNewScope();
}
