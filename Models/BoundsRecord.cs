using System;

namespace CoordenadasImagen.Models
{
    /// <summary>
    /// Estructura de datos para representar una caja de coordenadas encontrada en la imagen.
    /// Unifica Properties tanto para escaneo inteligente (PNG) como por cuadrícula (BMP).
    /// </summary>
    public class BoundsRecord
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}
