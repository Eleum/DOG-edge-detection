using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bsuir.DOG.Application
{
    public static class ImageProcessor
    {
        private static Bitmap ConvolutionFilter(Bitmap source, double[,] filter, double factor = 1, int bias = 0, bool convertToGrayscale = false)
        {
            BitmapData sourceData = source.LockBits(
                new Rectangle(0, 0, source.Width, source.Height), 
                ImageLockMode.ReadOnly, 
                PixelFormat.Format32bppArgb
            );

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            source.UnlockBits(sourceData);

            if (convertToGrayscale == true)
            {
                for (int k = 0; k < pixelBuffer.Length; k += 4)
                {
                    float rgb = pixelBuffer[k] * 0.11f;
                    rgb += pixelBuffer[k + 1] * 0.59f;
                    rgb += pixelBuffer[k + 2] * 0.3f;

                    pixelBuffer[k] = (byte)rgb;
                    pixelBuffer[k + 1] = pixelBuffer[k];
                    pixelBuffer[k + 2] = pixelBuffer[k];
                    pixelBuffer[k + 3] = 255;
                }
            }

            int filterWidth = filter.GetLength(1);
            //int filterHeight = filter.GetLength(0);

            int filterOffset = (filterWidth - 1) / 2;
            for (int offsetY = filterOffset; offsetY < source.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX < source.Width - filterOffset; offsetX++)
                {
                    double blue = 0;
                    double green = 0;
                    double red = 0;

                    int byteOffset = offsetY * sourceData.Stride + offsetX * 4;
                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            int calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);

                            blue += pixelBuffer[calcOffset] * filter[filterY + filterOffset, filterX + filterOffset];
                            green += pixelBuffer[calcOffset + 1] * filter[filterY + filterOffset, filterX + filterOffset];
                            red += pixelBuffer[calcOffset + 2] * filter[filterY + filterOffset, filterX + filterOffset];
                        }
                    }

                    blue = factor * blue + bias;
                    green = factor * green + bias;
                    red = factor * red + bias;

                    blue = blue < 0 ? 0 : (blue > 255 ? 255 : blue);
                    green = green < 0 ? 0 : (green > 255 ? 255 : green);
                    red = red < 0 ? 0 : (red > 255 ? 255 : red);

                    resultBuffer[byteOffset] = (byte)blue;
                    resultBuffer[byteOffset + 1] = (byte)green;
                    resultBuffer[byteOffset + 2] = (byte)red;
                    resultBuffer[byteOffset + 3] = 255;
                }
            }

            Bitmap result = new Bitmap(source.Width, source.Height);

            BitmapData resultData = result.LockBits(
                new Rectangle(0, 0, result.Width, result.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb
            );

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            result.UnlockBits(resultData);

            return result;
        }

        private static Bitmap SubtractImages(Bitmap source, Bitmap value, bool invert = false, int bias = 0)
        {
            BitmapData sourceData = source.LockBits(
                new Rectangle(0, 0, source.Width, source.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, resultBuffer, 0, resultBuffer.Length);

            BitmapData subtractData = value.LockBits(
                new Rectangle(0, 0, value.Width, value.Height),
                ImageLockMode.ReadOnly, 
                PixelFormat.Format32bppArgb);

            byte[] subtractBuffer = new byte[subtractData.Stride * subtractData.Height];

            Marshal.Copy(subtractData.Scan0, subtractBuffer, 0, subtractBuffer.Length);

            value.UnlockBits(subtractData);

            for (int k = 0; k < resultBuffer.Length && k < subtractBuffer.Length; k += 4)
            {
                int red;
                int green;
                int blue;

                if (invert == true)
                {
                    blue = 255 - resultBuffer[k] - subtractBuffer[k] + bias;
                    green = 255 - resultBuffer[k + 1] - subtractBuffer[k + 1] + bias;
                    red = 255 - resultBuffer[k + 2] - subtractBuffer[k + 2] + bias;
                }
                else
                {
                    blue = resultBuffer[k] - subtractBuffer[k] + bias;
                    green = resultBuffer[k + 1] - subtractBuffer[k + 1] + bias;
                    red = resultBuffer[k + 2] - subtractBuffer[k + 2] + bias;
                }

                blue = blue < 0 ? 0 : (blue > 255 ? 255 : blue);
                green = green < 0 ? 0 : (green > 255 ? 255 : green);
                red = red < 0 ? 0 : (red > 255 ? 255 : red);

                resultBuffer[k] = (byte)blue;
                resultBuffer[k + 1] = (byte)green;
                resultBuffer[k + 2] = (byte)red;
                resultBuffer[k + 3] = 255;
            }

            Marshal.Copy(resultBuffer, 0, sourceData.Scan0,
                                       resultBuffer.Length);

            source.UnlockBits(sourceData);
            return source;
        }

        public static Bitmap DifferenceOfGaussians(Bitmap source, MatrixType filterFirst, MatrixType filterSecond, int bias = 0, bool grayscale = false, bool invert = false)
        {
            Bitmap convolvedFirst = ConvolutionFilter(source, 
                Matrix.GaussianInstance(filterFirst), 
                Matrix.GetGaussianFactor(filterFirst), 
                0, grayscale);
            Bitmap convolvedSecond = ConvolutionFilter(source, 
                Matrix.GaussianInstance(filterSecond), 
                Matrix.GetGaussianFactor(filterSecond), 
                0, grayscale);

            var result = SubtractImages(convolvedFirst, convolvedSecond, invert, bias);

            return result;
        }
    }
}
