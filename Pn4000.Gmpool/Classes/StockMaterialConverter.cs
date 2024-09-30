using System;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Common.Converters;
using WiCAM.Pn4000.Materials;

namespace WiCAM.Pn4000.Gmpool.Classes
{
	internal class StockMaterialConverter : MetricConverter<StockMaterialInfo, StockMaterialViewModel>
	{
		public StockMaterialConverter() : base(SystemConfiguration.UseInch)
		{
		}
	}
    internal class StockAuftragConverter : MetricConverter<WiCAM.Pn4000.JobManager.Views.StockAuftragInfo, StockAuftragViewModel>
    {
        public StockAuftragConverter() : base(SystemConfiguration.UseInch)
        {
        }
    }
}