# 🎯 ASIGNACIÓN DE LOS 9 MÓDULOS DEL SIDEBAR

**Proyecto:** Sistema de Gestión Sauna KALIXTO  
**Fecha:** Octubre 27, 2025  
**Equipo:** 4 Developers + 1 Scrum Master

---

## 📊 RESUMEN VISUAL DEL SIDEBAR

```
┌────────────────────────────────────────┐
│       🏢 SAUNA KALIXTO                 │
│     Panel Administrativo               │
├────────────────────────────────────────┤
│ 📋 OPERACIONES                         │
│ ✅ 1️⃣ Entradas y Consumos            │ → ANGEL ZUÑIGA
│ ✅ 2️⃣ Pagos y Comprobantes           │ → NORMA ARANIBAR
│ ✅ 3️⃣ Clientes                       │ → JONATHAN PUMA ✅ LISTO
├────────────────────────────────────────┤
│ 💰 FINANZAS E INVENTARIO               │
│ ✅ 4️⃣ Caja y Flujo de Caja           │ → NORMA ARANIBAR
│ ✅ 5️⃣ Inventario                     │ → ANGEL ZUÑIGA
│ ✅ 6️⃣ Egresos                        │ → NORMA ARANIBAR
├────────────────────────────────────────┤
│ 📊 REPORTES                            │
│ ✅ 7️⃣ Reportes y Estadísticas        │ → LUIS VEGA
├────────────────────────────────────────┤
│ ⚙️ CONFIGURACIÓN                       │
│ ✅ 8️⃣ Usuarios                       │ → LUIS VEGA
│ ✅ 9️⃣ Promociones                    │ → JONATHAN PUMA (Opcional)
└────────────────────────────────────────┘
```

---

## 👥 EQUIPO DE DESARROLLO

| # | Developer            | Módulos Asignados                | Carga Total |
|---|-----------           |-------------------               |-------------|
| 1️⃣ | **Jonathan Puma**  | Clientes, Cuentas, Promociones   | 3 módulos |
| 2️⃣ | **Angel Zuñiga**   | Inventario, Consumos             | 2 módulos |
| 3️⃣ | **Norma Aranibar** | Pagos, Caja, Egresos             | 3 módulos |
| 4️⃣ | **Luis Vega**      | Usuarios, Reportes               | 2 módulos |

---

# 📋 DETALLE DE ASIGNACIÓN POR MÓDULO

---

## 1️⃣ MÓDULO: ENTRADAS Y CONSUMOS
**Nombre en Sidebar:** "Entradas y Consumos"  
**UserControl:** `UserControlConsumo.xaml`  
**Responsable:** 👤 **ANGEL ZUÑIGA CONDORI**  
**Semana:** 4 (13 Nov 2025)  
**Prioridad:** 🔴 CRÍTICA  
**Complejidad:** ⭐⭐⭐⭐ Alta

### 📝 ¿Qué hace este módulo?
Permite registrar **consumos en tiempo real** cuando el cliente ya está dentro del sauna y pide productos (agua, toalla, shampoo, etc.).

### 🎯 Funcionalidades principales:
1. **Buscar cliente con cuenta activa** (por DNI)
2. **Ver cuenta abierta del cliente** con detalle
3. **Agregar productos a la cuenta** (seleccionar producto + cantidad)
4. **Descontar stock automáticamente** al agregar consumo
5. **Calcular subtotal** en tiempo real
6. **Actualizar total de cuenta** automáticamente
7. **Ver lista de consumos** de la cuenta actual
8. **Eliminar consumo** (devuelve stock)

### 🗂️ Tablas involucradas:
- `DetalleConsumo` (insertar consumos)
- `Producto` (stock, precios)
- `Cuenta` (actualizar montoTotal)
- `Cliente` (buscar por DNI)

### 📦 Componentes a crear:
- ✅ `ConsumoService.cs` (lógica de negocio)
- ✅ `ConsumoViewModel.cs` (comandos, binding)
- ✅ `UserControlConsumo.xaml` (diseño con sidebar 380px)
- ✅ `ConsumoDTO.cs` (modelo de datos)

### 🔗 Dependencias:
- **DEPENDE DE:**
  - `CuentaRepository` (Jonathan) - Para buscar cuenta activa
  - `ProductoRepository` (Angel) - Para obtener productos y stock
- **LO NECESITAN:**
  - Módulo de Pagos (Norma) - Para calcular total a pagar

### ⚠️ NOTA IMPORTANTE:
**NO existe tabla `Orden`.** Todo va directo a `DetalleConsumo` vinculado a `Cuenta`.

---

## 2️⃣ MÓDULO: PAGOS Y COMPROBANTES
**Nombre en Sidebar:** "Pagos y Comprobantes"  
**UserControl:** `UserControlPago.xaml`  
**Responsable:** 👤 **NORMA ARANIBAR GROVAS**  
**Semana:** 5 (20 Nov 2025)  
**Prioridad:** 🔴 CRÍTICA  
**Complejidad:** ⭐⭐⭐ Media-Alta

### 📝 ¿Qué hace este módulo?
Permite **procesar el pago** de una cuenta cuando el cliente está listo para salir, y **generar el comprobante** (boleta o factura).

