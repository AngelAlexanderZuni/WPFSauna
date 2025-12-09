@echo off
chcp 65001 >nul 2>&1
title SAUNA KALIXTO - SISTEMA COMPLETO DE CONCURRENCIA
color 0A

cd /d "%~dp0..\ProyectoSauna"

:MENU
cls
echo ========================================================================
echo              🏛️ SAUNA KALIXTO - CONTROL DE CONCURRENCIA 
echo ========================================================================
echo.
echo ⚡ Sistema completo de concurrencia implementado para:
echo 📋 • CUENTAS: Stock, duplicados, sincronización inteligente
echo 👥 • CLIENTES: DNI únicos, correos, modificaciones seguras
echo 🔄 • COMUNICACIÓN: MemoryMappedFiles entre ventanas
echo 🛡️ • VALIDACIONES: Transacciones seguras y control de estado
echo.
echo ========================================================================
echo.
echo 🧪 SELECCIONA EL TIPO DE PRUEBA:
echo.
echo 1) PRUEBA AUTOMÁTICA COMPLETA (Recomendado) 
echo 2) Pruebas de CUENTAS Y STOCK
echo 3) Pruebas de CLIENTES Y DNI
echo 4) Control de Edición Simultánea (NUEVO)
echo 5) Gestión de Clientes Inactivos (NUEVO) 
echo 6) Abrir ventanas para pruebas manuales
echo 7) Compilar proyecto
echo 8) Salir
echo.
set /p "opcion=Selecciona una opción (1-8): "

if "%opcion%"=="1" goto COMPLETA
if "%opcion%"=="2" goto CUENTAS
if "%opcion%"=="3" goto CLIENTES
if "%opcion%"=="4" goto EDICION_SIMULTANEA
if "%opcion%"=="5" goto CLIENTES_INACTIVOS
if "%opcion%"=="6" goto MANUAL
if "%opcion%"=="7" goto COMPILAR
if "%opcion%"=="8" goto SALIR

echo.
echo ❌ Opción no válida. Intenta de nuevo.
timeout /t 2 /nobreak >nul
goto MENU

:COMPLETA
echo.
echo ========================================================================
echo                    🎯 PRUEBA AUTOMÁTICA COMPLETA
echo ========================================================================
echo.
echo Esta prueba cubre TODAS las funcionalidades implementadas:
echo.
echo 📝 PLAN INTEGRAL DE PRUEBAS:
echo 
echo 🔸 FASE 1: CUENTAS Y CONSUMOS
echo    • Control de stock de productos
echo    • Prevención de cuentas duplicadas
echo    • Sincronización inteligente entre ventanas
echo    • Sistema de actualización con pause/resume
echo.
echo 🔸 FASE 2: CLIENTES 
echo    • Prevención de DNI duplicados
echo    • Prevención de correos duplicados
echo    • Sincronización de tablas de clientes
echo    • Validación de modificación con cuentas activas
echo.
echo ⚠️ IMPORTANTE: Sigue las instrucciones paso a paso
echo.
pause

echo.
echo 🚀 Iniciando sistema con 3 ventanas...

start "" dotnet run
timeout /t 3 /nobreak >nul
start "" dotnet run
timeout /t 3 /nobreak >nul
start "" dotnet run

