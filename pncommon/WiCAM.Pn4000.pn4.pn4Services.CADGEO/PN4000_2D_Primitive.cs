using System;
using System.Collections.Generic;
using System.Globalization;
using WiCAM.Pn4000.Contracts.LogCenterServices;

namespace WiCAM.Pn4000.pn4.pn4Services.CADGEO;

public class PN4000_2D_Primitive
{
	private ILogCenterService logCenterService;

	public int color;

	public int[] data = new int[5];

	public int tp;

	public int dr;

	public double x1;

	public double y1;

	public double v3;

	public double v4;

	public double v5;

	public bool mark;

	public int id;

	public bool mark_by_user;

	public double aproximation_quality = 0.025;

	public bool delete_by_user;

	public PN4000_2D_Primitive(string txt, int idx, ILogCenterService logCenterService)
	{
		this.logCenterService = logCenterService;
		switch (idx)
		{
		case 1:
			this.GeoPrimitive(txt);
			break;
		case 2:
			this.SpecialPrimitive(txt);
			break;
		case 3:
			this.GeoPrimitiveV2(txt);
			break;
		}
		this.mark = false;
	}

	private void AddLine(double x1, double y1, double x2, double y2)
	{
		this.tp = 1;
		this.dr = 0;
		this.color = 1;
		this.x1 = x1;
		this.y1 = y1;
		this.v3 = x2;
		this.v4 = y2;
		this.v5 = 0.0;
	}

	private void AddFirstLine(string[] txt, int i)
	{
		this.tp = 1;
		this.dr = 0;
		this.color = 1;
		this.x1 = this.GetDouble(txt[i + 5], 0, 12) / 2.0;
		this.y1 = this.GetDouble(txt[i + 6], 0, 12) / 2.0;
		this.v3 = (0.0 - this.GetDouble(txt[i + 5], 0, 12)) / 2.0;
		this.v4 = this.GetDouble(txt[i + 6], 0, 12) / 2.0;
		this.v5 = 0.0;
	}

	private void AddSecondLine(string[] txt, int i)
	{
		this.tp = 1;
		this.dr = 0;
		this.color = 1;
		this.x1 = (0.0 - this.GetDouble(txt[i + 5], 0, 12)) / 2.0;
		this.y1 = this.GetDouble(txt[i + 6], 0, 12) / 2.0;
		this.v3 = (0.0 - this.GetDouble(txt[i + 5], 0, 12)) / 2.0;
		this.v4 = (0.0 - this.GetDouble(txt[i + 6], 0, 12)) / 2.0;
		this.v5 = 0.0;
	}

	private void AddThirdLine(string[] txt, int i)
	{
		this.tp = 1;
		this.dr = 0;
		this.color = 1;
		this.x1 = (0.0 - this.GetDouble(txt[i + 5], 0, 12)) / 2.0;
		this.y1 = (0.0 - this.GetDouble(txt[i + 6], 0, 12)) / 2.0;
		this.v3 = this.GetDouble(txt[i + 5], 0, 12) / 2.0;
		this.v4 = (0.0 - this.GetDouble(txt[i + 6], 0, 12)) / 2.0;
		this.v5 = 0.0;
	}

	private void AddFourthLine(string[] txt, int i)
	{
		this.tp = 1;
		this.dr = 0;
		this.color = 1;
		this.x1 = this.GetDouble(txt[i + 5], 0, 12) / 2.0;
		this.y1 = (0.0 - this.GetDouble(txt[i + 6], 0, 12)) / 2.0;
		this.v3 = this.GetDouble(txt[i + 5], 0, 12) / 2.0;
		this.v4 = this.GetDouble(txt[i + 6], 0, 12) / 2.0;
		this.v5 = 0.0;
	}

	private void AddCircle(string[] txt, int i)
	{
		this.tp = 2;
		this.dr = 1;
		this.color = 1;
		this.x1 = this.GetDouble(txt[i + 117], 0, 12);
		this.y1 = this.GetDouble(txt[i + 118], 0, 12);
		this.v3 = this.GetDouble(txt[i + 7], 0, 12);
		this.v4 = 0.0;
		this.v5 = 360.0;
	}

