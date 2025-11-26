# Promociones (Programa de Fidelización) y Aplicación de Descuentos

## Objetivo
Gestionar reglas de fidelización y descuentos sin afectar módulos existentes, y aplicar descuentos automáticamente al crear cuentas/consumos cuando corresponda.

## Modelo de Datos
- Tabla `ProgramaFidelizacion`:
  - `visitasParaDescuento` (int, NOT NULL)
  - `porcentajeDescuento` (decimal(5,2), NOT NULL)
  - `descuentoCumpleanos` (bit, NOT NULL)
  - `montoDescuentoCumpleanos` (decimal(12,2), NOT NULL)
- Entidad: `Models/Entities/ProgramaFidelizacion.cs`
- DbContext: `Models/SaunaDbContext.cs` con mapeo de decimales.

## Reglas de Negocio
- Descuento por cumpleaños:
  - Sólo aplica si hoy es el cumpleaños del cliente.
  - Debe tener un programa asignado y `descuentoCumpleanos = true`.
  - `montoDescuentoCumpleanos > 0`.
- Descuento por visitas:
  - Aplica si `visitasTotales >= visitasParaDescuento`.
  - Se usa `porcentajeDescuento` (0–100).
- Orden de aplicación:
  1. Descuento fijo de cumpleaños.
  2. Descuento porcentual por visitas sobre el monto resultante.

## Implementación
- Cálculo en `Models/Entities/ClienteExtensions.cs`:
  - `EsCumpleanosHoy()`
  - `ObtenerDescuentoCumpleanos()`
  - `ObtenerPorcentajeDescuentoVisitas()`
  - `AplicarDescuentos(monto)` y `CalcularDetallesDescuentos(monto)`
- Servicio `Services/DescuentoService.cs`:
  - `VerificarDescuentosAsync(idCliente)`
  - `CalcularTotalConDescuentosAsync(idCliente, montoOriginal)`
  - `ObtenerDetallesDescuentosAsync(idCliente, montoOriginal)`
- Carga de cliente con programa: `Repositories/CuentaRepository.cs:ObtenerClienteConProgramaAsync`.

## Módulo Promociones
- Vista: `UserControlPromociones.xaml`
- Lógica: `UserControlPromociones.xaml.cs`
- Validaciones:
  - `visitas` entero > 0.
  - `porcentaje` 0–100.
  - `montoCumple` ≥ 0; si `descuentoCumpleanos` está desactivado, se fuerza a `0`.
- Botón único Guardar/Actualizar con habilitación en tiempo real.
- Filtros: visitas exactas y “solo cumpleaños”.

## Integración en Cuentas y Consumos (a implementar)
1. Al seleccionar cliente o crear cuenta:
   - Cargar cliente con programa: `ObtenerClienteConProgramaAsync(idCliente)`.
   - Mostrar resumen de descuentos: `DescuentoService.VerificarDescuentosAsync(idCliente)`.
2. Al calcular totales:
   - Usar `DescuentoService.CalcularTotalConDescuentosAsync(idCliente, montoOriginal)`.
3. Para desglose en UI:
   - `DescuentoService.ObtenerDetallesDescuentosAsync(idCliente, montoOriginal)` y presentar `DetalleDescuentos.ObtenerResumen()`.

## Consideraciones
- Si no existe ningún programa o el cliente no tiene programa asignado, no se aplica ningún descuento (el sistema funciona sin necesidad de crear una promoción).
- La activación/desactivación de cumpleaños se consulta en tiempo real; no requiere reiniciar módulos.
- Seguridad: no se almacenan credenciales ni claves; se usa EF Core con consultas parametrizadas.

## Puntos de extensión
- Exportar programas a CSV.
- Mostrar indicadores de “ahorro total” en cuenta.
- Debounce en filtros para UX.