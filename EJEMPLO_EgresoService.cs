// EJEMPLO DE IMPLEMENTACIÓN - EgresoService.cs
// Este archivo muestra cómo debería implementarse el servicio si no existe

using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Interfaces;
using ProyectoSauna.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Services
{
    /// <summary>
    /// EJEMPLO DE IMPLEMENTACIÓN DEL SERVICIO DE EGRESOS
    /// Usar como referencia si el servicio no existe o necesita correcciones
    /// </summary>
    public class EgresoServiceEjemplo : IEgresoService
    {
        private readonly IEgresoRepository _egresoRepository;
        private readonly ITipoEgresoRepository _tipoEgresoRepository;

        public EgresoServiceEjemplo(IEgresoRepository egresoRepository, ITipoEgresoRepository tipoEgresoRepository)
        {
            _egresoRepository = egresoRepository;
            _tipoEgresoRepository = tipoEgresoRepository;
        }

        public async Task<IEnumerable<TipoEgresoDTO>> GetTiposEgresoAsync(string? filtro = null)
        {
            try
            {
                IEnumerable<TipoEgreso> tipos;

                if (string.IsNullOrWhiteSpace(filtro))
                {
                    tipos = await _tipoEgresoRepository.GetAllAsync();
                }
                else
                {
                    tipos = await _tipoEgresoRepository.GetAllAsync();
                    tipos = tipos.Where(t => t.nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase));
                }

                return tipos.Select(t => new TipoEgresoDTO
                {
                    idTipoEgreso = t.idTipoEgreso,
                    nombre = t.nombre
                }).OrderBy(t => t.nombre);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener tipos de egreso: {ex.Message}", ex);
            }
        }

        public async Task<(bool exito, string mensaje, CabEgresoDTO? egreso)> CrearEgresoAsync(CabEgresoDTO cabeceraDto)
        {
            try
            {
                // Validaciones
                if (cabeceraDto == null)
                    return (false, "Los datos del egreso son requeridos", null);

                if (!cabeceraDto.Detalles.Any())
                    return (false, "El egreso debe tener al menos un detalle", null);

                if (cabeceraDto.Detalles.Any(d => d.monto <= 0))
                    return (false, "Todos los detalles deben tener un monto mayor a 0", null);

                // Crear entidad de cabecera
                var cabEgreso = new CabEgreso
                {
                    fecha = cabeceraDto.fecha,
                    montoTotal = cabeceraDto.Detalles.Sum(d => d.monto),
                    idUsuario = cabeceraDto.idUsuario ?? GetUsuarioActualId() // Implementar según tu sistema
                };

                // Crear detalles
                var detalles = cabeceraDto.Detalles.Select(d => new DetEgreso
                {
                    concepto = d.concepto.Trim(),
                    monto = d.monto,
                    recurrente = d.recurrente,
                    comprobanteRuta = d.comprobanteRuta,
                    idTipoEgreso = d.idTipoEgreso
                }).ToList();

                // Asignar detalles a la cabecera
                cabEgreso.DetEgreso = detalles;

                // Guardar en repositorio
                var egresoCreado = await _egresoRepository.AddAsync(cabEgreso);

                // Convertir a DTO para respuesta
                var egresoDto = await ConvertirAEgresoDTO(egresoCreado);

                return (true, "Egreso creado exitosamente", egresoDto);
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear el egreso: {ex.Message}", null);
            }
        }

        public async Task<IEnumerable<CabEgresoDTO>> GetEgresosRecientesAsync(int count = 20)
        {
            try
            {
                var egresos = await _egresoRepository.GetAllAsync();
                
                var egresosRecientes = egresos
                    .OrderByDescending(e => e.fecha)
                    .Take(count)
                    .Select(async e => await ConvertirAEgresoDTO(e));

                return await Task.WhenAll(egresosRecientes);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener egresos recientes: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<DetEgresoDTO>> GetDetallesPorCabeceraAsync(int idCabEgreso)
        {
            try
            {
                var cabEgreso = await _egresoRepository.GetByIdAsync(idCabEgreso);
                if (cabEgreso == null)
                    return Enumerable.Empty<DetEgresoDTO>();

                var tipos = await _tipoEgresoRepository.GetAllAsync();

                return cabEgreso.DetEgreso.Select(d => new DetEgresoDTO
                {
                    idDetEgreso = d.idDetEgreso,
                    idCabEgreso = d.idCabEgreso,
                    concepto = d.concepto,
                    monto = d.monto,
                    recurrente = d.recurrente,
                    comprobanteRuta = d.comprobanteRuta,
                    idTipoEgreso = d.idTipoEgreso,
                    TipoEgreso = tipos.Where(t => t.idTipoEgreso == d.idTipoEgreso)
                                     .Select(t => new TipoEgresoDTO { idTipoEgreso = t.idTipoEgreso, nombre = t.nombre })
                                     .FirstOrDefault()
                }).OrderBy(d => d.idDetEgreso);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener detalles del egreso: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<DetEgresoDTO>> GetDetallesRecientesAsync(int cabCount = 10)
        {
            try
            {
                var egresosRecientes = await _egresoRepository.GetAllAsync();
                var cabeceras = egresosRecientes
                    .OrderByDescending(e => e.fecha)
                    .Take(cabCount);

                var detalles = new List<DetEgresoDTO>();
                var tipos = await _tipoEgresoRepository.GetAllAsync();

                foreach (var cabecera in cabeceras)
                {
                    var detallesCab = cabecera.DetEgreso.Select(d => new DetEgresoDTO
                    {
                        idDetEgreso = d.idDetEgreso,
                        idCabEgreso = d.idCabEgreso,
                        concepto = d.concepto,
                        monto = d.monto,
                        recurrente = d.recurrente,
                        comprobanteRuta = d.comprobanteRuta,
                        idTipoEgreso = d.idTipoEgreso,
                        TipoEgreso = tipos.Where(t => t.idTipoEgreso == d.idTipoEgreso)
                                         .Select(t => new TipoEgresoDTO { idTipoEgreso = t.idTipoEgreso, nombre = t.nombre })
                                         .FirstOrDefault()
                    });

                    detalles.AddRange(detallesCab);
                }

                return detalles.OrderByDescending(d => d.idDetEgreso);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener detalles recientes: {ex.Message}", ex);
            }
        }

        public async Task<bool> ActualizarDetalleAsync(DetEgresoDTO detalle)
        {
            try
            {
                var detalleEntity = await _egresoRepository.GetByIdAsync(detalle.idDetEgreso);
                if (detalleEntity == null)
                    return false;

                // Actualizar propiedades
                // TODO: Implementar actualización específica según tu repositorio
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar detalle: {ex.Message}", ex);
            }
        }

        public async Task<bool> EliminarDetalleAsync(int idDetEgreso)
        {
            try
            {
                // TODO: Implementar eliminación según tu repositorio
                // await _egresoRepository.DeleteDetalleAsync(idDetEgreso);
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar detalle: {ex.Message}", ex);
            }
        }

        // MÉTODOS AUXILIARES PRIVADOS

        private async Task<CabEgresoDTO> ConvertirAEgresoDTO(CabEgreso egreso)
        {
            var tipos = await _tipoEgresoRepository.GetAllAsync();

            var detalles = egreso.DetEgreso.Select(d => new DetEgresoDTO
            {
                idDetEgreso = d.idDetEgreso,
                idCabEgreso = d.idCabEgreso,
                concepto = d.concepto,
                monto = d.monto,
                recurrente = d.recurrente,
                comprobanteRuta = d.comprobanteRuta,
                idTipoEgreso = d.idTipoEgreso,
                TipoEgreso = tipos.Where(t => t.idTipoEgreso == d.idTipoEgreso)
                                 .Select(t => new TipoEgresoDTO { idTipoEgreso = t.idTipoEgreso, nombre = t.nombre })
                                 .FirstOrDefault()
            }).ToList();

            return new CabEgresoDTO
            {
                idCabEgreso = egreso.idCabEgreso,
                fecha = egreso.fecha,
                montoTotal = egreso.montoTotal ?? 0,
                idUsuario = egreso.idUsuario,
                Detalles = detalles
            };
        }

        private int? GetUsuarioActualId()
        {
            // TODO: Implementar según tu sistema de autenticación
            // Podría ser desde Session, Claims, etc.
            return 1; // Placeholder
        }
    }

    /// <summary>
    /// EJEMPLO DE REPOSITORIOS REQUERIDOS
    /// Si no existen, implementar según este patrón
    /// </summary>
    public interface IEgresoRepositoryEjemplo : IRepository<CabEgreso>
    {
        Task<CabEgreso?> GetByIdWithDetailsAsync(int idCabEgreso);
        Task<IEnumerable<CabEgreso>> GetByDateRangeAsync(DateTime fechaDesde, DateTime fechaHasta);
        Task<bool> DeleteDetalleAsync(int idDetEgreso);
    }

    public interface ITipoEgresoRepositoryEjemplo : IRepository<TipoEgreso>
    {
        Task<IEnumerable<TipoEgreso>> GetByNombreAsync(string nombre);
        Task<IEnumerable<TipoEgreso>> GetActivosAsync();
    }
}

/*
NOTAS DE IMPLEMENTACIÓN:

1. REGISTRAR EN DI (App.xaml.cs):
   services.AddScoped<IEgresoService, EgresoService>();

2. VALIDAR INTERFACE IEgresoService:
   Debe tener todos los métodos usados en el ViewModel

3. REPOSITORIOS:
   Implementar según el patrón del proyecto

4. AUTENTICACIÓN:
   Ajustar GetUsuarioActualId() según tu sistema

5. VALIDACIONES:
   Agregar validaciones específicas del negocio

6. TRANSACCIONES:
   Considerar usar transacciones para operaciones complejas
*/