### 🎯 Funcionalidades principales:
1. **Buscar cuenta por DNI** del cliente
2. **Ver total a pagar** (con descuentos aplicados)
3. **Seleccionar método de pago** (Efectivo, Tarjeta, Yape)
4. **Registrar monto recibido**
5. **Calcular vuelto** (si es efectivo)
6. **Cambiar estado de cuenta** a "Pagada"
7. **Generar comprobante** (Boleta o Factura)
8. **Mostrar comprobante** en pantalla
9. **Imprimir comprobante** (opcional)

### 🗂️ Tablas involucradas:
- `Pago` (insertar registro de pago)
- `Comprobante` (generar boleta/factura)
- `Cuenta` (cambiar estado a "Pagada")
- `MetodoPago` (Efectivo/Tarjeta)
- `TipoComprobante` (Boleta/Factura)

### 📦 Componentes a crear:
- ✅ `PagoRepository.cs` + Interface
- ✅ `ComprobanteRepository.cs` + Interface
- ✅ `PagoService.cs` (lógica de pago + comprobante)
- ✅ `PagoViewModel.cs` (comandos de pago)
- ✅ `UserControlPago.xaml` (diseño con sidebar 380px)
- ✅ `PagoDTO.cs`, `ComprobanteDTO.cs`

### 🔗 Dependencias:
- **DEPENDE DE:**
  - `CuentaRepository` (Jonathan) - Para buscar cuenta y cambiar estado
  - `DetalleConsumoRepository` (Angel) - Para obtener detalle de consumos
- **LO NECESITAN:**
  - Módulo de Caja (Norma) - Para calcular ingresos del día
  - Módulo de Reportes (Luis) - Para reportes de ingresos

### 📄 Estructura del Comprobante:
```
================================================
          SAUNA KALIXTO
    RUC: 20XXXXXXXXX
    Av. Principal 123 - Juliaca
================================================
BOLETA ELECTRÓNICA
Serie: B001 - Nº 000123
Fecha: 27/10/2025 14:30
================================================
CLIENTE: Juan Pérez Gómez
DNI: 12345678

DETALLE:
- Sauna 2 horas        S/.20.00
- Agua mineral (2)     S/. 4.00
- Toalla               S/. 3.00
                      ---------
SUBTOTAL:              S/.27.00
DESCUENTO (10%):       S/. 2.70
                      ---------
TOTAL A PAGAR:         S/.24.30

MÉTODO DE PAGO: Efectivo
MONTO RECIBIDO:        S/.30.00
VUELTO:                S/. 5.70
================================================
Cajero: Maria Lopez
      ¡Gracias por su visita!
================================================
```

---

## 3️⃣ MÓDULO: CLIENTES
**Nombre en Sidebar:** "Clientes"  
**UserControl:** `UserControlClientes.xaml`  
**Responsable:** 👤 **JONATHAN PUMA QUISPE** ✅  
**Semana:** 2 (30 Oct 2025)  
**Prioridad:** 🔴 CRÍTICA  
**Complejidad:** ⭐⭐⭐ Media  
**Estado:** ✅ **COMPLETADO AL 100%**

### 📝 ¿Qué hace este módulo?
Permite **administrar los datos personales** de los clientes del sauna (registrar, editar, buscar, desactivar).

### 🎯 Funcionalidades principales:
1. ✅ **Registrar cliente nuevo** (nombre, apellidos, DNI, teléfono, correo, dirección)
2. ✅ **Listar todos los clientes activos** en DataGrid
3. ✅ **Buscar cliente** por DNI o nombre
4. ✅ **Editar datos del cliente**
5. ✅ **Desactivar cliente** (eliminación lógica)
6. ✅ **Ver contador de visitas totales**
7. ✅ **Asignar automáticamente** a Programa de Fidelización
8. ✅ **Status bar** con feedback en tiempo real

### 🗂️ Tablas involucradas:
- `Cliente` (CRUD completo)
- `ProgramaFidelizacion` (asignación automática)

### 📦 Componentes creados: ✅
- ✅ `ClienteRepository.cs` + Interface
- ✅ `ClienteService.cs` (con transacciones EF Core)
- ✅ `ClientesViewModel.cs` (comandos completos)
- ✅ `UserControlClientes.xaml` (sidebar 380px + DataGrid)
- ✅ `ClienteDTO.cs` (con propiedades calculadas)
- ✅ `NullToVisibilityConverter.cs` (para botón Desactivar)

### 🔗 Dependencias:
- **LO NECESITAN:**
  - Módulo de Consumos (Angel) - Para buscar cliente al agregar consumo
  - Módulo de Cuentas (Jonathan) - Para abrir cuenta de cliente
  - Módulo de Reportes (Luis) - Para historial de cliente

### ✅ **MÓDULO 100% FUNCIONAL**
Este módulo YA ESTÁ COMPLETADO y funciona perfectamente. Sirve como **referencia de diseño** para los demás módulos.

---

