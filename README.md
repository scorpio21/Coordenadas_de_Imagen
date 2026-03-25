# Analizador de Sprite Sheets (.NET 8 Windows Forms)

Una potente herramienta con interfaz gráfica (Windows Forms) construida íntegramente en **C# y .NET 8** con arquitectura modular. Diseñada para detectar dinámicamente personajes, objetos y efectos en hojas de recortes (Sprite Sheets). Originalmente programada en PowerShell, ahora portada a Visual Studio.

## Novedades de la última versión:
- **Arquitectura Modular C#**: Proyecto estructurado bajo directrices sólidas incluyendo separación de `Forms`, `Services`, `Models` y `Utils`.
- **Escaneo Inteligente (Proyección X/Y)**: Para `.png` y `.jpg`. Detecta automáticamente bloques separados por canal alfa o negro. Inmune a las sombras oscuras que antes fracturaban dibujos como dragones o armaduras.
- **Modo Cuadrícula (Grid Slicer)**: Activo para `.bmp`. Corta exactamente el número de Filas y Columnas especificadas por el usuario (Ideal para particulas simétricas).
- **Modo Marcación Manual**: Dibuja tus propias cajas superpuestas arrastrando el ratón libremente como en Photoshop. Adapta inteligentemente las dimensiones usando un conversor visual de proporción `Zoom`.
- **Exportación de Registros**: Guarda todas tus coordenadas detectadas (tanto Automáticas como Manuales) limpiamente en un archivo Markdown (`Coordenadas.md`).

## Estructura de código
- `/Forms`: Interfaz visual `MainForm.cs` y eventos del ratón.
- `/Services`: `ImageScannerService.cs`, motor en C# de Proyección Auto-inteligente X/Y y escaneo algorítmico por celdas puras.
- `/Utils`: `CoordinateUtils.cs`, cálculos de transformaciones gráficas con el PictureBox.
- `/Models`: Entidades de datos (`BoundsRecord`).

## Uso:
El proyecto puede abrirse directamente en **Visual Studio 2022** (doble click en `CoordenadasImagen.csproj`) o ejecutarse desde la terminal utilizando la CLI de .NET 8:

```bash
# Para ejecutar la aplicación:
dotnet run

# Para compilar los binarios:
dotnet build -c Release
```
