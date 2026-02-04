using System;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using WiCAM.Pn4000.Contracts.PnPathServices;

namespace WiCAM.Pn4000.pn4.pn4Services;

public class PnColorsService : IPnColorsService, IDisposable
{
	private int[,] _pnColors;

	private global::System.Drawing.Pen[] _pens;

	private IPnPathService _pnPathService;

	public PnColorsService(IPnPathService pathService)
	{
		this._pnPathService = pathService;
	}

	public void Init()
	{
		this._pnColors = this.LoadPnColorsFromFile();
		this._pens = new global::System.Drawing.Pen[this._pnColors.Length];
	}

	public global::System.Drawing.Pen GetPen(int colorIdx)
	{
		int num = this.CorrectColorIndex(colorIdx);
		colorIdx = this.CorrectColorIndexWithPossibilityOfDiscontinuities(colorIdx);
		if (this._pens[colorIdx] == null)
		{
			this._pens[colorIdx] = new global::System.Drawing.Pen(global::System.Drawing.Color.FromArgb(this._pnColors[num, 0], this._pnColors[num, 1], this._pnColors[num, 2]));
			if (colorIdx > 49 && colorIdx < 70)
			{
				this._pens[colorIdx].DashPattern = new float[2] { 20f, 10f };
			}
			if (colorIdx > 69 && colorIdx < 90)
			{
				this._pens[colorIdx].DashPattern = new float[4] { 20f, 4f, 1f, 4f };
			}
		}
		return this._pens[colorIdx];
	}

	public global::System.Windows.Media.Brush GetWpfBrush(int colorIdx)
	{
		return new SolidColorBrush(this.GetWpfColor(colorIdx));
	}

	public global::System.Windows.Media.Color GetWpfColor(int colorIdx)
	{
		colorIdx = this.CorrectColorIndex(colorIdx);
		return global::System.Windows.Media.Color.FromRgb((byte)this._pnColors[colorIdx, 0], (byte)this._pnColors[colorIdx, 1], (byte)this._pnColors[colorIdx, 2]);
	}

	public global::System.Drawing.Color GetDrawingColor(int colorIdx)
	{
		while (colorIdx > 1000)
		{
			colorIdx -= 1000;
		}
		colorIdx = this.CorrectColorIndex(colorIdx);
		if (colorIdx < 0 || colorIdx >= this._pnColors.Length)
		{
			colorIdx = 0;
		}
		return global::System.Drawing.Color.FromArgb(this._pnColors[colorIdx, 0], this._pnColors[colorIdx, 1], this._pnColors[colorIdx, 2]);
	}

	private int[,] LoadPnColorsFromFile()
	{
		string[] array = File.ReadAllLines(this._pnPathService.XCOLDF());
		if (array == null || array.Length == 0)
		{
			return null;
		}
		int[,] array2 = new int[array.Length, 3];
		int num = 0;
		string[] array3 = array;
		foreach (string line in array3)
		{
			array2[num, 0] = PnColorsService.ToColor(line, 0, 3);
			array2[num, 1] = PnColorsService.ToColor(line, 4, 3);
			array2[num, 2] = PnColorsService.ToColor(line, 8, 3);
			num++;
		}
		return array2;
	}

	private static int ToColor(string line, int begin, int length)
	{
		return StringConvert.ToInt(line.Substring(begin, length).Trim());
	}

	private int CorrectColorIndex(int colorIdx)
	{
		colorIdx--;
		if (colorIdx > 99)
		{
			return colorIdx;
		}
		if (colorIdx >= 70)
		{
			colorIdx -= 70;
		}
		if (colorIdx >= 50)
		{
			colorIdx -= 50;
		}
		if (colorIdx < 0 || colorIdx > 50)
		{
			colorIdx = 0;
		}
		if (colorIdx >= this._pnColors.Length)
		{
			colorIdx = 0;
		}
		return colorIdx;
	}

	private int CorrectColorIndexWithPossibilityOfDiscontinuities(int colorIdx)
	{
		colorIdx--;
		if (colorIdx < 0 || colorIdx >= this._pnColors.Length)
		{
			colorIdx = 0;
		}
		return colorIdx;
	}

	public void Dispose()
	{
		if (this._pens != null && this._pens.Length != 0)
		{
			global::System.Drawing.Pen[] pens = this._pens;
			for (int i = 0; i < pens.Length; i++)
			{
				pens[i]?.Dispose();
			}
			GC.Collect();
		}
	}
}
