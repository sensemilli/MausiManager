using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WiCAM.Pn4000.JobManager.Enums;

namespace WiCAM.Pn4000.JobManager
{
    public static class FolderManager
    {
        public static ImageSource GetImageSource(string directory, ItemState folderType)
        {
            return GetImageSource(directory, new Size(16, 16), folderType);
        }

        public static ImageSource GetImageSource(string directory, Size size, ItemState folderType)
        {
            using (var icon = ShellManager.GetIcon(directory, ItemType.Folder, IconSize.Large, folderType))
            {
                return Imaging.CreateBitmapSourceFromHIcon(icon.Handle,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(size.Width, size.Height));
            }
        }
    }
}
