param(
    [Parameter(Mandatory=$true)]
    [string]$ImagePath,
    
    [Parameter(Mandatory=$true)]
    [int]$Cols,

    [Parameter(Mandatory=$true)]
    [int]$Rows
)

$code = @"
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class BoundsRecordBMP
{
    public int X;
    public int Y;
    public int Width;
    public int Height;
    public string Label;
}

public class BoundsFinderBMP
{
    public static BoundsRecordBMP[] GetRectangles(string imagePath, int cols, int rows)
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

            double cellW = width / (double)cols;
            double cellH = height / (double)rows;
            List<BoundsRecordBMP> results = new List<BoundsRecordBMP>();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int startX = (int)(c * cellW);
                    int startY = (int)(r * cellH);
                    int endX = (int)((c + 1) * cellW);
                    int endY = (int)((r + 1) * cellH);

                    if(endX > width) endX = width;
                    if(endY > height) endY = height;

                    int minX = endX, minY = endY, maxX = startX, maxY = startY;

                    for (int y = startY; y < endY; y++)
                    {
                        int rowOffset = y * bmpData.Stride;
                        for (int x = startX; x < endX; x++)
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
                                if (dr*dr + dg*dg + db*db < 800) 
                                {
                                    isBg = true;
                                    break;
                                }
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
                        BoundsRecordBMP rec = new BoundsRecordBMP();
                        rec.X = minX;
                        rec.Y = minY;
                        rec.Width = maxX - minX + 1;
                        rec.Height = maxY - minY + 1;
                        rec.Label = string.Format("F{0} C{1}", r+1, c+1);
                        results.Add(rec);
                    }
                }
            }

            return results.ToArray();
        }
    }
}
"@

if (-not ("BoundsFinderBMP" -as [type])) {
    Add-Type -TypeDefinition $code -ReferencedAssemblies "System.Drawing"
}

[BoundsFinderBMP]::GetRectangles($ImagePath, $Cols, $Rows)
