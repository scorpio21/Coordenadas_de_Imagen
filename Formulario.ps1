Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

[System.Windows.Forms.Application]::EnableVisualStyles()

$form = New-Object System.Windows.Forms.Form
$form.Text = "Visor de Limites y Coordenadas"
$form.Size = New-Object System.Drawing.Size(1250, 750)
$form.StartPosition = "CenterScreen"

$panelTop = New-Object System.Windows.Forms.Panel
$panelTop.Dock = "Top"
$panelTop.Height = 110

$btnLoad = New-Object System.Windows.Forms.Button
$btnLoad.Text = "1. Cargar Imagen"
$btnLoad.Location = New-Object System.Drawing.Point(10, 10)
$btnLoad.AutoSize = $true
$btnLoad.Height = 40
$btnLoad.Font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)

$btnRunPNG = New-Object System.Windows.Forms.Button
$btnRunPNG.Text = "2. Escaneo Inteligente Automatico (.png / .jpg)"
$btnRunPNG.Location = New-Object System.Drawing.Point(150, 10)
$btnRunPNG.AutoSize = $true
$btnRunPNG.Height = 40
$btnRunPNG.Font = New-Object System.Drawing.Font("Segoe UI", 9, [System.Drawing.FontStyle]::Bold)
$btnRunPNG.Enabled = $false
$btnRunPNG.Visible = $false

$btnRunBMP = New-Object System.Windows.Forms.Button
$btnRunBMP.Text = "2. Escaneo Estricto por Cuadricula (.bmp)"
$btnRunBMP.Location = New-Object System.Drawing.Point(150, 10)
$btnRunBMP.AutoSize = $true
$btnRunBMP.Height = 40
$btnRunBMP.Font = New-Object System.Drawing.Font("Segoe UI", 9, [System.Drawing.FontStyle]::Bold)
$btnRunBMP.Enabled = $false
$btnRunBMP.Visible = $false

$lblCols = New-Object System.Windows.Forms.Label
$lblCols.Text = "Cols:"
$lblCols.AutoSize = $true
$lblCols.Location = New-Object System.Drawing.Point(450, 22)
$lblCols.Visible = $false

$numCols = New-Object System.Windows.Forms.NumericUpDown
$numCols.Location = New-Object System.Drawing.Point(485, 20)
$numCols.Width = 50
$numCols.Value = 4
$numCols.Minimum = 1
$numCols.Maximum = 100
$numCols.Visible = $false

$lblRows = New-Object System.Windows.Forms.Label
$lblRows.Text = "Filas:"
$lblRows.AutoSize = $true
$lblRows.Location = New-Object System.Drawing.Point(540, 22)
$lblRows.Visible = $false

$numRows = New-Object System.Windows.Forms.NumericUpDown
$numRows.Location = New-Object System.Drawing.Point(575, 20)
$numRows.Width = 50
$numRows.Value = 4
$numRows.Minimum = 1
$numRows.Maximum = 100
$numRows.Visible = $false

$chkManual = New-Object System.Windows.Forms.CheckBox
$chkManual.Text = "Modo Marcacion Libre (Raton)"
$chkManual.Location = New-Object System.Drawing.Point(10, 60)
$chkManual.AutoSize = $true
$chkManual.Font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)
$chkManual.ForeColor = [System.Drawing.Color]::DarkRed

$btnLimpiar = New-Object System.Windows.Forms.Button
$btnLimpiar.Text = "Borrar Cajas Manuales"
$btnLimpiar.Location = New-Object System.Drawing.Point(240, 58)
$btnLimpiar.AutoSize = $true
$btnLimpiar.Height = 30
$btnLimpiar.Font = New-Object System.Drawing.Font("Segoe UI", 9, [System.Drawing.FontStyle]::Regular)
$btnLimpiar.Visible = $false

$lblStatus = New-Object System.Windows.Forms.Label
$lblStatus.Location = New-Object System.Drawing.Point(400, 65)
$lblStatus.AutoSize = $true
$lblStatus.Text = "Haz clic en '1. Cargar Imagen'."
$lblStatus.Font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Italic)
$lblStatus.ForeColor = [System.Drawing.Color]::Blue

