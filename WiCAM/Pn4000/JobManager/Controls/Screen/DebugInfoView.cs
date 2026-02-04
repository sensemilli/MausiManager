using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.Win32;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Serialization;
using WiCAM.Pn4000.BendModel.Serialization.SerializationStructure;
using WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.ScreenD3D.Controls;

public partial class DebugInfoView : System.Windows.Controls.UserControl, IDisposable, INotifyPropertyChanged, IComponentConnector
{
	public enum SearchTypes
	{
		Triangle,
		Face,
		FaceGroup,
		FaceHalfEdge,
		Macro
	}

	private Model _selectedModel;

	private Model _axisCross;

	private string _selectedType;

	private bool _edgeModifierActive;

	private bool _autoApplyColor;

	private bool _pickColor;

	private System.Windows.Media.Color _color = System.Windows.Media.Color.FromRgb(0, 0, 0);

	private SearchTypes _clickType = SearchTypes.Face;

	private SearchTypes _searchType = SearchTypes.Face;

	private string _selectedObjectBb;

	private bool _showLocalAxisCross;

	private double _translationX;

	private double _translationY;

	private double _translationZ;

	private Brush _backgroundSearch = Brushes.White;

	public ScreenD3D11 ScreenD3D { get; set; }

	public Screen3D Screen3D { get; set; }

	public PropertyGrid PropertyGrid { get; private set; }

	public new bool IsVisible { get; set; }

	public string SelectedType
	{
		get
		{
			return _selectedType;
		}
		set
		{
			if (_selectedType != value)
			{
				_selectedType = value;
				OnPropertyChanged("SelectedType");
			}
		}
	}

	public bool EdgeModifierActive
	{
		get
		{
			return _edgeModifierActive;
		}
		set
		{
			if (_edgeModifierActive != value)
			{
				_edgeModifierActive = value;
				OnPropertyChanged("EdgeModifierActive");
			}
		}
	}

	public bool AutoApplyColor
	{
		get
		{
			return _autoApplyColor;
		}
		set
		{
			if (_autoApplyColor != value)
			{
				_autoApplyColor = value;
				OnPropertyChanged("AutoApplyColor");
			}
		}
	}

	public bool PickColor
	{
		get
		{
			return _pickColor;
		}
		set
		{
			if (_pickColor != value)
			{
				_pickColor = value;
				OnPropertyChanged("PickColor");
			}
		}
	}

	public System.Windows.Media.Color Color
	{
		get
		{
			return _color;
		}
		set
		{
			if (_color != value)
			{
				_color = value;
				OnPropertyChanged("Color");
			}
		}
	}

	public SearchTypes ClickType
	{
		get
		{
			return _clickType;
		}
		set
		{
			if (_clickType != value)
			{
				_clickType = value;
				OnPropertyChanged("ClickType");
			}
		}
	}

	public SearchTypes SearchType
	{
		get
		{
			return _searchType;
		}
		set
		{
			if (_searchType != value)
			{
				_searchType = value;
				OnPropertyChanged("SearchType");
			}
		}
	}

	public IEnumerable<SearchTypes> AllSearchTypes => System.Enum.GetValues(typeof(SearchTypes)).Cast<SearchTypes>();

	public string SelectedObjectBb
	{
		get
		{
			return _selectedObjectBb;
		}
		set
		{
			if (_selectedObjectBb != value)
			{
				_selectedObjectBb = value;
				OnPropertyChanged("SelectedObjectBb");
			}
		}
	}

	public bool MoveScreenWhenSearching { get; set; } = true;

	public bool HighlightWhenSearching { get; set; }

	public bool ShowLocalAxisCross
	{
		get
		{
			return _showLocalAxisCross;
		}
		set
		{
			_showLocalAxisCross = value;
			if (_selectedModel != null && _axisCross != null)
			{
				if (_showLocalAxisCross)
				{
					if (_axisCross.Parent != null)
					{
						_axisCross.Parent.SubModels.Remove(_axisCross);
						ScreenD3D.RemoveModel(_axisCross);
					}
					_axisCross.Parent = _selectedModel;
					_selectedModel.SubModels.Add(_axisCross);
					ScreenD3D.AddModel(_axisCross, _selectedModel);
				}
				else
				{
					ScreenD3D.RemoveModel(_axisCross);
					_axisCross.Parent?.SubModels.Remove(_axisCross);
					_axisCross.Parent = null;
				}
			}
			OnPropertyChanged("ShowLocalAxisCross");
		}
	}

	public double TranslationX
	{
		get
		{
			return _translationX;
		}
		set
		{
			_translationX = value;
			OnPropertyChanged("TranslationX");
		}
	}

