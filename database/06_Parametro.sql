CREATE TABLE Parametro (
    IdParametro INT IDENTITY(1,1) NOT NULL,
    Clave       NVARCHAR(50)      NOT NULL,
    Valor       NVARCHAR(100)     NOT NULL,
    TipoDato    NVARCHAR(10)      NOT NULL,
    Descripcion NVARCHAR(200)     NOT NULL,
    UsuarioIng  NVARCHAR(50)      NOT NULL,
    FechaIng    DATETIME2         NOT NULL CONSTRAINT DF_Parametro_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct  NVARCHAR(50)      NULL,
    FechaAct    DATETIME2         NULL,
    CONSTRAINT PK_Parametro PRIMARY KEY (IdParametro),
    CONSTRAINT UQ_Parametro_Clave UNIQUE (Clave),
    CONSTRAINT CK_Parametro_TipoDato CHECK (TipoDato IN ('ENTERO', 'DECIMAL', 'TEXTO'))
);
GO

INSERT INTO Parametro (Clave, Valor, TipoDato, Descripcion, UsuarioIng) VALUES
    ('IGSS_LABORAL',           '4.83',                         'DECIMAL', N'Porcentaje de cuota laboral del IGSS (%)', 'SISTEMA'),
    ('IGSS_PATRONAL',          '10.67',                        'DECIMAL', N'Porcentaje de cuota patronal del IGSS (%)', 'SISTEMA'),
    ('BONIFICACION_INCENTIVO', '250.00',                       'DECIMAL', N'Bonificación incentivo mensual (Q)', 'SISTEMA'),
    ('FACTOR_HORA_EXTRA',      '1.5',                          'DECIMAL', N'Multiplicador del pago de horas extra', 'SISTEMA'),
    ('SALARIO_MINIMO',         '3816.90',                      'DECIMAL', N'Salario mínimo mensual no agrícola sin bonificación (Q), circunscripción 2, Acuerdo Gubernativo 256-2025', 'SISTEMA'),
    ('DIAS_MES',               '30',                           'ENTERO',  N'Días base del mes para calcular el valor del día', 'SISTEMA'),
    ('HORAS_MES',              '240',                          'ENTERO',  N'Horas base del mes para calcular el valor de la hora', 'SISTEMA'),
    ('NOMBRE_EMPRESA',         N'Distribuidora La Ceiba, S.A.', 'TEXTO',   N'Nombre de la empresa en boletas y reportes', 'SISTEMA'),
    ('NIT_EMPRESA',            '8475123-6',                    'TEXTO',   N'NIT de la empresa', 'SISTEMA'),
    ('DIRECCION_EMPRESA',      N'Cuilapa, Santa Rosa',          'TEXTO',   N'Dirección de la empresa', 'SISTEMA'),
    ('CUENTA_SUELDOS_POR_PAGAR',       '2101', 'TEXTO', N'Cuenta contable del líquido a pagar en la póliza de planilla', 'SISTEMA'),
    ('CUENTA_IGSS_PATRONAL_POR_PAGAR', '2103', 'TEXTO', N'Cuenta contable de la cuota patronal del IGSS por pagar', 'SISTEMA'),
    ('CUENTA_BANCOS',                  '1102', 'TEXTO', N'Cuenta contable del banco de la empresa para pagos por transferencia y cheque', 'SISTEMA'),
    ('CUENTA_CAJA',                    '1101', 'TEXTO', N'Cuenta contable de caja para pagos en efectivo', 'SISTEMA');
GO
