using System.Collections.Generic;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Motions;
using WiCAM.Pn4000.BendModel.Base.SpatialDataStructures;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiWpf.TabBend.EditFingers.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers;

internal class Fingers2DDragVisualizerR : IFingers2DDragVisualizerR
{
	private IPnBndDoc _doc;

	private IScreen3DMain _screen3D;

	private IFingers3DDragVisualizerR _dragVisualizer3D;

	private Model _ghostModel;

	private Model _dynamicSubModel;

	private HashSet<Model> _invisibleModels = new HashSet<Model>();

	public bool IsDragging => _dragVisualizer3D.IsDragging;

	public Fingers2DDragVisualizerR(IPnBndDoc doc, IScreen3DMain screen3D, IFingers3DDragVisualizerR dragVisualizer3D)
	{
		_doc = doc;
		_screen3D = screen3D;
		_dragVisualizer3D = dragVisualizer3D;
	}

	public void ColorModelParts(IPaintTool painter)
	{
		foreach (Model invisibleModel in _invisibleModels)
		{
			painter.SetModelVisibility(invisibleModel, visible: false);
		}
		if (_dynamicSubModel != null)
		{
			painter.SetModelEdgeColor(_dynamicSubModel, new Color(1f, 1f, 0f), 3f);
		}
	}

	public void MouseMove(object sender, MouseEventArgs e)
	{
		if (!e.Handled && IsDragging)
		{
			_screen3D.Screen3D.RecalculateWpFPointToPixelPoint(e.GetPosition(_screen3D.Screen3D), out var X, out var Y);
			_dragVisualizer3D.Drag(new Vector2f(X, Y));
		}
	}

	public void Start(PartRole selectedFinger)
	{
		if (IsDragging)
		{
			return;
		}
		_invisibleModels.Clear();
		IFingerStop fingerStop = selectedFinger switch
		{
			PartRole.LeftFinger => _doc.BendSimulation.State.MachineConfig.Geometry.LeftFinger, 
			PartRole.RightFinger => _doc.BendSimulation.State.MachineConfig.Geometry.RightFinger, 
			_ => null, 
		};
		Model fingerModel = fingerStop.FingerModel;
		AABB<Vector3d> maxDimensions = new ModelByLinearAxesMotionData(fingerModel, _doc.BendSimulation.State.MachineConfig.Geometry.LinearAxesByModel).GetMaxDimensions();
		_ghostModel = new Model();
		_dynamicSubModel = new Model(_ghostModel);
		_dynamicSubModel.PartRole = selectedFinger;
		_dynamicSubModel.Transform = Matrix4d.Translation(0.0, 0.0, 0.05);
		Model model = new Model(_ghostModel);
		model.Transform = Matrix4d.Translation(0.0, 0.0, 0.05);
		_invisibleModels.Add(fingerModel);
		foreach (Model item in fingerModel.GetAllSubModelsWithSelf())
		{
			if (item.Shell != null)
			{
				_ = item.WorldMatrix;
				new Model(_dynamicSubModel)
				{
					Shell = item.Shell,
					Transform = ((item == fingerModel) ? Matrix4d.Identity : item.Transform),
					ModelType = item.ModelType,
					PartRole = item.PartRole
				};
				new Model(model)
				{
					Shell = item.Shell,
					Transform = ((item == fingerModel) ? Matrix4d.Identity : item.Transform),
					ModelType = item.ModelType,
					PartRole = item.PartRole,
					Opacity = 0.3
				};
			}
		}
		if ((fingerStop?.OtherFinger?.DependsOn != null || fingerStop?.DependsOn != null) && _invisibleModels.Add(fingerStop.OtherFinger.FingerModel))
		{
			Matrix4d matrix4d = fingerStop.OtherFinger.FingerModel.WorldMatrix * fingerModel.WorldMatrix.Inverted;
			foreach (Model item2 in fingerStop.OtherFinger.FingerModel.GetAllSubModelsWithSelf())
			{
				if (item2.Shell != null)
				{
					_ = item2.WorldMatrix;
					new Model(_dynamicSubModel)
					{
						Shell = item2.Shell,
						Transform = matrix4d * ((item2 == fingerStop.OtherFinger.FingerModel) ? Matrix4d.Identity : item2.Transform),
						ModelType = item2.ModelType,
						PartRole = item2.PartRole
					};
					new Model(model)
					{
						Shell = item2.Shell,
						Transform = matrix4d * ((item2 == fingerStop.OtherFinger.FingerModel) ? Matrix4d.Identity : item2.Transform),
						ModelType = item2.ModelType,
						PartRole = item2.PartRole,
						Opacity = 0.3
					};
				}
			}
		}
		_dragVisualizer3D.Start(_dynamicSubModel, fingerModel, maxDimensions.Min.Z - fingerModel.WorldMatrix.TranslationVector.Z, maxDimensions.Max.Z - fingerModel.WorldMatrix.TranslationVector.Z);
		_screen3D.ScreenD3D.AddModel(_ghostModel, fingerModel, render: false);
	}

	public double Stop()
	{
		if (!IsDragging)
		{
			return 0.0;
		}
		double result = _dragVisualizer3D.Stop();
		_invisibleModels.Clear();
		_dynamicSubModel = null;
		if (_ghostModel != null)
		{
			_ghostModel.Parent?.SubModels.Remove(_ghostModel);
			_screen3D.ScreenD3D.RemoveModel(_ghostModel);
		}
		return result;
	}
}
