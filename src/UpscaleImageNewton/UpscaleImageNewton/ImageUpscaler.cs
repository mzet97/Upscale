using System;
using System.Drawing;
using System.Drawing.Imaging;

public class ImageUpscaler
{
    public static void UpscaleImageNewtonOptimizedJpg(string inputImagePath, double scaleFactor)
    {
        // Carregar a imagem em JPG
        Bitmap originalImage = new Bitmap(inputImagePath);

        // Verificar se a imagem é colorida ou em escala de cinza
        Bitmap upscaledImage;
        if (originalImage.PixelFormat == PixelFormat.Format24bppRgb)
        {
            // Imagem colorida (RGB), processar cada canal separadamente
            Bitmap redChannel = ExtractChannel(originalImage, 0);
            Bitmap greenChannel = ExtractChannel(originalImage, 1);
            Bitmap blueChannel = ExtractChannel(originalImage, 2);

            // Aplicar o algoritmo a cada canal
            Bitmap upscaledRed = ProcessChannel(redChannel, scaleFactor);
            Bitmap upscaledGreen = ProcessChannel(greenChannel, scaleFactor);
            Bitmap upscaledBlue = ProcessChannel(blueChannel, scaleFactor);

            // Combinar os canais novamente
            upscaledImage = CombineChannels(upscaledRed, upscaledGreen, upscaledBlue);
        }
        else
        {
            // Imagem em escala de cinza
            upscaledImage = ProcessChannel(originalImage, scaleFactor);
        }

        // Salvar a imagem final em JPG
        upscaledImage.Save("upscaled_image_newton_optimized.jpg", ImageFormat.Jpeg);
        upscaledImage.Dispose();
        originalImage.Dispose();
    }

    private static Bitmap ExtractChannel(Bitmap originalImage, int channelIndex)
    {
        Bitmap channelImage = new Bitmap(originalImage.Width, originalImage.Height);
        for (int y = 0; y < originalImage.Height; y++)
        {
            for (int x = 0; x < originalImage.Width; x++)
            {
                Color originalColor = originalImage.GetPixel(x, y);
                int value = channelIndex switch
                {
                    0 => originalColor.R,
                    1 => originalColor.G,
                    2 => originalColor.B,
                    _ => 0
                };
                channelImage.SetPixel(x, y, Color.FromArgb(value, value, value));
            }
        }
        return channelImage;
    }

    private static Bitmap ProcessChannel(Bitmap channel, double scaleFactor)
    {
        int newRows = (int)Math.Round(channel.Height * scaleFactor);
        int newCols = (int)Math.Round(channel.Width * scaleFactor);

        Bitmap upscaledChannel = new Bitmap(newCols, newRows);
        double[] xi = Linspace(1, channel.Height, newRows);
        double[] yi = Linspace(1, channel.Width, newCols);

        for (int i = 0; i < newRows; i++)
        {
            for (int j = 0; j < newCols; j++)
            {
                // Interpolação simples usando Newton (usando 4 vizinhos)
                double[] rowIdx = { Math.Max(1, Math.Floor(xi[i]) - 1), Math.Min(channel.Height, Math.Ceiling(xi[i]) + 1) };
                double[] colIdx = { Math.Max(1, Math.Floor(yi[j]) - 1), Math.Min(channel.Width, Math.Ceiling(yi[j]) + 1) };

                double newValue = NewtonInterpolation(channel, rowIdx, colIdx, xi[i], yi[j]);
                upscaledChannel.SetPixel(j, i, Color.FromArgb((int)newValue, (int)newValue, (int)newValue));
            }
        }

        return upscaledChannel;
    }

    private static double[] Linspace(double start, double end, int num)
    {
        double[] result = new double[num];
        double step = (end - start) / (num - 1);
        for (int i = 0; i < num; i++)
        {
            result[i] = start + i * step;
        }
        return result;
    }

    private static double NewtonInterpolation(Bitmap channel, double[] rowIdx, double[] colIdx, double xi, double yi)
    {
        // Método simplificado para realizar interpolação de Newton.
        // Este método deve ser ajustado para realizar a interpolação bidimensional correta.
        // Aqui é implementada uma interpolação aproximada com base nos valores locais.

        // Para simplificar, vamos pegar o valor mais próximo.
        int x = (int)Math.Min(channel.Height - 1, Math.Max(0, Math.Round(xi) - 1));
        int y = (int)Math.Min(channel.Width - 1, Math.Max(0, Math.Round(yi) - 1));

        return channel.GetPixel(y, x).R;
    }

    private static Bitmap CombineChannels(Bitmap redChannel, Bitmap greenChannel, Bitmap blueChannel)
    {
        Bitmap combinedImage = new Bitmap(redChannel.Width, redChannel.Height);
        for (int y = 0; y < combinedImage.Height; y++)
        {
            for (int x = 0; x < combinedImage.Width; x++)
            {
                int red = redChannel.GetPixel(x, y).R;
                int green = greenChannel.GetPixel(x, y).R;
                int blue = blueChannel.GetPixel(x, y).R;
                combinedImage.SetPixel(x, y, Color.FromArgb(red, green, blue));
            }
        }
        return combinedImage;
    }
}
