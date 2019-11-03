using Bsuir.DOG.Application;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bsuir.DOG.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = System.Console.ReadLine();
            var image = new Image<Bgr, byte>(path.Replace("\"", ""));

            var result = ImageProcessor.DifferenceOfGaussians(image.Bitmap, MatrixType.Gaussian3x3, MatrixType.Gaussian5x5Type1);
            result.Save("processed.jpg");
            Process.Start("processed.jpg");
        }
    }
}
