using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Interfaces;
using ProyectoSauna.Models;
using ProyectoSauna.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Services
{
    public class ReporteService : IReporteService
    {
        private readonly SaunaDbContext _context;

        public ReporteService(SaunaDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReporteIngresoDTO>> GetIngresosPorFechaAsync(DateTime inicio, DateTime fin)
        {
            var query = @"
                SELECT CAST(fechaHora AS DATE) as Fecha, SUM(monto) as Total
                FROM Pago
                WHERE CAST(fechaHora AS DATE) BETWEEN @inicio AND @fin
                GROUP BY CAST(fechaHora AS DATE)
                ORDER BY Fecha";

            var pInicio = new SqlParameter("@inicio", inicio.Date);
            var pFin = new SqlParameter("@fin", fin.Date);

            return await _context.Database
                .SqlQueryRaw<ReporteIngresoDTO>(query, pInicio, pFin)
                .ToListAsync();
        }

        public async Task<List<ReporteEgresoDTO>> GetEgresosMensualesAsync(int mes, int anio)
        {
            var query = @"
                SELECT te.nombre AS TipoEgreso, SUM(de.monto) AS Total
                FROM DetEgreso de
                JOIN TipoEgreso te ON de.idTipoEgreso = te.idTipoEgreso
                JOIN CabEgreso ce ON de.idCabEgreso = ce.idCabEgreso
                WHERE MONTH(ce.fecha) = @mes AND YEAR(ce.fecha) = @anio
                GROUP BY te.nombre
                ORDER BY te.nombre";

            var pMes = new SqlParameter("@mes", mes);
            var pAnio = new SqlParameter("@anio", anio);

            return await _context.Database
                .SqlQueryRaw<ReporteEgresoDTO>(query, pMes, pAnio)
                .ToListAsync();
        }

        public async Task<FlujoCajaDTO> GetFlujoCajaAsync(int mes, int anio)
        {
            var query = @"
                SELECT 
                    COALESCE((SELECT SUM(monto) FROM Pago WHERE MONTH(fechaHora) = @mes AND YEAR(fechaHora) = @anio), 0) AS TotalIngresos,
                    COALESCE((SELECT SUM(de.monto)
                              FROM DetEgreso de
                              JOIN CabEgreso ce ON de.idCabEgreso = ce.idCabEgreso
                              WHERE MONTH(ce.fecha) = @mes AND YEAR(ce.fecha) = @anio), 0) AS TotalEgresos,
                    (
                        COALESCE((SELECT SUM(monto) FROM Pago WHERE MONTH(fechaHora) = @mes AND YEAR(fechaHora) = @anio), 0) -
                        COALESCE((SELECT SUM(de.monto)
                                  FROM DetEgreso de
                                  JOIN CabEgreso ce ON de.idCabEgreso = ce.idCabEgreso
                                  WHERE MONTH(ce.fecha) = @mes AND YEAR(ce.fecha) = @anio), 0)
                    ) AS UtilidadNeta";

            var pMes = new SqlParameter("@mes", mes);
            var pAnio = new SqlParameter("@anio", anio);

            var result = await _context.Database
                .SqlQueryRaw<FlujoCajaDTO>(query, pMes, pAnio)
                .ToListAsync();

            return result.FirstOrDefault() ?? new FlujoCajaDTO();
        }

        public async Task<List<ReporteClienteDTO>> GetMejoresClientesAsync(int top = 10)
        {
            var query = @"
                SELECT TOP (@top)
                       c.nombre + ' ' + c.apellidos AS NombreCompleto,
                       COUNT(cta.idCuenta) AS Visitas,
                       COALESCE(SUM(cta.total), 0) AS TotalGastado
                FROM Cuenta cta
                JOIN Cliente c ON cta.idCliente = c.idCliente
                JOIN EstadoCuenta ec ON ec.idEstadoCuenta = cta.idEstadoCuenta
                WHERE ec.nombre = 'PAGADA'
                GROUP BY c.nombre, c.apellidos
                ORDER BY TotalGastado DESC";

            var pTop = new SqlParameter("@top", top);

            return await _context.Database
                .SqlQueryRaw<ReporteClienteDTO>(query, pTop)
                .ToListAsync();
        }

        public async Task<List<ReporteProductoDTO>> GetTopProductosAsync(int top = 10)
        {
            var query = @"
                SELECT TOP (@top) 
                       p.nombre as NombreProducto, 
                       SUM(dc.cantidad) as CantidadVendida, 
                       COALESCE(SUM(dc.subtotal), 0) as IngresosGenerados
                FROM DetalleConsumo dc
                JOIN Producto p ON dc.idProducto = p.idProducto
                GROUP BY p.nombre
                ORDER BY CantidadVendida DESC";

            var pTop = new SqlParameter("@top", top);

            return await _context.Database
                .SqlQueryRaw<ReporteProductoDTO>(query, pTop)
                .ToListAsync();
        }
    }
}