echo.
echo ========================================================================
echo                    📋 GUÍA COMPLETA DE PRUEBAS
echo ========================================================================
echo.
echo 🎯 CONFIGURACIÓN INICIAL (TODAS LAS FASES)
echo    1. Haz LOGIN en las 3 ventanas con tu usuario
echo    2. Ten las ventanas organizadas para verlas todas
echo    3. Sigue cada fase en orden
echo.
echo ========================================================================
echo                         🔸 FASE 1: CUENTAS
echo ========================================================================
echo.
echo 🧪 PRUEBA A: STOCK DE PRODUCTOS
echo    4. Ve al módulo "CUENTAS Y CONSUMOS" en todas las ventanas
echo    5. Busca un cliente y crea una cuenta en Ventana 1
echo    6. Agrega productos hasta agotar el stock (ej: shampoo)
echo    7. En Ventanas 2 y 3: verifica que el stock se actualiza automáticamente
echo    ✅ RESULTADO: Stock sincronizado en tiempo real
echo.
echo 🧪 PRUEBA B: CUENTAS DUPLICADAS  
echo    8. Busca el MISMO cliente en las 3 ventanas
echo    9. Intenta crear cuentas SIMULTÁNEAMENTE (al mismo tiempo)
echo   10. Solo UNA debe crearse exitosamente
echo    ✅ RESULTADO: Error "Cliente ya tiene cuenta activa" en las otras
echo.
echo 🧪 PRUEBA C: SINCRONIZACIÓN INTELIGENTE
echo   11. SIN SELECCIONAR cuenta: crea una nueva en Ventana 1
echo       ✅ Debe aparecer AUTOMÁTICAMENTE en V2 y V3
echo   12. SELECCIONA la cuenta en Ventana 2
echo   13. Crea otra cuenta nueva en Ventana 1  
echo       ✅ Aparece en V1 y V3, NO se actualiza V2 (pausada)
echo   14. En V2: Click "Limpiar Cuenta Activa"
echo       ✅ Inmediatamente aparece la nueva cuenta
echo.
echo ========================================================================
echo                         🔸 FASE 2: CLIENTES  
echo ========================================================================
echo.
echo 🧪 PRUEBA D: DNI DUPLICADOS
echo   15. Ve al módulo "CLIENTES" en todas las ventanas
echo   16. En V1: Crea cliente con DNI "12345678"
echo   17. En V2: AL MISMO TIEMPO, crea cliente con DNI "12345678"
echo   18. Solo UNA creación debe ser exitosa
echo    ✅ RESULTADO: Error "DNI duplicado" en la otra ventana
echo.
echo 🧪 PRUEBA E: CORREOS DUPLICADOS
echo   19. En V1: Crea cliente con correo "test@sauna.com"
echo   20. En V2: Intenta crear cliente con "test@sauna.com"  
echo   21. El segundo debe mostrar error
echo    ✅ RESULTADO: Error "Correo duplicado"
echo.
echo 🧪 PRUEBA F: SINCRONIZACIÓN DE CLIENTES
echo   22. SIN SELECCIONAR cliente: crea uno nuevo en V1
echo       ✅ Debe aparecer automáticamente en V2 y V3
echo   23. SELECCIONA el cliente en V2
echo   24. Crea otro cliente en V1
echo       ✅ Aparece en V1 y V3, NO en V2 (pausada)
echo   25. En V2: Cambia de cliente o limpia selección
echo       ✅ Se actualiza inmediatamente
echo.
echo 🧪 PRUEBA G: MODIFICACIÓN CON CUENTAS ACTIVAS
echo   26. Crea un cliente nuevo (ej: "Ana García", DNI "87654321")
echo   27. Ve a "CUENTAS Y CONSUMOS" y crea una cuenta para Ana
echo   28. Vuelve a "CLIENTES" e intenta modificar a Ana
echo   29. Debe mostrar error: "Cliente con cuentas activas"
echo   30. Cierra la cuenta de Ana en el módulo de cuentas
echo   31. Vuelve a intentar modificar el cliente
echo    ✅ RESULTADO: Ahora debe permitir la modificación
echo.
echo ========================================================================
echo.
echo ✅ RESULTADOS ESPERADOS GLOBALES:
echo 🔸 Stock controlado entre ventanas sin conflictos
echo 🔸 NO se crean cuentas duplicadas para el mismo cliente  
echo 🔸 NO se crean clientes con DNI o correos duplicados
echo 🔸 Tablas se sincronizan automáticamente
echo 🔸 NO se pierde selección durante trabajo activo
echo 🔸 Validaciones de negocio funcionan correctamente
echo.
pause
goto MENU

:CUENTAS
echo.
echo ========================================================================
echo                       📋 PRUEBAS DE CUENTAS Y STOCK
echo ========================================================================
echo.
echo Estas pruebas verifican el control de concurrencia en:
echo 🔸 Stock de productos entre ventanas
echo 🔸 Creación simultánea de cuentas
echo 🔸 Sincronización inteligente de listas
echo.

start "" dotnet run
timeout /t 2 /nobreak >nul
start "" dotnet run
timeout /t 2 /nobreak >nul
start "" dotnet run

