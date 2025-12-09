using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories;
using ProyectoSauna.Services;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace ProyectoSauna.Tests
{
    /// <summary>
    /// Pruebas automatizadas de concurrencia para validar el sistema
    /// </summary>
    public class AutomatedConcurrencyTest
    {
        private readonly ConcurrencyService _concurrencyService;
        private readonly CuentaValidacionService _validacionService;

        public AutomatedConcurrencyTest()
        {
            var context = new SaunaDbContext();
            _concurrencyService = new ConcurrencyService(context);
            _validacionService = new CuentaValidacionService();
        }

        /// <summary>
        /// Ejecuta todas las pruebas de concurrencia automáticamente
        /// </summary>
        public async Task<bool> EjecutarTodasLasPruebas()
        {
            Console.WriteLine("🧪 INICIANDO PRUEBAS AUTOMATIZADAS DE CONCURRENCIA");
            Console.WriteLine("=" * 60);

            bool todosPasaron = true;

            // Prueba 1: Stock concurrente
            Console.WriteLine("📦 Ejecutando Prueba 1: Modificación concurrente de stock...");
            todosPasaron &= await PruebaStockConcurrente();

            await Task.Delay(1000); // Pausa entre pruebas

            // Prueba 2: Totales de cuenta concurrente
            Console.WriteLine("💰 Ejecutando Prueba 2: Cálculo concurrente de totales...");
            todosPasaron &= await PruebaTotalesConcurrentes();

            await Task.Delay(1000);

            // Prueba 3: Validaciones de estado
            Console.WriteLine("🔒 Ejecutando Prueba 3: Validaciones de estado de cuenta...");
            todosPasaron &= await PruebaValidacionesEstado();

            Console.WriteLine("=" * 60);
            Console.WriteLine(todosPasaron ? "✅ TODAS LAS PRUEBAS PASARON" : "❌ ALGUNAS PRUEBAS FALLARON");
            
            return todosPasaron;
        }

        /// <summary>
        /// Simula múltiples usuarios modificando el stock del mismo producto simultáneamente
        /// </summary>
        private async Task<bool> PruebaStockConcurrente()
        {
            try
            {
                // Buscar un producto de prueba
                using var context = new SaunaDbContext();
                var producto = await context.Producto.FirstAsync(p => p.activo && p.stockActual > 10);
                var stockInicial = producto.stockActual;

                Console.WriteLine($"   📊 Producto: {producto.nombre}");
                Console.WriteLine($"   📈 Stock inicial: {stockInicial}");

                // Simular 5 usuarios tratando de reducir stock simultáneamente
                var tareas = new Task<bool>[5];
                for (int i = 0; i < 5; i++)
                {
                    int usuarioId = i + 1;
                    tareas[i] = SimularReduccionStock(producto.idProducto, 1, usuarioId);
                }

                var resultados = await Task.WhenAll(tareas);
                int exitosos = resultados.Count(r => r);
                int fallidos = resultados.Length - exitosos;

                // Verificar stock final
                await context.Entry(producto).ReloadAsync();
                var stockFinal = producto.stockActual;

                Console.WriteLine($"   📊 Operaciones exitosas: {exitosos}");
                Console.WriteLine($"   ⚠️  Operaciones con conflicto: {fallidos}");
                Console.WriteLine($"   📉 Stock final: {stockFinal}");
                Console.WriteLine($"   ✅ Diferencia esperada: {exitosos}, Real: {stockInicial - stockFinal}");

                bool pruebaPasada = (stockInicial - stockFinal) == exitosos;
                Console.WriteLine($"   🎯 Resultado: {(pruebaPasada ? "ÉXITO" : "FALLO")}");

                return pruebaPasada;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error en prueba de stock: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Simula modificación concurrente de totales de cuenta
        /// </summary>
        private async Task<bool> PruebaTotalesConcurrentes()
        {
            try
            {
                // Buscar una cuenta de prueba
                using var context = new SaunaDbContext();
                var cuenta = await context.Cuenta
                    .Where(c => c.idEstadoCuenta == 1) // Pendiente
                    .FirstAsync();

                var totalInicial = cuenta.total;
                Console.WriteLine($"   🏠 Cuenta ID: {cuenta.idCuenta}");
                Console.WriteLine($"   💰 Total inicial: S/ {totalInicial:N2}");

                // Simular 3 operaciones concurrentes de modificación de total
                var tareas = new Task<bool>[3];
                for (int i = 0; i < 3; i++)
                {
                    int operacionId = i + 1;
                    decimal incremento = 10.00m * operacionId;
                    tareas[i] = SimularModificacionTotal(cuenta.idCuenta, incremento, operacionId);
                }

                var resultados = await Task.WhenAll(tareas);
                int exitosos = resultados.Count(r => r);
                int fallidos = resultados.Length - exitosos;

                await context.Entry(cuenta).ReloadAsync();
                var totalFinal = cuenta.total;

                Console.WriteLine($"   📊 Operaciones exitosas: {exitosos}");
                Console.WriteLine($"   ⚠️  Operaciones con conflicto: {fallidos}");
                Console.WriteLine($"   💰 Total final: S/ {totalFinal:N2}");

                // El sistema debe manejar correctamente los conflictos
                bool pruebaPasada = exitosos >= 1 && fallidos >= 1; // Al menos una exitosa y una con conflicto
                Console.WriteLine($"   🎯 Resultado: {(pruebaPasada ? "ÉXITO" : "FALLO")}");

                return pruebaPasada;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error en prueba de totales: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Prueba las validaciones de estado de cuenta
        /// </summary>
        private async Task<bool> PruebaValidacionesEstado()
        {
            try
            {
                // Buscar una cuenta en estado "Pagada" (no debería poder modificarse)
                using var context = new SaunaDbContext();
                var cuentaPagada = await context.Cuenta
                    .Where(c => c.idEstadoCuenta == 2) // Pagada
                    .FirstOrDefaultAsync();

                if (cuentaPagada == null)
                {
                    // Crear una cuenta de prueba en estado "Pagada"
                    var cliente = await context.Cliente.FirstAsync(c => c.activo);
                    cuentaPagada = new Cuenta
                    {
                        idCliente = cliente.idCliente,
                        idEstadoCuenta = 2, // Pagada
                        fechaHoraCreacion = DateTime.Now,
                        subtotalConsumos = 50.00m,
                        total = 50.00m,
                        descuento = 0.00m,
                        idUsuarioCreador = 1
                    };
                    context.Cuenta.Add(cuentaPagada);
                    await context.SaveChangesAsync();
                }

                Console.WriteLine($"   🏠 Probando cuenta ID: {cuentaPagada.idCuenta} (Estado: Pagada)");

                // Intentar modificar cuenta pagada (debe fallar)
                var validacion = await _validacionService.ValidarCuentaParaModificacionAsync(cuentaPagada.idCuenta);
                
                Console.WriteLine($"   🔒 Validación cuenta pagada: {(validacion.esValida ? "PERMITIDA" : "BLOQUEADA")}");
                Console.WriteLine($"   📝 Mensaje: {validacion.mensaje}");

                bool pruebaPasada = !validacion.esValida; // Debe estar bloqueada
                Console.WriteLine($"   🎯 Resultado: {(pruebaPasada ? "ÉXITO" : "FALLO")}");

                return pruebaPasada;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error en prueba de validaciones: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SimularReduccionStock(int idProducto, int cantidad, int usuarioId)
        {
            try
            {
                await Task.Delay(new Random().Next(100, 500)); // Simular latencia variable

                var resultado = await _concurrencyService.SafeSaveChangesAsync(async () =>
                {
                    using var context = new SaunaDbContext();
                    var repo = new ProductoRepository(context);
                    var producto = await repo.GetByIdAsync(idProducto);
                    
                    if (producto.stockActual >= cantidad)
                    {
                        producto.stockActual -= cantidad;
                        await repo.UpdateAsync(producto);
                        return true;
                    }
                    
                    throw new InvalidOperationException("Stock insuficiente");
                });

                Console.WriteLine($"      👤 Usuario {usuarioId}: {(resultado.Success ? "✅ Éxito" : "⚠️ Conflicto")}");
                return resultado.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      👤 Usuario {usuarioId}: ❌ Error - {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SimularModificacionTotal(int idCuenta, decimal incremento, int operacionId)
        {
            try
            {
                await Task.Delay(new Random().Next(100, 300));

                var resultado = await _concurrencyService.SafeSaveChangesAsync(async () =>
                {
                    using var context = new SaunaDbContext();
                    var repo = new CuentaRepository();
                    var cuenta = await repo.GetCuentaByIdAsync(idCuenta);
                    
                    cuenta.total += incremento;
                    await repo.ActualizarCuentaAsync(cuenta);
                    
                    return cuenta.total;
                });

                Console.WriteLine($"      💰 Operación {operacionId} (+S/{incremento:N2}): {(resultado.Success ? "✅ Éxito" : "⚠️ Conflicto")}");
                return resultado.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      💰 Operación {operacionId}: ❌ Error - {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Programa principal para ejecutar las pruebas
    /// </summary>
    public class ProgramaPruebasConcurrencia
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("🧪 SISTEMA DE PRUEBAS DE CONCURRENCIA - SAUNA KALIXTO");
            Console.WriteLine("Presione ENTER para iniciar las pruebas automáticas...");
            Console.ReadLine();

            var pruebas = new AutomatedConcurrencyTest();
            bool resultado = await pruebas.EjecutarTodasLasPruebas();

            Console.WriteLine();
            Console.WriteLine(resultado ? 
                "🎉 SISTEMA DE CONCURRENCIA FUNCIONANDO CORRECTAMENTE" : 
                "⚠️ SE DETECTARON PROBLEMAS EN EL SISTEMA DE CONCURRENCIA");

            Console.WriteLine();
            Console.WriteLine("Presione ENTER para salir...");
            Console.ReadLine();
        }
    }
}