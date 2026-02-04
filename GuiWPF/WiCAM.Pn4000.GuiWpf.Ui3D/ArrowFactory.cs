using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D;

internal class ArrowFactory : IArrowFactory
{
	private readonly IPnPathService _pathService;

	private readonly IConfigProvider _configProvider;

	public ArrowFactory(IPnPathService pathService, IConfigProvider configProvider)
	{
		_pathService = pathService;
		_configProvider = configProvider;
	}

	public Model CreateDoubleArrow()
	{
		Model model = StlLoader.LoadStl(_pathService.PNDRIVE + "\\u\\pn\\pixmap\\img\\redarrowdoublebigflat.stl");
		double screenScaleDoubleArrow = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>().ScreenScaleDoubleArrow;
		model.ScreenSize = 0.001 * screenScaleDoubleArrow;
		model.Name = "doubleArrow";
		return model;
	}

	public Model CreateAxisCrossSimple()
	{
		Model model = new Model();
		Shell shell = new Shell(model);
		Face face = new Face(shell);
		Face face2 = new Face(shell);
		Face face3 = new Face(shell);
		model.Shell = shell;
		shell.Faces.Add(face);
		shell.Faces.Add(face2);
		shell.Faces.Add(face3);
		Vector3d pos = default(Vector3d);
		Vector3d pos2 = new Vector3d(100.0, 0.0, 0.0);
		Vector3d pos3 = new Vector3d(0.0, 100.0, 0.0);
		Vector3d pos4 = new Vector3d(0.0, 0.0, 100.0);
		Vertex vertex = new Vertex(ref pos);
		Vertex vertex2 = new Vertex(ref pos2);
		Vertex vertex3 = new Vertex(ref pos3);
		Vertex vertex4 = new Vertex(ref pos4);
		shell.VertexCache.Add(pos, vertex);
		shell.VertexCache.Add(pos2, vertex2);
		shell.VertexCache.Add(pos3, vertex3);
		shell.VertexCache.Add(pos4, vertex4);
		FaceHalfEdge faceHalfEdge = new FaceHalfEdge(face, EdgeType.Line);
		FaceHalfEdge faceHalfEdge2 = new FaceHalfEdge(face2, EdgeType.Line);
		FaceHalfEdge faceHalfEdge3 = new FaceHalfEdge(face3, EdgeType.Line);
		faceHalfEdge.AddVertex(vertex);
		faceHalfEdge.AddVertex(vertex2);
		faceHalfEdge.Color = new Color(1f, 0f, 0f, 1f);
		faceHalfEdge.Width = 2f;
		faceHalfEdge2.AddVertex(vertex);
		faceHalfEdge2.AddVertex(vertex3);
		faceHalfEdge2.Color = new Color(0f, 1f, 0f, 1f);
		faceHalfEdge2.Width = 2f;
		faceHalfEdge3.AddVertex(vertex);
		faceHalfEdge3.AddVertex(vertex4);
		faceHalfEdge3.Color = new Color(0f, 0f, 1f, 1f);
		faceHalfEdge3.Width = 2f;
		face.BoundaryEdgesCcw.Add(faceHalfEdge);
		face2.BoundaryEdgesCcw.Add(faceHalfEdge2);
		face3.BoundaryEdgesCcw.Add(faceHalfEdge3);
		face.Mesh.Add(new Triangle(face, vertex, vertex2, vertex));
		face2.Mesh.Add(new Triangle(face2, vertex, vertex3, vertex));
		face3.Mesh.Add(new Triangle(face3, vertex, vertex4, vertex));
		face.SurfaceDerivatives.Add(vertex, new SurfaceDerivatives(Vector3d.UnitX));
		face.SurfaceDerivatives.Add(vertex2, new SurfaceDerivatives(Vector3d.UnitX));
		face2.SurfaceDerivatives.Add(vertex, new SurfaceDerivatives(Vector3d.UnitY));
		face2.SurfaceDerivatives.Add(vertex3, new SurfaceDerivatives(Vector3d.UnitY));
		face3.SurfaceDerivatives.Add(vertex, new SurfaceDerivatives(Vector3d.UnitZ));
		face3.SurfaceDerivatives.Add(vertex4, new SurfaceDerivatives(Vector3d.UnitZ));
		return model;
	}
}
