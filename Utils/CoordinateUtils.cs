using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoordenadasImagen.Utils
{
    /// <summary>
    /// Utilidades para el manejo de conversión de coordenadas entre la visualización en pantalla y la imagen original real.
    /// </summary>
    public static class CoordinateUtils
    {
        /// <summary>
        /// Obtiene la posición real en la imagen a partir de la coordenada del ratón en el control PictureBox con SizeMode = Zoom.
        /// </summary>
        public static Point GetImagePos(PictureBox pb, Image image, int mouseX, int mouseY)
        {
            if (image == null) return new Point(0, 0);

            float cW = pb.ClientRectangle.Width;
            float cH = pb.ClientRectangle.Height;
            float iW = image.Width;
            float iH = image.Height;

            float ratio = Math.Min(cW / iW, cH / iH);
            float padX = (cW - (iW * ratio)) / 2f;
            float padY = (cH - (iH * ratio)) / 2f;

            int oX = (int)Math.Max(0, Math.Min(iW, Math.Round((mouseX - padX) / ratio)));
            int oY = (int)Math.Max(0, Math.Min(iH, Math.Round((mouseY - padY) / ratio)));

            return new Point(oX, oY);
        }

        /// <summary>
        /// Obtiene la posición traducida en el control PictureBox a partir de las coordenadas reales de la imagen.
        /// </summary>
        public static Point GetControlPos(PictureBox pb, Image image, int imgX, int imgY)
        {
            if (image == null) return new Point(0, 0);

            float cW = pb.ClientRectangle.Width;
            float cH = pb.ClientRectangle.Height;
            float iW = image.Width;
            float iH = image.Height;

            float ratio = Math.Min(cW / iW, cH / iH);
            float padX = (cW - (iW * ratio)) / 2f;
            float padY = (cH - (iH * ratio)) / 2f;

            int cX = (int)((imgX * ratio) + padX);
            int cY = (int)((imgY * ratio) + padY);

            return new Point(cX, cY);
        }

        /// <summary>
        /// Calcula la proporción de escala actual para usarla en los tamaños de ancho y alto de recuadros.
        /// </summary>
        public static float GetZoomRatio(PictureBox pb, Image image)
        {
             if (image == null) return 1f;
             return Math.Min((float)pb.ClientRectangle.Width / image.Width, (float)pb.ClientRectangle.Height / image.Height);
        }
    }
}
