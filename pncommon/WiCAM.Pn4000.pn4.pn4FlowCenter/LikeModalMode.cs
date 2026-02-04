using System.Threading;

namespace WiCAM.Pn4000.pn4.pn4FlowCenter;

public class LikeModalMode
{
	public delegate bool EndLikeModalMode();

	public bool like_modal_mode;

	public EndLikeModalMode OnEndLikeModalMode;

	public bool IsMode()
	{
		return this.like_modal_mode;
	}

	public void SetMode(EndLikeModalMode OnEndLikeModalMode)
	{
		this.like_modal_mode = true;
		this.OnEndLikeModalMode = OnEndLikeModalMode;
	}

	public void WaitEndModal()
	{
		while (this.like_modal_mode)
		{
			if (!User32Wrap.PumpMesseges())
			{
				Thread.Sleep(1);
			}
		}
	}

	public void ResetLikeModalMode()
	{
		this.like_modal_mode = false;
	}

	public void EndMode()
	{
		if (this.OnEndLikeModalMode != null && this.OnEndLikeModalMode())
		{
			this.like_modal_mode = false;
		}
	}
}
