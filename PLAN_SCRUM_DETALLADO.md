# 🏃‍♂️ PLAN SCRUM INTENSIVO - PROYECTO SAUNA KALIXTO

**Proyecto:** Sistema de Gestión Sauna KALIXTO  
**Metodología:** Scrum Adaptado - ENTREGA DICIEMBRE 2025  
**Duración Total:** 9 SEMANAS (2.5 meses)  
**Fecha Inicio:** 14 de Octubre, 2025  
**Fecha Fin:** 12 de Diciembre, 2025  
**⚠️ MODO INTENSIVO:** Entregas semanales obligatorias cada MIÉRCOLES  
**🎯 PRESENTACIONES FINALES:** Viernes de cada semana

---

## ⚠️ CONTEXTO Y RESTRICCIONES

### Estado Actual del Proyecto
- ✅ **Maqueta UI creada:** 13 UserControls con diseño
- ✅ **Login básico:** Funcional pero sin lógica completa
- ✅ **Base de datos:** Optimizada a **17 tablas** (sin Reportes, CierreCaja, Entrada, Orden)
- ✅ **Entidades:** Generadas con EF Core (17 entidades)
- 🔴 **Lógica de negocio:** 0% (TODO POR HACER)
- 🔴 **ViewModels:** 0% funcionales
- 🔴 **Services/Repositories:** 0% (17 repos necesarios)

### ⚠️ CAMBIO IMPORTANTE: Modelo Reducido
- **Antes:** 25 tablas (modelo original)
- **Ahora:** **17 tablas** (modelo optimizado)
- **Eliminadas:** Reporte, TipoReporte, CierreCaja, FlujoCaja, Entrada, Orden, EstadoEntrada, EstadoOrden
- **Solución:** Reportes y cierres se calculan con **queries SQL dinámicas**

### Restricciones de Tiempo
- 📅 **9 semanas disponibles** (hasta 12 Dic 2025)
- 🚨 **Entregas obligatorias:** Cada miércoles
- 🎤 **Presentaciones:** Cada viernes
- ⏰ **Integración:** Lunes y martes para juntar módulos

### Estrategia de Desarrollo
1. **Semanas 1-2:** Infraestructura base + módulos críticos
2. **Semanas 3-6:** Desarrollo paralelo de módulos core
3. **Semanas 7-8:** Integración + features secundarias
4. **Semana 9:** Testing + presentación final

---

## 👥 EQUIPO DE DESARROLLO

### 🎯 Roles y Responsabilidades

| Rol | Nombre | Responsabilidad Principal | Carga Trabajo |
|-----|--------|---------------------------|---------------|
| **Scrum Master** | Jonathan Puma Quispe | Coordinar, facilitar, integrar | 100% |
| **Developer 1** | Jonathan Puma Quispe | Clientes + Cuentas (4 tablas) | 110% |
| **Developer 2** | Angel Zuñiga Condori | Inventario + Consumo (5 tablas) | 120% |
| **Developer 3** | Norma Aranibar Grovas | Pagos + Egresos + Cierre (5 tablas + queries) | 115% |
| **Developer 4** | Luis Vega Benites | Login + Usuarios + Reportes (2 tablas + queries) | 105% |

---

## 📊 DISTRIBUCIÓN DE MÓDULOS POR DESARROLLADOR (PRIORIZADO)

### 🎯 MÓDULOS CORE (PRIORIDAD ALTA - Semanas 1-4)
Estos módulos son críticos y deben funcionar al 100%

### 👤 **JONATHAN PUMA QUISPE** (Scrum Master + Developer)

#### 1️⃣ MÓDULO: GESTIÓN DE CLIENTES (CRÍTICO)
**Sprint:** Semana 2  
**Prioridad:** 🔴 ALTA  
**Complejidad:** Media  
**Entrega:** Miércoles 30 Oct

**Tablas Asignadas (4):**
- Cliente
- Cuenta  
- ProgramaFidelizacion
- EstadoCuenta

**Componentes mínimos:**
- ✅ ClienteRepository + Interface (CRUD básico)
- ✅ ProgramaFidelizacionRepository + Interface
- ✅ EstadoCuentaRepository + Interface
- ✅ ClienteService + Interface (lógica básica)
- ✅ ClientesViewModel (funcional)
- ✅ UserControlClientes conectado con binding
- ✅ DTOs: ClienteDTO, ProgramaFidelizacionDTO

**Funcionalidades CORE (entregables):**
- ✅ Registrar cliente (nombre, apellidos, DNI, teléfono)
- ✅ Listar clientes en DataGrid
- ✅ Buscar cliente por DNI o nombre
- ✅ Editar cliente (básico)
- ✅ Validación de DNI (8 dígitos)

**Funcionalidades OPCIONAL (si hay tiempo):**
- ⚠️ Ver historial de visitas
- ⚠️ Desactivar cliente

**Dependencias:** Base de datos creada

---

#### 2️⃣ MÓDULO: GESTIÓN DE CUENTAS
**Sprint:** Semana 4  
**Prioridad:** 🔴 ALTA  
**Complejidad:** Alta  
**Entrega:** Miércoles 13 Nov

**Componentes mínimos:**
- ✅ CuentaRepository + Interface
- ✅ CuentaService + Interface
- ✅ CuentasViewModel
- ✅ UserControlCuentas conectado

**Funcionalidades CORE:**
- ✅ Crear cuenta cuando cliente ingresa
- ✅ Ver cuentas pendientes
- ✅ Ver detalle de cuenta (consumos agregados)
- ✅ Calcular total cuenta (subtotal + descuentos)
- ✅ Aplicar descuentos por fidelización (5ta visita)
- ✅ Cambiar estado: Pendiente → Pagada

**Funcionalidades OPCIONAL:**
- ⚠️ Ver historial de cuentas cerradas
- ⚠️ Filtros por fecha

**Dependencias:** Cliente, DetalleConsumo (Angel)

**⚠️ NOTA:** Ya NO existe tabla `Entrada` ni `Orden`. Todo se maneja desde `Cuenta` y `DetalleConsumo`

---

#### 3️⃣ MÓDULO: INTEGRACIÓN Y COORDINACIÓN (CONTINUO)
**Sprint:** Todas las semanas  
**Prioridad:** � CRÍTICO  
**Complejidad:** Alta  

**Responsabilidades:**
- ✅ Resolver conflictos de Git
- ✅ Integrar módulos de todos los developers
- ✅ Validar flujos completos
- ✅ Code reviews
- ✅ Daily Scrum
- ✅ Ayudar a developers bloqueados

**Nota:** Jonathan NO tiene módulo de Reportes. Eso lo hace Luis con queries SQL.

---

### 👤 **ANGEL ZUÑIGA CONDORI**

#### 4️⃣ MÓDULO: INVENTARIO (CRÍTICO)
**Sprint:** Semana 3  
**Prioridad:** 🔴 ALTA  
**Complejidad:** Alta  
**Entrega:** Miércoles 6 Nov

**Tablas Asignadas (5):**
- Producto
- MovimientoInventario
- DetalleConsumo
- CategoriaProducto
- TipoMovimiento

**Componentes mínimos:**
- ✅ ProductoRepository + Interface
- ✅ MovimientoInventarioRepository + Interface
- ✅ CategoriaProductoRepository + Interface
- ✅ TipoMovimientoRepository + Interface
- ✅ InventarioService + Interface
- ✅ InventarioViewModel
- ✅ UserControlInventario conectado
- ✅ DTOs: ProductoDTO (sin precioAlquiler), MovimientoDTO, CategoriaProductoDTO

**Funcionalidades CORE:**
- ✅ Registrar producto (código, nombre, precio compra/venta, stock, categoría)
- ✅ Listar productos en DataGrid
- ✅ Buscar producto por código o nombre
- ✅ Editar producto
- ✅ Registrar entrada de inventario (aumentar stock)
- ✅ Ver stock actual
- ✅ **ALERTA visual cuando stock <= stockMinimo**

**Funcionalidades OPCIONAL:**
- ⚠️ Historial de movimientos
- ⚠️ Reportes de margen

**Dependencias:** Categorías (datos maestros)

---

#### 5️⃣ MÓDULO: CONSUMO EN TIEMPO REAL (CRÍTICO)
**Sprint:** Semana 4  
**Prioridad:** 🔴 ALTA  
**Complejidad:** Alta  
**Entrega:** Miércoles 13 Nov

**Componentes mínimos:**
- ✅ DetalleConsumoRepository + Interface (ya incluido en Inventario)
- ✅ ConsumoService + Interface
- ✅ ConsumoViewModel
- ✅ UserControlConsumo conectado

**Funcionalidades CORE:**
- ✅ Buscar cliente con cuenta activa (por DNI)
- ✅ Agregar productos directamente a DetalleConsumo
- ✅ Ver lista de consumos de la cuenta
- ✅ Calcular subtotal automático
- ✅ **Actualizar total de cuenta en tiempo real**
- ✅ **Descontar stock automáticamente al agregar consumo**
- ✅ Eliminar consumo (devolver stock)

**Funcionalidades OPCIONAL:**
- ⚠️ Editar cantidad de consumo
- ⚠️ Historial de consumos

