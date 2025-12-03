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
using ProyectoSauna.ViewModels;
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

                        // Pagos y Comprobantes
                        services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
                        services.AddScoped<ITipoComprobanteRepository, TipoComprobanteRepository>();
                        services.AddScoped<ICuentaRepository, CuentaRepository>(); // Asegurando que esté registrado si no lo estaba
                        services.AddScoped<IPagoRepository, PagoRepository>();
                        services.AddScoped<IMetodoPagoRepository, MetodoPagoRepository>();

                        // Servicios
                        services.AddScoped<IComprobanteService, ComprobanteService>();
                        services.AddScoped<ICuentaService, CuentaService>();
                        services.AddScoped<IPagoService, PagoService>();
                        services.AddScoped<IMetodoPagoService, MetodoPagoService>();

                        // ViewModels
                        services.AddTransient<ComprobantesViewModel>();

                    })
                    .Build();

                await AppHost.StartAsync();
                await TestDatabaseConnectionAsync();
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error cr├¡tico al inicializar la aplicaci├│n:\n\n{ex.Message}",
                    "Error de Inicializaci├│n",
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
                
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    throw new InvalidOperationException("No se puede conectar a la base de datos");
                }
                
                #if DEBUG
                var totalClientes = await context.Cliente.CountAsync();
                var totalProductos = await context.Producto.CountAsync();
                System.Diagnostics.Debug.WriteLine($"Ô£à Conexi├│n exitosa! Clientes: {totalClientes}, Productos: {totalProductos}");
                #endif
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al probar conexi├│n: {ex.Message}", ex);
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
