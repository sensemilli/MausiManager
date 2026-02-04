using System.Windows;
using System.Windows.Media;

namespace WiCAM.Pn4000.pn4.pn4Services;

public interface IPnIconsService
{
	ImageSource GetSmallIcon(string name);

	ImageSource GetBigIcon(string name);

	void SetSmallIcon(DependencyObject obj, DependencyProperty prj, string name);

	void SetSmallIcon(object parent, DependencyObject obj, DependencyProperty prj, string name);

	void SetBigIcon(DependencyObject obj, DependencyProperty prj, string name);

	void SetBigIcon(object parent, DependencyObject obj, DependencyProperty prj, string name);

	void UnregisterIconData(DependencyObject obj);

	void SwapIcons(string keyName, string newIcon);
}
