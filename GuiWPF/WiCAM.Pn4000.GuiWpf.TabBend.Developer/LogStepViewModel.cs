using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.Contracts.ToolCalculation;

namespace WiCAM.Pn4000.GuiWpf.TabBend.Developer;

internal class LogStepViewModel
{
	public ILogStep LogStep { get; }

	public List<LogStepViewModel> SubSteps { get; }

	public string DescShort
	{
		get
		{
			string text = LogStep.DescShort;
			if (string.IsNullOrEmpty(text))
			{
				text = SubSteps.FirstOrDefault()?.DescShort;
			}
			if (LogStep.Duration.HasValue)
			{
				return $"{LogStep.Duration.Value.TotalMilliseconds / 1000.0:N3} {text}";
			}
			return text;
		}
	}

	public LogStepViewModel(ILogStep logStep)
	{
		LogStep = logStep;
		SubSteps = logStep.SubSteps.Select((ILogStep x) => new LogStepViewModel(x)).ToList();
	}
}