echo.
echo 📋 INSTRUCCIONES PARA CUENTAS:
echo.
echo 🎯 STOCK DE PRODUCTOS:
echo    1. Ve a "CUENTAS Y CONSUMOS" en todas las ventanas
echo    2. Crea cuentas y agrega productos
echo    3. Verifica que el stock se actualiza en todas las ventanas
echo.
echo 🎯 CUENTAS DUPLICADAS:
echo    4. Busca el mismo cliente en múltiples ventanas
echo    5. Intenta crear cuentas simultáneamente
echo    6. Solo una debe crearse exitosamente
echo.
echo 🎯 SINCRONIZACIÓN INTELIGENTE:
echo    7. Sin seleccionar: crea cuenta → aparece en todas
echo    8. Con selección: crea cuenta → solo se actualiza en las no pausadas
echo    9. Limpia selección → se actualiza inmediatamente
echo.
echo ✅ Ventanas abiertas. ¡Ejecuta las pruebas!
pause
goto MENU

:CLIENTES  
echo.
echo ========================================================================
echo                       👥 PRUEBAS DE CLIENTES Y DNI
echo ========================================================================
echo.
echo Estas pruebas verifican el control de concurrencia en:
echo 🔸 DNI únicos entre ventanas
echo 🔸 Correos únicos 
echo 🔸 Sincronización de tablas de clientes
echo 🔸 Validaciones con cuentas activas
echo.

start "" dotnet run
timeout /t 2 /nobreak >nul
start "" dotnet run

echo.
echo 👥 INSTRUCCIONES PARA CLIENTES:
echo.
echo 🎯 DNI DUPLICADOS:
echo    1. Ve a "CLIENTES" en ambas ventanas
echo    2. Intenta crear clientes con el mismo DNI simultáneamente
echo    3. Solo uno debe crearse exitosamente
echo.
echo 🎯 CORREOS DUPLICADOS:
echo    4. Intenta crear clientes con el mismo correo
echo    5. Debe mostrar error de correo duplicado
echo.
echo 🎯 MODIFICACIÓN CON CUENTAS:
echo    6. Crea un cliente
echo    7. Crea una cuenta para ese cliente
echo    8. Intenta modificar el cliente → debe dar error
echo    9. Cierra la cuenta e intenta de nuevo → debe permitirlo
echo.
echo 🎯 SINCRONIZACIÓN:
echo   10. Sin seleccionar: crea cliente → aparece en todas
echo   11. Con selección: updates inteligentes
echo.
echo ✅ Ventanas abiertas. ¡Ejecuta las pruebas!
pause
goto MENU

:MANUAL
echo.
echo ========================================================================
echo                      🪟 VENTANAS PARA PRUEBAS MANUALES
echo ========================================================================
echo.
echo 🚀 Abriendo 3 ventanas para que hagas tus propias pruebas...
echo.
echo 💡 PUEDES PROBAR LIBREMENTE:
echo    • Escenarios de stress con múltiples usuarios
echo    • Casos edge con datos reales
echo    • Combinaciones complejas de operaciones
echo    • Límites del sistema de concurrencia
echo    • Flujos de trabajo reales del negocio
echo.

start "" dotnet run
timeout /t 2 /nobreak >nul
start "" dotnet run
timeout /t 2 /nobreak >nul
start "" dotnet run

echo ✅ ¡Ventanas abiertas! Prueba todo lo que quieras.
echo.
echo 🔍 Si encuentras algún problema:
echo    • Revisa los logs en Visual Studio
echo    • Observa los mensajes de error en pantalla
echo    • Prueba las validaciones paso a paso
echo.
pause
goto MENU

:COMPILAR
echo.
echo 🔨 Compilando proyecto...
echo.
dotnet build --verbosity quiet
if %ERRORLEVEL% EQU 0 (
    echo ✅ Compilación exitosa
    echo.
    echo El sistema está listo para usar con todas las funciones de concurrencia:
    echo 📋 • Control de stock de productos
    echo 👥 • Validaciones únicas de clientes  
    echo 🔄 • Sincronización entre ventanas
    echo 🛡️ • Transacciones seguras
) else (
    echo ❌ Error en la compilación
    echo.
    echo Revisa los errores mostrados arriba y corrígelos antes de continuar.
)
echo.
pause
goto MENU

