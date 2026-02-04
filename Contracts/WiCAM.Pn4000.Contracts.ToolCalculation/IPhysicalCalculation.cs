using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.ToolCalculation;

public interface IPhysicalCalculation
{
	double CalculatePressForceByTrumpf(double bendLineLength, double thickness, double lowerToolOpeningWidth, double upperToolRadius, double tensileStrength, double minForce, double maxForce);

	double CalculatePressForceByHarsle(double bendLineLength, double thickness, double lowerToolOpeningWidth, double tensileStrength);

	double? CalculateSpringbackByTable(List<(double Ratio, double Springback)> table, double radius, double thickness, double angleEnd);

	double CalculateSpringback(double radius, double thickness, double angleEnd, double k, double yieldStrength, double EModul);
}