## 4️⃣ MÓDULO: CAJA Y FLUJO DE CAJA
**Nombre en Sidebar:** "Caja y Flujo de Caja"  
**UserControl:** `UserControlCaja.xaml`  
**Responsable:** 👤 **NORMA ARANIBAR GROVAS**  
**Semana:** 6 (27 Nov 2025)  
**Prioridad:** 🔴 CRÍTICA  
**Complejidad:** ⭐⭐⭐⭐ Alta

### 📝 ¿Qué hace este módulo?
Permite **cerrar la caja del día** y ver el **flujo de efectivo** (ingresos vs egresos).

### 🎯 Funcionalidades principales:
1. **Ver estado actual de caja** (abierta/cerrada)
2. **Calcular totales del día:**
   - Total ingresos (suma de todos los pagos)
   - Total ingresos por método de pago (Efectivo, Tarjeta)
   - Total egresos del día
   - Ganancia neta (ingresos - egresos)
3. **Mostrar resumen visual** en pantalla
4. **Generar reporte de cierre** (sin guardar en BD)
5. **Ver historial de cierres** por fecha

### 🗂️ Tablas involucradas:
⚠️ **NO hay tabla `CierreCaja`.** Todo se calcula con **queries SQL dinámicas**.

- `Pago` (leer todos los pagos del día)
- `Egreso` (leer todos los egresos del día)
- `MetodoPago` (agrupar por método)

### 📦 Componentes a crear:
- ✅ `CajaService.cs` (SIN Repository - solo queries SQL)
- ✅ `CajaViewModel.cs`
- ✅ `UserControlCaja.xaml` (dashboard con tarjetas de resumen)
- ✅ `CierreCajaDTO.cs` (modelo virtual calculado)

### 🔗 Dependencias:
- **DEPENDE DE:**
  - `PagoRepository` (Norma) - Para obtener todos los pagos del día
  - `EgresoRepository` (Norma) - Para obtener egresos del día
- **LO NECESITAN:**
  - Módulo de Reportes (Luis) - Para flujo de caja mensual

### 📊 Queries SQL necesarias:
```sql
-- Total ingresos del día
SELECT SUM(monto) AS TotalIngresos
FROM Pago
WHERE CAST(fechaHora AS DATE) = @fecha

-- Por método de pago
SELECT mp.nombre AS Metodo, SUM(p.monto) AS Total
FROM Pago p
INNER JOIN MetodoPago mp ON p.idMetodoPago = mp.idMetodoPago
WHERE CAST(p.fechaHora AS DATE) = @fecha
GROUP BY mp.nombre

-- Total egresos del día
SELECT SUM(monto) AS TotalEgresos
FROM Egreso
WHERE CAST(fecha AS DATE) = @fecha

-- Ganancia neta
SELECT 
    (SELECT SUM(monto) FROM Pago WHERE CAST(fechaHora AS DATE) = @fecha) - 
    (SELECT SUM(monto) FROM Egreso WHERE CAST(fecha AS DATE) = @fecha) 
AS GananciaNeta
```

### 📱 Diseño sugerido:
```
┌─────────────────────────────────────────────┐
│     📊 CIERRE DE CAJA - 27/10/2025          │
├─────────────────────────────────────────────┤
│ ┌─────────────┐  ┌─────────────┐           │
│ │ 💰 INGRESOS │  │ 💸 EGRESOS  │           │
│ │  S/.1,250   │  │  S/.320     │           │
│ └─────────────┘  └─────────────┘           │
│                                             │
│ 📋 DETALLE POR MÉTODO DE PAGO:              │
│ ┌───────────────────────────────┐           │
│ │ Efectivo:     S/. 850.00      │           │
│ │ Tarjeta:      S/. 300.00      │           │
│ │ Yape:         S/. 100.00      │           │
│ └───────────────────────────────┘           │
│                                             │
│ 💰 GANANCIA NETA: S/. 930.00               │
│                                             │
│ [📄 Generar Reporte] [📅 Ver Historial]    │
└─────────────────────────────────────────────┘
```

---

## 5️⃣ MÓDULO: INVENTARIO
**Nombre en Sidebar:** "Inventario"  
**UserControl:** `UserControlInventario.xaml`  
**Responsable:** 👤 **ANGEL ZUÑIGA CONDORI**  
**Semana:** 3 (6 Nov 2025)  
**Prioridad:** 🔴 CRÍTICA  
**Complejidad:** ⭐⭐⭐⭐ Alta

### 📝 ¿Qué hace este módulo?
Permite **administrar productos y servicios**, controlar el **stock**, y registrar **entradas/salidas de inventario**.

### 🎯 Funcionalidades principales:
1. **CRUD de Productos:**
   - Registrar producto (código, nombre, precio compra, precio venta, stock, categoría)
   - Editar producto
   - Listar todos los productos
   - Buscar por código o nombre
   - **Alerta visual cuando stock ≤ stockMinimo** (fondo rojo)
2. **CRUD de Servicios:**
   - Registrar servicio (nombre, descripción, precio, duración)
   - Editar servicio
   - Listar servicios
3. **Movimientos de Inventario:**
   - Registrar entrada de inventario (compra)
   - Registrar salida de inventario (ajuste)
   - Ver historial de movimientos
