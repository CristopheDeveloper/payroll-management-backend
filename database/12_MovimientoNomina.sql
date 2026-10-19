SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE MovimientoNomina (
    IdMovimiento INT IDENTITY(1,1) NOT NULL,
    IdNomina     INT               NOT NULL,
    IdEmpleado   INT               NOT NULL,
    IdRubro      INT               NOT NULL,
    Descripcion  NVARCHAR(200)     NULL,
    Monto        DECIMAL(10,2)     NOT NULL,
    UsuarioIng   NVARCHAR(50)      NOT NULL,
    FechaIng     DATETIME2         NOT NULL CONSTRAINT DF_MovimientoNomina_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct   NVARCHAR(50)      NULL,
    FechaAct     DATETIME2         NULL,
    CONSTRAINT PK_MovimientoNomina PRIMARY KEY (IdMovimiento),
    CONSTRAINT FK_MovimientoNomina_Nomina FOREIGN KEY (IdNomina) REFERENCES Nomina (IdNomina),
    CONSTRAINT FK_MovimientoNomina_Empleado FOREIGN KEY (IdEmpleado) REFERENCES Empleado (IdEmpleado),
    CONSTRAINT FK_MovimientoNomina_Rubro FOREIGN KEY (IdRubro) REFERENCES Rubro (IdRubro),
    CONSTRAINT CK_MovimientoNomina_Monto CHECK (Monto > 0)
);
GO

CREATE INDEX IX_MovimientoNomina_Nomina_Empleado ON MovimientoNomina (IdNomina, IdEmpleado);
GO
