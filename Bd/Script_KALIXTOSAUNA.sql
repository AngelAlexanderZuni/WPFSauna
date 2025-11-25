
-- Crear la base de datos
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'KALIXTOSAUNA')
BEGIN
    CREATE DATABASE KALIXTOSAUNA;
END
GO

USE KALIXTOSAUNA;
GO

-- Tabla: Rol
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rol')
BEGIN
    CREATE TABLE Rol (
        idRol INT PRIMARY KEY IDENTITY(1,1),
        nombre VARCHAR(100) NOT NULL
    );
END
GO

-- Tabla: Usuario
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuario')
BEGIN
    CREATE TABLE Usuario (
        idUsuario INT PRIMARY KEY IDENTITY(1,1),
        nombreUsuario VARCHAR(100) NOT NULL UNIQUE,
        contraseniaHash VARCHAR(255) NOT NULL,
        correo VARCHAR(150) NOT NULL,
        fechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        activo BIT NOT NULL DEFAULT 1,
        idRol INT NOT NULL,
        CONSTRAINT FK_Usuario_Rol FOREIGN KEY (idRol) REFERENCES Rol(idRol)
    );
END
GO

-- Tabla: CategoriaProducto
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CategoriaProducto')
BEGIN
    CREATE TABLE CategoriaProducto (
        idCategoriaProducto INT PRIMARY KEY IDENTITY(1,1),
        nombre VARCHAR(100) NOT NULL,
        tipo VARCHAR(50) NOT NULL
    );
END
GO

-- Tabla: Producto
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Producto')
BEGIN
    CREATE TABLE Producto (
        idProducto INT PRIMARY KEY IDENTITY(1,1),
        codigo VARCHAR(50) NOT NULL UNIQUE,
        nombre VARCHAR(150) NOT NULL,
        descripcion VARCHAR(500),
        precioCompra DECIMAL(18,2) NOT NULL,
        precioVenta DECIMAL(18,2) NOT NULL,
        stockActual INT NOT NULL DEFAULT 0,
        stockMinimo INT NOT NULL DEFAULT 0,
        activo BIT NOT NULL DEFAULT 1,
        idCategoriaProducto INT NOT NULL,
        CONSTRAINT FK_Producto_CategoriaProducto FOREIGN KEY (idCategoriaProducto) REFERENCES CategoriaProducto(idCategoriaProducto)
    );
END
GO

-- Tabla: TipoMovimiento
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TipoMovimiento')
BEGIN
    CREATE TABLE TipoMovimiento (
        idTipoMovimiento INT PRIMARY KEY IDENTITY(1,1),
        nombre VARCHAR(100) NOT NULL,
        tipo CHAR(1) NOT NULL CHECK (tipo IN ('E', 'S')) -- E=Entrada, S=Salida
    );
END
GO

-- Tabla: MovimientoInventario
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MovimientoInventario')
BEGIN
    CREATE TABLE MovimientoInventario (
        idMovimiento INT PRIMARY KEY IDENTITY(1,1),
        cantidad INT NOT NULL,
        costoUnitario DECIMAL(18,2) NOT NULL,
        costoTotal DECIMAL(18,2) NOT NULL,
        fecha DATETIME NOT NULL DEFAULT GETDATE(),
        observaciones VARCHAR(500),
        idTipoMovimiento INT NOT NULL,
        idProducto INT NOT NULL,
        idUsuario INT NOT NULL,
        CONSTRAINT FK_MovimientoInventario_TipoMovimiento FOREIGN KEY (idTipoMovimiento) REFERENCES TipoMovimiento(idTipoMovimiento),
        CONSTRAINT FK_MovimientoInventario_Producto FOREIGN KEY (idProducto) REFERENCES Producto(idProducto),
        CONSTRAINT FK_MovimientoInventario_Usuario FOREIGN KEY (idUsuario) REFERENCES Usuario(idUsuario)
    );
END
GO

-- Tabla: ProgramaFidelizacion
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProgramaFidelizacion')
BEGIN
    CREATE TABLE ProgramaFidelizacion (
        idPrograma INT PRIMARY KEY IDENTITY(1,1),
        visitasParaDescuento INT NOT NULL,
        porcentajeDescuento DECIMAL(5,2) NOT NULL,
        descuentoCumpleanos BIT NOT NULL DEFAULT 0,
        montoDescuentoCumpleanos DECIMAL(18,2) NOT NULL DEFAULT 0
    );
