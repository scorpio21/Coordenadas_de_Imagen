using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using CoordenadasImagen.Models;

namespace CoordenadasImagen.Services
{
    /// <summary>
    /// Servicio modular encargado de analizar imágenes para encontrar límites espaciales (bounds) en hojas de sprites.
    /// </summary>
    public class ImageScannerService
    {
        /// <summary>
        /// Algoritmo Inteligente por proyección en X e Y (Ideal para .png y .jpg).
        /// La proyección X se realiza de forma LOCAL por cada franja Y detectada,
        /// permitiendo segmentar correctamente sprites en filas con posiciones X distintas.
        /// </summary>
        public BoundsRecord[] GetRectanglesSmart(string imagePath)
        {
            using (Bitmap sourceBmp = new Bitmap(imagePath))
            using (Bitmap originalBmp = new Bitmap(sourceBmp.Width, sourceBmp.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(originalBmp))
                    g.DrawImage(sourceBmp, 0, 0);

                Rectangle rectLock = new Rectangle(0, 0, originalBmp.Width, originalBmp.Height);
                BitmapData bmpData = originalBmp.LockBits(rectLock, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                int stride = bmpData.Stride;
                int bpp = 4;
                byte[] pixels = new byte[Math.Abs(stride) * originalBmp.Height];
                Marshal.Copy(bmpData.Scan0, pixels, 0, pixels.Length);
                originalBmp.UnlockBits(bmpData);

                int width = originalBmp.Width;
                int height = originalBmp.Height;

                // Muestras de color de fondo en los 4 bordes
                List<Color> bgColors = new List<Color>();
                for (int x = 0; x < width; x += 10)
                {
                    int p1 = x * bpp;
                    bgColors.Add(Color.FromArgb(pixels[p1 + 2], pixels[p1 + 1], pixels[p1]));
                    int p2 = ((height - 1) * stride) + (x * bpp);
                    bgColors.Add(Color.FromArgb(pixels[p2 + 2], pixels[p2 + 1], pixels[p2]));
                }
                for (int y = 0; y < height; y += 10)
                {
                    int p1 = y * stride;
                    bgColors.Add(Color.FromArgb(pixels[p1 + 2], pixels[p1 + 1], pixels[p1]));
                    int p2 = (y * stride) + ((width - 1) * bpp);
                    bgColors.Add(Color.FromArgb(pixels[p2 + 2], pixels[p2 + 1], pixels[p2]));
                }

                // Determina si un pixel es de fondo según el umbral de color
                bool EsFondo(int x, int y)
                {
                    int off = (y * stride) + (x * bpp);
                    byte b = pixels[off], gc = pixels[off + 1], r = pixels[off + 2];
                    foreach (var bg in bgColors)
                    {
                        int dr = r - bg.R, dg = gc - bg.G, db = b - bg.B;
                        if (dr * dr + dg * dg + db * db < 150) return true;
                    }
                    return false;
                }

                // Paso 1: Proyección global en Y → detección de franjas horizontales
                bool[] rowHasFg = new bool[height];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        if (!EsFondo(x, y)) { rowHasFg[y] = true; break; }

                List<Point> yRanges = new List<Point>();
                int curStartY = -1;
                for (int y = 0; y < height; y++)
                {
                    if (rowHasFg[y]) { if (curStartY == -1) curStartY = y; }
                    else { if (curStartY != -1) { yRanges.Add(new Point(curStartY, y - curStartY)); curStartY = -1; } }
                }
                if (curStartY != -1) yRanges.Add(new Point(curStartY, height - curStartY));

                List<BoundsRecord> results = new List<BoundsRecord>();
                int rowIdx = 1;

                // Paso 2: Por cada franja Y, proyección LOCAL en X → columnas dentro de la franja
                foreach (var yR in yRanges)
                {
                    int yStart = yR.X;
                    int yEnd = yR.X + yR.Y;

                    bool[] colHasFgLocal = new bool[width];
                    for (int y = yStart; y < yEnd; y++)
                        for (int x = 0; x < width; x++)
                            if (!EsFondo(x, y)) colHasFgLocal[x] = true;

                    List<Point> xRanges = new List<Point>();
                    int curStartX = -1;
                    for (int x = 0; x < width; x++)
                    {
                        if (colHasFgLocal[x]) { if (curStartX == -1) curStartX = x; }
                        else { if (curStartX != -1) { xRanges.Add(new Point(curStartX, x - curStartX)); curStartX = -1; } }
                    }
                    if (curStartX != -1) xRanges.Add(new Point(curStartX, width - curStartX));

                    int colIdx = 1;
                    foreach (var xR in xRanges)
                    {
                        int xEnd = xR.X + xR.Y;
                        int tMinX = xEnd, tMinY = yEnd, tMaxX = xR.X, tMaxY = yStart;
                        bool hasFg = false;

                        for (int y = yStart; y < yEnd; y++)
                            for (int x = xR.X; x < xEnd; x++)
                                if (!EsFondo(x, y))
                                {
                                    hasFg = true;
                                    if (x < tMinX) tMinX = x;
                                    if (x > tMaxX) tMaxX = x;
                                    if (y < tMinY) tMinY = y;
                                    if (y > tMaxY) tMaxY = y;
                                }

                        if (hasFg)
                        {
                            results.Add(new BoundsRecord
                            {
                                X = tMinX,
                                Y = tMinY,
                                Width = tMaxX - tMinX + 1,
                                Height = tMaxY - tMinY + 1,
                                Label = string.Format("F{0} C{1}", rowIdx, colIdx)
                            });
                            colIdx++;
                        }
                    }
                    rowIdx++;
                }

                return results.ToArray();
            }
        }

        /// <summary>
        /// Algoritmo estricto por cuadrícula (Ideal para hojas de sprites sin separación real, típicamente .bmp)
        /// </summary>
        public BoundsRecord[] GetRectanglesGrid(string imagePath, int cols, int rows)
        {
            using (Bitmap sourceBmp = new Bitmap(imagePath))
            using (Bitmap originalBmp = new Bitmap(sourceBmp.Width, sourceBmp.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(originalBmp))
                    g.DrawImage(sourceBmp, 0, 0);

                Rectangle rectLock = new Rectangle(0, 0, originalBmp.Width, originalBmp.Height);
                BitmapData bmpData = originalBmp.LockBits(rectLock, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                int stride = bmpData.Stride;
                int bpp = 4;
                byte[] pixels = new byte[Math.Abs(stride) * originalBmp.Height];
                Marshal.Copy(bmpData.Scan0, pixels, 0, pixels.Length);
                originalBmp.UnlockBits(bmpData);

                int width = originalBmp.Width;
                int height = originalBmp.Height;

                List<Color> bgColors = new List<Color>();
                for (int x = 0; x < width; x += 10)
                {
                    int p1 = x * bpp;
                    bgColors.Add(Color.FromArgb(pixels[p1 + 2], pixels[p1 + 1], pixels[p1]));
                    int p2 = ((height - 1) * stride) + (x * bpp);
                    bgColors.Add(Color.FromArgb(pixels[p2 + 2], pixels[p2 + 1], pixels[p2]));
                }
                for (int y = 0; y < height; y += 10)
                {
                    int p1 = y * stride;
                    bgColors.Add(Color.FromArgb(pixels[p1 + 2], pixels[p1 + 1], pixels[p1]));
                    int p2 = (y * stride) + ((width - 1) * bpp);
                    bgColors.Add(Color.FromArgb(pixels[p2 + 2], pixels[p2 + 1], pixels[p2]));
                }

                double cellW = width / (double)cols;
                double cellH = height / (double)rows;
                List<BoundsRecord> results = new List<BoundsRecord>();

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        int startX = (int)(c * cellW);
                        int startY = (int)(r * cellH);
                        int endX = Math.Min((int)((c + 1) * cellW), width);
                        int endY = Math.Min((int)((r + 1) * cellH), height);

                        int minX = endX, minY = endY, maxX = startX, maxY = startY;

                        for (int y = startY; y < endY; y++)
                        {
                            int rowOffset = y * stride;
                            for (int x = startX; x < endX; x++)
                            {
                                int off = rowOffset + (x * bpp);
                                byte b = pixels[off], gc = pixels[off + 1], red = pixels[off + 2];
                                bool isBg = false;
                                foreach (var bg in bgColors)
                                {
                                    int dr = red - bg.R, dg = gc - bg.G, db = b - bg.B;
                                    if (dr * dr + dg * dg + db * db < 800) { isBg = true; break; }
                                }
                                if (!isBg)
                                {
                                    if (x < minX) minX = x;
                                    if (x > maxX) maxX = x;
                                    if (y < minY) minY = y;
                                    if (y > maxY) maxY = y;
                                }
                            }
                        }

                        if (minX <= maxX && minY <= maxY)
                        {
                            results.Add(new BoundsRecord
                            {
                                X = minX,
                                Y = minY,
                                Width = maxX - minX + 1,
                                Height = maxY - minY + 1,
                                Label = string.Format("F{0} C{1}", r + 1, c + 1)
                            });
                        }
                    }
                }

                return results.ToArray();
            }
        }
    }
}
