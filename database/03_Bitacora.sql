-- Registro automático de cada inserción, modificación y eliminación en cualquier tabla.
-- La escribe el backend (RegistroBitacora) al guardar; es de solo inserción: nadie la modifica ni la borra.
-- No tiene campos de auditoría porque ella misma es la auditoría, ni llaves foráneas porque
-- registra cambios de cualquier tabla (se vincula por Tabla + IdRegistro).
CREATE TABLE Bitacora (
    IdBitacora        BIGINT IDENTITY(1,1) NOT NULL,
    Tabla             NVARCHAR(50)         NOT NULL,
    IdRegistro        INT                  NOT NULL,
    Accion            NVARCHAR(10)         NOT NULL,
    ValoresAnteriores NVARCHAR(MAX)        NULL,
    ValoresNuevos     NVARCHAR(MAX)        NULL,
    Usuario           NVARCHAR(50)         NOT NULL,
    Fecha             DATETIME2            NOT NULL CONSTRAINT DF_Bitacora_Fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Bitacora PRIMARY KEY (IdBitacora),
    CONSTRAINT CK_Bitacora_Accion CHECK (Accion IN ('INSERT', 'UPDATE', 'DELETE'))
);
GO

-- Historial de un registro (por ejemplo, los cambios de salario de un empleado)
CREATE INDEX IX_Bitacora_Tabla_IdRegistro ON Bitacora (Tabla, IdRegistro);
-- Consulta por rango de fechas
CREATE INDEX IX_Bitacora_Fecha ON Bitacora (Fecha);
GO
