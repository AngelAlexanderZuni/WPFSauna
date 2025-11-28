using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using System.Linq;
using System.Windows.Threading;

namespace ProyectoSauna.ViewModels
{
    public class CuentasViewModel : INotifyPropertyChanged
    {
        private DispatcherTimer _timer;

        // Propiedades de búsqueda
        private string _dniBusqueda;
        public string DniBusqueda
        {
            get => _dniBusqueda;
            set { _dniBusqueda = value; OnPropertyChanged(); }
        }

        private bool _clienteEncontrado;
        public bool ClienteEncontrado
        {
            get => _clienteEncontrado;
            set { _clienteEncontrado = value; OnPropertyChanged(); }
        }

        private string _nombreClienteBuscado;
        public string NombreClienteBuscado
        {
            get => _nombreClienteBuscado;
            set { _nombreClienteBuscado = value; OnPropertyChanged(); }
        }

        private bool _estaCargando;
        public bool EstaCargando
        {
            get => _estaCargando;
            set { _estaCargando = value; OnPropertyChanged(); }
        }

        // Cuenta seleccionada
        private CuentaPendiente _cuentaSeleccionada;
        public CuentaPendiente CuentaSeleccionada
        {
            get => _cuentaSeleccionada;
            set
            {
                _cuentaSeleccionada = value;
                OnPropertyChanged();
                CargarConsumosActuales();
                ActualizarTotales();
            }
        }

        // Producto seleccionado
        private Producto _productoSeleccionado;
        public Producto ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set { _productoSeleccionado = value; OnPropertyChanged(); }
        }

        private int _cantidadProducto = 1;
        public int CantidadProducto
        {
            get => _cantidadProducto;
            set
            {
                if (value > 0) _cantidadProducto = value;
                OnPropertyChanged();
            }
        }

        // Totales
        private decimal _totalConsumos;
        public decimal TotalConsumos
        {
            get => _totalConsumos;
            set { _totalConsumos = value; OnPropertyChanged(); }
        }

        private decimal _totalGeneral;
        public decimal TotalGeneral
        {
            get => _totalGeneral;
            set { _totalGeneral = value; OnPropertyChanged(); }
        }

