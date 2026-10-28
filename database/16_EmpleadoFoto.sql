SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Una fotografía de perfil vigente por empleado. El archivo vive en el servidor SFTP;
-- esta tabla conserva la relación y la URL pública utilizada por el frontend.
CREATE TABLE EmpleadoFoto (
    IdEmpleado     INT             NOT NULL,
    Url            NVARCHAR(2048)  NOT NULL,
    RutaSftp       NVARCHAR(500)   NOT NULL,
    NombreArchivo  NVARCHAR(255)   NOT NULL,
    TipoContenido  NVARCHAR(100)   NOT NULL,
    TamanoBytes    BIGINT          NOT NULL,
    UsuarioIng     NVARCHAR(50)    NOT NULL,
    FechaIng       DATETIME2       NOT NULL CONSTRAINT DF_EmpleadoFoto_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct     NVARCHAR(50)    NULL,
    FechaAct       DATETIME2       NULL,
    CONSTRAINT PK_EmpleadoFoto PRIMARY KEY (IdEmpleado),
    CONSTRAINT FK_EmpleadoFoto_Empleado FOREIGN KEY (IdEmpleado) REFERENCES Empleado (IdEmpleado),
    CONSTRAINT CK_EmpleadoFoto_Tamano CHECK (TamanoBytes > 0)
);
GO
