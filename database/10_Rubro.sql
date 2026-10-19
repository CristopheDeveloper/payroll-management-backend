SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE Rubro (
    IdRubro        INT IDENTITY(1,1) NOT NULL,
    Nombre         NVARCHAR(100)     NOT NULL,
    Tipo           NVARCHAR(10)      NOT NULL,
    AfectoIgss     BIT               NOT NULL CONSTRAINT DF_Rubro_AfectoIgss DEFAULT (0),
    Descripcion    NVARCHAR(200)     NULL,
    CuentaContable NVARCHAR(20)      NULL,
    Activo         BIT               NOT NULL CONSTRAINT DF_Rubro_Activo DEFAULT (1),
    UsuarioIng     NVARCHAR(50)      NOT NULL,
    FechaIng       DATETIME2         NOT NULL CONSTRAINT DF_Rubro_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct     NVARCHAR(50)      NULL,
    FechaAct       DATETIME2         NULL,
    CONSTRAINT PK_Rubro PRIMARY KEY (IdRubro),
    CONSTRAINT UQ_Rubro_Nombre UNIQUE (Nombre),
    CONSTRAINT CK_Rubro_Tipo CHECK (Tipo IN ('INGRESO', 'DESCUENTO')),
    CONSTRAINT CK_Rubro_AfectoIgss CHECK (Tipo = 'INGRESO' OR AfectoIgss = 0)
);
GO

INSERT INTO Rubro (Nombre, Tipo, AfectoIgss, UsuarioIng) VALUES
    (N'Comisión por ventas',                         'INGRESO',   1, 'SISTEMA'),
    (N'Bono por desempeño',                          'INGRESO',   1, 'SISTEMA'),
    (N'Ajuste salarial de planilla anterior',        'INGRESO',   1, 'SISTEMA'),
    (N'Ajuste de bonificación de planilla anterior', 'INGRESO',   0, 'SISTEMA'),
    (N'Otros ingresos afectos',                      'INGRESO',   1, 'SISTEMA'),
    (N'Otros ingresos no afectos',                   'INGRESO',   0, 'SISTEMA'),
    (N'Anticipo de salario',                         'DESCUENTO', 0, 'SISTEMA'),
    (N'Préstamo',                                    'DESCUENTO', 0, 'SISTEMA'),
    (N'Descuento por daños',                         'DESCUENTO', 0, 'SISTEMA'),
    (N'Otros descuentos',                            'DESCUENTO', 0, 'SISTEMA');
GO