END
GO

-- Tabla: Cliente
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cliente')
BEGIN
    CREATE TABLE Cliente (
        idCliente INT PRIMARY KEY IDENTITY(1,1),
        nombre VARCHAR(100) NOT NULL,
        apellidos VARCHAR(100) NOT NULL,
        numeroDocumento VARCHAR(20) NOT NULL UNIQUE,
        telefono VARCHAR(20),
        correo VARCHAR(150),
        direccion VARCHAR(250),
        fechaNacimiento DATE,
        fechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
        visitasTotales INT NOT NULL DEFAULT 0,
        activo BIT NOT NULL DEFAULT 1,
        idPrograma INT,
        CONSTRAINT FK_Cliente_ProgramaFidelizacion FOREIGN KEY (idPrograma) REFERENCES ProgramaFidelizacion(idPrograma)
    );
END
GO

-- Tabla: EstadoCuenta
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EstadoCuenta')
BEGIN
    CREATE TABLE EstadoCuenta (
        idEstadoCuenta INT PRIMARY KEY IDENTITY(1,1),
        nombre VARCHAR(50) NOT NULL
    );
END
GO

-- Tabla: Cuenta
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cuenta')
BEGIN
    CREATE TABLE Cuenta (
        idCuenta INT PRIMARY KEY IDENTITY(1,1),
        fechaHoraIngreso DATETIME NOT NULL DEFAULT GETDATE(),
        fechaHoraSalida DATETIME,
        horaEntrada DECIMAL(18,2) NOT NULL DEFAULT 0,
        subtotalConsumos DECIMAL(18,2) NOT NULL DEFAULT 0,
        descuentos DECIMAL(18,2) NOT NULL DEFAULT 0,
        total DECIMAL(18,2) NOT NULL DEFAULT 0,
        montoPagado DECIMAL(18,2) NOT NULL DEFAULT 0,
        saldo DECIMAL(18,2) NOT NULL DEFAULT 0,
        idEstadoCuenta INT NOT NULL,
        idCliente INT NOT NULL,
        idUsuarioCreador INT NOT NULL,
        CONSTRAINT FK_Cuenta_EstadoCuenta FOREIGN KEY (idEstadoCuenta) REFERENCES EstadoCuenta(idEstadoCuenta),
        CONSTRAINT FK_Cuenta_Cliente FOREIGN KEY (idCliente) REFERENCES Cliente(idCliente),
        CONSTRAINT FK_Cuenta_Usuario FOREIGN KEY (idUsuarioCreador) REFERENCES Usuario(idUsuario)
    );
END
GO

-- Tabla: DetalleConsumo
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DetalleConsumo')
BEGIN
    CREATE TABLE DetalleConsumo (
        idDetalle INT PRIMARY KEY IDENTITY(1,1),
        cantidad INT NOT NULL,
        precioUnitario DECIMAL(18,2) NOT NULL,
        subtotal DECIMAL(18,2) NOT NULL,
        idCuenta INT NOT NULL,
        idProducto INT NOT NULL,
        CONSTRAINT FK_DetalleConsumo_Cuenta FOREIGN KEY (idCuenta) REFERENCES Cuenta(idCuenta),
        CONSTRAINT FK_DetalleConsumo_Producto FOREIGN KEY (idProducto) REFERENCES Producto(idProducto)
    );
END
GO

-- Tabla: TipoEgreso
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TipoEgreso')
BEGIN
    CREATE TABLE TipoEgreso (
        idTipoEgreso INT PRIMARY KEY IDENTITY(1,1),
        nombre VARCHAR(100) NOT NULL
    );
END
GO

-- Tabla: Egreso
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Egreso')
BEGIN
    CREATE TABLE Egreso (
        idEgreso INT PRIMARY KEY IDENTITY(1,1),
        concepto VARCHAR(250) NOT NULL,
        fecha DATETIME NOT NULL DEFAULT GETDATE(),
        monto DECIMAL(18,2) NOT NULL,
        recurrente BIT NOT NULL DEFAULT 0,
        comprobanteRuta VARCHAR(500),
        idTipoEgreso INT NOT NULL,
        idUsuario INT NOT NULL,
        CONSTRAINT FK_Egreso_TipoEgreso FOREIGN KEY (idTipoEgreso) REFERENCES TipoEgreso(idTipoEgreso),
        CONSTRAINT FK_Egreso_Usuario FOREIGN KEY (idUsuario) REFERENCES Usuario(idUsuario)
    );
