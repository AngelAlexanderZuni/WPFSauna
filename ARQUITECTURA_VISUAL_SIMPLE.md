# 🏛️ GUÍA VISUAL PARA LOS 4 PROGRAMADORES - PROYECTO SAUNA

> **Para el equipo que preguntó:** *"¿Qué debo poner en repositorios, o interfaces?"*  
> **Objetivo:** Gráficos claros + instrucciones simples SIN código técnico.

---

## 🎯 ¿QUÉ ES MVVM? (EXPLICACIÓN SÚPER SIMPLE)

**MVVM** = Separar la pantalla, la lógica y los datos

```
        � USUARIO INTERACTÚA
             ⬇️
    ┌─────────────────────────┐
    │   📱 VIEW (XAML)        │ ← Lo que VES: Botones, cajas de texto
    │   UserControl.xaml      │
    └──────────┬──────────────┘
               │ 🔗 Data Binding (enlace automático)
               ⬇️
    ┌─────────────────────────┐
    │   🎮 VIEWMODEL          │ ← Controla QUÉ pasa cuando haces clic
    │   ViewModel.cs          │   Propiedades + Comandos
    └──────────┬──────────────┘
               │ 📞 Llama a
               ⬇️
    ┌─────────────────────────┐
    │   ⚙️ SERVICE            │ ← VALIDA y aplica reglas de negocio
    │   Service.cs            │   ¿DNI único? ¿Formato correcto?
    └──────────┬──────────────┘
               │ 📞 Llama a
               ⬇️
    ┌─────────────────────────┐
    │   💾 REPOSITORY         │ ← Habla con la base de datos
    │   Repository.cs         │   CRUD: Crear, Leer, Actualizar, Borrar
    └──────────┬──────────────┘
               │ 🔌 Usa
               ⬇️
    ┌─────────────────────────┐
    │   🗃️ BASE DE DATOS      │ ← Donde se GUARDA todo
    │   SQL Server            │   17 Tablas
    └─────────────────────────┘
```

**🎯 Ventaja Principal:** Si cambias la pantalla, NO tocas la lógica. Si cambias la lógica, NO tocas la pantalla.

---

## 📂 LAS 8 CARPETAS PRINCIPALES (EXPLICACIÓN SIMPLE)

### 1️⃣ **Models/Entities/** - Las 17 clases de la base de datos
```
┌─────────────────────────────────────────────────────────┐
│  �️ ¿QUÉ ES?                                            │
│  Las 17 tablas convertidas automáticamente en clases C# │
│                                                          │
│  📄 Archivos:                                           │
│  • Cliente.cs                                           │
│  • Producto.cs                                          │
│  • Cuenta.cs                                            │
│  • ... (14 más)                                         │
│                                                          │
│  ⚠️ REGLA: NO TOCAR                                     │
│  Se generan con: Scaffold-DbContext                     │
│                                                          │
│  💡 Ejemplo simple:                                     │
│  Tabla Cliente en BD → Clase Cliente.cs aquí           │
└─────────────────────────────────────────────────────────┘
```

---

### 2️⃣ **Models/DTOs/** - Versión SIMPLE para la pantalla
```
┌─────────────────────────────────────────────────────────┐
│  📦 ¿QUÉ ES?                                            │
│  Objetos simplificados para enviar datos a la UI       │
│  SIN relaciones complejas, SIN referencias circulares  │
│                                                          │
│  ✅ TÚ LOS CREAS manualmente                            │
│                                                          │
│  📄 Ejemplo: ClienteDTO.cs                              │
│  ┌─────────────────────────────────────┐               │
│  │ class ClienteDTO                    │               │
│  │ {                                   │               │
│  │     int IdCliente                   │               │
│  │     string Nombre                   │               │
│  │     string Dni                      │               │
│  │     string Telefono                 │               │
│  │     string Email                    │               │
│  │ }                                   │               │
│  └─────────────────────────────────────┘               │
│                                                          │
│  🎯 Para qué: Enviar solo lo necesario a la pantalla   │
└─────────────────────────────────────────────────────────┘
```

---

### 3️⃣ **Repositories/** - Operaciones CRUD en la base de datos
```
┌─────────────────────────────────────────────────────────┐
│  💾 ¿QUÉ ES?                                            │
│  Clases que HABLAN con la base de datos                │
│                                                          │
│  📄 Archivos (siempre 2 por tabla):                    │
│  • IClienteRepository.cs  (Interfaz - el contrato)     │
│  • ClienteRepository.cs   (Implementación - el código) │
│                                                          │
│  🔧 Métodos básicos:                                    │
│  • ObtenerTodosAsync()      → SELECT * FROM            │
│  • ObtenerPorIdAsync(id)    → SELECT WHERE id =        │
│  • AgregarAsync(objeto)     → INSERT INTO              │
│  • ActualizarAsync(objeto)  → UPDATE                   │
│  • EliminarAsync(id)        → DELETE                   │
│                                                          │
│  ⚠️ EXCEPCIONES: NO crear para Cierre Caja ni Reportes│
└─────────────────────────────────────────────────────────┘
```

---

