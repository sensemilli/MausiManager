using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Gmpool.Classes;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.Gmpool
{
	internal class ApplicationConfigurationInfo : ApplicationArgumentsBase
	{
		private static ApplicationConfigurationInfo _instance;

		private readonly static string __material;

		private readonly static string __thickness;

		private readonly static string __machine;

		private readonly static string __readOnly;

		private readonly static string __topMost;

		private readonly static string __fromPnControl;

		private readonly static string __plateName;

		private readonly static string __userName;

		private readonly static string __ncNumber;

		private readonly static string __iniPath;

		private readonly static string __buttons;

		private readonly static string __restMachine;

		private readonly static string __trace;

		private readonly static string __backup;

		internal ButtonsStateManager ButtonsState
		{
			get;
			private set;
		}

		public List<FilterConfigurationInfo> FilterConfiguration
		{
			get;
			set;
		}

		public string IniPath
		{
			get;
			private set;
		}

		public static ApplicationConfigurationInfo Instance
		{
			get
			{
				if (ApplicationConfigurationInfo._instance == null)
				{
					Logger.Verbose("Initialize ApplicationConfigurationInfo");
					ApplicationConfigurationInfo._instance = new ApplicationConfigurationInfo();
				}
				return ApplicationConfigurationInfo._instance;
			}
		}

		public bool IsFromPnControl
		{
			get;
			private set;
		}

		public string NcNumber
		{
			get;
			private set;
		}

		public string PlateName
		{
			get;
			private set;
		}

		public ArchiveInfo PlatesArchiv
		{
			get;
			private set;
		}

		public string RestMachineNumber
		{
			get;
			private set;
		}

		public bool TracingEnabled
		{
			get;
			private set;
		}

		public string UserName
		{
			get;
			private set;
		}

		public bool WriteBackup
		{
			get;
			private set;
		}

		static ApplicationConfigurationInfo()
		{
			ApplicationConfigurationInfo.__material = "material";
			ApplicationConfigurationInfo.__thickness = "thickness";
			ApplicationConfigurationInfo.__machine = "machine";
			ApplicationConfigurationInfo.__readOnly = "write";
			ApplicationConfigurationInfo.__topMost = "topMost";
			ApplicationConfigurationInfo.__fromPnControl = "pncontrol";
			ApplicationConfigurationInfo.__plateName = "platename";
			ApplicationConfigurationInfo.__userName = "user";
			ApplicationConfigurationInfo.__ncNumber = "ncnumber";
			ApplicationConfigurationInfo.__iniPath = "inipath";
			ApplicationConfigurationInfo.__buttons = "buttons";
			ApplicationConfigurationInfo.__restMachine = "restmachine";
			ApplicationConfigurationInfo.__trace = "debug";
			ApplicationConfigurationInfo.__backup = "/backup";
		}

		private ApplicationConfigurationInfo()
		{
			base.Initialize(Environment.CommandLine);
			this.TracingEnabled = Environment.CommandLine.IndexOf(ApplicationConfigurationInfo.__trace, StringComparison.CurrentCultureIgnoreCase) > -1;
			this.WriteBackup = Environment.CommandLine.IndexOf(ApplicationConfigurationInfo.__backup, StringComparison.CurrentCultureIgnoreCase) > -1;
			this.IsFromPnControl = Environment.CommandLine.IndexOf(ApplicationConfigurationInfo.__fromPnControl, StringComparison.CurrentCultureIgnoreCase) > -1;
			this.PlateName = base.ArgumentByName(ApplicationConfigurationInfo.__plateName);
			this.UserName = base.ArgumentByName(ApplicationConfigurationInfo.__userName);
			this.NcNumber = base.ArgumentByName(ApplicationConfigurationInfo.__ncNumber);
			this.IniPath = base.ArgumentByName(ApplicationConfigurationInfo.__iniPath);
			this.RestMachineNumber = base.ArgumentByName(ApplicationConfigurationInfo.__restMachine);
			this.ButtonsState = new ButtonsStateManager(base.ArgumentByName(ApplicationConfigurationInfo.__buttons), this.IsFromPnControl);
		}

		public bool IsReadOnly()
		{
			string s0 = base.ArgumentByNameNotNullable(ApplicationConfigurationInfo.__readOnly);
			s0 = "1";  //änderung
			if (string.Empty == s0)
			{
				s0 = WiCAM.Pn4000.Common.CS.S0;
			}
			return s0 == WiCAM.Pn4000.Common.CS.S0;
		}

		public bool IsTopMost()
		{
			string s0 = base.ArgumentByNameNotNullable(ApplicationConfigurationInfo.__topMost);
			if (string.Empty == s0)
			{
				s0 = WiCAM.Pn4000.Common.CS.S0;
			}
			return s0 == WiCAM.Pn4000.Common.CS.S1;
		}

		public int MachineNumber()
		{
			return base.IntArgumentByName(ApplicationConfigurationInfo.__machine);
		}

		public int MaterialNumber()
		{
			return base.IntArgumentByName(ApplicationConfigurationInfo.__material);
		}

		private void ReadArchiveStructure()
		{
			if (!EnumerableHelper.IsNullOrEmpty(ArchiveStructureHelper.Instance.ReadAllArchives()))
			{
				this.PlatesArchiv = ArchiveStructureHelper.Instance.FindArchiveOrSubArchive(90);
				if (this.PlatesArchiv != null)
				{
					ArchiveStructureHelper.Instance.ReadArchivePaths(this.PlatesArchiv);
				}
			}
		}

		public bool ReadSettings()
		{
			bool flag = true;
			if (string.IsNullOrEmpty(this.IniPath))
			{
				flag = IniFileHelper.Instance.ReadIniFile();
				if (flag)
				{
					this.UpdateIniData();
				}
			}
			else
			{
				flag = IniFileHelper.Instance.ReadIniFile(this.IniPath);
				if (flag)
				{
					this.UpdateIniData();
				}
			}
			return flag;
		}

		public double Thickness()
		{
			return base.DoubleArgumentByName(ApplicationConfigurationInfo.__thickness);
		}

		private void UpdateIniData()
		{
			IniFileHelper.Instance.WindowSize.IsTopMost = this.IsTopMost();
			StringResourceHelper.Instance.UpdateText(IniFileHelper.Instance.Texts, IniFileHelper.Instance.MaterialsColumns, IniFileHelper.Instance.MaterialArtsColumns);
			this.ReadArchiveStructure();
		}
	}
}