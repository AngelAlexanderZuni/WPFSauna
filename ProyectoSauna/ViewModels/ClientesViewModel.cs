using ProyectoSauna.Commands;
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ProyectoSauna.ViewModels
{
    public class ClientesViewModel : BaseViewModel
    {
        private readonly IClienteService _clienteService;

        // Propiedades observables
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
                _clienteSeleccionado = value;
                OnPropertyChanged();
                if (value != null)
                {
                    CargarDatosParaEditar(value);
                }
            }
        }

        // Propiedades del formulario
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
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set { _textoBusqueda = value; OnPropertyChanged(); }
        }

        private string _tipoBusqueda = "DNI";
        public string TipoBusqueda
        {
            get => _tipoBusqueda;
            set { _tipoBusqueda = value; OnPropertyChanged(); }
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

        // Comandos
        public ICommand GuardarClienteCommand { get; }
        public ICommand BuscarClienteCommand { get; }
        public ICommand LimpiarFormularioCommand { get; }
        public ICommand CancelarEdicionCommand { get; }
        public ICommand MostrarTodosCommand { get; }
        public ICommand DesactivarClienteCommand { get; }

        public ClientesViewModel(IClienteService clienteService)
        {
            _clienteService = clienteService;

            // Inicializar comandos
            GuardarClienteCommand = new AsyncRelayCommand(async _ => await GuardarClienteAsync());
            BuscarClienteCommand = new AsyncRelayCommand(async _ => await BuscarClienteAsync());
            LimpiarFormularioCommand = new RelayCommand(() => LimpiarFormulario());
            CancelarEdicionCommand = new RelayCommand(() => CancelarEdicion());
            MostrarTodosCommand = new AsyncRelayCommand(async _ => await CargarTodosLosClientesAsync());
            DesactivarClienteCommand = new AsyncRelayCommand(async _ => await DesactivarClienteAsync());


            // Cargar datos iniciales (solo activos)
            _ = CargarTodosLosClientesAsync();
        }

        private async System.Threading.Tasks.Task GuardarClienteAsync()
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
                    fechaNacimiento = FechaNacimiento.HasValue ? DateOnly.FromDateTime(FechaNacimiento.Value) : null
                };

                if (ModoEdicion)
                {
                    // Actualizar cliente existente
                    var resultado = await _clienteService.ActualizarClienteAsync(clienteDto);

                    if (resultado.exito)
                    {
                        // Ô£à Limpiar b├║squeda y recargar lista completa
                        TextoBusqueda = string.Empty;
                        await CargarTodosLosClientesAsync();
                        LimpiarFormulario();
                        MensajeEstado = "Ô£à Cliente actualizado correctamente";

                        // Mensaje breve (opcional)
                        MessageBox.Show($"Cliente '{clienteDto.nombre} {clienteDto.apellidos}' actualizado correctamente.",
                            "Ô£à ├ëxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(resultado.mensaje, "ÔØî Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        MensajeEstado = "ÔØî Error al actualizar cliente";
                    }
                }
                else
                {
                    // Crear nuevo cliente
                    var resultado = await _clienteService.CrearClienteAsync(clienteDto);

                    if (resultado.exito)
                    {
                        // Ô£à Limpiar b├║squeda y recargar lista completa
                        TextoBusqueda = string.Empty;
                        await CargarTodosLosClientesAsync();
                        LimpiarFormulario();
                        MensajeEstado = $"Ô£à Cliente registrado correctamente. Total: {Clientes.Count} activos";

                        // Mensaje breve (opcional)
                        MessageBox.Show($"Cliente '{clienteDto.nombre} {clienteDto.apellidos}' registrado correctamente.",
                            "Ô£à ├ëxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(resultado.mensaje, "ÔØî Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        MensajeEstado = "ÔØî Error al registrar cliente";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}\n\nPor favor, contacte al administrador del sistema.",
                    "ÔØî Error Cr├¡tico", MessageBoxButton.OK, MessageBoxImage.Error);
                MensajeEstado = "Error cr├¡tico";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async System.Threading.Tasks.Task BuscarClienteAsync()
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

                // Ô£à Crear un NUEVO servicio con contexto fresco para evitar cache
                using var context = new ProyectoSauna.Models.SaunaDbContext();
                var repo = new ProyectoSauna.Repositories.ClienteRepository(context);
                var servicio = new ProyectoSauna.Services.ClienteService(repo);

                if (TipoBusqueda == "DNI")
                {
                    var cliente = await servicio.GetClienteByDNIAsync(TextoBusqueda.Trim());
                    // Ô£à Solo mostrar si el cliente est├í activo
                    if (cliente != null && cliente.activo)
                    {
                        Clientes = new ObservableCollection<ClienteDTO> { cliente };
                        MensajeEstado = "1 cliente encontrado";
                    }
                    else
                    {
                        Clientes = new ObservableCollection<ClienteDTO>();
                        MessageBox.Show("No se encontr├│ ning├║n cliente activo con ese DNI.", "Ôä╣´©Å Sin resultados",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        MensajeEstado = "Sin resultados";
                    }
                }
                else // B├║squeda por nombre
                {
                    var clientesEncontrados = await servicio.BuscarClientesPorNombreAsync(TextoBusqueda.Trim());
                    // Ô£à Filtrar solo clientes activos
                    var clientesActivos = clientesEncontrados.Where(c => c.activo).ToList();

                    Clientes = new ObservableCollection<ClienteDTO>(clientesActivos);

                    if (clientesActivos.Count == 0)
                    {
                        MessageBox.Show("No se encontraron clientes activos con ese nombre.", "Ôä╣´©Å Sin resultados",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        MensajeEstado = "Sin resultados";
                    }
                    else
                    {
                        MensajeEstado = $"{clientesActivos.Count} cliente(s) encontrado(s)";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar cliente: {ex.Message}\n\nPor favor, intente nuevamente.",
                    "ÔØî Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MensajeEstado = "Error en b├║squeda";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async System.Threading.Tasks.Task CargarTodosLosClientesAsync()
        {
            try
            {
                IsLoading = true;
                MensajeEstado = "Cargando clientes...";

                // Ô£à Crear un NUEVO servicio con contexto fresco para evitar cache
                using var context = new ProyectoSauna.Models.SaunaDbContext();
                var repo = new ProyectoSauna.Repositories.ClienteRepository(context);
                var servicio = new ProyectoSauna.Services.ClienteService(repo);

                // Ô£à Cargar datos FRESCOS desde la BD
                var clientes = await servicio.GetClientesActivosAsync();

                // Crear nueva instancia para forzar actualizaci├│n de UI
                Clientes = new ObservableCollection<ClienteDTO>(
                    clientes.OrderByDescending(c => c.fechaRegistro)
                );

                MensajeEstado = $"{Clientes.Count} cliente(s) activo(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}\n\nVerifique la conexi├│n a la base de datos.",
                    "ÔØî Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MensajeEstado = "Error al cargar datos";
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
            FechaNacimiento = cliente.fechaNacimiento.HasValue
                ? cliente.fechaNacimiento.Value.ToDateTime(TimeOnly.MinValue)
                : null;
            ModoEdicion = true;
        }

        private void LimpiarFormulario()
        {
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
        }

        private void CancelarEdicion()
        {
            LimpiarFormulario();
        }

        private async System.Threading.Tasks.Task DesactivarClienteAsync()
        {
            if (ClienteSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un cliente de la lista para desactivar.",
                    "Ôä╣´©Å Informaci├│n", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Confirmar desactivaci├│n
            var resultado = MessageBox.Show(
                $"┬┐Est├í seguro que desea desactivar al cliente '{ClienteSeleccionado.NombreCompleto}'?\n\n" +
                $"ÔÜá´©Å IMPORTANTE:\n" +
                $"ÔÇó El cliente NO se eliminar├í de la base de datos\n" +
                $"ÔÇó Se mantendr├í todo su historial de compras\n" +
                $"ÔÇó Solo se marcar├í como inactivo\n" +
                $"ÔÇó Puede reactivarlo en cualquier momento\n\n" +
                $"┬┐Desea continuar?",
                "ÔÜá´©Å Confirmar Desactivaci├│n",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (resultado != MessageBoxResult.Yes)
                return;

            if (IsLoading) return;

            try
            {
                IsLoading = true;
                MensajeEstado = "Desactivando cliente...";

                var respuesta = await _clienteService.DesactivarClienteAsync(ClienteSeleccionado.idCliente);

                if (respuesta.exito)
                {
                    // Guardar nombre antes de limpiar
                    var nombreCliente = ClienteSeleccionado.NombreCompleto;

                    // Ô£à Limpiar b├║squeda y recargar lista completa
                    TextoBusqueda = string.Empty;
                    await CargarTodosLosClientesAsync();
                    LimpiarFormulario();
                    MensajeEstado = "Ô£à Cliente desactivado correctamente";

                    // Mensaje breve (opcional)
                    MessageBox.Show(
                        $"Cliente '{nombreCliente}' desactivado correctamente.\nSu historial se mantiene intacto.",
                        "Ô£à Cliente Desactivado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    MessageBox.Show(respuesta.mensaje, "ÔØî Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    MensajeEstado = "ÔØî Error al desactivar cliente";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error inesperado al desactivar el cliente:\n{ex.Message}\n\nPor favor, contacte al administrador del sistema.",
                    "ÔØî Error Cr├¡tico",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                MensajeEstado = "Error cr├¡tico al desactivar";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