**Dependencias:** Cuenta (Jonathan), Producto (Angel)

**⚠️ NOTA:** Ya NO existe tabla `Orden`. Todo va directo a `DetalleConsumo` vinculado a `Cuenta`

---

#### 6️⃣ MÓDULO: PAGOS Y COMPROBANTES (CRÍTICO)
**Sprint:** Semana 5  
**Prioridad:** 🔴 ALTA  
**Complejidad:** Media  
**Entrega:** Miércoles 20 Nov

**⚠️ REASIGNADO A NORMA** (Angel se enfoca solo en Inventario + Consumo)

**Tablas Asignadas (5):**
- Pago
- Comprobante
- MetodoPago
- TipoComprobante
- (más Egreso y TipoEgreso en semana 6)

**Componentes mínimos:**
- ✅ PagoRepository + Interface
- ✅ ComprobanteRepository + Interface
- ✅ MetodoPagoRepository + Interface
- ✅ TipoComprobanteRepository + Interface
- ✅ PagoService + Interface
- ✅ ComprobanteService + Interface
- ✅ PagoViewModel
- ✅ UserControlPago conectado

**Funcionalidades CORE:**
- ✅ Ver total de cuenta a pagar
- ✅ Seleccionar método de pago (Efectivo/Tarjeta)
- ✅ Registrar pago
- ✅ Generar comprobante (Boleta o Factura)
- ✅ Cambiar estado cuenta a "Pagada"
- ✅ Imprimir/Mostrar comprobante

**Funcionalidades OPCIONAL:**
- ⚠️ Calcular vuelto (si efectivo)
- ⚠️ Serie y numeración automática

**Dependencias:** Cuenta (Jonathan)

---

### 👤 **NORMA ARANIBAR GROVAS**

#### 7️⃣ MÓDULO: CIERRE DE CAJA (CRÍTICO)
**Sprint:** Semana 6  
**Prioridad:** 🔴 ALTA  
**Complejidad:** Alta  
**Entrega:** Miércoles 27 Nov

**Componentes mínimos:**
- ✅ CajaService + Interface (**SIN Repository - solo queries**)
- ✅ CajaViewModel
- ✅ UserControlCaja conectado
- ✅ DTOs: CierreCajaDTO (virtual, calculado)

**⚠️ IMPORTANTE:** Ya NO existe tabla `CierreCaja`. Todo se calcula con queries SQL dinámicas.

**Funcionalidades CORE (TODO POR QUERIES):**
- ✅ Calcular totales del día:
  - Total ingresos (SUM de Pagos del día)
  - Total por método de pago (Efectivo, Tarjeta)
  - Total egresos del día
  - Ganancia neta (ingresos - egresos)
- ✅ Mostrar resumen en pantalla
- ✅ Generar reporte visual (sin guardar en BD)
- ✅ Ver historial de cierres (query por fecha)

**Queries SQL Necesarias:**
```sql
-- Total ingresos del día
SELECT SUM(monto) FROM Pago WHERE CAST(fechaHora AS DATE) = @fecha

-- Por método de pago
SELECT mp.nombre, SUM(p.monto)
FROM Pago p
INNER JOIN MetodoPago mp ON p.idMetodoPago = mp.idMetodoPago
WHERE CAST(p.fechaHora AS DATE) = @fecha
GROUP BY mp.nombre

-- Total egresos del día
SELECT SUM(monto) FROM Egreso WHERE CAST(fecha AS DATE) = @fecha
```

**Funcionalidades OPCIONAL:**
- ⚠️ Exportar PDF
- ⚠️ Comparar con días anteriores

**Dependencias:** Pago, Egreso

---

#### 8️⃣ MÓDULO: EGRESOS (SECUNDARIO)
**Sprint:** Semana 5  
**Prioridad:** 🟡 MEDIA  
**Complejidad:** Baja  
**Entrega:** Miércoles 20 Nov

**Tablas Asignadas (2):**
- Egreso
- TipoEgreso

**Componentes mínimos:**
- ✅ EgresoRepository + Interface
- ✅ TipoEgresoRepository + Interface
- ✅ EgresoService + Interface
- ✅ EgresosViewModel
- ✅ UserControlEgresos conectado
- ✅ DTOs: EgresoDTO, TipoEgresoDTO

**Funcionalidades CORE:**
- ✅ Registrar egreso (concepto, monto, tipo, fecha)
- ✅ Listar egresos
- ✅ Ver total de egresos por fecha
- ✅ Filtrar por tipo de egreso

**Funcionalidades OPCIONAL:**
- ⚠️ Editar egreso
- ⚠️ Egresos recurrentes

**Dependencias:** TipoEgreso (datos maestros)

---

#### 9️⃣ MÓDULO: PAGOS Y COMPROBANTES (REASIGNADO)
**Sprint:** Semana 5  
**Prioridad:** 🔴 ALTA  
**Complejidad:** Media  
**Entrega:** Miércoles 20 Nov

Ver detalles en sección de Angel (módulo 6). Norma toma este módulo.

---

### 👤 **LUIS VEGA BENITES**

#### 🔟 MÓDULO: USUARIOS Y LOGIN (CRÍTICO)
**Sprint:** Semana 1  
**Prioridad:** 🔴 CRÍTICA  
**Complejidad:** Media  
**Entrega:** Miércoles 23 Oct

**Tablas Asignadas (2):**
- Usuario
- Rol

**Componentes mínimos:**
- ✅ UsuarioRepository + Interface
- ✅ RolRepository + Interface
- ✅ AuthenticationService + Interface
- ✅ UsuarioService + Interface
- ✅ LoginViewModel (mejorar actual)
- ✅ UsuariosViewModel
- ✅ CurrentUser singleton (sesión)
- ✅ PasswordHelper (encriptación BCrypt)
- ✅ DTOs: UsuarioDTO, LoginDTO, RolDTO

**Funcionalidades CORE:**
- ✅ **Login mejorado con validación real BD**
- ✅ Encriptar contraseñas (SHA256 mínimo)
- ✅ Guardar sesión (CurrentUser)
- ✅ Logout
- ✅ CRUD de usuarios (solo admin)
- ✅ Asignar roles (Administrador/Recepcionista)
- ✅ Validar permisos básicos

**Funcionalidades OPCIONAL:**
- ⚠️ Cambiar contraseña
- ⚠️ Recuperar contraseña

**Dependencias:** Base de datos

---

#### 1️⃣1️⃣ MÓDULO: REPORTES (SECUNDARIO) - **POR QUERIES SQL**
**Sprint:** Semana 7-8  
**Prioridad:** � MEDIA  
**Complejidad:** Alta  
**Entrega:** Miércoles 4 Dic

**⚠️ IMPORTANTE:** Ya NO existe tabla `Reporte` ni `TipoReporte`. Todo se genera con queries SQL dinámicas.

**Componentes mínimos:**
- ✅ ReporteService + Interface (**SIN Repository - solo queries**)
- ✅ ReporteViewModel
- ✅ UserControlReporte conectado
- ✅ DTOs: ReporteIngresoDTO, ReporteEgresoDTO, ReporteProductoDTO, FlujoCajaDTO

**Funcionalidades CORE (TODO POR QUERIES):**

**A. Reporte de Ingresos:**
```sql
-- Ingresos del día/rango
SELECT CAST(fechaHora AS DATE) as Fecha, SUM(monto) as Total
FROM Pago
WHERE fechaHora BETWEEN @fechaInicio AND @fechaFin
GROUP BY CAST(fechaHora AS DATE)
```
- ✅ Ingresos del día
- ✅ Ingresos por rango de fechas
- ✅ Desglose por método de pago
- ✅ Gráfico de ingresos (LiveCharts)

**B. Reporte de Egresos:**
```sql
-- Egresos por tipo
SELECT te.nombre, SUM(e.monto) as Total
FROM Egreso e
INNER JOIN TipoEgreso te ON e.idTipoEgreso = te.idTipoEgreso
WHERE fecha BETWEEN @fechaInicio AND @fechaFin
GROUP BY te.nombre
```
- ✅ Egresos del mes
- ✅ Desglose por tipo
- ✅ Gráfico de egresos

**C. Reporte de Inventario:**
```sql
-- Productos más vendidos
SELECT TOP 10 p.nombre, SUM(dc.cantidad) as TotalVendido
FROM DetalleConsumo dc
INNER JOIN Producto p ON dc.idProducto = p.idProducto
GROUP BY p.nombre
ORDER BY TotalVendido DESC
```
- ✅ Productos con stock bajo
- ✅ Top 10 productos más vendidos
- ✅ Valor total del inventario

**D. Reporte de Flujo de Caja:**
```sql
-- Flujo mensual
SELECT 
    (SELECT SUM(monto) FROM Pago WHERE MONTH(fechaHora) = @mes) as TotalIngresos,
    (SELECT SUM(monto) FROM Egreso WHERE MONTH(fecha) = @mes) as TotalEgresos,
    (TotalIngresos - TotalEgresos) as UtilidadNeta
```
- ✅ Flujo mensual calculado
- ✅ Saldo inicial y final
- ✅ Utilidad neta
- ✅ Gráfico de flujo