4. **Reportes rápidos:**
   - Valor total del inventario
   - Productos con stock bajo

### 🗂️ Tablas involucradas:
- `Producto` (CRUD completo)
- `Servicio` (CRUD completo)
- `CategoriaProducto` (maestros)
- `MovimientoInventario` (historial)
- `TipoMovimiento` (Entrada/Salida/Ajuste)

### 📦 Componentes a crear:
- ✅ `ProductoRepository.cs` + Interface
- ✅ `ServicioRepository.cs` + Interface
- ✅ `MovimientoInventarioRepository.cs` + Interface
- ✅ `CategoriaProductoRepository.cs` + Interface
- ✅ `InventarioService.cs` (lógica de stock)
- ✅ `InventarioViewModel.cs`
- ✅ **`UserControlInventario.xaml`** (REDISEÑAR con sidebar 380px como Clientes)
- ✅ `ProductoDTO.cs`, `ServicioDTO.cs`, `MovimientoDTO.cs`

### 🔗 Dependencias:
- **DEPENDE DE:**
  - `CategoriaProducto` (datos maestros)
  - `TipoMovimiento` (datos maestros)
- **LO NECESITAN:**
  - Módulo de Consumos (Angel) - Para agregar productos a cuenta
  - Módulo de Reportes (Luis) - Para productos más vendidos

### ⚠️ PROBLEMA ACTUAL:
El `UserControlInventario.xaml` actual usa **layout vertical** (filas) en lugar de **sidebar lateral** (columnas).

**Debe rediseñarse con:**
```xml
<Grid.ColumnDefinitions>
    <ColumnDefinition Width="380"/>  <!-- Sidebar formulario -->
    <ColumnDefinition Width="12"/>   <!-- Gap -->
    <ColumnDefinition Width="*"/>    <!-- DataGrid -->
</Grid.ColumnDefinitions>
```

### 🎨 Diseño sugerido:
```
┌─────────────────────────────────────────────┐
│ [380px SIDEBAR]      │  [DataGrid Productos]│
│                      │                       │
│ Código: _______      │ Código │ Nombre │... │
│ Nombre: _______      │ P001   │ Agua   │... │
│ Precio Compra: ___   │ P002   │ Toalla │... │
│ Precio Venta: ____   │ P003   │Shampoo │... │
│ Stock: _____         │                       │
│ Stock Min: ____      │ [ALERTA: Stock bajo]  │
│ Categoría: [▼]       │ P005   │Gel ducha│...│
│                      │                       │
│ [GUARDAR] [LIMPIAR]  │                       │
│ [ENTRADA STOCK]      │                       │
└─────────────────────────────────────────────┘
```

---

## 6️⃣ MÓDULO: EGRESOS
**Nombre en Sidebar:** "Egresos"  
**UserControl:** `UserControlEgresos.xaml`  
**Responsable:** 👤 **NORMA ARANIBAR GROVAS**  
**Semana:** 5 (20 Nov 2025)  
**Prioridad:** 🟡 MEDIA  
**Complejidad:** ⭐⭐ Baja

### 📝 ¿Qué hace este módulo?
Permite **registrar los gastos operativos** del sauna (compras, servicios, salarios, etc.).

### 🎯 Funcionalidades principales:
1. **Registrar egreso** (concepto, monto, tipo, fecha)
2. **Listar todos los egresos**
3. **Buscar egreso** por fecha o concepto
4. **Editar egreso**
5. **Ver total de egresos por día/mes**
6. **Filtrar por tipo de egreso**

### 🗂️ Tablas involucradas:
- `Egreso` (CRUD completo)
- `TipoEgreso` (maestros: Compras, Servicios, Salarios, Mantenimiento)

### 📦 Componentes a crear:
- ✅ `EgresoRepository.cs` + Interface
- ✅ `TipoEgresoRepository.cs` + Interface
- ✅ `EgresoService.cs`
- ✅ `EgresosViewModel.cs`
- ✅ `UserControlEgresos.xaml` (sidebar 380px)
- ✅ `EgresoDTO.cs`, `TipoEgresoDTO.cs`

### 🔗 Dependencias:
- **DEPENDE DE:**
  - `TipoEgreso` (datos maestros)
- **LO NECESITAN:**
  - Módulo de Caja (Norma) - Para calcular ganancia neta
  - Módulo de Reportes (Luis) - Para reporte de egresos

### 🎨 Diseño sugerido:
```
┌─────────────────────────────────────────────┐
│ [380px SIDEBAR]      │  [DataGrid Egresos]  │
│                      │                       │
│ Concepto: _______    │ Fecha│Concepto│Monto │
│ Monto: S/._______    │27/10 │Compra  │S/.50 │
│ Tipo: [▼]            │26/10 │Servicio│S/.80 │
│ Fecha: [📅]          │25/10 │Salario │S/.500│
│                      │                       │
│ [REGISTRAR]          │ TOTAL: S/. 630       │
│ [LIMPIAR]            │                       │
└─────────────────────────────────────────────┘
```

---

