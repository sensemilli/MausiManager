using System;
using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.BendDataBase;

public interface IBendTable
{
	double DefaultKFactor { get; set; }

	string Name { get; set; }

	int GetEntryCount();

	void AddEntry(IBendTableItem entry);

	void ImportEntries(IEnumerable<IBendTableItem> newItems, bool removeOldEntries);

	void RemoveEntry(IBendTableItem entry);

	void Clear();

	IEnumerable<IBendTableItem> GetEntries();

	double GetKFactorByTools(IMaterialArt material, double thickness, double angle, double radius, IToolProfile? punchGroup, IToolProfile? dieGroup, bool ignoreBendTable, Func<double, double, double> getRadiusTolerance, out string algorithm, out double resultRadius, out BendTableReturnValues result, out double? springBack);

	double GetKFactorByRadius(IMaterialArt material, double thickness, double angle, double radius, double bendLength, bool ignoreBendTable, Func<double, double, double> getRadiusTolerance, out string algorithm, out double resultRadius, out BendTableReturnValues result, out double? springBack);

	double GetBendAllowance(IMaterialArt material, double thickness, double angle, double radius, out string algorithm, bool ignoreBendTable, Func<double, double, double> getRadiusTolerance, out BendTableReturnValues result, IToolProfile? upperTool, IToolProfile? lowerTool, double bendLength);

	double GetRadiusByTools(IMaterialArt material, double thickness, double angle, double radius, double radiusTolerance, IPunchGroup punchGroup, IDieGroup dieGroup, double bendLength, bool ignoreBendTable, out BendTableReturnValues result);

	double InterpolateKFactor(IMaterialArt material, double radius, double thickness, double absAngle, Func<double, double, double> getRadiusTolerance, out double resultRadius, out string algorithm, out BendTableReturnValues result, out double? springBack);

	bool IsItemExisting(IBendTableItem entry);
}
