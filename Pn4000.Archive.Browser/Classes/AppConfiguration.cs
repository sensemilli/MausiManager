using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Archive.Browser.Classes
{
	[Serializable]
	public sealed class AppConfiguration
	{
		private readonly static string __filterCfgFileName;

		private readonly static string __filterNestingCfgFileName;

		private readonly static string __filterCadCfgFileName;

		private readonly static string __filterVarCfgFileName;

		private readonly static string __filter3dCfgFileName;

		internal static int Erg0Format;

		private WpfCaptionDictionary _dataListCaptions;

		private List<CppConfigurationLineInfo> _listConfiguration;

		private IniFileInfo _iniInfo;

		private static AppConfiguration __instance;

		public ArchiveFileType ArchiveType
		{
			get;
			set;
		}

		public WpfCaptionDictionary DataListCaptions
		{
			get
			{
				return this._dataListCaptions;
			}
			set
			{
				this._dataListCaptions = value;
			}
		}

		public static AppConfiguration Instance
		{
			get
			{
				if (AppConfiguration.__instance == null)
				{
					Logger.Verbose("Initialize AppConfiguration");
					AppConfiguration.__instance = new AppConfiguration();
				}
				return AppConfiguration.__instance;
			}
		}

		public List<CppConfigurationLineInfo> ListConfiguration
		{
			get
			{
				return this._listConfiguration;
			}
			set
			{
				this._listConfiguration = value;
			}
		}

		static AppConfiguration()
		{
			AppConfiguration.__filterCfgFileName = "PnArBrDb.Filter.xml";
			AppConfiguration.__filterNestingCfgFileName = "PnArBrDb.FilterNesting.xml";
			AppConfiguration.__filterCadCfgFileName = "PnArBrDb.CadPart.xml";
			AppConfiguration.__filterVarCfgFileName = "PnArBrDb.VarPart.xml";
			AppConfiguration.__filter3dCfgFileName = "PnArBrDb.3DPart.xml";
			AppConfiguration.Erg0Format = 2;
		}

		internal AppConfiguration()
		{
			this._dataListCaptions = new WpfCaptionDictionary();
			this._listConfiguration = new List<CppConfigurationLineInfo>();
		}

		internal bool Initialize()
		{
			bool flag = true;
			List<string> strs = new List<string>()
			{
				"N2D_",
				"M2D_",
				"MPL_",
				"M3D_",
				"VAR_"
			};
			this._iniInfo = (new IniManager()).Read("PNARCF.ini", "PNARCF", strs);
			if (EnumerableHelper.IsNullOrEmpty(this._iniInfo.Tables) || this._iniInfo.Tables.Count < 5)
			{
				string str = PnPathBuilder.PathInPnHome(new object[] { "PNARCF.INI" });
				string str1 = string.Concat(str, ".err");
				IOHelper.FileDelete(str1);
				IOHelper.FileMove(str, str1);
				this.Initialize();
			}
			else
			{
				this.ArchiveType = AppArguments.Instance.ArchiveType();
				if (this.ArchiveType == ArchiveFileType.N2D)
				{
					this._listConfiguration = this._iniInfo.Tables[strs[0]];
					PnPathBuilder.PathInAppData(new object[] { "WiCAM.ArchivBrowser", AppConfiguration.__filterCfgFileName });
				}
				else if (this.ArchiveType == ArchiveFileType.M2D)
				{
					this._listConfiguration = this._iniInfo.Tables[strs[1]];
					PnPathBuilder.PathInAppData(new object[] { "WiCAM.ArchivBrowser", AppConfiguration.__filterCadCfgFileName });
				}
				else if (this.ArchiveType == ArchiveFileType.MPL)
				{
					this._listConfiguration = this._iniInfo.Tables[strs[2]];
					PnPathBuilder.PathInAppData(new object[] { "WiCAM.ArchivBrowser", AppConfiguration.__filterNestingCfgFileName });
				}
				else if (this.ArchiveType == ArchiveFileType.N3D)
				{
					this._listConfiguration = this._iniInfo.Tables[strs[3]];
					PnPathBuilder.PathInAppData(new object[] { "WiCAM.ArchivBrowser", AppConfiguration.__filter3dCfgFileName });
				}
				else if (this.ArchiveType == ArchiveFileType.VAR)
				{
					this._listConfiguration = this._iniInfo.Tables[strs[4]];
					PnPathBuilder.PathInAppData(new object[] { "WiCAM.ArchivBrowser", AppConfiguration.__filterVarCfgFileName });
				}
			}
			return flag;
		}

		internal void SaveSettings()
		{
			(new IniManager()).Write(this._iniInfo);
		}
	}
}