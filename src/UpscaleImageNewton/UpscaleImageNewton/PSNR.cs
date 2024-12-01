using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

public class PSNR
{
    public static decimal Calculate(string pathOriginal, string pathAltered)
    {
        using (Bitmap original = new Bitmap(pathOriginal))
        using (Bitmap altered = ResizeImage(new Bitmap(pathAltered), original.Width, original.Height))
        {
            int width = original.Width;
            int height = original.Height;

            // Pré-carregar os pixels em arrays para evitar acesso direto ao Bitmap
            Color[,] originalPixels = LoadPixels(original);
            Color[,] alteredPixels = LoadPixels(altered);

            decimal mse = 0m;
            object lockObject = new object();

            Parallel.For(0, height, y =>
            {
                decimal localMse = 0m;

                for (int x = 0; x < width; x++)
                {
                    Color origPixel = originalPixels[y, x];
                    Color altPixel = alteredPixels[y, x];

                    decimal diffR = origPixel.R - altPixel.R;
                    decimal diffG = origPixel.G - altPixel.G;
                    decimal diffB = origPixel.B - altPixel.B;

                    localMse += (diffR * diffR + diffG * diffG + diffB * diffB) / 3m;
                }

                lock (lockObject)
                {
                    mse += localMse;
                }
            });

            mse /= (width * height);

            if (mse == 0)
                return decimal.MaxValue;

            decimal maxPixel = 255m;
            return 10m * (decimal)Math.Log10((double)(maxPixel * maxPixel) / (double)mse);
        }
    }

    private static Bitmap ResizeImage(Bitmap image, int width, int height)
    {
        Bitmap resized = new Bitmap(width, height);
        using (Graphics graphics = Graphics.FromImage(resized))
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(image, 0, 0, width, height);
        }
        return resized;
    }

    private static Color[,] LoadPixels(Bitmap bitmap)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;
        Color[,] pixels = new Color[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                pixels[y, x] = bitmap.GetPixel(x, y);
            }
        }

        return pixels;
    }
}
