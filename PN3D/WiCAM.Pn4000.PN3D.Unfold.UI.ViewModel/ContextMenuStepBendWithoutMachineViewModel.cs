using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendTable;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration.ViewModels;
using WiCAM.Pn4000.PN3D.Converter;
using WiCAM.Pn4000.PN3D.Unfold.UI.Model;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;

public class ContextMenuStepBendWithoutMachineViewModel : ScreenControlBaseViewModel, ISubViewModel
{
	private readonly IPnBndDoc _doc;

	private int _numberOfBends;

	private InchConverter _radius;

	private InchConverter _bendDeductionMiddle;

	private InchConverter _bendDeductionStartEnd;

	private bool _applyDeductionValues;

	private Visibility _visible;

	private ICommand _applyChanges;

	private ICommand _repealStepBend;

	public int NumberOfBends
	{
		get
		{
			return this._numberOfBends;
		}
		set
		{
			this._numberOfBends = value;
			base.NotifyPropertyChanged("NumberOfBends");
		}
	}

	public double Radius
	{
		get
		{
			return this._radius.ConvertedValue;
		}
		set
		{
			this._radius.ConvertedValue = value;
			base.NotifyPropertyChanged("Radius");
		}
	}

	public bool ApplyDeductionValues
	{
		get
		{
			return this._applyDeductionValues;
		}
		set
		{
			this._applyDeductionValues = value;
			base.NotifyPropertyChanged("ApplyDeductionValues");
		}
	}

	public double BendDeductionMiddle
	{
		get
		{
			return this._bendDeductionMiddle.ConvertedValue;
		}
		set
		{
			this._bendDeductionMiddle.ConvertedValue = value;
			base.NotifyPropertyChanged("BendDeductionMiddle");
		}
	}

	public double BendDeductionStartEnd
	{
		get
		{
			return this._bendDeductionStartEnd.ConvertedValue;
		}
		set
		{
			this._bendDeductionStartEnd.ConvertedValue = value;
			base.NotifyPropertyChanged("BendDeductionStartEnd");
		}
	}

	public Visibility Visible
	{
		get
		{
			return this._visible;
		}
		set
		{
			if (this._visible != value)
			{
				this._visible = value;
				base.NotifyPropertyChanged("Visible");
			}
		}
	}

	public ContextMenuStepBendWithoutMachineModel ModelImpl { get; }

	public ICommand ApplyChangesCommand => this._applyChanges ?? (this._applyChanges = new RelayCommand(ApplyChanges));

	public ICommand RepealStepBendCommand => this._repealStepBend ?? (this._repealStepBend = new RelayCommand(RepealStepBend));

	public event Action<ISubViewModel, Triangle, global::WiCAM.Pn4000.BendModel.Model, double, double, Vector3d, MouseButtonEventArgs> Closed;

	public event Action RequestRepaint;

	public ContextMenuStepBendWithoutMachineViewModel(Screen3D screen, global::WiCAM.Pn4000.BendModel.Model modelRoot, Vector3d anchorPoint3d, ContextMenuStepBendWithoutMachineModel modelImpl, IGlobals globals, IPnBndDoc doc)
		: base(screen, modelRoot, anchorPoint3d)
	{
		this._doc = doc;
		this.ModelImpl = modelImpl;
		base.SetOpacityMin(globals.ConfigProvider.InjectOrCreate<GeneralUserSettingsConfig>().DialogOpacity);
		List<FaceGroup> list = this.ModelImpl.StepBendsInUnfoldModel.OrderBy((FaceGroup x) => x.SubBendIndex).ToList();
		FaceGroup faceGroup = list[0];
		FaceGroup faceGroup2 = list[1];
		double thickness = faceGroup.GetShell().Thickness;
		this.NumberOfBends = this.ModelImpl.StepBendsInUnfoldModel.Count;
		this._radius = new InchConverter(faceGroup.ConcaveAxis.Radius);
		this._bendDeductionMiddle = new InchConverter(BendDataCalculator.BendDeductionFromKFactor(thickness, faceGroup2.ConvexAxis.OpeningAngle, faceGroup2.ConcaveAxis.Radius, faceGroup2.KFactor));
		this._bendDeductionStartEnd = new InchConverter(BendDataCalculator.BendDeductionFromKFactor(thickness, faceGroup.ConvexAxis.OpeningAngle, faceGroup.ConcaveAxis.Radius, faceGroup.KFactor));
		this.OnViewChanged();
	}

	public override bool Close()
	{
		this.Closed?.Invoke(this, null, null, 0.0, 0.0, Vector3d.Zero, null);
		return base.Close();
	}

	public void SetActive(bool active)
	{
	}

	public void KeyUp(object sender, IPnInputEventArgs e)
	{
	}

	public void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
	}

	public void ColorModelParts(IPaintTool paintTool)
	{
	}

	public void MouseEnterCommand()
	{
		base.Opacity = base.OpacityMax;
	}

	public void MouseLeaveCommand()
	{
		base.Opacity = base.OpacityMin;
	}

	private void ApplyChanges(object param)
	{
		this.ModelImpl.ApplyParameters(this.NumberOfBends, this._radius.Value, this._applyDeductionValues ? new double?(this._bendDeductionMiddle.Value) : ((double?)null), this._applyDeductionValues ? new double?(this._bendDeductionStartEnd.Value) : ((double?)null));
		this._doc.SetModelDefaultColors();
		base.Screen.ScreenD3D.UpdateAllModelAppearance(render: true);
		this.Close();
	}

	private void RepealStepBend(object param)
	{
		this.ModelImpl.RepealStepBend();
		this.Close();
	}

	public void MouseMove(object sender, MouseEventArgs e)
	{
	}
}
