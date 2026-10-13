-- Tabla de roles del sistema
CREATE TABLE Rol (
    Id          INT IDENTITY(1,1) NOT NULL,
    Nombre      NVARCHAR(50)      NOT NULL,
    Descripcion NVARCHAR(200)     NULL,
    Activo      BIT               NOT NULL CONSTRAINT DF_Rol_Activo DEFAULT (1),
    CONSTRAINT PK_Rol PRIMARY KEY (Id),
    CONSTRAINT UQ_Rol_Nombre UNIQUE (Nombre)
);
GO
-- semilla inicial
INSERT INTO Rol (Nombre, Descripcion) VALUES
    ('Admin', 'Administrador del sistema'),
    ('RRHH',  'Recursos Humanos');
GO
