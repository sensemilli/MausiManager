using System;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.Interfaces;

public interface ICurrentDocProvider
{
	IDoc3d CurrentDoc { get; set; }

	IScopedFactorio CurrentFactorio { get; }

	event Action<IDoc3d, IDoc3d> CurrentDocChanged;
}
