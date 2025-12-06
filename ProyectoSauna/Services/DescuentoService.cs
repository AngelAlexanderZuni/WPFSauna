// Services/DescuentoService.cs - CORREGIDO
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Services
{
    public class DescuentoService
    {
        private readonly IPromocionesRepository _promocionesRepo;
        private readonly IClienteRepository _clienteRepo;

        public DescuentoService(IPromocionesRepository promocionesRepo, IClienteRepository clienteRepo)
        {
            _promocionesRepo = promocionesRepo;
            _clienteRepo = clienteRepo;
        }

        /// <summary>
        /// Calcula todos los descuentos disponibles para un cliente en una cuenta específica
        /// </summary>
        public async Task<ResultadoDescuento> CalcularDescuentosDisponiblesAsync(int idCliente, decimal montoBase)
        {
            if (montoBase <= 0)
            {
                return new ResultadoDescuento
                {
                    MontoBase = montoBase,
                    TotalDescuento = 0,
                    MontoFinal = montoBase,
                    DescuentosAplicados = new List<DetalleDescuento>()
                };
            }

            var promocionesActivas = (await _promocionesRepo.ObtenerActivasAsync()).ToList();
            var descuentosAplicables = new List<DetalleDescuento>();

            // 🎯 EVALUAR TODAS LAS PROMOCIONES DISPONIBLES
            foreach (var promocion in promocionesActivas)
            {
                var descuento = await EvaluarPromocionAsync(promocion, idCliente, montoBase);
                if (descuento != null && descuento.MontoDescuento > 0)
                {
                    descuentosAplicables.Add(descuento);
                    // 🐛 DEBUG: Mostrar qué promociones están aplicándose
                    System.Diagnostics.Debug.WriteLine($"🎁 Promoción aplicada: {descuento.NombrePromocion} - Tipo: {descuento.TipoDescuento} - Monto: S/. {descuento.MontoDescuento:N2}");
                }
            }

            // ✅ APLICAR SOLO UN DESCUENTO (el de mayor monto si hay varios)
            DetalleDescuento? mejorDescuento = null;
            if (descuentosAplicables.Any())
            {
                // Como las promociones por visitas ahora son exactas, 
                // solo puede haber máximo una promoción por visitas aplicable
                mejorDescuento = descuentosAplicables.OrderByDescending(d => d.MontoDescuento).First();
                System.Diagnostics.Debug.WriteLine($"🏆 Descuento final seleccionado: {mejorDescuento.NombrePromocion} - S/. {mejorDescuento.MontoDescuento:N2}");
            }

            var totalDescuento = mejorDescuento?.MontoDescuento ?? 0;
            var descuentosFinales = mejorDescuento != null ? new List<DetalleDescuento> { mejorDescuento } : new List<DetalleDescuento>();
            var montoFinal = Math.Max(0, montoBase - totalDescuento);

            return new ResultadoDescuento
            {
                MontoBase = montoBase,
                TotalDescuento = totalDescuento,
                MontoFinal = montoFinal,
                DescuentosAplicados = descuentosFinales
            };
        }

        /// <summary>
        /// Evalúa si una promoción específica aplica para un cliente
        /// </summary>
        private async Task<DetalleDescuento?> EvaluarPromocionAsync(Promociones promocion, int idCliente, decimal montoBase)
        {
            // 🛡️ VALIDACIONES DE PROMOCIÓN
            if (promocion == null || !promocion.activo)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Promoción inválida o inactiva: {promocion?.nombreDescuento ?? "null"}");
                return null;
            }

            if (promocion.idTipoDescuentoNavigation == null)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Promoción sin tipo de descuento: {promocion.nombreDescuento}");
                return null;
            }

            if (promocion.montoDescuento <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Promoción con monto inválido: {promocion.nombreDescuento} - Monto: {promocion.montoDescuento}");
                return null;
            }

            var tipoDescuento = promocion.idTipoDescuentoNavigation.nombre.ToLower().Trim();
            System.Diagnostics.Debug.WriteLine($"✅ Evaluando promoción válida: {promocion.nombreDescuento} - Tipo: {tipoDescuento}");

            return tipoDescuento switch
            {
                "porcentaje" => EvaluarDescuentoPorcentaje(promocion, montoBase),
                "monto fijo" => EvaluarDescuentoMontoFijo(promocion, montoBase),
                "cumpleaños" or "cumpleanos" => await EvaluarDescuentoCumpleanosAsync(promocion, idCliente, montoBase),
                "visitas" or "fidelidad" => await EvaluarDescuentoVisitasAsync(promocion, idCliente, montoBase),
                _ => null
            };
        }

        /// <summary>
        /// Evalúa descuento por porcentaje
        /// </summary>
        private DetalleDescuento EvaluarDescuentoPorcentaje(Promociones promocion, decimal montoBase)
        {
            var porcentaje = Math.Min(100, Math.Max(0, promocion.montoDescuento));
            var descuento = montoBase * (porcentaje / 100);

            return new DetalleDescuento
            {
                IdPromocion = promocion.idPromocion,
                NombrePromocion = promocion.nombreDescuento,
                TipoDescuento = "Porcentaje",
                MontoDescuento = Math.Round(descuento, 2),
                Descripcion = $"{porcentaje:N2}% de descuento",
                Motivo = promocion.motivo
            };
        }

        /// <summary>
        /// Evalúa descuento por monto fijo
        /// </summary>
        private DetalleDescuento EvaluarDescuentoMontoFijo(Promociones promocion, decimal montoBase)
        {
            var descuento = Math.Min(promocion.montoDescuento, montoBase);

            return new DetalleDescuento
            {
                IdPromocion = promocion.idPromocion,
                NombrePromocion = promocion.nombreDescuento,
                TipoDescuento = "Monto Fijo",
                MontoDescuento = Math.Round(descuento, 2),
                Descripcion = $"S/ {promocion.montoDescuento:N2} de descuento",
                Motivo = promocion.motivo
            };
        }

        /// <summary>
        /// Evalúa descuento por cumpleaños del cliente
        /// </summary>
        private async Task<DetalleDescuento?> EvaluarDescuentoCumpleanosAsync(Promociones promocion, int idCliente, decimal montoBase)
        {
            var cliente = await _clienteRepo.GetByIdAsync(idCliente);
            if (cliente?.fechaNacimiento == null)
                return null;

            var hoy = DateTime.Today;
            var fechaNac = cliente.fechaNacimiento.Value;
            var cumpleanos = new DateTime(hoy.Year, fechaNac.Month, fechaNac.Day);

            var diasDiferencia = Math.Abs((cumpleanos - hoy).Days);

            if (diasDiferencia <= promocion.valorCondicion)
            {
                var descuento = Math.Min(promocion.montoDescuento, montoBase);

                return new DetalleDescuento
                {
                    IdPromocion = promocion.idPromocion,
                    NombrePromocion = promocion.nombreDescuento,
                    TipoDescuento = "Cumpleaños",
                    MontoDescuento = Math.Round(descuento, 2),
                    Descripcion = $"¡Feliz cumpleaños! Descuento de S/ {promocion.montoDescuento:N2}",
                    Motivo = promocion.motivo
                };
            }

            return null;
        }

        /// <summary>
        /// Evalúa descuento por número de visitas del cliente (COINCIDENCIA EXACTA)
        /// </summary>
        private async Task<DetalleDescuento?> EvaluarDescuentoVisitasAsync(Promociones promocion, int idCliente, decimal montoBase)
        {
            var cliente = await _clienteRepo.GetByIdAsync(idCliente);
            if (cliente == null)
                return null;

            var visitasTotales = cliente.visitasTotales;

            // 🎯 COINCIDENCIA EXACTA: Las visitas del cliente deben ser IGUALES a la condición de la promoción
            if (visitasTotales == promocion.valorCondicion)
            {
                // 💰 DESCUENTO FIJO POR VISITAS EXACTAS
                var descuentoFijo = promocion.montoDescuento;
                
                // 🐛 DEBUG: Verificar coincidencia exacta
                System.Diagnostics.Debug.WriteLine($"✅ COINCIDENCIA EXACTA: {visitasTotales} visitas == {promocion.valorCondicion} requeridas = S/. {descuentoFijo:N2}");

                return new DetalleDescuento
                {
                    IdPromocion = promocion.idPromocion,
                    NombrePromocion = promocion.nombreDescuento,
                    TipoDescuento = "Visitas",
                    MontoDescuento = Math.Round(descuentoFijo, 2),
                    Descripcion = $"Exactamente {visitasTotales} visitas: S/ {promocion.montoDescuento:N2} de descuento",
                    Motivo = promocion.motivo
                };
            }

            // 🐛 DEBUG: No coincide exactamente
            System.Diagnostics.Debug.WriteLine($"❌ NO COINCIDE: {visitasTotales} visitas != {promocion.valorCondicion} requeridas");
            return null;
        }

        /// <summary>
        /// Obtiene todas las promociones activas
        /// </summary>
        public async Task<List<Promociones>> ObtenerPromocionesActivasAsync()
        {
            return (await _promocionesRepo.ObtenerActivasAsync()).ToList();
        }

        /// <summary>
        /// Obtiene información resumida de descuentos disponibles para un cliente
        /// </summary>
        public async Task<InfoDescuentosCliente> ObtenerInfoDescuentosClienteAsync(int idCliente)
        {
            // 🔄 OBTENER DATOS FRESCOS DEL CLIENTE DESDE BD
            var cliente = await _clienteRepo.GetByIdAsync(idCliente);
            if (cliente == null)
            {
                return new InfoDescuentosCliente
                {
                    TieneDescuentos = false,
                    Mensaje = "Cliente no encontrado"
                };
            }

            // 🐛 DEBUG: Verificar datos del cliente obtenidos
            System.Diagnostics.Debug.WriteLine($"🔍 Cliente obtenido desde BD - ID: {cliente.idCliente}, Visitas: {cliente.visitasTotales}");

            var promocionesActivas = (await _promocionesRepo.ObtenerActivasAsync()).ToList();
            var descuentosDisponibles = new List<string>();

            // 🐛 DEBUG: Verificar promociones activas
            System.Diagnostics.Debug.WriteLine($"🎁 Promociones activas encontradas: {promocionesActivas.Count}");

            foreach (var promo in promocionesActivas)
            {
                var tipoDescuento = promo.idTipoDescuentoNavigation?.nombre.ToLower().Trim() ?? "";
                
                // 🐛 DEBUG: Evaluar cada promoción
                System.Diagnostics.Debug.WriteLine($"🔍 Evaluando promoción: {promo.nombreDescuento} - Tipo: {tipoDescuento} - Condición: {promo.valorCondicion}");

                switch (tipoDescuento)
                {
                    case "cumpleaños" or "cumpleanos":
                        if (cliente.fechaNacimiento != null)
                        {
                            var hoy = DateTime.Today;
                            var fechaNac = cliente.fechaNacimiento.Value;
                            var cumpleanos = new DateTime(hoy.Year, fechaNac.Month, fechaNac.Day);
                            var diasDiferencia = Math.Abs((cumpleanos - hoy).Days);

                            if (diasDiferencia <= promo.valorCondicion)
                            {
                                descuentosDisponibles.Add($"🎂 {promo.nombreDescuento}: S/ {promo.montoDescuento:N2}");
                                System.Diagnostics.Debug.WriteLine($"✅ Promoción cumpleaños aplicada: {promo.nombreDescuento}");
                            }
                        }
                        break;

                    case "visitas" or "fidelidad":
                        System.Diagnostics.Debug.WriteLine($"🔍 Comparando visitas: {cliente.visitasTotales} == {promo.valorCondicion}?");
                        if (cliente.visitasTotales == promo.valorCondicion) // 🎯 COINCIDENCIA EXACTA
                        {
                            descuentosDisponibles.Add($"⭐ {promo.nombreDescuento}: S/ {promo.montoDescuento:N2}");
                            System.Diagnostics.Debug.WriteLine($"✅ Promoción por visitas aplicada: {promo.nombreDescuento}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ No aplica promoción por visitas: {cliente.visitasTotales} != {promo.valorCondicion}");
                        }
                        break;

                    case "porcentaje":
                        descuentosDisponibles.Add($"💰 {promo.nombreDescuento}: {promo.montoDescuento:N2}%");
                        break;

                    case "monto fijo":
                        descuentosDisponibles.Add($"💵 {promo.nombreDescuento}: S/ {promo.montoDescuento:N2}");
                        break;
                }
            }

            return new InfoDescuentosCliente
            {
                IdCliente = idCliente,
                NombreCliente = $"{cliente.nombre} {cliente.apellidos}".Trim(),
                VisitasTotales = cliente.visitasTotales,
                TieneDescuentos = descuentosDisponibles.Any(),
                DescuentosDisponibles = descuentosDisponibles,
                Mensaje = descuentosDisponibles.Any()
                    ? $"Cliente con {descuentosDisponibles.Count} descuento(s) disponible(s)"
                    : "Sin descuentos disponibles"
            };
        }

        /// <summary>
        /// Verifica si un cliente tiene descuentos disponibles sin calcular montos
        /// </summary>
        public async Task<bool> TieneDescuentosDisponiblesAsync(int idCliente)
        {
            var info = await ObtenerInfoDescuentosClienteAsync(idCliente);
            return info.TieneDescuentos;
        }
    }

    public class ResultadoDescuento
    {
        public decimal MontoBase { get; set; }
        public decimal TotalDescuento { get; set; }
        public decimal MontoFinal { get; set; }
        public List<DetalleDescuento> DescuentosAplicados { get; set; } = new();

        public bool TieneDescuentos => DescuentosAplicados.Any();
        public int CantidadDescuentos => DescuentosAplicados.Count;
        public decimal PorcentajeDescuento => MontoBase > 0 ? (TotalDescuento / MontoBase) * 100 : 0;

        public string ObtenerResumen()
        {
            if (!TieneDescuentos)
                return "Sin descuentos aplicados";

            var lineas = new List<string>
            {
                $"Monto Base: S/ {MontoBase:N2}",
                "Descuentos aplicados:"
            };

            foreach (var descuento in DescuentosAplicados)
            {
                lineas.Add($"  • {descuento.NombrePromocion}: -S/ {descuento.MontoDescuento:N2}");
            }

            lineas.Add($"Total Descuento: -S/ {TotalDescuento:N2}");
            lineas.Add($"Monto Final: S/ {MontoFinal:N2}");

            return string.Join(Environment.NewLine, lineas);
        }
    }

    public class DetalleDescuento
    {
        public int IdPromocion { get; set; }
        public string NombrePromocion { get; set; } = string.Empty;
        public string TipoDescuento { get; set; } = string.Empty;
        public decimal MontoDescuento { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
    }

    public class InfoDescuentosCliente
    {
        public int IdCliente { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public int VisitasTotales { get; set; }
        public bool TieneDescuentos { get; set; }
        public List<string> DescuentosDisponibles { get; set; } = new();
        public string Mensaje { get; set; } = string.Empty;
    }
}