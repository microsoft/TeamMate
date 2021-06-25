using stdole;
using System.Drawing;
using System.Windows.Forms;

namespace Microsoft.Internal.Tools.TeamMate.Office.AddIns
{
    internal class PictureDispInteropUtilities : AxHost
    {
        private PictureDispInteropUtilities() : base(null) { }

        static public IPictureDisp ImageToPictureDisp(Image image)
        {
            return (IPictureDisp) GetIPictureDispFromPicture(image);
        }

        static public IPictureDisp IconToPictureDisp(Icon icon)
        {
            return ImageToPictureDisp(icon.ToBitmap());
        }

        static public Image PictureDispToImage(IPictureDisp picture)
        {
            return GetPictureFromIPicture(picture);
        }
    }
}
