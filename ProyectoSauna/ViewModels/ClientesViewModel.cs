// ViewModels/ClientesViewModel.cs - COMPLETAMENTE CORREGIDO
using ProyectoSauna.Commands;
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ProyectoSauna.ViewModels
{
    public class ClientesViewModel : BaseViewModel, IDisposable
    {
        private readonly IClienteService _clienteService;
        
        // 🛡️ SERVICIOS DE CONCURRENCIA Y SEGURIDAD
        private readonly Services.ClienteUnicaService _clienteUnicaService;
        private readonly Services.ClienteValidacionService _clienteValidacionService;
        private readonly Services.ConcurrencyService _concurrencyService;
        private readonly Services.InventoryEventService _inventoryEventService;
        private readonly Services.ClienteEnEdicionService _clienteEnEdicionService;
        private readonly Models.SaunaDbContext _sharedContext;
        
        // 🔄 CONTROL DE SINCRONIZACIÓN INTELIGENTE
        private bool _actualizacionHabilitada = true;
        private bool _hayPendienteActualizacion = false;
        
        // 🔒 CONTROL DE EDICIÓN SIMULTÁNEA
        private int? _clienteEnEdicionActual = null;
        
        // 🚫 CONTROL DE DOBLE EJECUCIÓN
        private bool _isSettingClienteSeleccionado = false;
        
        private string _nombreUsuario => ProyectoSauna.Models.SesionActual.EstaLogueado 
            ? $"{ProyectoSauna.Models.SesionActual.NombreCompleto} ({ProyectoSauna.Models.SesionActual.Rol})"
            : Environment.UserName;

        // 🚫 PREVENCIÓN DE MENSAJES DUPLICADOS
        private string _ultimoMensajeConcurrencia = string.Empty;
        private DateTime _ultimoTiempoMensaje = DateTime.MinValue;

        private ObservableCollection<ClienteDTO> _clientes = new();
        public ObservableCollection<ClienteDTO> Clientes
        {
            get => _clientes;
            set { _clientes = value; OnPropertyChanged(); }
        }

        private ClienteDTO? _clienteSeleccionado;
        public ClienteDTO? ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                // 🚫 GUARD: Prevenir doble ejecución
                if (_isSettingClienteSeleccionado)
                    return;
                
                // 🛡️ VERIFICACIÓN PREVIA: Si es un nuevo cliente, verificar si está disponible
                if (value != null && value != _clienteSeleccionado)
                {
                    var estadoCliente = _clienteEnEdicionService.VerificarClienteEnEdicion(value.idCliente);
                    if (estadoCliente.enEdicion)
                    {
                        // 🔒 VERIFICAR SI EL USUARIO ACTUAL ES EL QUE ESTÁ EDITANDO
                        if (estadoCliente.usuarioEditor.Equals(_nombreUsuario, StringComparison.OrdinalIgnoreCase))
                        {
                            // ✅ ES EL MISMO USUARIO - PERMITIR SELECCIÓN SIN MENSAJE
                            System.Diagnostics.Debug.WriteLine($"🔄 Usuario {_nombreUsuario} regresando a su propio cliente {value.idCliente}");
                        }
                        else
                        {
                            // 🚫 CLIENTE BLOQUEADO POR OTRO USUARIO - SOLO MOSTRAR MENSAJE
                            var mensaje = $"⚠️ El cliente '{value.NombreCompleto}' ya está siendo editado por {estadoCliente.usuarioEditor}.\n\n" +
                                          "No puedes seleccionar este cliente mientras esté en edición.";
                            
                            MostrarMensajeConcurrenciaSinDuplicados(mensaje);
                            
                            // ❌ NO CAMBIAR LA SELECCIÓN - MANTENER LA ACTUAL
                            return; // Salir sin modificar _clienteSeleccionado
                        }
                    }
                }
                
                _isSettingClienteSeleccionado = true;
                
                try
                {
                    // Liberar bloqueo del cliente anterior
                    if (_clienteSeleccionado != null && _clienteEnEdicionActual.HasValue)
                    {
                        _clienteEnEdicionService.LiberarBloqueoCliente(_clienteEnEdicionActual.Value, _nombreUsuario);
                        _clienteEnEdicionActual = null;
                    }

                    _clienteSeleccionado = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TextoBotonAccion)); // ← NOTIFICAR CAMBIO DEL TEXTO DEL BOTÓN
                    
                    if (value != null)
                    {
                        // 🔒 SISTEMA DE BLOQUEO DE EDICIÓN SIMULTÁNEA RESTAURADO
                        // Previene que el mismo cliente sea editado desde múltiples ventanas
                        var resultado = _clienteEnEdicionService.IntentarBloquearCliente(value.idCliente, _nombreUsuario);
                        
                        if (resultado.exito)
                        {
                            _clienteEnEdicionActual = value.idCliente;
                            CargarDatosParaEditar(value);
                            PausarActualizaciones();
                        }
                        else
                        {
                            // 🚨 ERROR INESPERADO: El cliente debería estar disponible
                            System.Diagnostics.Debug.WriteLine($"❌ Error inesperado: Cliente {value.idCliente} no disponible después de verificación previa");
                            
                            // Cancelar selección SIN RECURSION
                            _clienteSeleccionado = null;
                            OnPropertyChanged(nameof(ClienteSeleccionado));
                            return;
                        }
                    }
                    else
                    {
                        _ = ReactivarActualizacionesAsync();
                    }
                }
                finally
                {
                    _isSettingClienteSeleccionado = false;
                }
            }
        }

        private int _idCliente;
        public int IdCliente
        {
            get => _idCliente;
            set { _idCliente = value; OnPropertyChanged(); }
        }

        private string _nombre = string.Empty;
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); }
        }

        private string _apellidos = string.Empty;
        public string Apellidos
        {
            get => _apellidos;
            set { _apellidos = value; OnPropertyChanged(); }
        }

        private string _numeroDocumento = string.Empty;
        public string NumeroDocumento
        {
            get => _numeroDocumento;
            set { _numeroDocumento = value; OnPropertyChanged(); }
        }

        private string _telefono = string.Empty;
        public string Telefono
        {
            get => _telefono;
            set { _telefono = value; OnPropertyChanged(); }
        }

        private string _correo = string.Empty;
        public string Correo
        {
            get => _correo;
            set { _correo = value; OnPropertyChanged(); }
        }

        private string _direccion = string.Empty;
        public string Direccion
        {
            get => _direccion;
            set { _direccion = value; OnPropertyChanged(); }
        }

        private DateTime? _fechaNacimiento;
        public DateTime? FechaNacimiento
        {
            get => _fechaNacimiento;
            set { _fechaNacimiento = value; OnPropertyChanged(); }
        }

        private string _textoBusqueda = string.Empty;
        private System.Threading.Timer? _searchTimer;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set 
            { 
                _textoBusqueda = value; 
                OnPropertyChanged();
                // Iniciar búsqueda automática con debounce
                IniciarBusquedaAutomatica();
            }
        }

        private string _tipoBusqueda = "DNI";
        public string TipoBusqueda
        {
            get => _tipoBusqueda;
            set 
            { 
                _tipoBusqueda = value; 
                OnPropertyChanged();
                // Ejecutar búsqueda inmediatamente cuando cambia el tipo
                _ = Task.Run(async () => await EjecutarBusquedaAsync());
            }
        }

        // 📋 GESTIÓN DE CLIENTES INACTIVOS - UI UNIFICADA
        private bool _mostrarInactivos = false;
        public bool MostrarInactivos
        {
            get => _mostrarInactivos;
            set 
            { 
                _mostrarInactivos = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(ClientesActuales));
                OnPropertyChanged(nameof(TituloLista));
                
                // Ejecutar búsqueda inmediatamente cuando cambia el filtro
                _ = Task.Run(async () => await EjecutarBusquedaAsync());
            }
        }

        private ObservableCollection<ClienteDTO> _clientesInactivos = new();
        public ObservableCollection<ClienteDTO> ClientesInactivos
        {
            get => _clientesInactivos;
            set { _clientesInactivos = value; OnPropertyChanged(); OnPropertyChanged(nameof(ClientesActuales)); }
        }

        // 🔄 COLECCIÓN UNIFICADA PARA UN SOLO DATAGRID
        public ObservableCollection<ClienteDTO> ClientesActuales
        {
            get 
            {
                try 
                {
                    return MostrarInactivos ? (ClientesInactivos ?? new ObservableCollection<ClienteDTO>()) : (Clientes ?? new ObservableCollection<ClienteDTO>());
                }
                catch
                {
                    return new ObservableCollection<ClienteDTO>();
                }
            }
        }

        // 📝 TÍTULO DINÁMICO PARA LA LISTA
        public string TituloLista
        {
            get => MostrarInactivos ? "🚫 Clientes Inactivos" : "✅ Clientes Activos";
        }

        // 📝 TEXTO DINÁMICO PARA EL BOTÓN DE ACCIÓN
        public string TextoBotonAccion
        {
            get
            {
                if (ClienteSeleccionado == null) return "Seleccionar Cliente";
                return ClienteSeleccionado.activo ? "🚫 Desactivar" : "✅ Reactivar";
            }
        }

        private string _textoBusquedaInactivos = string.Empty;
        public string TextoBusquedaInactivos
        {
            get => _textoBusquedaInactivos;
            set { _textoBusquedaInactivos = value; OnPropertyChanged(); }
        }

        private bool _modoEdicion = false;
        public bool ModoEdicion
        {
            get => _modoEdicion;
            set { _modoEdicion = value; OnPropertyChanged(); OnPropertyChanged(nameof(TextoBotonGuardar)); }
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private string _mensajeEstado = string.Empty;
        public string MensajeEstado
        {
            get => _mensajeEstado;
            set { _mensajeEstado = value; OnPropertyChanged(); }
        }

        public string TextoBotonGuardar => ModoEdicion ? "Actualizar" : "Registrar";

        public ICommand GuardarClienteCommand { get; }
        public ICommand BuscarClienteCommand { get; }
        public ICommand MostrarTodosCommand { get; }
        public ICommand ToggleActivarClienteCommand { get; } // ← COMANDO UNIFICADO
        public ICommand BuscarClienteInactivoCommand { get; }
        public ICommand LimpiarFormularioCommand { get; }

        public ClientesViewModel(IClienteService clienteService)
        {
            _clienteService = clienteService;
            
            // 🛡️ INICIALIZAR SERVICIOS DE CONCURRENCIA
            _sharedContext = new Models.SaunaDbContext();
            _clienteUnicaService = new Services.ClienteUnicaService(_sharedContext);
            _clienteValidacionService = new Services.ClienteValidacionService(_sharedContext);
            _concurrencyService = new Services.ConcurrencyService(_sharedContext);
            _inventoryEventService = Services.InventoryEventService.Instance;
            _clienteEnEdicionService = new Services.ClienteEnEdicionService();

            // 🔄 SUSCRIPCIÓN A EVENTOS DE SINCRONIZACIÓN ENTRE VENTANAS
            _inventoryEventService.StockChanged += OnClienteChanged_SincronizarTablas;

            // 🔒 CONFIGURAR TIMER PARA VERIFICACIÓN DE BLOQUEOS EN TIEMPO REAL
            _verificacionBloqueoTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _verificacionBloqueoTimer.Tick += (s, e) => VerificarEstadoEdicionClientes();
            _verificacionBloqueoTimer.Start();

            GuardarClienteCommand = new Commands.AsyncRelayCommand(GuardarClienteCommandExecuteAsync);
            BuscarClienteCommand = new Commands.AsyncRelayCommand(BuscarClienteCommandExecuteAsync);
            MostrarTodosCommand = new Commands.AsyncRelayCommand(MostrarTodosCommandExecuteAsync);
            ToggleActivarClienteCommand = new Commands.AsyncRelayCommand(ToggleActivarClienteCommandExecuteAsync);
            BuscarClienteInactivoCommand = new Commands.AsyncRelayCommand(BuscarClienteInactivoCommandExecuteAsync);
            LimpiarFormularioCommand = new Commands.RelayCommand(LimpiarFormularioWrapper);

            // Cargar clientes al inicializar - en el UI thread
            _ = Task.Run(async () => 
            {
                await Application.Current.Dispatcher.InvokeAsync(async () => 
                {
                    await CargarClientesSegunFiltroAsync();
                });
            });
        }

        // 🛡️ MÉTODO SEGURO PARA GUARDAR CLIENTE CON CONTROL DE CONCURRENCIA
        private async Task GuardarClienteSeguroAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                MensajeEstado = "Validando datos...";

                var nombre = (Nombre ?? string.Empty).Trim();
                var apellidos = (Apellidos ?? string.Empty).Trim();
                var dni = (NumeroDocumento ?? string.Empty).Trim();
                var telefono = (Telefono ?? string.Empty).Trim();
                var correo = (Correo ?? string.Empty).Trim();
                var direccion = (Direccion ?? string.Empty).Trim();

                // 🔍 VALIDACIONES BÁSICAS
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    MessageBox.Show("El nombre es obligatorio.", "❌ Datos Inválidos",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    MensajeEstado = "❌ Datos inválidos";
                    return;
                }

                if (nombre.Length < 3)
                {
                    MessageBox.Show("El nombre debe tener al menos 3 caracteres.", "❌ Datos Inválidos",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    MensajeEstado = "❌ Datos inválidos";
                    return;
                }

                if (string.IsNullOrWhiteSpace(apellidos))
                {
                    MessageBox.Show("Los apellidos son obligatorios.", "❌ Datos Inválidos",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    MensajeEstado = "❌ Datos inválidos";
                    return;
                }

                if (apellidos.Length < 3)
                {
                    MessageBox.Show("Los apellidos deben tener al menos 3 caracteres.", "❌ Datos Inválidos",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    MensajeEstado = "❌ Datos inválidos";
                    return;
                }

                if (string.IsNullOrWhiteSpace(dni))
                {
                    MessageBox.Show("El número de documento es obligatorio.", "❌ Datos Inválidos",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    MensajeEstado = "❌ Datos inválidos";
                    return;
                }

                if (!Regex.IsMatch(dni, @"^\d{8}$"))
                {
                    MessageBox.Show("El DNI debe tener exactamente 8 dígitos (solo números).", "❌ Datos Inválidos",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    MensajeEstado = "❌ Datos inválidos";
                    return;
                }

                if (string.IsNullOrWhiteSpace(telefono))
                {
                    MessageBox.Show("El teléfono es obligatorio.", "❌ Datos Inválidos",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    MensajeEstado = "❌ Datos inválidos";
                    return;
                }

                if (!Regex.IsMatch(telefono, @"^\d{9}$"))
                {
                    MessageBox.Show("El teléfono debe tener exactamente 9 dígitos (solo números).", "❌ Datos Inválidos",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    MensajeEstado = "❌ Datos inválidos";
                    return;
                }

                if (string.IsNullOrWhiteSpace(correo))
                {
                    MessageBox.Show("El correo electrónico es obligatorio.", "❌ Datos Inválidos",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    MensajeEstado = "❌ Datos inválidos";
                    return;
                }

                if (!IsValidEmailSimple(correo))
                {
                    MessageBox.Show("El correo debe contener un '@' y caracteres antes y después.", "❌ Datos Inválidos",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    MensajeEstado = "❌ Datos inválidos";
                    return;
                }

                if (!string.IsNullOrWhiteSpace(direccion) && direccion.Length < 3)
                {
                    MessageBox.Show("La dirección debe tener al menos 3 caracteres.", "❌ Datos Inválidos",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    MensajeEstado = "❌ Datos inválidos";
                    return;
                }

                // Normalizar valores para guardar (evitar espacios)
                Nombre = nombre;
                Apellidos = apellidos;
                NumeroDocumento = dni;
                Telefono = telefono;
                Correo = correo;
                Direccion = direccion;

                if (ModoEdicion)
                {
                    await ActualizarClienteSeguroAsync();
                }
                else
                {
                    await CrearClienteSeguroAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "❌ Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                MensajeEstado = "❌ Error inesperado";
                System.Diagnostics.Debug.WriteLine($"❌ Error en GuardarClienteSeguroAsync: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private static bool IsValidEmailSimple(string email)
        {
            // Requisito: debe tener un '@' y caracteres antes y después.
            // (Validación simple y consistente con Usuarios)
            if (string.IsNullOrWhiteSpace(email)) return false;

            var trimmed = email.Trim();
            if (trimmed.Contains(' ')) return false;

            var at = trimmed.IndexOf('@');
            if (at <= 0) return false;
            if (at >= trimmed.Length - 1) return false;
            if (trimmed.LastIndexOf('@') != at) return false;

            return true;
        }

        // 🆕 CREACIÓN SEGURA DE CLIENTE
        private async Task CrearClienteSeguroAsync()
        {
            MensajeEstado = "Creando cliente...";

            // 🛡️ USAR SERVICIO SEGURO PARA EVITAR DUPLICADOS
            var resultado = await _clienteUnicaService.CrearClienteSeguroAsync(
                Nombre, Apellidos, NumeroDocumento, Telefono, Correo, Direccion, FechaNacimiento);

            if (resultado.exito)
            {
                // 🔄 NOTIFICAR A OTRAS VENTANAS
                _inventoryEventService?.OnStockChanged(new Services.StockChangedEventArgs
                {
                    ProductoId = 0,
                    NuevoStock = 0,
                    TipoMovimiento = "CLIENTE_CREADO",
                    IdCuenta = resultado.idClienteCreado,
                    Descripcion = $"{Nombre} {Apellidos}"
                });

                await RecargarTablaClientesAsync();
                LimpiarFormulario();
                MensajeEstado = "✅ Cliente creado exitosamente";
                
                MessageBox.Show(resultado.mensaje, "✅ Cliente Creado",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(resultado.mensaje, "❌ Error de Creación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                MensajeEstado = "❌ Error al crear cliente";
            }
        }

        // ✏️ ACTUALIZACIÓN SEGURA DE CLIENTE
        private async Task ActualizarClienteSeguroAsync()
        {
            MensajeEstado = "Actualizando cliente...";

            var clienteDto = new ClienteDTO
            {
                idCliente = IdCliente,
                nombre = Nombre,
                apellidos = Apellidos,
                numero_documento = NumeroDocumento,
                telefono = Telefono,
                correo = Correo,
                direccion = Direccion,
                fechaNacimiento = FechaNacimiento
            };

            var resultado = await _clienteService.ActualizarClienteAsync(clienteDto);

            if (resultado.exito)
            {
                // 🔄 NOTIFICAR A OTRAS VENTANAS
                _inventoryEventService?.OnStockChanged(new Services.StockChangedEventArgs
                {
                    ProductoId = 0,
                    NuevoStock = 0,
                    TipoMovimiento = "CLIENTE_MODIFICADO",
                    IdCuenta = IdCliente,
                    Descripcion = $"{Nombre} {Apellidos}"
                });

                await RecargarTablaClientesAsync();
                LimpiarFormulario();
                MensajeEstado = "✅ Cliente actualizado correctamente";
                
                MessageBox.Show($"Cliente '{clienteDto.nombre} {clienteDto.apellidos}' actualizado correctamente.",
                    "✅ Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(resultado.mensaje, "❌ Error de Actualización",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                MensajeEstado = "❌ Error al actualizar cliente";
            }
        }

        // 🗑️ DESACTIVACIÓN SIMPLIFICADA DE CLIENTE (SIN CONCURRENCIA)
        private async Task DesactivarClienteSimpleAsync()
        {
            if (ClienteSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un cliente para desactivar.", "❌ Información",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Guardar referencia antes de limpiar
            var nombreCompleto = $"{ClienteSeleccionado.nombre} {ClienteSeleccionado.apellidos}";
            var clienteId = ClienteSeleccionado.idCliente;

            var confirmacion = MessageBox.Show(
                $"¿Está seguro de que desea DESACTIVAR al cliente?\n\n" +
                $"• Nombre: {ClienteSeleccionado.nombre} {ClienteSeleccionado.apellidos}\n" +
                $"• Documento: {ClienteSeleccionado.numero_documento}\n\n" +
                $"❌ El cliente será desactivado temporalmente.\n" +
                $"✅ Podrá reactivarlo posteriormente desde la vista de inactivos.",
                "⚠️ Confirmar Desactivación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                MensajeEstado = "Desactivando cliente...";

                var resultado = await _clienteService.DesactivarClienteAsync(clienteId);

                if (resultado.exito)
                {
                    // 🔄 NOTIFICAR A OTRAS VENTANAS
                    _inventoryEventService?.OnStockChanged(new Services.StockChangedEventArgs
                    {
                        ProductoId = 0,
                        NuevoStock = 0,
                        TipoMovimiento = "CLIENTE_DESACTIVADO",
                        IdCuenta = clienteId,
                        Descripcion = nombreCompleto
                    });

                    // Limpiar primero para evitar problemas de referencia
                    LimpiarFormulario();
                    
                    // Luego recargar la tabla
                    await CargarClientesSegunFiltroAsync();
                    
                    MensajeEstado = "✅ Cliente desactivado correctamente";
                    
                    MessageBox.Show($"Cliente '{nombreCompleto}' desactivado correctamente.",
                        "✅ Cliente Desactivado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(resultado.mensaje, "❌ Error al desactivar",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    MensajeEstado = "❌ Error al desactivar cliente";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al desactivar cliente: {ex.Message}", "❌ Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                MensajeEstado = "❌ Error inesperado";
                System.Diagnostics.Debug.WriteLine($"❌ Error en DesactivarClienteSimpleAsync: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GuardarClienteAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                MensajeEstado = "Guardando...";

                var clienteDto = new ClienteDTO
                {
                    idCliente = IdCliente,
                    nombre = Nombre,
                    apellidos = Apellidos,
                    numero_documento = NumeroDocumento,
                    telefono = Telefono,
                    correo = Correo,
                    direccion = Direccion,
                    fechaNacimiento = FechaNacimiento
                };

                if (ModoEdicion)
                {
                    var resultado = await _clienteService.ActualizarClienteAsync(clienteDto);

                    if (resultado.exito)
                    {
                        TextoBusqueda = string.Empty;
                        await CargarTodosLosClientesAsync();
                        LimpiarFormulario();
                        MensajeEstado = "✅ Cliente actualizado correctamente";
                        MessageBox.Show($"Cliente '{clienteDto.nombre} {clienteDto.apellidos}' actualizado correctamente.",
                            "✅ Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(resultado.mensaje, "❌ Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        MensajeEstado = "❌ Error al actualizar cliente";
                    }
                }
                else
                {
                    var resultado = await _clienteService.CrearClienteAsync(clienteDto);

                    if (resultado.exito)
                    {
                        // 🔄 NOTIFICAR A OTRAS VENTANAS
                        _inventoryEventService?.OnStockChanged(new Services.StockChangedEventArgs
                        {
                            ProductoId = 0,
                            NuevoStock = 0,
                            TipoMovimiento = "CLIENTE_CREADO",
                            IdCuenta = resultado.cliente?.idCliente ?? 0,
                            Descripcion = $"{clienteDto.nombre} {clienteDto.apellidos}"
                        });

                        TextoBusqueda = string.Empty;
                        await CargarTodosLosClientesAsync();
                        LimpiarFormulario();
                        MensajeEstado = $"✅ Cliente registrado correctamente. Total: {Clientes.Count} activos";
                        MessageBox.Show($"Cliente '{clienteDto.nombre} {clienteDto.apellidos}' registrado correctamente.",
                            "✅ Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(resultado.mensaje, "❌ Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        MensajeEstado = "❌ Error al registrar cliente";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}\n\nPor favor, contacte al administrador del sistema.",
                    "❌ Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                MensajeEstado = "Error crítico";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task BuscarClienteAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                MensajeEstado = "Buscando...";

                if (string.IsNullOrWhiteSpace(TextoBusqueda))
                {
                    await CargarTodosLosClientesAsync();
                    return;
                }

                using var context = new ProyectoSauna.Models.SaunaDbContext();
                var repo = new ProyectoSauna.Repositories.ClienteRepository(context);
                var servicio = new ProyectoSauna.Services.ClienteService(repo);

                if (TipoBusqueda.ToUpper() == "DNI")
                {
                    // Búsqueda parcial por DNI (como LIKE)
                    var clientesEncontrados = await servicio.BuscarClientesPorDNIAsync(TextoBusqueda.Trim());
                    
                    if (clientesEncontrados.Any())
                    {
                        // Filtrar por estado según la selección
                        var clientesFiltrados = clientesEncontrados.Where(c => 
                            MostrarInactivos ? !c.activo : c.activo).ToList();
                        
                        if (clientesFiltrados.Any())
                        {
                            // Actualizar la colección correcta según el estado
                            if (MostrarInactivos)
                            {
                                ClientesInactivos = new ObservableCollection<ClienteDTO>(clientesFiltrados);
                                Clientes = new ObservableCollection<ClienteDTO>(); // Limpiar la otra lista
                            }
                            else
                            {
                                Clientes = new ObservableCollection<ClienteDTO>(clientesFiltrados);
                                ClientesInactivos = new ObservableCollection<ClienteDTO>(); // Limpiar la otra lista
                            }
                            MensajeEstado = $"{clientesFiltrados.Count} cliente(s) encontrado(s)";
                        }
                        else
                        {
                            // Hay clientes con ese DNI pero no coinciden con el filtro
                            Clientes = new ObservableCollection<ClienteDTO>();
                            ClientesInactivos = new ObservableCollection<ClienteDTO>();
                            string estadoBuscado = MostrarInactivos ? "inactivos" : "activos";
                            MensajeEstado = $"Encontrados clientes con DNI que contiene '{TextoBusqueda}', pero ninguno está en {estadoBuscado}";
                        }
                    }
                    else
                    {
                        // No se encontraron clientes
                        Clientes = new ObservableCollection<ClienteDTO>();
                        ClientesInactivos = new ObservableCollection<ClienteDTO>();
                        MensajeEstado = $"No se encontraron clientes con DNI que contenga '{TextoBusqueda}'";
                    }
                }
                else // Búsqueda por nombre
                {
                    var clientesEncontrados = await servicio.BuscarClientesPorNombreAsync(TextoBusqueda.Trim());
                    
                    if (MostrarInactivos)
                    {
                        var clientesInactivos = clientesEncontrados.Where(c => !c.activo).ToList();
                        ClientesInactivos = new ObservableCollection<ClienteDTO>(clientesInactivos);
                        Clientes = new ObservableCollection<ClienteDTO>(); // Limpiar la otra lista
                        MensajeEstado = clientesInactivos.Count == 0 
                            ? "No se encontraron clientes inactivos con ese nombre" 
                            : $"{clientesInactivos.Count} cliente(s) inactivo(s) encontrado(s)";
                    }
                    else
                    {
                        var clientesActivos = clientesEncontrados.Where(c => c.activo).ToList();
                        Clientes = new ObservableCollection<ClienteDTO>(clientesActivos);
                        ClientesInactivos = new ObservableCollection<ClienteDTO>(); // Limpiar la otra lista
                        MensajeEstado = clientesActivos.Count == 0 
                            ? "No se encontraron clientes activos con ese nombre" 
                            : $"{clientesActivos.Count} cliente(s) activo(s) encontrado(s)";
                    }
                }
                
                // Actualizar la UI
                OnPropertyChanged(nameof(ClientesActuales));
            }
            catch (Exception ex)
            {
                // Para búsqueda manual, mostrar el error
                MessageBox.Show($"Error al buscar cliente: {ex.Message}\n\nPor favor, intente nuevamente.",
                    "❌ Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MensajeEstado = "Error en búsqueda";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CargarTodosLosClientesAsync()
        {
            await CargarClientesSegunFiltroAsync();
        }

        private async Task CargarClientesSegunFiltroAsync()
        {
            try
            {
                IsLoading = true;
                MensajeEstado = "Cargando clientes...";

                using var context = new ProyectoSauna.Models.SaunaDbContext();
                var repo = new ProyectoSauna.Repositories.ClienteRepository(context);
                var servicio = new ProyectoSauna.Services.ClienteService(repo);

                if (MostrarInactivos)
                {
                    var clientesInactivos = await servicio.GetClientesInactivosAsync();
                    if (clientesInactivos != null)
                    {
                        ClientesInactivos = new ObservableCollection<ClienteDTO>(clientesInactivos.OrderByDescending(c => c.fechaRegistro));
                        MensajeEstado = $"{ClientesInactivos.Count} cliente(s) inactivo(s)";
                    }
                    else
                    {
                        ClientesInactivos = new ObservableCollection<ClienteDTO>();
                        MensajeEstado = "0 cliente(s) inactivo(s)";
                    }
                }
                else
                {
                    var clientesActivos = await servicio.GetClientesActivosAsync();
                    if (clientesActivos != null)
                    {
                        Clientes = new ObservableCollection<ClienteDTO>(clientesActivos.OrderByDescending(c => c.fechaRegistro));
                        
                        // 🎯 CONFIGURAR ESTADO DE RADIOBUTTONS SEGÚN BLOQUEO
                        foreach (var cliente in Clientes)
                        {
                            var estadoBloqueo = _clienteEnEdicionService.VerificarClienteEnEdicion(cliente.idCliente);
                            if (estadoBloqueo.enEdicion && !estadoBloqueo.usuarioEditor.Equals(_nombreUsuario, StringComparison.OrdinalIgnoreCase))
                            {
                                // Cliente bloqueado por otro usuario - RadioButton deshabilitado
                                cliente.IsRadioButtonEnabled = false;
                                System.Diagnostics.Debug.WriteLine($"🔒 RadioButton deshabilitado para cliente {cliente.idCliente} (editado por {estadoBloqueo.usuarioEditor})");
                            }
                            else
                            {
                                // Cliente disponible - RadioButton habilitado
                                cliente.IsRadioButtonEnabled = true;
                            }
                        }
                        
                        MensajeEstado = $"{Clientes.Count} cliente(s) activo(s)";
                    }
                    else
                    {
                        Clientes = new ObservableCollection<ClienteDTO>();
                        MensajeEstado = "0 cliente(s) activo(s)";
                    }
                }
                
                OnPropertyChanged(nameof(ClientesActuales));
                
                // 🔄 ACTIVAR VERIFICACIÓN TRAS CARGAR DATOS
                VerificarEstadoEdicionClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}\n\nVerifique la conexión a la base de datos.",
                    "❌ Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MensajeEstado = "Error al cargar datos";
                
                // Asegurar que las colecciones no queden en estado nulo
                if (MostrarInactivos)
                {
                    ClientesInactivos = new ObservableCollection<ClienteDTO>();
                }
                else
                {
                    Clientes = new ObservableCollection<ClienteDTO>();
                }
                OnPropertyChanged(nameof(ClientesActuales));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CargarDatosParaEditar(ClienteDTO cliente)
        {
            IdCliente = cliente.idCliente;
            Nombre = cliente.nombre;
            Apellidos = cliente.apellidos;
            NumeroDocumento = cliente.numero_documento;
            Telefono = cliente.telefono ?? string.Empty;
            Correo = cliente.correo ?? string.Empty;
            Direccion = cliente.direccion ?? string.Empty;
            FechaNacimiento = cliente.fechaNacimiento;
            ModoEdicion = true;
        }

        public void LimpiarFormulario()
        {
            // 🔓 Liberar bloqueo de cliente si existe
            if (_clienteEnEdicionActual.HasValue)
            {
                _clienteEnEdicionService.LiberarBloqueoCliente(_clienteEnEdicionActual.Value, _nombreUsuario);
                _clienteEnEdicionActual = null;
            }

            IdCliente = 0;
            Nombre = string.Empty;
            Apellidos = string.Empty;
            NumeroDocumento = string.Empty;
            Telefono = string.Empty;
            Correo = string.Empty;
            Direccion = string.Empty;
            FechaNacimiento = null;
            ModoEdicion = false;
            ClienteSeleccionado = null;
            MensajeEstado = string.Empty;
        }

        private void CancelarEdicion()
        {
            LimpiarFormulario();
        }



        #region 🔄 Sincronización en Tiempo Real

        private readonly DispatcherTimer _verificacionBloqueoTimer;

        #endregion

        #region 🔄 Sincronización entre Ventanas

        /// <summary>
        /// 🔄 Maneja la sincronización entre ventanas cuando se crean, modifican o eliminan clientes
        /// INTELIGENTE: No actualiza si hay cliente seleccionado para evitar perder selección
        /// </summary>
        private async void OnClienteChanged_SincronizarTablas(object sender, Services.StockChangedEventArgs e)
        {
            try
            {
                // Solo sincronizar cuando se trata de eventos de clientes
                if (e.TipoMovimiento == "CLIENTE_CREADO" || 
                    e.TipoMovimiento == "CLIENTE_MODIFICADO" || 
                    e.TipoMovimiento == "CLIENTE_ELIMINADO" ||
                    e.TipoMovimiento == "CLIENTE_DESACTIVADO" ||
                    e.TipoMovimiento == "CLIENTE_REACTIVADO")
                {
                    await Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        // 🧠 ACTUALIZACIÓN INTELIGENTE
                        if (!_actualizacionHabilitada || ClienteSeleccionado != null)
                        {
                            // Marcar que hay una actualización pendiente
                            _hayPendienteActualizacion = true;
                            System.Diagnostics.Debug.WriteLine($"📋 Actualización de tabla PAUSADA - Hay cliente seleccionado: {ClienteSeleccionado?.nombre} {ClienteSeleccionado?.apellidos}");
                            return;
                        }

                        // Realizar actualización inmediata
                        await RecargarTablaClientesAsync();
                        
                        System.Diagnostics.Debug.WriteLine($"🔄 Tabla de clientes sincronizada: {e.TipoMovimiento} - {e.Descripcion}");
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en sincronización de clientes: {ex.Message}");
            }
        }

        /// <summary>
        /// 🔄 Recarga la tabla de clientes manteniendo el filtro actual
        /// </summary>
        private async Task RecargarTablaClientesAsync()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(TextoBusqueda))
                {
                    // Mantener búsqueda actual
                    await BuscarClienteAsync();
                }
                else
                {
                    // Recargar según filtro actual (activos/inactivos)
                    await CargarClientesSegunFiltroAsync();
                }
                
                // 🔒 Verificar bloqueos tras recargar
                VerificarEstadoEdicionClientes();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error recargando tabla de clientes: {ex.Message}");
            }
        }

        /// <summary>
        /// 🔒 Verifica el estado de edición de todos los clientes para mostrar indicadores visuales
        /// </summary>
        private void VerificarEstadoEdicionClientes()
        {
            try
            {
                if (Clientes == null || !Clientes.Any()) return;

                var usuarioActual = string.IsNullOrEmpty(ProyectoSauna.Models.SesionActual.NombreCompleto) 
                    ? Environment.UserName ?? "Usuario" 
                    : ProyectoSauna.Models.SesionActual.NombreCompleto;

                System.Diagnostics.Debug.WriteLine($"🔍 Verificando estado de edición de {Clientes.Count} clientes...");

                foreach (var cliente in Clientes)
                {
                    var estadoAnterior = cliente.IsRadioButtonEnabled;
                    
                    // 🔍 VERIFICAR ESTADO ACTUAL DEL BLOQUEO usando ClienteEnEdicionService
                    var estado = _clienteEnEdicionService.VerificarClienteEnEdicion(cliente.idCliente);
                    
                    // ✅ Cliente está habilitado SI:
                    // - No está en edición, O
                    // - Está en edición por el usuario actual
                    bool debeEstarHabilitado = !estado.enEdicion || 
                        (!string.IsNullOrEmpty(estado.usuarioEditor) &&
                         estado.usuarioEditor.Equals(usuarioActual, StringComparison.OrdinalIgnoreCase));
                    
                    // 🔄 ACTUALIZAR ESTADO SI HAY CAMBIO
                    if (estadoAnterior != debeEstarHabilitado)
                    {
                        Application.Current?.Dispatcher.InvokeAsync(() =>
                        {
                            cliente.IsRadioButtonEnabled = debeEstarHabilitado;
                            System.Diagnostics.Debug.WriteLine($"🔄 Cliente {cliente.idCliente} ({cliente.nombre}) RadioButton: {estadoAnterior} → {debeEstarHabilitado} (Editor: {estado.usuarioEditor})");
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error verificando estado de clientes: {ex.Message}");
            }
        }

        /// <summary>
        /// ⏸️ Pausa las actualizaciones automáticas cuando se selecciona un cliente
        /// </summary>
        public void PausarActualizaciones()
        {
            _actualizacionHabilitada = false;
            System.Diagnostics.Debug.WriteLine("⏸️ Actualizaciones de tabla pausadas - Cliente seleccionado");
        }

        /// <summary>
        /// ▶️ Reactiva las actualizaciones automáticas y procesa pendientes
        /// </summary>
        public async Task ReactivarActualizacionesAsync()
        {
            _actualizacionHabilitada = true;
            
            if (_hayPendienteActualizacion)
            {
                _hayPendienteActualizacion = false;
                await RecargarTablaClientesAsync();
                System.Diagnostics.Debug.WriteLine("▶️ Actualizaciones reactivadas - Procesando actualización pendiente");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("▶️ Actualizaciones reactivadas - Sin pendientes");
            }
        }

        /// <summary>
        /// 🧹 Limpia formulario y reactiva actualizaciones automáticamente
        /// </summary>
        public void LimpiarFormularioYReactivar()
        {
            LimpiarFormulario();
            _ = ReactivarActualizacionesAsync();
        }

        #endregion

        #region 📋 GESTIÓN DE CLIENTES INACTIVOS

        /// <summary>
        /// 📋 Carga todos los clientes inactivos
        /// </summary>
        private async Task CargarClientesInactivosAsync()
        {
            try
            {
                IsLoading = true;
                MensajeEstado = "Cargando clientes inactivos...";

                var clientesInactivos = await _clienteService.GetAllAsync();
                ClientesInactivos = new ObservableCollection<ClienteDTO>(
                    clientesInactivos.Where(c => !c.activo).OrderBy(c => c.apellidos));

                MensajeEstado = $"✅ {ClientesInactivos.Count} clientes inactivos cargados";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes inactivos:\n{ex.Message}",
                    "❌ Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MensajeEstado = "❌ Error cargando clientes inactivos";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 🔍 Busca clientes inactivos por criterio
        /// </summary>
        private async Task BuscarClienteInactivoAsync()
        {
            if (string.IsNullOrWhiteSpace(TextoBusquedaInactivos))
            {
                await CargarClientesSegunFiltroAsync(); // Usar método unificado
                return;
            }

            try
            {
                IsLoading = true;
                MensajeEstado = "Buscando clientes inactivos...";

                var todosLosInactivos = await _clienteService.GetAllAsync();
                var clientesFiltrados = todosLosInactivos
                    .Where(c => !c.activo && (
                        c.numero_documento.Contains(TextoBusquedaInactivos, StringComparison.OrdinalIgnoreCase) ||
                        c.nombre.Contains(TextoBusquedaInactivos, StringComparison.OrdinalIgnoreCase) ||
                        c.apellidos.Contains(TextoBusquedaInactivos, StringComparison.OrdinalIgnoreCase) ||
                        (c.correo?.Contains(TextoBusquedaInactivos, StringComparison.OrdinalIgnoreCase) ?? false)
                    ))
                    .OrderBy(c => c.apellidos);

                ClientesInactivos = new ObservableCollection<ClienteDTO>(clientesFiltrados);

                MensajeEstado = ClientesInactivos.Count > 0 
                    ? $"✅ {ClientesInactivos.Count} clientes inactivos encontrados"
                    : "ℹ️ No se encontraron clientes inactivos con ese criterio";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar clientes inactivos:\n{ex.Message}",
                    "❌ Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MensajeEstado = "❌ Error en búsqueda de inactivos";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Wrapper para el comando GuardarClienteCommand
        /// </summary>
        private async Task GuardarClienteCommandExecuteAsync(object? parameter)
        {
            await GuardarClienteSeguroAsync();
        }

        /// <summary>
        /// Wrapper para el comando BuscarClienteCommand
        /// </summary>
        private async Task BuscarClienteCommandExecuteAsync(object? parameter)
        {
            await BuscarClienteAsync();
        }

        /// <summary>
        /// Wrapper para el comando MostrarTodosCommand
        /// </summary>
        private async Task MostrarTodosCommandExecuteAsync(object? parameter)
        {
            await CargarTodosLosClientesAsync();
        }

        /// <summary>
        /// Wrapper para el comando unificado ToggleActivarClienteCommand
        /// </summary>
        private async Task ToggleActivarClienteCommandExecuteAsync(object? parameter)
        {
            if (ClienteSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un cliente de la lista.",
                    "ℹ️ Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ClienteSeleccionado.activo)
            {
                await DesactivarClienteSimpleAsync(); // Desactivar cliente
            }
            else
            {
                await ReactivarClienteSimpleAsync(); // Reactivar cliente
            }
        }

        /// <summary>
        /// Wrapper para el comando DesactivarClienteCommand (OBSOLETO - usar ToggleActivarClienteCommand)
        /// </summary>
        private async Task DesactivarClienteCommandExecuteAsync(object? parameter)
        {
            await DesactivarClienteSimpleAsync();
        }

        /// <summary>
        /// Wrapper para el comando LimpiarFormularioCommand
        /// </summary>
        private void LimpiarFormularioWrapper(object? parameter)
        {
            LimpiarFormularioYReactivar();
        }

        /// <summary>
        /// Wrapper para el comando BuscarClienteInactivoCommand
        /// </summary>
        private async Task BuscarClienteInactivoCommandExecuteAsync(object? parameter)
        {
            await BuscarClienteInactivoAsync();
        }

        /// <summary>
        /// Wrapper para el comando ReactivarClienteCommand
        /// </summary>
        private async Task ReactivarClienteCommandExecuteAsync(object? parameter)
        {
            await ReactivarClienteSimpleAsync();
        }

        /// <summary>
        /// 🔄 REACTIVACIÓN SIMPLIFICADA DE CLIENTE (SIN CONCURRENCIA)
        /// </summary>
        private async Task ReactivarClienteSimpleAsync()
        {
            if (ClienteSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un cliente inactivo de la lista para reactivar.",
                    "ℹ️ Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ClienteSeleccionado.activo)
            {
                MessageBox.Show("El cliente seleccionado ya está activo.",
                    "ℹ️ Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Está seguro que desea reactivar al cliente '{ClienteSeleccionado.NombreCompleto}'?\n\n" +
                $"✅ El cliente volverá a estar disponible para:\n" +
                $"• Crear nuevas cuentas\n" +
                $"• Realizar compras\n" +
                $"• Aparecer en búsquedas activas\n\n" +
                $"¿Desea continuar?",
                "✅ Confirmar Reactivación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (confirmacion != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;
                MensajeEstado = "Reactivando cliente...";

                var resultado = await _clienteService.ReactivarClienteAsync(ClienteSeleccionado.idCliente);

                if (resultado.exito)
                {
                    var nombreCliente = ClienteSeleccionado.NombreCompleto;

                    // 🔄 NOTIFICAR CAMBIOS A OTRAS VENTANAS
                    _inventoryEventService?.OnStockChanged(new Services.StockChangedEventArgs
                    {
                        ProductoId = 0,
                        NuevoStock = 0,
                        TipoMovimiento = "CLIENTE_REACTIVADO",
                        IdCuenta = ClienteSeleccionado.idCliente,
                        Descripcion = nombreCliente
                    });

                    // Recargar lista según filtro actual
                    await CargarClientesSegunFiltroAsync();
                    LimpiarFormulario();

                    MensajeEstado = "✅ Cliente reactivado correctamente";
                    MessageBox.Show($"Cliente '{nombreCliente}' reactivado correctamente.\nYa puede realizar operaciones normales.",
                        "✅ Cliente Reactivado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(resultado.mensaje ?? "Error desconocido al reactivar cliente",
                        "❌ Error de Reactivación", MessageBoxButton.OK, MessageBoxImage.Error);
                    MensajeEstado = "❌ Error al reactivar cliente";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al reactivar el cliente:\n{ex.Message}\n\nPor favor, contacte al administrador del sistema.",
                    "❌ Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                MensajeEstado = "Error crítico al reactivar";
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Búsqueda Automática con Debounce

        /// <summary>
        /// Inicia una búsqueda automática con retraso (debounce) para evitar demasiadas consultas
        /// </summary>
        private void IniciarBusquedaAutomatica()
        {
            // Cancelar el timer anterior si existe
            _searchTimer?.Dispose();
            
            // Crear nuevo timer que se ejecutará después de 500ms
            _searchTimer = new System.Threading.Timer(async _ => await EjecutarBusquedaAsync(), null, 500, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Ejecuta la búsqueda de manera unificada (para activos e inactivos)
        /// </summary>
        private async Task EjecutarBusquedaAsync()
        {
            try
            {
                // Ejecutar en el hilo de UI
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    if (IsLoading) return;

                    // Si no hay texto de búsqueda, cargar todos
                    if (string.IsNullOrWhiteSpace(TextoBusqueda))
                    {
                        await CargarClientesSegunFiltroAsync();
                        return;
                    }

                    // Ejecutar búsqueda unificada que ya maneja activos e inactivos
                    await BuscarClienteAsync();
                });
            }
            catch (Exception ex)
            {
                // Manejar errores silenciosamente para búsqueda automática
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MensajeEstado = "Error en búsqueda automática";
                });
            }
        }

        #endregion

        /// <summary>
        /// 🚫 Muestra mensaje de concurrencia evitando duplicados consecutivos
        /// Solo muestra si es diferente al último mensaje o han pasado más de 2 segundos
        /// </summary>
        private void MostrarMensajeConcurrenciaSinDuplicados(string mensaje)
        {
            var ahora = DateTime.Now;
            var tiempoTranscurrido = ahora - _ultimoTiempoMensaje;
            
            // Solo mostrar si es un mensaje diferente O han pasado más de 2 segundos
            if (mensaje != _ultimoMensajeConcurrencia || tiempoTranscurrido.TotalSeconds > 2)
            {
                MessageBox.Show(
                    mensaje,
                    "🔒 Cliente en Edición",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                
                _ultimoMensajeConcurrencia = mensaje;
                _ultimoTiempoMensaje = ahora;
            }
        }

        #region IDisposable Implementation

        public void Dispose()
        {
            // 🧹 LIMPIAR EVENTOS Y TIMERS
            if (_inventoryEventService != null)
            {
                _inventoryEventService.StockChanged -= OnClienteChanged_SincronizarTablas;
            }
            
            _verificacionBloqueoTimer?.Stop();
            _searchTimer?.Dispose();
            
            System.Diagnostics.Debug.WriteLine("🧹 ClientesViewModel disposed");
        }

        #endregion

    }
}