	private void AddFirstArc(string[] txt, int i)
	{
		this.tp = 2;
		this.dr = 1;
		this.color = 1;
		this.x1 = (0.0 - this.GetDouble(txt[i + 5], 0, 12)) / 2.0 + this.GetDouble(txt[i + 117], 0, 12);
		this.y1 = this.GetDouble(txt[i + 118], 0, 12);
		this.v3 = this.GetDouble(txt[i + 7], 0, 12);
		this.v4 = 90.0;
		this.v5 = 270.0;
	}

	private void AddSecondArc(string[] txt, int i)
	{
		this.tp = 2;
		this.dr = 1;
		this.color = 1;
		this.x1 = this.GetDouble(txt[i + 5], 0, 12) / 2.0 + this.GetDouble(txt[i + 117], 0, 12);
		this.y1 = this.GetDouble(txt[i + 118], 0, 12);
		this.v3 = this.GetDouble(txt[i + 7], 0, 12);
		this.v4 = 270.0;
		this.v5 = 90.0;
	}

	private void GeoPrimitive(string txt)
	{
		this.id = this.GetInt(txt, 19, 6);
		this.tp = this.GetInt(txt, 36, 6);
		this.dr = this.GetInt(txt, 42, 6);
		this.color = this.GetInt(txt, 0, 6);
		this.data = this.GetArrayInt(txt, 6, 6);
		this.x1 = this.GetDouble(txt, 48, 14);
		this.y1 = this.GetDouble(txt, 62, 14);
		this.v3 = this.GetDouble(txt, 76, 14);
		this.v4 = this.GetDouble(txt, 90, 14);
		this.v5 = this.GetDouble(txt, 104, 14);
	}

	private void GeoPrimitiveV2(string txt)
	{
		this.id = 0;
		this.tp = this.GetInt(txt, 16, 2);
		this.dr = this.GetInt(txt, 18, 2);
		this.color = this.GetInt(txt, 0, 2);
		this.x1 = this.GetDouble(txt, 20, 12);
		this.y1 = this.GetDouble(txt, 32, 12);
		this.v3 = this.GetDouble(txt, 44, 12);
		this.v4 = this.GetDouble(txt, 56, 12);
		this.v5 = this.GetDouble(txt, 68, 12);
	}

	private void SpecialPrimitive(string txt)
	{
		this.dr = Convert.ToInt32(this.GetDouble(txt, 64, 8));
		this.color = 1;
		this.data[0] = this.GetInt(txt, 72, 4);
		this.data[1] = this.GetInt(txt, 76, 4);
		this.x1 = this.GetDouble(txt, 4, 12);
		this.y1 = this.GetDouble(txt, 16, 12);
		this.v3 = this.GetDouble(txt, 28, 12);
		this.v4 = this.GetDouble(txt, 40, 12);
		this.v5 = this.GetDouble(txt, 52, 12);
		this.tp = this.GetInt(txt, 0, 4);
	}

	public int GetInt(string txt, int idx, int cnt)
	{
		try
		{
			return Convert.ToInt32(txt.Substring(idx, cnt).Trim());
		}
		catch (Exception e)
		{
			this.logCenterService.CatchRaport(e);
		}
		return 0;
	}

	public double GetDouble(string txt, int idx, int cnt)
	{
		try
		{
			return Convert.ToDouble(txt.Substring(idx, cnt).Trim(), CultureInfo.InvariantCulture);
		}
		catch (Exception e)
		{
			this.logCenterService.CatchRaport(e);
		}
		return 0.0;
	}

	public int[] GetArrayInt(string txt, int idx, int cnt)
	{
		try
		{
			for (int i = 0; i < 5; i++)
			{
				this.data[i] = Convert.ToInt32(txt.Substring(idx, cnt).Trim());
				idx += cnt;
			}
			return this.data;
		}
		catch (Exception e)
		{
			this.logCenterService.CatchRaport(e);
		}
		return this.data;
	}

