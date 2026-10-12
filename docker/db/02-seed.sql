-- Development seed: one company and one ROOT user so the app can be used right after `docker compose up`.
-- Login: company 1, account "root", password "root". Change it after the first sign-in.
USE [HelpDeskDB]
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[EMPRESA])
BEGIN
    INSERT INTO [dbo].[EMPRESA] ([razon_social], [secuenciaticket], [habilitado], [limit])
    VALUES (N'Demo Company', N'0', 1, 5);

    INSERT INTO [dbo].[USUARIO] ([nombre], [cuenta_usuario], [contrasena], [acceso], [idEmpresa], [habilitado])
    VALUES (N'Root', N'root', N'root', N'ROOT', SCOPE_IDENTITY(), 1);
END
GO