	public double TranslationY
	{
		get
		{
			return _translationY;
		}
		set
		{
			_translationY = value;
			OnPropertyChanged("TranslationY");
		}
	}

	public double TranslationZ
	{
		get
		{
			return _translationZ;
		}
		set
		{
			_translationZ = value;
			OnPropertyChanged("TranslationZ");
		}
	}

	public double RotAngle { get; set; }

	public Brush BackgroundSearch
	{
		get
		{
			return _backgroundSearch;
		}
		set
		{
			if (_backgroundSearch != value)
			{
				_backgroundSearch = value;
				OnPropertyChanged("BackgroundSearch");
			}
		}
	}

	public bool DoNotSelect { get; set; }

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	public DebugInfoView()
	{


	}
	public DebugInfoView(Screen3D screen3D, ScreenD3D11 screenD3D)
	{
		Screen3D = screen3D;
		ScreenD3D = screenD3D;
		InitializeComponent();
		PropertyGrid = new PropertyGrid
		{
			Dock = DockStyle.Fill,
			PropertySort = PropertySort.NoSort
		};
		_axisCross = new Model();
		Shell shell = new Shell(_axisCross);
		Face face = new Face(shell);
		Face face2 = new Face(shell);
		Face face3 = new Face(shell);
		_axisCross.Shells.Add(shell);
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
		faceHalfEdge.Color = new WiCAM.Pn4000.BendModel.Base.Color(1f, 0f, 0f, 1f);
		faceHalfEdge.Width = 2f;
		faceHalfEdge2.AddVertex(vertex);
		faceHalfEdge2.AddVertex(vertex3);
		faceHalfEdge2.Color = new WiCAM.Pn4000.BendModel.Base.Color(0f, 1f, 0f, 1f);
		faceHalfEdge2.Width = 2f;
		faceHalfEdge3.AddVertex(vertex);
		faceHalfEdge3.AddVertex(vertex4);
		faceHalfEdge3.Color = new WiCAM.Pn4000.BendModel.Base.Color(0f, 0f, 1f, 1f);
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
	}

	public void Dispose()
	{
		WindowsFormsHost?.Dispose();
		PropertyGrid = null;
	}