$panelTop.Controls.Add($btnLoad)
$panelTop.Controls.Add($btnRunPNG)
$panelTop.Controls.Add($btnRunBMP)
$panelTop.Controls.Add($lblCols)
$panelTop.Controls.Add($numCols)
$panelTop.Controls.Add($lblRows)
$panelTop.Controls.Add($numRows)
$panelTop.Controls.Add($chkManual)
$panelTop.Controls.Add($btnLimpiar)
$panelTop.Controls.Add($lblStatus)
$form.Controls.Add($panelTop)

$split = New-Object System.Windows.Forms.SplitContainer
$split.Dock = "Fill"
$split.SplitterDistance = 800

$pb = New-Object System.Windows.Forms.PictureBox
$pb.Dock = "Fill"
$pb.SizeMode = "Zoom"
$pb.BackColor = [System.Drawing.Color]::LightGray
$split.Panel1.Controls.Add($pb)

$panelRight = New-Object System.Windows.Forms.Panel
$panelRight.Dock = "Fill"
$panelRight.Padding = New-Object System.Windows.Forms.Padding(10)

$lblRight = New-Object System.Windows.Forms.Label
$lblRight.Text = "Resultados (Coordenadas):"
$lblRight.Dock = "Top"
$lblRight.Height = 25
$lblRight.Font = New-Object System.Drawing.Font("Segoe UI", 9, [System.Drawing.FontStyle]::Bold)

$btnExport = New-Object System.Windows.Forms.Button
$btnExport.Text = "Exportar a .md"
$btnExport.Dock = "Bottom"
$btnExport.Height = 40
$btnExport.Enabled = $false
$btnExport.Font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)

$txtCoords = New-Object System.Windows.Forms.TextBox
$txtCoords.Multiline = $true
$txtCoords.ScrollBars = "Vertical"
$txtCoords.Dock = "Fill"
$txtCoords.ReadOnly = $true
$txtCoords.Font = New-Object System.Drawing.Font("Consolas", 9)

$panelRight.Controls.Add($txtCoords)
$panelRight.Controls.Add($lblRight)
$panelRight.Controls.Add($btnExport)

$split.Panel2.Controls.Add($panelRight)
$form.Controls.Add($split)
$split.BringToFront()

# Variables Globales
$script:loadedFilename = $null
$script:originalBmp = $null
$script:lastCoordinates = ""

$script:isDrawing = $false
$script:rectStartImg = New-Object System.Drawing.Point(0,0)
$script:currentMouseImg = New-Object System.Drawing.Point(0,0)
$script:manualRects = New-Object System.Collections.ArrayList
$script:autoRects = New-Object System.Collections.ArrayList

function Get-ImagePos($mouseX, $mouseY) {
    if (-not $pb.Image) { return New-Object System.Drawing.Point(0,0) }
    $cW = $pb.ClientRectangle.Width; $cH = $pb.ClientRectangle.Height
    $iW = $script:originalBmp.Width; $iH = $script:originalBmp.Height
    $ratio = [math]::Min($cW/$iW, $cH/$iH)
    $padX = ($cW - ($iW * $ratio)) / 2; $padY = ($cH - ($iH * $ratio)) / 2
    $oX = [math]::Max(0, [math]::Min($iW, [math]::Round(($mouseX - $padX) / $ratio)))
    $oY = [math]::Max(0, [math]::Min($iH, [math]::Round(($mouseY - $padY) / $ratio)))
    return New-Object System.Drawing.Point([int]$oX, [int]$oY)
}

function Get-ControlPos($imgX, $imgY) {
    if (-not $pb.Image) { return New-Object System.Drawing.Point(0,0) }
    $cW = $pb.ClientRectangle.Width; $cH = $pb.ClientRectangle.Height
    $iW = $script:originalBmp.Width; $iH = $script:originalBmp.Height
    $ratio = [math]::Min($cW/$iW, $cH/$iH)
    $padX = ($cW - ($iW * $ratio)) / 2; $padY = ($cH - ($iH * $ratio)) / 2
    $cX = ($imgX * $ratio) + $padX; $cY = ($imgY * $ratio) + $padY
    return New-Object System.Drawing.Point([int]$cX, [int]$cY)
}

