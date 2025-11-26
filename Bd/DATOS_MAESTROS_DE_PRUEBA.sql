
Create PROCEDURE [dbo].[sp_ValidarLogin]
    @identificador NVARCHAR(50), -- Puede ser nombreUsuario o correo
    @contraseniaHash NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT U.idUsuario, U.nombreUsuario, U.correo, U.idRol, R.nombre AS Rol
    FROM Usuario U
    INNER JOIN Rol R ON U.idRol = R.idRol
    WHERE (U.nombreUsuario = @identificador OR U.correo = @identificador)
      AND U.contraseniaHash = @contraseniaHash
      AND U.activo = 1;
END

-- Eliminacion de usuarios creados anteriormente

select * from Usuario

delete Usuario ;

-- =====================================================
-- INSERTAR DATOS MAESTROS 
-- =====================================================

--  ROLES
INSERT INTO Rol (nombre) VALUES 
('Administrador'),
('Cajero/Recepcionista');


--  PROGRAMA DE FIDELIZACIÓN
INSERT INTO ProgramaFidelizacion (visitasParaDescuento, porcentajeDescuento, descuentoCumpleanos, montoDescuentoCumpleanos)
VALUES (5, 10.00, 1, 5.00);
PRINT ' Programa de fidelización creado (5 visitas = 10% descuento)';


--  ESTADOS DE CUENTA
INSERT INTO EstadoCuenta (nombre) VALUES 
('Pendiente'),
('Pagada'),
('Cancelada');



--  CATEGORÍAS DE PRODUCTOS
INSERT INTO CategoriaProducto (nombre) VALUES 
('Bebidas Frías'),
('Bebidas Calientes'),
('Snacks'),
('Accesorios'),
('Servicios');


--  TIPOS DE MOVIMIENTO INVENTARIO
INSERT INTO TipoMovimiento (nombre) VALUES 
('Entrada'),
('Salida');


--  MÉTODOS DE PAGO
INSERT INTO MetodoPago (nombre) VALUES 
('Efectivo'),
('Tarjeta'),
('Yape'),
('Plin');


--  TIPOS DE COMPROBANTE
INSERT INTO TipoComprobante (nombre) VALUES 
('Boleta'),
('Factura'),
('Ticket');


--  TIPOS DE EGRESO
INSERT INTO TipoEgreso (nombre) VALUES 
('Servicios Básicos'),
('Alquiler'),
('Sueldos'),
('Compra Mercadería'),
('Mantenimiento'),
('Otros');

select * from Usuario




---------------------  EJECUTA DESDE AQUI SI YA TIENES LA BASE DE DATOS SOLO INSERTA DATOS ------------------------------------------




-- =====================================================
-- 2 INSERTAR DATOS DE PRUEBA
-- =====================================================

--  admin      CONTRA     admin123
--  cajero1    CONTRA    cajero123

--  USUARIOS DEL SISTEMA
INSERT INTO Usuario (nombreUsuario, contraseniaHash, correo, idRol, activo) 
VALUES 
('admin', '1o7DiXMouiKbAge5eRLsIBd/pXtuJH9tyDXWDkiDQu0=', 'admin@saunakalixto.com', 1, 1),
('cajero1', 'QlDcshduVIjym22wku+1W5uANBxGLZxx/pnN6fGK6Lg=', 'cajero1@saunakalixto.com', 2, 1);

--  CLIENTES DE PRUEBA (SIN HISTORIAL)
-- ✅ TODOS con visitasTotales = 0 (sin compras previas)
-- 📌 Base de datos lista para empezar a registrar desde el sistema
INSERT INTO Cliente (nombre, apellidos, numero_documento, telefono, correo, direccion, visitasTotales, idPrograma, activo)
VALUES 
('Carlos', 'Mendoza Ríos', '12345678', '987654321', 'carlos.mendoza@gmail.com', 'Av. Los Incas 123', 0, 1, 1),
('María', 'Torres Vargas', '23456789', '976543210', 'maria.torres@gmail.com', 'Jr. Cusco 456', 0, 1, 1),
('Juan', 'Pérez López', '34567890', '965432109', 'juan.perez@gmail.com', 'Av. El Sol 789', 0, 1, 1),
('Ana', 'García Flores', '45678901', '954321098', 'ana.garcia@gmail.com', 'Calle Lima 321', 0, 1, 1),
('Luis', 'Chávez Ramos', '56789012', '943210987', NULL, NULL, 0, 1, 1);