## 7️⃣ MÓDULO: REPORTES Y ESTADÍSTICAS
**Nombre en Sidebar:** "Reportes y Estadísticas"  
**UserControl:** `UserControlReporte.xaml`  
**Responsable:** 👤 **LUIS VEGA BENITES**  
**Semana:** 7-8 (4 Dic 2025)  
**Prioridad:** 🟡 MEDIA  
**Complejidad:** ⭐⭐⭐⭐⭐ Muy Alta

### 📝 ¿Qué hace este módulo?
Permite **generar reportes y estadísticas** del negocio usando **queries SQL dinámicas** (sin tabla Reporte).

### 🎯 Funcionalidades principales:

#### **A. Reporte de Ingresos:**
- Total ingresos del día/semana/mes
- Ingresos por rango de fechas
- Desglose por método de pago
- Gráfico de ingresos (opcional con LiveCharts)

#### **B. Reporte de Egresos:**
- Total egresos del mes
- Desglose por tipo de egreso
- Gráfico circular de egresos

#### **C. Reporte de Inventario:**
- Top 10 productos más vendidos
- Productos con stock bajo
- Valor total del inventario actual

#### **D. Reporte de Flujo de Caja:**
- Flujo mensual (ingresos - egresos)
- Utilidad neta
- Comparación mes actual vs anterior

#### **E. Reporte de Clientes:**
- Clientes más frecuentes
- Historial de visitas de un cliente específico
- Clientes próximos a beneficio de fidelización

### 🗂️ Tablas involucradas:
⚠️ **NO hay tabla `Reporte`.** Todo se genera con **queries SQL dinámicas**.

- `Pago` (ingresos)
- `Egreso` (gastos)
- `DetalleConsumo` (productos vendidos)
- `Cliente` (historial de visitas)
- `Cuenta` (cuentas cerradas)

### 📦 Componentes a crear:
- ✅ `ReporteService.cs` (SIN Repository - solo queries SQL)
- ✅ `ReporteViewModel.cs`
- ✅ `UserControlReporte.xaml` (dashboard con pestañas)
- ✅ `ReporteIngresoDTO.cs`, `ReporteEgresoDTO.cs`, `ReporteProductoDTO.cs`, `FlujoCajaDTO.cs`

### 🔗 Dependencias:
- **DEPENDE DE:**
  - TODAS las tablas transaccionales (Pago, Egreso, Cuenta, DetalleConsumo, Cliente)

### 📊 Queries SQL necesarias:
```sql
-- Ingresos del día
SELECT CAST(fechaHora AS DATE) as Fecha, SUM(monto) as Total
FROM Pago
WHERE CAST(fechaHora AS DATE) = @fecha
GROUP BY CAST(fechaHora AS DATE)

-- Top productos vendidos
SELECT TOP 10 p.nombre, SUM(dc.cantidad) as TotalVendido, SUM(dc.subtotal) as Ingresos
FROM DetalleConsumo dc
INNER JOIN Producto p ON dc.idProducto = p.idProducto
GROUP BY p.nombre
ORDER BY TotalVendido DESC

-- Flujo de caja mensual
SELECT 
    (SELECT SUM(monto) FROM Pago WHERE MONTH(fechaHora) = @mes) as TotalIngresos,
    (SELECT SUM(monto) FROM Egreso WHERE MONTH(fecha) = @mes) as TotalEgresos,
    (TotalIngresos - TotalEgresos) as UtilidadNeta

-- Historial de cliente
SELECT c.fechaApertura, c.fechaCierre, c.montoTotal, 
       (SELECT COUNT(*) FROM DetalleConsumo WHERE idCuenta = c.idCuenta) as TotalConsumos
FROM Cuenta c
WHERE c.idCliente = @idCliente AND c.estado = 'Pagada'
ORDER BY c.fechaCierre DESC
```

### 🎨 Diseño sugerido (con pestañas):
```
┌─────────────────────────────────────────────┐
│ [Ingresos] [Egresos] [Inventario] [Clientes]│
├─────────────────────────────────────────────┤
│ 📅 Fecha: [▼ Hoy] [▼ Esta Semana] [▼ Mes]  │
│                                             │
│ 💰 TOTAL INGRESOS: S/. 1,250.00            │
│                                             │
│ 📊 POR MÉTODO DE PAGO:                      │
│ ┌────────────────────────────┐              │
│ │ Efectivo:    S/. 850.00    │              │
│ │ Tarjeta:     S/. 300.00    │              │
│ │ Yape:        S/. 100.00    │              │
│ └────────────────────────────┘              │
│                                             │
│ 📈 [GRÁFICO DE INGRESOS]                    │
│                                             │
│ [📄 Exportar PDF] [📊 Exportar Excel]      │
└─────────────────────────────────────────────┘
```

---

## 8️⃣ MÓDULO: USUARIOS
**Nombre en Sidebar:** "Usuarios"  
**UserControl:** `UserControlUsuarios.xaml`  
**Responsable:** 👤 **LUIS VEGA BENITES**  
**Semana:** 1 + mejora en Semana 8  
**Prioridad:** 🔴 CRÍTICA (Login) + 🟡 MEDIA (Gestión)  
**Complejidad:** ⭐⭐⭐ Media

