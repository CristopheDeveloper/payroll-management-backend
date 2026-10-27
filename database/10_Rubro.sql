SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE Rubro (
    IdRubro        INT IDENTITY(1,1) NOT NULL,
    Codigo         NVARCHAR(30)      NOT NULL,
    Nombre         NVARCHAR(100)     NOT NULL,
    Tipo           NVARCHAR(10)      NOT NULL,
    Origen         NVARCHAR(10)      NOT NULL CONSTRAINT DF_Rubro_Origen DEFAULT ('MANUAL'),
    AfectoIgss     BIT               NOT NULL CONSTRAINT DF_Rubro_AfectoIgss DEFAULT (0),
    EsPrestacion   BIT               NOT NULL CONSTRAINT DF_Rubro_EsPrestacion DEFAULT (0),
    Orden          INT               NOT NULL,
    Descripcion    NVARCHAR(200)     NULL,
    CuentaContable NVARCHAR(20)      NULL,
    Activo         BIT               NOT NULL CONSTRAINT DF_Rubro_Activo DEFAULT (1),
    UsuarioIng     NVARCHAR(50)      NOT NULL,
    FechaIng       DATETIME2         NOT NULL CONSTRAINT DF_Rubro_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct     NVARCHAR(50)      NULL,
    FechaAct       DATETIME2         NULL,
    CONSTRAINT PK_Rubro PRIMARY KEY (IdRubro),
    CONSTRAINT UQ_Rubro_Codigo UNIQUE (Codigo),
    CONSTRAINT UQ_Rubro_Nombre UNIQUE (Nombre),
    CONSTRAINT CK_Rubro_Codigo CHECK (Codigo COLLATE Latin1_General_BIN NOT LIKE '%[^A-Z0-9_]%'),
    CONSTRAINT CK_Rubro_Tipo CHECK (Tipo IN ('INGRESO', 'DESCUENTO', 'PATRONAL')),
    CONSTRAINT CK_Rubro_Origen CHECK (Origen IN ('SISTEMA', 'MANUAL')),
    CONSTRAINT CK_Rubro_ManualTipo CHECK (Origen = 'SISTEMA' OR Tipo IN ('INGRESO', 'DESCUENTO')),
    CONSTRAINT CK_Rubro_AfectoIgss CHECK (Tipo = 'INGRESO' OR AfectoIgss = 0),
    CONSTRAINT CK_Rubro_EsPrestacion CHECK (Tipo = 'INGRESO' OR EsPrestacion = 0),
    CONSTRAINT CK_Rubro_Orden CHECK (Orden BETWEEN 1 AND 999)
);
GO

INSERT INTO Rubro (Codigo, Nombre, Tipo, Origen, AfectoIgss, EsPrestacion, Orden, UsuarioIng) VALUES
    ('SALARIO_ORDINARIO',      N'Salario ordinario',                           'INGRESO',   'SISTEMA', 1, 0, 10,  'SISTEMA'),
    ('HORAS_EXTRA',            N'Horas extra',                                 'INGRESO',   'SISTEMA', 1, 0, 20,  'SISTEMA'),
    ('BONIFICACION_INCENTIVO', N'Bonificación incentivo',                      'INGRESO',   'SISTEMA', 0, 0, 30,  'SISTEMA'),
    ('BONO_14',                N'Bono 14',                                     'INGRESO',   'SISTEMA', 0, 1, 40,  'SISTEMA'),
    ('AGUINALDO',              N'Aguinaldo',                                   'INGRESO',   'SISTEMA', 0, 1, 50,  'SISTEMA'),
    ('COMISION',               N'Comisión por ventas',                         'INGRESO',   'MANUAL',  1, 0, 100, 'SISTEMA'),
    ('BONO_DESEMPENO',         N'Bono por desempeño',                          'INGRESO',   'MANUAL',  1, 0, 110, 'SISTEMA'),
    ('AJUSTE_SALARIO',         N'Ajuste salarial de planilla anterior',        'INGRESO',   'MANUAL',  1, 0, 120, 'SISTEMA'),
    ('AJUSTE_BONIFICACION',    N'Ajuste de bonificación de planilla anterior', 'INGRESO',   'MANUAL',  0, 0, 130, 'SISTEMA'),
    ('OTROS_INGRESOS_AFECTOS', N'Otros ingresos afectos',                      'INGRESO',   'MANUAL',  1, 0, 140, 'SISTEMA'),
    ('OTROS_INGRESOS',         N'Otros ingresos no afectos',                   'INGRESO',   'MANUAL',  0, 0, 150, 'SISTEMA'),
    ('IGSS_LABORAL',           N'IGSS cuota laboral',                          'DESCUENTO', 'SISTEMA', 0, 0, 200, 'SISTEMA'),
    ('ISR',                    N'ISR retenido',                                'DESCUENTO', 'SISTEMA', 0, 0, 210, 'SISTEMA'),
    ('ANTICIPO',               N'Anticipo de salario',                         'DESCUENTO', 'MANUAL',  0, 0, 300, 'SISTEMA'),
    ('PRESTAMO',               N'Préstamo',                                    'DESCUENTO', 'MANUAL',  0, 0, 310, 'SISTEMA'),
    ('DANOS',                  N'Descuento por daños',                         'DESCUENTO', 'MANUAL',  0, 0, 320, 'SISTEMA'),
    ('OTROS_DESCUENTOS',       N'Otros descuentos',                            'DESCUENTO', 'MANUAL',  0, 0, 330, 'SISTEMA'),
    ('IGSS_PATRONAL',          N'IGSS cuota patronal',                         'PATRONAL',  'SISTEMA', 0, 0, 500, 'SISTEMA');
GO