**Funcionalidades OPCIONAL:**
- ⚠️ Exportar a PDF
- ⚠️ Gráficos avanzados

**Dependencias:** Todas las tablas transaccionales

---

#### 1️⃣2️⃣ MÓDULO: FIDELIZACIÓN Y PROMOCIONES (OPCIONAL)
**Sprint:** Semana 8  
**Prioridad:** 🟢 BAJA  
**Complejidad:** Baja  
**Entrega:** Miércoles 4 Dic (si hay tiempo)

**Funcionalidades CORE:**
- ✅ Ver configuración de programa de fidelización
- ✅ Ver clientes próximos a 5ta visita
- ✅ Aplicar descuento por cumpleaños (manual)
- ✅ Ver clientes con cumpleaños del mes

**Funcionalidades OPCIONAL:**
- ⚠️ Notificaciones automáticas
- ⚠️ Historial de beneficios aplicados

**Dependencias:** Cliente, ProgramaFidelizacion

**⚠️ NOTA:** Ya NO existe tabla `Entrada`. La lógica de fidelización se maneja en `CuentaService` al crear cuenta.

**Dependencias:** Cliente, ProgramaFidelizacion

**⚠️ NOTA:** Ya NO existe tabla `Entrada`. La lógica de fidelización se maneja en `CuentaService` al crear cuenta.

---

## 📅 CALENDARIO SEMANAL DETALLADO (9 SEMANAS)

---

## 🏁 SEMANA 1: INFRAESTRUCTURA CORE
**Fechas:** 14 - 20 Octubre 2025  
**Entrega:** **Miércoles 16 Oct** (adelantada)  
**Presentación:** Viernes 18 Oct

### 🎯 OBJETIVO SEMANAL
Preparar infraestructura base + Login funcional 100%

### 📋 TAREAS POR DESARROLLADOR

#### 👤 **JONATHAN** (Scrum Master)
**Lunes-Martes:**
- [x] Ejecutar script SQL completo en servidor
- [x] Insertar datos maestros (roles, estados, categorías)
- [x] Crear estructura de carpetas: `Repositories/`, `Services/`, `DTOs/`
- [x] Crear `IRepository<T>` genérico
- [x] Crear `Repository<T>` base

**Miércoles (DÍA DE ENTREGA):**
- [ ] Crear `IClienteRepository` (vacío por ahora)
- [ ] Probar conexión a BD desde proyecto
- [ ] **ENTREGA:** Infraestructura base lista

#### 👤 **ANGEL**
**Lunes-Martes:**
- [x] Crear `IProductoRepository` + `ProductoRepository` (esqueleto)
- [x] Crear `CategoriaProductoRepository`
- [x] Insertar productos de prueba en BD (10 productos barra, 10 accesorios)

**Miércoles (DÍA DE ENTREGA):**
- [ ] Crear DTOs: `ProductoDTO`, `CategoriaProductoDTO`
- [ ] **ENTREGA:** Repositorios de inventario base

#### 👤 **NORMA**
**Lunes-Martes:**
- [x] Crear `IEgresoRepository` + `EgresoRepository` (esqueleto)
- [x] Crear `TipoEgresoRepository`
- [x] Insertar tipos de egreso en BD

**Miércoles (DÍA DE ENTREGA):**
- [ ] Crear DTOs: `EgresoDTO`, `TipoEgresoDTO`
- [ ] **ENTREGA:** Repositorios de egresos base

#### 👤 **LUIS** ⚠️ CRÍTICO
**Lunes-Martes:**
- [x] Crear `PasswordHelper` (SHA256 o BCrypt)
- [x] Crear `CurrentUser` singleton
- [x] Crear `IUsuarioRepository` + `UsuarioRepository`
- [x] Crear `IAuthenticationService` + `AuthenticationService`
- [x] Encriptar contraseñas de usuarios en BD

**Miércoles (DÍA DE ENTREGA):**
- [x] **Mejorar `LoginViewModel` con validación real**
- [x] **Login 100% funcional con BD**
- [x] Guardar sesión en `CurrentUser`
- [x] Redireccionar a MainWindow tras login exitoso
- [ ] **DEMO:** Login funcionando en vivo
- [ ] **ENTREGA:** Sistema de autenticación completo

### 🎤 PRESENTACIÓN VIERNES 18 OCT
**Demostrar:**
- ✅ Base de datos creada y poblada
- ✅ Login funcional con validación
- ✅ Infraestructura de repositorios
- ✅ Encriptación de contraseñas

### 📊 MÉTRICAS SEMANA 1
- **Story Points completados:** 15 SP
- **Módulos al 100%:** Login
- **Módulos al 50%:** Infraestructura

---

## 🚀 SEMANA 2: CLIENTES + BASE DE INVENTARIO
**Fechas:** 21 - 27 Octubre 2025  
**Entrega:** **Miércoles 23 Oct**  
**Presentación:** Viernes 25 Oct

### 🎯 OBJETIVO SEMANAL
Módulo de Clientes 100% funcional + Gestión básica de usuarios

### 📋 TAREAS POR DESARROLLADOR

#### 👤 **JONATHAN** ⚠️ CRÍTICO
**Lunes:**
- [ ] Terminar `ClienteRepository` (CRUD completo)
- [ ] Crear `IClienteService` + `ClienteService`
- [ ] Implementar validaciones (DNI, correo, teléfono)

**Martes:**
- [ ] Crear `ClientesViewModel` completo
- [ ] Conectar `UserControlClientes.xaml` con ViewModel
- [ ] Implementar comandos: Agregar, Editar, Buscar
- [ ] Binding de DataGrid

**Miércoles (DÍA DE ENTREGA):**
- [ ] Testing manual de todas las funciones
- [ ] Búsqueda por DNI y nombre funcional
- [ ] **DEMO:** Registrar 5 clientes en vivo
- [ ] **ENTREGA:** Módulo Clientes 100%

#### 👤 **ANGEL**
**Lunes-Martes:**
- [ ] Completar `ProductoRepository` (métodos de búsqueda)
- [ ] Crear `IInventarioService` + `InventarioService` (básico)
- [ ] Crear `InventarioViewModel` (CRUD productos)

**Miércoles (DÍA DE ENTREGA):**
- [ ] Conectar `UserControlInventario.xaml`
- [ ] Listar productos en DataGrid
- [ ] Agregar/Editar producto funcional
- [ ] **ENTREGA:** Gestión productos al 70%

#### 👤 **NORMA**
**Lunes-Martes:**
- [ ] Completar `EgresoRepository`
- [ ] Crear `IEgresoService` + `EgresoService`
- [ ] Crear `EgresosViewModel`

**Miércoles (DÍA DE ENTREGA):**
- [ ] Conectar `UserControlEgresos.xaml`
- [ ] Registrar egreso funcional (formulario básico)
- [ ] Listar egresos
- [ ] **ENTREGA:** Módulo Egresos al 60%

#### 👤 **LUIS**
**Lunes-Martes:**
- [ ] Crear `IUsuarioService` + `UsuarioService`
- [ ] Crear `UsuariosViewModel`
- [ ] Conectar `UserControlUsuarios.xaml`

**Miércoles (DÍA DE ENTREGA):**
- [ ] CRUD de usuarios funcional
- [ ] Asignar roles
- [ ] Validar que solo admin puede acceder
- [ ] **ENTREGA:** Gestión de usuarios al 80%

### 🎤 PRESENTACIÓN VIERNES 25 OCT
**Demostrar:**
- ✅ Registrar cliente completo
- ✅ Buscar cliente por DNI
- ✅ Listar productos
- ✅ Registrar egreso
- ✅ CRUD de usuarios

### 📊 MÉTRICAS SEMANA 2
- **Story Points completados:** 22 SP
- **Módulos al 100%:** Clientes
- **Módulos al 60-80%:** Inventario, Egresos, Usuarios

---

## 🎯 SEMANA 3: CUENTAS + INVENTARIO COMPLETO
**Fechas:** 28 Oct - 3 Noviembre 2025  
**Entrega:** **Miércoles 30 Oct**  
**Presentación:** Viernes 1 Nov

### 🎯 OBJETIVO SEMANAL
Creación de Cuentas funcional + Inventario con alertas

### 📋 TAREAS POR DESARROLLADOR

#### 👤 **JONATHAN**
**Lunes-Martes:**
- [ ] Crear `CuentaRepository` completo
- [ ] Crear `CuentaService` con lógica de negocio:
  - Crear cuenta nueva
  - Calcular total de cuenta
  - Ver cuentas activas/pendientes
  - Cambiar estado de cuenta

**Miércoles (DÍA DE ENTREGA):**
- [ ] Crear `CuentasViewModel`
- [ ] Conectar `UserControlCuenta.xaml`
- [ ] **DEMO:** Crear cuenta, calcular total
- [ ] **ENTREGA:** Módulo Cuenta funcional

#### 👤 **ANGEL** ⚠️ CRÍTICO
**Lunes:**
- [ ] Implementar alertas de stock en ViewModel
- [ ] Crear notificación visual cuando `stock <= stockMinimo`
- [ ] Filtros de búsqueda en inventario

