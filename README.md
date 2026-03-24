# Tamano Project

Visor de cortes y límites para imágenes divididas en matrices, mostrando las áreas de contenido detectadas.

## Funcionalidad actual

El script `GetBounds.ps1` implementa una aplicación visual con Windows Forms. Al ejecutarlo:

1. Muestra un botón "Cargar Imagen".
2. Abre un seleccionador de archivos para elegir una imagen.
3. Procesa y segmenta la imagen, dibujando cuadros en cada área no correspondiente a los bordes de fondo detectados.
4. Muestra un "Grid" de referencia y marcas visuales encima de la imagen procesada.

## Cambios recientes

- Se refactorizó la lógica en C# con `System.Windows.Forms` para mostrar un visualizador de estado UI maximizado.
- Se agregó el botón para cargar imagen dinámicamente (`OpenFileDialog`).
- Se introdujo renderizado gráfico en la imagen con `Graphics.DrawRectangle` para resaltar las secciones.
