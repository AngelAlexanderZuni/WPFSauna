using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Interfaces;
using ProyectoSauna.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Services
{
    public class ComprobanteService : IComprobanteService
    {
        private readonly IComprobanteRepository _comprobanteRepository;
        private readonly ITipoComprobanteRepository _tipoComprobanteRepository;

        public ComprobanteService(IComprobanteRepository comprobanteRepository, ITipoComprobanteRepository tipoComprobanteRepository)
        {
            _comprobanteRepository = comprobanteRepository;
            _tipoComprobanteRepository = tipoComprobanteRepository;
        }

        public async Task<List<ComprobanteDTO>> GetAllAsync()
        {
            var entities = await _comprobanteRepository.GetAllAsync();
            return entities.Select(MapToDTO).ToList();
        }

        public async Task<ComprobanteDTO?> GetByIdAsync(int id)
        {
            var entity = await _comprobanteRepository.GetByIdAsync(id);
            return entity == null ? null : MapToDTO(entity);
        }

        public async Task<ComprobanteDTO> CreateAsync(ComprobanteDTO dto)
        {
            var entity = new Comprobante
            {
                serie = dto.serie,
                numero = dto.numero,
                fechaEmision = dto.fechaEmision,
                subtotal = dto.subtotal,
                igv = dto.igv,
                total = dto.total,
                idTipoComprobante = dto.idTipoComprobante,
                idCuenta = dto.idCuenta
            };

            var created = await _comprobanteRepository.AddAsync(entity);
            return MapToDTO(created);
        }

        public async Task UpdateAsync(ComprobanteDTO dto)
        {
            var entity = await _comprobanteRepository.GetByIdAsync(dto.idComprobante);
            if (entity != null)
            {
                entity.serie = dto.serie;
                entity.numero = dto.numero;
                entity.fechaEmision = dto.fechaEmision;
                entity.subtotal = dto.subtotal;
                entity.igv = dto.igv;
                entity.total = dto.total;
                entity.idTipoComprobante = dto.idTipoComprobante;
                entity.idCuenta = dto.idCuenta;

                await _comprobanteRepository.UpdateAsync(entity);
            }
        }

        public async Task DeleteAsync(int id)
        {
            await _comprobanteRepository.DeleteAsync(id);
        }

        public async Task<List<TipoComprobanteDTO>> GetTiposComprobanteAsync()
        {
            var entities = await _tipoComprobanteRepository.GetAllAsync();
            return entities.Select(t => new TipoComprobanteDTO
            {
                idTipoComprobante = t.idTipoComprobante,
                nombre = t.nombre
            }).ToList();
        }

        public async Task<List<ComprobanteDTO>> GetByCuentaIdAsync(int idCuenta)
        {
            var entities = await _comprobanteRepository.GetByCuentaIdAsync(idCuenta);
            return entities.Select(MapToDTO).ToList();
        }

        private ComprobanteDTO MapToDTO(Comprobante entity)
        {
            return new ComprobanteDTO
            {
                idComprobante = entity.idComprobante,
                serie = entity.serie,
                numero = entity.numero,
                fechaEmision = entity.fechaEmision,
                subtotal = entity.subtotal,
                igv = entity.igv,
                total = entity.total,
                idTipoComprobante = entity.idTipoComprobante,
                idCuenta = entity.idCuenta,
                TipoComprobanteNombre = entity.idTipoComprobanteNavigation?.nombre
            };
        }
    }
}
