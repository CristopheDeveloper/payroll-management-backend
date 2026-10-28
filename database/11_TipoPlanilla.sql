SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE TipoPlanilla (
    IdTipoPlanilla INT IDENTITY(1,1) NOT NULL,
    Codigo         NVARCHAR(20)      NOT NULL,
    Nombre         NVARCHAR(100)     NOT NULL,
    Descripcion    NVARCHAR(200)     NULL,
    IncluyeIgss    BIT               NOT NULL CONSTRAINT DF_TipoPlanilla_IncluyeIgss DEFAULT (1),
    Activo         BIT               NOT NULL CONSTRAINT DF_TipoPlanilla_Activo DEFAULT (1),
    UsuarioIng     NVARCHAR(50)      NOT NULL,
    FechaIng       DATETIME2         NOT NULL CONSTRAINT DF_TipoPlanilla_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct     NVARCHAR(50)      NULL,
    FechaAct       DATETIME2         NULL,
    CONSTRAINT PK_TipoPlanilla PRIMARY KEY (IdTipoPlanilla),
    CONSTRAINT UQ_TipoPlanilla_Codigo UNIQUE (Codigo),
    CONSTRAINT UQ_TipoPlanilla_Nombre UNIQUE (Nombre),
    CONSTRAINT CK_TipoPlanilla_Codigo CHECK (Codigo IN ('MENSUAL', 'BONO14', 'AGUINALDO'))
);
GO

INSERT INTO TipoPlanilla (Codigo, Nombre, Descripcion, IncluyeIgss, Activo, UsuarioIng) VALUES
    ('MENSUAL',   N'Planilla mensual', N'Salario ordinario, horas extra y bonificación incentivo del mes', 1, 1, 'SISTEMA'),
    ('BONO14',    N'Bono 14',          N'Bonificación anual, Decreto 42-92 (del 1 de julio al 30 de junio)', 0, 0, 'SISTEMA'),
    ('AGUINALDO', N'Aguinaldo',        N'Aguinaldo, Decreto 76-78 (del 1 de diciembre al 30 de noviembre)', 0, 0, 'SISTEMA');
GO
