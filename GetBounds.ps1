$code = @"
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

public class BoundsFinderForm : Form
{
    private PictureBox pb;
    private Button btnLoad;
    private Bitmap currentBmp;

    public BoundsFinderForm()
    {
        this.Text = "Visor de Límites de Imagen";
        this.Size = new Size(1024, 768);
        this.WindowState = FormWindowState.Maximized;

        Panel panelTop = new Panel();
        panelTop.Dock = DockStyle.Top;
        panelTop.Height = 50;
        this.Controls.Add(panelTop);

        btnLoad = new Button();
        btnLoad.Text = "Cargar Imagen";
        btnLoad.Location = new Point(10, 10);
        btnLoad.AutoSize = true;
        btnLoad.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btnLoad.Click += BtnLoad_Click;
        panelTop.Controls.Add(btnLoad);

        pb = new PictureBox();
        pb.Dock = DockStyle.Fill;
        pb.SizeMode = PictureBoxSizeMode.Zoom;
        pb.BackColor = Color.LightGray;
        this.Controls.Add(pb);
        pb.BringToFront();
    }

    private void BtnLoad_Click(object sender, EventArgs e)
    {
        using (OpenFileDialog ofd = new OpenFileDialog())
        {
            ofd.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp";
            ofd.Title = "Seleccionar imagen a analizar";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ProcessImage(ofd.FileName);
            }
        }
    }

    public void ProcessImage(string filename)
    {
        if (currentBmp != null) currentBmp.Dispose();

        Bitmap original = new Bitmap(filename);
        currentBmp = new Bitmap(original);
        
        int cols = 5;
        int rows = 4;
        double cellW = original.Width / (double)cols;
        double cellH = original.Height / (double)rows;

        List<Color> bgColors = new List<Color>();
        for (int x = 0; x < original.Width; x += 10)
        {
            bgColors.Add(original.GetPixel(x, 0));
            bgColors.Add(original.GetPixel(x, original.Height - 1));
        }
        for (int y = 0; y < original.Height; y += 10)
        {
            bgColors.Add(original.GetPixel(0, y));
            bgColors.Add(original.GetPixel(original.Width - 1, y));
        }

        using (Graphics g = Graphics.FromImage(currentBmp))
        {
            // Dibujar cuadrícula de fondo referencial (opcional)
            using (Pen gridPen = new Pen(Color.FromArgb(100, Color.Blue), 1))
            {
                gridPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                for (int c = 1; c < cols; c++) g.DrawLine(gridPen, (int)(c * cellW), 0, (int)(c * cellW), original.Height);
                for (int r = 1; r < rows; r++) g.DrawLine(gridPen, 0, (int)(r * cellH), original.Width, (int)(r * cellH));
            }

            int penThickness = Math.Max(2, original.Width / 400);
            using (Pen pen = new Pen(Color.Red, penThickness))
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (r == 3 && c == 4) continue;

                        int startX = (int)(c * cellW);
                        int startY = (int)(r * cellH);
                        int endX = (int)((c + 1) * cellW);
                        int endY = (int)((r + 1) * cellH);

                        int minX = endX, minY = endY, maxX = startX, maxY = startY;

                        for (int y = startY; y < endY; y++)
                        {
                            for (int x = startX; x < endX; x++)
                            {
                                Color p = original.GetPixel(x, y);

                                bool isBg = false;
                                foreach(var bg in bgColors)
                                {
                                    int dr = p.R - bg.R;
                                    int dg = p.G - bg.G;
                                    int db = p.B - bg.B;
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
                            g.DrawRectangle(pen, minX, minY, maxX - minX, maxY - minY);
                            
                            // Etiqueta para saber qué fila y columna es (opcional)
                            string label = $"F{r+1} C{c+1}";
                            Font font = new Font("Arial", penThickness * 4, FontStyle.Bold);
                            g.DrawString(label, font, Brushes.Yellow, minX, minY > 20 ? minY - (penThickness*5) : minY);
                        }
                    }
                }
            }
        }

        pb.Image = currentBmp;
        original.Dispose();
    }
}
"@

Add-Type -TypeDefinition $code -ReferencedAssemblies "System.Drawing", "System.Windows.Forms"

# Habilitar estilos visuales nativos de Windows
[System.Windows.Forms.Application]::EnableVisualStyles()
$form = New-Object BoundsFinderForm
[System.Windows.Forms.Application]::Run($form)
