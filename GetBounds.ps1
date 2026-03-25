param(
    [Parameter(Mandatory=$true)]
    [string]$ImagePath
)

$code = @"
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class BoundsRecord
{
    public int X;
    public int Y;
    public int Width;
    public int Height;
    public string Label;
}

public class BoundsFinder
{
    public static BoundsRecord[] GetRectangles(string imagePath)
    {
        using (Bitmap sourceBmp = new Bitmap(imagePath))
        using (Bitmap originalBmp = new Bitmap(sourceBmp.Width, sourceBmp.Height, PixelFormat.Format32bppArgb))
        {
            using (Graphics g = Graphics.FromImage(originalBmp))
            {
                g.DrawImage(sourceBmp, 0, 0);
            }

            Rectangle rectLock = new Rectangle(0, 0, originalBmp.Width, originalBmp.Height);
            BitmapData bmpData = originalBmp.LockBits(rectLock, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int bytesPerPixel = 4;
            int byteCount = Math.Abs(bmpData.Stride) * originalBmp.Height;
            byte[] pixels = new byte[byteCount];
            Marshal.Copy(bmpData.Scan0, pixels, 0, byteCount);
            originalBmp.UnlockBits(bmpData);

            int width = originalBmp.Width;
            int height = originalBmp.Height;

            List<Color> bgColors = new List<Color>();
            for (int x = 0; x < width; x += 10)
            {
                int p1 = (x * bytesPerPixel);
                bgColors.Add(Color.FromArgb(pixels[p1 + 2], pixels[p1 + 1], pixels[p1]));
                int p2 = ((height - 1) * bmpData.Stride) + (x * bytesPerPixel);
                bgColors.Add(Color.FromArgb(pixels[p2 + 2], pixels[p2 + 1], pixels[p2]));
            }
            for (int y = 0; y < height; y += 10)
            {
                int p1 = (y * bmpData.Stride);
                bgColors.Add(Color.FromArgb(pixels[p1 + 2], pixels[p1 + 1], pixels[p1]));
                int p2 = (y * bmpData.Stride) + ((width - 1) * bytesPerPixel);
                bgColors.Add(Color.FromArgb(pixels[p2 + 2], pixels[p2 + 1], pixels[p2]));
            }

            bool[] colHasFg = new bool[width];
            bool[] rowHasFg = new bool[height];

            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * bmpData.Stride;
                for (int x = 0; x < width; x++)
                {
                    int pixelOffset = rowOffset + (x * bytesPerPixel);
                    byte b = pixels[pixelOffset];
                    byte gCol = pixels[pixelOffset + 1];
                    byte red = pixels[pixelOffset + 2];

                    bool isBg = false;
                    foreach(var bg in bgColors)
                    {
                        int dr = red - bg.R;
                        int dg = gCol - bg.G;
                        int db = b - bg.B;
                        // Umbral muy estricto para no comerse las sombras de los dibujos
                        if (dr*dr + dg*dg + db*db < 150) 
                        {
                            isBg = true;
                            break;
                        }
                    }
                    if (!isBg)
                    {
                        colHasFg[x] = true;
                        rowHasFg[y] = true;
                    }
                }
            }

            List<Point> xRanges = new List<Point>();
            int curStartX = -1;
            for(int x = 0; x < width; x++) {
                if(colHasFg[x]) {
                    if(curStartX == -1) curStartX = x;
                } else {
                    if(curStartX != -1) {
                        xRanges.Add(new Point(curStartX, x - curStartX));
                        curStartX = -1;
                    }
                }
            }
            if(curStartX != -1) xRanges.Add(new Point(curStartX, width - curStartX));

            List<Point> yRanges = new List<Point>();
            int curStartY = -1;
            for(int y = 0; y < height; y++) {
                if(rowHasFg[y]) {
                    if(curStartY == -1) curStartY = y;
                } else {
                    if(curStartY != -1) {
                        yRanges.Add(new Point(curStartY, y - curStartY));
                        curStartY = -1;
                    }
                }
            }
            if(curStartY != -1) yRanges.Add(new Point(curStartY, height - curStartY));

            List<BoundsRecord> results = new List<BoundsRecord>();
            int rowIdx = 1;

            foreach(var yR in yRanges) {
                int colIdx = 1;
                foreach(var xR in xRanges) {
                    bool hasFg = false;
                    int endX = xR.X + xR.Y;
                    int endY = yR.X + yR.Y;
                    int tMinX = endX, tMinY = endY, tMaxX = xR.X, tMaxY = yR.X;

                    for(int y = yR.X; y < endY; y++) {
                        int rowOffset = y * bmpData.Stride;
                        for(int x = xR.X; x < endX; x++) {
                            int pixelOffset = rowOffset + (x * bytesPerPixel);
                            byte b = pixels[pixelOffset];
                            byte gCol = pixels[pixelOffset + 1];
                            byte red = pixels[pixelOffset + 2];
                            bool isBg = false;
                            foreach(var bg in bgColors) {
                                int dr = red - bg.R; int dg = gCol - bg.G; int db = b - bg.B;
                                if(dr*dr + dg*dg + db*db < 150) { isBg = true; break; }
                            }
                            if(!isBg) {
                                hasFg = true;
                                if(x < tMinX) tMinX = x;
                                if(x > tMaxX) tMaxX = x;
                                if(y < tMinY) tMinY = y;
                                if(y > tMaxY) tMaxY = y;
                            }
                        }
                    }
                    if(hasFg) {
                        BoundsRecord rec = new BoundsRecord();
                        rec.X = tMinX;
                        rec.Y = tMinY;
                        rec.Width = tMaxX - tMinX + 1;
                        rec.Height = tMaxY - tMinY + 1;
                        rec.Label = string.Format("F{0} C{1}", rowIdx, colIdx);
                        results.Add(rec);
                        colIdx++;
                    }
                }
                rowIdx++;
            }

            return results.ToArray();
        }
    }
}
"@

if (-not ("BoundsFinder" -as [type])) {
    Add-Type -TypeDefinition $code -ReferencedAssemblies "System.Drawing"
}

[BoundsFinder]::GetRectangles($ImagePath)