**Martes:**
- [ ] Crear `MovimientoInventarioRepository`
- [ ] Registrar entrada de inventario (aumentar stock)
- [ ] Registrar salida de inventario (disminuir stock)
- [ ] Ver historial de movimientos

**Miércoles (DÍA DE ENTREGA):**
- [ ] **DEMO:** Alertas de stock funcionando
- [ ] **DEMO:** Registrar movimientos de inventario
- [ ] **ENTREGA:** Módulo Inventario 100%

#### 👤 **NORMA**
**Lunes-Martes:**
- [ ] Terminar módulo Egresos
- [ ] Filtros por tipo y fecha
- [ ] Validaciones

**Miércoles (DÍA DE ENTREGA):**
- [ ] **ENTREGA:** Módulo Egresos 100%

#### 👤 **LUIS**
**Lunes-Martes:**
- [ ] Terminar ajustes de Login
- [ ] Mejorar mensajes de error
- [ ] Validaciones adicionales

**Miércoles:**
- [ ] Preparar estructura para módulo de Reportes (queries)
- [ ] Apoyo a equipo con integraciones

### 🎤 PRESENTACIÓN VIERNES 1 NOV
**Demostrar:**
- ✅ Crear cuenta para cliente
- ✅ Calcular total de cuenta
- ✅ Alertas de stock bajo
- ✅ Registrar movimientos de inventario
- ✅ Egresos con filtros

### 📊 MÉTRICAS SEMANA 3
- **Story Points completados:** 20 SP
- **Módulos al 100%:** Clientes, Cuentas (parcial), Inventario, Egresos
- **Progreso total:** ~35%

---

## 💰 SEMANA 4: CONSUMO + CUENTAS
**Fechas:** 4 - 10 Noviembre 2025  
**Entrega:** **Miércoles 6 Nov**  
**Presentación:** Viernes 8 Nov

### 🎯 OBJETIVO SEMANAL
Registro de consumos en tiempo real + Gestión de cuentas

### 📋 TAREAS POR DESARROLLADOR

#### 👤 **JONATHAN** ⚠️ CRÍTICO
**Lunes:**
- [ ] Completar `CuentaService` con toda la lógica
- [ ] Calcular subtotales automáticamente
- [ ] Crear `CuentasViewModel`

**Martes:**
- [ ] Conectar `UserControlCuentas.xaml`
- [ ] Ver cuentas pendientes (DataGrid)
- [ ] Ver detalle de cuenta (entrada + consumos)

**Miércoles (DÍA DE ENTREGA):**
- [ ] Calcular total cuenta
- [ ] **DEMO:** Ver cuenta con consumos
- [ ] **ENTREGA:** Módulo Cuentas 100%

#### 👤 **ANGEL** ⚠️ CRÍTICO
**Lunes:**
- [ ] Crear `DetalleConsumoRepository` (Ya NO existe OrdenRepository)
- [ ] Crear `IConsumoService` + `ConsumoService`
- [ ] Lógica: agregar producto directo a DetalleConsumo → actualizar cuenta → descontar stock

**Martes:**
- [ ] Crear `ConsumoViewModel`
- [ ] Conectar `UserControlConsumo.xaml`
- [ ] Buscar cliente por DNI (cuenta activa)
- [ ] ComboBox con productos

**Miércoles (DÍA DE ENTREGA):**
- [ ] Agregar productos directamente a DetalleConsumo
- [ ] Ver lista de productos agregados (DataGrid)
- [ ] Actualizar total cuenta EN TIEMPO REAL
- [ ] Descontar stock al confirmar
- [ ] **DEMO:** Agregar 3 productos y ver cuenta actualizada
- [ ] **ENTREGA:** Módulo Consumo 100%

#### 👤 **NORMA**
**Lunes-Martes:**
- [ ] Comenzar módulo Pagos (preparación)
- [ ] Crear `PagoRepository`
- [ ] Crear `ComprobanteRepository`

**Miércoles (DÍA DE ENTREGA):**
- [ ] **ENTREGA:** Repositorio Pagos al 50%

#### 👤 **LUIS**
**Lunes-Martes:**
- [ ] Implementar lógica de fidelización en CuentaService (YA NO existe EntradaService)
- [ ] Detectar 5ta visita al crear cuenta
- [ ] Aplicar cuenta gratis automáticamente

**Miércoles (DÍA DE ENTREGA):**
- [ ] **DEMO:** Cliente con 5ta visita entra gratis
- [ ] **ENTREGA:** Fidelización básica funcional

### 🎤 PRESENTACIÓN VIERNES 8 NOV
**Demostrar:**
- ✅ Cliente entra → se crea cuenta
- ✅ Agregar consumos directos a DetalleConsumo → cuenta se actualiza
- ✅ Ver detalle de cuenta
- ✅ Stock se descuenta automáticamente

### 📊 MÉTRICAS SEMANA 4
- **Story Points completados:** 30 SP
- **Módulos al 100%:** Clientes, Cuentas, Inventario, Egresos, Consumo
- **Progreso total:** ~60%

---

## 💳 SEMANA 5: PAGOS + COMPROBANTES
**Fechas:** 11 - 17 Noviembre 2025  
**Entrega:** **Miércoles 13 Nov**  
**Presentación:** Viernes 15 Nov

### 🎯 OBJETIVO SEMANAL
Sistema de pagos completo + Generación de comprobantes

### 📋 TAREAS POR DESARROLLADOR

#### 👤 **JONATHAN**
**Lunes-Miércoles:**
- [ ] Ayudar con integración de módulos
- [ ] Resolver bugs de módulos anteriores
- [ ] Testing de flujo completo

#### 👤 **ANGEL** ⚠️ CRÍTICO
**Lunes:**
- [ ] Crear `PagoRepository` + `ComprobanteRepository`
- [ ] Crear `IPagoService` + `PagoService`
- [ ] Lógica de generación de comprobante

**Martes:**
- [ ] Crear `PagoViewModel`
- [ ] Conectar `UserControlPago.xaml`
- [ ] Buscar cuenta pendiente
- [ ] Mostrar total a pagar

**Miércoles (DÍA DE ENTREGA):**
- [ ] Seleccionar método de pago (RadioButton)
- [ ] Registrar pago
- [ ] Generar comprobante (Boleta/Factura)
- [ ] Cambiar estado cuenta a "Pagada"
- [ ] Mostrar comprobante en ventana
- [ ] **DEMO:** Flujo completo: entrada → consumo → pago
- [ ] **ENTREGA:** Módulo Pagos 100%

#### 👤 **NORMA**
**Lunes-Miércoles:**
- [ ] Continuar desarrollo Cierre de Caja
- [ ] Crear `ICierreCajaService` + `CierreCajaService`
- [ ] Implementar queries para totales del día

#### 👤 **LUIS**
**Lunes-Miércoles:**
- [ ] Terminar detalles de Usuarios
- [ ] Implementar control de permisos
- [ ] Validar que recepcionista no acceda a módulos admin

### 🎤 PRESENTACIÓN VIERNES 15 NOV
**Demostrar:**
- ✅ **FLUJO COMPLETO:**
  1. Cliente entra
  2. Consume productos
  3. Paga (efectivo o tarjeta)
  4. Recibe comprobante
- ✅ Cuenta cambia a "Pagada"

### 📊 MÉTRICAS SEMANA 5
- **Story Points completados:** 28 SP
- **Módulos al 100%:** 7 módulos
- **Progreso total:** ~75%

---

## 📊 SEMANA 6: CIERRE DE CAJA + INTEGRACIÓN
**Fechas:** 18 - 24 Noviembre 2025  
**Entrega:** **Miércoles 20 Nov**  
**Presentación:** Viernes 22 Nov

### 🎯 OBJETIVO SEMANAL
Cierre de caja funcional + Integración de todos los módulos

### 📋 TAREAS POR DESARROLLADOR

#### 👤 **JONATHAN**
**Lunes-Martes:**
- [ ] Integración general de módulos
- [ ] Testing de flujo completo
- [ ] Corrección de bugs

**Miércoles (DÍA DE ENTREGA):**
- [ ] **DEMO:** Flujo completo sin errores
- [ ] **ENTREGA:** Sistema integrado

#### 👤 **ANGEL**
**Lunes-Martes:**
- [ ] Refinamiento módulo Pagos
- [ ] Implementar serie y numeración de comprobantes
- [ ] Testing

**Miércoles (DÍA DE ENTREGA):**
- [ ] **ENTREGA:** Pagos 100% refinado

#### 👤 **NORMA** ⚠️ CRÍTICO
**Lunes:**
- [ ] Crear `CajaViewModel`
- [ ] Conectar `UserControlCaja.xaml`
- [ ] Botón "Realizar Cierre"

**Martes:**
- [ ] Implementar cálculos:
  - Total entradas (query)
  - Total barra (query categoría)
  - Total accesorios (query categoría)
  - Total efectivo vs tarjeta
  - Total egresos
  - Ganancia neta

**Miércoles (DÍA DE ENTREGA):**
- [ ] Ver historial de cierres
- [ ] Validar cuentas pendientes
- [ ] **DEMO:** Cierre de caja del día
- [ ] **ENTREGA:** Módulo Cierre Caja 100%

