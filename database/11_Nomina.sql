SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE Nomina (
    IdNomina              INT IDENTITY(1,1) NOT NULL,
    Anio                  INT               NOT NULL,
    Mes                   INT               NOT NULL,
    FechaInicio           DATE              NOT NULL,
    FechaFin              DATE              NOT NULL,
    Estado                NVARCHAR(15)      NOT NULL CONSTRAINT DF_Nomina_Estado DEFAULT ('BORRADOR'),
    TotalIngresos         DECIMAL(12,2)     NOT NULL CONSTRAINT DF_Nomina_TotalIngresos DEFAULT (0),
    TotalDescuentos       DECIMAL(12,2)     NOT NULL CONSTRAINT DF_Nomina_TotalDescuentos DEFAULT (0),
    TotalLiquido          DECIMAL(12,2)     NOT NULL CONSTRAINT DF_Nomina_TotalLiquido DEFAULT (0),
    TotalPatronal         DECIMAL(12,2)     NOT NULL CONSTRAINT DF_Nomina_TotalPatronal DEFAULT (0),
    FechaCalculo          DATETIME2         NULL,
    IgssLaboral           DECIMAL(9,4)      NULL,
    IgssPatronal          DECIMAL(9,4)      NULL,
    Irtra                 DECIMAL(9,4)      NULL,
    Intecap               DECIMAL(9,4)      NULL,
    BonificacionIncentivo DECIMAL(10,2)     NULL,
    FactorHoraExtra       DECIMAL(9,4)      NULL,
    DiasMes               INT               NULL,
    HorasMes              INT               NULL,
    UsuarioCierre         NVARCHAR(50)      NULL,
    FechaCierre           DATETIME2         NULL,
    MotivoReapertura      NVARCHAR(200)     NULL,
    Version               ROWVERSION        NOT NULL,
    UsuarioIng            NVARCHAR(50)      NOT NULL,
    FechaIng              DATETIME2         NOT NULL CONSTRAINT DF_Nomina_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct            NVARCHAR(50)      NULL,
    FechaAct              DATETIME2         NULL,
    CONSTRAINT PK_Nomina PRIMARY KEY (IdNomina),
    CONSTRAINT UQ_Nomina_Anio_Mes UNIQUE (Anio, Mes),
    CONSTRAINT CK_Nomina_Mes CHECK (Mes BETWEEN 1 AND 12),
    CONSTRAINT CK_Nomina_Anio CHECK (Anio BETWEEN 2000 AND 2100),
    CONSTRAINT CK_Nomina_Fechas CHECK (FechaInicio = DATEFROMPARTS(Anio, Mes, 1) AND FechaFin = EOMONTH(FechaInicio)),
    CONSTRAINT CK_Nomina_Estado CHECK (Estado IN ('BORRADOR', 'CALCULADA', 'CERRADA')),
    CONSTRAINT CK_Nomina_Cierre CHECK (
        (Estado = 'CERRADA'  AND UsuarioCierre IS NOT NULL AND FechaCierre IS NOT NULL)
     OR (Estado <> 'CERRADA' AND UsuarioCierre IS NULL     AND FechaCierre IS NULL)),
    CONSTRAINT CK_Nomina_Parametros CHECK (
        FechaCalculo IS NULL
     OR (IgssLaboral IS NOT NULL AND IgssPatronal IS NOT NULL AND Irtra IS NOT NULL AND Intecap IS NOT NULL
         AND BonificacionIncentivo IS NOT NULL AND FactorHoraExtra IS NOT NULL AND DiasMes IS NOT NULL AND HorasMes IS NOT NULL))
);
GO
