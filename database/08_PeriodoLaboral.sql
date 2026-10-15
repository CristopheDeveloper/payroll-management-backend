SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE PeriodoLaboral (
    IdPeriodoLaboral INT IDENTITY(1,1) NOT NULL,
    IdEmpleado       INT               NOT NULL,
    FechaIngreso     DATE              NOT NULL,
    FechaBaja        DATE              NULL,
    TipoBaja         NVARCHAR(20)      NULL,
    MotivoBaja       NVARCHAR(200)     NULL,
    UsuarioIng       NVARCHAR(50)      NOT NULL,
    FechaIng         DATETIME2         NOT NULL CONSTRAINT DF_PeriodoLaboral_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct       NVARCHAR(50)      NULL,
    FechaAct         DATETIME2         NULL,
    CONSTRAINT PK_PeriodoLaboral PRIMARY KEY (IdPeriodoLaboral),
    CONSTRAINT FK_PeriodoLaboral_Empleado FOREIGN KEY (IdEmpleado) REFERENCES Empleado (IdEmpleado),
    CONSTRAINT CK_PeriodoLaboral_Fechas CHECK (FechaBaja IS NULL OR FechaBaja >= FechaIngreso),
    CONSTRAINT CK_PeriodoLaboral_TipoBaja CHECK (TipoBaja IN ('RENUNCIA', 'DESPIDO', 'FIN_CONTRATO', 'OTRO')),
    CONSTRAINT CK_PeriodoLaboral_Baja CHECK ((FechaBaja IS NULL AND TipoBaja IS NULL AND MotivoBaja IS NULL) OR (FechaBaja IS NOT NULL AND TipoBaja IS NOT NULL))
);
GO

CREATE UNIQUE INDEX UX_PeriodoLaboral_Abierto ON PeriodoLaboral (IdEmpleado, FechaBaja) WHERE FechaBaja IS NULL;
GO
