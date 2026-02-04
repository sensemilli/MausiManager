using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

public class PopupValidationPendingViewModel : ViewModelBase
{
	public class StepProgress : ViewModelBase
	{
		private readonly ITranslator _translator;

		private string _desc;

		private int _current;

		private bool _hasCollision;

		public ISimulationStep step { get; set; }

		public string Desc
		{
			get
			{
				if (string.IsNullOrEmpty(this._desc))
				{
					string text = this.step.GetType().Name.ToString();
					this._desc = this._translator.Translate("l_popup.ValidationPending.SimulationStep" + text) ?? text;
				}
				return this._desc;
			}
		}

		public int BendOrderUi => this.step.BendInfo.Order + 1;

		public int StepCountValidation { get; set; }

		public int Current
		{
			get
			{
				return this._current;
			}
			set
			{
				if (this._current != value)
				{
					this._current = value;
					base.NotifyPropertyChanged("Current");
					base.NotifyPropertyChanged("Progress");
				}
			}
		}

		public bool HasCollision
		{
			get
			{
				return this._hasCollision;
			}
			set
			{
				if (value != this._hasCollision)
				{
					this._hasCollision = value;
					base.NotifyPropertyChanged("HasCollision");
				}
			}
		}

		public string Progress => this.Current + "/" + this.StepCountValidation;

		public StepProgress(ITranslator translator)
		{
			this._translator = translator;
		}
	}

	public ITranslator Translator { get; }

	public bool AutoContinueIfOk { get; }

	public bool HasErrors { get; private set; }

	public string ToolsValidationMessage { get; }

	public Brush ToolsValidationMessageColor { get; }

	public string ToolsSafetyDistanceMessage { get; }

	public Brush ToolsSafetyDistanceMessageColor { get; }

	public ObservableCollection<StepProgress> steps { get; set; } = new ObservableCollection<StepProgress>();

	public string ResultSummary { get; set; }

	public PopupValidationPendingViewModel(ITranslator translator, bool toolsValid, string toolsValidationMessage, bool autoContinueIfOk)
	{
		this.Translator = translator;
		this.AutoContinueIfOk = autoContinueIfOk;
		this.ToolsValidationMessage = toolsValidationMessage;
		if (toolsValid)
		{
			this.ToolsValidationMessageColor = Brushes.Green;
		}
		else
		{
			this.ToolsValidationMessageColor = Brushes.Red;
			this.HasErrors = true;
		}
		this.ResultSummary = translator.Translate("l_popup.ValidationPending.waiting");
	}

	public void SimulationTick(ISimulationThread simulation, bool visible)
	{
		ISimulationStep step = simulation.State.ActiveStep;
		bool hasCollision = simulation.State.HasCollisions;
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			StepProgress stepProgress = this.steps.LastOrDefault();
			if (stepProgress?.step != step)
			{
				stepProgress = new StepProgress(this.Translator)
				{
					step = step,
					StepCountValidation = step.StepCountValidation,
					Current = 1,
					HasCollision = false
				};
				this.steps.Add(stepProgress);
			}
			else
			{
				stepProgress.Current++;
			}
			if (!stepProgress.HasCollision && hasCollision)
			{
				stepProgress.HasCollision = true;
			}
		});
	}

	public void StopEvent(ISimulationThread obj)
	{
		List<(double start, double end, bool userAccepted)> collisions = obj.GetCollisionIntervals().ToList();
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			if (collisions.Count > 0)
			{
				this.ResultSummary = this.Translator.Translate("BendView.BendPipe.SimulationValidationCollisions", collisions.Count);
				this.HasErrors = true;
			}
			else
			{
				this.ResultSummary = this.Translator.Translate("BendView.BendPipe.SimulationValidationOK");
			}
			base.NotifyPropertyChanged("ResultSummary");
		});
	}
}
