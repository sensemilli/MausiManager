using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnPathServices;

namespace WiCAM.Pn4000.pn4.pn4Services;

public class PnIconsService : IPnIconsService
{
	private class RegisterIcon
	{
		public object Parent { get; set; }

		public DependencyObject Obj { get; set; }

		public DependencyProperty Prj { get; set; }

		public string Name { get; set; }

		public bool Big { get; set; }
	}

	private IPnPathService _pnPathService;

	private ILogCenterService _logCenterService;

	private List<RegisterIcon> _registerIcons = new List<RegisterIcon>();

	private Dictionary<string, string> _swapList = new Dictionary<string, string>();

	public PnIconsService(IPnPathService pathService, ILogCenterService logCenterService)
	{
		this._pnPathService = pathService;
		this._logCenterService = logCenterService;
	}

	public ImageSource GetSmallIcon(string name)
	{
		if (name == null)
		{
			return null;
		}
		string name2 = name;
		if (this._swapList.ContainsKey(name))
		{
			name2 = this._swapList[name];
		}
		return this.GetIcon(name2, "16");
	}

	public ImageSource GetBigIcon(string name)
	{
		if (name == null)
		{
			return null;
		}
		string name2 = name;
		if (this._swapList.ContainsKey(name))
		{
			name2 = this._swapList[name];
		}
		return this.GetIcon(name2, "32");
	}

	private ImageSource GetIcon(string name, string group)
	{
		if (name == null)
		{
			return null;
		}
		ImageSource imageSource = this.SubGetIcon(name, group);
		if (imageSource != null)
		{
			return imageSource;
		}
		int num = name.IndexOf('(');
		if (num > 0)
		{
			imageSource = this.SubGetIcon(name.Substring(0, num), group);
			if (imageSource != null)
			{
				return imageSource;
			}
		}
		return null;
	}

	private ImageSource SubGetIcon(string name, string group)
	{
		string text = this._pnPathService.PixmapPng(group, name);
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.UriSource = new Uri(text);
				bitmapImage.EndInit();
				bitmapImage.Freeze();
				return bitmapImage;
			}
			catch (Exception e)
			{
				this._logCenterService.CatchRaport(e);
			}
		}
		return null;
	}

	public void SetSmallIcon(DependencyObject obj, DependencyProperty prj, string name)
	{
		this.SetSmallIcon(null, obj, prj, name);
	}

	public void SetSmallIcon(object parent, DependencyObject obj, DependencyProperty prj, string name)
	{
		this._registerIcons.Add(new RegisterIcon
		{
			Parent = parent,
			Obj = obj,
			Prj = prj,
			Name = name,
			Big = false
		});
		string name2 = name;
		if (this._swapList.ContainsKey(name))
		{
			name2 = this._swapList[name];
		}
		obj.SetValue(prj, this.GetIcon(name2, "16"));
	}

	public void SetBigIcon(DependencyObject obj, DependencyProperty prj, string name)
	{
		this.SetBigIcon(null, obj, prj, name);
	}

	public void SetBigIcon(object parent, DependencyObject obj, DependencyProperty prj, string name)
	{
		this._registerIcons.Add(new RegisterIcon
		{
			Parent = parent,
			Obj = obj,
			Prj = prj,
			Name = name,
			Big = true
		});
		string name2 = name;
		if (this._swapList.ContainsKey(name))
		{
			name2 = this._swapList[name];
		}
		obj.SetValue(prj, this.GetIcon(name2, "32"));
	}

	public void UnregisterIconData(DependencyObject obj)
	{
		List<RegisterIcon> list = new List<RegisterIcon>();
		foreach (RegisterIcon registerIcon in this._registerIcons)
		{
			if (registerIcon.Obj == obj)
			{
				list.Add(registerIcon);
			}
			if (registerIcon.Parent != null && registerIcon.Parent == obj)
			{
				list.Add(registerIcon);
			}
		}
		foreach (RegisterIcon item in list)
		{
			this._registerIcons.Remove(item);
		}
	}

	public void SwapIcons(string keyName, string newIcon)
	{
		if (this._swapList.ContainsKey(keyName))
		{
			this._swapList.Remove(keyName);
		}
		foreach (RegisterIcon registerIcon in this._registerIcons)
		{
			if (registerIcon.Name == keyName)
			{
				if (registerIcon.Big)
				{
					registerIcon.Obj.SetValue(registerIcon.Prj, this.GetIcon(newIcon, "32"));
				}
				else
				{
					registerIcon.Obj.SetValue(registerIcon.Prj, this.GetIcon(newIcon, "16"));
				}
			}
		}
		if (keyName != newIcon)
		{
			this._swapList.Add(keyName, newIcon);
		}
	}
}