### 4️⃣ **Services/** - La LÓGICA de tu aplicación
```
┌─────────────────────────────────────────────────────────┐
│  ⚙️ ¿QUÉ ES?                                            │
│  Aquí va TODA la lógica de negocio y validaciones      │
│                                                          │
│  📄 Archivos (siempre 2 por módulo):                   │
│  • IClienteService.cs   (Interfaz)                     │
│  • ClienteService.cs    (Implementación)               │
│                                                          │
│  🧠 Responsabilidades:                                  │
│  • Validar datos (DNI único, formato email)            │
│  • Aplicar reglas de negocio                            │
│  • Mapear Entity ↔ DTO                                 │
│  • Llamar a Repositories                                │
│  • Devolver DTOs (NO Entities)                         │
│                                                          │
│  📝 Ejemplo de validación:                             │
│  Antes de guardar cliente:                              │
│  1. ¿DNI tiene 8 dígitos?                              │
│  2. ¿DNI ya existe en BD?                              │
│  3. ¿Email tiene formato correcto?                     │
│  4. Si todo OK → Guardar                               │
└─────────────────────────────────────────────────────────┘
```

---

### 5️⃣ **ViewModels/** - El PUENTE entre pantalla y lógica
```
┌─────────────────────────────────────────────────────────┐
│  🎮 ¿QUÉ ES?                                            │
│  Conecta lo que el usuario VE con los datos            │
│                                                          │
│  📄 Un archivo por pantalla:                           │
│  • ClientesViewModel.cs                                 │
│                                                          │
│  📦 Contiene 2 cosas importantes:                      │
│                                                          │
│  1️⃣ PROPIEDADES (lo que se muestra):                  │
│     • string NombreCliente                             │
│     • string DniCliente                                │
│     • ObservableCollection<ClienteDTO> Clientes       │
│                                                          │
│  2️⃣ COMANDOS (lo que pasa al hacer clic):             │
│     • GuardarCommand  → Al hacer clic en "Guardar"     │
│     • BuscarCommand   → Al hacer clic en "Buscar"      │
│     • EliminarCommand → Al hacer clic en "Eliminar"    │
│                                                          │
│  🔗 Se enlaza automáticamente con el XAML              │
└─────────────────────────────────────────────────────────┘
```

---

### 6️⃣ **Views/** - Lo que el USUARIO VE
```
┌─────────────────────────────────────────────────────────┐
│  � ¿QUÉ ES?                                            │
│  Las PANTALLAS de tu aplicación                        │
│                                                          │
│  📄 2 archivos por pantalla:                           │
│  • UserControlClientes.xaml     (Diseño visual)        │
│  • UserControlClientes.xaml.cs  (Code-behind - VACÍO) │
│                                                          │
│  🎨 Elementos visuales:                                │
│  • TextBox    → Cajas para escribir                    │
│  • Button     → Botones para hacer clic                │
│  • DataGrid   → Tablas para mostrar listas             │
│  • ComboBox   → Listas desplegables                    │
│  • DatePicker → Seleccionar fechas                     │
│                                                          │
│  🔗 Todo se enlaza con {Binding ...}                   │
│  No escribes código aquí, solo diseñas                 │
└─────────────────────────────────────────────────────────┘
```

---

### 7️⃣ **Helpers/** - Funciones ÚTILES reutilizables
```
┌─────────────────────────────────────────────────────────┐
│  🛠️ ¿QUÉ ES?                                            │
│  Funciones que usas en VARIOS lugares                  │
│                                                          │
│  📄 Archivos útiles (YA CREADOS):                      │
│  • ValidationHelper.cs  → Validar DNI, email, teléfono│
│  • PasswordHelper.cs    → Encriptar contraseñas        │
│  • DialogService.cs     → Mostrar mensajes al usuario  │
│  • NavigationService.cs → Cambiar de pantalla          │
│                                                          │
│  ✅ Solo ÚSALOS, no los modifiques                     │
│                                                          │
│  💡 Ejemplo de uso:                                    │
│  ValidationHelper.ValidarDNI("12345678")               │
│  → Devuelve true si es válido                          │
└─────────────────────────────────────────────────────────┘
```

---

### 8️⃣ **Commands/** - Comandos para BOTONES
```
┌─────────────────────────────────────────────────────────┐
│  🎯 ¿QUÉ ES?                                            │
│  Clases que conectan botones con métodos               │
│                                                          │
│  📄 Archivos (YA CREADOS):                             │
│  • RelayCommand.cs      → Para comandos normales       │
│  • AsyncRelayCommand.cs → Para comandos async/await    │
│                                                          │
│  ✅ Solo ÚSALOS en tus ViewModels                      │
│                                                          │
│  💡 Ejemplo de uso:                                    │
│  GuardarCommand = new AsyncRelayCommand(GuardarAsync); │
│  → Al hacer clic, ejecuta el método GuardarAsync()    │
└─────────────────────────────────────────────────────────┘
```

---

## 👥 GUÍA PARA LOS 4 PROGRAMADORES

### 👤 **PROGRAMADOR 1: JONATHAN**

**Tu módulo:** CLIENTES

