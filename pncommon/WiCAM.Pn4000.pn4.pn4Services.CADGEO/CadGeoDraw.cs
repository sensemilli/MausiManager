using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.PN3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.pn4.pn4Services.CADGEO;

public class CadGeoDraw
{
	public static class Imaging
	{
		private static bool InvokeRequired => Dispatcher.CurrentDispatcher != Application.Current.Dispatcher;

		public static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap, ILogCenterService logCenterService)
		{
			if (bitmap == null)
			{
				return null;
			}
			if (Application.Current.Dispatcher == null)
			{
				return null;
			}
			try
			{
				using MemoryStream memoryStream = new MemoryStream();
				bitmap.Save(memoryStream, ImageFormat.Png);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				if (Imaging.InvokeRequired)
				{
					return (BitmapSource)Application.Current.Dispatcher.Invoke(new Func<Stream, BitmapSource>(CreateBitmapSourceFromBitmap), DispatcherPriority.Normal, memoryStream);
				}
				return Imaging.CreateBitmapSourceFromBitmap(memoryStream);
			}
			catch (Exception e)
			{
				logCenterService.CatchRaport(e);
				return null;
			}
		}

		private static BitmapSource CreateBitmapSourceFromBitmap(Stream stream)
		{
			WriteableBitmap writeableBitmap = new WriteableBitmap(BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames.Single());
			writeableBitmap.Freeze();
			return writeableBitmap;
		}
	}

	private PN4000_2D_Database _lastDb;

	private readonly IPnColorsService _pnColorsService;

	private readonly ILogCenterService _logCenterService;

	private readonly IConfigProvider _configProvider;

	internal ImageSource DrawCadGeoWithScale(PN4000_2D_Database db, ref int width, ref int height, int background, float initialScalePartpane, ImageSource oldImage, int penPnColor)
	{
		int num = width;
		int num2 = height;
		float num3 = (float)Math.Abs(db.maxx - db.minx);
		float num4 = (float)Math.Abs(db.maxy - db.miny);
		if (num3 <= 0f || num4 <= 0f || float.IsInfinity(num3) || float.IsInfinity(num4))
		{
			width = 0;
			height = 0;
			return null;
		}
		float num5 = (float)(db.minx + (double)num3 / 2.0);
		float num6 = (float)(db.miny + (double)num4 / 2.0);
		float num7 = 1f / initialScalePartpane;
		width = (int)((double)(num3 / num7) + 10.0);
		height = (int)((double)(num4 / num7) + 10.0);
		bool flag = false;
		if (height > num2)
		{
			num7 = num4 / (float)(num2 - 10);
			width = (int)((double)(num3 / num7) + 10.0);
			height = (int)((double)(num4 / num7) + 10.0);
			flag = true;
		}
		if (width > num)
		{
			num7 = num3 / (float)(num - 10);
			width = (int)((double)(num3 / num7) + 10.0);
			height = (int)((double)(num4 / num7) + 10.0);
			flag = true;
		}
		float num8 = (float)((double)(width - 10) / 2.0 - (double)(num5 / num7) + 5.0);
		float num9 = (float)((double)(height - 10) / 2.0 - (double)(num6 / num7) + 5.0);
		if (oldImage != null && oldImage is BitmapSource && (int)(oldImage as BitmapSource).Width == width && (int)(oldImage as BitmapSource).Height == height)
		{
			return oldImage;
		}
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
		global::System.Drawing.Pen pen = ((penPnColor > 0) ? this._pnColorsService.GetPen(penPnColor) : ((!flag) ? new global::System.Drawing.Pen(global::System.Drawing.Color.Black) : new global::System.Drawing.Pen(global::System.Drawing.Color.Brown)));
		foreach (PN4000_2D_Primitive primitive in db.primitives)
		{
			if (primitive.tp == 1)
			{
				float x = (float)primitive.x1 / num7 + num8;
				float num10 = (float)primitive.y1 / num7 + num9;
				float x2 = (float)primitive.v3 / num7 + num8;
				float num11 = (float)primitive.v4 / num7 + num9;
				if (background >= 0)
				{
					pen = this._pnColorsService.GetPen(primitive.color);
				}
				graphics.DrawLine(pen, x, (float)height - num10, x2, (float)height - num11);
			}
			else if (primitive.tp == 2)
			{
				this.CheckData(primitive);
				float angle = this.GetAngle(primitive);
				float num12 = (float)primitive.x1 / num7 + num8;
				float num13 = (float)primitive.y1 / num7 + num9;
				float num14 = (float)primitive.v3 / num7;
				if (background >= 0)
				{
					pen = this._pnColorsService.GetPen(primitive.color);
				}
				try
				{
					graphics.DrawArc(pen, num12 - num14 / 2f, (float)height - (num13 + num14 / 2f), num14, num14, 360f - (float)primitive.v5, angle);
				}
				catch (Exception e)
				{
					this._logCenterService.CatchRaport(e);
				}
			}
		}
		BitmapSource bitmapSource = CadGeoDraw.CreateBitmapSourceFromBitmap(bitmap, this._logCenterService);
		if (bitmapSource != null)
		{
			bitmapSource.Freeze();
			return bitmapSource;
		}
		return bitmapSource;
	}

	internal ImageSource DrawCadGeo(string filename, int width, int height, int background, int cadgeoId)
	{
		return this.DrawCadGeo(filename, width, height, background, cadgeoId, 0);
	}

	public CadGeoDraw(IConfigProvider configProvider, ILogCenterService logCenterService, IPnColorsService pnColorsService)
	{
		this._configProvider = configProvider;
		this._logCenterService = logCenterService;
		this._pnColorsService = pnColorsService;
	}

	internal ImageSource DrawCadGeo(string filename, int width, int height, int background, int cadgeoId, int penPnColor)
	{
		this._lastDb = new PN4000_2D_Database(this._logCenterService, filename, cadgeoId);
		return this.DrawCadGeo(this._lastDb, width, height, background, cadgeoId, penPnColor);
	}

	internal ImageSource DrawCadGeo(PN4000_2D_Database db, int width, int height, int background, int cadgeoId, int penPnColor)
	{
		float num = (float)Math.Abs(db.maxx - db.minx);
		float num2 = (float)Math.Abs(db.maxy - db.miny);
		float num3 = (float)(db.minx + (double)num / 2.0);
		float num4 = (float)(db.miny + (double)num2 / 2.0);
		float num5 = num / (float)(width - 10);
		float num6 = num2 / (float)(height - 10);
		float num7 = num5;
		if (num5 < num6)
		{
			num7 = num6;
		}
		float num8 = (float)((double)(width - 10) / 2.0 - (double)(num3 / num7) + 5.0);
		float num9 = (float)((double)(height - 10) / 2.0 - (double)(num4 / num7) + 5.0);
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
		if (penPnColor > 0)
		{
			pen = this._pnColorsService.GetPen(penPnColor);
		}
		foreach (PN4000_2D_Primitive primitive in db.primitives)
		{
			if (primitive.tp == 1)
			{
				float x = (float)primitive.x1 / num7 + num8;
				float num10 = (float)primitive.y1 / num7 + num9;
				float x2 = (float)primitive.v3 / num7 + num8;
				float num11 = (float)primitive.v4 / num7 + num9;
				if (background >= 0)
				{
					pen = this._pnColorsService.GetPen(primitive.color);
				}
				graphics.DrawLine(pen, x, (float)height - num10, x2, (float)height - num11);
			}
			else if (primitive.tp == 2)
			{
				this.CheckData(primitive);
				float angle = this.GetAngle(primitive);
				float num12 = (float)primitive.x1 / num7 + num8;
				float num13 = (float)primitive.y1 / num7 + num9;
				float num14 = (float)primitive.v3 / num7;
				if (background >= 0)
				{
					pen = this._pnColorsService.GetPen(primitive.color);
				}
				try
				{
					graphics.DrawArc(pen, num12 - num14 / 2f, (float)height - (num13 + num14 / 2f), num14, num14, 360f - (float)primitive.v5, angle);
				}
				catch (Exception e)
				{
					this._logCenterService.CatchRaport(e);
				}
			}
		}
		BitmapSource bitmapSource = CadGeoDraw.CreateBitmapSourceFromBitmap(bitmap, this._logCenterService);
		if (bitmapSource != null)
		{
			bitmapSource.Freeze();
			return bitmapSource;
		}
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

	[DllImport("gdi32.dll")]
	private static extern bool DeleteObject(nint hObject);

	public static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap, ILogCenterService logCenterService)
	{
		return Imaging.CreateBitmapSourceFromBitmap(bitmap, logCenterService);
	}
}
