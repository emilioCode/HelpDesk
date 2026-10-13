-- The company's mail password is now stored encrypted, which takes about 115 characters.
-- Run once on databases created before this change. It is safe to run again.
USE [HelpDeskDB]
GO
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[EMPRESA]') AND name = N'contrasena' AND max_length <> -1 AND max_length < 500
)
BEGIN
    ALTER TABLE [dbo].[EMPRESA] ALTER COLUMN [contrasena] VARCHAR(500) NULL;
END
GO