#### 👤 **LUIS**
**Lunes-Miércoles:**
- [ ] Testing de permisos
- [ ] Refinamiento de Entradas
- [ ] Ayudar con integración

### 🎤 PRESENTACIÓN VIERNES 22 NOV
**Demostrar:**
- ✅ Cierre de caja completo
- ✅ Todos los módulos integrados
- ✅ Sistema funcionando de inicio a fin

### 📊 MÉTRICAS SEMANA 6
- **Story Points completados:** 25 SP
- **Módulos al 100%:** 8 módulos principales
- **Progreso total:** ~85%

---

## 📈 SEMANA 7: REPORTES + PULIDO
**Fechas:** 25 Nov - 1 Diciembre 2025  
**Entrega:** **Miércoles 27 Nov**  
**Presentación:** Viernes 29 Nov

### 🎯 OBJETIVO SEMANAL
Reportes básicos con gráficos + Pulido de UI

### 📋 TAREAS POR DESARROLLADOR

#### 👤 **JONATHAN** ⚠️ CRÍTICO
**Lunes:**
- [ ] Crear `IReporteService` + `ReporteService`
- [ ] Queries para reportes de ingresos

**Martes:**
- [ ] Crear `ReporteViewModel`
- [ ] Conectar `UserControlReporte.xaml`
- [ ] Reporte de ingresos del día

**Miércoles (DÍA DE ENTREGA):**
- [ ] Reporte por rango de fechas
- [ ] Gráfico simple con LiveCharts (ingresos por día)
- [ ] Top 5 productos vendidos
- [ ] **DEMO:** Reportes con gráficos
- [ ] **ENTREGA:** Módulo Reportes al 80%

#### 👤 **ANGEL**
**Lunes-Miércoles:**
- [ ] Refinar UI de Inventario
- [ ] Mejorar alertas de stock
- [ ] Testing exhaustivo

#### 👤 **NORMA**
**Lunes-Miércoles:**
- [ ] Intentar módulo Flujo de Caja básico
- [ ] Al menos mostrar ingresos vs egresos del mes
- [ ] Gráfico simple

#### 👤 **LUIS**
**Lunes-Miércoles:**
- [ ] Comenzar módulo Promociones (básico)
- [ ] Ver clientes próximos a 5ta visita
- [ ] Configuración de programa fidelización

### 🎤 PRESENTACIÓN VIERNES 29 NOV
**Demostrar:**
- ✅ Reportes con gráficos
- ✅ UI pulida y profesional
- ✅ Sistema estable

### 📊 MÉTRICAS SEMANA 7
- **Story Points completados:** 22 SP
- **Progreso total:** ~90%

---

## 🎨 SEMANA 8: FEATURES FINALES + TESTING
**Fechas:** 2 - 8 Diciembre 2025  
**Entrega:** **Miércoles 4 Dic**  
**Presentación:** Viernes 6 Dic

### 🎯 OBJETIVO SEMANAL
Completar features secundarias + Testing intensivo

### 📋 TAREAS POR DESARROLLADOR

#### 👤 **TODO EL EQUIPO**
**Lunes:**
- [ ] Lista de bugs conocidos
- [ ] Priorizar correcciones

**Martes:**
- [ ] Corrección de bugs críticos
- [ ] Testing manual de todos los flujos

**Miércoles (DÍA DE ENTREGA):**
- [ ] Sistema sin bugs críticos
- [ ] Todas las funciones core operativas
- [ ] **ENTREGA:** Sistema al 95%

#### Features opcionales si hay tiempo:
- [ ] Módulo Promociones completo (Luis)
- [ ] Flujo de Caja completo (Norma)
- [ ] Reportes avanzados (Jonathan)
- [ ] Mejoras de UI (Angel)

### 🎤 PRESENTACIÓN VIERNES 6 DIC
**Demostrar:**
- ✅ Sistema completo sin errores
- ✅ Demo de 20 minutos flujo real
- ✅ Features secundarias implementadas

### 📊 MÉTRICAS SEMANA 8
- **Story Points completados:** 20 SP
- **Progreso total:** ~95%

---

## 🎓 SEMANA 9: PRESENTACIÓN FINAL
**Fechas:** 9 - 12 Diciembre 2025  
**Presentación Final:** **Viernes 12 Dic** 🎯

### 🎯 OBJETIVO SEMANAL
Preparar presentación final impecable

### 📋 TAREAS

**Lunes 9 Dic:**
- [ ] Reunión: Revisar TODO el sistema
- [ ] Crear presentación PowerPoint
- [ ] Preparar script de demostración

**Martes 10 Dic:**
- [ ] Ensayo general de presentación
- [ ] Cronometrar: 30-40 minutos
- [ ] Asignar quién presenta qué módulo

**Miércoles 11 Dic:**
- [ ] Último ensayo
- [ ] Preparar laptop de presentación
- [ ] Base de datos con datos demo limpios

**Jueves 12 Dic (DÍA ANTERIOR):**
- [ ] Verificar que TODO funcione
- [ ] Preparar backup del proyecto
- [ ] Descansar temprano 😴

### 🎤 PRESENTACIÓN FINAL - VIERNES 12 DIC

**Estructura de presentación (40 min):**

1. **Introducción (5 min)** - Jonathan
   - Problemática de la empresa
   - Solución propuesta
   - Tecnologías utilizadas

2. **Arquitectura del Sistema (3 min)** - Jonathan
   - Diagrama N-Capas
   - Base de datos (mostrar diagrama)

3. **DEMOSTRACIÓN EN VIVO (25 min):**
   
   **Bloque 1: Gestión Base (5 min)** - Luis
   - Login
   - Gestión de usuarios
   - Gestión de clientes
   
   **Bloque 2: Operación Diaria (10 min)** - Luis + Angel
   - Registrar entrada de cliente
   - Agregar consumos en tiempo real
   - Procesar pago y generar comprobante
   
   **Bloque 3: Gestión Inventario (5 min)** - Angel
   - Ver inventario
   - Alertas de stock bajo
   - Registrar entrada de mercancía
   
   **Bloque 4: Financiero y Reportes (5 min)** - Norma + Jonathan
   - Registrar egresos
   - Cierre de caja diario
   - Reportes con gráficos

4. **Fidelización (2 min)** - Luis
   - Demostrar cliente en 5ta visita (entrada gratis)

5. **Conclusiones y Mejoras Futuras (3 min)** - Jonathan
   - Logros alcanzados
   - Requerimientos cumplidos
   - Mejoras futuras

6. **Preguntas y Respuestas (5 min)** - TODO EL EQUIPO

### 📊 MÉTRICAS FINALES
- **Módulos completados:** 8-9 de 12
- **Requerimientos funcionales:** 80-85%
- **Sistema funcional:** ✅ SÍ

---

#### 🎯 Objetivos:
- ✅ Crear base de datos SQL Server
- ✅ Ejecutar script `Sauna_Kalixto.sql`
- ✅ Insertar datos maestros iniciales
- ✅ Configurar estructura de carpetas completa
- ✅ Definir estándares de código
- ✅ Configurar Git y branches

#### 📋 Tareas para TODO EL EQUIPO:

**Jonathan (Scrum Master):**
- [ ] Crear repositorio Git
- [ ] Definir estrategia de branching (GitFlow)
- [ ] Configurar .gitignore
- [ ] Crear base de datos
- [ ] Preparar script de datos maestros

**Angel:**
- [ ] Crear carpeta `Repositories/` con subcarpetas
- [ ] Crear `IRepository<T>` genérico
- [ ] Crear `Repository<T>` base

**Norma:**
- [ ] Crear carpeta `Services/` con subcarpetas
- [ ] Crear `IService<T>` genérico (opcional)
- [ ] Definir estructura de DTOs

**Luis:**
- [ ] Configurar DI en `App.xaml.cs`
- [ ] Crear helper para encriptación
- [ ] Preparar `CurrentUser` singleton

#### 📊 Entregables Sprint 0:
- Base de datos operativa con datos maestros
- Estructura de carpetas completa
- Repositorio Git configurado
- Estándares de código documentados

#### 🔗 Datos Maestros a Insertar:

```sql
-- Roles
INSERT INTO Rol (nombre) VALUES ('Administrador'), ('Recepcionista');

-- Estados
INSERT INTO EstadoEntrada (nombre) VALUES ('Activo'), ('Finalizado');
INSERT INTO EstadoCuenta (nombre) VALUES ('Pendiente'), ('Pagada'), ('Cancelada');
INSERT INTO EstadoOrden (nombre) VALUES ('Pendiente'), ('Finalizado');

-- Categorías
INSERT INTO CategoriaProducto (nombre) VALUES ('Barra'), ('Accesorios'), ('Servicios');

-- Tipos
INSERT INTO TipoMovimiento (nombre) VALUES ('Entrada'), ('Salida');
INSERT INTO MetodoPago (nombre) VALUES ('Efectivo'), ('Tarjeta');
INSERT INTO TipoComprobante (nombre) VALUES ('Boleta'), ('Factura');
INSERT INTO TipoEgreso (nombre) VALUES ('Agua'), ('Luz'), ('Limpieza'), ('Mantenimiento'), ('Sueldos'), ('Insumos');
INSERT INTO TipoReporte (nombre) VALUES ('Ingresos'), ('Egresos'), ('Flujo Caja'), ('Inventario'), ('Clientes');

-- Programa Fidelización
INSERT INTO ProgramaFidelizacion (visitasParaDescuento, porcentajeDescuento, descuentoCumpleanos, montoDescuentoCumpleanos)
VALUES (5, 100.00, 1, 10.00);

-- Usuario admin inicial (contraseña: admin123 - encriptada)
INSERT INTO Usuario (nombreUsuario, contraseniaHash, correo, idRol)
VALUES ('admin', 'HASH_AQUI', 'admin@sauna.com', 1);
```

