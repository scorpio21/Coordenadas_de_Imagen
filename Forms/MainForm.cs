using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CoordenadasImagen.Models;
using CoordenadasImagen.Services;
using CoordenadasImagen.Utils;

namespace CoordenadasImagen.Forms
{
    public partial class MainForm : Form
    {
        private string _loadedFilename = string.Empty;
        private Bitmap? _originalBmp = null;
        private string _lastCoordinates = string.Empty;

        private bool _isDrawing = false;
        private Point _rectStartImg = new Point(0, 0);
        private Point _currentMouseImg = new Point(0, 0);

        private List<Rectangle> _manualRects = new List<Rectangle>();
        private List<BoundsRecord> _autoRects = new List<BoundsRecord>();
        private readonly ImageScannerService _scannerService;

        public MainForm()
        {
            InitializeComponent();
            _scannerService = new ImageScannerService();
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Imagenes|*.jpg;*.jpeg;*.png;*.bmp";
                ofd.Title = "Selecciona la imagen a escanear";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (_originalBmp != null)
                    {
                        _originalBmp.Dispose();
                    }

                    _loadedFilename = ofd.FileName;
                    _originalBmp = new Bitmap(_loadedFilename);
                    pictureBox1.Image = _originalBmp;

                    _manualRects.Clear();
                    _autoRects.Clear();
                    UpdateCoordText();

                    string ext = Path.GetExtension(_loadedFilename).ToLower();
                    if (ext == ".bmp")
                    {
                        btnRunBMP.Enabled = true;
                        btnRunBMP.Visible = true;
                        btnRunPNG.Enabled = false;
                        btnRunPNG.Visible = false;

                        lblCols.Visible = true;
                        numCols.Visible = true;
                        lblRows.Visible = true;
                        numRows.Visible = true;

                        lblStatus.Text = "Archivo .bmp detectado. Usa la cuadricula manual BMP.";
                    }
                    else
                    {
                        btnRunBMP.Enabled = false;
                        btnRunBMP.Visible = false;
                        btnRunPNG.Enabled = true;
                        btnRunPNG.Visible = true;

                        lblCols.Visible = false;
                        numCols.Visible = false;
                        lblRows.Visible = false;
                        numRows.Visible = false;

                        lblStatus.Text = "Archivo .png / .jpg detectado. Usa el algoritmo automático inteligente.";
                    }
                }
            }
        }

        private void BtnRunPNG_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_loadedFilename)) return;

            btnRunPNG.Enabled = false;
            btnLoad.Enabled = false;
            btnExport.Enabled = false;
            lblStatus.Text = "Ejecutando escaneo automatico por proyeccion X/Y...";
            Application.DoEvents();

            try
            {
                var rectangles = _scannerService.GetRectanglesSmart(_loadedFilename);
                _autoRects.Clear();

                if (rectangles != null)
                {
                    _autoRects.AddRange(rectangles);
                }

                UpdateCoordText();
                lblStatus.Text = "Escaneo inteligente automatico completado con exito!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                lblStatus.Text = "Error ejecutando el análisis PNG.";
            }
            finally
            {
                btnRunPNG.Enabled = true;
                btnLoad.Enabled = true;
            }
        }

        private void BtnRunBMP_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_loadedFilename)) return;

            btnRunBMP.Enabled = false;
            btnLoad.Enabled = false;
            btnExport.Enabled = false;
            numCols.Enabled = false;
            numRows.Enabled = false;
            lblStatus.Text = "Ejecutando escaneo manual estricto por cuadricula BMP...";
            Application.DoEvents();

            try
            {
                int gridCols = (int)numCols.Value;
                int gridRows = (int)numRows.Value;

                var rectangles = _scannerService.GetRectanglesGrid(_loadedFilename, gridCols, gridRows);
                _autoRects.Clear();

                if (rectangles != null)
                {
                    _autoRects.AddRange(rectangles);
                }

                UpdateCoordText();
                lblStatus.Text = "Escaneo de cuadricula BMP completo (Revisa las cajas).";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                lblStatus.Text = "Error ejecutando el análisis BMP.";
            }
            finally
            {
                btnRunBMP.Enabled = true;
                btnLoad.Enabled = true;
                numCols.Enabled = true;
                numRows.Enabled = true;
            }
        }

        private void ChkManual_CheckedChanged(object sender, EventArgs e)
        {
            btnLimpiar.Visible = chkManual.Checked;
            if (chkManual.Checked)
            {
                lblStatus.Text = "Modo Libre: Pulsa y arrastra sobre la imagen ajustada a la pantalla para crear cajas.";
            }
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            _manualRects.Clear();
            UpdateCoordText();
            lblStatus.Text = "Cajas manuales borradas.";
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(_loadedFilename) || !chkManual.Checked) return;
            _isDrawing = true;
            _rectStartImg = CoordinateUtils.GetImagePos(pictureBox1, _originalBmp, e.X, e.Y);
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _currentMouseImg = CoordinateUtils.GetImagePos(pictureBox1, _originalBmp, e.X, e.Y);
                pictureBox1.Invalidate();
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _isDrawing = false;
                _currentMouseImg = CoordinateUtils.GetImagePos(pictureBox1, _originalBmp, e.X, e.Y);

                int x = Math.Min(_rectStartImg.X, _currentMouseImg.X);
                int y = Math.Min(_rectStartImg.Y, _currentMouseImg.Y);
                int w = Math.Abs(_currentMouseImg.X - _rectStartImg.X);
                int h = Math.Abs(_currentMouseImg.Y - _rectStartImg.Y);

                if (w > 2 && h > 2)
                {
                    _manualRects.Add(new Rectangle(x, y, w, h));
                    UpdateCoordText();
                }
                pictureBox1.Invalidate();
            }
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (string.IsNullOrEmpty(_loadedFilename) || _originalBmp == null) return;

            var g = e.Graphics;
            float ratio = CoordinateUtils.GetZoomRatio(pictureBox1, _originalBmp);

            using (Pen penAuto = new Pen(Color.LimeGreen, 2))
            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            using (SolidBrush brushYellow = new SolidBrush(Color.Yellow))
            {
                // Dibujar autodetectados
                foreach (var r in _autoRects)
                {
                    Point p = CoordinateUtils.GetControlPos(pictureBox1, _originalBmp, r.X, r.Y);
                    float drW = r.Width * ratio;
                    float drH = r.Height * ratio;

                    g.DrawRectangle(penAuto, p.X, p.Y, (int)drW, (int)drH);
                    float txtY = p.Y > 15 ? p.Y - 15 : p.Y;
                    g.DrawString(r.Label, font, brushYellow, p.X, txtY);
                }

                // Dibujar manuales
                using (Pen penManual = new Pen(Color.DeepPink, 2))
                using (SolidBrush brushWhite = new SolidBrush(Color.White))
                {
                    int i = 1;
                    foreach (var r in _manualRects)
                    {
                        Point p = CoordinateUtils.GetControlPos(pictureBox1, _originalBmp, r.X, r.Y);
                        float drW = r.Width * ratio;
                        float drH = r.Height * ratio;

                        g.DrawRectangle(penManual, p.X, p.Y, (int)drW, (int)drH);
                        float txtY = p.Y > 15 ? p.Y - 15 : p.Y;
                        g.DrawString($"M{i}", font, brushWhite, p.X, txtY);
                        i++;
                    }
                }

                // Dibujar arrastre actual
                if (_isDrawing)
                {
                    int x = Math.Min(_rectStartImg.X, _currentMouseImg.X);
                    int y = Math.Min(_rectStartImg.Y, _currentMouseImg.Y);
                    int w = Math.Abs(_currentMouseImg.X - _rectStartImg.X);
                    int h = Math.Abs(_currentMouseImg.Y - _rectStartImg.Y);

                    Point p = CoordinateUtils.GetControlPos(pictureBox1, _originalBmp, x, y);
                    float drW = w * ratio;
                    float drH = h * ratio;

                    using (Pen dashPen = new Pen(Color.Cyan, 2))
                    {
                        dashPen.DashStyle = DashStyle.Dash;
                        g.DrawRectangle(dashPen, p.X, p.Y, (int)drW, (int)drH);
                    }
                }
            }
        }

        private void UpdateCoordText()
        {
            StringBuilder sb = new StringBuilder();
            
            foreach (var r in _autoRects)
            {
                sb.AppendLine($"- **{r.Label}**: X={r.X}, Y={r.Y}, Ancho={r.Width}, Alto={r.Height}");
            }

            int i = 1;
            foreach (var r in _manualRects)
            {
                sb.AppendLine($"- **Manual {i}**: X={r.X}, Y={r.Y}, Ancho={r.Width}, Alto={r.Height}");
                i++;
            }

            _lastCoordinates = sb.ToString();
            txtCoords.Text = _lastCoordinates;

            btnExport.Enabled = _lastCoordinates.Length > 0;
            pictureBox1.Invalidate();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Markdown Files|*.md|Text Files|*.txt|All Files|*.*";
                sfd.FileName = "Coordenadas.md";
                sfd.Title = "Exportar coordenadas";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder mdContent = new StringBuilder();
                        mdContent.AppendLine("# Reporte de Coordenadas de Imagen");
                        mdContent.AppendLine();
                        mdContent.AppendLine($"Archivo analizado: `{Path.GetFileName(_loadedFilename)}`");
                        mdContent.AppendLine();
                        mdContent.AppendLine("## Resultados");
                        mdContent.AppendLine();
                        mdContent.Append(_lastCoordinates);

                        File.WriteAllText(sfd.FileName, mdContent.ToString(), Encoding.UTF8);
                        MessageBox.Show("Archivo guardado con exito.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al guardar archivo: {ex.Message}");
                    }
                }
            }
        }
    }
}
