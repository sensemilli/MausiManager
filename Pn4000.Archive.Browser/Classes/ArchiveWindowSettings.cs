using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using WiCAM.Pn4000.Archive.Browser;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager;

namespace WiCAM.Pn4000.Archive.Browser.Classes
{
	[Serializable]
	public class ArchiveWindowSettings : WindowSettings
	{
		public const int MinSplitterWidth = 25;

		public const int StdSplitterWidth = 270;

		public int SplitterWidth
		{
			get;
			set;
		}

		public ArchiveWindowSettings()
		{
		}

		public ArchiveWindowSettings(string applicationName, string windowName)
		{
			base.WindowName = windowName;
			this.Path = PnPathBuilder.PathInAppData(new object[] { applicationName, string.Concat(windowName, WiCAM.Pn4000.Common.CS.ExtXml) });
			ArchiveWindowSettings archiveWindowSetting = SerializeHelper.DeserialiseFromXml<ArchiveWindowSettings>(this.Path);
			if (archiveWindowSetting != null)
			{
				base.Top = archiveWindowSetting.Top;
				base.Left = archiveWindowSetting.Left;
				base.Width = archiveWindowSetting.Width;
				base.Height = archiveWindowSetting.Height;
				base.IsTopMost = archiveWindowSetting.IsTopMost;
				this.SplitterWidth = archiveWindowSetting.SplitterWidth;
			}
		}

		public void ApplyWindowSettings(MainWindow wnd)
		{
			base.Apply(wnd);
			if (this.SplitterWidth != 0 && this.SplitterWidth <= 25)
			{
				wnd.Expander.IsExpanded = false;
				wnd.OpenedArchivControlWidth = new GridLength(270, GridUnitType.Pixel);
				return;
			}
			if (this.SplitterWidth == 0)
			{
				this.SplitterWidth = 270;
			}
			wnd.Expander.IsExpanded = true;
			wnd.OpenedArchivControlWidth = new GridLength((double)this.SplitterWidth, GridUnitType.Pixel);
			wnd.ColumnExpanded.Width = wnd.OpenedArchivControlWidth;
		}

		public void ReadWindowSettings(MainWindow wnd)
		{
			base.ReadSettings(wnd);
			if (!wnd.Expander.IsExpanded)
			{
				this.SplitterWidth = 25;
				return;
			}
			this.SplitterWidth = (int)wnd.ColumnExpanded.Width.Value;
		}

		public bool SaveWindowSettings()
		{
			return SerializeHelper.SerializeToXml<ArchiveWindowSettings>(this, this.Path, false);
		}
	}
}