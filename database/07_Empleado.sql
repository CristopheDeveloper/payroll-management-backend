SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE Empleado (
    IdEmpleado      INT IDENTITY(1,1) NOT NULL,
    CodigoEmpleado  AS (CAST('EMP-' + CASE WHEN IdEmpleado < 10000 THEN RIGHT('0000' + CAST(IdEmpleado AS VARCHAR(10)), 4) ELSE CAST(IdEmpleado AS VARCHAR(10)) END AS NVARCHAR(15))) PERSISTED,
    Nombres         NVARCHAR(100)     NOT NULL,
    Apellidos       NVARCHAR(100)     NOT NULL,
    Dpi             CHAR(13)          NOT NULL,
    Pasaporte       NVARCHAR(20)      NULL,
    Nit             NVARCHAR(15)      NULL,
    NumeroIgss      NVARCHAR(20)      NULL,
    FechaNacimiento DATE              NOT NULL,
    Genero          CHAR(1)           NOT NULL,
    Direccion       NVARCHAR(200)     NOT NULL,
    Telefono        NVARCHAR(15)      NULL,
    Correo          NVARCHAR(100)     NULL,
    TipoPago        NVARCHAR(15)      NOT NULL CONSTRAINT DF_Empleado_TipoPago DEFAULT ('TRANSFERENCIA'),
    Banco           NVARCHAR(50)      NULL,
    CuentaBancaria  NVARCHAR(30)      NULL,
    IdPuesto        INT               NOT NULL,
    IdDepartamento  INT               NOT NULL,
    Salario         DECIMAL(10,2)     NOT NULL,
    RetencionIsr    DECIMAL(10,2)     NOT NULL CONSTRAINT DF_Empleado_RetencionIsr DEFAULT (0),
    BonificacionIncentivo DECIMAL(10,2) NOT NULL,
    Activo          BIT               NOT NULL CONSTRAINT DF_Empleado_Activo DEFAULT (1),
    UsuarioIng      NVARCHAR(50)      NOT NULL,
    FechaIng        DATETIME2         NOT NULL CONSTRAINT DF_Empleado_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct      NVARCHAR(50)      NULL,
    FechaAct        DATETIME2         NULL,
    CONSTRAINT PK_Empleado PRIMARY KEY (IdEmpleado),
    CONSTRAINT UQ_Empleado_CodigoEmpleado UNIQUE (CodigoEmpleado),
    CONSTRAINT UQ_Empleado_Dpi UNIQUE (Dpi),
    CONSTRAINT FK_Empleado_Puesto FOREIGN KEY (IdPuesto) REFERENCES Puesto (IdPuesto),
    CONSTRAINT FK_Empleado_Departamento FOREIGN KEY (IdDepartamento) REFERENCES Departamento (IdDepartamento),
    CONSTRAINT CK_Empleado_Dpi CHECK (LEN(Dpi) = 13 AND Dpi NOT LIKE '%[^0123456789]%'),
    CONSTRAINT CK_Empleado_Genero CHECK (Genero IN ('M', 'F')),
    CONSTRAINT CK_Empleado_TipoPago CHECK (TipoPago IN ('EFECTIVO', 'CHEQUE', 'TRANSFERENCIA')),
    CONSTRAINT CK_Empleado_Salario CHECK (Salario > 0),
    CONSTRAINT CK_Empleado_RetencionIsr CHECK (RetencionIsr >= 0),
    CONSTRAINT CK_Empleado_BonificacionIncentivo CHECK (BonificacionIncentivo >= 0)
);
GO
