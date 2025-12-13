-- Seed data aligned to dbo schema in Bd/sauna_final.sql
-- Range: Oct–Dec 2025

SET NOCOUNT ON;
BEGIN TRAN;

USE ProyectoSauna;

/* 1) Lookups and base data */
-- Roles
IF NOT EXISTS (SELECT 1 FROM dbo.Rol WHERE nombre = N'Administrador')
    INSERT INTO dbo.Rol (nombre) VALUES (N'Administrador');
DECLARE @idRolAdmin INT = (SELECT TOP 1 idRol FROM dbo.Rol WHERE nombre = N'Administrador');

-- Usuario creador
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE nombreUsuario = N'admin')
    INSERT INTO dbo.Usuario (nombreUsuario, contraseniaHash, correo, fechaCreacion, activo, idRol)
    VALUES (N'admin', N'adminhash', N'admin@example.com', SYSDATETIME(), 1, @idRolAdmin);
DECLARE @idUsuarioAdmin INT = (SELECT TOP 1 idUsuario FROM dbo.Usuario WHERE nombreUsuario = N'admin');

-- Estados de Cuenta
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoCuenta WHERE nombre = N'PENDIENTE')
    INSERT INTO dbo.EstadoCuenta (nombre) VALUES (N'PENDIENTE');
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoCuenta WHERE nombre = N'PAGADA')
    INSERT INTO dbo.EstadoCuenta (nombre) VALUES (N'PAGADA');
DECLARE @idEstadoPendiente INT = (SELECT TOP 1 idEstadoCuenta FROM dbo.EstadoCuenta WHERE nombre = N'PENDIENTE');
DECLARE @idEstadoPagada   INT = (SELECT TOP 1 idEstadoCuenta FROM dbo.EstadoCuenta WHERE nombre = N'PAGADA');

-- Métodos de pago
IF NOT EXISTS (SELECT 1 FROM dbo.MetodoPago WHERE nombre = 'Efectivo')
    INSERT INTO dbo.MetodoPago (nombre) VALUES ('Efectivo');
IF NOT EXISTS (SELECT 1 FROM dbo.MetodoPago WHERE nombre = 'Tarjeta')
    INSERT INTO dbo.MetodoPago (nombre) VALUES ('Tarjeta');
IF NOT EXISTS (SELECT 1 FROM dbo.MetodoPago WHERE nombre = 'Yape')
    INSERT INTO dbo.MetodoPago (nombre) VALUES ('Yape');
DECLARE @idPagoEfectivo INT = (SELECT TOP 1 idMetodoPago FROM dbo.MetodoPago WHERE nombre = 'Efectivo');
DECLARE @idPagoTarjeta  INT = (SELECT TOP 1 idMetodoPago FROM dbo.MetodoPago WHERE nombre = 'Tarjeta');

-- Tipos de comprobante
IF NOT EXISTS (SELECT 1 FROM dbo.TipoComprobante WHERE nombre = N'Boleta')
    INSERT INTO dbo.TipoComprobante (nombre) VALUES (N'Boleta');
IF NOT EXISTS (SELECT 1 FROM dbo.TipoComprobante WHERE nombre = N'Factura')
    INSERT INTO dbo.TipoComprobante (nombre) VALUES (N'Factura');
DECLARE @idBoleta  INT = (SELECT TOP 1 idTipoComprobante FROM dbo.TipoComprobante WHERE nombre = N'Boleta');
DECLARE @idFactura INT = (SELECT TOP 1 idTipoComprobante FROM dbo.TipoComprobante WHERE nombre = N'Factura');

-- Categorías
IF NOT EXISTS (SELECT 1 FROM dbo.CategoriaProducto WHERE nombre = N'Bebidas')
    INSERT INTO dbo.CategoriaProducto (nombre, descripcion, activo) VALUES (N'Bebidas', N'Bebidas frías', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.CategoriaProducto WHERE nombre = N'Snacks')
    INSERT INTO dbo.CategoriaProducto (nombre, descripcion, activo) VALUES (N'Snacks', N'Piqueos', 1);