```
┌────────────────────────────────────────────────────────────────┐
│  ❓ ¿QUÉ VAS A HACER?                                          │
│  Crear el módulo completo para registrar y buscar clientes    │
└────────────────────────────────────────────────────────────────┘

📋 PASO A PASO:

1️⃣ Crear DTO (Models/DTOs/ClienteDTO.cs)
   ├─ Propiedades simples: Id, Nombre, DNI, Teléfono, Email
   └─ SIN relaciones complejas

2️⃣ Crear Interfaz (Repositories/IClienteRepository.cs)
   ├─ Método: ObtenerTodosAsync()
   ├─ Método: ObtenerPorIdAsync(int id)
   ├─ Método: BuscarPorDniAsync(string dni)
   ├─ Método: AgregarAsync(Cliente cliente)
   ├─ Método: ActualizarAsync(Cliente cliente)
   └─ Método: EliminarAsync(int id)

3️⃣ Crear Repository (Repositories/ClienteRepository.cs)
   ├─ Implementa la interfaz
   ├─ Usa _context.Clientes
   └─ Usa ToListAsync(), AddAsync(), SaveChangesAsync()

4️⃣ Crear Interfaz de Servicio (Services/IClienteService.cs)
   ├─ Método: CrearClienteAsync(ClienteDTO dto)
   ├─ Método: ObtenerTodosAsync()
   └─ Método: BuscarPorDniAsync(string dni)

5️⃣ Crear Servicio (Services/ClienteService.cs)
   ├─ Validar DNI único (llamar a repository)
   ├─ Validar formato DNI (usar ValidationHelper)
   ├─ Mapear DTO → Entity
   └─ Guardar con repository

6️⃣ Crear ViewModel (ViewModels/ClientesViewModel.cs)
   ├─ Propiedades: NombreCliente, DniCliente, TelefonoCliente
   ├─ Propiedad: Clientes (lista observable)
   ├─ Comando: GuardarCommand
   ├─ Comando: BuscarCommand
   └─ Comando: EliminarCommand

7️⃣ Conectar XAML (Views/UserControlClientes.xaml)
   ├─ TextBox enlazado a NombreCliente
   ├─ TextBox enlazado a DniCliente
   ├─ Button enlazado a GuardarCommand
   └─ DataGrid enlazado a Clientes

┌────────────────────────────────────────────────────────────────┐
│  📁 CARPETAS QUE USARÁS:                                       │
│  ✅ Models/DTOs/          → ClienteDTO.cs                     │
│  ✅ Repositories/         → IClienteRepository.cs              │
│                             ClienteRepository.cs               │
│  ✅ Services/             → IClienteService.cs                 │
│                             ClienteService.cs                  │
│  ✅ ViewModels/           → ClientesViewModel.cs               │
│  ✅ Views/                → UserControlClientes.xaml           │
│  ✅ Helpers/              → ValidationHelper (solo usar)       │
└────────────────────────────────────────────────────────────────┘

🔄 FLUJO COMPLETO:
┌──────────────────────────────────────────────────────────┐
│ 1. Usuario escribe datos en TextBox                     │
│    ↓                                                      │
│ 2. Usuario hace clic en botón "Guardar"                 │
│    ↓                                                      │
│ 3. ViewModel.GuardarCommand se ejecuta                   │
│    ↓                                                      │
│ 4. ViewModel llama a ClienteService.CrearClienteAsync()  │
│    ↓                                                      │
│ 5. Service valida:                                       │
│    • ¿DNI tiene 8 dígitos? ✓                            │
│    • ¿DNI ya existe? ✓                                  │
│    • ¿Email válido? ✓                                   │
│    ↓                                                      │
│ 6. Service llama a ClienteRepository.AgregarAsync()      │
│    ↓                                                      │
│ 7. Repository guarda en base de datos                    │
│    ↓                                                      │
│ 8. ViewModel recarga lista de clientes                   │
│    ↓                                                      │
│ 9. DataGrid muestra el nuevo cliente ✅                  │
└──────────────────────────────────────────────────────────┘
```

---

### 👤 **PROGRAMADOR 2: ANGEL**

**Tu módulo:** INVENTARIO

