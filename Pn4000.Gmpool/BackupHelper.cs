using System;
using System.Collections.Generic;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Materials;

namespace WiCAM.Pn4000.Gmpool
{
	internal class BackupHelper
	{
		public BackupHelper()
		{
		}

		public bool WriteBackup(IEnumerable<StockMaterialInfo> materials)
		{
			string str = PnPathBuilder.PathInPnDriveWithFormat("u\\sfa\\saving\\gmpool\\GMPOOL_{0}_{1}.txt", new object[] { DateTime.Now.ToString("yyyyMMdd_HHmmss_ff"), Environment.UserName });
			bool flag = (new StockMaterialFileHelper(null)).WriteFile(materials, str);
			if (!flag)
			{
				Logger.Error("GMPOOL can't be written");
			}
			return flag;
		}
	}
}