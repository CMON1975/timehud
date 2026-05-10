Add-Type -AssemblyName System.Windows.Forms

# create form
$form = New-Object System.Windows.Forms.Form
if ([System.Windows.Forms.Screen]::AllScreens.Count -gt 1) {
    $screen = [System.Windows.Forms.Screen]::AllScreens[0]
}
else {
    $screen = [System.Windows.Forms.Screen]::PrimaryScreen
}
$form.FormBorderStyle = 'None'
$form.Opacity = 0.65
$form.BackColor = 'Black'
$form.TopMost = $true
$form.ShowInTaskbar = $false
$form.StartPosition = 'Manual'
# $form.Location = $screen.Bounds.Location

# Form dimensions (should match your existing values)
$formWidth = 200
$horizontalOffset = 100

# Compute position: top-right with offset
$topRightX = $screen.Bounds.Right - $formWidth - $horizontalOffset
$topY = $screen.Bounds.Top

$form.Location = New-Object System.Drawing.Point($topRightX, $topY)

Add-Type @"
using System;
using System.Runtime.InteropServices;

public class Win32 {
    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateRoundRectRgn(
        int nLeftRect, int nTopRect,
        int nRightRect, int nBottomRect,
        int nWidthEllipse, int nHeightEllipse
    );

    [DllImport("user32.dll")]
    public static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);
}
"@

# create label
$label = New-Object System.Windows.Forms.Label
$label.AutoSize = $true
$label.Font = 'Consolas, 18'
$label.ForeColor = 'White'
$label.BackColor = 'Black'
$label.TextAlign = 'MiddleCenter'
$label.Dock = 'Fill'
$form.Controls.Add($label)

# update label text & resize
$update = {
    $label.Text = (Get-Date).ToString('HH:mm:ss')
    $form.ClientSize = $label.PreferredSize

    # apply rounded corner region
    $rgn = [Win32]::CreateRoundRectRgn(0, 0, $form.Width, $form.Height, 20, 20)
    [Win32]::SetWindowRgn($form.Handle, $rgn, $true) | Out-Null
}

$timer = New-Object System.Windows.Forms.Timer
$timer.Interval = 1000
$timer.Add_Tick($update)
$timer.Start()
$update.Invoke()

# context menu (right‑click to close)
$menu = New-Object System.Windows.Forms.ContextMenuStrip
$closeItem = $menu.Items.Add("Close")

$closeItem.Add_Click({ $form.Close() })
$label.ContextMenuStrip = $menu
$form.ContextMenuStrip = $menu

# dragging support
$script:mouseDown = $false
$script:startMousePos = $null
$script:startFormPos = $null

$onMouseDown = {
    param($src, $e)
    if ($e.Button -eq 'Left') {
        $script:mouseDown = $true
        $script:startMousePos = [System.Windows.Forms.Cursor]::Position
        $script:startFormPos = $form.Location
    }
}

$onMouseMove = {
    param($src, $e)
    if ($script:mouseDown) {
        $currentMousePos = [System.Windows.Forms.Cursor]::Position
        $dx = $currentMousePos.X - $script:startMousePos.X
        $dy = $currentMousePos.Y - $script:startMousePos.Y
        $form.Location = [System.Drawing.Point]::new(
            $script:startFormPos.X + $dx,
            $script:startFormPos.Y + $dy
        )
    }
}

$onMouseUp = {
    if ($script:mouseDown) {
        $script:mouseDown = $false
    }
}

# bind drag handlers
foreach ($c in @($form, $label)) {
    $c.Add_MouseDown($onMouseDown)
    $c.Add_MouseMove($onMouseMove)
    $c.Add_MouseUp($onMouseUp)
}

# show the window
$form.ShowDialog()