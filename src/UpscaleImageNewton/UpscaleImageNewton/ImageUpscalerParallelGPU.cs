using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Algorithms;

public class ImageUpscalerParallelGPU
{
    public static void UpscaleImageNewtonOptimizedJpg(string inputImagePath, string outputImagePath, double scaleFactor)
    {
        using var context = Context.CreateDefault();
        using var accelerator = context.GetPreferredDevice(preferCPU: false).CreateAccelerator(context);

        using var originalImage = new Bitmap(inputImagePath);

        var originalBytes = GetImageBytes(originalImage, out int width, out int height, out PixelFormat pixelFormat);

        int newRows = (int)Math.Round(height * scaleFactor);
        int newCols = (int)Math.Round(width * scaleFactor);
        byte[] upscaledBytes = new byte[newCols * newRows * 4];

        using var originalBuffer = accelerator.Allocate1D<byte>(originalBytes.Length);
        using var upscaledBuffer = accelerator.Allocate1D<byte>(upscaledBytes.Length);
        float[] xi = Linspace(0, height - 1, newRows);
        float[] yi = Linspace(0, width - 1, newCols);
        using var xiBuffer = accelerator.Allocate1D(xi);
        using var yiBuffer = accelerator.Allocate1D(yi);

        originalBuffer.CopyFromCPU(originalBytes);
        xiBuffer.CopyFromCPU(xi);
        yiBuffer.CopyFromCPU(yi);

        var kernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView<byte>, ArrayView<byte>, int, int, ArrayView<float>, ArrayView<float>>(GPUProcessKernel);
        var extent = new Index2D(newRows, newCols);
        kernel(extent, originalBuffer.View, upscaledBuffer.View, width, height, xiBuffer.View, yiBuffer.View);
        accelerator.Synchronize();

        upscaledBuffer.CopyToCPU(upscaledBytes);

        var upscaledImage = CreateBitmapFromBytes(upscaledBytes, newCols, newRows, PixelFormat.Format32bppArgb);

        upscaledImage.Save(outputImagePath, ImageFormat.Jpeg);
    }

    static void GPUProcessKernel(
        Index2D index,
        ArrayView<byte> original,
        ArrayView<byte> upscaled,
        int originalWidth,
        int originalHeight,
        ArrayView<float> xi,
        ArrayView<float> yi)
    {
        int i = index.X;
        int j = index.Y;

        float xValue = xi[i];
        float yValue = yi[j];

        int x0 = XMath.Clamp((int)Math.Floor(xValue) - 1, 0, originalHeight - 3);
        int y0 = XMath.Clamp((int)Math.Floor(yValue) - 1, 0, originalWidth - 3);

        float[] xPoints = new float[3];
        float[] yPoints = new float[3];

        xPoints[0] = x0;
        xPoints[1] = x0 + 1;
        xPoints[2] = x0 + 2;

        yPoints[0] = y0;
        yPoints[1] = y0 + 1;
        yPoints[2] = y0 + 2;

        for (int c = 0; c < 4; c++)
        {
            float f00 = original[(x0 * originalWidth + y0) * 4 + c];
            float f01 = original[(x0 * originalWidth + y0 + 1) * 4 + c];
            float f02 = original[(x0 * originalWidth + y0 + 2) * 4 + c];
            float f10 = original[((x0 + 1) * originalWidth + y0) * 4 + c];
            float f11 = original[((x0 + 1) * originalWidth + y0 + 1) * 4 + c];
            float f12 = original[((x0 + 1) * originalWidth + y0 + 2) * 4 + c];
            float f20 = original[((x0 + 2) * originalWidth + y0) * 4 + c];
            float f21 = original[((x0 + 2) * originalWidth + y0 + 1) * 4 + c];
            float f22 = original[((x0 + 2) * originalWidth + y0 + 2) * 4 + c];

            float fx0 = NewtonInterpolation(xPoints, new float[] { f00, f10, f20 }, xValue);
            float fx1 = NewtonInterpolation(xPoints, new float[] { f01, f11, f21 }, xValue);
            float fx2 = NewtonInterpolation(xPoints, new float[] { f02, f12, f22 }, xValue);

            float interpolatedValue = NewtonInterpolation(yPoints, new float[] { fx0, fx1, fx2 }, yValue);

            upscaled[(i * upscaled.Extent.X / 4 + j) * 4 + c] = (byte)XMath.Clamp(interpolatedValue, 0f, 255f);
        }
    }

    static float NewtonInterpolation(float[] x, float[] y, float value)
    {
        float a0 = y[0];
        float a1 = (y[1] - y[0]) / (x[1] - x[0]);
        float a2 = ((y[2] - y[1]) / (x[2] - x[1]) - a1) / (x[2] - x[0]);

        float result = a0 + a1 * (value - x[0]) + a2 * (value - x[0]) * (value - x[1]);
        return result;
    }

    static float[] Linspace(float start, float end, int num)
    {
        float[] result = new float[num];
        float step = (end - start) / (num - 1);
        for (int i = 0; i < num; i++)
        {
            result[i] = start + i * step;
        }
        return result;
    }

    static byte[] GetImageBytes(Bitmap image, out int width, out int height, out PixelFormat pixelFormat)
    {
        width = image.Width;
        height = image.Height;
        pixelFormat = image.PixelFormat;
        var imageData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
        byte[] bytes = new byte[imageData.Stride * imageData.Height];
        Marshal.Copy(imageData.Scan0, bytes, 0, bytes.Length);
        image.UnlockBits(imageData);
        return bytes;
    }

    static Bitmap CreateBitmapFromBytes(byte[] bytes, int width, int height, PixelFormat pixelFormat)
    {
        var image = new Bitmap(width, height, pixelFormat);
        var imageData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, pixelFormat);
        Marshal.Copy(bytes, 0, imageData.Scan0, bytes.Length);
        image.UnlockBits(imageData);
        return image;
    }
}
