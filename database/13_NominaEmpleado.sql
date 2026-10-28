SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE NominaEmpleado (
    IdNominaEmpleado    INT IDENTITY(1,1) NOT NULL,
    IdNomina            INT               NOT NULL,
    IdEmpleado          INT               NOT NULL,
    IdPuesto            INT               NOT NULL,
    IdDepartamento      INT               NOT NULL,
    Salario             DECIMAL(10,2)     NOT NULL,
    RetencionIsr        DECIMAL(10,2)     NOT NULL,
    BonificacionIncentivo DECIMAL(10,2)   NOT NULL,
    TipoPago            NVARCHAR(15)      NOT NULL,
    Banco               NVARCHAR(50)      NULL,
    CuentaBancaria      NVARCHAR(30)      NULL,
    DiasPeriodo         INT               NOT NULL,
    DiasFalta           INT               NOT NULL,
    DiasSuspension      INT               NOT NULL,
    DiasPagarSugeridos  INT               NOT NULL,
    DiasPagar           INT               NOT NULL,
    HorasExtraSugeridas DECIMAL(6,2)      NOT NULL,
    HorasExtra          DECIMAL(6,2)      NOT NULL,
    Isr                 DECIMAL(10,2)     NOT NULL,
    DatosAjustados      BIT               NOT NULL CONSTRAINT DF_NominaEmpleado_DatosAjustados DEFAULT (0),
    TotalIngresos       DECIMAL(12,2)     NOT NULL CONSTRAINT DF_NominaEmpleado_TotalIngresos DEFAULT (0),
    TotalDescuentos     DECIMAL(12,2)     NOT NULL CONSTRAINT DF_NominaEmpleado_TotalDescuentos DEFAULT (0),
    Liquido             DECIMAL(12,2)     NOT NULL CONSTRAINT DF_NominaEmpleado_Liquido DEFAULT (0),
    TotalPatronal       DECIMAL(12,2)     NOT NULL CONSTRAINT DF_NominaEmpleado_TotalPatronal DEFAULT (0),
    UsuarioIng          NVARCHAR(50)      NOT NULL,
    FechaIng            DATETIME2         NOT NULL CONSTRAINT DF_NominaEmpleado_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct          NVARCHAR(50)      NULL,
    FechaAct            DATETIME2         NULL,
    CONSTRAINT PK_NominaEmpleado PRIMARY KEY (IdNominaEmpleado),
    CONSTRAINT UQ_NominaEmpleado_Nomina_Empleado UNIQUE (IdNomina, IdEmpleado),
    CONSTRAINT FK_NominaEmpleado_Nomina FOREIGN KEY (IdNomina) REFERENCES Nomina (IdNomina) ON DELETE CASCADE,
    CONSTRAINT FK_NominaEmpleado_Empleado FOREIGN KEY (IdEmpleado) REFERENCES Empleado (IdEmpleado),
    CONSTRAINT FK_NominaEmpleado_Puesto FOREIGN KEY (IdPuesto) REFERENCES Puesto (IdPuesto),
    CONSTRAINT FK_NominaEmpleado_Departamento FOREIGN KEY (IdDepartamento) REFERENCES Departamento (IdDepartamento),
    CONSTRAINT CK_NominaEmpleado_TipoPago CHECK (TipoPago IN ('EFECTIVO', 'CHEQUE', 'TRANSFERENCIA')),
    CONSTRAINT CK_NominaEmpleado_Montos CHECK (Salario > 0 AND RetencionIsr >= 0 AND BonificacionIncentivo >= 0 AND Isr >= 0),
    CONSTRAINT CK_NominaEmpleado_DiasPeriodo CHECK (DiasPeriodo BETWEEN 0 AND 366),
    CONSTRAINT CK_NominaEmpleado_Dias CHECK (
        DiasFalta >= 0 AND DiasSuspension >= 0
        AND DiasPagarSugeridos BETWEEN 0 AND DiasPeriodo
        AND DiasPagar BETWEEN 0 AND DiasPeriodo),
    CONSTRAINT CK_NominaEmpleado_Horas CHECK (HorasExtraSugeridas BETWEEN 0 AND 999.99 AND HorasExtra BETWEEN 0 AND 999.99)
);
GO