function Update-CoordText {
    $coordText = ""
    $i = 1
    foreach ($r in $script:autoRects) {
        $coordText += "- **$($r.Label)**: X=$($r.X), Y=$($r.Y), Ancho=$($r.Width), Alto=$($r.Height)`r`n"
    }
    foreach ($r in $script:manualRects) {
        $coordText += "- **Manual $i**: X=$($r.X), Y=$($r.Y), Ancho=$($r.Width), Alto=$($r.Height)`r`n"
        $i++
    }
    $txtCoords.Text = $coordText
    $script:lastCoordinates = $coordText
    $btnExport.Enabled = ($coordText.Length -gt 0)
    $pb.Invalidate()
}

$btnLoad.Add_Click({
    $ofd = New-Object System.Windows.Forms.OpenFileDialog
    $ofd.Filter = "Imagenes|*.jpg;*.jpeg;*.png;*.bmp"
    $ofd.Title = "Selecciona la imagen a escanear"
    if ($ofd.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {
        if ($script:originalBmp) { $script:originalBmp.Dispose() }
        
        $script:loadedFilename = $ofd.FileName
        $script:originalBmp = New-Object System.Drawing.Bitmap $script:loadedFilename
        $pb.Image = $script:originalBmp

        $script:manualRects.Clear()
        $script:autoRects.Clear()
        Update-CoordText
        
        $ext = [System.IO.Path]::GetExtension($script:loadedFilename).ToLower()
        if ($ext -eq ".bmp") {
            $btnRunBMP.Enabled = $true
            $btnRunBMP.Visible = $true
            $btnRunPNG.Enabled = $false
            $btnRunPNG.Visible = $false
            
            $lblCols.Visible = $true
            $numCols.Visible = $true
            $lblRows.Visible = $true
            $numRows.Visible = $true
            
            $lblStatus.Text = "Archivo .bmp detectado. Usa la cuadricula manual BMP."
        } else {
            $btnRunBMP.Enabled = $false
            $btnRunBMP.Visible = $false
            $btnRunPNG.Enabled = $true
            $btnRunPNG.Visible = $true
            
            $lblCols.Visible = $false
            $numCols.Visible = $false
            $lblRows.Visible = $false
            $numRows.Visible = $false
            
            $lblStatus.Text = "Archivo .png / .jpg detectado. Usa el algoritmo automático inteligente."
        }
    }
})

$btnRunPNG.Add_Click({
    if (-not $script:loadedFilename) { return }
    $btnRunPNG.Enabled = $false
    $btnLoad.Enabled = $false
    $btnExport.Enabled = $false
    $lblStatus.Text = "Ejecutando escaneo automatico por proyeccion X/Y..."
    [System.Windows.Forms.Application]::DoEvents()

    try {
        $scriptPath = Join-Path -Path $PSScriptRoot -ChildPath "GetBounds.ps1"
        if (-not (Test-Path $scriptPath)) { $scriptPath = "e:\xampp\tamano\GetBounds.ps1" }

        $rectangles = & $scriptPath -ImagePath $script:loadedFilename
        $script:autoRects.Clear()

        if ($rectangles) {
            foreach ($r in $rectangles) {
                # Add dummy object mirroring BoundsRecord struct properties for drawing
                $obj = New-Object PSObject
                $obj | Add-Member -Type NoteProperty -Name "X" -Value $r.X
                $obj | Add-Member -Type NoteProperty -Name "Y" -Value $r.Y
                $obj | Add-Member -Type NoteProperty -Name "Width" -Value $r.Width
                $obj | Add-Member -Type NoteProperty -Name "Height" -Value $r.Height
                $obj | Add-Member -Type NoteProperty -Name "Label" -Value $r.Label
                $script:autoRects.Add($obj) | Out-Null
            }
        }
        Update-CoordText
        $lblStatus.Text = "Escaneo inteligente automatico completado con exito!"

    } catch {
        [System.Windows.Forms.MessageBox]::Show("Error: $_")
        $lblStatus.Text = "Error ejecutando el script PNG."
    } finally {
        $btnRunPNG.Enabled = $true
        $btnLoad.Enabled = $true
    }
})

$btnRunBMP.Add_Click({
    if (-not $script:loadedFilename) { return }
    $btnRunBMP.Enabled = $false
    $btnLoad.Enabled = $false
    $btnExport.Enabled = $false
    $numCols.Enabled = $false
    $numRows.Enabled = $false
    $lblStatus.Text = "Ejecutando escaneo manual estricto por cuadricula BMP..."
    [System.Windows.Forms.Application]::DoEvents()

    try {
        $scriptPath = Join-Path -Path $PSScriptRoot -ChildPath "GetBoundsBMP.ps1"
        if (-not (Test-Path $scriptPath)) { $scriptPath = "e:\xampp\tamano\GetBoundsBMP.ps1" }

        $gridCols = [int]$numCols.Value
        $gridRows = [int]$numRows.Value

        $rectangles = & $scriptPath -ImagePath $script:loadedFilename -Cols $gridCols -Rows $gridRows
        $script:autoRects.Clear()

        if ($rectangles) {
            foreach ($r in $rectangles) {
                $obj = New-Object PSObject
                $obj | Add-Member -Type NoteProperty -Name "X" -Value $r.X
                $obj | Add-Member -Type NoteProperty -Name "Y" -Value $r.Y
                $obj | Add-Member -Type NoteProperty -Name "Width" -Value $r.Width
                $obj | Add-Member -Type NoteProperty -Name "Height" -Value $r.Height
                $obj | Add-Member -Type NoteProperty -Name "Label" -Value $r.Label
                $script:autoRects.Add($obj) | Out-Null
            }
        }
        Update-CoordText
        $lblStatus.Text = "Escaneo de cuadricula BMP completo (Revisa las cajas)."

    } catch {
        [System.Windows.Forms.MessageBox]::Show("Error: $_")
        $lblStatus.Text = "Error ejecutando el script BMP."
    } finally {
        $btnRunBMP.Enabled = $true
        $btnLoad.Enabled = $true
        $numCols.Enabled = $true
        $numRows.Enabled = $true
    }
})

$chkManual.Add_CheckedChanged({
    $btnLimpiar.Visible = $chkManual.Checked
    if ($chkManual.Checked) {
        $lblStatus.Text = "Modo Libre: Pulsa y arrastra sobre la imagen ajustada a la pantalla para crear cajas."
    }
})

$btnLimpiar.Add_Click({
    $script:manualRects.Clear()
    Update-CoordText
    $lblStatus.Text = "Cajas manuales borradas."
})

$pb.Add_MouseDown({
    if (-not $script:loadedFilename -or -not $chkManual.Checked) { return }
    $script:isDrawing = $true
    $script:rectStartImg = Get-ImagePos $_.X $_.Y
})

$pb.Add_MouseMove({
    if ($script:isDrawing) {
        $script:currentMouseImg = Get-ImagePos $_.X $_.Y
        $pb.Invalidate()
    }
})

$pb.Add_MouseUp({
    if ($script:isDrawing) {
        $script:isDrawing = $false
        $script:currentMouseImg = Get-ImagePos $_.X $_.Y
        $x = [math]::Min($script:rectStartImg.X, $script:currentMouseImg.X)
        $y = [math]::Min($script:rectStartImg.Y, $script:currentMouseImg.Y)
        $w = [math]::Abs($script:currentMouseImg.X - $script:rectStartImg.X)
        $h = [math]::Abs($script:currentMouseImg.Y - $script:rectStartImg.Y)
        if ($w -gt 2 -and $h -gt 2) {
            $rect = New-Object System.Drawing.Rectangle($x, $y, $w, $h)
            $script:manualRects.Add($rect) | Out-Null
            Update-CoordText
        }
        $pb.Invalidate()
    }
})

$pb.Add_Paint({
    if (-not $script:loadedFilename) { return }
    $g = $_.Graphics

    $cW = $pb.ClientRectangle.Width; $cH = $pb.ClientRectangle.Height
    $iW = $script:originalBmp.Width; $iH = $script:originalBmp.Height
    $ratio = [math]::Min($cW/$iW, $cH/$iH)

    $penAuto = New-Object System.Drawing.Pen([System.Drawing.Color]::LimeGreen, 2)
    $font = New-Object System.Drawing.Font("Arial", 10, [System.Drawing.FontStyle]::Bold)
    $brush = [System.Drawing.Brushes]::Yellow

    # Dibujar autodetectados
    foreach ($r in $script:autoRects) {
        $p = Get-ControlPos $r.X $r.Y
        $drW = $r.Width * $ratio; $drH = $r.Height * $ratio
        $g.DrawRectangle($penAuto, $p.X, $p.Y, [int]$drW, [int]$drH)
        $txtY = if ($p.Y -gt 15) { $p.Y - 15 } else { $p.Y }
        $g.DrawString($r.Label, $font, $brush, $p.X, $txtY)
    }

    # Dibujar manuales
    $penManual = New-Object System.Drawing.Pen([System.Drawing.Color]::DeepPink, 2)
    $i = 1
    foreach ($r in $script:manualRects) {
        $p = Get-ControlPos $r.X $r.Y
        $drW = $r.Width * $ratio; $drH = $r.Height * $ratio
        $g.DrawRectangle($penManual, $p.X, $p.Y, [int]$drW, [int]$drH)
        $txtY = if ($p.Y -gt 15) { $p.Y - 15 } else { $p.Y }
        $g.DrawString("M$i", $font, [System.Drawing.Brushes]::White, $p.X, $txtY)
        $i++
    }

    # Dibujar arrastre actual
    if ($script:isDrawing) {
        $x = [math]::Min($script:rectStartImg.X, $script:currentMouseImg.X)
        $y = [math]::Min($script:rectStartImg.Y, $script:currentMouseImg.Y)
        $w = [math]::Abs($script:currentMouseImg.X - $script:rectStartImg.X)
        $h = [math]::Abs($script:currentMouseImg.Y - $script:rectStartImg.Y)
        $p = Get-ControlPos $x $y
        $drW = $w * $ratio; $drH = $h * $ratio

        $dashPen = New-Object System.Drawing.Pen([System.Drawing.Color]::Cyan, 2)
        $dashPen.DashStyle = [System.Drawing.Drawing2D.DashStyle]::Dash
        $g.DrawRectangle($dashPen, $p.X, $p.Y, [int]$drW, [int]$drH)
        $dashPen.Dispose()
    }

    $penAuto.Dispose()
    $penManual.Dispose()
    $font.Dispose()
})

$btnExport.Add_Click({
    $sfd = New-Object System.Windows.Forms.SaveFileDialog
    $sfd.Filter = "Markdown Files|*.md|Text Files|*.txt|All Files|*.*"
    $sfd.FileName = "Coordenadas.md"
    $sfd.Title = "Exportar coordenadas"
    if ($sfd.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {
        try {
            $mdContent = "# Reporte de Coordenadas de Imagen`r`n`r`n"
            $mdContent += "Archivo analizado: `$($script:loadedFilename)`r`n`r`n"
            $mdContent += "## Resultados`r`n`r`n"
            $mdContent += $script:lastCoordinates

            [System.IO.File]::WriteAllText($sfd.FileName, $mdContent, [System.Text.Encoding]::UTF8)
            [System.Windows.Forms.MessageBox]::Show("Archivo guardado con exito.", "Exportar", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Information)
        } catch {
            [System.Windows.Forms.MessageBox]::Show("Error al guardar archivo: $_")
        }
    }
})

$form.ShowDialog() | Out-Null