	private void TxtSearch_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			if (SearchObject(txtSearch.Text))
			{
				BackgroundSearch = Brushes.Aquamarine;
			}
			else
			{
				BackgroundSearch = Brushes.MistyRose;
			}
		}
	}

	private bool SearchObject(string searchValue)
	{
		if (_selectedModel != null && int.TryParse(searchValue, out var result))
		{
			switch (SearchType)
			{
			case SearchTypes.Face:
				var (face, model2) = _selectedModel.GetFaceModelById(result);
				if (face != null)
				{
					if (HighlightWhenSearching)
					{
						Model rootParent = _selectedModel.GetRootParent();
						RememberAndUnHighlightAllFaces(rootParent);
						WiCAM.Pn4000.BendModel.Base.Color value = new WiCAM.Pn4000.BendModel.Base.Color(0f, 1f, 0f, 0.9f);
						face.HighlightColor = value;
						ScreenD3D.UpdateAllModelAppearance(render: true);
					}
					ZoomToVertices(face.BoundaryEdgesCcw.SelectMany((FaceHalfEdge x) => x.Vertices).ToList(), face.Shell.GetWorldMatrix(model2));
					SelectObject(face, model2);
					return true;
				}
				break;
			case SearchTypes.FaceGroup:
				var (faceGroup, model3) = _selectedModel.GetFaceGroupModelById(result);
				if (faceGroup == null)
				{
					break;
				}
				if (HighlightWhenSearching)
				{
					Model rootParent2 = _selectedModel.GetRootParent();
					RememberAndUnHighlightAllFaces(rootParent2);
					WiCAM.Pn4000.BendModel.Base.Color value2 = new WiCAM.Pn4000.BendModel.Base.Color(0f, 1f, 0f, 0.9f);
					WiCAM.Pn4000.BendModel.Base.Color value3 = new WiCAM.Pn4000.BendModel.Base.Color(0f, 0f, 1f, 0.9f);
					WiCAM.Pn4000.BendModel.Base.Color value4 = new WiCAM.Pn4000.BendModel.Base.Color(0f, 0.5f, 0.5f, 0.9f);
					foreach (Face item in faceGroup.Side0)
					{
						item.HighlightColor = value2;
					}
					foreach (Face item2 in faceGroup.Side1)
					{
						item2.HighlightColor = value3;
					}
					foreach (Face connectingFace in faceGroup.ConnectingFaces)
					{
						connectingFace.HighlightColor = value4;
					}
					ScreenD3D.UpdateAllModelAppearance(render: true);
				}
				ZoomToVertices(faceGroup.GetAllFaces().SelectMany((Face f) => f.BoundaryEdgesCcw.SelectMany((FaceHalfEdge x) => x.Vertices)).ToList(), faceGroup.GetAllFaces().First().Shell.GetWorldMatrix(model3));
				SelectObject(faceGroup, model3);
				return true;
			case SearchTypes.FaceHalfEdge:
				var (faceHalfEdge, model4) = _selectedModel.GetFaceHalfEdgeModelById(result);
				if (faceHalfEdge != null)
				{
					if (HighlightWhenSearching)
					{
						Model rootParent3 = _selectedModel.GetRootParent();
						RememberAndUnHighlightAllEdges(rootParent3);
						WiCAM.Pn4000.BendModel.Base.Color value5 = new WiCAM.Pn4000.BendModel.Base.Color(0f, 1f, 0f, 0.9f);
						faceHalfEdge.HighlightColor = value5;
					}
					ZoomToVertices(faceHalfEdge.Vertices, faceHalfEdge.Face.Shell.GetWorldMatrix(model4));
					SelectObject(faceHalfEdge, model4);
					return true;
				}
				break;
			case SearchTypes.Triangle:
				var (triangle, model5) = _selectedModel.GetTriangleModelById(result);
				if (triangle != null)
				{
					ZoomToVertices(new List<Vertex> { triangle.V0, triangle.V1, triangle.V2 }, triangle.Face.Shell.GetWorldMatrix(model5));
					SelectObject(triangle, model5);
					return true;
				}
				break;
			case SearchTypes.Macro:
				var (macro, model) = _selectedModel.GetMacroModelById(result);
				if (macro == null)
				{
					break;
				}
				ZoomToVertices(macro.Faces.SelectMany((Face f) => f.BoundaryEdgesCcw.SelectMany((FaceHalfEdge x) => x.Vertices)).ToList(), macro.Faces.First().Shell.GetWorldMatrix(model));
				SelectObject(macro, model);
				return true;
			}
		}
		return false;
	}

	private Dictionary<FaceHalfEdge, WiCAM.Pn4000.BendModel.Base.Color?> RememberAndUnHighlightAllEdges(Model root)
	{
		Dictionary<FaceHalfEdge, WiCAM.Pn4000.BendModel.Base.Color?> dictionary = new Dictionary<FaceHalfEdge, WiCAM.Pn4000.BendModel.Base.Color?>();
		if (root != null)
		{
			WiCAM.Pn4000.BendModel.Base.Color value = new WiCAM.Pn4000.BendModel.Base.Color(1f, 1f, 1f, 0.2f);
			foreach (FaceHalfEdge item in root.GetAllFacesIEnumerable().SelectMany((Face f) => f.GetAllEdges()))
			{
				dictionary.Add(item, item.HighlightColor);
				item.HighlightColor = value;
			}
		}
		return dictionary;
	}

	private Dictionary<Face, WiCAM.Pn4000.BendModel.Base.Color?> RememberAndUnHighlightAllFaces(Model root)
	{
		Dictionary<Face, WiCAM.Pn4000.BendModel.Base.Color?> dictionary = new Dictionary<Face, WiCAM.Pn4000.BendModel.Base.Color?>();
		if (root != null)
		{
			WiCAM.Pn4000.BendModel.Base.Color value = new WiCAM.Pn4000.BendModel.Base.Color(1f, 1f, 1f, 0.2f);
			foreach (Face item in root.GetAllFacesIEnumerable())
			{
				dictionary.Add(item, item.HighlightColor);
				item.HighlightColor = value;
			}
		}
		return dictionary;
	}

	private void ZoomToVertices(List<Vertex> points, Matrix4d? worldMatrix)
	{
		double num = double.PositiveInfinity;
		double num2 = double.PositiveInfinity;
		double num3 = double.PositiveInfinity;
		double num4 = double.NegativeInfinity;
		double num5 = double.NegativeInfinity;
		double num6 = double.NegativeInfinity;
		if (points == null || points.Count <= 0 || points.First().Faces.Count <= 0 || !worldMatrix.HasValue)
		{
			return;
		}
		foreach (Vertex point in points)
		{
			Vector3d vector3d = worldMatrix.Value.Transform(point.Pos);
			num = Math.Min(num, vector3d.X);
			num2 = Math.Min(num2, vector3d.Y);
			num3 = Math.Min(num3, vector3d.Z);
			num4 = Math.Max(num4, vector3d.X);
			num5 = Math.Max(num5, vector3d.Y);
			num6 = Math.Max(num6, vector3d.Z);
		}
		Vector3d bbMin = new Vector3d(num, num2, num3);
		Vector3d bbMax = new Vector3d(num4, num5, num6);
		SelectedObjectBb = "BB World:" + Environment.NewLine + "Min " + bbMin.ToString("F1") + Environment.NewLine + "Max " + bbMax.ToString("F2");
		if (MoveScreenWhenSearching)
		{
			ScreenD3D.ZoomExtend(render: true, bbMin, bbMax, 10.0, fitFront: true, null);
		}
	}

	public void SelectTriangle(Triangle tri, Model model)
	{
		if (DoNotSelect)
		{
			DoNotSelect = false;
		}
		else if (IsVisible)
		{
			PropertyGrid.SelectedObject = tri;
			SelectedType = tri?.GetType()?.Name;
			_selectedModel = model;
			SelectedObjectBb = null;
			BackgroundSearch = Brushes.White;
			switch (ClickType)
			{
			case SearchTypes.Face:
				SelectObject(tri?.Face, model);
				break;
			case SearchTypes.FaceGroup:
				SelectObject(tri?.Face?.FaceGroup, model);
				break;
			case SearchTypes.Macro:
				SelectObject(tri?.Face?.Macro, model);
				break;
			case SearchTypes.FaceHalfEdge:
				SelectObject(tri?.Face, model);
				break;
			case SearchTypes.Triangle:
				SelectObject(tri, model);
				break;
			}
		}
	}

	private void SelectObject<T>(T obj, Model model)
	{
		if (!IsVisible)
		{
			return;
		}
		PropertyGrid.SelectedObject = new Pair<T, Model>(obj, model);
		PropertyGrid.SelectedGridItem.Expanded = true;
		SelectedType = obj?.GetType()?.Name;
		if (obj is Face face)
		{
			if (PickColor)
			{
				Color = System.Windows.Media.Color.FromArgb((byte)(face.Color.A * 255f), (byte)(face.Color.R * 255f), (byte)(face.Color.G * 255f), (byte)(face.Color.B * 255f));
				PickColor = false;
			}
			else if (AutoApplyColor)
			{
				ApplyColor_Click(null, null);
			}
		}
	}

	private void DebugInfoView_OnLoaded(object sender, RoutedEventArgs e)
	{
		System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel
		{
			Dock = DockStyle.Fill
		};
		panel.Controls.Add(PropertyGrid);
		WindowsFormsHost.Child = panel;
	}

	private void ApplyTranslate_Click(object sender, RoutedEventArgs e)
	{
		if (_selectedModel == null || _selectedModel.Shells.Count != 1)
		{
			return;
		}
		Vector3d vector3d = new Vector3d(TranslationX, TranslationY, TranslationZ);
		List<Vertex> list = _selectedModel.Shells[0].VertexCache.Values.ToList();
		_selectedModel.Shells[0].VertexCache.Clear();
		foreach (Vertex item in list)
		{
			Vector3d pos = item.Pos;
			pos += vector3d;
			item.Pos = pos;
			_selectedModel.Shells[0].VertexCache.Add(pos, item);
		}
		foreach (Face face in _selectedModel.Shells[0].Faces)
		{
			if (face.CylindricalFaceRotationAxis != null)
			{
				face.CylindricalFaceRotationAxis.Origin += vector3d;
			}
			if (face.FlatFacePlane != null)
			{
				face.FlatFacePlane.Origin += vector3d;
			}
		}
		_selectedModel.Shells[0].CreateCollisionTree();
		ScreenD3D.UpdateModelGeometry(_selectedModel, render: true);
	}

	private void RotX_Click(object sender, RoutedEventArgs e)
	{
		if (_selectedModel != null && _selectedModel.Shells.Count == 1)
		{
			Matrix4d transform = Matrix4d.RotationX(RotAngle / 180.0 * Math.PI);
			_selectedModel.ModifyVertices(transform, transformSubModels: false);
			ScreenD3D.UpdateModelGeometry(_selectedModel, render: true);
		}
	}

	private void RotY_Click(object sender, RoutedEventArgs e)
	{
		if (_selectedModel != null && _selectedModel.Shells.Count == 1)
		{
			Matrix4d transform = Matrix4d.RotationY(RotAngle / 180.0 * Math.PI);
			_selectedModel.ModifyVertices(transform, transformSubModels: false);
			ScreenD3D.UpdateModelGeometry(_selectedModel, render: true);
		}
	}

	private void RotZ_Click(object sender, RoutedEventArgs e)
	{
		if (_selectedModel != null && _selectedModel.Shells.Count == 1)
		{
			Matrix4d transform = Matrix4d.RotationZ(RotAngle / 180.0 * Math.PI);
			_selectedModel.ModifyVertices(transform, transformSubModels: false);
			ScreenD3D.UpdateModelGeometry(_selectedModel, render: true);
		}
	}

	private void SaveModel_Click(object sender, RoutedEventArgs e)
	{
		if (_selectedModel != null)
		{
			Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
			saveFileDialog.Filter = "Models |*.c3mo";
			if (saveFileDialog.ShowDialog() == true)
			{
				ModelConverter modelConverter = new ModelConverter();
				SModel sModel = modelConverter.Convert(_selectedModel);
				sModel.SubModels.Clear();
				modelConverter.Serialize(sModel, saveFileDialog.FileName);
			}
		}
	}

	private void SelectColor_Click(object sender, RoutedEventArgs e)
	{
		ColorDialog colorDialog = new ColorDialog();
		if (colorDialog.ShowDialog() == DialogResult.OK)
		{
			ColorField.Background = new SolidColorBrush(new System.Windows.Media.Color
			{
				R = colorDialog.Color.R,
				G = colorDialog.Color.G,
				B = colorDialog.Color.B,
				A = colorDialog.Color.A
			});
		}
	}

	private void ApplyColor_Click(object sender, RoutedEventArgs e)
	{
		if (PropertyGrid.SelectedObject is Pair<Face, Model> pair)
		{
			Face item = pair.Item1;
			System.Windows.Media.Color color = Color;
			item.Color = new WiCAM.Pn4000.BendModel.Base.Color((float)(int)color.R / 255f, (float)(int)color.G / 255f, (float)(int)color.B / 255f, (float)(int)color.A / 255f);
			item.ColorInitial = item.Color;
			ScreenD3D.UpdateModelAppearance(_selectedModel, render: true);
		}
	}

	private void SelectPoint_Click(object sender, RoutedEventArgs e)
	{
		if (_selectedModel == null)
		{
			return;
		}
		DoNotSelect = true;
		Vector3d last = new Vector3d(TranslationX, TranslationY, TranslationZ);
		Screen3D.InteractionMode = new SelectPointInteractionMode(Screen3D, delegate(Vector3d p)
		{
			last = p;
		}, delegate(bool ret)
		{
			if (ret)
			{
				Vector3d vector3d = _selectedModel.WorldMatrix.Inverted.Transform(last);
				TranslationX = 0.0 - vector3d.X;
				TranslationY = 0.0 - vector3d.Y;
				TranslationZ = 0.0 - vector3d.Z;
			}
		});
	}

	private void EdgeDetailSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		if (!(sender is RadSlider radSlider) || _selectedModel == null)
		{
			return;
		}
		Shell shell = _selectedModel.Shells.FirstOrDefault();
		if (shell == null)
		{
			return;
		}
		lock (shell)
		{
			double num = radSlider.Value / 180.0 * Math.PI;
			foreach (Face face in shell.Faces)
			{
				face.BoundaryEdgesCcw.Clear();
				face.HoleEdgesCw.Clear();
			}
			foreach (Triangle item in shell.Faces.SelectMany((Face f) => f.Mesh))
			{
				TriangleHalfEdge[] array = new TriangleHalfEdge[3] { item.E0, item.E1, item.E2 };
				foreach (TriangleHalfEdge triangleHalfEdge in array)
				{
					if (triangleHalfEdge.CounterEdge == null || triangleHalfEdge.CounterEdge.Triangle.CalculatedTriangleNormal.UnsignedAngle(item.CalculatedTriangleNormal) >= num)
					{
						FaceHalfEdge faceHalfEdge = new FaceHalfEdge(item.Face, EdgeType.Line);
						faceHalfEdge.IsHole = true;
						faceHalfEdge.AddVertex(triangleHalfEdge.V0);
						faceHalfEdge.AddVertex(triangleHalfEdge.V1);
						item.Face.HoleEdgesCw.Add(new List<FaceHalfEdge> { faceHalfEdge });
					}
				}
			}
		}
		ScreenD3D.RemoveModel(_selectedModel, render: false);
		ScreenD3D.AddModel(_selectedModel, _selectedModel.Parent);
	}

	private void PickColor_Click(object sender, RoutedEventArgs e)
	{
		PickColor = true;
	}

	private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (sender is GridSplitter { Parent: Grid parent } gridSplitter)
		{
			parent.RowDefinitions[Grid.GetRow(gridSplitter) - 1].Height = GridLength.Auto;
		}
	}
}
