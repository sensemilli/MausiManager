using System;
using System.Collections.Generic;
using System.Reflection;
using WiCAM.Pn4000.JobManager.Views;
using WiCAM.Pn4000.Materials;

namespace WiCAM.Pn4000.Gmpool.Classes
{
	internal class ItemsComparer
	{
		private readonly List<PropertyInfo> _properties = new List<PropertyInfo>(typeof(StockMaterialInfo).GetProperties());

		public ItemsComparer()
		{
		}

		public List<CompareResult> Compare(StockMaterialInfo first, StockMaterialInfo second)
		{
			List<CompareResult> compareResults = new List<CompareResult>();
			foreach (PropertyInfo _property in this._properties)
			{
				if (_property.Name.IndexOf("Modified", StringComparison.CurrentCultureIgnoreCase) > -1 || _property.Name.IndexOf("MaterialName", StringComparison.CurrentCultureIgnoreCase) > -1)
				{
					continue;
				}
				object value = _property.GetValue(first, null);
				object empty = _property.GetValue(second, null);
				if (value == null)
				{
					value = string.Empty;
				}
				if (empty == null)
				{
					empty = string.Empty;
				}
				if (value.Equals(empty))
				{
					continue;
				}
				compareResults.Add(new CompareResult()
				{
					FirstValue = value,
					SecondValue = empty,
					ModifiedProperty = _property,
					OriginalProperty = _property,
					SelectFirstValue = true
				});
			}
			return compareResults;
		}

        public List<CompareResult> Compare(StockAuftragInfo first, StockAuftragInfo second)
        {
            List<CompareResult> compareResults = new List<CompareResult>();
            foreach (PropertyInfo _property in this._properties)
            {
                if (_property.Name.IndexOf("Modified", StringComparison.CurrentCultureIgnoreCase) > -1 || _property.Name.IndexOf("AuftragsNummer", StringComparison.CurrentCultureIgnoreCase) > -1)
                {
                    continue;
                }
                object value = _property.GetValue(first, null);
                object empty = _property.GetValue(second, null);
                if (value == null)
                {
                    value = string.Empty;
                }
                if (empty == null)
                {
                    empty = string.Empty;
                }
                if (value.Equals(empty))
                {
                    continue;
                }
                compareResults.Add(new CompareResult()
                {
                    FirstValue = value,
                    SecondValue = empty,
                    ModifiedProperty = _property,
                    OriginalProperty = _property,
                    SelectFirstValue = true
                });
            }
            return compareResults;
        }
    }
}