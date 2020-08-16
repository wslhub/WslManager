using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace WslManager.Screens.MainForm
{
    // Image List
    partial class MainForm
    {
        private ImageList largeImageList;
        private ImageList smallImageList;
        private ImageList stateImageList;

        partial void InitializeImageList(IContainer components)
        {
            largeImageList = new ImageList()
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(96, 96),
            };
            components.Add(largeImageList);

            smallImageList = new ImageList()
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(32, 32),
            };
            components.Add(smallImageList);

            foreach (KeyValuePair<string, string> pairs in Resources.LogoImages)
            {
                using var memStream = new MemoryStream(Convert.FromBase64String(pairs.Value), false);
                var loadedImage = Image.FromStream(memStream, true);
                largeImageList.Images.Add(pairs.Key, loadedImage);
                var smallImage = ResizeImage(loadedImage, smallImageList.ImageSize.Width, smallImageList.ImageSize.Height);
                smallImageList.Images.Add(pairs.Key, smallImage);
            }

            stateImageList = new ImageList()
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(16, 16),
            };
            components.Add(stateImageList);

            foreach (KeyValuePair<string, string> pairs in Resources.StateImages)
            {
                using var memStream = new MemoryStream(Convert.FromBase64String(pairs.Value), false);
                var loadedImage = Image.FromStream(memStream, true);
                stateImageList.Images.Add(pairs.Key, loadedImage);
            }
        }

        private Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }

            return destImage;
        }
    }
}
