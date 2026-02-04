using System;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.pn4.pn4Services.CADGEO;

public class PnImageSource
{
	private readonly ILogCenterService _logCenterService;

	private readonly IPnColorsService _pnColorsService;

	private readonly IConfigProvider _configProvider;

	private readonly string _pndrive = Environment.GetEnvironmentVariable("PNDRIVE");

	public PnImageSource(ILogCenterService logCenterService, IPnColorsService pnColorsService, IConfigProvider configProvider)
	{
		this._logCenterService = logCenterService;
		this._pnColorsService = pnColorsService;
		this._configProvider = configProvider;
	}

	public ImageSource GetCadGeoImageSourceScaled(PN4000_2D_Database db, ref int width, ref int height, int background, float initialScalePartpane, ImageSource oldImage, int maxw, int maxh, int pnPenColor)
	{
		if (width > maxw)
		{
			width = maxw;
		}
		if (height > maxh)
		{
			height = maxh;
		}
		return new CadGeoDraw(this._configProvider, this._logCenterService, this._pnColorsService).DrawCadGeoWithScale(db, ref width, ref height, background, initialScalePartpane, oldImage, pnPenColor);
	}

	public ImageSource GetCadGeoImage(PN4000_2D_Database db, int width, int height, int background, int penPnColor)
	{
		return new CadGeoDraw(this._configProvider, this._logCenterService, this._pnColorsService).DrawCadGeo(db, width, height, background, 0, penPnColor);
	}

	public ImageSource GetImageSource(string key, int width, int height, int background)
	{
		if (key == null)
		{
			return null;
		}
		string text = string.Empty;
		int cadgeoId = 0;
		if (!key.Contains("ID="))
		{
			if (File.Exists(key))
			{
				text = key;
			}
			else
			{
				string text2 = $"{this._pndrive}\\u\\pn\\pfiles\\img\\{key}";
				if (File.Exists(text2))
				{
					text = text2;
				}
			}
		}
		else
		{
			int num = key.IndexOf(" ID=");
			text = key.Substring(0, num);
			cadgeoId = Convert.ToInt32(key.Substring(num + 4));
			if (!File.Exists(text))
			{
				text = null;
			}
		}
		if (text == null || text == string.Empty)
		{
			if (width == 0)
			{
				width = 100;
				height = 100;
			}
			Bitmap bitmap = new Bitmap(width, height);
			global::System.Drawing.Color drawingColor = this._pnColorsService.GetDrawingColor(background);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.FillRectangle(new SolidBrush(drawingColor), 0, 0, width, height);
			graphics.DrawRectangle(new global::System.Drawing.Pen(global::System.Drawing.Color.Black), 0, 0, width - 1, height - 1);
			BitmapSource bitmapSource = CadGeoDraw.CreateBitmapSourceFromBitmap(bitmap, this._logCenterService);
			bitmapSource.Freeze();
			return bitmapSource;
		}
		if (text != string.Empty && Path.GetExtension(text) != string.Empty)
		{
			string text3 = Path.GetExtension(text).ToUpper();
			if (text3.Contains("JPG") || text3.Contains("JPEG") || text3.Contains("BMP") || text3.Contains("PNG"))
			{
				try
				{
					BitmapImage bitmapImage = new BitmapImage();
					bitmapImage.BeginInit();
					bitmapImage.UriSource = new Uri(text);
					bitmapImage.EndInit();
					if (bitmapImage.DpiX == 96.0)
					{
						return bitmapImage;
					}
					return PnImageSource.ConvertBitmapTo96Dpi(bitmapImage);
				}
				catch (Exception e)
				{
					this._logCenterService.CatchRaport(e);
				}
			}
		}
		return new CadGeoDraw(this._configProvider, this._logCenterService, this._pnColorsService).DrawCadGeo(text, width, height, background, cadgeoId);
	}

	public static BitmapSource ConvertBitmapTo96Dpi(BitmapImage bitmapImage)
	{
		double num = 96.0;
		int pixelWidth = bitmapImage.PixelWidth;
		int pixelHeight = bitmapImage.PixelHeight;
		int num2 = pixelWidth * 4;
		byte[] pixels = new byte[num2 * pixelHeight];
		bitmapImage.CopyPixels(pixels, num2, 0);
		return BitmapSource.Create(pixelWidth, pixelHeight, num, num, PixelFormats.Bgra32, null, pixels, num2);
	}
}
