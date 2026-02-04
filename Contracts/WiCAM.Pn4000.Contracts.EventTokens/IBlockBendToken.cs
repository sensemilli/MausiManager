using System;

namespace WiCAM.Pn4000.Contracts.EventTokens;

public interface IBlockBendToken
{
	event Action OnBlock;

	event Action OnUnBlock;

	void RaiseBlock();

	void RaiseUnBlock();
}
