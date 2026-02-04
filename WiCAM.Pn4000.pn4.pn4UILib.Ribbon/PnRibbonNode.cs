using System.Collections.Generic;
using pncommon.WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4Services;

namespace WiCAM.Pn4000.pn4.pn4UILib.Ribbon;

public class PnRibbonNode : IPnRibbonNode
{
	public int OrginalType { get; set; }

	public IPnRibbonNode Parent { get; set; }

	public IPnCommand Command { get; set; }

	public IPnCommand SplitCommand { get; set; }

	public List<IPnRibbonNode> Children { get; set; }

	public bool IsDefaultSmall { get; set; }

	public bool IsDefaultSplit { get; set; }

	public int UnspecifiedAdditionalInt1 { get; set; }

	public int UnspecifiedAdditionalInt2 { get; set; }

	public string UnspecifiedAdditionalString1 { get; set; }

	public string UnspecifiedAdditionalString2 { get; set; }

	public int Runtime_VisualCompretionLevel { get; set; }

	public int Runtime_RealVisualCompretionLevel { get; set; }

	public PnRibbonNode(IRFileRecord item, IPnRibbonNode Parent)
	{
		if (item.IconSize == 16)
		{
			IsDefaultSmall = true;
		}
		UnspecifiedAdditionalInt1 = item.AddValue1;
		UnspecifiedAdditionalInt2 = item.AddValue2;
		OrginalType = (int)item.Type;
		if (item.Type == RFileType.RibbonSplitButton)
		{
			IsDefaultSplit = true;
			Command = new PnCommand(item.SubRecords[0]);
			SplitCommand = new PnCommand(item);
			item.SubRecords.RemoveAt(0);
		}
		else
		{
			Command = new PnCommand(item);
		}
		this.Parent = Parent;
	}

	public PnRibbonNode()
	{
	}

	public PnRibbonNode(MFileExpert expert)
	{
		Command = new PnCommand();
		Children = new List<IPnRibbonNode>();
		PnRibbonNode pnRibbonNode = CreateEmptyGroup();
		int num = 1;
		foreach (MFileData line in expert.lines)
		{
			if (line.PnCommand == null && line.PnGroup == 0)
			{
				Children.Add(pnRibbonNode);
				pnRibbonNode = CreateEmptyGroup();
			}
			else
			{
				PnRibbonNode item = new PnRibbonNode
				{
					Command = new PnCommand
					{
						Command = line.PnCommand,
						Group = num,
						ToolTipId = line.PnTooltipId,
						LabelId = line.PnTextId,
						DefaultLabel = line.PnText,
						PnColorIndex = line.PnColorIndex
					}
				};
				pnRibbonNode.Children.Add(item);
			}
			num++;
		}
		Children.Add(pnRibbonNode);
	}

	public PnRibbonNode(List<IRFileRecord> items)
	{
		Command = new PnCommand();
		Children = new List<IPnRibbonNode>();
		PnRibbonNode pnRibbonNode = CreateEmptyGroup();
		foreach (IRFileRecord item in items)
		{
			item.IconSize = 32;
			pnRibbonNode.Children.Add(new PnRibbonNode(item, this));
		}
		Children.Add(pnRibbonNode);
	}

	public PnRibbonNode(IPnCommand current)
	{
		Command = current;
	}

	private PnRibbonNode CreateEmptyGroup()
	{
		PnRibbonNode pnRibbonNode = new PnRibbonNode();
		pnRibbonNode.Command = new PnCommand();
		pnRibbonNode.Command.DefaultLabel = string.Empty;
		pnRibbonNode.Children = new List<IPnRibbonNode>();
		return pnRibbonNode;
	}
}
