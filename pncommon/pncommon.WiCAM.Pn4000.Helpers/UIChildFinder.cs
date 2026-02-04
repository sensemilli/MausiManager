using System;
using System.Windows;
using System.Windows.Media;

namespace pncommon.WiCAM.Pn4000.Helpers;

public static class UIChildFinder
{
	public static DependencyObject FindChild(this DependencyObject reference, string childName, Type childType)
	{
		DependencyObject result = null;
		if (reference != null)
		{
			int childrenCount = VisualTreeHelper.GetChildrenCount(reference);
			for (int i = 0; i < childrenCount; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(reference, i);
				if (child.GetType() != childType)
				{
					result = child.FindChild(childName, childType);
					continue;
				}
				if (!string.IsNullOrEmpty(childName))
				{
					if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
					{
						result = child;
						break;
					}
					continue;
				}
				result = child;
				break;
			}
		}
		return result;
	}
}
