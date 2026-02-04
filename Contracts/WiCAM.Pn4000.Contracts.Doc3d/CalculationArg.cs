using System;
using System.Threading;

namespace WiCAM.Pn4000.Contracts.Doc3d;

public class CalculationArg : ICalculationArg
{
	public string Status { get; private set; }

	public double? Progress { get; private set; }

	public CancellationToken CancellationToken { get; }

	public CancellationTokenSource? CancellationTokenSource { get; }

	public ICalculationDebugArg? DebugInfo { get; set; }

	public event Action<ICalculationArg>? StatusChanged;

	public CalculationArg(bool canCancel)
	{
		if (canCancel)
		{
			this.CancellationTokenSource = new CancellationTokenSource();
			this.CancellationToken = this.CancellationTokenSource.Token;
		}
		else
		{
			this.CancellationToken = CancellationToken.None;
		}
	}

	public void SetStatus(string status, bool update)
	{
		this.Status = status;
		if (update)
		{
			this.StatusChanged?.Invoke(this);
		}
	}

	public void SetProgress(double? progress, bool update)
	{
		this.Progress = progress;
		if (update)
		{
			this.StatusChanged?.Invoke(this);
		}
	}

	public bool TryCancelCalculation(bool update = true)
	{
		if (!this.CancellationToken.CanBeCanceled)
		{
			return false;
		}
		this.CancellationTokenSource?.Cancel();
		if (update)
		{
			this.StatusChanged?.Invoke(this);
		}
		return true;
	}
}
