# Analizador de Sprite Sheets (PowerShell)

Una potente herramienta con interfaz gráfica (Windows Forms) construida íntegramente en PowerShell apoyada por rutinas compiladas en C# nativo para detectar dinámicamente personajes, objetos y efectos en hojas de recortes (Sprite Sheets).

## Novedades de la última versión:
- **Escaneo Inteligente (Proyección X/Y)**: Para `.png` y `.jpg`. Detecta automáticamente bloques separados por canal alfa o negro. Inmune a las sombras oscuras que antes fracturaban dibujos como dragones o armaduras.
- **Modo Cuadrícula (Grid Slicer)**: Activo para `.bmp`. Corta exactamente el número de Filas y Columnas especificadas por el usuario (Ideal para particulas simétricas).
- **Modo Marcación Manual**: Dibuja tus propias cajas superpuestas arrastrando el ratón libremente como en Photoshop. Adapta inteligentemente las dimensiones usando un conversor visual de proporción `Zoom`.
- **Exportación de Registros**: Guarda todas tus coordenadas detectadas (tanto Automáticas como Manuales) limpiamente en un archivo Markdown (`Coordenadas.md`).
- **Seguridad de Archivos**: Reprogramado internamente para cargar BMP de 8-bits y GIFs sobre un lienzo de 32-bits virtuales eliminando para siempre el temido *Índice fuera de límites* (OutOfRangeMemory).

## Estructura de código
- `Formulario.ps1`: Interfaz visual y eventos del ratón.
- `GetBounds.ps1`: Motor en C# de Proyección Auto-inteligente X/Y.
- `GetBoundsBMP.ps1`: Motor en C# de escaneo algorítmico por celdas puras.

## Uso:
Para lanzar la utilidad, ejecuta desde terminal o haz click derecho y "Ejecutar con PowerShell":
```powershell
powershell.exe -WindowStyle Hidden -File .\Formulario.ps1
```
