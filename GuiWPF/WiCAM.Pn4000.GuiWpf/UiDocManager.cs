using System;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.GuiWpf;

public class UiDocManager : IUiDocManager
{
	private readonly ICurrentDocProvider _currentDocProvider;

	public IDoc3d CurrentDoc
	{
		get
		{
			return _currentDocProvider.CurrentDoc;
		}
		set
		{
			_currentDocProvider.CurrentDoc = value;
		}
	}

	public event Action<IDoc3d, IDoc3d>? CurrentDocChanged;

	public UiDocManager(ICurrentDocProvider currentDocProvider)
	{
		_currentDocProvider = currentDocProvider;
		_currentDocProvider.CurrentDocChanged += _pn3DGlobals_CurrentDocChanged;
	}

	private void _pn3DGlobals_CurrentDocChanged(IDoc3d docOld, IDoc3d docNew)
	{
		this.CurrentDocChanged?.Invoke(docOld, docNew);
	}
}