DECLARE @idCatBebidas INT = (SELECT TOP 1 idCategoriaProducto FROM dbo.CategoriaProducto WHERE nombre = N'Bebidas');
DECLARE @idCatSnacks  INT = (SELECT TOP 1 idCategoriaProducto FROM dbo.CategoriaProducto WHERE nombre = N'Snacks');

IF NOT EXISTS (SELECT 1 FROM dbo.CategoriaServicio WHERE nombre = N'Entrada')
    INSERT INTO dbo.CategoriaServicio (nombre, activo) VALUES (N'Entrada', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.CategoriaServicio WHERE nombre = N'Terapias')
    INSERT INTO dbo.CategoriaServicio (nombre, activo) VALUES (N'Terapias', 1);
DECLARE @idCatEntrada INT = (SELECT TOP 1 idCategoriaServicio FROM dbo.CategoriaServicio WHERE nombre = N'Entrada');
DECLARE @idCatTerapia INT = (SELECT TOP 1 idCategoriaServicio FROM dbo.CategoriaServicio WHERE nombre = N'Terapias');

-- Productos (códigos únicos requeridos)
IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE codigo = N'P001')
    INSERT INTO dbo.Producto (codigo, nombre, descripcion, precioCompra, precioVenta, stockActual, stockMinimo, activo, idCategoriaProducto, fechaRegistro, precioCosto, unidadMedida)
    VALUES (N'P001', N'Coca Cola 500ml', N'Gaseosa 500ml', 2.50, 3.50, 100, 10, 1, @idCatBebidas, GETDATE(), 2.50, N'und');
IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE codigo = N'P002')
    INSERT INTO dbo.Producto (codigo, nombre, descripcion, precioCompra, precioVenta, stockActual, stockMinimo, activo, idCategoriaProducto, fechaRegistro, precioCosto, unidadMedida)
    VALUES (N'P002', N'Cerveza Pilsen', N'Botella 620ml', 5.50, 7.00, 80, 10, 1, @idCatBebidas, GETDATE(), 5.50, N'und');
IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE codigo = N'P003')
    INSERT INTO dbo.Producto (codigo, nombre, descripcion, precioCompra, precioVenta, stockActual, stockMinimo, activo, idCategoriaProducto, fechaRegistro, precioCosto, unidadMedida)
    VALUES (N'P003', N'Agua Mineral', N'Botella 500ml', 1.20, 2.00, 120, 15, 1, @idCatBebidas, GETDATE(), 1.20, N'und');
IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE codigo = N'P004')
    INSERT INTO dbo.Producto (codigo, nombre, descripcion, precioCompra, precioVenta, stockActual, stockMinimo, activo, idCategoriaProducto, fechaRegistro, precioCosto, unidadMedida)
    VALUES (N'P004', N'Piqueo Mixto', N'Mix snacks', 8.00, 12.00, 40, 5, 1, @idCatSnacks, GETDATE(), 8.00, N'und');
DECLARE @idProdCoca   INT = (SELECT TOP 1 idProducto FROM dbo.Producto WHERE codigo = N'P001');
DECLARE @idProdPilsen INT = (SELECT TOP 1 idProducto FROM dbo.Producto WHERE codigo = N'P002');
DECLARE @idProdAgua   INT = (SELECT TOP 1 idProducto FROM dbo.Producto WHERE codigo = N'P003');
DECLARE @idProdPiqueo INT = (SELECT TOP 1 idProducto FROM dbo.Producto WHERE codigo = N'P004');

-- Servicios
IF NOT EXISTS (SELECT 1 FROM dbo.Servicio WHERE nombre = N'Entrada Sauna')
    INSERT INTO dbo.Servicio (nombre, precio, duracionEstimada, activo, idCategoriaServicio)
    VALUES (N'Entrada Sauna', 20.00, 120, 1, @idCatEntrada);
