using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base.Geometry2D;

namespace WiCAM.Pn4000.Contracts.CadGeo;

public interface ICadGeoLoader
{
	List<Polygon2D> LoadCadGeo2DContours(string fileName);

	Model LoadCadGeo3D(string fileName, double thickness, double maxTriLength = 0.0, bool toolEdges = false, List<double> zLevels = null);

	Model LoadCadGeo3D(string fileName, double thickness, double maxTriLength, bool toolEdges, double yMin, double yMax, List<double> zLevels = null);
}
