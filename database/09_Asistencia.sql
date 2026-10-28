SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE Asistencia (
    IdAsistencia INT IDENTITY(1,1) NOT NULL,
    IdEmpleado   INT               NOT NULL,
    Fecha        DATE              NOT NULL,
    Estado       NVARCHAR(20)      NOT NULL CONSTRAINT DF_Asistencia_Estado DEFAULT ('PRESENTE'),
    HorasExtra   DECIMAL(5,2)      NOT NULL CONSTRAINT DF_Asistencia_HorasExtra DEFAULT (0),
    Observacion  NVARCHAR(200)     NULL,
    UsuarioIng   NVARCHAR(50)      NOT NULL,
    FechaIng     DATETIME2         NOT NULL CONSTRAINT DF_Asistencia_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct   NVARCHAR(50)      NULL,
    FechaAct     DATETIME2         NULL,
    CONSTRAINT PK_Asistencia PRIMARY KEY (IdAsistencia),
    CONSTRAINT UQ_Asistencia_Empleado_Fecha UNIQUE (IdEmpleado, Fecha),
    CONSTRAINT FK_Asistencia_Empleado FOREIGN KEY (IdEmpleado) REFERENCES Empleado (IdEmpleado),
    CONSTRAINT CK_Asistencia_Estado CHECK (Estado IN ('PRESENTE', 'FALTA', 'PERMISO', 'SUSPENSION_IGSS')),
    CONSTRAINT CK_Asistencia_HorasExtra CHECK (HorasExtra >= 0 AND (Estado = 'PRESENTE' OR HorasExtra = 0))
);
GO

CREATE INDEX IX_Asistencia_Fecha ON Asistencia (Fecha);
GO
