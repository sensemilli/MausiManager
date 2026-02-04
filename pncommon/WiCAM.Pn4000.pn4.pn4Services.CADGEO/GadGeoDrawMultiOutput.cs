using System;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.PN3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.pn4.pn4Services.CADGEO;

public class GadGeoDrawMultiOutput
{
	private PN4000_2D_Database _db;

	private readonly ILogCenterService _logCenterService;

	private readonly IConfigProvider _configProvider;

	private readonly IPnColorsService _pnColorsService;

	private float _midx;

	private float _midy;

	private float _xsize;

	private float _ysize;

	public GadGeoDrawMultiOutput(IPnColorsService pnColorsService, ILogCenterService logCenterService, IConfigProvider configProvider)
	{
		this._pnColorsService = pnColorsService;
		this._logCenterService = logCenterService;
		this._configProvider = configProvider;
	}

	public void Init(PN4000_2D_Database edb)
	{
		this._db = edb;
		this.SetDbSize();
	}

	public void Init(string filepath)
	{
		if (!File.Exists(filepath))
		{
			this._db = new PN4000_2D_Database(this._logCenterService);
			return;
		}
		this._db = new PN4000_2D_Database(this._logCenterService, filepath, 0);
		this.SetDbSize();
	}

	private void SetDbSize()
	{
		this._xsize = (float)Math.Abs(this._db.maxx - this._db.minx);
		this._ysize = (float)Math.Abs(this._db.maxy - this._db.miny);
		this._midx = (float)(this._db.minx + (double)this._xsize / 2.0);
		this._midy = (float)(this._db.miny + (double)this._ysize / 2.0);
	}

	public ImageSource DrawCadGeo(int width, int height, int background)
	{
		float num = this._xsize / (float)(width - 10);
		float num2 = this._ysize / (float)(height - 10);
		float num3 = num;
		if (num < num2)
		{
			num3 = num2;
		}
		float num4 = (float)((double)(width - 10) / 2.0 - (double)(this._midx / num3) + 5.0);
		float num5 = (float)((double)(height - 10) / 2.0 - (double)(this._midy / num3) + 5.0);
		Bitmap bitmap = new Bitmap(width, height);
		Graphics graphics = Graphics.FromImage(bitmap);
		global::System.Drawing.Color color = global::System.Drawing.Color.White;
		GeneralUserSettingsConfig generalUserSettingsConfig = this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (background > 0)
		{
			color = this._pnColorsService.GetDrawingColor(background - 1);
		}
		if (background == 0)
		{
			color = generalUserSettingsConfig.BackgroundColor1.ToFormsColor();
		}
		if (background != -2)
		{
			graphics.FillRectangle(new SolidBrush(color), 0, 0, width, height);
		}
		global::System.Drawing.Pen pen = new global::System.Drawing.Pen(global::System.Drawing.Color.Black);
		if (this._db != null)
		{
			foreach (PN4000_2D_Primitive primitive in this._db.primitives)
			{
				if (primitive.tp == 1)
				{
					float x = (float)primitive.x1 / num3 + num4;
					float num6 = (float)primitive.y1 / num3 + num5;
					float x2 = (float)primitive.v3 / num3 + num4;
					float num7 = (float)primitive.v4 / num3 + num5;
					if (background >= 0)
					{
						pen = this._pnColorsService.GetPen(primitive.color);
					}
					graphics.DrawLine(pen, x, (float)height - num6, x2, (float)height - num7);
				}
				else if (primitive.tp == 2)
				{
					this.CheckData(primitive);
					float angle = this.GetAngle(primitive);
					float num8 = (float)primitive.x1 / num3 + num4;
					float num9 = (float)primitive.y1 / num3 + num5;
					float num10 = (float)Math.Max(primitive.v3 / (double)num3, 0.0001);
					if (background >= 0)
					{
						pen = this._pnColorsService.GetPen(primitive.color);
					}
					try
					{
						graphics.DrawArc(pen, num8 - num10 / 2f, (float)height - (num9 + num10 / 2f), num10, num10, 360f - (float)primitive.v5, angle);
					}
					catch (Exception e)
					{
						this._logCenterService.CatchRaport(e);
					}
				}
			}
		}
		BitmapSource bitmapSource = CadGeoDraw.Imaging.CreateBitmapSourceFromBitmap(bitmap, this._logCenterService);
		bitmapSource.Freeze();
		return bitmapSource;
	}

	private void CheckData(PN4000_2D_Primitive p)
	{
		if (p.dr == 1 && p.v4 > p.v5 && p.v5 == 0.0)
		{
			p.v5 = 360.0;
		}
		if (p.v4 == p.v5)
		{
			p.v4 = 0.0;
			p.v5 = 360.0;
		}
		if (p.v4 == 0.0 && p.v5 == 360.0)
		{
			p.dr = 1;
		}
	}

	private float GetAngle(PN4000_2D_Primitive p)
	{
		float num = (float)Math.Abs(p.v5 - p.v4);
		if (p.dr == 1 && p.v4 > p.v5)
		{
			num = 360f - num;
		}
		if (p.dr < 0)
		{
			num = ((!(p.v4 < p.v5)) ? (0f - num) : (-360f + num));
		}
		return num;
	}
}
