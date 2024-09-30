using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Gmpool
{
	internal class IniFileHelper
	{
		private readonly static string __iniName;

		private readonly static string __newIniName;

		private readonly static string __keyMaterial;

		private readonly static string __keyMatpool;

		private IniFileInfo _iniFile;

		private static IniFileHelper _instance;

		public static IniFileHelper Instance
		{
			get
			{
				if (IniFileHelper._instance == null)
				{
					Logger.Verbose("Initialize IniFileHelper");
					IniFileHelper._instance = new IniFileHelper();
				}
				return IniFileHelper._instance;
			}
		}

		public List<CppConfigurationLineInfo> MaterialArtsColumns
		{
			get;
			set;
		}

		public List<CppConfigurationLineInfo> MaterialsColumns
		{
			get;
			set;
		}

		public Dictionary<string, string> Texts
		{
			get
			{
				return this._iniFile.Texts;
			}
		}

		public WindowSettings WindowSize
		{
			get
			{
				return this._iniFile.SettingsOfWindow;
			}
		}

		static IniFileHelper()
		{
			IniFileHelper.__iniName = "PNGMCF";
			IniFileHelper.__newIniName = "GMPOOL.INI";
			IniFileHelper.__keyMaterial = "MATERIAL_";
			IniFileHelper.__keyMatpool = "MATPOOL_";
		}

		private IniFileHelper()
		{
		}

		private CppConfigurationLineInfo CreateCpp(string key, string alignment)
		{
			return new CppConfigurationLineInfo()
			{
				Key = key,
				Caption = StringResourceHelper.Instance.FindString(key),
				Visibility = 2,
				Alignment = alignment,
				Width = 70
			};
		}

		public string IniPath()
		{
			return this._iniFile.IniFilePath;
		}

		public bool ReadIniFile(string path)
		{
			bool flag = true;
			IniManager iniManager = new IniManager();
			List<string> strs = new List<string>()
			{
				IniFileHelper.__keyMaterial,
				IniFileHelper.__keyMatpool
			};
			this._iniFile = iniManager.ReadByPath(path, IniFileHelper.__iniName, strs);
			if (!EnumerableHelper.IsNullOrEmpty(this._iniFile.Tables) && this._iniFile.Tables.Count >= 2)
			{
				this.MaterialsColumns = this._iniFile.Tables[IniFileHelper.__keyMatpool];
				this.MaterialArtsColumns = this._iniFile.Tables[IniFileHelper.__keyMaterial];
				if (this._iniFile.FileVersion == "2.0" || this._iniFile.FileVersion == "3.0" || this._iniFile.FileVersion == "4.0")
				{
					this.UpdateToVersion41();
				}
				flag = true;
			}
			else if (MessageHelper.Question(StringResourceHelper.Instance.FindString("MessageIniIsCorrupt")) != MessageBoxResult.Yes)
			{
				flag = false;
			}
			else
			{
				string str = string.Concat(path, ".err");
				IOHelper.FileDelete(str);
				IOHelper.FileMove(path, str);
				this.ReadIniFile();
			}
			return flag;
		}

		public bool ReadIniFile()
		{
			bool flag = true;
			IniManager iniManager = new IniManager();
			List<string> strs = new List<string>()
			{
				IniFileHelper.__keyMaterial,
				IniFileHelper.__keyMatpool
			};
			this._iniFile = iniManager.Read(IniFileHelper.__newIniName, IniFileHelper.__iniName, strs);
			if (!EnumerableHelper.IsNullOrEmpty(this._iniFile.Tables) && this._iniFile.Tables.Count >= 2)
			{
				this.MaterialsColumns = this._iniFile.Tables[IniFileHelper.__keyMatpool];
				this.MaterialArtsColumns = this._iniFile.Tables[IniFileHelper.__keyMaterial];
				if (this._iniFile.FileVersion == "2.0" || this._iniFile.FileVersion == "3.0" || this._iniFile.FileVersion == "4.0")
				{
					this.UpdateToVersion41();
				}
				flag = true;
			}
			else if (MessageHelper.Question(StringResourceHelper.Instance.FindString("MessageIniIsCorrupt")) != MessageBoxResult.Yes)
			{
				flag = false;
			}
			else
			{
				string str = PnPathBuilder.PathInPnHome(new object[] { "GMPOOL.INI" });
				string str1 = string.Concat(str, ".err");
				IOHelper.FileDelete(str1);
				IOHelper.FileMove(str, str1);
				this.ReadIniFile();
			}
			return flag;
		}

		private void UpdateToVersion41()
		{
			this.MaterialsColumns.Add(this.CreateCpp("PLATE_AREA", "R"));
			this.MaterialsColumns.Add(this.CreateCpp("PLATE_WEIGHT", "R"));
			this._iniFile.FileVersion = "4.1";
		}

		public bool WriteIniFile()
		{
			return (new IniManager()).Write(this._iniFile);
		}
	}
}