---

### SPRINT 1: Infraestructura Core (2 semanas)
**Duración:** 21 Oct - 3 Nov 2025  
**Objetivo:** Implementar autenticación y bases de arquitectura

#### 🎯 Story Points: 34

#### 📋 User Stories:

**US-01: Login de Usuario (8 SP)** - Luis
- Como usuario, quiero iniciar sesión para acceder al sistema
- **Criterios de aceptación:**
  - [x] Pantalla de login funcional
  - [x] Validación de credenciales
  - [x] Contraseñas encriptadas
  - [x] Mensaje de error si credenciales incorrectas
  - [x] Redirección a MainWindow tras login exitoso

**US-02: Gestión de Usuarios (13 SP)** - Luis
- Como administrador, quiero gestionar usuarios del sistema
- **Criterios de aceptación:**
  - [x] CRUD de usuarios
  - [x] Asignar roles
  - [x] Activar/desactivar usuarios
  - [x] Cambiar contraseña
  - [x] Validar permisos por rol

**US-03: Repositorios Base (13 SP)** - TODO EL EQUIPO
- Como desarrollador, quiero repositorios genéricos
- **Criterios de aceptación:**
  - [x] IRepository<T> con métodos CRUD
  - [x] Repository<T> implementación base
  - [x] Todos los repositorios específicos creados (25)
  - [x] Unit of Work (opcional)

#### 🔄 Tareas por Desarrollador:

**Jonathan:**
- [ ] ClienteRepository + Interface
- [ ] CuentaRepository + Interface
- [ ] ProgramaFidelizacionRepository + Interface (Ya NO existe EntradaRepository)

**Angel:**
- [ ] ProductoRepository + Interface
- [ ] MovimientoInventarioRepository + Interface
- [ ] DetalleConsumoRepository + Interface (Ya NO existe OrdenRepository)
- [ ] PagoRepository + Interface
- [ ] ComprobanteRepository + Interface
- [ ] MetodoPagoRepository + Interface
- [ ] TipoComprobanteRepository + Interface

**Norma:**
- [ ] EgresoRepository + Interface
- [ ] TipoEgresoRepository + Interface (Ya NO existe CierreCajaRepository ni FlujoCajaRepository)

**Luis:**
- [ ] UsuarioRepository + Interface
- [ ] RolRepository + Interface
- [ ] AuthenticationService completo
- [ ] LoginViewModel completo
- [ ] CurrentUser singleton

#### 📊 Entregables Sprint 1:
- Sistema de login funcional
- 17 repositorios implementados (YA NO son 25)
- Gestión de usuarios operativa
- Contraseñas encriptadas
- Control de roles básico

---

### SPRINT 2: Módulos de Cliente y Cuentas (2 semanas)
**Duración:** 4-17 Nov 2025  
**Objetivo:** Implementar gestión de clientes y creación de cuentas (YA NO existe módulo Entrada)

#### 🎯 Story Points: 40

#### 📋 User Stories:

**US-04: Gestión de Clientes (13 SP)** - Jonathan
- Como recepcionista, quiero gestionar clientes
- **Criterios:**
  - [x] Registrar nuevo cliente
  - [x] Consultar clientes
  - [x] Actualizar información
  - [x] Buscar por DNI, nombre, teléfono
  - [x] Ver historial de visitas

**US-05: Creación de Cuentas (13 SP)** - Jonathan (YA NO es "Registro de Entrada")
- Como recepcionista, quiero crear cuentas para clientes
- **Criterios:**
  - [x] Buscar cliente por DNI
  - [x] Crear cuenta automática al entrar
  - [x] Aplicar fidelización (5ta visita gratis)
  - [x] Ver cuentas activas
  - [x] Cambiar estado de cuenta

**US-06: Historial de Cliente (8 SP)** - Jonathan
- Como administrador, quiero ver historial completo de cliente
- **Criterios:**
  - [x] Ver todas las cuentas/visitas
  - [x] Ver todos los consumos
  - [x] Total gastado
  - [x] Promedio por visita

**US-07: Alertas de Fidelización (6 SP)** - Luis
- Como sistema, quiero mostrar alertas de fidelización
- **Criterios:**
  - [x] Detectar 5ta visita al crear cuenta
  - [x] Mostrar mensaje "cuenta gratis"
  - [x] Detectar cumpleaños
  - [x] Aplicar descuento automáticamente

---

### SPRINT 3: Inventario y Consumo (2 semanas)
**Duración:** 18 Nov - 1 Dic 2025  
**Objetivo:** Implementar control de inventario y registro de consumos

#### 🎯 Story Points: 42

#### 📋 User Stories:

**US-08: Gestión de Productos (13 SP)** - Angel
- Como administrador, quiero gestionar productos del inventario
- **Criterios:**
  - [x] CRUD de productos
  - [x] Categorías (Barra, Accesorios)
  - [x] Control de stock actual y mínimo
  - [x] Precios de compra y venta
  - [x] Búsqueda por código, nombre, categoría

**US-09: Movimientos de Inventario (8 SP)** - Angel
- Como administrador, quiero registrar movimientos de inventario (entradas/salidas de stock)
- **Criterios:**
  - [x] Registrar entrada de mercancía (aumentar stock)
  - [x] Registrar salida (merma, uso interno, disminuir stock)
  - [x] Historial de movimientos
  - [x] Calcular costo total

**US-10: Alertas de Stock (8 SP)** - Angel
- Como sistema, quiero alertar cuando stock sea bajo
- **Criterios:**
  - [x] Detectar stockActual <= stockMinimo
  - [x] Mostrar alerta visual
  - [x] Lista de productos por reorden
  - [x] Notificación persistente

**US-11: Registro de Consumo (13 SP)** - Angel
- Como recepcionista, quiero registrar consumos de clientes
- **Criterios:**
  - [x] Buscar cliente/cuenta activa
  - [x] Agregar productos (barra o accesorios)
  - [x] Actualizar cuenta en tiempo real
  - [x] Descontar stock automáticamente
  - [x] Ver detalle de consumos por cuenta

---

### SPRINT 4: Pagos y Caja (2 semanas)
**Duración:** 2-15 Dic 2025  
**Objetivo:** Implementar sistema de pagos y cierre de caja

#### 🎯 Story Points: 38

#### 📋 User Stories:

**US-12: Proceso de Pago (13 SP)** - Norma (YA NO es Angel - según distribución 17 tablas)
- Como recepcionista, quiero procesar pagos de clientes
- **Criterios:**
  - [x] Ver total de cuenta (consumos)
  - [x] Aplicar descuentos (fidelización)
  - [x] Seleccionar método (efectivo/tarjeta)
  - [x] Generar boleta o factura
  - [x] Cambiar estado cuenta a "Pagada"
  - [x] IGV incluido en factura

**US-13: Cierre de Caja Diario (13 SP)** - Norma
- Como administrador, quiero realizar cierre de caja diario (TODO CON QUERIES - sin tabla CierreCaja)
- **Criterios:**
  - [x] Calcular totales automáticos con queries SQL:
    - Total consumos del día
    - Total efectivo vs tarjeta
    - Total ingresos (suma de pagos)
    - Total egresos
    - Ganancia neta
  - [x] Ver historial de cierres calculados dinámicamente
  - [x] Imprimir reporte de cierre
  - [x] Validar que no haya cuentas pendientes

**US-14: Comprobantes (12 SP)** - Norma (YA NO es Angel - según distribución 17 tablas)
- Como sistema, quiero generar comprobantes automáticos
- **Criterios:**
  - [x] Serie y numeración automática
  - [x] Boleta (sin RUC)
  - [x] Factura (con RUC)
  - [x] Incluir IGV 18%
  - [x] Detalle de productos
  - [x] Imprimir o exportar PDF

---

### SPRINT 5: Egresos y Control Financiero (2 semanas) - YA NO existe FlujoCaja como tabla
**Duración:** 16-29 Dic 2025  
**Objetivo:** Implementar control financiero (egresos) - Reportes financieros van en Sprint 6

#### 🎯 Story Points: 34

#### 📋 User Stories:

**US-15: Registro de Egresos (13 SP)** - Norma
- Como administrador, quiero registrar gastos operativos
- **Criterios:**
  - [x] Registrar egresos por tipo (agua, luz, etc.)
  - [x] Marcar si es recurrente
  - [x] Adjuntar comprobante
  - [x] Ver historial de egresos
  - [x] Filtros por fecha y tipo
  - [x] Reporte mensual de egresos