IF NOT EXISTS (SELECT 1 FROM dbo.Servicio WHERE nombre = N'Masaje Relax')
    INSERT INTO dbo.Servicio (nombre, precio, duracionEstimada, activo, idCategoriaServicio)
    VALUES (N'Masaje Relax', 30.00, 60, 1, @idCatTerapia);
DECLARE @idSrvEntrada INT = (SELECT TOP 1 idServicio FROM dbo.Servicio WHERE nombre = N'Entrada Sauna');
DECLARE @idSrvMasaje  INT = (SELECT TOP 1 idServicio FROM dbo.Servicio WHERE nombre = N'Masaje Relax');

-- Clientes
IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE numero_documento = N'12345678')
    INSERT INTO dbo.Cliente (nombre, apellidos, numero_documento, telefono, correo, direccion, fechaNacimiento, fechaRegistro, visitasTotales, activo)
    VALUES (N'Maria', N'Garcia Lopez', N'12345678', N'999111222', N'maria@example.com', N'Av. 1', '1995-05-10', SYSDATETIME(), 0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE numero_documento = N'87654321')
    INSERT INTO dbo.Cliente (nombre, apellidos, numero_documento, telefono, correo, direccion, fechaNacimiento, fechaRegistro, visitasTotales, activo)
    VALUES (N'Carlos', N'Rodriguez Perez', N'87654321', N'988222333', N'carlos@example.com', N'Av. 2', '1990-08-22', SYSDATETIME(), 0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE numero_documento = N'45678912')
    INSERT INTO dbo.Cliente (nombre, apellidos, numero_documento, telefono, correo, direccion, fechaNacimiento, fechaRegistro, visitasTotales, activo)
    VALUES (N'Juan', N'Perez Gomez', N'45678912', N'977333444', N'juan@example.com', N'Av. 3', '1992-01-15', SYSDATETIME(), 0, 1);
DECLARE @idCli1 INT = (SELECT TOP 1 idCliente FROM dbo.Cliente WHERE numero_documento = N'12345678');
DECLARE @idCli2 INT = (SELECT TOP 1 idCliente FROM dbo.Cliente WHERE numero_documento = N'87654321');
DECLARE @idCli3 INT = (SELECT TOP 1 idCliente FROM dbo.Cliente WHERE numero_documento = N'45678912');

/* 2) Cuentas Oct–Nov–Dec 2025 */
-- Helper to insert a cuenta
DECLARE @oct1 DATETIME2 = '2025-10-05 10:15';
DECLARE @oct2 DATETIME2 = '2025-10-12 11:20';
DECLARE @oct3 DATETIME2 = '2025-10-25 18:40';
DECLARE @nov1 DATETIME2 = '2025-11-08 09:55';
DECLARE @nov2 DATETIME2 = '2025-11-15 12:30';
DECLARE @nov3 DATETIME2 = '2025-11-27 19:05';
DECLARE @dec1 DATETIME2 = '2025-12-03 10:05';
DECLARE @dec2 DATETIME2 = '2025-12-10 11:35';
DECLARE @dec3 DATETIME2 = '2025-12-20 17:45';