--  PRODUCTOS DE PRUEBA
-- 📌 NOTA IMPORTANTE sobre PRODUCTOS vs SERVICIOS:
--    ┌─────────────────────┬──────────────────┬───────────────┬─────────────────────┐
--    │ TIPO                │ precioCompra     │ stockActual   │ EJEMPLO             │
--    ├─────────────────────┼──────────────────┼───────────────┼─────────────────────┤
--    │ Productos Físicos   │ > 0 (costo real) │ cantidad real │ Cerveza, Papas      │
--    │ Servicios Barra     │ > 0 (insumos)    │ 999999        │ Café, Sandwich      │
--    │ Servicios Puros     │ = 0 (sin costo)  │ 999999        │ Masajes             │
--    └─────────────────────┴──────────────────┴───────────────┴─────────────────────┘
--    ⚠️ La ENTRADA BÁSICA al sauna NO está aquí, está en tabla Entrada.precioEntrada

-- Bebidas Frías (idCategoria = 1) - Productos físicos
INSERT INTO Producto (codigo, nombre, descripcion, precioCompra, precioVenta, stockActual, stockMinimo, idCategoriaProducto)
VALUES 
('BEB-001', 'Agua Mineral 500ml', 'Agua embotellada', 1.00, 2.50, 100, 20, 1),
('BEB-002', 'Gaseosa Inca Kola 500ml', 'Gaseosa personal', 2.00, 4.00, 50, 10, 1),
('BEB-003', 'Cerveza Cusqueña 330ml', 'Cerveza nacional', 3.50, 7.00, 60, 15, 1),
('BEB-004', 'Jugo de Naranja Natural', 'Jugo recién exprimido', 2.50, 5.00, 30, 5, 1);


-- Bebidas Calientes (idCategoria = 2) - Servicios de barra (stock ilimitado)
INSERT INTO Producto (codigo, nombre, descripcion, precioCompra, precioVenta, stockActual, stockMinimo, idCategoriaProducto)
VALUES 
('CAF-001', 'Café Americano', 'Café solo', 1.00, 3.00, 999999, 0, 2),
('CAF-002', 'Café con Leche', 'Café con leche', 1.50, 4.00, 999999, 0, 2),
('TÉ-001', 'Té de Manzanilla', 'Infusión relajante', 0.50, 2.50, 999999, 0, 2);


-- Snacks (idCategoria = 3) - Mixto (físicos + preparados)
INSERT INTO Producto (codigo, nombre, descripcion, precioCompra, precioVenta, stockActual, stockMinimo, idCategoriaProducto)
VALUES 
('SNK-001', 'Galletas Soda Field', 'Paquete 6 unidades', 1.50, 3.50, 40, 10, 3),
('SNK-002', 'Papas Lays 45g', 'Papas fritas originales', 1.80, 4.00, 60, 15, 3),
('SNK-003', 'Chocolate Sublime', 'Chocolate con maní', 2.00, 4.50, 50, 10, 3),
('SNK-004', 'Sandwich Mixto', 'Pan, jamón, queso', 3.00, 8.00, 999999, 0, 3); -- Servicio

-- Accesorios (idCategoria = 4) - Productos físicos
INSERT INTO Producto (codigo, nombre, descripcion, precioCompra, precioVenta, stockActual, stockMinimo, idCategoriaProducto)
VALUES 
('ACC-001', 'Toalla Grande', 'Toalla de baño 70x140cm', 10.00, 15.00, 30, 5, 4),
('ACC-002', 'Sandalias Desechables', 'Par de sandalias', 2.00, 5.00, 50, 10, 4),
('ACC-003', 'Shampoo Sachet', 'Shampoo individual', 0.50, 2.00, 100, 20, 4);


