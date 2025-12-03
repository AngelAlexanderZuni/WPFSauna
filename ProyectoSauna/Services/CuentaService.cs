using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Interfaces;
using ProyectoSauna.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Services
{
    public class CuentaService : ICuentaService
    {
        private readonly ICuentaRepository _cuentaRepository;

        public CuentaService(ICuentaRepository cuentaRepository)
        {
            _cuentaRepository = cuentaRepository;
        }

        public async Task<CuentaDTO?> GetByIdAsync(int id)
        {
            var entity = await _cuentaRepository.GetCuentaByIdAsync(id);
            return entity == null ? null : MapToDTO(entity);
        }

        public async Task<List<CuentaDTO>> GetCuentasActivasAsync()
        {
            // Asumiendo que "Pendientes" equivale a "Activas" en este contexto
            var entities = await _cuentaRepository.GetCuentasPendientesAsync();
            return entities.Select(MapToDTO).ToList();
        }

        private CuentaDTO MapToDTO(Cuenta entity)
        {
            return new CuentaDTO
            {
                idCuenta = entity.idCuenta,
                fechaHoraIngreso = entity.fechaHoraCreacion,
                fechaHoraSalida = entity.fechaHoraSalida,
                subtotalConsumos = entity.subtotalConsumos,
                descuento = entity.descuento,
                total = entity.total,
                idCliente = entity.idCliente,
                idEstadoCuenta = entity.idEstadoCuenta,
                idUsuario = entity.idUsuarioCreador,
                
                // Mapeo de propiedades de navegación si están cargadas
                NombreCliente = entity.idClienteNavigation != null ? $"{entity.idClienteNavigation.nombre} {entity.idClienteNavigation.apellidos}" : "",
                DocumentoCliente = entity.idClienteNavigation?.numero_documento ?? "",
                EstadoCuenta = entity.idEstadoCuentaNavigation?.nombre ?? "",
                // NombreUsuario no está directo en la entidad Cuenta según el esquema usual, 
                // pero si existe la navegación se puede mapear.
                // Asumimos que idUsuarioCreadorNavigation existe.
                // Si no, se deja vacío o se ajusta según la entidad real.
            };
        }
    }
}