-- Insert cuentas (mix de PENDIENTE/PAGADA)
INSERT INTO dbo.Cuenta (fechaHoraCreacion, fechaHoraSalida, subtotalConsumos, descuentos, total, idEstadoCuenta, idUsuarioCreador, idCliente, idPromocion, montoPagado, saldo, horaEntrada)
VALUES
(@oct1, DATEADD(HOUR,2,@oct1), 0, 0, 0, @idEstadoPagada,   @idUsuarioAdmin, @idCli1, NULL, 0, 0, 10.15),
(@oct2, DATEADD(HOUR,2,@oct2), 0, 0, 0, @idEstadoPagada,   @idUsuarioAdmin, @idCli2, NULL, 0, 0, 11.20),
(@oct3, DATEADD(HOUR,2,@oct3), 0, 0, 0, @idEstadoPagada,   @idUsuarioAdmin, @idCli3, NULL, 0, 0, 18.40),
(@nov1, DATEADD(HOUR,2,@nov1), 0, 0, 0, @idEstadoPagada,   @idUsuarioAdmin, @idCli1, NULL, 0, 0,  9.55),
(@nov2, DATEADD(HOUR,2,@nov2), 0, 0, 0, @idEstadoPagada,   @idUsuarioAdmin, @idCli2, NULL, 0, 0, 12.30),
(@nov3, DATEADD(HOUR,2,@nov3), 0, 0, 0, @idEstadoPendiente,@idUsuarioAdmin, @idCli3, NULL, 0, 0, 19.05),
(@dec1, DATEADD(HOUR,2,@dec1), 0, 0, 0, @idEstadoPagada,   @idUsuarioAdmin, @idCli1, NULL, 0, 0, 10.05),
(@dec2, DATEADD(HOUR,2,@dec2), 0, 0, 0, @idEstadoPagada,   @idUsuarioAdmin, @idCli2, NULL, 0, 0, 11.35),
(@dec3, DATEADD(HOUR,2,@dec3), 0, 0, 0, @idEstadoPagada,   @idUsuarioAdmin, @idCli3, NULL, 0, 0, 17.45);

-- Capture IDs por fecha y cliente
DECLARE @idOct1 INT = (SELECT TOP 1 idCuenta FROM dbo.Cuenta WHERE idCliente=@idCli1 AND CONVERT(date, fechaHoraCreacion)='2025-10-05');
DECLARE @idOct2 INT = (SELECT TOP 1 idCuenta FROM dbo.Cuenta WHERE idCliente=@idCli2 AND CONVERT(date, fechaHoraCreacion)='2025-10-12');
DECLARE @idOct3 INT = (SELECT TOP 1 idCuenta FROM dbo.Cuenta WHERE idCliente=@idCli3 AND CONVERT(date, fechaHoraCreacion)='2025-10-25');
DECLARE @idNov1 INT = (SELECT TOP 1 idCuenta FROM dbo.Cuenta WHERE idCliente=@idCli1 AND CONVERT(date, fechaHoraCreacion)='2025-11-08');
DECLARE @idNov2 INT = (SELECT TOP 1 idCuenta FROM dbo.Cuenta WHERE idCliente=@idCli2 AND CONVERT(date, fechaHoraCreacion)='2025-11-15');
DECLARE @idNov3 INT = (SELECT TOP 1 idCuenta FROM dbo.Cuenta WHERE idCliente=@idCli3 AND CONVERT(date, fechaHoraCreacion)='2025-11-27');
DECLARE @idDec1 INT = (SELECT TOP 1 idCuenta FROM dbo.Cuenta WHERE idCliente=@idCli1 AND CONVERT(date, fechaHoraCreacion)='2025-12-03');
DECLARE @idDec2 INT = (SELECT TOP 1 idCuenta FROM dbo.Cuenta WHERE idCliente=@idCli2 AND CONVERT(date, fechaHoraCreacion)='2025-12-10');
DECLARE @idDec3 INT = (SELECT TOP 1 idCuenta FROM dbo.Cuenta WHERE idCliente=@idCli3 AND CONVERT(date, fechaHoraCreacion)='2025-12-20');

/* 3) Detalle de Servicios (Entrada) */
INSERT INTO dbo.DetalleServicio (cantidad, precioUnitario, subtotal, idCuenta, idServicio)
VALUES
(1, 20.00, 20.00, @idOct1, @idSrvEntrada),
(1, 20.00, 20.00, @idOct2, @idSrvEntrada),
(1, 20.00, 20.00, @idOct3, @idSrvEntrada),
(1, 20.00, 20.00, @idNov1, @idSrvEntrada),
(1, 20.00, 20.00, @idNov2, @idSrvEntrada),
(1, 20.00, 20.00, @idNov3, @idSrvEntrada),
(1, 20.00, 20.00, @idDec1, @idSrvEntrada),
(1, 20.00, 20.00, @idDec2, @idSrvEntrada),
(1, 20.00, 20.00, @idDec3, @idSrvEntrada);

