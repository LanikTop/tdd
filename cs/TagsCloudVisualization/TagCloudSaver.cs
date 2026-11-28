using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace TagsCloudVisualization;

public static class TagCloudSaver
{
    [SupportedOSPlatform("windows")]
    public static void Save(CircularCloudLayouter tagCloud, string path, int width, int height)
    {
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);
        foreach (var rectangle in tagCloud.CloudRectangles)
        {
            if (rectangle.Width >= 2 && rectangle.Height >= 2)
            {
                graphics.DrawRectangle(Pens.Black, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
            }
            else
            {
                graphics.FillRectangle(Brushes.Black, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            }
        }
        graphics.FillRectangle(Brushes.Red, tagCloud.Center.X, tagCloud.Center.Y, 1, 1);
        bitmap.Save(path, ImageFormat.Png);
    }
}
