namespace CoordenadasImagen.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (_originalBmp != null)
            {
                _originalBmp.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnRunPNG = new System.Windows.Forms.Button();
            this.btnRunBMP = new System.Windows.Forms.Button();
            this.lblCols = new System.Windows.Forms.Label();
            this.numCols = new System.Windows.Forms.NumericUpDown();
            this.lblRows = new System.Windows.Forms.Label();
            this.numRows = new System.Windows.Forms.NumericUpDown();
            this.chkManual = new System.Windows.Forms.CheckBox();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panelRight = new System.Windows.Forms.Panel();
            this.lblRight = new System.Windows.Forms.Label();
            this.btnExport = new System.Windows.Forms.Button();
            this.txtCoords = new System.Windows.Forms.TextBox();
            
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCols)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRows)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panelRight.SuspendLayout();
            this.SuspendLayout();
            
            // panelTop
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 110;
            this.panelTop.Controls.Add(this.btnLoad);
            this.panelTop.Controls.Add(this.btnRunPNG);
            this.panelTop.Controls.Add(this.btnRunBMP);
            this.panelTop.Controls.Add(this.lblCols);
            this.panelTop.Controls.Add(this.numCols);
            this.panelTop.Controls.Add(this.lblRows);
            this.panelTop.Controls.Add(this.numRows);
            this.panelTop.Controls.Add(this.chkManual);
            this.panelTop.Controls.Add(this.btnLimpiar);
            this.panelTop.Controls.Add(this.lblStatus);
            
            // btnLoad
            this.btnLoad.Text = "1. Cargar Imagen";
            this.btnLoad.Location = new System.Drawing.Point(10, 10);
            this.btnLoad.AutoSize = true;
            this.btnLoad.Height = 40;
            this.btnLoad.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnLoad.Click += new System.EventHandler(this.BtnLoad_Click);
            
            // btnRunPNG
            this.btnRunPNG.Text = "2. Escaneo Inteligente Automatico (.png / .jpg)";
            this.btnRunPNG.Location = new System.Drawing.Point(150, 10);
            this.btnRunPNG.AutoSize = true;
            this.btnRunPNG.Height = 40;
            this.btnRunPNG.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRunPNG.Enabled = false;
            this.btnRunPNG.Visible = false;
            this.btnRunPNG.Click += new System.EventHandler(this.BtnRunPNG_Click);
            
            // btnRunBMP
            this.btnRunBMP.Text = "2. Escaneo Estricto por Cuadricula (.bmp)";
            this.btnRunBMP.Location = new System.Drawing.Point(150, 10);
            this.btnRunBMP.AutoSize = true;
            this.btnRunBMP.Height = 40;
            this.btnRunBMP.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRunBMP.Enabled = false;
            this.btnRunBMP.Visible = false;
            this.btnRunBMP.Click += new System.EventHandler(this.BtnRunBMP_Click);
            
            // lblCols
            this.lblCols.Text = "Cols:";
            this.lblCols.AutoSize = true;
            this.lblCols.Location = new System.Drawing.Point(450, 22);
            this.lblCols.Visible = false;
            
            // numCols
            this.numCols.Location = new System.Drawing.Point(485, 20);
            this.numCols.Width = 50;
            this.numCols.Value = 4;
            this.numCols.Minimum = 1;
            this.numCols.Maximum = 100;
            this.numCols.Visible = false;
            
            // lblRows
            this.lblRows.Text = "Filas:";
            this.lblRows.AutoSize = true;
            this.lblRows.Location = new System.Drawing.Point(540, 22);
            this.lblRows.Visible = false;
            
            // numRows
            this.numRows.Location = new System.Drawing.Point(575, 20);
            this.numRows.Width = 50;
            this.numRows.Value = 4;
            this.numRows.Minimum = 1;
            this.numRows.Maximum = 100;
            this.numRows.Visible = false;
            
            // chkManual
            this.chkManual.Text = "Modo Marcacion Libre (Raton)";
            this.chkManual.Location = new System.Drawing.Point(10, 60);
            this.chkManual.AutoSize = true;
            this.chkManual.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.chkManual.ForeColor = System.Drawing.Color.DarkRed;
            this.chkManual.CheckedChanged += new System.EventHandler(this.ChkManual_CheckedChanged);
            
            // btnLimpiar
            this.btnLimpiar.Text = "Borrar Cajas Manuales";
            this.btnLimpiar.Location = new System.Drawing.Point(240, 58);
            this.btnLimpiar.AutoSize = true;
            this.btnLimpiar.Height = 30;
            this.btnLimpiar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.btnLimpiar.Visible = false;
            this.btnLimpiar.Click += new System.EventHandler(this.BtnLimpiar_Click);
            
            // lblStatus
            this.lblStatus.Location = new System.Drawing.Point(400, 65);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Text = "Haz clic en '1. Cargar Imagen'.";
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Italic);
            this.lblStatus.ForeColor = System.Drawing.Color.Blue;
            
            // splitContainer1
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.SplitterDistance = 800;
            
            // pictureBox1
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.BackColor = System.Drawing.Color.LightGray;
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBox1_MouseDown);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBox1_MouseMove);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBox1_MouseUp);
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBox1_Paint);
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
            
            // panelRight
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Padding = new System.Windows.Forms.Padding(10);
            this.panelRight.Controls.Add(this.txtCoords);
            this.panelRight.Controls.Add(this.btnExport);
            this.panelRight.Controls.Add(this.lblRight);
            this.splitContainer1.Panel2.Controls.Add(this.panelRight);
            
            // lblRight
            this.lblRight.Text = "Resultados (Coordenadas):";
            this.lblRight.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRight.Height = 25;
            this.lblRight.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            
            // btnExport
            this.btnExport.Text = "Exportar a .md";
            this.btnExport.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnExport.Height = 40;
            this.btnExport.Enabled = false;
            this.btnExport.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnExport.Click += new System.EventHandler(this.BtnExport_Click);
            
            // txtCoords
            this.txtCoords.Multiline = true;
            this.txtCoords.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCoords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCoords.ReadOnly = true;
            this.txtCoords.Font = new System.Drawing.Font("Consolas", 9F);
            
            // MainForm
            this.ClientSize = new System.Drawing.Size(1250, 750);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panelTop);
            this.Text = "Visor de Limites y Coordenadas .NET 8";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCols)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRows)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panelRight.ResumeLayout(false);
            this.panelRight.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnRunPNG;
        private System.Windows.Forms.Button btnRunBMP;
        private System.Windows.Forms.Label lblCols;
        private System.Windows.Forms.NumericUpDown numCols;
        private System.Windows.Forms.Label lblRows;
        private System.Windows.Forms.NumericUpDown numRows;
        private System.Windows.Forms.CheckBox chkManual;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Label lblRight;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.TextBox txtCoords;
    }
}
