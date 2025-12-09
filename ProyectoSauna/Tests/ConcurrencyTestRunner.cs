// Tests/ConcurrencyTestRunner.cs - Script para probar control de concurrencia de clientes
using ProyectoSauna.Models;
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Repositories;
using ProyectoSauna.Services;
using System.Diagnostics;

namespace ProyectoSauna.Tests
{
    /// <summary>
    /// Runner para probar escenarios de concurrencia en clientes
    /// ATENCIÓN: Este es un test agresivo que simula condiciones extremas
    /// </summary>
    public class ConcurrencyTestRunner
    {
        private readonly SaunaDbContext _context;
        private readonly ClienteService _clienteService;
        private readonly ClienteConcurrencyService _concurrencyService;
        private readonly ClienteAuditService _auditService;

        public ConcurrencyTestRunner()
        {
            _context = new SaunaDbContext();
            var clienteRepository = new ClienteRepository(_context);
            _auditService = new ClienteAuditService(_context);
            _concurrencyService = new ClienteConcurrencyService(clienteRepository, _context);
            
            // Crear servicio con control de concurrencia habilitado
            _clienteService = new ClienteService(clienteRepository, _concurrencyService, _auditService, useConcurrencyControl: true);
        }

        /// <summary>
        /// Test 1: Creación simultánea de clientes con mismo DNI
        /// </summary>
        public async Task TestCreacionSimultaneaDNIDuplicado()
        {
            Console.WriteLine("🧪 TEST 1: Creación simultánea con DNI duplicado");
            Console.WriteLine(new string('=', 60));

            var dniTest = "12345678-TEST";
            var tasks = new List<Task<(bool, string, ClienteDTO?)>>();

            // Simular 5 usuarios creando el mismo cliente al mismo tiempo
            for (int i = 0; i < 5; i++)
            {
                var clienteDto = new ClienteDTO
                {
                    nombre = $"Cliente{i}",
                    apellidos = "Test Concurrencia",
                    numero_documento = dniTest,
                    telefono = $"123456{i}",
                    fechaNacimiento = DateTime.Now.AddYears(-25)
                };

                tasks.Add(_clienteService.CrearClienteAsync(clienteDto));
            }

            var resultados = await Task.WhenAll(tasks);

            // Análisis de resultados
            var exitosos = resultados.Count(r => r.Item1);
            var fallidos = resultados.Count(r => !r.Item1);

            Console.WriteLine($"✅ Creaciones exitosas: {exitosos}");
            Console.WriteLine($"❌ Creaciones fallidas: {fallidos}");
            Console.WriteLine($"🎯 Resultado esperado: 1 exitosa, 4 fallidas");

            if (exitosos == 1 && fallidos == 4)
            {
                Console.WriteLine("🎉 TEST 1 PASADO: Control de concurrencia funcionó correctamente");
            }
            else
            {
                Console.WriteLine("❌ TEST 1 FALLÓ: Control de concurrencia no está funcionando");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Test 2: Actualización simultánea del mismo cliente
        /// </summary>
        public async Task TestActualizacionSimultanea()
        {
            Console.WriteLine("🧪 TEST 2: Actualización simultánea del mismo cliente");
            Console.WriteLine(new string('=', 60));

            // Crear un cliente de prueba primero
            var clienteInicial = new ClienteDTO
            {
                nombre = "Cliente",
                apellidos = "Para Actualizar",
                numero_documento = "UPDATE-TEST-001",
                telefono = "999000111",
                fechaNacimiento = DateTime.Now.AddYears(-30)
            };

            var (exito, _, cliente) = await _clienteService.CrearClienteAsync(clienteInicial);
            if (!exito || cliente == null)
            {
                Console.WriteLine("❌ No se pudo crear cliente para test");
                return;
            }

            Console.WriteLine($"Cliente creado con ID: {cliente.idCliente}");

            // Simular 3 actualizaciones simultáneas
            var tasks = new List<Task<(bool, string)>>();

            for (int i = 0; i < 3; i++)
            {
                var clienteActualizar = new ClienteDTO
                {
                    idCliente = cliente.idCliente,
                    nombre = $"Cliente Actualizado {i}",
                    apellidos = "Test Concurrencia",
                    numero_documento = $"UPDATE-TEST-{i:000}",
                    telefono = $"999000{i:000}",
                    fechaNacimiento = DateTime.Now.AddYears(-30 - i)
                };

                tasks.Add(_clienteService.ActualizarClienteAsync(clienteActualizar));
            }

            var resultados = await Task.WhenAll(tasks);

            // Análisis de resultados
            var exitosos = resultados.Count(r => r.Item1);
            var fallidos = resultados.Count(r => !r.Item1);

            Console.WriteLine($"✅ Actualizaciones exitosas: {exitosos}");
            Console.WriteLine($"❌ Actualizaciones fallidas: {fallidos}");

            Console.WriteLine();
        }

        /// <summary>
        /// Test 3: Carga masiva de clientes
        /// </summary>
        public async Task TestCargaMasiva()
        {
            Console.WriteLine("🧪 TEST 3: Carga masiva de clientes");
            Console.WriteLine(new string('=', 60));

            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task<(bool, string, ClienteDTO?)>>();

            // Crear 20 clientes simultáneamente
            for (int i = 0; i < 20; i++)
            {
                var clienteDto = new ClienteDTO
                {
                    nombre = $"Cliente{i:000}",
                    apellidos = "Test Masivo",
                    numero_documento = $"MASIVO-{i:000}",
                    telefono = $"555{i:0000}",
                    fechaNacimiento = DateTime.Now.AddYears(-20 - (i % 40))
                };

                tasks.Add(_clienteService.CrearClienteAsync(clienteDto));
            }

            var resultados = await Task.WhenAll(tasks);
            stopwatch.Stop();

            var exitosos = resultados.Count(r => r.Item1);
            var fallidos = resultados.Count(r => !r.Item1);

            Console.WriteLine($"✅ Creaciones exitosas: {exitosos}");
            Console.WriteLine($"❌ Creaciones fallidas: {fallidos}");
            Console.WriteLine($"⏱️ Tiempo total: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"📊 Promedio por cliente: {stopwatch.ElapsedMilliseconds / 20.0:F2}ms");

            Console.WriteLine();
        }

        /// <summary>
        /// Test 4: Análisis de estadísticas
        /// </summary>
        public async Task TestEstadisticas()
        {
            Console.WriteLine("🧪 TEST 4: Análisis de estadísticas de concurrencia");
            Console.WriteLine(new string('=', 60));

            // Esperar un poco para que se procesen todas las operaciones
            await Task.Delay(1000);

            var concurrencyStats = _clienteService.GetConcurrencyStats();
            var operationStats = _clienteService.GetOperationStats();
            var issues = _clienteService.DetectConcurrencyIssues();

            if (concurrencyStats != null)
            {
                Console.WriteLine($"🔒 Locks activos de DNI: {concurrencyStats.ActiveDNILocks}");
                Console.WriteLine($"🔒 Locks activos de cliente: {concurrencyStats.ActiveClienteLocks}");
            }

            if (operationStats != null)
            {
                Console.WriteLine($"📊 Operaciones totales (5 min): {operationStats.TotalOperationsLast5Min}");
                Console.WriteLine($"➕ Creaciones: {operationStats.CreateOperations}");
                Console.WriteLine($"✏️ Actualizaciones: {operationStats.UpdateOperations}");
                Console.WriteLine($"❌ Operaciones fallidas: {operationStats.FailedOperations}");
                Console.WriteLine($"⏱️ Duración promedio: {operationStats.AverageDurationMs:F2}ms");
                Console.WriteLine($"⚠️ Problemas detectados: {operationStats.ConcurrentOperationsDetected}");
            }

            if (issues.Any())
            {
                Console.WriteLine("\n⚠️ PROBLEMAS DE CONCURRENCIA DETECTADOS:");
                foreach (var issue in issues)
                {
                    Console.WriteLine($"  - {issue}");
                }
            }
            else
            {
                Console.WriteLine("\n✅ No se detectaron problemas de concurrencia");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Ejecuta todos los tests de concurrencia
        /// </summary>
        public async Task EjecutarTodosLosTests()
        {
            Console.WriteLine("🚀 INICIANDO TESTS DE CONCURRENCIA PARA CLIENTES");
            Console.WriteLine(new string('=', 80));
            Console.WriteLine();

            try
            {
                await TestCreacionSimultaneaDNIDuplicado();
                await TestActualizacionSimultanea();
                await TestCargaMasiva();
                await TestEstadisticas();

                Console.WriteLine("🎉 TODOS LOS TESTS COMPLETADOS");
                Console.WriteLine(new string('=', 80));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR EN TESTS: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Limpia datos de prueba
        /// </summary>
        public async Task LimpiarDatosDePrueba()
        {
            Console.WriteLine("🧹 Limpiando datos de prueba...");
            
            var clientesTest = _context.Cliente
                .Where(c => c.numero_documento.Contains("TEST") || 
                           c.numero_documento.Contains("MASIVO") ||
                           c.numero_documento.Contains("UPDATE"))
                .ToList();

            if (clientesTest.Any())
            {
                _context.Cliente.RemoveRange(clientesTest);
                await _context.SaveChangesAsync();
                Console.WriteLine($"🗑️ {clientesTest.Count} clientes de prueba eliminados");
            }
        }
    }

    /// <summary>
    /// Programa principal para ejecutar los tests
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var testRunner = new ConcurrencyTestRunner();

            // Limpiar datos previos
            await testRunner.LimpiarDatosDePrueba();

            // Ejecutar tests
            await testRunner.EjecutarTodosLosTests();

            // Opcionalmente limpiar después del test
            Console.WriteLine("¿Desea limpiar los datos de prueba? (s/n):");
            var respuesta = Console.ReadLine();
            if (respuesta?.ToLower() == "s")
            {
                await testRunner.LimpiarDatosDePrueba();
            }

            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }
}