/* 4) Detalle de Consumos (Productos) */
INSERT INTO dbo.DetalleConsumo (cantidad, precioUnitario, subtotal, idCuenta, idProducto)
VALUES
(2, 3.50, 7.00,  @idOct1, @idProdCoca),
(1, 12.00, 12.00, @idOct1, @idProdPiqueo),
(1, 2.00,  2.00,  @idOct2, @idProdAgua),
(2, 7.00,  14.00, @idOct3, @idProdPilsen),
(1, 3.50,  3.50,  @idNov1, @idProdCoca),
(2, 2.00,  4.00,  @idNov2, @idProdAgua),
(1, 7.00,  7.00,  @idNov3, @idProdPilsen),
(1, 12.00, 12.00, @idDec1, @idProdPiqueo),
(3, 3.50,  10.50, @idDec2, @idProdCoca),
(1, 2.00,  2.00,  @idDec3, @idProdAgua);

/* 5) Actualizar totales de Cuenta */
UPDATE c
SET c.subtotalConsumos = ISNULL(dc.totalConsumo,0) + ISNULL(ds.totalServicio,0),
    c.descuentos       = 0,
    c.total            = ISNULL(dc.totalConsumo,0) + ISNULL(ds.totalServicio,0),
    c.montoPagado      = CASE WHEN c.idEstadoCuenta = @idEstadoPagada THEN ISNULL(dc.totalConsumo,0) + ISNULL(ds.totalServicio,0) ELSE 0 END,
    c.saldo            = CASE WHEN c.idEstadoCuenta = @idEstadoPagada THEN 0 ELSE ISNULL(dc.totalConsumo,0) + ISNULL(ds.totalServicio,0) END
FROM dbo.Cuenta c
OUTER APPLY (
    SELECT SUM(subtotal) AS totalConsumo
    FROM dbo.DetalleConsumo dcc
    WHERE dcc.idCuenta = c.idCuenta
) dc
OUTER APPLY (
    SELECT SUM(subtotal) AS totalServicio
    FROM dbo.DetalleServicio dss
    WHERE dss.idCuenta = c.idCuenta
) ds;

/* 6) Pagos para cuentas PAGADAS */
INSERT INTO dbo.Pago (fechaHora, monto, numeroReferencia, idMetodoPago, idCuenta)
SELECT DATEADD(HOUR,1,fechaHoraCreacion), total, CONCAT('REF', idCuenta),
             CASE (idCuenta % 2) WHEN 0 THEN @idPagoTarjeta ELSE @idPagoEfectivo END,
             idCuenta
FROM dbo.Cuenta
WHERE idEstadoCuenta = @idEstadoPagada
    AND total > 0;

/* 7) Comprobantes (boleta) para cuentas PAGADAS */
INSERT INTO dbo.Comprobante (serie, numero, fechaEmision, subtotal, igv, total, idTipoComprobante, idCuenta)
SELECT 'B001', RIGHT('00000000' + CAST(idCuenta AS VARCHAR(8)), 8), fechaHoraSalida,
       total, 0, total, @idBoleta, idCuenta
FROM dbo.Cuenta
WHERE idEstadoCuenta = @idEstadoPagada
  AND NOT EXISTS (SELECT 1 FROM dbo.Comprobante c WHERE c.idCuenta = dbo.Cuenta.idCuenta);

/* 8) Egresos por mes via CabEgreso + DetEgreso */
-- Tipos de egreso
IF NOT EXISTS (SELECT 1 FROM dbo.TipoEgreso WHERE nombre = N'Servicios')
    INSERT INTO dbo.TipoEgreso (nombre) VALUES (N'Servicios');
