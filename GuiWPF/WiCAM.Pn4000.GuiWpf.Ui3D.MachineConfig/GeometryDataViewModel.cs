using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel.Base.Enum;
using WiCAM.Pn4000.BendModel.Base.Motions;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class GeometryDataViewModel : ViewModelBase
{
	public class EditableAxis
	{
		private readonly Action _changedAction;

		private double _speed;

		public AxisType AxisType { get; set; }

		public AxisKind AxisKind { get; set; }

		public double Speed
		{
			get
			{
				return _speed;
			}
			set
			{
				_speed = value;
				_changedAction?.Invoke();
			}
		}

		public List<EditableSpeedItem> Speeds { get; set; }

		public bool HasSpeeds
		{
			get
			{
				if (Speeds != null)
				{
					return Speeds.Count > 0;
				}
				return false;
			}
		}

		public EditableAxis(AxisType axisType, double speed, List<EditableSpeedItem> speeds, AxisKind axisKind, Action changedAction)
		{
			AxisType = axisType;
			_speed = speed;
			Speeds = speeds;
			AxisKind = axisKind;
			_changedAction = changedAction;
		}
	}

	public class EditableSpeedItem
	{
		private readonly Action _changedAction;

		private double _speed;

		public SpeedType Type { get; set; }

		public double Speed
		{
			get
			{
				return _speed;
			}
			set
			{
				_speed = value;
				_changedAction?.Invoke();
			}
		}

		public EditableSpeedItem(SpeedType type, double speed, Action changedAction)
		{
			Type = type;
			_speed = speed;
			_changedAction = changedAction;
		}
	}

	public enum AxisKind
	{
		LinearAxis,
		RotationAxis
	}

	private ChangedConfigType _changed;

	public Action<ChangedConfigType> DataChanged;

	public List<EditableAxis> EditableAxes { get; set; }

	public void Init(IBendMachineGeometry geometry)
	{
		EditableAxes = new List<EditableAxis>();
		List<KeyValuePair<AxisType, MotionAxisBase>> list = new List<KeyValuePair<AxisType, MotionAxisBase>>();
		list.AddRange(geometry.LinearAxesByType.ToList().ConvertAll((KeyValuePair<AxisType, MotionLinearAxis> x) => new KeyValuePair<AxisType, MotionAxisBase>(x.Key, x.Value)));
		list.AddRange(geometry.RotationAxisByType.ToList().ConvertAll((KeyValuePair<AxisType, MotionRotationAxis> x) => new KeyValuePair<AxisType, MotionAxisBase>(x.Key, x.Value)));
		Action propertyChangedAction = delegate
		{
			_changed = ChangedConfigType.MachineConfig;
		};
		foreach (KeyValuePair<AxisType, MotionAxisBase> item in list)
		{
			AxisKind axisKind = ((item.Value.GetType() == typeof(MotionRotationAxis)) ? AxisKind.RotationAxis : AxisKind.LinearAxis);
			List<EditableSpeedItem> speeds = item.Value.Speeds.ConvertAll((SpeedItem x) => new EditableSpeedItem(x.Type, x.Speed, propertyChangedAction));
			EditableAxes.Add(new EditableAxis(item.Key, item.Value.Speed, speeds, axisKind, propertyChangedAction));
		}
		_changed = ChangedConfigType.NoChanges;
	}

	public void Save(IBendMachineGeometry saveTarget)
	{
		foreach (EditableAxis editableAxis in EditableAxes)
		{
			List<SpeedItem> speeds = editableAxis.Speeds.ConvertAll((EditableSpeedItem x) => new SpeedItem
			{
				Speed = x.Speed,
				Type = x.Type
			});
			if (editableAxis.AxisKind == AxisKind.LinearAxis)
			{
				MotionLinearAxis motionLinearAxis = saveTarget.LinearAxesByType[editableAxis.AxisType];
				motionLinearAxis.Speed = editableAxis.Speed;
				motionLinearAxis.Speeds = speeds;
			}
			else if (editableAxis.AxisKind == AxisKind.RotationAxis)
			{
				MotionRotationAxis motionRotationAxis = saveTarget.RotationAxisByType[editableAxis.AxisType];
				motionRotationAxis.Speed = editableAxis.Speed;
				motionRotationAxis.Speeds = speeds;
			}
		}
		DataChanged?.Invoke(_changed);
	}

	public void Dispose()
	{
	}
}
