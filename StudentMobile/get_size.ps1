Add-Type -AssemblyName System.Drawing
$img = [System.Drawing.Image]::FromFile('c:\Users\ACT-STUDENT\Desktop\SAMS-CLONING\StudentMobile\Resources\Splash\mkasplash.png')
Write-Host "Width: $($img.Width), Height: $($img.Height)"
