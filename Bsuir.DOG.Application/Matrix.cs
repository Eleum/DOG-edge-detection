using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bsuir.DOG.Application
{
    public enum MatrixType
    {
        Gaussian3x3,
        Gaussian5x5Type1,
        Gaussian5x5Type2
    }

    public static class Matrix
    {
        public static double[,] GaussianInstance(MatrixType type)
        {
            switch (type)
            {
                case MatrixType.Gaussian3x3:
                    return new double[,]
                    {
                        { 1, 2, 1 },
                        { 2, 4, 2 },
                        { 1, 2, 1 },
                    };
                case MatrixType.Gaussian5x5Type1:
                    return new double[,]
                    {
                        { 2, 04, 05, 04, 2 },
                        { 4, 09, 12, 09, 4 },
                        { 5, 12, 15, 12, 5 },
                        { 4, 09, 12, 09, 4 },
                        { 2, 04, 05, 04, 2 },
                    };
                case MatrixType.Gaussian5x5Type2:
                    return new double[,]
                    {
                        { 1,   4,  6,  4,  1 },
                        { 4,  16, 24, 16,  4 },
                        { 6,  24, 36, 24,  6 },
                        { 4,  16, 24, 16,  4 },
                        { 1,   4,  6,  4,  1 },
                    };
                default:
                    return null;
            }
        }

        public static double GetGaussianFactor(MatrixType type)
        {
            switch (type)
            {
                case MatrixType.Gaussian3x3:
                    return 1.0 / 16.0;
                case MatrixType.Gaussian5x5Type1:
                    return 1.0 / 159.0;
                case MatrixType.Gaussian5x5Type2:
                    return 1.0 / 256.0;
                default:
                    return -1;
            }
        }
    }
}