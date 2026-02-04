using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Enum;
using WiCAM.Pn4000.BendModel.Base.Motions;
using WiCAM.Pn4000.BendModel.Base.SpatialDataStructures;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiWpf.TabBend.EditFingers.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers;

internal class Fingers2DDragVisualizerXZ : IFingers2DDragVisualizerXZ
{
	private IPnBndDoc _doc;

	private IScreen3DMain _screen3D;

	private IFingers3DDragVisualizerXZ _dragVisualizer3D;

	private Model _ghostModel;

	private Model _dynamicSubModel;

	private Model _transparentSubModel;

	private Model _ghostModel2;

	private Model _dynamicSubModel2;

	private HashSet<Model> _invisibleModels = new HashSet<Model>();

	public bool IsDragging => _dragVisualizer3D.IsDragging;

	public event Action RaiseColorModelParts;

	public Fingers2DDragVisualizerXZ(IPnBndDoc doc, IScreen3DMain screen3D, IFingers3DDragVisualizerXZ dragVisualizer3D)
	{
		_doc = doc;
		_screen3D = screen3D;
		_dragVisualizer3D = dragVisualizer3D;
	}

	public void ColorModelParts(IPaintTool painter)
	{
		painter.SetModelOpacity(_transparentSubModel, 0.30000001192092896, applyToSubModels: true);
		foreach (Model invisibleModel in _invisibleModels)
		{
			painter.SetModelVisibility(invisibleModel, visible: false, applyToSubModels: true);
		}
		painter.SetModelVisibility(_ghostModel, visible: true, applyToSubModels: true);
		painter.SetModelVisibility(_ghostModel2, visible: true, applyToSubModels: true);
		painter.SetModelEdgeColor(_dynamicSubModel, new Color(1f, 1f, 0f), 3f, applyToSubModels: true);
	}

