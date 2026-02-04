using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.PnGeometry;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolGeometryVerifier
{
	bool? Verify(GeometryVerifierDieParameters profile, string profilePath, out double? maxError, out double? predictedVWidth, out double? predictedVAngleRad, out double? predictedCornerRadius, out List<GeoSegment2D> svgSegments);
}
