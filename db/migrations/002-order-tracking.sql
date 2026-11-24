-- Customer tracking: the table of the links, and the flag that says whether the customer sees an activity.
-- Run once on databases created before this change. It is safe to run again.
USE [HelpDeskDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SEGUIMIENTO]') AND name = N'visibleAlCliente')
BEGIN
    ALTER TABLE [dbo].[SEGUIMIENTO] ADD [visibleAlCliente] BIT NULL;
END
GO
IF OBJECT_ID(N'[dbo].[ENLACE_SEGUIMIENTO]') IS NULL
BEGIN
    CREATE TABLE [dbo].[ENLACE_SEGUIMIENTO](
        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [idEmpresa] INT NOT NULL,
        [idSolicitud] INT NOT NULL,
        [token] VARCHAR(64) NOT NULL,
        [creadoEn] DATETIME2 NOT NULL,
        [venceEn] DATETIME2 NULL,
        [revocadoEn] DATETIME2 NULL,
        [habilitado] BIT NULL
    );
    CREATE UNIQUE INDEX [UX_ENLACE_SEGUIMIENTO_token] ON [dbo].[ENLACE_SEGUIMIENTO]([token]);
    CREATE INDEX [IX_ENLACE_SEGUIMIENTO_idSolicitud] ON [dbo].[ENLACE_SEGUIMIENTO]([idSolicitud]);
END
GO