        // Propiedades compatibles con pruebas unitarias
        private decimal _totalProductos;
        public decimal TotalProductos
        {
            get => _totalProductos;
            set { _totalProductos = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalCuenta)); }
        }

        private decimal _totalServicios;
        public decimal TotalServicios
        {
            get => _totalServicios;
            set { _totalServicios = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalCuenta)); }
        }

        public decimal TotalCuenta
        {
            get
            {
                var entrada = CuentaSeleccionada?.precioEntrada ?? 0m;
                var descuento = CuentaSeleccionada?.descuento ?? 0m;
                return entrada - descuento + TotalProductos + TotalServicios;
            }
        }

        // Colecciones
        public ObservableCollection<CuentaPendiente> CuentasPendientes { get; set; }
        public ObservableCollection<Producto> ProductosDisponibles { get; set; }
        public ObservableCollection<ConsumoDetalle> ConsumosActuales { get; set; }

        // Comandos
        public ICommand BuscarClienteCommand { get; }
        public ICommand CrearCuentaCommand { get; }
        public ICommand LimpiarBusquedaCommand { get; }
        public ICommand ActualizarListaCommand { get; }
        public ICommand AgregarConsumoCommand { get; }
        public ICommand EliminarConsumoCommand { get; }

        // Constructor
        public CuentasViewModel()
        {
            CuentasPendientes = new ObservableCollection<CuentaPendiente>();
            ProductosDisponibles = new ObservableCollection<Producto>();
            ConsumosActuales = new ObservableCollection<ConsumoDetalle>();

            // Inicializar comandos
            BuscarClienteCommand = new RelayCommand(BuscarCliente, () => !string.IsNullOrWhiteSpace(DniBusqueda) && DniBusqueda.Length == 8);
            CrearCuentaCommand = new RelayCommand(CrearCuenta, () => ClienteEncontrado);
            LimpiarBusquedaCommand = new RelayCommand(LimpiarBusqueda);
            ActualizarListaCommand = new RelayCommand(ActualizarLista);
            AgregarConsumoCommand = new RelayCommand(AgregarConsumo, () => CuentaSeleccionada != null && ProductoSeleccionado != null);
            EliminarConsumoCommand = new RelayCommand<ConsumoDetalle>(EliminarConsumo);

            // Cargar datos iniciales
            CargarProductosDisponibles();
            CargarCuentasPendientes();

            // Timer para actualizar tiempo transcurrido
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _timer.Tick += (s, e) => ActualizarTiempos();
            _timer.Start();
        }

        private void BuscarCliente()
        {
            EstaCargando = true;

            try
            {
                // AQUÍ DEBES CONECTAR CON TU BASE DE DATOS
                // Ejemplo simulado:
                System.Threading.Thread.Sleep(500);

                if (DniBusqueda?.Length == 8)
                {
                    // Simulación - Reemplaza con tu lógica de base de datos
                    ClienteEncontrado = true;
                    NombreClienteBuscado = $"Juan Pérez Gómez (DNI: {DniBusqueda})";

                    MessageBox.Show("✓ Cliente encontrado", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private void CrearCuenta()
        {
            try
            {
                // AQUÍ INSERTAS EN LA BASE DE DATOS
                var nuevaCuenta = new CuentaPendiente
                {
                    idCuenta = CuentasPendientes.Count + 1,
                    NombreCliente = NombreClienteBuscado.Split('(')[0].Trim(),
                    DocumentoCliente = DniBusqueda,
                    HoraIngreso = DateTime.Now.ToString("HH:mm"),
                    FechaHoraIngreso = DateTime.Now,
                    precioEntrada = 20.00m,
                    descuento = 0,
                    total = 20.00m,
                    saldo = 20.00m,
                    EstadoCuenta = "PENDIENTE"
                };

                CuentasPendientes.Add(nuevaCuenta);

                MessageBox.Show("✓ Cuenta creada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LimpiarBusqueda();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear cuenta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimpiarBusqueda()
        {
            DniBusqueda = string.Empty;
            ClienteEncontrado = false;
            NombreClienteBuscado = string.Empty;
        }

        private void ActualizarLista()
        {
            // AQUÍ RECARGAS DESDE LA BASE DE DATOS
            CargarCuentasPendientes();
            MessageBox.Show("✓ Lista actualizada", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CargarCuentasPendientes()
        {
            // AQUÍ CARGAS DESDE TU BASE DE DATOS
            // Datos de ejemplo:
            CuentasPendientes.Clear();

            CuentasPendientes.Add(new CuentaPendiente
            {
                idCuenta = 1,
                NombreCliente = "María García López",
                DocumentoCliente = "12345678",
                HoraIngreso = "10:30",
                FechaHoraIngreso = DateTime.Now.AddHours(-2),
                precioEntrada = 20.00m,
                descuento = 0,
                total = 35.50m,
                saldo = 35.50m,
                EstadoCuenta = "PENDIENTE"
            });

            CuentasPendientes.Add(new CuentaPendiente
            {
                idCuenta = 2,
                NombreCliente = "Carlos Rodríguez",
                DocumentoCliente = "87654321",
                HoraIngreso = "11:15",
                FechaHoraIngreso = DateTime.Now.AddHours(-1),
                precioEntrada = 20.00m,
                descuento = 0,
                total = 20.00m,
                saldo = 20.00m,
                EstadoCuenta = "PENDIENTE"
            });

            ActualizarTiempos();
        }

        private void CargarProductosDisponibles()
        {
            // AQUÍ CARGAS DESDE TU BASE DE DATOS
            ProductosDisponibles.Clear();

            ProductosDisponibles.Add(new Producto { idProducto = 1, nombre = "Coca Cola 500ml", precio = 3.50m, stock = 45 });
            ProductosDisponibles.Add(new Producto { idProducto = 2, nombre = "Cerveza Pilsen", precio = 7.00m, stock = 28 });
            ProductosDisponibles.Add(new Producto { idProducto = 3, nombre = "Agua Mineral", precio = 2.00m, stock = 62 });
            ProductosDisponibles.Add(new Producto { idProducto = 4, nombre = "Gaseosa Inca Kola", precio = 3.50m, stock = 35 });
            ProductosDisponibles.Add(new Producto { idProducto = 5, nombre = "Piqueo Mixto", precio = 12.00m, stock = 15 });
        }

        private void CargarConsumosActuales()
        {
            ConsumosActuales.Clear();

            if (CuentaSeleccionada != null)
            {
                // AQUÍ CARGAS LOS CONSUMOS DE LA BASE DE DATOS
                // Ejemplo simulado:
                ConsumosActuales.Add(new ConsumoDetalle
                {
                    idDetalle = 1,
                    NombreProducto = "Coca Cola 500ml",
                    cantidad = 2,
                    precioUnitario = 3.50m,
                    subtotal = 7.00m
                });

                ConsumosActuales.Add(new ConsumoDetalle
                {
                    idDetalle = 2,
                    NombreProducto = "Piqueo Mixto",
                    cantidad = 1,
                    precioUnitario = 12.00m,
                    subtotal = 12.00m
                });

                ActualizarTotales();
            }
        }

        private void AgregarConsumo()
        {
            if (CuentaSeleccionada == null || ProductoSeleccionado == null) return;

            try
            {
                var nuevoConsumo = new ConsumoDetalle
                {
                    idDetalle = ConsumosActuales.Count + 1,
                    NombreProducto = ProductoSeleccionado.nombre,
                    cantidad = CantidadProducto,
                    precioUnitario = ProductoSeleccionado.precio,
                    subtotal = CantidadProducto * ProductoSeleccionado.precio
                };

                // AQUÍ INSERTAS EN LA BASE DE DATOS
                ConsumosActuales.Add(nuevoConsumo);

                // Actualizar stock
                ProductoSeleccionado.stock -= CantidadProducto;

                ActualizarTotales();

                CantidadProducto = 1;
                MessageBox.Show("✓ Producto agregado", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EliminarConsumo(ConsumoDetalle consumo)
        {
            if (consumo == null) return;

            var result = MessageBox.Show(
                $"¿Eliminar {consumo.NombreProducto}?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // AQUÍ ELIMINAS DE LA BASE DE DATOS
                ConsumosActuales.Remove(consumo);
                ActualizarTotales();
            }
        }

        private void ActualizarTotales()
        {
            if (CuentaSeleccionada == null)
            {
                TotalConsumos = 0;
                TotalGeneral = 0;
                return;
            }

            TotalConsumos = ConsumosActuales.Sum(c => c.subtotal);
            TotalGeneral = CuentaSeleccionada.precioEntrada - CuentaSeleccionada.descuento + TotalConsumos;

            // Actualizar en la cuenta seleccionada
            CuentaSeleccionada.total = TotalGeneral;
            CuentaSeleccionada.saldo = TotalGeneral;
        }

        private void ActualizarTiempos()
        {
            foreach (var cuenta in CuentasPendientes)
            {
                cuenta.ActualizarTiempo();
            }
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // CLASES DE MODELO
    public class CuentaPendiente : INotifyPropertyChanged
    {
        public int idCuenta { get; set; }
        public string NombreCliente { get; set; }
        public string DocumentoCliente { get; set; }
        public string HoraIngreso { get; set; }
        public DateTime FechaHoraIngreso { get; set; }
        public decimal precioEntrada { get; set; }
        public decimal descuento { get; set; }
        public decimal total { get; set; }
        public decimal saldo { get; set; }
        public string EstadoCuenta { get; set; }

        private string _tiempoTranscurrido;
        public string TiempoTranscurrido
        {
            get => _tiempoTranscurrido;
            set { _tiempoTranscurrido = value; OnPropertyChanged(); }
        }

        public void ActualizarTiempo()
        {
            var tiempo = DateTime.Now - FechaHoraIngreso;
            TiempoTranscurrido = $"{(int)tiempo.TotalHours:D2}:{tiempo.Minutes:D2}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Producto : INotifyPropertyChanged
    {
        public int idProducto { get; set; }
        public string nombre { get; set; }

        private decimal _precio;
        public decimal precio
        {
            get => _precio;
            set { _precio = value; OnPropertyChanged(); }
        }

        private int _stock;
        public int stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ConsumoDetalle
    {
        public int idDetalle { get; set; }
        public string NombreProducto { get; set; }
        public int cantidad { get; set; }
        public decimal precioUnitario { get; set; }
        public decimal subtotal { get; set; }
    }

    // COMANDO REUTILIZABLE
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object parameter) => _execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;
        public void Execute(object parameter) => _execute((T)parameter);
    }
}
