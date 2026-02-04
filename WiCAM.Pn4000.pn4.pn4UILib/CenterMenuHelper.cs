using System.Windows;
using WiCAM.Pn4000.Contracts.PnCommands;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public class CenterMenuHelper
{
	public CenterMenu _centerMenu;

	private bool _isCenterMenuInit;

	public Window Owner;

	private string _activeModule;

	public CenterMenuHelper(CenterMenu centerMenu)
	{
		_centerMenu = centerMenu;
	}

	public void Init()
	{
		_centerMenu.Show();
		_centerMenu.Hide();
	}

	public bool AddCommandIfVisible(IPnCommand command)
	{
		if (!_isCenterMenuInit)
		{
			return false;
		}
		if (_centerMenu.Visibility != 0)
		{
			return false;
		}
		_centerMenu.AddCommand(command);
		return true;
	}

	internal void ActivateModule(string moduleName)
	{
		_activeModule = moduleName;
		_centerMenu.ActivateModule(moduleName);
	}

	public void OnCenterMenuShow(int x, int y, string filename, bool reload)
	{
		if (!_isCenterMenuInit)
		{
			if (Owner != null)
			{
				_centerMenu.Owner = Owner;
			}
			if (_activeModule != null)
			{
				_centerMenu.ActivateModule(_activeModule);
			}
			_isCenterMenuInit = true;
		}
		_centerMenu.OnCenterMenuShow(x, y, filename, reload);
	}
}
