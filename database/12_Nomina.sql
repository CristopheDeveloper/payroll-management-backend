SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE Nomina (
    IdNomina              INT IDENTITY(1,1) NOT NULL,
    IdTipoPlanilla        INT               NOT NULL,
    Codigo                NVARCHAR(20)      NOT NULL,
    Descripcion           NVARCHAR(200)     NOT NULL,
    Anio                  INT               NOT NULL,
    Mes                   INT               NOT NULL,
    FechaInicio           DATE              NOT NULL,
    FechaFin              DATE              NOT NULL,
    FechaPago             DATE              NOT NULL,
    Comentario            NVARCHAR(500)     NULL,
    Estado                NVARCHAR(15)      NOT NULL CONSTRAINT DF_Nomina_Estado DEFAULT ('CREADA'),
    TotalIngresos         DECIMAL(12,2)     NOT NULL CONSTRAINT DF_Nomina_TotalIngresos DEFAULT (0),
    TotalDescuentos       DECIMAL(12,2)     NOT NULL CONSTRAINT DF_Nomina_TotalDescuentos DEFAULT (0),
    TotalLiquido          DECIMAL(12,2)     NOT NULL CONSTRAINT DF_Nomina_TotalLiquido DEFAULT (0),
    TotalPatronal         DECIMAL(12,2)     NOT NULL CONSTRAINT DF_Nomina_TotalPatronal DEFAULT (0),
    FechaPreparacion      DATETIME2         NULL,
    FechaGeneracion       DATETIME2         NULL,
    IgssLaboral           DECIMAL(9,4)      NULL,
    IgssPatronal          DECIMAL(9,4)      NULL,
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
    CONSTRAINT FK_Nomina_TipoPlanilla FOREIGN KEY (IdTipoPlanilla) REFERENCES TipoPlanilla (IdTipoPlanilla),
    CONSTRAINT UQ_Nomina_Codigo UNIQUE (Codigo),
    CONSTRAINT CK_Nomina_Mes CHECK (Mes BETWEEN 1 AND 12),
    CONSTRAINT CK_Nomina_Anio CHECK (Anio BETWEEN 2000 AND 2100),
    CONSTRAINT CK_Nomina_Periodo CHECK (FechaFin >= FechaInicio AND DATEDIFF(DAY, FechaInicio, FechaFin) < 366),
    CONSTRAINT CK_Nomina_Estado CHECK (Estado IN ('CREADA', 'PREPARADA', 'GENERADA', 'CERRADA')),
    CONSTRAINT CK_Nomina_Cierre CHECK (
        (Estado = 'CERRADA'  AND UsuarioCierre IS NOT NULL AND FechaCierre IS NOT NULL)
     OR (Estado <> 'CERRADA' AND UsuarioCierre IS NULL     AND FechaCierre IS NULL)),
    CONSTRAINT CK_Nomina_Preparacion CHECK (
        (Estado = 'CREADA' AND FechaPreparacion IS NULL)
     OR (Estado <> 'CREADA' AND FechaPreparacion IS NOT NULL
         AND IgssLaboral IS NOT NULL AND IgssPatronal IS NOT NULL
         AND BonificacionIncentivo IS NOT NULL AND FactorHoraExtra IS NOT NULL AND DiasMes IS NOT NULL AND HorasMes IS NOT NULL)),
    CONSTRAINT CK_Nomina_Generacion CHECK (
        (Estado IN ('CREADA', 'PREPARADA') AND FechaGeneracion IS NULL)
     OR (Estado IN ('GENERADA', 'CERRADA') AND FechaGeneracion IS NOT NULL))
);
GO

CREATE INDEX IX_Nomina_Tipo_Anio_Mes ON Nomina (IdTipoPlanilla, Anio, Mes);
GO