```
┌────────────────────────────────────────────────────────────────┐
│  ❓ ¿QUÉ VAS A HACER?                                          │
│  Crear el módulo para gestionar productos y movimientos       │
└────────────────────────────────────────────────────────────────┘

📋 PASO A PASO:

1️⃣ Crear DTOs (Models/DTOs/)
   ├─ ProductoDTO.cs
   ├─ CategoriaProductoDTO.cs
   ├─ MovimientoInventarioDTO.cs
   └─ TipoMovimientoDTO.cs

2️⃣ Crear 4 pares Repository + Interfaz (Repositories/)
   ├─ IProductoRepository + ProductoRepository
   ├─ ICategoriaProductoRepository + CategoriaProductoRepository
   ├─ IMovimientoInventarioRepository + MovimientoInventarioRepository
   └─ ITipoMovimientoRepository + TipoMovimientoRepository

3️⃣ Crear Servicios (Services/)
   ├─ IInventarioService + InventarioService
   │  ├─ Validar stock mínimo
   │  ├─ Alerta si stock <= stockMinimo
   │  └─ Registrar entrada/salida de productos
   └─ IMovimientoService + MovimientoService

4️⃣ Crear ViewModel (ViewModels/InventarioViewModel.cs)
   ├─ Propiedades: NombreProducto, Stock, StockMinimo, Precio
   ├─ Propiedad: Productos (lista con alertas)
   ├─ Comando: GuardarProductoCommand
   ├─ Comando: RegistrarMovimientoCommand
   └─ Método: MostrarAlertaStockBajo()

5️⃣ Conectar XAML (Views/UserControlInventario.xaml)
   ├─ TextBox para nombre, stock, precio
   ├─ Button "Guardar Producto"
   ├─ Button "Registrar Movimiento"
   └─ DataGrid con productos (resaltar en rojo si stock bajo)

┌────────────────────────────────────────────────────────────────┐
│  📁 CARPETAS QUE USARÁS:                                       │
│  ✅ Models/DTOs/          → ProductoDTO.cs (y 3 más)          │
│  ✅ Repositories/         → 4 pares (Interfaz + Clase)        │
│  ✅ Services/             → InventarioService.cs               │
│  ✅ ViewModels/           → InventarioViewModel.cs             │
│  ✅ Views/                → UserControlInventario.xaml         │
└────────────────────────────────────────────────────────────────┘

🔄 FLUJO COMPLETO:
┌──────────────────────────────────────────────────────────┐
│ 1. Usuario registra producto:                           │
│    • Nombre: "Cerveza Pilsen"                           │
│    • Stock: 5                                            │
│    • StockMinimo: 10  ⚠️                                │
│    ↓                                                      │
│ 2. Usuario hace clic en "Guardar Producto"              │
│    ↓                                                      │
│ 3. ViewModel.GuardarProductoCommand ejecuta              │
│    ↓                                                      │
│ 4. InventarioService valida datos                        │
│    ↓                                                      │
│ 5. Service detecta: Stock (5) < StockMinimo (10) 🚨     │
│    ↓                                                      │
│ 6. Service guarda producto en BD                         │
│    ↓                                                      │
│ 7. ViewModel recibe alerta de stock bajo                 │
│    ↓                                                      │
│ 8. ViewModel muestra mensaje: "⚠️ Stock bajo"           │
│    ↓                                                      │
│ 9. DataGrid muestra producto en ROJO ✅                  │
└──────────────────────────────────────────────────────────┘
```

---

### 👤 **PROGRAMADOR 3: NORMA**

**Tus módulos:** PAGOS, EGRESOS y CIERRE DE CAJA

```
┌────────────────────────────────────────────────────────────────┐
│  ❓ ¿QUÉ VAS A HACER?                                          │
│  Crear 3 módulos: Pagos, Egresos y Cierre de Caja             │
└────────────────────────────────────────────────────────────────┘

📋 PASO A PASO (PAGOS):

1️⃣ Crear DTOs (Models/DTOs/)
   ├─ PagoDTO.cs
   ├─ ComprobanteDTO.cs
   ├─ MetodoPagoDTO.cs
   └─ TipoComprobanteDTO.cs

2️⃣ Crear 4 pares Repository (Repositories/)
   ├─ IPagoRepository + PagoRepository
   ├─ IComprobanteRepository + ComprobanteRepository
   ├─ IMetodoPagoRepository + MetodoPagoRepository
   └─ ITipoComprobanteRepository + TipoComprobanteRepository

3️⃣ Crear Servicio (Services/)
   ├─ IPagoService + PagoService
   │  ├─ Validar monto > 0
   │  ├─ Cambiar estado de cuenta a "Pagada"
   │  └─ Generar comprobante automático

4️⃣ Crear ViewModel (ViewModels/PagoViewModel.cs)
   ├─ Propiedades: MontoCuenta, MetodoSeleccionado
   ├─ Comando: ProcesarPagoCommand
   └─ Comando: GenerarComprobanteCommand

📋 PASO A PASO (CIERRE DE CAJA):

⚠️ IMPORTANTE: NO crear CierreCajaRepository (no existe tabla)

1️⃣ Crear DTO (Models/DTOs/CierreCajaDTO.cs)
   ├─ Propiedades: Fecha, TotalIngresos, TotalEgresos
   └─ GananciaNeta, DetalleMetodos

2️⃣ Crear SOLO Servicio (Services/)
   ├─ ICajaService + CajaService
   │  ├─ CalcularCierreDiarioAsync(fecha)
   │  ├─ Usa _context.Pagos directamente (SIN repository)
   │  ├─ Suma ingresos con query SQL
   │  └─ Suma egresos con query SQL

3️⃣ Crear ViewModel (ViewModels/CajaViewModel.cs)
   ├─ Propiedades: FechaSeleccionada, TotalIngresos, TotalEgresos
   ├─ Comando: CalcularCierreCommand
   └─ Mostrar resultados en pantalla

┌────────────────────────────────────────────────────────────────┐
│  📁 CARPETAS QUE USARÁS:                                       │
│  ✅ Models/DTOs/          → PagoDTO, EgresoDTO, CierreCajaDTO │
│  ✅ Repositories/         → Pago, Comprobante, Egreso         │
│  ⚠️ NO crear CierreCajaRepository                             │
│  ✅ Services/             → PagoService, EgresoService,       │
│                             CajaService                        │
│  ✅ ViewModels/           → PagoViewModel, EgresosViewModel,  │
│                             CajaViewModel                      │
│  ✅ Views/                → UserControlPago, UserControlEgresos│
│                             UserControlCaja                    │
└────────────────────────────────────────────────────────────────┘

🔄 FLUJO CIERRE DE CAJA:
┌─────────────────────────────────────────────────────────────┐
│ 1. Usuario selecciona fecha: 26/10/2025                    │
│    ↓                                                         │
│ 2. Usuario hace clic en "Calcular Cierre"                  │
│    ↓                                                         │
│ 3. CajaViewModel.CalcularCierreCommand ejecuta              │
│    ↓                                                         │
│ 4. CajaService.CalcularCierreDiarioAsync(fecha)             │
│    ↓                                                         │
│ 5. Service hace query SQL:                                  │
│    • SELECT SUM(monto) FROM Pago                           │
│      WHERE CAST(fechaHora AS DATE) = '2025-10-26'          │
│    • Resultado: S/. 500.00 (total ingresos) 💰            │
│    ↓                                                         │
│ 6. Service hace query SQL:                                  │
│    • SELECT SUM(monto) FROM Egreso                         │
│      WHERE CAST(fecha AS DATE) = '2025-10-26'              │
│    • Resultado: S/. 150.00 (total egresos) 💸             │
│    ↓                                                         │
│ 7. Service calcula:                                         │
│    • GananciaNeta = 500 - 150 = S/. 350.00 ✅              │
│    ↓                                                         │
│ 8. ViewModel recibe CierreCajaDTO                           │
│    ↓                                                         │
│ 9. Pantalla muestra:                                        │
│    • Ingresos: S/. 500.00                                  │
│    • Egresos: S/. 150.00                                   │
│    • Ganancia Neta: S/. 350.00 🎉                          │
└─────────────────────────────────────────────────────────────┘
```

