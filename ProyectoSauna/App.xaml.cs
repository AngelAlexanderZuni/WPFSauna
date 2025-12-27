using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProyectoSauna.Data;
using ProyectoSauna.Models;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;
using ProyectoSauna.Repositories;
using ProyectoSauna.Services;
using ProyectoSauna.Services.Interfaces; 
using System.Windows;

namespace ProyectoSauna
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                AppHost = Host.CreateDefaultBuilder()
                    .ConfigureLogging(logging =>
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        // DbContext
                        services.AddDbContext<SaunaDbContext>(options =>
                        {
                            options.UseSqlServer(DatabaseConfig.GetConnectionString());
                            
                            #if DEBUG
                            options.EnableSensitiveDataLogging();
                            options.EnableDetailedErrors();
                            #endif
                        });

                        // Registrar repositorios 
                        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                        services.AddScoped<IClienteRepository, ClienteRepository>();
                        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
                        services.AddScoped<IRolRepository, RolRepository>();
                        services.AddScoped<IProductoRepository, ProductoRepository>();
                        services.AddScoped<CategoriaProductoRepository>();
                        services.AddScoped<ITipoMovimientoRepository, TipoMovimientoRepository>();
                        services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
                        services.AddScoped<IServicioRepository, ServicioRepository>();
                        services.AddScoped<CategoriaServicioRepository>();
                        services.AddScoped<IDetalleServicioRepository, DetalleServicioRepository>();
                        services.AddScoped<ICuentaRepository, CuentaRepository>();                   
                        services.AddScoped<IDetalleConsumoRepository, DetalleConsumoRepository>();
                        services.AddScoped<ITipoDescuentoRepository, TipoDescuentoRepository>();
                        services.AddScoped<IPromocionesRepository, PromocionesRepository>();
                        services.AddScoped<IPagoRepository, PagoRepository>();
                        services.AddScoped<IMetodoPagoRepository, MetodoPagoRepository>();
                        services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
                        services.AddScoped<ITipoComprobanteRepository, TipoComprobanteRepository>();

                        // Egresos Registrations
                        services.AddScoped<IEgresoRepository, EgresoRepository>();
                        services.AddScoped<ITipoEgresoRepository, TipoEgresoRepository>();
                        services.AddScoped<IEgresoService, EgresoService>();
                        services.AddTransient<ProyectoSauna.ViewModels.EgresosViewModel>();
                        
                        // Pagos Registrations
                        services.AddScoped<IPagoService, PagoService>();
                        services.AddScoped<IMetodoPagoService, MetodoPagoService>();
                        services.AddTransient<ProyectoSauna.ViewModels.PagosViewModel>();
                        services.AddTransient<ProyectoSauna.ViewModels.ComprobantesViewModel>(); // Added
                        services.AddScoped<PagoService>();
                        services.AddScoped<MetodoPagoService>();
                        services.AddScoped<IComprobanteService, ComprobanteService>(); // Added

                        // Servicios
                        services.AddTransient<DescuentoService>();
                    })
                    .Build();

                await AppHost.StartAsync();
                await TestDatabaseConnectionAsync();
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error crítico al inicializar la aplicación:\n\n{ex}",
                    "Error de Inicialización",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Environment.Exit(1);
            }
        }

        private async Task TestDatabaseConnectionAsync()
        {
            try
            {
                using var scope = AppHost!.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SaunaDbContext>();

                // Importante: CanConnectAsync puede devolver false sin dar el error real.
                // Abrir conexión fuerza la excepción específica (p.ej. SqlException con Number).
                await context.Database.OpenConnectionAsync();
                await context.Database.CloseConnectionAsync();
                
                #if DEBUG
                var totalClientes = await context.Cliente.CountAsync();
                var totalProductos = await context.Producto.CountAsync();
                System.Diagnostics.Debug.WriteLine($"✅ Conexión exitosa! Clientes: {totalClientes}, Productos: {totalProductos}");
                #endif
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al probar conexión: {ex.Message}", ex);
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {
                if (AppHost != null)
                {
                    await AppHost.StopAsync();
                    AppHost.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error durante cierre: {ex.Message}");
            }
            finally
            {
                base.OnExit(e);
            }
        }
    }
}
