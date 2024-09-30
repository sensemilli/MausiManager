using System;
using System.Collections.Generic;
using System.Windows;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Gmpool
{
	internal class StringResourceHelper : ResourceHelperBase
	{
		private static StringResourceHelper _instance;

		public static StringResourceHelper Instance
		{
			get
			{
				if (StringResourceHelper._instance == null)
				{
					Logger.Verbose("Initialize StringResourceHelper");
					StringResourceHelper._instance = new StringResourceHelper();
				}
				return StringResourceHelper._instance;
			}
		}

		private StringResourceHelper() : base("..\\Resources\\StringResources")
		{
			base.FindDictionary(SystemConfiguration.PnLanguage);
		}

		private void AddToResourceDictionary(List<CppConfigurationLineInfo> config, ResourceDictionary dictionary)
		{
			try
			{
				foreach (CppConfigurationLineInfo cppConfigurationLineInfo in config)
				{
					if (!dictionary.Contains(cppConfigurationLineInfo.Key))
					{
						dictionary.Add(cppConfigurationLineInfo.Key, cppConfigurationLineInfo.Caption);
					}
					else
					{
						base.ReplaceText(cppConfigurationLineInfo.Key, cppConfigurationLineInfo.Caption);
					}
				}
			}
			catch (Exception exception)
			{
				Logger.Exception(exception);
			}
		}

		public void UpdateText(Dictionary<string, string> source, List<CppConfigurationLineInfo> materials, List<CppConfigurationLineInfo> materialArts)
		{
			this.AddToResourceDictionary(materials, base.Resources);
			this.AddToResourceDictionary(materialArts, base.Resources);
			this.UpdateText(base.Resources, "formatDouble", SystemConfiguration.WpfDoubleFormat);
			this.UpdateText(base.Resources, "MatNormal", source["1"]);
			this.UpdateText(base.Resources, "MatCutOut", source["2"]);
			this.UpdateText(base.Resources, "MatRest", source["3"]);
			this.UpdateText(base.Resources, "MatCoil", source["4"]);
			this.UpdateText(base.Resources, "ButtonAdd", source["5"]);
			this.UpdateText(base.Resources, "ButtonEdit", source["6"]);
			this.UpdateText(base.Resources, "ButtonDelete", source["7"]);
			this.UpdateText(base.Resources, "ButtonOK", source["9"]);
			this.UpdateText(base.Resources, "ButtonCancel", source["8"]);
			this.UpdateText(base.Resources, "ButtonCopy", source["11"]);
		}

		private void UpdateText(ResourceDictionary dictionary, string key, string value)
		{
			if (dictionary.Contains(key))
			{
				base.ReplaceText(key, value);
				return;
			}
			dictionary.Add(key, value);
		}
	}
}