END
GO

-- Tabla: MetodoPago
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MetodoPago')
BEGIN
    CREATE TABLE MetodoPago (
        idMetodoPago INT PRIMARY KEY IDENTITY(1,1),
        nombre VARCHAR(50) NOT NULL
    );
END
GO

-- Tabla: Pago
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pago')
BEGIN
    CREATE TABLE Pago (
        idPago INT PRIMARY KEY IDENTITY(1,1),
        fechaHora DATETIME NOT NULL DEFAULT GETDATE(),
        monto DECIMAL(18,2) NOT NULL,
        numeroReferencia VARCHAR(100),
        idMetodoPago INT NOT NULL,
        idCuenta INT NOT NULL,
        CONSTRAINT FK_Pago_MetodoPago FOREIGN KEY (idMetodoPago) REFERENCES MetodoPago(idMetodoPago),
        CONSTRAINT FK_Pago_Cuenta FOREIGN KEY (idCuenta) REFERENCES Cuenta(idCuenta)
    );
END
GO

-- Tabla: TipoComprobante
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TipoComprobante')
BEGIN
    CREATE TABLE TipoComprobante (
        idTipoComprobante INT PRIMARY KEY IDENTITY(1,1),
        nombre VARCHAR(50) NOT NULL
    );
END
GO

-- Tabla: Comprobante
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Comprobante')
BEGIN
    CREATE TABLE Comprobante (
        idComprobante INT PRIMARY KEY IDENTITY(1,1),
        serie VARCHAR(10) NOT NULL,
        numero VARCHAR(20) NOT NULL,
        fechaEmision DATETIME NOT NULL DEFAULT GETDATE(),
        subtotal DECIMAL(18,2) NOT NULL,
        igv DECIMAL(18,2) NOT NULL,
        total DECIMAL(18,2) NOT NULL,
        idTipoComprobante INT NOT NULL,
        idCuenta INT NOT NULL,
        CONSTRAINT FK_Comprobante_TipoComprobante FOREIGN KEY (idTipoComprobante) REFERENCES TipoComprobante(idTipoComprobante),
        CONSTRAINT FK_Comprobante_Cuenta FOREIGN KEY (idCuenta) REFERENCES Cuenta(idCuenta),
        CONSTRAINT UQ_Comprobante_SerieNumero UNIQUE (serie, numero, idTipoComprobante)
    );
END
GO

-- Insertar datos iniciales
INSERT INTO Usuario(nombreUsuario,)
-- Roles
INSERT INTO Rol (nombre) VALUES 
('Administrador'),
('Cajero');
GO

-- Estados de Cuenta
INSERT INTO EstadoCuenta (nombre) VALUES 
('Abierta'),
('Cerrada'),
('Cancelada');
GO

select * from EstadoCuenta

-- Tipos de Movimiento
INSERT INTO TipoMovimiento (nombre, tipo) VALUES 
('Compra', 'E'),
('Venta', 'S'),
('Ajuste Entrada', 'E'),
('Ajuste Salida', 'S'),
('Merma', 'S');
GO

-- Métodos de Pago
INSERT INTO MetodoPago (nombre) VALUES 
('Efectivo'),
('Tarjeta'),
('Yape'),
('Plin'),
('Transferencia');
GO

-- Tipos de Comprobante
INSERT INTO TipoComprobante (nombre) VALUES 
('Boleta'),
('Factura');
GO

-- Tipos de Egreso
INSERT INTO TipoEgreso (nombre) VALUES 
('Servicios'),
('Alquiler'),
('Mantenimiento'),
('Sueldos'),
('Otros');
GO

-- Programa de Fidelización por defecto
INSERT INTO ProgramaFidelizacion (visitasParaDescuento, porcentajeDescuento, descuentoCumpleanos, montoDescuentoCumpleanos)
VALUES (10, 10.00, 1, 15.00);
GO

PRINT 'Base de datos KALIXTOSAUNA creada exitosamente con todas las tablas y datos iniciales.';
GO