	public void MouseMove(object sender, MouseEventArgs e)
	{
		if (e.Handled || !IsDragging)
		{
			return;
		}
		_screen3D.Screen3D.RecalculateWpFPointToPixelPoint(e.GetPosition(_screen3D.Screen3D), out var X, out var Y);
		_dragVisualizer3D.Drag(new Vector2f(X, Y));
		if (_dynamicSubModel.PartRole == PartRole.LeftFinger)
		{
			Model fingerModel = _doc.BendSimulation.State.MachineConfig.Geometry.LeftFinger.FingerModel;
			Model fingerModel2 = _doc.BendSimulation.State.MachineConfig.Geometry.RightFinger.FingerModel;
			ModelByLinearAxesMotionData modelByLinearAxesMotionData = new ModelByLinearAxesMotionData(fingerModel, _doc.BendSimulation.State.MachineConfig.Geometry.LinearAxesByModel);
			ModelByLinearAxesMotionData modelByLinearAxesMotionData2 = new ModelByLinearAxesMotionData(fingerModel2, _doc.BendSimulation.State.MachineConfig.Geometry.LinearAxesByModel);
			Matrix4d worldMatrix = fingerModel2.WorldMatrix;
			Matrix4d worldMatrix2 = _dynamicSubModel.WorldMatrix;
			Dictionary<MotionLinearAxis, double> startPositions;
			Dictionary<MotionLinearAxis, double> endPositions;
			double maxMotionTime;
			MotionByLinearAxes motionByLinearAxes = modelByLinearAxesMotionData.MoveToLocalPoint(out startPositions, out endPositions, out maxMotionTime, worldMatrix2.TranslationVector);
			motionByLinearAxes.GoToEnd();
			AABB<Vector3d> maxDimensions = modelByLinearAxesMotionData2.GetMaxDimensions(new List<MotionLinearAxis> { _doc.BendSimulation.State.MachineConfig.Geometry.LinearAxesByType[AxisType.X1] });
			AABB<Vector3d> boundingBox = fingerModel.Shell.AABBTree.Root.BoundingBox;
			AABB<Vector3d> boundingBox2 = fingerModel2.Shell.AABBTree.Root.BoundingBox;
			maxDimensions.Min = new Vector3d(Math.Max(maxDimensions.Min.X, boundingBox.Max.X + worldMatrix2.TranslationVector.X - boundingBox2.Min.X + _doc.BendMachine.FingerStopSettings.MinFingerDistance), maxDimensions.Min.Y, maxDimensions.Min.Z);
			Vector3d translationVector = worldMatrix.TranslationVector;
			if (!maxDimensions.PointTest(translationVector, 1E-06))
			{
				Vector3d v = new Vector3d(MathExt.Clamp(translationVector.X, maxDimensions.Min.X, maxDimensions.Max.X), MathExt.Clamp(translationVector.Y, maxDimensions.Min.Y, maxDimensions.Max.Y), MathExt.Clamp(translationVector.Z, maxDimensions.Min.Z, maxDimensions.Max.Z));
				if (_ghostModel2 == null)
				{
					_ghostModel2 = new Model();
					_dynamicSubModel2 = new Model(_ghostModel2);
					_dynamicSubModel2.PartRole = fingerModel2.PartRole;
					_dynamicSubModel2.Transform = worldMatrix;
					Model model = new Model(_ghostModel2);
					model.PartRole = fingerModel2.PartRole;
					model.Transform = worldMatrix;
					_invisibleModels.Add(fingerModel2);
					foreach (Model item in fingerModel2.GetAllSubModelsWithSelf())
					{
						if (item.Shell != null)
						{
							new Model(_dynamicSubModel2)
							{
								Shell = item.Shell,
								Transform = ((item == fingerModel2) ? Matrix4d.Identity : item.Transform),
								ModelType = fingerModel2.ModelType,
								PartRole = fingerModel2.PartRole
							};
							new Model(model)
							{
								Shell = item.Shell,
								Transform = ((item == fingerModel2) ? Matrix4d.Identity : item.Transform),
								ModelType = fingerModel2.ModelType,
								PartRole = fingerModel2.PartRole,
								Opacity = 0.3
							};
						}
					}
					_screen3D.ScreenD3D.AddModel(_ghostModel2);
					this.RaiseColorModelParts?.Invoke();
				}
				if (_dynamicSubModel2 != null)
				{
					_dynamicSubModel2.Transform = Matrix4d.Translation(v);
					_screen3D.ScreenD3D.UpdateModelTransform(_dynamicSubModel2, render: true);
				}
			}
			else if (_ghostModel2 != null)
			{
				_screen3D.ScreenD3D.RemoveModel(_ghostModel2, render: false);
				_ghostModel2 = null;
				_dynamicSubModel2 = null;
				_invisibleModels.Remove(fingerModel2);
				this.RaiseColorModelParts?.Invoke();
			}
			motionByLinearAxes.GoToTime(0.0);
		}
		else
		{
			if (_dynamicSubModel.PartRole != PartRole.RightFinger)
			{
				return;
			}
			Model fingerModel3 = _doc.BendSimulation.State.MachineConfig.Geometry.LeftFinger.FingerModel;
			Model fingerModel4 = _doc.BendSimulation.State.MachineConfig.Geometry.RightFinger.FingerModel;
			ModelByLinearAxesMotionData modelByLinearAxesMotionData3 = new ModelByLinearAxesMotionData(fingerModel3, _doc.BendSimulation.State.MachineConfig.Geometry.LinearAxesByModel);
			ModelByLinearAxesMotionData modelByLinearAxesMotionData4 = new ModelByLinearAxesMotionData(fingerModel4, _doc.BendSimulation.State.MachineConfig.Geometry.LinearAxesByModel);
			Matrix4d worldMatrix3 = _dynamicSubModel.WorldMatrix;
			Matrix4d worldMatrix4 = fingerModel3.WorldMatrix;
			Dictionary<MotionLinearAxis, double> startPositions2;
			Dictionary<MotionLinearAxis, double> endPositions2;
			double maxMotionTime2;
			MotionByLinearAxes motionByLinearAxes2 = modelByLinearAxesMotionData3.MoveToLocalPoint(out startPositions2, out endPositions2, out maxMotionTime2, worldMatrix3.TranslationVector);
			motionByLinearAxes2.GoToEnd();
			List<MotionLinearAxis> excludedAxes = (from x in modelByLinearAxesMotionData4.LocalAxisToLocalDependentAxes.SelectMany((KeyValuePair<MotionLinearAxis, List<MotionLinearAxis>> x) => x.Value)
				where x.AxisType != AxisType.DieHeight
				select x).ToList();
			List<MotionLinearAxis> source = (from x in modelByLinearAxesMotionData4.LocalAxisToLocalDependentAxes
				where x.Value.All((MotionLinearAxis y) => y.AxisType != AxisType.DieHeight)
				select x.Key).ToList();
			AABB<Vector3d> maxDimensions2 = modelByLinearAxesMotionData3.GetMaxDimensions(excludedAxes);
			AABB<Vector3d> boundingBox3 = fingerModel3.Shell.AABBTree.Root.BoundingBox;
			AABB<Vector3d> boundingBox4 = fingerModel4.Shell.AABBTree.Root.BoundingBox;
			double x2 = source.Where((MotionLinearAxis x) => x.Dim == "X").Sum((MotionLinearAxis x) => x.Start.X);
			double x3 = source.Where((MotionLinearAxis x) => x.Dim == "X").Sum((MotionLinearAxis x) => x.End.X);
			double y2 = source.Where((MotionLinearAxis x) => x.Dim == "Y").Sum((MotionLinearAxis x) => x.Start.Y);
			double y3 = source.Where((MotionLinearAxis x) => x.Dim == "Y").Sum((MotionLinearAxis x) => x.End.Y);
			double z = source.Where((MotionLinearAxis x) => x.Dim == "Z").Sum((MotionLinearAxis x) => x.Start.Z);
			double z2 = source.Where((MotionLinearAxis x) => x.Dim == "Z").Sum((MotionLinearAxis x) => x.End.Z);
			maxDimensions2.Max = new Vector3d(Math.Min(maxDimensions2.Max.X, boundingBox4.Min.X + worldMatrix3.TranslationVector.X - boundingBox3.Max.X - _doc.BendMachine.FingerStopSettings.MinFingerDistance), maxDimensions2.Max.Y, maxDimensions2.Max.Z);
			maxDimensions2.Min += new Vector3d(x2, y2, z);
			maxDimensions2.Max += new Vector3d(x3, y3, z2);
			Vector3d translationVector2 = worldMatrix4.TranslationVector;
			if (!maxDimensions2.PointTest(translationVector2, 1E-06))
			{
				Vector3d v2 = new Vector3d(MathExt.Clamp(translationVector2.X, maxDimensions2.Min.X, maxDimensions2.Max.X), MathExt.Clamp(translationVector2.Y, maxDimensions2.Min.Y, maxDimensions2.Max.Y), MathExt.Clamp(translationVector2.Z, maxDimensions2.Min.Z, maxDimensions2.Max.Z));
				if (_ghostModel2 == null)
				{
					_ghostModel2 = new Model();
					_dynamicSubModel2 = new Model(_ghostModel2);
					_dynamicSubModel2.PartRole = fingerModel3.PartRole;
					_dynamicSubModel2.Transform = worldMatrix4;
					Model model2 = new Model(_ghostModel2);
					model2.PartRole = fingerModel3.PartRole;
					model2.Transform = worldMatrix4;
					_invisibleModels.Add(fingerModel3);
					foreach (Model item2 in fingerModel3.GetAllSubModelsWithSelf())
					{
						if (item2.Shell != null)
						{
							new Model(_dynamicSubModel2)
							{
								Shell = item2.Shell,
								Transform = ((item2 == fingerModel3) ? Matrix4d.Identity : item2.Transform),
								ModelType = fingerModel3.ModelType,
								PartRole = fingerModel3.PartRole
							};
							new Model(model2)
							{
								Shell = item2.Shell,
								Transform = ((item2 == fingerModel3) ? Matrix4d.Identity : item2.Transform),
								ModelType = fingerModel3.ModelType,
								PartRole = fingerModel3.PartRole,
								Opacity = 0.3
							};
						}
					}
					_screen3D.ScreenD3D.AddModel(_ghostModel2);
					this.RaiseColorModelParts?.Invoke();
				}
				if (_dynamicSubModel2 != null)
				{
					_dynamicSubModel2.Transform = Matrix4d.Translation(v2);
					_screen3D.ScreenD3D.UpdateModelTransform(_dynamicSubModel2, render: true);
				}
			}
			else if (_ghostModel2 != null)
			{
				_screen3D.ScreenD3D.RemoveModel(_ghostModel2, render: false);
				_ghostModel2 = null;
				_dynamicSubModel2 = null;
				_invisibleModels.Remove(fingerModel3);
				this.RaiseColorModelParts?.Invoke();
			}
			motionByLinearAxes2.GoToTime(0.0);
		}
	}

