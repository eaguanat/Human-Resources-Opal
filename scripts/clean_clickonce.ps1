<#
Script breve para limpiar cache de ClickOnce y eliminar bin/obj del repo.
Ejecutar desde PowerShell como: .\scripts\clean_clickonce.ps1
Advertencia: este script borra carpetas `bin` y `obj` recursivamente en el repo.
#>

# Detectar si Visual Studio está abierto
$vs = Get-Process -Name devenv -ErrorAction SilentlyContinue
if ($vs) {
    Write-Host "Detectado Visual Studio en ejecución (devenv). Cierra Visual Studio antes de continuar." -ForegroundColor Yellow
    Read-Host "Pulsa Enter para continuar (o Ctrl+C para abortar) una vez hayas cerrado VS"
}

Write-Host "1) Limpieza de la caché ClickOnce (dfshim)"
try {
    rundll32.exe dfshim.dll,CleanOnlineAppCache
    Write-Host "-> dfshim executed" -ForegroundColor Green
} catch {
    Write-Host "-> No se pudo ejecutar dfshim.dll,CleanOnlineAppCache: $_" -ForegroundColor Red
}

# Si mage.exe está disponible, limpiar su caché
$mage = Get-Command mage.exe -ErrorAction SilentlyContinue
if ($mage) {
    Write-Host "2) Ejecutando 'mage -cc' (si está disponible)"
    mage.exe -cc
} else {
    Write-Host "2) 'mage.exe' no se encontró en PATH, se omite." -ForegroundColor Yellow
}

# Renombrar la carpeta de caché de ClickOnce para forzar recreación
$localApps = Join-Path $env:LOCALAPPDATA "Apps\2.0"
if (Test-Path $localApps) {
    $backup = "$localApps" + "_backup_" + (Get-Date -Format "yyyyMMdd_HHmmss")
    try {
        Rename-Item -Path $localApps -NewName (Split-Path $backup -Leaf) -ErrorAction Stop
        Write-Host "3) Renombrada la carpeta de caché ClickOnce a: $backup" -ForegroundColor Green
    } catch {
        Write-Host "-> No se pudo renombrar $localApps: $_" -ForegroundColor Red
    }
} else {
    Write-Host "3) No existe la carpeta de caché ClickOnce: $localApps" -ForegroundColor Yellow
}

# Eliminar bin y obj en el repo (peligroso): confirmar
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $root) { $root = Get-Location }
Write-Host "Raíz usada para buscar bin/obj: $root"
$confirm = Read-Host "¿Eliminar recursivamente todas las carpetas 'bin' y 'obj' bajo esta raíz? (S/N)"
if ($confirm -notin @('S','s','Si','SI','Y','y','Yes')) {
    Write-Host "Omitiendo eliminación de bin/obj." -ForegroundColor Yellow
} else {
    Write-Host "Eliminando carpetas 'bin' y 'obj'..."
    Get-ChildItem -Path $root -Recurse -Directory -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -in @('bin','obj') } |
        ForEach-Object {
            try {
                Remove-Item -LiteralPath $_.FullName -Recurse -Force -ErrorAction Stop
                Write-Host "-> Eliminado: $($_.FullName)" -ForegroundColor Green
            } catch {
                Write-Host "-> Falló al eliminar $($_.FullName): $_" -ForegroundColor Red
            }
        }
}

Write-Host "
PASOS FINALES (manuales):" -ForegroundColor Cyan
Write-Host " - Abre Configuración > Aplicaciones y desinstala la app ClickOnce si aparece (p. ej. 'Human Resources')."
Write-Host " - Abre Visual Studio, reconstruye la solución y asegúrate en Propiedades > Debug que 'Start project' esté seleccionado."

# Intentar reconstruir si msbuild está disponible
$msbuild = Get-Command msbuild.exe -ErrorAction SilentlyContinue
if ($msbuild) {
    $sln = Get-ChildItem -Path $root -Filter *.sln -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($sln) {
        Write-Host "Intentando msbuild para reconstruir solución: $($sln.FullName)"
        & msbuild.exe $sln.FullName /t:Rebuild /p:Configuration=Debug
    } else {
        Write-Host "No se encontró .sln en la raíz. Abre la solución y reconstruye desde Visual Studio." -ForegroundColor Yellow
    }
} else {
    Write-Host "msbuild.exe no se encontró en PATH. Reconstruye la solución desde Visual Studio." -ForegroundColor Yellow
}

Write-Host "Script finalizado." -ForegroundColor Cyan