	public int GetDir()
	{
		return this.dr;
	}

	public double GetX1()
	{
		return this.x1;
	}

	public double GetY1()
	{
		return this.y1;
	}

	public double GetV3()
	{
		return this.v3;
	}

	public double GetV4()
	{
		return this.v4;
	}

	public double GetV5()
	{
		return this.v5;
	}

	public double[] GetEnd()
	{
		if (this.tp == 1)
		{
			return new double[2] { this.v3, this.v4 };
		}
		if (this.tp == 2)
		{
			return this.GetAnglePoint(this.v5);
		}
		return null;
	}

	public double[] GetStart()
	{
		if (this.tp == 1)
		{
			return new double[2] { this.x1, this.y1 };
		}
		if (this.tp == 2)
		{
			return this.GetAnglePoint(this.v4);
		}
		return null;
	}

	public double[] GetMiddle()
	{
		if (this.tp == 2)
		{
			return this.GetAnglePoint((!(this.v4 < this.v5)) ? ((360.0 - this.v5 + this.v4) / 2.0) : ((this.v5 - this.v4) / 2.0));
		}
		return null;
	}

	private double[] GetAnglePoint(double angle)
	{
		return new double[2]
		{
			this.x1 + this.v3 / 2.0 * Math.Cos(angle * Math.PI / 180.0),
			this.y1 + this.v3 / 2.0 * Math.Sin(angle * Math.PI / 180.0)
		};
	}

	public List<double[]> GetAproximatePoints()
	{
		double num = this.aproximation_quality * this.v3 * Math.PI;
		double num2 = this.dr;
		if (this.v3 == 0.0 && this.dr != 0)
		{
			return null;
		}
		double num3 = 180.0 * Math.Acos((2.0 * this.v3 / 2.0 * this.v3 / 2.0 - num * num) / (2.0 * this.v3 / 2.0 * this.v3 / 2.0)) / Math.PI;
		List<double[]> list = new List<double[]>();
		if (this.v3 < num * Math.Sqrt(2.0))
		{
			num = this.v3 / Math.Sqrt(2.0);
			num3 = 180.0 * Math.Acos((2.0 * this.v3 / 2.0 * this.v3 / 2.0 - num * num) / (2.0 * this.v3 / 2.0 * this.v3 / 2.0)) / Math.PI;
		}
		list.Add(this.GetStart());
		if (Math.Round(this.GetStart()[0], 3) == Math.Round(this.GetEnd()[0], 3) && Math.Round(this.GetEnd()[1], 3) == Math.Round(this.GetStart()[1], 3))
		{
			for (int i = 1; i < Convert.ToInt32(360.0 / num3); i++)
			{
				this.AddPointToListOfPoints(list, num2, num3, i);
			}
		}
		else if ((this.v4 > this.v5 && num2 == 1.0) || (this.v4 < this.v5 && num2 == -1.0))
		{
			for (int j = 1; j < Convert.ToInt32((360.0 - Math.Abs(this.v5 - this.v4)) / num3); j++)
			{
				this.AddPointToListOfPoints(list, num2, num3, j);
			}
		}
		else
		{
			for (int k = 1; k < Convert.ToInt32(Math.Abs(this.v5 - this.v4) / num3); k++)
			{
				this.AddPointToListOfPoints(list, num2, num3, k);
			}
		}
		if (this.GetStart()[0] != this.GetEnd()[0] || this.GetEnd()[1] != this.GetStart()[1])
		{
			list.Add(this.GetEnd());
		}
		return list;
	}

	public void AddPointToListOfPoints(List<double[]> points, double direction, double alpha, int i)
	{
		if (direction == -1.0)
		{
			points.Add(this.GetAnglePoint(this.v4 - alpha * (double)i));
		}
		else if (direction == 1.0)
		{
			points.Add(this.GetAnglePoint(this.v4 + alpha * (double)i));
		}
	}
}
