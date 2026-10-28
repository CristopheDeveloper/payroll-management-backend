SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE NominaValor (
    IdNominaValor    INT IDENTITY(1,1) NOT NULL,
    IdNominaEmpleado INT               NOT NULL,
    IdRubro          INT               NOT NULL,
    Monto            DECIMAL(10,2)     NOT NULL,
    Descripcion      NVARCHAR(200)     NULL,
    UsuarioIng       NVARCHAR(50)      NOT NULL,
    FechaIng         DATETIME2         NOT NULL CONSTRAINT DF_NominaValor_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct       NVARCHAR(50)      NULL,
    FechaAct         DATETIME2         NULL,
    CONSTRAINT PK_NominaValor PRIMARY KEY (IdNominaValor),
    CONSTRAINT UQ_NominaValor_Empleado_Rubro UNIQUE (IdNominaEmpleado, IdRubro),
    CONSTRAINT FK_NominaValor_NominaEmpleado FOREIGN KEY (IdNominaEmpleado) REFERENCES NominaEmpleado (IdNominaEmpleado) ON DELETE CASCADE,
    CONSTRAINT FK_NominaValor_Rubro FOREIGN KEY (IdRubro) REFERENCES Rubro (IdRubro),
    CONSTRAINT CK_NominaValor_Monto CHECK (Monto > 0)
);
GO