:SALIR
echo.
echo ========================================================================
echo                   🎉 SAUNA KALIXTO - SISTEMA COMPLETO
echo ========================================================================
echo.
echo ¡Gracias por usar el sistema de control de concurrencia!
echo.
echo 📊 RESUMEN DE LO IMPLEMENTADO:
echo.
echo ✅ CUENTAS Y CONSUMOS:
echo    • Control de stock en tiempo real
echo    • Prevención de cuentas duplicadas
echo    • Sincronización inteligente con pause/resume
echo    • Comunicación entre procesos con MemoryMappedFiles
echo.
echo ✅ CLIENTES:
echo    • DNI únicos con validación transaccional
echo    • Correos únicos con verificación
echo    • Modificaciones seguras con validación de estado
echo    • Sincronización automática de tablas
echo    • Control de edición simultánea (NUEVO)
echo    • Gestión de clientes inactivos (NUEVO)
echo.
echo ✅ INFRAESTRUCTURA:
echo    • Entity Framework con Optimistic Concurrency
echo    • Servicios especializados de validación
echo    • Sistema de eventos inter-proceso
echo    • Manejo robusto de excepciones
echo    • MemoryMappedFiles para control de edición
echo.
echo 💡 Para soporte técnico, revisa:
echo    • Los comentarios en el código fuente
echo    • Los logs de debug en Visual Studio
echo    • La documentación de cada servicio
echo.

:EDICION_SIMULTANEA
echo.
echo ========================================================================
echo                🔒 CONTROL DE EDICIÓN SIMULTÁNEA
echo ========================================================================
echo.
echo 🎯 Esta prueba demuestra el sistema de prevención de edición simultánea:
echo.
echo 📋 FUNCIONALIDADES:
echo    • Solo un usuario puede editar un cliente a la vez
echo    • Mensajes de advertencia cuando cliente está en edición
echo    • Liberación automática de bloqueos
echo    • Comunicación entre procesos para sincronización
echo.
echo 🧪 CÓMO PROBAR:
echo    1. Abrir 2 ventanas de la aplicación
echo    2. Ir al módulo CLIENTES en ambas
echo    3. En Ventana 1: Seleccionar un cliente para editar
echo    4. En Ventana 2: Intentar editar el mismo cliente
echo    5. Verificar mensaje "Cliente ya está en edición"
echo    6. En Ventana 1: Limpiar formulario o guardar
echo    7. En Ventana 2: Ahora puede editar el cliente
echo.
echo ⚠️ IMPORTANTE: No cerrar ventanas abruptamente durante las pruebas
echo.
pause

echo 🚀 Abriendo 2 ventanas para pruebas...
start "" dotnet run
timeout /t 3 /nobreak >nul
start "" dotnet run

echo.
echo ✅ Ventanas abiertas. Sigue las instrucciones de prueba.
echo 📋 Resultado esperado: Control exitoso de edición simultánea
pause
goto MENU

:CLIENTES_INACTIVOS
echo.
echo ========================================================================
echo                📋 GESTIÓN DE CLIENTES INACTIVOS
echo ========================================================================
echo.
echo 🎯 Esta función permite gestionar clientes inactivos de manera segura:
echo.
echo 🆕 NUEVAS FUNCIONALIDADES:
echo    • Checkbox "Gestionar Clientes Inactivos"
echo    • Vista especial para clientes desactivados
echo    • Buscador específico de clientes inactivos
echo    • Función de reactivación de clientes
echo    • No eliminación física - solo desactivación
echo.
echo 🧪 FLUJO DE PRUEBAS:
echo    1. Abrir aplicación y ir a módulo CLIENTES
echo    2. Seleccionar un cliente activo cualquiera
echo    3. Usar botón "Eliminar" para desactivar (NO elimina)
echo    4. Activar checkbox "Gestionar Clientes Inactivos"
echo    5. Buscar el cliente desactivado
echo    6. Seleccionarlo y usar "Reactivar"
echo    7. Desactivar checkbox para volver a vista normal
echo    8. Verificar que cliente aparece nuevamente activo
echo.
echo 🔄 CONCURRENCIA: Todas las operaciones mantienen sincronización
echo.
pause

echo 🚀 Abriendo aplicación para pruebas de clientes inactivos...
start "" dotnet run

echo.
echo ✅ Aplicación abierta. Sigue el flujo de pruebas.
echo 📋 Resultado esperado: Gestión completa de estados activo/inactivo
pause
goto MENU
echo ¡El sistema está listo para producción! 🚀
echo.
pause
exit