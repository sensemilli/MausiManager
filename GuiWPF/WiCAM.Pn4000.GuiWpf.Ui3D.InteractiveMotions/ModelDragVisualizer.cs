using SharpDX;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.ToolCalculation.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.InteractiveMotions;

internal abstract class ModelDragVisualizer<T> where T : IVector
{
	protected readonly IPnBndDoc _doc;

	protected readonly IScreen3DMain _screen3D;

	protected Model _dynamicSubModel;

	protected Vector3d _origin;

	protected Vector3d _lastPosition;

	protected Vector3d? _startingPosition;

	protected ScreenD3D11 ScreenD3D => _screen3D.ScreenD3D;

	public Vector3d ModelOrigin => _origin;

	public Vector3d PrimaryDirection { get; set; } = Vector3d.UnitX;

	public Vector3d? SecondaryDirectionForPlane { get; set; }

	public bool IsDragging { get; protected set; }

	public ISnapPoint? SnapPrimary { get; protected set; }

	public ISnapPoint? SnapSecondary { get; protected set; }

	public ModelDragVisualizer(IPnBndDoc doc, IScreen3DMain screen3D, Vector3d firstDirection, Vector3d? secondDirectionForPlane)
	{
		_doc = doc;
		_screen3D = screen3D;
		PrimaryDirection = firstDirection;
		SecondaryDirectionForPlane = secondDirectionForPlane;
	}

	public void Start(Model model, Model referenceSystemModel)
	{
		if (!IsDragging)
		{
			IsDragging = true;
			_origin = referenceSystemModel.WorldMatrix.Transform(Vector3d.Zero);
			_dynamicSubModel = model;
		}
	}

	public (double distanceOnPrimaryDir, double distanceOnSecondaryDir) Stop()
	{
		if (!IsDragging || !_startingPosition.HasValue)
		{
			IsDragging = false;
			return (distanceOnPrimaryDir: 0.0, distanceOnSecondaryDir: 0.0);
		}
		IsDragging = false;
		Line line = new Line(_origin, PrimaryDirection.Normalized);
		double num = line.ParameterOfClosestPointOnAxis(_startingPosition.Value);
		double num2 = line.ParameterOfClosestPointOnAxis(_lastPosition) - num;
		double num3 = 0.0;
		if (SecondaryDirectionForPlane.HasValue)
		{
			Line line2 = new Line(_origin, SecondaryDirectionForPlane.Value.Normalized);
			double num4 = line2.ParameterOfClosestPointOnAxis(_startingPosition.Value);
			num3 = line2.ParameterOfClosestPointOnAxis(_lastPosition) - num4;
		}
		(ISnapPoint? snapPointPrimary, ISnapPoint? snapPointSecondary) tuple = ApplySnapPoints(num2, num3);
		ISnapPoint item = tuple.snapPointPrimary;
		ISnapPoint item2 = tuple.snapPointSecondary;
		(double distanceOnPrimaryDir, double distanceOnSecondaryDir) result = ApplyBlockedIntervals(item?.PointOffsetX ?? num2, item2?.PointOffsetX ?? num3);
		SnapPrimary = null;
		SnapSecondary = null;
		_lastPosition = Vector3d.Zero;
		_startingPosition = null;
		_origin = Vector3d.Zero;
		return result;
	}

	public virtual void Drag(Vector2f pos)
	{
		if (!IsDragging)
		{
			return;
		}
		Matrix transformation = ScreenD3D.Renderer.Root.Transform ?? Matrix.Identity;
		ScreenD3D.CreateRay(pos.X, pos.Y, ref transformation, out var eyePos, out var eyeDir);
		Vector3d normalized = PrimaryDirection.Cross(eyeDir).Normalized;
		normalized = PrimaryDirection.Cross(SecondaryDirectionForPlane ?? normalized).Normalized;
		Vector3d? startingPosition = new WiCAM.Pn4000.BendModel.Base.Plane(_origin, normalized)?.IntersectRay(eyePos, eyeDir);
		if (startingPosition.HasValue)
		{
			_lastPosition = startingPosition.Value;
			Vector3d? startingPosition2 = _startingPosition;
			if (!startingPosition2.HasValue)
			{
				_startingPosition = startingPosition;
			}
			Line line = new Line(_origin, PrimaryDirection.Normalized);
			double num = line.ParameterOfClosestPointOnAxis(_startingPosition.Value);
			double num2 = line.ParameterOfClosestPointOnAxis(_lastPosition) - num;
			double num3 = 0.0;
			if (SecondaryDirectionForPlane.HasValue)
			{
				Line line2 = new Line(_origin, SecondaryDirectionForPlane.Value.Normalized);
				double num4 = line2.ParameterOfClosestPointOnAxis(_startingPosition.Value);
				num3 = line2.ParameterOfClosestPointOnAxis(_lastPosition) - num4;
			}
			(ISnapPoint? snapPointPrimary, ISnapPoint? snapPointSecondary) tuple = ApplySnapPoints(num2, num3);
			ISnapPoint item = tuple.snapPointPrimary;
			ISnapPoint item2 = tuple.snapPointSecondary;
			(double, double) tuple2 = ApplyBlockedIntervals(item?.PointOffsetX ?? num2, item2?.PointOffsetX ?? num3);
			SnapPrimary = item;
			SnapSecondary = item2;
			_dynamicSubModel.Transform = Matrix4d.Translation(PrimaryDirection * tuple2.Item1 + (SecondaryDirectionForPlane ?? Vector3d.Zero) * tuple2.Item2);
			UpdateModelAppearance(num2, num3);
			ScreenD3D.UpdateModelTransform(_dynamicSubModel);
		}
	}

	protected abstract (ISnapPoint? snapPointPrimary, ISnapPoint? snapPointSecondary) ApplySnapPoints(double distanceOnPrimaryDir, double distanceOnSecondaryDir);

	protected abstract (double distanceOnPrimaryDir, double distanceOnSecondaryDir) ApplyBlockedIntervals(double distanceOnPrimaryDir, double distanceOnSecondaryDir);

	protected virtual void UpdateModelAppearance(double distanceOnPrimaryDir, double distanceOnSecondaryDir)
	{
	}
}