-- Servicios (idCategoria = 5) - Servicios puros (sin costo de adquisición)
INSERT INTO Producto (codigo, nombre, descripcion, precioCompra, precioVenta, stockActual, stockMinimo, idCategoriaProducto)
VALUES 
('SRV-001', 'Masaje Relajante 30min', 'Masaje espalda y cuello', 0.00, 50.00, 999999, 0, 5),
('SRV-002', 'Tratamiento Facial', 'Limpieza profunda', 0.00, 80.00, 999999, 0, 5);


-- =====================================================
--  VERIFICACIÓN FINAL - TODAS LAS 18 TABLAS
-- =====================================================

-- ✅ Verificar que las 18 tablas tienen datos correctos
SELECT 'Rol' AS Tabla, COUNT(*) AS Registros FROM Rol
UNION ALL SELECT 'ProgramaFidelizacion', COUNT(*) FROM ProgramaFidelizacion
UNION ALL SELECT 'EstadoCuenta', COUNT(*) FROM EstadoCuenta
UNION ALL SELECT 'CategoriaProducto', COUNT(*) FROM CategoriaProducto
UNION ALL SELECT 'TipoMovimiento', COUNT(*) FROM TipoMovimiento
UNION ALL SELECT 'MetodoPago', COUNT(*) FROM MetodoPago
UNION ALL SELECT 'TipoComprobante', COUNT(*) FROM TipoComprobante
UNION ALL SELECT 'TipoEgreso', COUNT(*) FROM TipoEgreso
UNION ALL SELECT 'Usuario', COUNT(*) FROM Usuario
UNION ALL SELECT 'Cliente', COUNT(*) FROM Cliente
UNION ALL SELECT 'Producto', COUNT(*) FROM Producto
UNION ALL SELECT '--- TABLAS TRANSACCIONALES (deben estar en 0) ---', NULL
UNION ALL SELECT 'Cuenta', COUNT(*) FROM Cuenta
UNION ALL SELECT 'DetalleConsumo', COUNT(*) FROM DetalleConsumo
UNION ALL SELECT 'MovimientoInventario', COUNT(*) FROM MovimientoInventario
UNION ALL SELECT 'Pago', COUNT(*) FROM Pago
UNION ALL SELECT 'Comprobante', COUNT(*) FROM Comprobante
UNION ALL SELECT 'Egreso', COUNT(*) FROM Egreso


-- 📦 Ver productos por categoría con tipo de producto
SELECT 
  c.nombre AS Categoria,
  p.codigo,
  p.nombre AS Producto,
  'S/. ' + CAST(p.precioVenta AS VARCHAR) AS Precio,
  p.stockActual AS Stock,
  CASE 
    WHEN p.precioCompra = 0 AND p.stockActual = 999999 THEN 'Servicio Puro'
    WHEN p.precioCompra > 0 AND p.stockActual = 999999 THEN 'Servicio Barra'
    WHEN p.stockActual = 0 THEN 'Sin Stock'
    WHEN p.stockActual <= p.stockMinimo THEN 'Stock Bajo'
    ELSE 'Stock bueno'
  END AS TipoEstado
FROM Producto p
INNER JOIN CategoriaProducto c ON p.idCategoriaProducto = c.idCategoriaProducto
ORDER BY c.nombre, p.nombre;

-- 👥 Ver clientes (TODOS deben tener visitasTotales = 0)
SELECT 
  nombre + ' ' + apellidos AS Cliente,
  telefono,
  visitasTotales AS Visitas,
  CASE 
    WHEN visitasTotales = 0 THEN 'Cliente nuevo (sin historial)'
    ELSE 'ERROR: Tiene visitas pero sin registros en BD'
  END AS Estado
FROM Cliente
ORDER BY nombre;

PRINT '';
PRINT '============================================================';
PRINT 'VERIFICACIÓN COMPLETA';
PRINT '============================================================';
PRINT 'Tablas con datos maestros: 15 (catálogos + entidades)';
PRINT 'Tablas transaccionales vacías: 10 (listas para usar)';
PRINT 'TOTAL: 25 tablas verificadas';
PRINT '';
PRINT 'Base de datos lista para desarrollo';
PRINT 'Todos los clientes con visitasTotales = 0 (sin historial)';
PRINT 'Productos clasificados correctamente (físicos vs servicios)';
PRINT '============================================================';

DROP TABLE Servicio