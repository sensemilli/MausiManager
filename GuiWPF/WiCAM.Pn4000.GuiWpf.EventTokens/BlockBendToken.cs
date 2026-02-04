using System;
using WiCAM.Pn4000.Contracts.EventTokens;

namespace WiCAM.Pn4000.GuiWpf.EventTokens;

public class BlockBendToken : IBlockBendToken
{
	public event Action OnBlock;

	public event Action OnUnBlock;

	public void RaiseBlock()
	{
		this.OnBlock?.Invoke();
	}

	public void RaiseUnBlock()
	{
		this.OnUnBlock?.Invoke();
	}
}
