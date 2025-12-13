using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoSauna.Models.DTOs;

namespace ProyectoSauna.Interfaces
{
    public interface IReporteService
    {
        Task<List<ReporteIngresoDTO>> GetIngresosPorFechaAsync(DateTime inicio, DateTime fin);
        Task<List<ReporteEgresoDTO>> GetEgresosMensualesAsync(int mes, int anio);
        Task<List<ReporteProductoDTO>> GetTopProductosAsync(int top = 10);
        Task<FlujoCajaDTO> GetFlujoCajaAsync(int mes, int anio);
        Task<List<ReporteClienteDTO>> GetMejoresClientesAsync(int top = 10);
    }
}
