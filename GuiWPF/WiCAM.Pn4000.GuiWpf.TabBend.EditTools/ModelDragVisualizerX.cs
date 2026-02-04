using System;
using System.Collections.Generic;
using SharpDX;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.TabBend.EditTools.Interafces;
using WiCAM.Pn4000.GuiWpf.Ui3D.InteractiveMotions;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.ScreenD3D.Renderer;
using WiCAM.Pn4000.ToolCalculation.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal class ModelDragVisualizerX : ModelDragVisualizer<Vector1d>, IModelDragVisualizerX
{
	protected ICollection<IRange> _blockedIntervals = new List<IRange>();

	protected ICollection<ISnapPoint> _snapPoints = new List<ISnapPoint>();

	private IToolsToMachineModel _toolsToMachine;

	private readonly IEditToolsSelection _toolsSelection;

	private Model? _snapModel;

	public event Action<double> DistanceChanged;

	protected override void UpdateModelAppearance(double distanceOnPrimaryDir, double distanceOnSecondaryDir)
	{
		base.UpdateModelAppearance(distanceOnPrimaryDir, distanceOnSecondaryDir);
		this.DistanceChanged?.Invoke(distanceOnPrimaryDir);
	}

	public ModelDragVisualizerX(IPnBndDoc doc, IScreen3DMain screen3D, IToolsToMachineModel toolsToMachine, IEditToolsSelection toolsSelection)
		: base(doc, screen3D, Vector3d.UnitX, (Vector3d?)null)
	{
		_toolsToMachine = toolsToMachine;
		_toolsSelection = toolsSelection;
	}

	public void Start(Model model, Model referenceSystemModel, ICollection<IRange> blockedIntervals, ICollection<ISnapPoint> snapPoints)
	{
		SetSnapPoints(blockedIntervals, snapPoints);
		Start(model, referenceSystemModel);
	}

	public void SetSnapPoints(ICollection<IRange> blockedIntervals, ICollection<ISnapPoint> snapPoints)
	{
		_blockedIntervals = blockedIntervals;
		_snapPoints = snapPoints;
	}

	public new double Stop()
	{
		if (_snapModel != null)
		{
			_screen3D.ScreenD3D.RemoveModel(_snapModel);
			_snapModel = null;
		}
		return base.Stop().distanceOnPrimaryDir;
	}

	protected override (ISnapPoint? snapPointPrimary, ISnapPoint? snapPointSecondary) ApplySnapPoints(double distance, double notUsed)
	{
		Renderer renderer = base.ScreenD3D.Renderer;
		double num = double.MaxValue;
		ISnapPoint item = null;
		Matrix4d matrix4d = ConvertMatrix(renderer.Root.Transform ?? Matrix.Identity);
		Matrix4d matrix4d2 = _toolsToMachine.GetToolSystemModel(_doc, upper: true).WorldMatrix * matrix4d;
		Matrix4d matrix4d3 = _toolsToMachine.GetToolSystemModel(_doc, upper: false).WorldMatrix * matrix4d;
		int width = renderer.Width;
		int height = renderer.Height;
		Matrix4d identity = Matrix4d.Identity;
		identity.M00 = (double)width / 2.0;
		identity.M11 = (double)(-height) / 2.0;
		identity.M30 = (double)width / 2.0;
		identity.M31 = (double)height / 2.0;
		Matrix4d matrix4d4 = ConvertMatrix(renderer.View * renderer.Projection) * identity;
		Matrix4d matrix4d5 = matrix4d2 * matrix4d4;
		Matrix4d matrix4d6 = matrix4d3 * matrix4d4;
		var (tuple2, tuple3, tuple4, tuple5, tuple6, tuple7) = renderer.GetViewPlanes(width, height, 1.0);
		foreach (ISnapPoint snapPoint in _snapPoints)
		{
			Matrix4d matrix4d7 = (snapPoint.IsUpper ? matrix4d2 : matrix4d3);
			Vector3d vector3d = matrix4d7.Transform(snapPoint.Position1);
			Vector3d vector3d2 = matrix4d7.Transform(snapPoint.Position2);
			if ((!(vector3d.Dot(tuple2.Item2) < tuple2.Item1) && !(vector3d.Dot(tuple3.Item2) < tuple3.Item1) && !(vector3d.Dot(tuple4.Item2) < tuple4.Item1) && !(vector3d.Dot(tuple5.Item2) < tuple5.Item1) && !(vector3d.Dot(tuple6.Item2) < tuple6.Item1) && !(vector3d.Dot(tuple7.Item2) < tuple7.Item1)) || (!(vector3d2.Dot(tuple2.Item2) < tuple2.Item1) && !(vector3d2.Dot(tuple3.Item2) < tuple3.Item1) && !(vector3d2.Dot(tuple4.Item2) < tuple4.Item1) && !(vector3d2.Dot(tuple5.Item2) < tuple5.Item1) && !(vector3d2.Dot(tuple6.Item2) < tuple6.Item1) && !(vector3d2.Dot(tuple7.Item2) < tuple7.Item1)))
			{
				Vector3d position = snapPoint.Position1;
				Vector3d vector3d3 = position;
				vector3d3.X = snapPoint.TargetOffsetX + distance;
				Vector3d v = vector3d3;
				Matrix4d matrix4d8 = (snapPoint.IsUpper ? matrix4d5 : matrix4d6);
				Vector2d xY = matrix4d8.Transform(position).XY;
				Vector2d xY2 = matrix4d8.Transform(v).XY;
				double length = (xY - xY2).Length;
				if (length < snapPoint.Epsilon && length < num)
				{
					num = length;
					item = snapPoint;
				}
			}
		}
		return (snapPointPrimary: item, snapPointSecondary: null);
	}

	protected override (double distanceOnPrimaryDir, double distanceOnSecondaryDir) ApplyBlockedIntervals(double distance, double distanceOnSecondaryDir)
	{
		foreach (IRange blockedInterval in _blockedIntervals)
		{
			if (!(blockedInterval.End < distance))
			{
				if (blockedInterval.Start < distance)
				{
					distance = ((distance - blockedInterval.Start < blockedInterval.End - distance) ? blockedInterval.Start : blockedInterval.End);
					break;
				}
				if (blockedInterval.Start > distance)
				{
					break;
				}
			}
		}
		return (distanceOnPrimaryDir: distance, distanceOnSecondaryDir: 0.0);
	}

	public override void Drag(Vector2f pos)
	{
		base.Drag(pos);
		SnapPointVisualization();
	}

	protected static Matrix4d ConvertMatrix(Matrix matrix)
	{
		Matrix4d result = default(Matrix4d);
		result.M00 = matrix.M11;
		result.M01 = matrix.M12;
		result.M02 = matrix.M13;
		result.M03 = matrix.M14;
		result.M10 = matrix.M21;
		result.M11 = matrix.M22;
		result.M12 = matrix.M23;
		result.M13 = matrix.M24;
		result.M20 = matrix.M31;
		result.M21 = matrix.M32;
		result.M22 = matrix.M33;
		result.M23 = matrix.M34;
		result.M30 = matrix.M41;
		result.M31 = matrix.M42;
		result.M32 = matrix.M43;
		result.M33 = matrix.M44;
		return result;
	}

	public void SnapPointVisualization()
	{
		if (!base.IsDragging)
		{
			return;
		}
		ISnapPoint snapPrimary = base.SnapPrimary;
		if (snapPrimary != null)
		{
			if (_snapModel == null)
			{
				Model model = new Model();
				model.Shell = new Shell(model);
				GeometryGenerator.AddCube(color: new WiCAM.Pn4000.BendModel.Base.Color(0f, 0f, 1f, 0.3f), shell: model.CreateAuxiliaryShell(model.Shell, AuxiliaryShellType.DebugGeometry), center: Vector3d.Zero, size: 1.0);
				_snapModel = model;
				_screen3D.ScreenD3D.AddModel(model);
			}
			Model model2 = (snapPrimary.IsUpper ? _toolsSelection.CurrentMachine.Geometry.UpperToolSystemTools : _toolsSelection.CurrentMachine.Geometry.LowerToolSystemTools);
			double z = (snapPrimary.Position1 - snapPrimary.Position2).Z;
			_snapModel.Transform = Matrix4d.Scale(10.0, 100.0, z + 20.0) * Matrix4d.Translation((snapPrimary.Position1 + snapPrimary.Position2) * 0.5) * model2.WorldMatrix;
			_screen3D.ScreenD3D.UpdateModelTransform(_snapModel);
		}
		else if (_snapModel != null)
		{
			_screen3D.ScreenD3D.RemoveModel(_snapModel);
			_snapModel = null;
		}
		snapPrimary = base.SnapSecondary;
	}
}