📋 PASO A PASO (EGRESOS):

1️⃣ Crear DTOs (Models/DTOs/)
   ├─ EgresoDTO.cs
   └─ TipoEgresoDTO.cs

2️⃣ Crear 2 pares Repository (Repositories/)
   ├─ IEgresoRepository + EgresoRepository
   └─ ITipoEgresoRepository + TipoEgresoRepository

3️⃣ Crear Servicio (Services/)
   ├─ IEgresoService + EgresoService
   │  ├─ Validar monto > 0
   │  ├─ Validar concepto no vacío
   │  └─ Registrar egreso con fecha actual

4️⃣ Crear ViewModel (ViewModels/EgresosViewModel.cs)
   ├─ Propiedades: Concepto, Monto, TipoSeleccionado, FechaEgreso
   ├─ Propiedades: ObservableCollection<EgresoDTO> Egresos
   ├─ Comando: RegistrarEgresoCommand
   ├─ Comando: BuscarPorFechaCommand
   └─ Comando: LimpiarFormularioCommand

5️⃣ Conectar XAML (Views/UserControlEgresos.xaml)
   ├─ TextBox para concepto y monto
   ├─ ComboBox para tipo de egreso
   ├─ DatePicker para fecha
   ├─ Botón "Registrar Egreso"
   └─ DataGrid mostrando lista de egresos

┌────────────────────────────────────────────────────────────────┐
│  📁 CARPETAS QUE USARÁS (EGRESOS):                             │
│  ✅ Models/DTOs/          → EgresoDTO, TipoEgresoDTO          │
│  ✅ Repositories/         → EgresoRepository, TipoEgresoRepo  │
│  ✅ Services/             → EgresoService                      │
│  ✅ ViewModels/           → EgresosViewModel                   │
│  ✅ Views/                → UserControlEgresos.xaml            │
└────────────────────────────────────────────────────────────────┘

🔄 FLUJO REGISTRAR EGRESO:
┌─────────────────────────────────────────────────────────────┐
│ 1. Usuario ingresa datos:                                   │
│    • Concepto: "Compra de toallas"                         │
│    • Monto: S/. 150.00                                     │
│    • Tipo: "Compras" (del ComboBox)                        │
│    • Fecha: 27/10/2025                                     │
│    ↓                                                         │
│ 2. Usuario hace clic en "Registrar Egreso"                 │
│    ↓                                                         │
│ 3. EgresosViewModel.RegistrarEgresoCommand ejecuta          │
│    ↓                                                         │
│ 4. ViewModel llama a EgresoService.RegistrarEgresoAsync()   │
│    ↓                                                         │
│ 5. Service valida:                                          │
│    • ¿Monto > 0? ✓                                         │
│    • ¿Concepto no vacío? ✓                                │
│    • ¿Tipo de egreso válido? ✓                            │
│    ↓                                                         │
│ 6. Service llama a EgresoRepository.AgregarAsync()          │
│    ↓                                                         │
│ 7. Repository guarda egreso en BD:                          │
│    INSERT INTO Egreso (concepto, monto, idTipoEgreso, fecha)│
│    VALUES ('Compra de toallas', 150.00, 1, '2025-10-27')  │
│    ↓                                                         │
│ 8. ViewModel recarga lista de egresos                       │
│    ↓                                                         │
│ 9. DataGrid muestra el nuevo egreso:                        │
│    ┌──────────┬─────────────────┬─────────┬─────────┐     │
│    │ Fecha    │ Concepto        │ Tipo    │ Monto   │     │
│    ├──────────┼─────────────────┼─────────┼─────────┤     │
│    │ 27/10/25 │ Compra toallas  │ Compras │ S/.150  │ ✅  │
│    │ 26/10/25 │ Pago luz        │Servicios│ S/.80   │     │
│    └──────────┴─────────────────┴─────────┴─────────┘     │
│    ↓                                                         │
│ 10. Status bar muestra: "✅ Egreso registrado" 🎉          │
└─────────────────────────────────────────────────────────────┘

