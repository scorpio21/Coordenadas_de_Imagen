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
        // Tamaño mínimo (px) para considerar un componente como sprite válido
        private const int AnchoMinimo = 20;
        private const int AltoMinimo = 20;

        // Tolerancia de agrupación por fila (px): sprites cuyo centro Y difiera menos que esto se consideran en la misma fila
        private const int ToleranciaFila = 30;

        /// <summary>
        /// Construye la lista de colores de fondo muestreando los 4 bordes de la imagen.
        /// </summary>
        private static List<Color> DetectarColoresFondo(byte[] pixels, int width, int height, int stride, int bpp)
        {
            var bgColors = new List<Color>();
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
            return bgColors;
        }

        /// <summary>
        /// Determina si un píxel pertenece al fondo según la distancia en color con las muestras de borde.
        /// </summary>
        private static bool EsFondo(int x, int y, byte[] pixels, int stride, int bpp, List<Color> bgColors, int umbral = 150)
        {
            int off = (y * stride) + (x * bpp);
            byte b = pixels[off], gc = pixels[off + 1], r = pixels[off + 2];
            foreach (var bg in bgColors)
            {
                int dr = r - bg.R, dg = gc - bg.G, db = b - bg.B;
                if (dr * dr + dg * dg + db * db < umbral) return true;
            }
            return false;
        }

        /// <summary>
        /// Algoritmo Inteligente basado en Componentes Conectados con BFS.
        /// 1. Detecta píxeles de primer plano.
        /// 2. Dilata 1 px para cerrar huecos finos (bastones, armas transparentes).
        /// 3. Etiqueta componentes con BFS 8-conectado sobre la máscara dilatada.
        /// 4. Calcula bounding boxes sobre la máscara ORIGINAL (sin dilatar).
        /// 5. Filtra componentes pequeños (ruido).
        /// 6. Agrupa por fila (tolerancia Y) y etiqueta como F1 C1, F2 C1, etc.
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

                var bgColors = DetectarColoresFondo(pixels, width, height, stride, bpp);

                // Paso 1: Construir máscara de primer plano
                bool[,] fg = new bool[width, height];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        fg[x, y] = !EsFondo(x, y, pixels, stride, bpp, bgColors);

                // Paso 2: Dilatación de 1 px (cierra huecos de armas finas de 1-2px)
                bool[,] fgDil = new bool[width, height];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        if (!fg[x, y]) continue;
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = x + dx, ny = y + dy;
                                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                                    fgDil[nx, ny] = true;
                            }
                    }

                // Paso 3: BFS 8-conectado sobre la máscara dilatada para etiquetar componentes
                int[] compLabel = new int[width * height]; // 0 = no visitado
                var compBboxes = new Dictionary<int, (int minX, int minY, int maxX, int maxY)>();
                int nextLabel = 1;

                int[] dxs = { -1, 0, 1, -1, 1, -1, 0, 1 };
                int[] dys = { -1, -1, -1, 0, 0, 1, 1, 1 };

                for (int sy = 0; sy < height; sy++)
                {
                    for (int sx = 0; sx < width; sx++)
                    {
                        int idx = sy * width + sx;
                        if (!fgDil[sx, sy] || compLabel[idx] != 0) continue;

                        int lbl = nextLabel++;
                        var bbox = (minX: width, minY: height, maxX: 0, maxY: 0);

                        var queue = new Queue<(int x, int y)>();
                        queue.Enqueue((sx, sy));
                        compLabel[idx] = lbl;

                        while (queue.Count > 0)
                        {
                            var (cx, cy) = queue.Dequeue();

                            // Solo actualizar bbox con píxeles de la máscara ORIGINAL
                            if (fg[cx, cy])
                            {
                                if (cx < bbox.minX) bbox.minX = cx;
                                if (cx > bbox.maxX) bbox.maxX = cx;
                                if (cy < bbox.minY) bbox.minY = cy;
                                if (cy > bbox.maxY) bbox.maxY = cy;
                            }

                            for (int d = 0; d < 8; d++)
                            {
                                int nx = cx + dxs[d], ny = cy + dys[d];
                                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                                int nidx = ny * width + nx;
                                if (!fgDil[nx, ny] || compLabel[nidx] != 0) continue;
                                compLabel[nidx] = lbl;
                                queue.Enqueue((nx, ny));
                            }
                        }

                        if (bbox.maxX >= bbox.minX && bbox.maxY >= bbox.minY)
                            compBboxes[lbl] = bbox;
                    }
                }

                // Paso 4: Filtrar componentes demasiado pequeños (ruido)
                var sprites = new List<Rectangle>();
                foreach (var kv in compBboxes)
                {
                    var b = kv.Value;
                    int w = b.maxX - b.minX + 1;
                    int h = b.maxY - b.minY + 1;
                    if (w >= AnchoMinimo && h >= AltoMinimo)
                        sprites.Add(new Rectangle(b.minX, b.minY, w, h));
                }

                // Paso 5: Ordenar por centro Y → agrupar filas → ordenar por X → etiquetar
                sprites.Sort((a, b) => (a.Y + a.Height / 2).CompareTo(b.Y + b.Height / 2));

                var results = new List<BoundsRecord>();
                int rowIdx = 1;
                int i = 0;
                while (i < sprites.Count)
                {
                    // Recopilar todos los sprites de la misma fila (cuyo centro Y esté dentro de tolerancia)
                    int centroYRef = sprites[i].Y + sprites[i].Height / 2;
                    var fila = new List<Rectangle>();
                    while (i < sprites.Count)
                    {
                        int centroY = sprites[i].Y + sprites[i].Height / 2;
                        if (Math.Abs(centroY - centroYRef) > ToleranciaFila) break;
                        fila.Add(sprites[i]);
                        i++;
                    }

                    // Ordenar por X dentro de la fila
                    fila.Sort((a, b) => a.X.CompareTo(b.X));

                    int colIdx = 1;
                    foreach (var r in fila)
                    {
                        results.Add(new BoundsRecord
                        {
                            X = r.X,
                            Y = r.Y,
                            Width = r.Width,
                            Height = r.Height,
                            Label = string.Format("F{0} C{1}", rowIdx, colIdx++)
                        });
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

                var bgColors = DetectarColoresFondo(pixels, width, height, stride, bpp);

                double cellW = width / (double)cols;
                double cellH = height / (double)rows;
                var results = new List<BoundsRecord>();

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
                            for (int x = startX; x < endX; x++)
                                if (!EsFondo(x, y, pixels, stride, bpp, bgColors, 800))
                                {
                                    if (x < minX) minX = x;
                                    if (x > maxX) maxX = x;
                                    if (y < minY) minY = y;
                                    if (y > maxY) maxY = y;
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