**US-16: Cálculo de Flujo de Caja (13 SP)** - Luis (YA NO es tabla - se calcula con queries)
- Como administrador, quiero ver flujo de caja calculado dinámicamente
- **Criterios:**
  - [x] Calcular saldo con queries SQL (sin tabla FlujoCaja)
  - [x] Total ingresos y egresos del período
  - [x] Utilidad neta calculada
  - [x] Detalle de movimientos
  - [x] Gráfico ingresos vs egresos
  - [x] Comparar períodos

**US-17: Gestión de Cuentas (8 SP)** - Jonathan
- Como recepcionista, quiero ver y gestionar cuentas
- **Criterios:**
  - [x] Ver todas las cuentas
  - [x] Filtrar por estado (activa, pagada, cancelada)
  - [x] Ver detalle de cuenta
  - [x] Cancelar cuenta (si necesario)
  - [x] Buscar por cliente

---

### SPRINT 6: Reportes y Análisis (2 semanas) - TODO CON QUERIES (sin tablas Reporte/TipoReporte)
**Duración:** 30 Dic 2025 - 12 Ene 2026  
**Objetivo:** Implementar sistema completo de reportes con queries SQL dinámicas

#### 🎯 Story Points: 40

#### 📋 User Stories:

**US-18: Reportes de Ingresos (13 SP)** - Luis (YA NO es Jonathan - reportes son de Luis)
- Como administrador, quiero reportes dinámicos de ingresos (sin tabla Reporte)
- **Criterios:**
  - [x] Ingresos por tipo (entrada, barra, accesorios)
  - [x] Ingresos por producto
  - [x] Ingresos por día/semana/mes
  - [x] Filtros por fecha y rango
  - [x] Exportar a PDF
  - [x] Gráficos estadísticos con LiveCharts
  - [x] TODO calculado con queries SQL (sin tabla Reporte)

**US-19: Reportes de Egresos (8 SP)** - Luis
- Como administrador, quiero reportes dinámicos de gastos
- **Criterios:**
  - [x] Egresos por tipo (queries SQL)
  - [x] Egresos por período
  - [x] Comparar períodos
  - [x] Gráfico de distribución

**US-20: Análisis de Consumo (13 SP)** - Luis
- Como administrador, quiero analizar patrones de consumo con queries
- **Criterios:**
  - [x] Horarios de mayor consumo (query sobre DetalleConsumo)
  - [x] Días con más ingresos (query sobre Pago)
  - [x] Productos más vendidos (Top 10 con query)
  - [x] Clientes frecuentes (query sobre Cuenta)
  - [x] Gráficos con LiveCharts
  - [x] Dashboard general

**US-21: Reporte de Inventario (6 SP)** - Angel
- Como administrador, quiero reporte dinámico de inventario
- **Criterios:**
  - [x] Productos por reorden (query)
  - [x] Valor total del inventario (calculado)
  - [x] Costo de ventas
  - [x] Margen de ganancia por producto

---

### SPRINT 7: Promociones y Fidelización (2 semanas)
**Duración:** 13-26 Ene 2026  
**Objetivo:** Implementar sistema completo de fidelización y notificaciones

#### 🎯 Story Points: 34

#### 📋 User Stories:

**US-22: Programa de Fidelización (13 SP)** - Luis
- Como administrador, quiero configurar programa de fidelización
- **Criterios:**
  - [x] Configurar visitas para premio
  - [x] Porcentaje de descuento
  - [x] Descuento cumpleaños
  - [x] Ver clientes próximos a premio
  - [x] Historial de premios otorgados

**US-23: Envío de Promociones Email (8 SP)** - Luis
- Como administrador, quiero enviar promociones por correo
- **Criterios:**
  - [x] Plantillas de correo
  - [x] Seleccionar destinatarios
  - [x] Envío masivo
  - [x] Programar envío
  - [x] Historial de envíos

**US-24: Envío de Promociones WhatsApp (8 SP)** - Luis
- Como administrador, quiero enviar promociones por WhatsApp
- **Criterios:**
  - [x] Integración con API (Twilio o simulado)
  - [x] Plantillas de mensaje
  - [x] Envío individual o masivo
  - [x] Ver estado de envío

**US-25: Alertas Automáticas (5 SP)** - Luis
- Como sistema, quiero generar alertas automáticas de fidelización
- **Criterios:**
  - [x] Detectar eventos (5ta visita, cumpleaños)
  - [x] Mostrar alerta en pantalla
  - [x] Aplicar descuento automático
  - [x] Log de alertas generadas

---

### SPRINT 8: Testing, Validaciones y Documentación (2 semanas)
**Duración:** 27 Ene - 9 Feb 2026  
**Objetivo:** Asegurar calidad y completar documentación

#### 🎯 Story Points: 26

#### 📋 Tareas:

**Validaciones (8 SP)** - TODO EL EQUIPO
- [ ] Validación de DNI (8 dígitos)
- [ ] Validación de RUC (11 dígitos)
- [ ] Validación de correo electrónico
- [ ] Validación de teléfono
- [ ] Validación de montos positivos
- [ ] Validación de fechas
- [ ] Validación de stock disponible
- [ ] Mensajes de error claros

**Manejo de Excepciones (6 SP)** - TODO EL EQUIPO
- [ ] Try-catch en todos los métodos críticos
- [ ] Log de errores
- [ ] Mensajes amigables al usuario
- [ ] Rollback de transacciones en error

**Testing Unitario (8 SP)** - TODO EL EQUIPO
- [ ] Tests de Repositories (mínimo 10)
- [ ] Tests de Services (mínimo 10)
- [ ] Tests de ViewModels (mínimo 5)
- [ ] Cobertura mínima 60%

**Documentación (4 SP)** - Jonathan (Scrum Master)
- [ ] Comentarios XML en código
- [ ] Manual de usuario básico
- [ ] Guía de instalación
- [ ] Documentación de arquitectura
- [ ] Diagramas (clases, secuencia, componentes)

---

## 📊 DISTRIBUCIÓN DE CARGA DE TRABAJO

### Resumen por Desarrollador

| Desarrollador | Módulos | Story Points | Horas Estimadas |
|---------------|---------|--------------|-----------------|
| **Jonathan Puma** | 3 + Scrum Master | 95 SP | 200h |
| **Angel Zuñiga** | 3 | 98 SP | 196h |
| **Norma Aranibar** | 3 | 85 SP | 170h |
| **Luis Vega** | 3 | 92 SP | 184h |
| **TOTAL** | 12 módulos | 370 SP | 750h |

**Nota:** 1 Story Point ≈ 2 horas de trabajo

---

## 🎯 CEREMONIAS SCRUM

### Daily Standup (15 min) - Lunes a Viernes
**Hora:** 9:00 AM  
**Formato:** Virtual (Discord/Zoom) o Presencial

**Preguntas:**
1. ¿Qué hice ayer?
2. ¿Qué haré hoy?
3. ¿Tengo algún impedimento?

**Responsable:** Jonathan (Scrum Master)

---

### Sprint Planning (2 horas) - Primer día de cada Sprint
**Objetivos:**
1. Revisar Product Backlog
2. Seleccionar User Stories del Sprint
3. Estimar Story Points
4. Asignar tareas
5. Definir Sprint Goal

**Participantes:** Todo el equipo

---

### Sprint Review (1 hora) - Último día de cada Sprint
**Objetivos:**
1. Demostrar funcionalidades completadas
2. Recoger feedback del Product Owner
3. Actualizar Product Backlog

**Formato:** Demostración en vivo del sistema

---

### Sprint Retrospective (1 hora) - Último día de cada Sprint
**Objetivos:**
1. ¿Qué salió bien?
2. ¿Qué salió mal?
3. ¿Qué podemos mejorar?
4. Action items para próximo Sprint

**Formato:** Abierto y honesto

---

## 🔧 HERRAMIENTAS Y ESTÁNDARES

### Control de Versiones: Git

**Estrategia de Branching:**
```
main (producción)
├── develop (desarrollo)
    ├── feature/jonathan-clientes
    ├── feature/jonathan-cuentas
    ├── feature/jonathan-reportes
    ├── feature/angel-inventario
    ├── feature/angel-consumo
    ├── feature/angel-pagos
    ├── feature/norma-caja
    ├── feature/norma-egresos
    ├── feature/norma-flujocaja
    ├── feature/luis-usuarios
    ├── feature/luis-entradas
    └── feature/luis-promociones
```

**Commits:**
- Formato: `[TIPO] Descripción corta`
- Tipos: `[FEAT]`, `[FIX]`, `[REFACTOR]`, `[DOCS]`, `[TEST]`
- Ejemplo: `[FEAT] Implementar CRUD de clientes`

**Pull Requests:**
- Requiere aprobación de al menos 1 desarrollador
- Pasar validaciones automáticas (build)
- Sin conflictos con develop

---

### Estándares de Código

**C# / WPF:**
- **Naming:**
  - Clases: PascalCase (ClienteService)
  - Métodos: PascalCase (ObtenerCliente)
  - Variables: camelCase (clienteActual)
  - Propiedades: PascalCase (NombreCliente)
  - Interfaces: IPascalCase (IClienteRepository)