⚠️ IMPORTANTE PARA NORMA:
• Los egresos se usan en el módulo CIERRE DE CAJA
• CajaService usa EgresoRepository para calcular total de egresos
• Sin egresos registrados, el cierre de caja NO puede calcular la ganancia neta
• DEBES completar Egresos ANTES de Cierre de Caja

---

### 👤 **PROGRAMADOR 4: LUIS**

**Tu módulo:** LOGIN y REPORTES

```
┌────────────────────────────────────────────────────────────────┐
│  ❓ ¿QUÉ VAS A HACER?                                          │
│  Login funcional + Reportes con queries SQL                    │
└────────────────────────────────────────────────────────────────┘

📋 PASO A PASO (LOGIN):

1️⃣ Crear DTOs (Models/DTOs/)
   ├─ UsuarioDTO.cs
   ├─ LoginDTO.cs
   └─ RolDTO.cs

2️⃣ Crear Repositories (Repositories/)
   ├─ IUsuarioRepository + UsuarioRepository
   └─ IRolRepository + RolRepository

3️⃣ Crear Servicios (Services/)
   ├─ IAuthenticationService + AuthenticationService
   │  ├─ ValidarLoginAsync(usuario, contraseña)
   │  ├─ Encriptar contraseña con PasswordHelper
   │  └─ Guardar sesión en CurrentUser
   └─ IUsuarioService + UsuarioService

4️⃣ Crear ViewModel (ViewModels/LoginViewModel.cs)
   ├─ Propiedades: Usuario, Contraseña
   ├─ Comando: LoginCommand
   └─ Validar formulario

📋 PASO A PASO (REPORTES):

⚠️ IMPORTANTE: NO crear ReporteRepository (no existe tabla)

1️⃣ Crear DTOs (Models/DTOs/)
   ├─ ReporteIngresoDTO.cs
   ├─ ReporteEgresoDTO.cs
   ├─ ReporteProductoDTO.cs
   └─ FlujoCajaDTO.cs

2️⃣ Crear SOLO Servicio (Services/)
   ├─ IReporteService + ReporteService
   │  ├─ ObtenerIngresosPorRangoAsync(inicio, fin)
   │  ├─ ObtenerTop10ProductosAsync()
   │  ├─ ObtenerEgresosPorTipoAsync()
   │  └─ TODO con queries SQL directas (SIN repository)

3️⃣ Crear ViewModel (ViewModels/ReporteViewModel.cs)
   ├─ Propiedades: FechaInicio, FechaFin, DatosGrafico
   ├─ Comando: GenerarReporteCommand
   └─ Usar LiveCharts para gráficos

┌────────────────────────────────────────────────────────────────┐
│  📁 CARPETAS QUE USARÁS:                                       │
│  ✅ Models/DTOs/          → UsuarioDTO, ReporteIngresoDTO     │
│  ✅ Repositories/         → Solo Usuario y Rol                 │
│  ⚠️ NO crear ReporteRepository                                │
│  ✅ Services/             → AuthService, ReporteService        │
│  ✅ ViewModels/           → LoginViewModel, ReporteViewModel   │
│  ✅ Views/                → LoginSauna.xaml, UserControlReporte│
│  ✅ Helpers/              → PasswordHelper (solo usar)         │
└────────────────────────────────────────────────────────────────┘

🔄 FLUJO REPORTES:
┌─────────────────────────────────────────────────────────────┐
│ 1. Usuario selecciona rango de fechas:                     │
│    • Inicio: 01/10/2025                                    │
│    • Fin: 26/10/2025                                       │
│    ↓                                                         │
│ 2. Usuario hace clic en "Generar Reporte"                  │
│    ↓                                                         │
│ 3. ReporteViewModel.GenerarReporteCommand ejecuta           │
│    ↓                                                         │
│ 4. ReporteService.ObtenerIngresosPorRangoAsync()            │
│    ↓                                                         │
│ 5. Service hace query SQL compleja:                         │
│    • SELECT CAST(fechaHora AS DATE) as Fecha,              │
│             SUM(monto) as Total                             │
│      FROM Pago                                              │
│      WHERE fechaHora BETWEEN @inicio AND @fin               │
│      GROUP BY CAST(fechaHora AS DATE)                       │
│    ↓                                                         │
│ 6. Resultado del query:                                     │
│    • 01/10: S/. 450                                        │
│    • 02/10: S/. 520                                        │
│    • 03/10: S/. 380                                        │
│    • ... (26 días)                                         │
│    ↓                                                         │
│ 7. ViewModel procesa datos para LiveCharts                  │
│    ↓                                                         │
│ 8. Pantalla muestra gráfico de barras 📊                   │
│    • Eje X: Fechas                                         │
│    • Eje Y: Montos                                         │
│    • Total del período: S/. 12,450 ✅                      │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 RESUMEN DE CARPETAS POR FUNCIÓN

```
┌────────────────────────────────────────────────────────────────┐
│  ❓ ¿CUÁNDO USAR CADA CARPETA?                                 │
└────────────────────────────────────────────────────────────────┘

