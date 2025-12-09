# 🧪 SCRIPT DE PRUEBAS DE CONCURRENCIA - SAUNA KALIXTO
# Ejecuta múltiples instancias para simular usuarios concurrentes

param(
    [int]$NumeroInstancias = 3,
    [int]$TiempoEspera = 2
)

# Detectar automáticamente la ruta del proyecto
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$RutaProyecto = Join-Path (Split-Path -Parent $scriptPath) "ProyectoSauna"

Write-Host "🚀 INICIANDO PRUEBAS DE CONCURRENCIA" -ForegroundColor Green
Write-Host "📊 Configuración:"
Write-Host "   - Instancias a crear: $NumeroInstancias"
Write-Host "   - Tiempo entre lanzamientos: $TiempoEspera segundos"
Write-Host "   - Ruta del proyecto: $RutaProyecto"
Write-Host ""

# Verificar que existe el proyecto
if (!(Test-Path "$RutaProyecto\ProyectoSauna.csproj")) {
    Write-Host "❌ ERROR: No se encuentra el proyecto ProyectoSauna.csproj en $RutaProyecto" -ForegroundColor Red
    Write-Host "💡 Asegúrese de que el script está en la carpeta Tools/ del proyecto" -ForegroundColor Yellow
    exit 1
}

# Array para almacenar los procesos
$procesos = @()

Write-Host "🔄 Lanzando instancias..." -ForegroundColor Yellow

for ($i = 1; $i -le $NumeroInstancias; $i++) {
    Write-Host "   ▶️ Lanzando instancia $i..." -ForegroundColor Cyan
    
    # Iniciar nueva instancia del proyecto
    $proceso = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory $RutaProyecto -PassThru
    $procesos += $proceso
    
    Write-Host "      ✅ Instancia $i iniciada (PID: $($proceso.Id))"
    
    # Esperar antes de lanzar la siguiente
    if ($i -lt $NumeroInstancias) {
        Start-Sleep -Seconds $TiempoEspera
    }
}

Write-Host ""
Write-Host "🎯 PRUEBAS SUGERIDAS:" -ForegroundColor Green
Write-Host "1. 👥 Abrir módulo 'Cuentas' en todas las instancias"
Write-Host "2. 🛒 Buscar el MISMO cliente en todas las ventanas"
Write-Host "3. 📦 Intentar agregar el MISMO producto simultáneamente"
Write-Host "4. 🔄 Observar mensajes de concurrencia y stock actualizado"
Write-Host "5. 🗑️ Intentar eliminar cuentas al mismo tiempo"
Write-Host ""
Write-Host "⚠️  ESCENARIOS CRÍTICOS A PROBAR:" -ForegroundColor Yellow
Write-Host "   • Stock de productos con cantidad limitada (ej: 1-5 unidades)"
Write-Host "   • Modificación de totales de cuenta simultánea"
Write-Host "   • Eliminación de cuentas con productos"
Write-Host "   • Cambio de estado de cuenta (Pendiente -> Pagada)"
Write-Host ""

Write-Host "📋 PROCESOS ACTIVOS:"
foreach ($proceso in $procesos) {
    if (!$proceso.HasExited) {
        Write-Host "   🟢 Instancia PID: $($proceso.Id) - Activa" -ForegroundColor Green
    } else {
        Write-Host "   🔴 Instancia PID: $($proceso.Id) - Terminada" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "🛑 Para detener todas las instancias, presione CTRL+C"
Write-Host "💡 O ejecute: Get-Process dotnet | Where-Object {`$_.ProcessName -eq 'dotnet'} | Stop-Process"

# Esperar a que el usuario presione una tecla
Read-Host "Presione ENTER para salir del script (las instancias seguirán ejecutándose)"