SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE NominaRubro (
    IdNominaRubro    INT IDENTITY(1,1) NOT NULL,
    IdNominaEmpleado INT               NOT NULL,
    IdRubro          INT               NOT NULL,
    Orden            INT               NOT NULL,
    Base             DECIMAL(12,2)     NULL,
    Monto            DECIMAL(12,2)     NOT NULL,
    Descripcion      NVARCHAR(200)     NULL,
    UsuarioIng       NVARCHAR(50)      NOT NULL,
    FechaIng         DATETIME2         NOT NULL CONSTRAINT DF_NominaRubro_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct       NVARCHAR(50)      NULL,
    FechaAct         DATETIME2         NULL,
    CONSTRAINT PK_NominaRubro PRIMARY KEY (IdNominaRubro),
    CONSTRAINT UQ_NominaRubro_Empleado_Rubro UNIQUE (IdNominaEmpleado, IdRubro),
    CONSTRAINT FK_NominaRubro_NominaEmpleado FOREIGN KEY (IdNominaEmpleado) REFERENCES NominaEmpleado (IdNominaEmpleado) ON DELETE CASCADE,
    CONSTRAINT FK_NominaRubro_Rubro FOREIGN KEY (IdRubro) REFERENCES Rubro (IdRubro),
    CONSTRAINT CK_NominaRubro_Monto CHECK (Monto >= 0),
    CONSTRAINT CK_NominaRubro_Base CHECK (Base IS NULL OR Base >= 0)
);
GO

CREATE INDEX IX_NominaRubro_NominaEmpleado_Orden ON NominaRubro (IdNominaEmpleado, Orden);
GO