📁 Models/Entities/
   └─ ⚠️ NO TOCAR - Generado automáticamente
   └─ Son las 17 tablas de la BD

📁 Models/DTOs/
   └─ ✅ CREAR SIEMPRE - Una por cada entidad
   └─ Versión simple sin relaciones complejas
   └─ Ejemplo: Si tienes Cliente entity, creas ClienteDTO

📁 Repositories/
   └─ ✅ CREAR para tablas normales
   └─ ⚠️ NO CREAR para Cierre de Caja ni Reportes
   └─ Métodos: Obtener, Agregar, Actualizar, Eliminar

📁 Services/
   └─ ✅ CREAR SIEMPRE - Uno por módulo
   └─ Aquí van las validaciones
   └─ Aquí van las reglas de negocio
   └─ Llama a repositories O hace queries directas

📁 ViewModels/
   └─ ✅ CREAR SIEMPRE - Uno por pantalla (UserControl)
   └─ Propiedades que se enlazan a TextBox/ComboBox
   └─ Comandos que se enlazan a botones
   └─ Llama a Services (NUNCA a Repositories directos)

📁 Views/
   └─ ✅ CREAR SIEMPRE - Uno por módulo
   └─ Archivo .xaml con diseño visual
   └─ Archivo .xaml.cs casi vacío

📁 Helpers/
   └─ ✅ USAR (ya están creados)
   └─ ValidationHelper: Validar DNI, email, teléfono
   └─ PasswordHelper: Encriptar contraseñas
   └─ DialogService: Mostrar mensajes

📁 Commands/
   └─ ✅ USAR (ya están creados)
   └─ RelayCommand: Para comandos normales
   └─ AsyncRelayCommand: Para comandos con async/await
```

---

## ⚠️ CASOS ESPECIALES (SOLO PARA NORMA Y LUIS)

```
┌────────────────────────────────────────────────────────────────┐
│  CIERRE DE CAJA (NORMA) y REPORTES (LUIS)                     │
│  ⚠️ NO TIENEN REPOSITORY                                       │
└────────────────────────────────────────────────────────────────┘

FLUJO NORMAL (CON REPOSITORY):
ViewModel → Service → Repository → BD

FLUJO ESPECIAL (SIN REPOSITORY):
ViewModel → Service (hace queries directas) → BD

¿POR QUÉ?
Porque NO existen tablas CierreCaja ni Reporte en la BD.
Todo se CALCULA con queries SQL sobre otras tablas:
- Cierre de Caja: Calcula sumando Pagos y Egresos
- Reportes: Calcula con queries complejas (GROUP BY, SUM, etc.)

ENTONCES:
✅ SÍ crear: DTO, Service, ViewModel, View
⚠️ NO crear: Repository
```

---

## 📊 TABLA RÁPIDA: ¿QUÉ CREAR?

| Archivo | Jonathan | Angel | Norma | Luis |
|---------|----------|-------|-------|------|
| **DTO** | ✅ ClienteDTO | ✅ ProductoDTO, CategoriaDTO, MovimientoDTO, TipoMovimientoDTO | ✅ PagoDTO, ComprobanteDTO, CierreCajaDTO, EgresoDTO | ✅ UsuarioDTO, RolDTO, ReporteDTO |
| **Repository** | ✅ ClienteRepository | ✅ 4 Repositories | ✅ Solo Pago, Comprobante, Egreso | ✅ Solo Usuario, Rol |
| **Service** | ✅ ClienteService | ✅ InventarioService | ✅ PagoService, CajaService | ✅ AuthService, ReporteService |
| **ViewModel** | ✅ ClientesViewModel | ✅ InventarioViewModel | ✅ PagoViewModel, CajaViewModel | ✅ LoginViewModel, ReporteViewModel |
| **View** | ✅ UserControlClientes | ✅ UserControlInventario | ✅ UserControlPago, UserControlCaja | ✅ LoginSauna, UserControlReporte |

---

## ✅ REGLAS DE ORO + EJEMPLOS BÁSICOS

### 📋 Regla 1: SIEMPRE crear DTO por cada entidad

**¿Qué es un DTO?** Versión simple de la Entity para la UI

```
Ejemplo ClienteDTO.cs:
┌────────────────────────────┐
│ class ClienteDTO           │
│ {                          │
│     int IdCliente          │
│     string Nombre          │
│     string Dni             │
│     string Telefono        │
│     string Email           │
│     DateTime FechaNac      │
│     int VisitasTotales     │
│ }                          │
└────────────────────────────┘

Ejemplo ProductoDTO.cs:
┌────────────────────────────┐
│ class ProductoDTO          │
│ {                          │
│     int IdProducto         │
│     string Nombre          │
│     decimal Precio         │
│     int Stock              │
│     int StockMinimo        │
│     int IdCategoria        │
│     string NombreCategoria │ ← Para mostrar en UI
│ }                          │
└────────────────────────────┘

