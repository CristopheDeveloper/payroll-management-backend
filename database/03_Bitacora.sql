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

CREATE INDEX IX_Bitacora_Tabla_IdRegistro ON Bitacora (Tabla, IdRegistro);
CREATE INDEX IX_Bitacora_Fecha ON Bitacora (Fecha);
GO
