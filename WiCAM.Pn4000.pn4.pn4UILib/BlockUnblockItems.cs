using WiCAM.Pn4000.Contracts.EventTokens;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.pn4.pn4UILib.Ribbon;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public class BlockUnblockItems
{
	private readonly PnRibbon _ribbon;

	private readonly Toolbars _toolbars;

	private readonly IBlockBendToken _blockBendToken;

	public bool IsBlockCommand { get; private set; }

	public bool bme { get; set; }

	public bool bndme { get; set; }

	public BlockUnblockItems(PnRibbon ribbon, Toolbars toolbars, IBlockBendToken blockBendToken)
	{
		_ribbon = ribbon;
		_toolbars = toolbars;
		_blockBendToken = blockBendToken;
		bme = false;
		bndme = false;
	}

	public static bool NonDisplay(IRFileRecord r)
	{
		if (r.FunctionGroup != 3 && r.FunctionName != "HG3DYN")
		{
			return r.FunctionName != "HG3ROT";
		}
		return false;
	}

	public void Block()
	{
		Command("BME");
		IsBlockCommand = true;
	}

	public void Unblock()
	{
		IsBlockCommand = false;
		Command("UBNDME");
		Command("UBME");
	}

	public void Command(string statement)
	{
		if (!IsBlockCommand)
		{
			switch (statement)
			{
			case "BME":
				_ribbon.BlockMenuEvent();
				_toolbars.BlockMenuEvent();
				_blockBendToken.RaiseBlock();
				bme = true;
				break;
			case "UBME":
				_ribbon.UnBlockMenuEvent();
				_toolbars.UnBlockMenuEvent();
				_blockBendToken.RaiseUnBlock();
				bme = false;
				break;
			case "BNDME":
				_ribbon.BlockMenuEventNotDisplay();
				_toolbars.BlockMenuEventNotDisplay();
				_blockBendToken.RaiseBlock();
				bndme = true;
				break;
			case "UBNDME":
				_ribbon.UnBlockMenuEventNotDisplay();
				_toolbars.UnBlockMenuEventNotDisplay();
				_blockBendToken.RaiseUnBlock();
				bndme = false;
				break;
			}
		}
	}
}
