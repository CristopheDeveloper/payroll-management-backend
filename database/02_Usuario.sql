-- Cuentas que pueden entrar al sistema. Cada usuario tiene exactamente un rol.
CREATE TABLE Usuario (
    IdUsuario      INT IDENTITY(1,1) NOT NULL,
    NombreUsuario  NVARCHAR(50)      NOT NULL,
    PasswordHash   NVARCHAR(255)     NOT NULL,
    NombreCompleto NVARCHAR(150)     NOT NULL,
    IdRol          INT               NOT NULL,
    UltimoAcceso   DATETIME2         NULL,
    Activo         BIT               NOT NULL CONSTRAINT DF_Usuario_Activo DEFAULT (1),
    UsuarioIng     NVARCHAR(50)      NOT NULL,
    FechaIng       DATETIME2         NOT NULL CONSTRAINT DF_Usuario_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct     NVARCHAR(50)      NULL,
    FechaAct       DATETIME2         NULL,
    CONSTRAINT PK_Usuario PRIMARY KEY (IdUsuario),
    CONSTRAINT UQ_Usuario_NombreUsuario UNIQUE (NombreUsuario),
    CONSTRAINT FK_Usuario_Rol FOREIGN KEY (IdRol) REFERENCES Rol (IdRol)
);
GO

INSERT INTO Usuario (NombreUsuario, PasswordHash, NombreCompleto, IdRol, UsuarioIng) VALUES
    ('admin', '$2b$11$5iv0VAxcoiWUF5VnQJXBIu5ug6bnJYcrOrWIYxDyPVVu75GHnxmB6', 'Administrador del Sistema', 1, 'SISTEMA');
GO
