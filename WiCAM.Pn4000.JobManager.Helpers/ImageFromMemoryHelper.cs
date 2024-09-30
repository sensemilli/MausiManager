using System;
using System.IO;
using System.Windows.Media.Imaging;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager.Helpers;

internal class ImageFromMemoryHelper
{
	public BitmapImage CreateImage(string path)
	{
		BitmapImage bitmapImage = new BitmapImage();
		if (IOHelper.FileExists(path))
		{
			FileStream fileStream = null;
			try
			{
				fileStream = File.OpenRead(path);
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = fileStream;
				bitmapImage.EndInit();
			}
			catch (Exception ex)
			{
				Logger.Exception(ex);
			}
			finally
			{
				fileStream?.Close();
			}
		}
		return bitmapImage;
	}
}