IF NOT EXISTS (SELECT 1 FROM dbo.TipoEgreso WHERE nombre = N'Sueldos')
    INSERT INTO dbo.TipoEgreso (nombre) VALUES (N'Sueldos');
IF NOT EXISTS (SELECT 1 FROM dbo.TipoEgreso WHERE nombre = N'Mantenimiento')
    INSERT INTO dbo.TipoEgreso (nombre) VALUES (N'Mantenimiento');
DECLARE @idEgrServicios INT = (SELECT TOP 1 idTipoEgreso FROM dbo.TipoEgreso WHERE nombre = N'Servicios');
DECLARE @idEgrSueldos  INT = (SELECT TOP 1 idTipoEgreso FROM dbo.TipoEgreso WHERE nombre = N'Sueldos');
DECLARE @idEgrMantto   INT = (SELECT TOP 1 idTipoEgreso FROM dbo.TipoEgreso WHERE nombre = N'Mantenimiento');

-- CabEgreso por mes
DECLARE @cabOct INT, @cabNov INT, @cabDec INT;
INSERT INTO dbo.CabEgreso (fecha, montoTotal, idUsuario) VALUES ('2025-10-31', 0, @idUsuarioAdmin);
SET @cabOct = SCOPE_IDENTITY();
INSERT INTO dbo.CabEgreso (fecha, montoTotal, idUsuario) VALUES ('2025-11-30', 0, @idUsuarioAdmin);
SET @cabNov = SCOPE_IDENTITY();
INSERT INTO dbo.CabEgreso (fecha, montoTotal, idUsuario) VALUES ('2025-12-31', 0, @idUsuarioAdmin);
SET @cabDec = SCOPE_IDENTITY();

-- Detalles de egreso
INSERT INTO dbo.DetEgreso (idCabEgreso, concepto, monto, recurrente, comprobanteRuta, idTipoEgreso)
VALUES
(@cabOct, N'Servicios públicos', 250.00, 0, NULL, @idEgrServicios),
(@cabOct, N'Sueldos Octubre',   1200.00, 0, NULL, @idEgrSueldos),
(@cabNov, N'Servicios Noviembre',260.00, 0, NULL, @idEgrServicios),
(@cabNov, N'Mantto equipos',     400.00, 0, NULL, @idEgrMantto),
(@cabDec, N'Sueldos Diciembre', 1300.00, 0, NULL, @idEgrSueldos),
(@cabDec, N'Servicios Diciembre',270.00, 0, NULL, @idEgrServicios);

-- Actualizar montos en CabEgreso
UPDATE ce
SET ce.montoTotal = ISNULL(deSum.TotalMonto, 0)
FROM dbo.CabEgreso ce
OUTER APPLY (
    SELECT SUM(monto) AS TotalMonto
    FROM dbo.DetEgreso de
    WHERE de.idCabEgreso = ce.idCabEgreso
) deSum
WHERE ce.idCabEgreso IN (@cabOct, @cabNov, @cabDec);

COMMIT;

/* 9) Validación rápida (opcionales) */
-- Ingresos por mes
-- SELECT FORMAT(fechaHora, 'yyyy-MM') AS Mes, SUM(monto) AS TotalIngresos
-- FROM dbo.Pago
-- WHERE fechaHora >= '2025-10-01' AND fechaHora < '2026-01-01'
-- GROUP BY FORMAT(fechaHora, 'yyyy-MM')
-- ORDER BY Mes;

-- Egresos por mes
-- SELECT FORMAT(ce.fecha, 'yyyy-MM') AS Mes, SUM(de.monto) AS TotalEgresos
-- FROM dbo.CabEgreso ce
-- JOIN dbo.DetEgreso de ON de.idCabEgreso = ce.idCabEgreso
-- WHERE ce.fecha >= '2025-10-01' AND ce.fecha < '2026-01-01'
-- GROUP BY FORMAT(ce.fecha, 'yyyy-MM')
-- ORDER BY Mes;
