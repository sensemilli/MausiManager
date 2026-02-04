using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using WiCAM.Pn4000.Contracts.LogCenterServices;

namespace WiCAM.Pn4000.pn4.pn4Services.CADGEO;

public class PN4000_2D_Database
{
	private string version;

	public double thickness;

	private string name0;

	public string name1;

	private int cadgeo_id;

	private readonly ILogCenterService _logCenterService;

	public double minx;

	public double maxx;

	public double miny;

	public double maxy;

	public List<PN4000_2D_Primitive> primitives;

	public PN4000_2D_Database(ILogCenterService logCenterService)
	{
		this._logCenterService = logCenterService;
		this.Reset();
		this.thickness = 1.5;
		this.name0 = (this.name1 = string.Empty);
		this.version = " V  3.0";
	}

	public PN4000_2D_Database(ILogCenterService logCenterService, string filename, int cadgeo_id)
	{
		this._logCenterService = logCenterService;
		this.Reset();
		if (!File.Exists(filename))
		{
			return;
		}
		this.minx = double.MaxValue;
		this.maxx = double.MinValue;
		this.miny = double.MaxValue;
		this.maxy = double.MinValue;
		this.cadgeo_id = cadgeo_id;
		try
		{
			List<string> list = new List<string>();
			using (StreamReader streamReader = new StreamReader(filename))
			{
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					list.Add(text.Replace('\0', ' '));
				}
			}
			if (list.Count() < 20)
			{
				return;
			}
			this.version = list[0].Trim();
			if (this.version == "V  2.0")
			{
				if (list[19].Trim() != string.Empty)
				{
					this.thickness = Convert.ToDouble(list[19].Trim(), CultureInfo.InvariantCulture);
				}
				this.name0 = string.Empty;
				this.name1 = list[20].Trim();
			}
			else
			{
				this.thickness = Convert.ToDouble(list[38].Trim(), CultureInfo.InvariantCulture);
				this.name0 = list[39].Trim();
				this.name1 = list[40].Trim();
			}
			if (this.thickness == 0.0)
			{
				this.thickness = 2.0;
			}
			this.CADGEOIn(list, this.version == "V  2.0");
		}
		catch (Exception e)
		{
			logCenterService.CatchRaport(e);
		}
	}

	private void Reset()
	{
		this.primitives = new List<PN4000_2D_Primitive>();
	}

	public void ResetPrimitives()
	{
		foreach (PN4000_2D_Primitive primitive in this.primitives)
		{
			primitive.mark = false;
		}
	}

	public void CADGEOOut()
	{
		if (this.primitives.Count <= 0)
		{
			return;
		}
		int count = this.primitives.Count;
		StreamWriter streamWriter = new StreamWriter("CADGEO");
		streamWriter.WriteLine(this.version);
		for (int i = 1; i < 38; i++)
		{
			streamWriter.WriteLine(' ');
		}
		streamWriter.WriteLine(this.AddGaps(this.thickness.ToString(".00", CultureInfo.InvariantCulture), 7));
		streamWriter.WriteLine(this.name0);
		streamWriter.WriteLine(this.name1);
		for (int j = 0; j < count; j++)
		{
			PN4000_2D_Primitive pN4000_2D_Primitive = this.primitives.ElementAt(j);
			string text = this.AddGaps(pN4000_2D_Primitive.color.ToString(), 6);
			if (pN4000_2D_Primitive.tp == 1)
			{
				text += "     0     0     1     0     0     1     0";
			}
			streamWriter.WriteLine(string.Concat(string.Concat(string.Concat(string.Concat(text + this.AddGaps(pN4000_2D_Primitive.GetX1().ToString(".00000", CultureInfo.InvariantCulture), 14), this.AddGaps(pN4000_2D_Primitive.GetY1().ToString(".00000", CultureInfo.InvariantCulture), 14)), this.AddGaps(pN4000_2D_Primitive.GetV3().ToString(".00000", CultureInfo.InvariantCulture), 14)), this.AddGaps(pN4000_2D_Primitive.GetV4().ToString(".00000", CultureInfo.InvariantCulture), 14)), this.AddGaps(pN4000_2D_Primitive.GetV5().ToString(".00000", CultureInfo.InvariantCulture), 14)));
		}
		streamWriter.Close();
	}

	private string AddGaps(string line, int spaces)
	{
		if (line.Length < spaces)
		{
			string text = null;
			int num = spaces - line.Length;
			for (int i = 0; i < num; i++)
			{
				text += " ";
			}
			line = text + line;
		}
		return line;
	}

	private void CADGEOIn(List<string> strs, bool v2_0)
	{
		if (v2_0)
		{
			for (int i = 21; i < strs.Count; i++)
			{
				if (strs[i].Length >= 80)
				{
					this.AddCADGEOLine_v2_0(strs[i]);
				}
			}
			return;
		}
		for (int j = 41; j < strs.Count; j++)
		{
			if (strs[j].Length >= 118)
			{
				this.AddCADGEOLine(strs[j]);
			}
		}
	}

	public void UpdateDatabaseBaseOnAllElements()
	{
		this.primitives = new List<PN4000_2D_Primitive>();
		foreach (PN4000_2D_Primitive primitive in this.primitives)
		{
			if (primitive.color < 50)
			{
				this.primitives.Add(primitive);
			}
		}
	}

	private void AddCADGEOLine_v2_0(string txt)
	{
		this.AddCADGEOLine(new PN4000_2D_Primitive(txt, 3, this._logCenterService));
	}

	public void AddCADGEOLine(string txt)
	{
		PN4000_2D_Primitive pN4000_2D_Primitive = new PN4000_2D_Primitive(txt, 1, this._logCenterService);
		if (this.cadgeo_id == 0 || pN4000_2D_Primitive.id == 0 || pN4000_2D_Primitive.id == this.cadgeo_id)
		{
			this.AddCADGEOLine(pN4000_2D_Primitive);
		}
	}

	private void AddCADGEOLine(PN4000_2D_Primitive p)
	{
		switch (p.tp)
		{
		case 1:
			this.MinMaxCalc(p.x1, p.y1);
			this.MinMaxCalc(p.v3, p.v4);
			break;
		case 2:
		{
			_ = new double[2];
			double[] array = new double[2];
			_ = new double[2];
			double num = p.v3 / 2.0;
			double num2;
			double num3;
			if (p.dr == 1)
			{
				num2 = p.v4 * Math.PI / 180.0;
				num3 = p.v5 * Math.PI / 180.0;
			}
			else
			{
				num2 = p.v5 * Math.PI / 180.0;
				num3 = p.v4 * Math.PI / 180.0;
			}
			if (num2 == num3)
			{
				num2 = 0.0;
				num3 = Math.PI * 2.0;
			}
			double num4 = Math.PI * 2.0;
			num4 = ((!(num2 > num3)) ? (num3 - num2) : (Math.PI * 2.0 - num2 + num3));
			if (num4 == 0.0)
			{
				num4 = Math.PI * 2.0;
			}
			for (int i = 0; i <= 10; i++)
			{
				double num5 = num2 + (double)i / 10.0 * num4;
				array[0] = num * Math.Cos(num5) + p.x1;
				array[1] = num * Math.Sin(num5) + p.y1;
				this.MinMaxCalc(array[0], array[1]);
			}
			break;
		}
		}
		this.primitives.Add(p);
	}

	private void MinMaxCalc(double x, double y)
	{
		if (x < this.minx)
		{
			this.minx = x;
		}
		if (x > this.maxx)
		{
			this.maxx = x;
		}
		if (y < this.miny)
		{
			this.miny = y;
		}
		if (y > this.maxy)
		{
			this.maxy = y;
		}
	}
}
