using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WiCAM.Pn4000.PN3D.Assembly;

public static class DisassembyHtml
{
	private const string htmlfilepath = "cad3d2pn\\Parts.html";

	internal static void GenerateHtmlOutput(List<DisassemblyPart> disasseblyParts, IGlobals globals)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("<!DOCTYPE html>");
		stringBuilder.AppendLine("<html lang = \"en\">");
		stringBuilder.AppendLine("<head>");
		stringBuilder.AppendLine("<meta charset = \"UTF -8\">");
		stringBuilder.AppendLine("<title > Parts </title>");
		stringBuilder.AppendLine("<style>");
		stringBuilder.AppendLine("#main { width: 700px; margin-left: auto; margin-right: auto; padding: 2px; padding-top: 10px; }");
		stringBuilder.AppendLine("hr { color: rgb(171, 173, 179); background - color: rgb(171, 173, 179); height: 2px; border - width: 0px; }");
		stringBuilder.AppendLine(".part_image { margin: 37.5px; float: left; width: 75px; height: 75px; }");
		stringBuilder.AppendLine(".part_info { width: 550px; height: 150px; border-spacing: 0px; }");
		stringBuilder.AppendLine(".part_info > tbody > tr { height: 30px; text-align: center; }");
		stringBuilder.AppendLine(".part_info > tbody > tr > th { font-size: 16px; text-align: left; }");
		stringBuilder.AppendLine(".part_info > tbody > tr > td { font-size: 18px; width: 50 %; border: 1px solid rgb(194, 197, 201); }");
		stringBuilder.AppendLine("</style>");
		stringBuilder.AppendLine("</head>");
		stringBuilder.AppendLine("<body>");
		stringBuilder.AppendLine("<div id = \"main\">");
		foreach (DisassemblyPart disasseblyPart in disasseblyParts)
		{
			if (disasseblyPart != null && disasseblyPart.PartHistory != DisassemblyPartHistory.Deleted)
			{
				stringBuilder.AppendLine("<div class = \"part\">");
				stringBuilder.AppendFormat("<img src = \"{0}.png\" alt=\"\" class =\"part_image\">", disasseblyPart.ID);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("<table class=\"part_info\">");
				stringBuilder.AppendLine("<tbody>");
				stringBuilder.AppendLine("<tr> ");
				stringBuilder.AppendFormat("<th colspan = \"2\">{0}</th>", disasseblyPart.Name);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("</tr>");
				stringBuilder.AppendLine("<tr> ");
				stringBuilder.AppendLine("<td>Part ID</td> ");
				stringBuilder.AppendFormat("<td>{0}</td>", "#" + disasseblyPart.ID);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("</tr>");
				stringBuilder.AppendLine("<tr> ");
				stringBuilder.AppendLine("<td>Instance NR</td> ");
				stringBuilder.AppendFormat("<td>{0}</td>", "(*" + disasseblyPart.InstanceNumber + ")");
				stringBuilder.AppendLine();
				if (disasseblyPart.MaterialName != null && disasseblyPart.MaterialName != "")
				{
					stringBuilder.AppendLine("</tr>");
					stringBuilder.AppendLine("<tr> ");
					stringBuilder.AppendLine("<td>Material name</td>");
					stringBuilder.AppendFormat("<td>{0}</td>", disasseblyPart.MaterialName);
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("</tr>");
				}
				if (disasseblyPart.PnMaterialID > 0)
				{
					stringBuilder.AppendLine("<tr> ");
					stringBuilder.AppendLine("<td>PN Material</td>");
					stringBuilder.AppendFormat("<td>{0}({1})</td>", globals.Materials.GetMaterialName(disasseblyPart.PnMaterialID), disasseblyPart.PnMaterialID);
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("</tr>");
				}
				stringBuilder.AppendLine("<tr> ");
				stringBuilder.AppendLine("<td>Part Type</td>");
				stringBuilder.AppendFormat("<td>{0}</td>", disasseblyPart.PartInfo.PartType.ToString());
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("</tr>");
				stringBuilder.AppendLine("<tr> ");
				stringBuilder.AppendLine("<td>Dimensions</td>");
				stringBuilder.AppendFormat("<td>{0}</td>", disasseblyPart.LenX.ToString("f2") + " x " + disasseblyPart.LenY.ToString("f2") + " x " + disasseblyPart.LenZ.ToString("f2") + " mm");
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("</tr>");
				if (disasseblyPart.Thickness > 0.0)
				{
					stringBuilder.AppendLine("<tr> ");
					stringBuilder.AppendLine("<td>Thickness</td>");
					stringBuilder.AppendFormat("<td>{0} mm</td>", disasseblyPart.Thickness);
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("</tr>");
				}
				if (disasseblyPart.BendsCount > 0)
				{
					stringBuilder.AppendLine("<tr> ");
					stringBuilder.AppendLine("<td>Bends</td>");
					stringBuilder.AppendFormat("<td>{0}</td>", disasseblyPart.BendsCount);
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("</tr>");
				}
				stringBuilder.AppendLine("</tbody>");
				stringBuilder.AppendLine("</table>");
				stringBuilder.AppendLine("<hr />");
				stringBuilder.AppendLine("</div>");
			}
		}
		stringBuilder.AppendLine("</div>");
		stringBuilder.AppendLine("</body>");
		stringBuilder.AppendLine("</html>");
		try
		{
			File.WriteAllText("cad3d2pn\\Parts.html", stringBuilder.ToString(), Encoding.UTF8);
		}
		catch (Exception e)
		{
			globals.logCenterService.CatchRaport(e);
		}
	}
}
