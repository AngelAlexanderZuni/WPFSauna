// ViewModels/ClientesViewModel.cs - COMPLETAMENTE CORREGIDO
using ProyectoSauna.Commands;
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProyectoSauna.ViewModels
{
    public class ClientesViewModel : BaseViewModel
    {
        private readonly IClienteService _clienteService;

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

        public ICommand GuardarClienteCommand { get; }
        public ICommand BuscarClienteCommand { get; }
        public ICommand LimpiarFormularioCommand { get; }
        public ICommand CancelarEdicionCommand { get; }
        public ICommand MostrarTodosCommand { get; }
        public ICommand DesactivarClienteCommand { get; }

        public ClientesViewModel(IClienteService clienteService)
        {
            _clienteService = clienteService;

            GuardarClienteCommand = new AsyncRelayCommand(_ => GuardarClienteAsync());
            BuscarClienteCommand = new AsyncRelayCommand(_ => BuscarClienteAsync());
            MostrarTodosCommand = new AsyncRelayCommand(_ => CargarTodosLosClientesAsync());
            DesactivarClienteCommand = new AsyncRelayCommand(_ => DesactivarClienteAsync());

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

                using var context = new ProyectoSauna.Models.SaunaDbContext();
                var repo = new ProyectoSauna.Repositories.ClienteRepository(context);
                var servicio = new ProyectoSauna.Services.ClienteService(repo);

                if (TipoBusqueda == "DNI")
                {
                    var cliente = await servicio.GetClienteByDNIAsync(TextoBusqueda.Trim());
                    if (cliente != null && cliente.activo)
                    {
                        Clientes = new ObservableCollection<ClienteDTO> { cliente };
                        MensajeEstado = "1 cliente encontrado";
                    }
                    else
                    {
                        Clientes = new ObservableCollection<ClienteDTO>();
                        MessageBox.Show("No se encontró ningún cliente activo con ese DNI.", "ℹ️ Sin resultados",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        MensajeEstado = "Sin resultados";
                    }
                }
                else
                {
                    var clientesEncontrados = await servicio.BuscarClientesPorNombreAsync(TextoBusqueda.Trim());
                    var clientesActivos = clientesEncontrados.Where(c => c.activo).ToList();
                    Clientes = new ObservableCollection<ClienteDTO>(clientesActivos);

                    if (clientesActivos.Count == 0)
                    {
                        MessageBox.Show("No se encontraron clientes activos con ese nombre.", "ℹ️ Sin resultados",
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
                    "❌ Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MensajeEstado = "Error en búsqueda";
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

                using var context = new ProyectoSauna.Models.SaunaDbContext();
                var repo = new ProyectoSauna.Repositories.ClienteRepository(context);
                var servicio = new ProyectoSauna.Services.ClienteService(repo);

                var clientes = await servicio.GetClientesActivosAsync();
                Clientes = new ObservableCollection<ClienteDTO>(clientes.OrderByDescending(c => c.fechaRegistro));
                MensajeEstado = $"{Clientes.Count} cliente(s) activo(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}\n\nVerifique la conexión a la base de datos.",
                    "❌ Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            FechaNacimiento = cliente.fechaNacimiento;
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

        private async System.Threading.Tasks.Task DesactivarClienteAsync()
        {
            if (ClienteSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un cliente de la lista para desactivar.",
                    "ℹ️ Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Está seguro que desea desactivar al cliente '{ClienteSeleccionado.NombreCompleto}'?\n\n" +
                $"⚠️ IMPORTANTE:\n" +
                $"• El cliente NO se eliminará de la base de datos\n" +
                $"• Se mantendrá todo su historial de compras\n" +
                $"• Solo se marcará como inactivo\n" +
                $"• Puede reactivarlo en cualquier momento\n\n" +
                $"¿Desea continuar?",
                "⚠️ Confirmar Desactivación",
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
                    var nombreCliente = ClienteSeleccionado.NombreCompleto;
                    TextoBusqueda = string.Empty;
                    await CargarTodosLosClientesAsync();
                    LimpiarFormulario();
                    MensajeEstado = "✅ Cliente desactivado correctamente";
                    MessageBox.Show($"Cliente '{nombreCliente}' desactivado correctamente.\nSu historial se mantiene intacto.",
                        "✅ Cliente Desactivado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(respuesta.mensaje, "❌ Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    MensajeEstado = "❌ Error al desactivar cliente";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al desactivar el cliente:\n{ex.Message}\n\nPor favor, contacte al administrador del sistema.",
                    "❌ Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                MensajeEstado = "Error crítico al desactivar";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}