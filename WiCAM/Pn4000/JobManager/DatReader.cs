using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Dat;
using WiCAM.Pn4000.JobManager.Helpers;

namespace WiCAM.Pn4000.JobManager;

public class DatReader<T> where T : class, IDatItem, new()
{
	private readonly DatLineManager _lineManager = new DatLineManager();

	public void UpdatePlateDat(PlateProductionInfo plate)
	{
		Logger.Info("DatReader : UpdatePlateDat : {0}", plate.Plate.Path);
		string text = IOHelper.FileReadAllText(plate.Plate.Path);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(text);
		plate.Plate.PLATE_PRODUCED += plate.AmountToProduce;
		if (plate.Plate.PLATE_PRODUCED > plate.Plate.NUMBER_OF_PLATES)
		{
			plate.Plate.PLATE_PRODUCED = plate.Plate.NUMBER_OF_PLATES;
		}
		plate.Plate.PLATE_PRODUCED_ACT = plate.AmountToProduce;
		plate.Plate.PLATE_STORNO += plate.AmountStorno;
		if (plate.Plate.PLATE_STORNO > plate.Plate.NUMBER_OF_PLATES)
		{
			plate.Plate.PLATE_STORNO = plate.Plate.NUMBER_OF_PLATES;
		}
		if (plate.Plate.PLATE_STORNO == plate.Plate.NUMBER_OF_PLATES)
		{
			plate.Plate.PLATE_STATUS = 1;
		}
		else if (plate.Plate.RestOfProduction() <= 0)
		{
			plate.Plate.PLATE_STATUS = 3;
		}
		_lineManager.ReplaceLine(stringBuilder, "PLATE_STATUS", 0, plate.Plate.PLATE_STATUS);
		_lineManager.ReplaceLine(stringBuilder, "PLATE_PRODUCED", 0, plate.Plate.PLATE_PRODUCED);
		_lineManager.ReplaceLine(stringBuilder, "PLATE_PRODUCED_ACT", 0, plate.Plate.PLATE_PRODUCED_ACT);
		_lineManager.ReplaceLine(stringBuilder, "PLATE_STORNO", 0, plate.Plate.PLATE_STORNO);
		_lineManager.ReplaceLine(stringBuilder, "PLATE_TIME_REAL", 0, plate.Plate.PLATE_TIME_REAL);
		double num = plate.Plate.PLATE_TIME_REAL / plate.Plate.PLATE_TIME_WORK;
		foreach (PlatePartProductionInfo platePart in plate.PlateParts)
		{
			string value = _lineManager.BuildDatLine("PLATE_PART_NUMBER", platePart.PlatePart.PLATE_PART_NUMBER.ToString());
			int num2 = text.IndexOf(value, StringComparison.CurrentCultureIgnoreCase);
			if (num2 > -1)
			{
				if (plate.AmountStorno > 0)
				{
					platePart.AmountToStorno = platePart.PlatePart.PLATE_PART_AMOUNT * plate.AmountStorno;
					platePart.PlatePart.PLATE_PART_AMOUNT_RE += platePart.AmountToStorno;
					_lineManager.ReplaceLine(stringBuilder, "PLATE_PART_AMOUNT_RE", num2, platePart.PlatePart.PLATE_PART_AMOUNT_RE);
				}
				if (platePart.AmountToReject > 0)
				{
					platePart.PlatePart.PLATE_PART_AMOUNT_SC += platePart.AmountToReject;
					_lineManager.ReplaceLine(stringBuilder, "PLATE_PART_AMOUNT_SC", num2, platePart.PlatePart.PLATE_PART_AMOUNT_SC);
				}
				platePart.PlatePart.PLATE_PART_TIME_REAL = platePart.PlatePart.PLATE_PART_TIME * num;
				_lineManager.ReplaceLine(stringBuilder, "PLATE_PART_TIME_REAL", num2, platePart.PlatePart.PLATE_PART_TIME_REAL);
			}
			if (platePart.PlatePart.CollectiveOrders.Count > 0)
			{
				new CollectiveOrderHelper().UpdateContent(platePart.PlatePart.CollectiveOrders, stringBuilder, text, platePart.PlatePart.PLATE_PART_NUMBER);
			}
			UpdatePartDat(platePart);
		}
		_lineManager.ReplaceNotExistingLine(stringBuilder, " PRODUCTION_CHARGE", plate.ProductionData.ChargeNumber);
		_lineManager.ReplaceNotExistingLine(stringBuilder, " PRODUCTION_USER", plate.ProductionData.UserName);
		_lineManager.ReplaceNotExistingLine(stringBuilder, " PRODUCTION_MACHINE", plate.ProductionData.MachineName);
		if (IOHelper.FileCanBeRead(plate.Plate.Path, 30))
		{
			IOHelper.FileWriteAllText(plate.Plate.Path, stringBuilder.ToString());
		}
		plate.Plate.JobReference.CheckStatus();
	}