- **Organización:**
  - Un archivo por clase
  - Agrupar por responsabilidad
  - Máximo 300 líneas por archivo

- **Comentarios:**
  - XML comments en métodos públicos
  - Comentarios inline solo si es necesario
  - TODO para tareas pendientes

**Ejemplo:**
```csharp
/// <summary>
/// Obtiene un cliente por su ID
/// </summary>
/// <param name="id">ID del cliente</param>
/// <returns>Objeto Cliente o null si no existe</returns>
public async Task<Cliente?> ObtenerClientePorIdAsync(int id)
{
    // TODO: Agregar caché
    return await _context.Cliente
        .Include(c => c.idProgramaNavigation)
        .FirstOrDefaultAsync(c => c.idCliente == id);
}
```

---

### Estructura de Archivos

**Repositories:**
```csharp
// IClienteRepository.cs
public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> ObtenerPorDocumentoAsync(string documento);
    Task<List<Cliente>> ObtenerActivosAsync();
}

// ClienteRepository.cs
public class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(SaunaDbContext context) : base(context) { }
    
    // Implementación específica
}
```

**Services:**
```csharp
// IClienteService.cs
public interface IClienteService
{
    Task<ClienteDTO> CrearClienteAsync(ClienteDTO clienteDto);
    Task<List<ClienteDTO>> ObtenerTodosAsync();
}

// ClienteService.cs
public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    
    public ClienteService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }
    
    // Lógica de negocio
}
```

**ViewModels:**
```csharp
public class ClientesViewModel : BaseViewModel
{
    private readonly IClienteService _clienteService;
    
    // Propiedades observables
    private ObservableCollection<ClienteDTO> _clientes;
    public ObservableCollection<ClienteDTO> Clientes
    {
        get => _clientes;
        set => SetProperty(ref _clientes, value);
    }
    
    // Commands
    public ICommand AgregarCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand EliminarCommand { get; }
    
    // Constructor con DI
    public ClientesViewModel(IClienteService clienteService)
    {
        _clienteService = clienteService;
        InicializarCommands();
        CargarClientes();
    }
}
```

---

## 📈 MÉTRICAS Y SEGUIMIENTO

### Velocidad del Equipo
- **Sprint 1:** 34 SP (baseline)
- **Meta por Sprint:** 35-45 SP
- **Total del Proyecto:** 370 SP

### Definition of Done (DoD)

Una User Story está DONE cuando:
- [x] Código implementado y funcional
- [x] Code review aprobado
- [x] Sin errores de compilación
- [x] Probado manualmente (happy path + edge cases)
- [x] Comentarios XML en métodos públicos
- [x] Merged a develop
- [x] Demo al Product Owner aprobada

### Burndown Chart
- Actualizar diariamente
- Usar herramienta: Trello, Jira, o Excel compartido

---

## 🎲 GESTIÓN DE RIESGOS

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|--------------|---------|------------|
| **Conflictos en Git** | Alta | Medio | Branching strategy, comunicación, PRs pequeños |
| **Dependencias entre módulos** | Alta | Alto | Definir interfaces primero, mocks |
| **Falta de tiempo** | Media | Alto | Priorizar MVP, features opcionales al final |
| **Bugs en producción** | Media | Medio | Testing exhaustivo, Sprint de estabilización |
| **Cambios de requerimientos** | Baja | Medio | Comunicación con PO, scope claro |
| **Enfermedad de miembro** | Baja | Medio | Documentar bien, pair programming |

---

## 🚀 ENTREGABLES FINALES

### Sprint 8 - Producto Completo:

✅ **Sistema Funcional con:**
1. Login y autenticación segura
2. Gestión de clientes con historial
3. Registro de entradas y salidas
4. Consumo en tiempo real
5. Sistema de pagos y comprobantes
6. Control de inventario con alertas
7. Cierre de caja diario
8. Registro de egresos
9. Flujo de caja mensual
10. Reportes con gráficos estadísticos
11. Programa de fidelización
12. Envío de promociones (email + WhatsApp)
13. Gestión de usuarios con roles

✅ **Documentación:**
- Manual de usuario
- Guía de instalación
- Documentación de arquitectura
- Código comentado

✅ **Base de Datos:**
- Script SQL completo
- Datos maestros
- Datos de prueba

✅ **Testing:**
- Mínimo 25 tests unitarios
- Cobertura 60%

---

## 📞 COMUNICACIÓN DEL EQUIPO

### Canales:
- **Daily Standups:** Presencial/Zoom (9:00 AM)
- **Chat:** WhatsApp/Discord para consultas rápidas
- **Reuniones técnicas:** Según necesidad
- **Documentación:** GitHub Wiki o Google Docs compartido

### Disponibilidad:
- **Horario de trabajo:** Lunes a Viernes, 9:00 AM - 6:00 PM
- **Sábados opcionales:** Para alcanzar objetivos

---

## ✅ CHECKLIST FINAL

### Antes de cada Sprint:
- [ ] Sprint Planning realizado
- [ ] User Stories claras y estimadas
- [ ] Tareas asignadas
- [ ] Sprint Goal definido

### Durante el Sprint:
- [ ] Daily Standups diarios
- [ ] Actualizar estado de tareas
- [ ] Comunicar impedimentos
- [ ] Code reviews

### Al final del Sprint:
- [ ] Sprint Review con demo
- [ ] Sprint Retrospective
- [ ] Actualizar velocidad del equipo
- [ ] Planificar siguiente Sprint

---

## 🎓 CONCLUSIÓN - PLAN INTENSIVO 9 SEMANAS

Este plan Scrum INTENSIVO está diseñado para entregar un **MVP funcional en 9 semanas** con **4 desarrolladores trabajando en paralelo**.

### ✅ CARACTERÍSTICAS DEL PLAN

1. **Entregas semanales:** Cada miércoles para mostrar avance
2. **Módulos independientes:** Trabajo paralelo sin bloqueos
3. **Priorización clara:** Core features primero, secundarias después
4. **Integración continua:** Lunes y martes para juntar código
5. **Presentaciones:** Viernes con demos reales

### 🎯 OBJETIVO REALISTA

**Al 12 de Diciembre 2025 tendremos:**

**OBLIGATORIO (MVP):**
- ✅ Login funcional con roles
- ✅ Gestión de clientes
- ✅ Registro de entradas al sauna
- ✅ Control de inventario con alertas
- ✅ Registro de consumos en tiempo real
- ✅ Sistema de pagos y comprobantes
- ✅ Cierre de caja diario
- ✅ Registro de egresos

**IDEAL (SI HAY TIEMPO):**
- ⚠️ Reportes con gráficos
- ⚠️ Sistema de fidelización completo
- ⚠️ Gestión avanzada de usuarios
- ⚠️ Flujo de caja mensual

**OPCIONAL (NICE TO HAVE):**
- 🟢 Envío de promociones
- 🟢 Reportes avanzados
- 🟢 Exportar a PDF

### 💪 ÉXITO DEL PROYECTO DEPENDE DE:

1. **Consistencia:** Trabajar todos los días, no dejarlo todo al final
2. **Comunicación:** Daily updates obligatorios
3. **Integración:** Juntar código lunes y martes
4. **Priorización:** Features core primero, secundarias después
5. **Ayuda mutua:** Si terminas antes, ayuda a otros
6. **Testing:** Probar mientras desarrollas, no al final

### ⚠️ RECORDATORIOS IMPORTANTES

- 📅 **Entregas TODOS los miércoles:** No negociable
- 🔄 **Integración lunes/martes:** Obligatoria
- 💬 **Comunicación diaria:** Por WhatsApp mínimo
- 🚨 **Avisar bloqueos:** Inmediatamente, no esperar
- ✅ **Commits frecuentes:** Al menos 2 por día
- 🧪 **Testing continuo:** No dejar para el final

### 🏆 MENSAJE FINAL

**¡EQUIPO, PODEMOS LOGRARLO! 💪**

Tienen:
- ✅ Base de datos diseñada (50% del trabajo)
- ✅ Vistas XAML creadas (30% del trabajo)
- ✅ Plan detallado semana a semana
- ✅ 4 personas trabajando juntas
- ✅ 9 semanas de tiempo

**No necesitan terminar TODO, solo el MVP funcional.**

**Meta:** Entregar un sistema que resuelva los problemas críticos del Sauna KALIXTO.

**¡Éxitos, equipo! Nos vemos el lunes 14 de octubre para empezar! 🚀**

---

**Documento creado por:** GitHub Copilot Senior Analyst  
**Fecha:** 13 de Octubre, 2025  
**Versión:** 2.0 - INTENSIVO 9 SEMANAS  
**Deadline:** 12 de Diciembre, 2025  
**Entregas:** Cada miércoles  
**Presentaciones:** Cada viernes

**PRÓXIMA REUNIÓN:**  
📅 **Lunes 14 Octubre, 9:00 AM**  
🎯 **Objetivo:** Iniciar Semana 1 - Crear base de datos y login funcional

**PRÓXIMA ENTREGA:**  
📅 **Miércoles 16 Octubre**  
🎯 **Demostrar:** Base de datos creada + Login funcional 100%
