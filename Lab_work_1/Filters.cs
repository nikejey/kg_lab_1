using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Lab_work_1
{
    abstract class Filters
    {
        protected abstract Color calculateNewPixelColor(Bitmap sourseImage, int x, int y);

        virtual public Bitmap processImage (Bitmap sourseImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourseImage.Width, sourseImage.Height);
            for(int i = 0; i < sourseImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourseImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourseImage, i, j));
                }
            }
            return resultImage;
        }

        public int Clamp (int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }

    //точечные фильтры
    //инверсия

    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            Color sourceColor = sourseImage.GetPixel(x, y);

            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }
    }

    //полутоновый фильтр

    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int intensity = Convert.ToInt32(sourceColor.R * 0.299 + sourceColor.G * 0.587 + sourceColor.B * 0.114);
            intensity = Clamp(intensity, 0, 255);
            Color resultColor = Color.FromArgb(intensity,
                                               intensity,
                                               intensity);
            return resultColor;
        }
    }
    
    //сепия

    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            Color sourceColor = sourseImage.GetPixel(x, y);

            double k = 60.0;
            double intensity = 0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B;

            Color resultColor = Color.FromArgb(Clamp((int)(intensity + 2.0 * k), 0, 255), 
                                                  Clamp((int)(intensity + k * 0.5), 0, 255), 
                                                    Clamp((int)(intensity - 1.0 * k), 0, 255));
            return resultColor;
        }
    }
    
    //перенос

    class TransferFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            if (x + 50 > sourseImage.Width - 1)
                return Color.Transparent;
            else
            {
                Color resultColor = sourseImage.GetPixel(x + 50, y);
                return resultColor;
            }
        }
    }
    
    //поворот

    class TurnFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            int x0 = (int)(sourseImage.Width / 2), y0 = (int)(sourseImage.Height / 2);
            double w = Math.PI / 4.0;
            int xx = (int)((x - x0) * Math.Cos(w) - (y - y0) * Math.Sin(w) + x0);
            int yy = (int)((x - x0) * Math.Sin(w) + (y - y0) * Math.Cos(w) + y0);

            if((xx < sourseImage.Width - 1)&&(yy < sourseImage.Height - 1)&&(xx > 0)&&(yy > 0))
            {
                Color resultColor = sourseImage.GetPixel(xx, yy);
                return resultColor;
            }
            return Color.Transparent;
        }
    }
    
    //волны

    class WavesFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            int xx = (int)(x + 20*Math.Sin(2.0 * Math.PI * y / 60.0));
            int yy = y;

            if ((xx < sourseImage.Width - 1) && (yy < sourseImage.Height - 1) && (xx > 0) && (yy > 0))
            {
                Color resultColor = sourseImage.GetPixel(xx, yy);
                return resultColor;
            }
            return Color.Transparent;
        }
    }
    
    //фильтр "серый мир"

    class GrayWorldFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            Color sourceColor = sourseImage.GetPixel(x, y);

            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }

        public override Bitmap processImage(Bitmap sourseImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourseImage.Width, sourseImage.Height);

            double R = 0.0, G = 0.0, B = 0.0;
            double Avg = 0.0;

            Color temp;
            for (int i = 0; i < sourseImage.Width; i++)
                for (int j = 0; j < sourseImage.Height; j++)
                {
                    temp = sourseImage.GetPixel(i, j);
                    R += temp.R; G += temp.G; B += temp.B;
                }
            R = (double)R / (sourseImage.Width * sourseImage.Height);
            G = (double)G / (sourseImage.Width * sourseImage.Height);
            B = (double)B / (sourseImage.Width * sourseImage.Height);
            Avg = (R + G + B) / 3.0d;

            Color sourceColor, resultColor;

            for (int i = 0; i < sourseImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourseImage.Height; j++)
                {
                    sourceColor = sourseImage.GetPixel(i, j);
                    resultColor = Color.FromArgb(Clamp((int)(sourceColor.R * Avg / R), 0, 255),
                                                Clamp((int)(sourceColor.G * Avg / G), 0, 255),
                                                  Clamp((int)(sourceColor.B * Avg / B), 0, 255));

                    resultImage.SetPixel(i, j, resultColor);
                }
            }
            return resultImage;
        }
    }

    //фильтр "идеальный отражатель"

    class PerfectReflectorFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            Color sourceColor = sourseImage.GetPixel(x, y);

            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }

        public override Bitmap processImage(Bitmap sourseImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourseImage.Width, sourseImage.Height);

            double Rm = 0.0, Gm = 0.0, Bm = 0.0;

            Color temp;
            for (int i = 0; i < sourseImage.Width; i++)
                for (int j = 0; j < sourseImage.Height; j++)
                {
                    temp = sourseImage.GetPixel(i, j);
                    if (temp.R > Rm)
                        Rm = temp.R;
                    if (temp.G > Gm)
                        Gm = temp.G;
                    if (temp.B > Bm)
                        Bm = temp.B;
                }

            Color sourceColor, resultColor;

            for (int i = 0; i < sourseImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourseImage.Height; j++)
                {
                    sourceColor = sourseImage.GetPixel(i, j);
                    resultColor = Color.FromArgb(Clamp((int)(sourceColor.R * 255 / Rm), 0, 255),
                                                    Clamp((int)(sourceColor.G * 255 / Gm), 0, 255),
                                                        Clamp((int)(sourceColor.B * 255 / Bm), 0, 255));

                    resultImage.SetPixel(i, j, resultColor);
                }
            }
            return resultImage;
        }
    }

    //медианный фильтр

    class MedianFilter : Filters
    {
        private
            int size;
        public MedianFilter(int size)
        {
            this.size = size;
        }
        public override Bitmap processImage(Bitmap sourseImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourseImage.Width, sourseImage.Height);
            int N = 3;
            int radius = (int)(N / 2);
            
            for (int i = radius; i < sourseImage.Width - radius; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = radius; j < sourseImage.Height - radius; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourseImage, i, j));
                }
            }
            return resultImage;
        }

        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            Color sourceColor = sourseImage.GetPixel(x, y);
            int radius = (size / 2);
            int ind_median = size * size / 2;

            int[] local_R = new int [9];
            int[] local_G = new int [9];
            int[] local_B = new int [9];

            int k = 0;
            for (int i = x - radius; i <= x + radius; i++)
                for (int j = y - radius; j <= y + radius; j++)
                {
                    local_R[k] = sourseImage.GetPixel(i, j).R;
                    local_G[k] = sourseImage.GetPixel(i, j).G;
                    local_B[k] = sourseImage.GetPixel(i, j).B;
                    k++;
                }
            Array.Sort(local_R);
            Array.Sort(local_G);
            Array.Sort(local_B);


            Color resultColor = Color.FromArgb(local_R[ind_median], local_G[ind_median], local_B[ind_median]);
            return resultColor;
        }

    }

    //увеличение яркости

    class IncreaseBrightness : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(Clamp(sourceColor.R + 10, 0, 255),
                                               Clamp(sourceColor.G + 10, 0, 255),
                                               Clamp(sourceColor.B + 10, 0, 255));
            return resultColor;
        }
    }

    //коррекция с опорным цветом

    class CorrectionWithReferenceColor : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return sourceImage.GetPixel(x, y);
        }

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            double Rsrc = 124, Gsrc = 149, Bsrc = 171;  

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / sourceImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    int newR = Clamp((int)(calculateNewPixelColor(sourceImage, i, j).R * (255 - Math.Abs(Rsrc - calculateNewPixelColor(sourceImage, i, j).R)) / Rsrc), 0, 255);
                    int newG = Clamp((int)(calculateNewPixelColor(sourceImage, i, j).G * (255 - Math.Abs(Gsrc - calculateNewPixelColor(sourceImage, i, j).G)) / Gsrc), 0, 255);
                    int newB = Clamp((int)(calculateNewPixelColor(sourceImage, i, j).B * (255 - Math.Abs(Bsrc - calculateNewPixelColor(sourceImage, i, j).B)) / Bsrc), 0, 255);
                    resultImage.SetPixel(i, j, Color.FromArgb(newR, newG, newB));
                }
            }
            return resultImage;
        }
    }

    //линейное растяжение гистограммы

    class LinearStretchingHistogram : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return sourceImage.GetPixel(x, y);
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap result = new Bitmap(sourceImage.Width, sourceImage.Height);
            int XminR = 0, XmaxR = 0, XmaxG = 0, XminG = 0, XmaxB = 0, XminB = 0;
            double progress = 0.0;

            for (int i = 0; i < sourceImage.Width; i++, progress += 0.5)
            {
                worker.ReportProgress((int)((float)progress / sourceImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color tmp = sourceImage.GetPixel(i, j);
                    if (XminR > tmp.R)
                        XminR = tmp.R;

                    if (XmaxR < tmp.R)
                        XmaxR = tmp.R;

                    if (XminG > tmp.G)
                        XminG = tmp.G;

                    if (XmaxG < tmp.G)
                        XmaxG = tmp.G;

                    if (XminB > tmp.B)
                        XminB = tmp.B;

                    if (XmaxB < tmp.B)
                        XmaxB = tmp.B;
                }
            }
            for (int i = 0; i < sourceImage.Width; i++, progress += 0.5)
            {
                worker.ReportProgress((int)((float)progress / sourceImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    int R = sourceImage.GetPixel(i, j).R;
                    int G = sourceImage.GetPixel(i, j).G;
                    int B = sourceImage.GetPixel(i, j).B;
                    result.SetPixel(i, j, Color.FromArgb(Clamp(Clamp(((255 * (R - XminR)) / (XmaxR - XminR)), 0, 255) + R, 0, 255),
                                                         Clamp(Clamp(((255 * (G - XminR)) / (XmaxG - XminG)), 0, 255) + G, 0, 255),
                                                         Clamp(Clamp(((255 * (B - XminR)) / (XmaxB - XminB)), 0, 255) + B, 0, 255)));
                }
            }
            return result;
        }
    }

    //эффект стекла

    class GlassFilter : Filters
    {
        private Random rand = new Random();
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int k, l;
            k = Clamp((int)(x + (rand.NextDouble() - 0.5) * 10), 0, sourceImage.Width - 1);
            l = Clamp((int)(y + (rand.NextDouble() - 0.5) * 10), 0, sourceImage.Height - 1);
            Color sourceColor = sourceImage.GetPixel(k, l);
            Color resultColor = Color.FromArgb(sourceColor.R, sourceColor.G, sourceColor.B);

            return resultColor;
        }
    }

    //перевод в бинарное

    class BinaryFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color s = sourceImage.GetPixel(x, y);
            if (s.R < 127 && s.G < 127 && s.B < 127)
                return Color.FromArgb(0, 0, 0);
            else
                return Color.FromArgb(255, 255, 255);
        }
    }



    //Матричные фильтры

    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float [,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            float resultR = 0;
            float resultG = 0;
            float resultB = 0;

            for(int l = -radiusY; l <= radiusY; l++)
                for(int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourseImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourseImage.Height - 1);
                    Color neighColor = sourseImage.GetPixel(idX, idY);
                    resultR += neighColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighColor.B * kernel[k + radiusX, l + radiusY];
                }
            return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
        }
    }

    //блюр (размытие)

    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                {
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
                }
        }
    }
    
    //фильтр Гаусса

    class GaussianFilter : MatrixFilter
    {
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }
        public void createGaussianKernel(int radius, float sigma)
        {
            //определение размера ядра
            int size = 2 * radius + 1;
            //создание ядра фильтра
            kernel = new float[size, size];
            //коэффициент нормировки ядра
            float norm = 0;
            //рассчитывание ядра линейного фильтра
            for(int i = -radius; i <= radius; i++)
                for(int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (2 * sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            //нормирование ядра
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }
    }
    
    //резкость

    class SharpnessFilter : MatrixFilter
    {
        public SharpnessFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                {
                    if((i == 1)&&(j == 1))
                    {
                        kernel[i, j] = 9.0f;
                    }
                    else
                        kernel[i, j] = -1.0f;
                }
        }
    }
    
    //блюр в движении

    class MotionBlurFilter : MatrixFilter
    {
        public MotionBlurFilter()
        {
            int sizeX = 9;
            int sizeY = 9;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                {
                    if(i == j)
                        kernel[i, j] = 1.0f / (float)(sizeY);
                    else
                        kernel[i, j] = 0.0f;
                }
        }
    }

    //фильтр Собеля

    class SobelFilter : MatrixFilter
    {
        protected float[,] xkernel = null;
        protected float[,] ykernel = null;

        public SobelFilter()
        {
            int size = 3;
            xkernel = new float[size, size];
            xkernel[0, 0] = -1.0f; xkernel[0, 1] = 0.0f; xkernel[0, 2] = 1.0f;
            xkernel[1, 0] = -2.0f; xkernel[1, 1] = 0.0f; xkernel[1, 1] = 2.0f;
            xkernel[2, 0] = -1.0f; xkernel[2, 1] = 0.0f; xkernel[2, 2] = 1.0f;
            ykernel = new float[size, size];
            ykernel[0, 0] = -1.0f; ykernel[0, 1] = -2.0f; ykernel[0, 2] = -1.0f;
            ykernel[1, 0] = 0.0f; ykernel[1, 1] = 0.0f; ykernel[1, 1] = 0.0f;
            ykernel[2, 0] = 1.0f; ykernel[2, 1] = 2.0f; ykernel[2, 2] = 1.0f;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int xradiusX = xkernel.GetLength(0) / 2;
            int xradiusY = xkernel.GetLength(1) / 2;
            float xresultR = 0.0f;
            float xresultG = 0.0f;
            float xresultB = 0.0f;
            for (int l = -xradiusY; l <= xradiusY; l++)
            {
                for (int k = -xradiusX; k <= xradiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    xresultR += neighborColor.R * xkernel[k + xradiusX, l + xradiusY];
                    xresultG += neighborColor.G * xkernel[k + xradiusX, l + xradiusY];
                    xresultB += neighborColor.B * xkernel[k + xradiusX, l + xradiusY];
                }
            }

            int yradiusX = ykernel.GetLength(0) / 2;
            int yradiusY = ykernel.GetLength(1) / 2;
            float yresultR = 0.0f;
            float yresultG = 0.0f;
            float yresultB = 0.0f;
            for (int l = -yradiusY; l <= yradiusY; l++)
            {
                for (int k = -yradiusX; k <= yradiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    yresultR += neighborColor.R * ykernel[k + yradiusX, l + yradiusY];
                    yresultG += neighborColor.G * ykernel[k + yradiusX, l + yradiusY];
                    yresultB += neighborColor.B * ykernel[k + yradiusX, l + yradiusY];
                }
            }

            float result = xresultR * xresultR + xresultG * xresultG + xresultB * xresultB;
            result += yresultR * yresultR + yresultG * yresultG + yresultB * yresultB;
            result = (float)Math.Sqrt(result);

            return Color.FromArgb(Clamp((int)result, 0, 255),
                                  Clamp((int)result, 0, 255),
                                  Clamp((int)result, 0, 255));
        }
    }

    class BorderSelectionFilter : MatrixFilter
    {
        protected int[,] X = null;
        protected int[,] Y = null;
        public BorderSelectionFilter()
        {
            X = new int[3, 3] { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
            Y = new int[3, 3] { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = 1;
            int radiusY = 1;
            float resultRX = 0; float resultGX = 0; float resultBX = 0;
            float resultRY = 0; float resultGY = 0; float resultBY = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color NeighbourColor = sourceImage.GetPixel(idX, idY);
                    resultRX += NeighbourColor.R * X[k + radiusX, l + radiusY];
                    resultGX += NeighbourColor.G * X[k + radiusX, l + radiusY];
                    resultBX += NeighbourColor.B * X[k + radiusX, l + radiusY];
                    resultRY += NeighbourColor.R * Y[k + radiusX, l + radiusY];
                    resultGY += NeighbourColor.G * Y[k + radiusX, l + radiusY];
                    resultBY += NeighbourColor.B * Y[k + radiusX, l + radiusY];
                }
            int resultR = Clamp((int)Math.Sqrt(Math.Pow(resultRX, 2.0) + Math.Pow(resultRY, 2.0)), 0, 255);
            int resultG = Clamp((int)Math.Sqrt(Math.Pow(resultGX, 2.0) + Math.Pow(resultGY, 2.0)), 0, 255);
            int resultB = Clamp((int)Math.Sqrt(Math.Pow(resultBX, 2.0) + Math.Pow(resultBY, 2.0)), 0, 255);
            return Color.FromArgb(Clamp(resultR, 0, 255), Clamp(resultG, 0, 255), Clamp(resultB, 0, 255));
        }

    }

    class StampingFilter : MatrixFilter
    {
        public StampingFilter()
        {
            createStampingKernel();
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            return Color.FromArgb(Clamp((int)resultR + 128, 0, 255), Clamp((int)resultG + 128, 0, 255), Clamp((int)resultB + 128, 0, 255));
        }
        public void createStampingKernel()
        {
            kernel = new float[3, 3];
            kernel[0, 0] = 0;
            kernel[0, 1] = 1;
            kernel[0, 2] = 0;
            kernel[1, 0] = 1;
            kernel[1, 1] = 0;
            kernel[1, 2] = -1;
            kernel[2, 0] = 0;
            kernel[2, 1] = -1;
            kernel[2, 2] = 0;
        }
    }
}



  