### 📝 ¿Qué hace este módulo?
Permite **administrar usuarios del sistema** (cajeros, administradores) y controlar el **login/logout**.

### 🎯 Funcionalidades principales:

#### **Fase 1: Login (Semana 1)** ✅ PRIORIDAD MÁXIMA
1. ✅ **Login funcional con BD**
2. ✅ **Validar usuario y contraseña**
3. ✅ **Encriptar contraseñas** (SHA256 o BCrypt)
4. ✅ **Guardar sesión** en `CurrentUser` singleton
5. ✅ **Redireccionar a MainWindow** tras login exitoso
6. ✅ **Logout**

#### **Fase 2: Gestión de Usuarios (Semana 8)** 🟡
1. **CRUD de usuarios** (solo Administrador)
2. **Asignar roles** (Administrador, Cajero)
3. **Cambiar contraseña**
4. **Desactivar usuario** (eliminación lógica)
5. **Ver usuarios activos**

### 🗂️ Tablas involucradas:
- `Usuario` (CRUD completo)
- `Rol` (maestros: Administrador, Cajero)

### 📦 Componentes a crear:

**Fase 1 (Semana 1):**
- ✅ `UsuarioRepository.cs` + Interface
- ✅ `RolRepository.cs` + Interface
- ✅ `AuthenticationService.cs` (login/logout)
- ✅ `PasswordHelper.cs` (encriptación)
- ✅ `CurrentUser.cs` (singleton de sesión)
- ✅ `LoginViewModel.cs` (mejorar actual)
- ✅ `UsuarioDTO.cs`, `LoginDTO.cs`

**Fase 2 (Semana 8):**
- ✅ `UsuarioService.cs` (lógica CRUD)
- ✅ `UsuariosViewModel.cs`
- ✅ **`UserControlUsuarios.xaml`** (REDISEÑAR con sidebar 380px)

### 🔗 Dependencias:
- **LO NECESITAN:**
  - TODOS los módulos (para validar permisos)
  - Módulo de Pagos (para registrar usuario que hizo el cobro)
  - Módulo de Caja (para registrar usuario que cerró caja)

### ⚠️ PROBLEMA ACTUAL:
El `UserControlUsuarios.xaml` actual usa **columnas proporcionales** (`Width="*"` y `Width="2*"`) en lugar de ancho fijo.

**Debe rediseñarse con:**
```xml
<Grid.ColumnDefinitions>
    <ColumnDefinition Width="380"/>  <!-- Sidebar fijo -->
    <ColumnDefinition Width="12"/>   
    <ColumnDefinition Width="*"/>    <!-- DataGrid -->
</Grid.ColumnDefinitions>
```

### 🎨 Diseño sugerido:
```
┌─────────────────────────────────────────────┐
│ [380px SIDEBAR]      │  [DataGrid Usuarios] │
│                      │                       │
│ Usuario: _______     │ Usuario│Rol│Activo   │
│ Contraseña: ****     │ admin  │Adm│  ✅     │
│ Correo: _______      │ cajero1│Caj│  ✅     │
│ Rol: [▼]             │ cajero2│Caj│  ❌     │
│                      │                       │
│ ☑ Usuario activo     │                       │
│                      │                       │
│ [GUARDAR] [LIMPIAR]  │                       │
│ [CAMBIAR CONTRASEÑA] │                       │
└─────────────────────────────────────────────┘
```

---

## 9️⃣ MÓDULO: PROMOCIONES
**Nombre en Sidebar:** "Promociones"  
**UserControl:** `UserControlPromociones.xaml`  
**Responsable:** 👤 **JONATHAN PUMA QUISPE**  
**Semana:** 8-9 (Opcional)  
**Prioridad:** 🟢 BAJA (OPCIONAL)  
**Complejidad:** ⭐⭐ Baja

### 📝 ¿Qué hace este módulo?
Permite **ver y gestionar el programa de fidelización** y promociones especiales.

### 🎯 Funcionalidades principales:
1. **Ver configuración** del Programa de Fidelización
   - Descuento por 5ta visita (10%)
   - Descuento por cumpleaños (S/.5)
2. **Ver clientes próximos a beneficio** (4 visitas)
3. **Ver clientes con cumpleaños del mes**
4. **Aplicar descuento manual** (si es necesario)
5. **Ver historial de beneficios aplicados**

### 🗂️ Tablas involucradas:
- `ProgramaFidelizacion` (solo 1 registro - configuración global)
- `Cliente` (ver visitasTotales y fechaNacimiento)
- `Cuenta` (aplicar descuento)

### 📦 Componentes a crear:
- ✅ `PromocionService.cs` (lógica de fidelización)
- ✅ `PromocionViewModel.cs`
- ✅ `UserControlPromociones.xaml` (dashboard informativo)
- ✅ `PromocionDTO.cs`

### 🔗 Dependencias:
- **DEPENDE DE:**
  - `ClienteRepository` (Jonathan)
  - `ProgramaFidelizacionRepository` (Jonathan)
- **LO NECESITAN:**
  - Módulo de Cuentas (Jonathan) - Para aplicar descuento automático