Ejemplo PagoDTO.cs:
┌────────────────────────────┐
│ class PagoDTO              │
│ {                          │
│     int IdPago             │
│     int IdCuenta           │
│     decimal Monto          │
│     DateTime FechaHora     │
│     int IdMetodoPago       │
│     string NombreMetodo    │ ← "Efectivo" o "Tarjeta"
│ }                          │
└────────────────────────────┘
```

**🎯 Consejo:** Solo propiedades básicas. Si necesitas mostrar "nombre de categoría", agrégala como string simple.

---

### 📋 Regla 2: SIEMPRE crear Service por cada módulo

**¿Qué hace un Service?** Valida y aplica reglas de negocio

```
Responsabilidades del Service:
┌───────────────────────────────────────────────────┐
│ 1️⃣ Validar datos ANTES de guardar                │
│    • ¿DNI único?                                  │
│    • ¿Email válido?                               │
│    • ¿Stock suficiente?                           │
│                                                    │
│ 2️⃣ Aplicar reglas de negocio                     │
│    • Calcular total de cuenta                     │
│    • Descontar stock automáticamente              │
│    • Detectar 5ta visita gratis                   │
│                                                    │
│ 3️⃣ Mapear Entity ↔ DTO                           │
│    • Convertir Cliente → ClienteDTO               │
│    • Convertir ClienteDTO → Cliente               │
│                                                    │
│ 4️⃣ Llamar al Repository                          │
│    • Guardar en BD                                │
│    • Obtener de BD                                │
└───────────────────────────────────────────────────┘
```

---

### 📋 Regla 3: SIEMPRE crear ViewModel por cada pantalla

**¿Qué contiene un ViewModel?** Propiedades + Comandos

```
Ejemplo ClientesViewModel:
┌─────────────────────────────────────────────┐
│ PROPIEDADES (lo que el usuario escribe):   │
│ • string NombreCliente                      │
│ • string DniCliente                         │
│ • string TelefonoCliente                    │
│ • string EmailCliente                       │
│                                              │
│ LISTA (lo que se muestra en DataGrid):     │
│ • ObservableCollection<ClienteDTO> Clientes│
│                                              │
│ COMANDOS (botones):                         │
│ • ICommand GuardarCommand                   │
│ • ICommand BuscarCommand                    │
│ • ICommand EliminarCommand                  │
│ • ICommand LimpiarCommand                   │
└─────────────────────────────────────────────┘
```

---

### 📋 Regla 4: SIEMPRE crear View (XAML) por cada módulo

**¿Qué va en el XAML?** Diseño visual con binding

```
Elementos básicos:
┌─────────────────────────────────────────────┐
│ TextBox → {Binding NombreCliente}          │
│ TextBox → {Binding DniCliente}             │
│ Button  → {Binding GuardarCommand}         │
│ DataGrid → {Binding Clientes}              │
└─────────────────────────────────────────────┘

⚠️ El .xaml.cs debe estar casi VACÍO
Solo tiene: InitializeComponent();
```

---

### 📋 Regla 5: CREAR Repository EXCEPTO para Cierre y Reportes

```
✅ SÍ crear Repository para:
┌────────────────────────────┐
│ • Cliente                  │
│ • Producto                 │
│ • Cuenta                   │
│ • DetalleConsumo           │
│ • Pago                     │
│ • Comprobante              │
│ • MovimientoInventario     │
│ • Egreso                   │
│ • Usuario                  │
│ • Rol                      │
│ • (8 catálogos más)        │
└────────────────────────────┘

❌ NO crear Repository para:
┌────────────────────────────┐
│ • CierreCaja (no existe)   │
│ • Reporte (no existe)      │
│ • FlujoCaja (no existe)    │
└────────────────────────────┘
```

---

### 📋 Regla 6: ViewModel NUNCA llama a Repository directamente

```
❌ MAL:
ViewModel → Repository → BD

✅ BIEN:
ViewModel → Service → Repository → BD

¿Por qué?
El Service valida ANTES de guardar
```

---

### 📋 Regla 7: Service decide si usar Repository o queries directas

```
CASO NORMAL (con Repository):
ClienteService → ClienteRepository → BD

CASO ESPECIAL (sin Repository):
CajaService → Queries SQL directas → BD
ReporteService → Queries SQL directas → BD

¿Por qué?
Porque NO existen tablas CierreCaja ni Reporte
Todo se CALCULA sumando otras tablas
```

---

### 📋 Regla 8: View (XAML) solo muestra datos, NO tiene lógica

```
❌ MAL:
Poner código C# en el .xaml.cs

✅ BIEN:
Todo en el ViewModel
El .xaml.cs solo tiene:
public UserControlClientes()
{
    InitializeComponent();
}
```

---

## 🆘 AYUDA RÁPIDA

**"¿Dónde pongo validaciones?"**
→ En **Service** (ej: validar DNI único)

**"¿Dónde hago SELECT a la BD?"**
→ En **Repository** (para tablas normales) o **Service** (para Cierre/Reportes)

**"¿Dónde pongo propiedades para TextBox?"**
→ En **ViewModel**

**"¿Dónde diseño los botones y cajas de texto?"**
→ En **View (XAML)**

**"¿Debo crear Repository para Cierre de Caja?"**
→ ❌ NO - Solo Service con queries directas

**"¿Qué diferencia hay entre Entity y DTO?"**
→ **Entity**: Clase de BD con relaciones (NO tocar)
→ **DTO**: Versión simple para UI (TÚ creas)