	private void UpdatePartDat(PlatePartProductionInfo partProduction)
	{
		PartInfo part = partProduction.PlatePart.Part;
		string value = IOHelper.FileReadAllText(part.Path);
		if (!string.IsNullOrEmpty(value))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(value);
			part.PART_PRODUCED += partProduction.AmountToProduce;
			if (part.PART_PRODUCED > part.PART_ACOUNT)
			{
				part.PART_PRODUCED = part.PART_ACOUNT;
			}
			part.PART_STORNO += partProduction.AmountToStorno;
			if (part.PART_STORNO > part.PART_ACOUNT)
			{
				part.PART_STORNO = part.PART_ACOUNT;
			}
			part.PART_REJECT += partProduction.AmountToReject;
			if (part.PART_REJECT > part.PART_ACOUNT)
			{
				part.PART_REJECT = part.PART_ACOUNT;
			}
			_lineManager.ReplaceLine(stringBuilder, "PART_PRODUCED", 0, part.PART_PRODUCED);
			_lineManager.ReplaceLine(stringBuilder, "PART_STORNO", 0, part.PART_STORNO);
			_lineManager.ReplaceLine(stringBuilder, "PART_REJECT", 0, part.PART_REJECT);
			if (IOHelper.FileCanBeRead(part.Path, 30))
			{
				IOHelper.FileWriteAllText(part.Path, stringBuilder.ToString());
			}
		}
	}

	public void UpdatePartDatReject(PlatePartProductionInfo partProduction)
	{
		PartInfo part = partProduction.PlatePart.Part;
		string value = IOHelper.FileReadAllText(part.Path);
		if (!string.IsNullOrEmpty(value))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(value);
			part.PART_REJECT += partProduction.AmountToReject;
			if (part.PART_REJECT > part.PART_ACOUNT)
			{
				part.PART_REJECT = part.PART_ACOUNT;
			}
			_lineManager.ReplaceLine(stringBuilder, "PART_REJECT", 0, part.PART_REJECT);
			if (IOHelper.FileCanBeRead(part.Path, 30))
			{
				IOHelper.FileWriteAllText(part.Path, stringBuilder.ToString());
			}
		}
		value = IOHelper.FileReadAllText(partProduction.PlatePart.Plate.Path);
		if (!string.IsNullOrEmpty(value))
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append(value);
			string value2 = _lineManager.BuildDatLine("PLATE_PART_NUMBER", partProduction.PlatePart.PLATE_PART_NUMBER.ToString());
			int num = value.IndexOf(value2, StringComparison.CurrentCultureIgnoreCase);
			if (num > -1 && partProduction.AmountToReject > 0)
			{
				partProduction.PlatePart.PLATE_PART_AMOUNT_RE += partProduction.AmountToReject;
				_lineManager.ReplaceLine(stringBuilder2, "PLATE_PART_AMOUNT_RE", num, partProduction.PlatePart.PLATE_PART_AMOUNT_RE);
			}
			if (IOHelper.FileCanBeRead(partProduction.PlatePart.Plate.Path, 30))
			{
				IOHelper.FileWriteAllText(partProduction.PlatePart.Plate.Path, stringBuilder2.ToString());
			}
		}
	}

	public T Read(string path, ReadStrategyInfo strategy)
	{
		string text = IOHelper.FileReadAllText(path);
		if (!string.IsNullOrEmpty(text))
		{
			T val = new T();
			{
				foreach (ReadPropertyInfo property in strategy.Properties)
				{
					string value = FindValue(text, property.DatKey);
					if (!string.IsNullOrEmpty(value))
					{
						ModifyValue(val, property.Property, value);
					}
				}
				return val;
			}
		}
		return null;
	}

	public void UpdateFromDat(T result, string path, ReadStrategyInfo strategy)
	{
		string text = IOHelper.FileReadAllText(path);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		foreach (ReadPropertyInfo property in strategy.Properties)
		{
			string value = FindValue(text, property.DatKey);
			if (!string.IsNullOrEmpty(value))
			{
				ModifyValue(result, property.Property, value);
			}
		}
	}

	public PlateInfo ReadPlate(string path, ReadStrategyInfo strategy, ReadStrategyInfo strategyPart)
	{
		string text = IOHelper.FileReadAllText(path);
		if (!string.IsNullOrEmpty(text))
		{
			PlateInfo plateInfo = new PlateInfo();
			foreach (ReadPropertyInfo property in strategy.Properties)
			{
				string value = FindValue(text, property.DatKey);
				if (!string.IsNullOrEmpty(value))
				{
					ModifyValue(plateInfo, property.Property, value);
				}
			}
			CollectiveOrderHelper collectiveOrderHelper = new CollectiveOrderHelper();
			string value2 = "PLATE_PART_NUMBER";
			int num = text.IndexOf(value2, StringComparison.CurrentCultureIgnoreCase);
			int num2 = text.IndexOf(value2, num + 1, StringComparison.CurrentCultureIgnoreCase);
			while (num > -1)
			{
				PlatePartInfo platePartInfo = new PlatePartInfo
				{
					Plate = plateInfo
				};
				foreach (ReadPropertyInfo property2 in strategyPart.Properties)
				{
					string value3 = FindValue(text, property2.DatKey, num);
					if (!string.IsNullOrEmpty(value3))
					{
						ModifyValue(platePartInfo, property2.Property, value3);
					}
				}
				IEnumerable<PlatePartCollectiveOrder> enumerable = collectiveOrderHelper.ResolveByPart(text, num, num2);
				if (!EnumerableHelper.IsNullOrEmpty(enumerable))
				{
					platePartInfo.CollectiveOrders.AddRange(enumerable);
				}
				plateInfo.PlateParts.Add(platePartInfo);
				num = num2;
				num2 = text.IndexOf(value2, num + 1, StringComparison.CurrentCultureIgnoreCase);
			}
			return plateInfo;
		}
		return null;
	}

	public PlateInfo UpdatePlateFromDat(PlateInfo result, string path, ReadStrategyInfo strategy, ReadStrategyInfo strategyPart)
	{
		string text = IOHelper.FileReadAllText(path);
		if (!string.IsNullOrEmpty(text))
		{
			foreach (ReadPropertyInfo property in strategy.Properties)
			{
				string value = FindValue(text, property.DatKey);
				if (!string.IsNullOrEmpty(value))
				{
					ModifyValue(result, property.Property, value);
				}
			}
			int num = 0;
			int num2 = text.IndexOf("PLATE_PART_NUMBER", StringComparison.CurrentCultureIgnoreCase);
			while (num2 > -1)
			{
				PlatePartInfo item = result.PlateParts[num];
				foreach (ReadPropertyInfo property2 in strategyPart.Properties)
				{
					string value2 = FindValue(text, property2.DatKey, num2);
					if (!string.IsNullOrEmpty(value2))
					{
						ModifyValue(item, property2.Property, value2);
					}
				}
				num2 = text.IndexOf("PLATE_PART_NUMBER", ++num2, StringComparison.CurrentCultureIgnoreCase);
				num++;
			}
			return result;
		}
		return null;
	}

	private void ModifyValue(object item, PropertyInfo pi, string value)
	{
		if (ValueTypeHelper.IsNumericType(pi.PropertyType))
		{
			if (ValueTypeHelper.IsFloatingPointType(pi.PropertyType))
			{
				double num = ToDouble(value);
				pi.SetValue(item, num);
			}
			else
			{
				int num2 = ToInt(value);
				pi.SetValue(item, num2);
			}
		}
		else if (pi.PropertyType == typeof(DateTime))
		{
			DateTime dateTime = ToDate(value);
			pi.SetValue(item, dateTime);
		}
		else if (pi.PropertyType == typeof(TimeSpan))
		{
			TimeSpan timeSpan = ToTime(value);
			pi.SetValue(item, timeSpan);
		}
		else
		{
			if (value == null)
			{
				value = string.Empty;
			}
			pi.SetValue(item, value);
		}
	}

	private string FindValue(string content, string key, int searchBeginPosition = 0)
	{
		int num = content.IndexOf(key, searchBeginPosition, StringComparison.CurrentCultureIgnoreCase);
		if (num > -1)
		{
			int num2 = content.IndexOf("=", num, StringComparison.CurrentCultureIgnoreCase);
			if (num2 > -1)
			{
				int num3 = content.IndexOf("\n", num2, StringComparison.CurrentCultureIgnoreCase);
				if (num3 > -1)
				{
					return content.Substring(num2 + 1, num3 - num2 - 1).Trim();
				}
			}
		}
		return string.Empty;
	}

	private DateTime ToDate(string input)
	{
		DateTime result = DateTime.MinValue;
		if (!DateTime.TryParseExact(input, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
		{
			result = DateTime.MinValue;
			if (!DateTime.TryParseExact(input, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
			{
				result = DateTime.MinValue;
			}
		}
		return result;
	}

	private TimeSpan ToTime(string input)
	{
		TimeSpan result = TimeSpan.Zero;
		TimeSpan.TryParseExact(input, "h\\:mm\\:ss", CultureInfo.InvariantCulture, TimeSpanStyles.None, out result);
		return result;
	}

	private double ToDouble(string input)
	{
		double result = 0.0;
		if (!string.IsNullOrEmpty(input) && !double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
		{
			result = 0.0;
		}
		return result;
	}

	private int ToInt(string input)
	{
		int result = 0;
		if (!string.IsNullOrEmpty(input) && !int.TryParse(input, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result))
		{
			result = 0;
		}
		return result;
	}
}