### ⚠️ NOTA IMPORTANTE:
**La lógica de fidelización se aplica automáticamente** en `CuentaService` al crear/cerrar cuenta. Este módulo es solo para **visualización y configuración**.

### 🎨 Diseño sugerido:
```
┌─────────────────────────────────────────────┐
│     🎁 PROGRAMA DE FIDELIZACIÓN             │
├─────────────────────────────────────────────┤
│ 📋 CONFIGURACIÓN ACTUAL:                    │
│ • Descuento 5ta visita: 10%                 │
│ • Descuento cumpleaños: S/.5                │
│                                             │
│ 👥 CLIENTES PRÓXIMOS A BENEFICIO (4 visitas):│
│ ┌───────────────────────────────┐           │
│ │ Juan Pérez    | 4 visitas     │           │
│ │ Maria Lopez   | 4 visitas     │           │
│ │ Carlos Gomez  | 4 visitas     │           │
│ └───────────────────────────────┘           │
│                                             │
│ 🎂 CUMPLEAÑOS ESTE MES:                     │
│ ┌───────────────────────────────┐           │
│ │ Ana Torres    | 05 Nov        │           │
│ │ Luis Ramirez  | 12 Nov        │           │
│ └───────────────────────────────┘           │
└─────────────────────────────────────────────┘
```

---

# 📋 RESUMEN EJECUTIVO DE ASIGNACIONES

## 👤 JONATHAN PUMA QUISPE (Scrum Master)
**Módulos:** 3 (Clientes ✅, Cuentas, Promociones)  
**Semanas:** 2, 4, 9

| # | Módulo | Semana | Estado |
|---|--------|--------|--------|
| 3️⃣ | Clientes | Semana 2 | ✅ **COMPLETADO** |
| - | Gestión de Cuentas | Semana 4 | 🔄 Pendiente |
| 9️⃣ | Promociones | Semana 9 | 🔵 Opcional |

**Responsabilidades adicionales:**
- ✅ Coordinar integración semanal
- ✅ Resolver conflictos de Git
- ✅ Code reviews
- ✅ Daily Scrum

---

## 👤 ANGEL ZUÑIGA CONDORI
**Módulos:** 2 (Inventario, Consumos)  
**Semanas:** 3, 4

| # | Módulo | Semana | Complejidad |
|---|--------|--------|-------------|
| 5️⃣ | Inventario | Semana 3 | ⭐⭐⭐⭐ Alta |
| 1️⃣ | Entradas y Consumos | Semana 4 | ⭐⭐⭐⭐ Alta |

**Tareas clave:**
- ⚠️ **REDISEÑAR** `UserControlInventario.xaml` con sidebar lateral (380px)
- ✅ Implementar alerta visual de stock bajo
- ✅ Descontar stock automáticamente al agregar consumo
- ✅ Actualizar total de cuenta en tiempo real

---

## 👤 NORMA ARANIBAR GROVAS
**Módulos:** 3 (Pagos, Egresos, Caja)  
**Semanas:** 5, 6

| # | Módulo | Semana | Complejidad |
|---|--------|--------|-------------|
| 6️⃣ | Egresos | Semana 5 | ⭐⭐ Baja |
| 2️⃣ | Pagos y Comprobantes | Semana 5 | ⭐⭐⭐ Media |
| 4️⃣ | Caja y Flujo de Caja | Semana 6 | ⭐⭐⭐⭐ Alta |

**Tareas clave:**
- ✅ Generar comprobantes (Boleta/Factura) con formato correcto
- ✅ Calcular vuelto automáticamente
- ✅ Implementar queries SQL para cierre de caja (sin tabla CierreCaja)
- ✅ Dashboard visual de cierre de caja

---

## 👤 LUIS VEGA BENITES
**Módulos:** 2 (Usuarios, Reportes)  
**Semanas:** 1, 7-8

| # | Módulo | Semana | Complejidad |
|---|--------|--------|-------------|
| 8️⃣ | Usuarios (Login) | Semana 1 | ⭐⭐⭐ Media (CRÍTICO) |
| 7️⃣ | Reportes y Estadísticas | Semanas 7-8 | ⭐⭐⭐⭐⭐ Muy Alta |
| 8️⃣ | Usuarios (Gestión) | Semana 8 | ⭐⭐ Baja (mejora) |

**Tareas clave:**
- ✅ **Login 100% funcional** (Semana 1 - PRIORIDAD MÁXIMA)
- ✅ Encriptar contraseñas con SHA256/BCrypt
- ✅ Implementar queries SQL para todos los reportes (sin tabla Reporte)
- ✅ Crear 5 tipos de reportes: Ingresos, Egresos, Inventario, Flujo, Clientes
- ⚠️ **REDISEÑAR** `UserControlUsuarios.xaml` con sidebar lateral (380px)

---

# 🎯 PRIORIDADES Y DEPENDENCIAS

## 🔴 CRÍTICO (Semanas 1-4):
1. **Login** (Luis - Semana 1) → TODO depende de esto
2. **Clientes** (Jonathan - Semana 2) ✅ LISTO
3. **Inventario** (Angel - Semana 3) → Consumos depende de esto
4. **Consumos** (Angel - Semana 4) → Pagos depende de esto
5. **Cuentas** (Jonathan - Semana 4) → Pagos depende de esto

