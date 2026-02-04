using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.PnGeometry;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;

public class SvgReader
{
	public static List<GeoSegment2D> Read(Stream stream)
	{
		List<GeoSegment2D> list = new List<GeoSegment2D>();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(stream);
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
		xmlNamespaceManager.AddNamespace("ns", "http://www.w3.org/2000/svg");
		XmlNodeList xmlNodeList = xmlDocument.SelectNodes("//ns:path", xmlNamespaceManager);
		int num = 0;
		if (num < xmlNodeList.Count)
		{
			string value = xmlNodeList[num].Attributes["d"].Value;
			string pattern = "(M|L|A|m|l|a|\\s)";
			string[] array = (from x in Regex.Split(value, pattern)
				where x.Trim().Length > 0
				select x).ToArray();
			Vector2d lastPoint = default(Vector2d);
			for (int i = 0; i < array.Length; i++)
			{
				Vector2d start = lastPoint;
				switch (array[i])
				{
				case "M":
					lastPoint = new Vector2d(double.Parse(array[i + 1], CultureInfo.InvariantCulture), double.Parse(array[i + 2], CultureInfo.InvariantCulture));
					i++;
					break;
				case "L":
					lastPoint = new Vector2d(double.Parse(array[i + 1], CultureInfo.InvariantCulture), double.Parse(array[i + 2], CultureInfo.InvariantCulture));
					list.Add(new LineSegment2D(start, lastPoint));
					i += 2;
					break;
				case "l":
					lastPoint += new Vector2d(double.Parse(array[i + 1], CultureInfo.InvariantCulture), double.Parse(array[i + 2], CultureInfo.InvariantCulture));
					list.Add(new LineSegment2D(start, lastPoint));
					i += 2;
					break;
				case "A":
				{
					double rx = double.Parse(array[i + 1], CultureInfo.InvariantCulture);
					double.Parse(array[i + 2], CultureInfo.InvariantCulture);
					double.Parse(array[i + 3], CultureInfo.InvariantCulture);
					int num2 = int.Parse(array[i + 4], CultureInfo.InvariantCulture);
					int sweepFlag = int.Parse(array[i + 5], CultureInfo.InvariantCulture);
					lastPoint = new Vector2d(double.Parse(array[i + 6], CultureInfo.InvariantCulture), double.Parse(array[i + 7], CultureInfo.InvariantCulture));
					List<Vector2d> list2 = Circle2D.IntersectCircles(start, rx, lastPoint, rx);
					if (list2.Count == 0)
					{
						list2.Add(0.5 * (start + lastPoint));
					}
					List<CircleSegment2D> list3 = list2.Select((Vector2d center) => new CircleSegment2D(rx, center, start, lastPoint, sweepFlag == 1, CircleSegmentCreationMode.KeepStartAndEndAndRadius)).ToList();
					if (list3.Count > 0)
					{
						if (num2 == 1)
						{
							CircleSegment2D item = list3.MaxBy((CircleSegment2D x) => x.Angle);
							list.Add(item);
						}
						else
						{
							CircleSegment2D item2 = list3.MinBy((CircleSegment2D x) => x.Angle);
							list.Add(item2);
						}
					}
					i += 7;
					break;
				}
				}
			}
		}
		return list;
	}
}