	public void Start(PartRole selectedFinger)
	{
		if (IsDragging)
		{
			return;
		}
		_invisibleModels.Clear();
		Model model = selectedFinger switch
		{
			PartRole.LeftFinger => _doc.BendSimulation.State.MachineConfig.Geometry.LeftFinger.FingerModel, 
			PartRole.RightFinger => _doc.BendSimulation.State.MachineConfig.Geometry.RightFinger.FingerModel, 
			_ => null, 
		};
		if (model == null)
		{
			return;
		}
		AABB<Vector3d> maxDimensions = new ModelByLinearAxesMotionData(model, _doc.BendSimulation.State.MachineConfig.Geometry.LinearAxesByModel).GetMaxDimensions();
		_ghostModel = new Model(model);
		_dynamicSubModel = new Model(_ghostModel);
		_dynamicSubModel.PartRole = selectedFinger;
		_dynamicSubModel.Transform = Matrix4d.Translation(0.0, 0.0, 0.05);
		_transparentSubModel = new Model(_ghostModel);
		_transparentSubModel.Transform = Matrix4d.Translation(0.0, 0.0, 0.05);
		_invisibleModels.Add(model);
		foreach (Model item in model.GetAllSubModelsWithSelf().ToList())
		{
			if (item.Shell != null)
			{
				_ = item.WorldMatrix;
				new Model(_dynamicSubModel)
				{
					Shell = item.Shell,
					Transform = ((item == model) ? Matrix4d.Identity : item.Transform),
					ModelType = item.ModelType,
					PartRole = item.PartRole
				};
				new Model(_transparentSubModel)
				{
					Shell = item.Shell,
					Transform = ((item == model) ? Matrix4d.Identity : item.Transform),
					ModelType = item.ModelType,
					PartRole = item.PartRole,
					Opacity = 0.3
				};
			}
		}
		_dragVisualizer3D.Start(_dynamicSubModel, model, maxDimensions.Min.X - model.WorldMatrix.TranslationVector.X, maxDimensions.Max.X - model.WorldMatrix.TranslationVector.X, maxDimensions.Min.Y - model.WorldMatrix.TranslationVector.Y, maxDimensions.Max.Y - model.WorldMatrix.TranslationVector.Y);
		_screen3D.ScreenD3D.AddModel(_ghostModel, model, render: false);
		this.RaiseColorModelParts?.Invoke();
	}

	public (double distanceOnPrimaryDir, double distanceOnSecondaryDir, Vector3d? otherFinger) Stop()
	{
		if (!IsDragging)
		{
			return (distanceOnPrimaryDir: 0.0, distanceOnSecondaryDir: 0.0, otherFinger: null);
		}
		(double, double) tuple = _dragVisualizer3D.Stop();
		Vector3d? item = _dynamicSubModel2?.WorldMatrix.TranslationVector;
		_invisibleModels.Clear();
		_dynamicSubModel = null;
		_dynamicSubModel2 = null;
		if (_ghostModel != null)
		{
			_ghostModel.Parent?.SubModels.Remove(_ghostModel);
			_screen3D.ScreenD3D.RemoveModel(_ghostModel);
			_ghostModel = null;
		}
		if (_ghostModel2 != null)
		{
			_ghostModel2.Parent?.SubModels.Remove(_ghostModel2);
			_screen3D.ScreenD3D.RemoveModel(_ghostModel2);
			_ghostModel2 = null;
		}
		return (distanceOnPrimaryDir: tuple.Item1, distanceOnSecondaryDir: tuple.Item2, otherFinger: item);
	}
}