## 🟡 IMPORTANTE (Semanas 5-6):
6. **Pagos** (Norma - Semana 5) → Caja depende de esto
7. **Egresos** (Norma - Semana 5) → Caja depende de esto
8. **Caja** (Norma - Semana 6) → Reportes depende de esto

## 🟢 SECUNDARIO (Semanas 7-9):
9. **Reportes** (Luis - Semanas 7-8)
10. **Usuarios Gestión** (Luis - Semana 8)
11. **Promociones** (Jonathan - Semana 9) → OPCIONAL

---

# 📊 DIAGRAMA DE DEPENDENCIAS

```
        ┌───────────┐
        │  LOGIN    │ ← Luis (Semana 1) CRÍTICO
        │ (Semana 1)│
        └─────┬─────┘
              │
      ┌───────┴────────┐
      │                │
┌─────▼─────┐   ┌─────▼─────┐
│ CLIENTES  │   │INVENTARIO │ ← Angel (Semana 3)
│ (Semana 2)│   │(Semana 3) │
└─────┬─────┘   └─────┬─────┘
      │               │
      │         ┌─────▼─────┐
      │         │ CONSUMOS  │ ← Angel (Semana 4)
      │         │(Semana 4) │
      │         └─────┬─────┘
      │               │
┌─────▼─────┐   ┌────▼──────┐
│  CUENTAS  │───┤  PAGOS    │ ← Norma (Semana 5)
│ (Semana 4)│   │(Semana 5) │
└───────────┘   └─────┬─────┘
                      │
                ┌─────▼─────┐
                │  EGRESOS  │ ← Norma (Semana 5)
                │(Semana 5) │
                └─────┬─────┘
                      │
                ┌─────▼─────┐
                │   CAJA    │ ← Norma (Semana 6)
                │(Semana 6) │
                └─────┬─────┘
                      │
                ┌─────▼─────┐
                │ REPORTES  │ ← Luis (Semanas 7-8)
                │(Semana 7-8)│
                └───────────┘

              ┌───────────┐
              │ USUARIOS  │ ← Luis (Semana 8 - mejora)
              │(Semana 8) │
              └───────────┘

              ┌───────────┐
              │PROMOCIONES│ ← Jonathan (Semana 9 - opcional)
              │(Semana 9) │
              └───────────┘
```

---

# ✅ CHECKLIST DE ENTREGABLES POR MÓDULO

Cada módulo debe entregar:

1. ✅ **Repositorio(s) + Interfaces** (si aplica)
2. ✅ **Service + Interface** (lógica de negocio)
3. ✅ **ViewModel** (comandos, binding)
4. ✅ **DTO(s)** (modelos de transferencia)
5. ✅ **UserControl.xaml** (diseño con sidebar 380px)
6. ✅ **Funcionalidades CORE** probadas y funcionando
7. ✅ **Demo en vivo** el miércoles de entrega
8. ✅ **Código en repositorio Git** antes de las 11:59 PM del miércoles

---

# 🚨 ADVERTENCIAS IMPORTANTES

## ⚠️ PARA ANGEL:
- **REDISEÑAR** `UserControlInventario.xaml` con sidebar lateral de 380px (actualmente usa layout vertical)
- Cambiar colores: `#00E5FF` → `#4CC9F0`, `#0F1117` → `#14161C`
- Usar el diseño de `UserControlClientes.xaml` como referencia

## ⚠️ PARA LUIS:
- **PRIORIDAD MÁXIMA:** Login funcional en Semana 1 (TODO depende de esto)
- **REDISEÑAR** `UserControlUsuarios.xaml` con sidebar lateral de 380px en Semana 8
- Reportes son complejos (queries SQL avanzadas) - empezar temprano

## ⚠️ PARA NORMA:
- **NO existe tabla `CierreCaja`** - usar queries SQL dinámicas
- Generar comprobante con formato profesional (ver ejemplo en módulo 2)
- Calcular vuelto automáticamente si pago es en efectivo

## ⚠️ PARA JONATHAN:
- **Módulo Clientes YA ESTÁ LISTO** ✅ - enfocarse en Cuentas (Semana 4)
- **NO existe tabla `Entrada` ni `Orden`** - todo se maneja desde `Cuenta` y `DetalleConsumo`
- Promociones es opcional (solo si hay tiempo en Semana 9)

---

# 📞 CONTACTO Y COORDINACIÓN

**Reuniones semanales:**
- **Lunes 9:00 AM:** Planning semanal
- **Miércoles 11:00 AM:** Demo de entregables
- **Viernes 2:00 PM:** Presentación al profesor
- **Daily Scrum:** Todos los días 8:30 AM (15 minutos)

**Canal de comunicación:** WhatsApp grupo "Proyecto Sauna Kalixto"

---

**Documento creado:** 27 de Octubre, 2025  
**Versión:** 1.0  
**Próxima revisión:** Después de cada Sprint

---

**¡ÉXITO EN EL DESARROLLO! 🚀**
