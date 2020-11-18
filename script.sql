USE [master]
GO
/****** Object:  Database [HelpDeskDB]    Script Date: 11/17/2020 00:37:36 ******/
CREATE DATABASE [HelpDeskDB] ON  PRIMARY 
( NAME = N'HelpDeskDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\HelpDeskDB.mdf' , SIZE = 12544KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'HelpDeskDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\HelpDeskDB_log.LDF' , SIZE = 13632KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [HelpDeskDB] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [HelpDeskDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [HelpDeskDB] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [HelpDeskDB] SET ANSI_NULLS OFF
GO
ALTER DATABASE [HelpDeskDB] SET ANSI_PADDING OFF
GO
ALTER DATABASE [HelpDeskDB] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [HelpDeskDB] SET ARITHABORT OFF
GO
ALTER DATABASE [HelpDeskDB] SET AUTO_CLOSE ON
GO
ALTER DATABASE [HelpDeskDB] SET AUTO_CREATE_STATISTICS ON
GO
ALTER DATABASE [HelpDeskDB] SET AUTO_SHRINK OFF
GO
ALTER DATABASE [HelpDeskDB] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [HelpDeskDB] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [HelpDeskDB] SET CURSOR_DEFAULT  GLOBAL
GO
ALTER DATABASE [HelpDeskDB] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [HelpDeskDB] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [HelpDeskDB] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [HelpDeskDB] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [HelpDeskDB] SET  ENABLE_BROKER
GO
ALTER DATABASE [HelpDeskDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO
ALTER DATABASE [HelpDeskDB] SET DATE_CORRELATION_OPTIMIZATION OFF
GO
ALTER DATABASE [HelpDeskDB] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [HelpDeskDB] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [HelpDeskDB] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [HelpDeskDB] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [HelpDeskDB] SET HONOR_BROKER_PRIORITY OFF
GO
ALTER DATABASE [HelpDeskDB] SET  READ_WRITE
GO
ALTER DATABASE [HelpDeskDB] SET RECOVERY SIMPLE
GO
ALTER DATABASE [HelpDeskDB] SET  MULTI_USER
GO
ALTER DATABASE [HelpDeskDB] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [HelpDeskDB] SET DB_CHAINING OFF
GO
USE [HelpDeskDB]
GO
/****** Object:  Table [dbo].[USUARIO]    Script Date: 11/17/2020 00:37:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[USUARIO](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](255) NOT NULL,
	[num_documento] [varchar](30) NULL,
	[cuenta_usuario] [varchar](20) NOT NULL,
	[contrasena] [varchar](max) NOT NULL,
	[acceso] [varchar](20) NULL,
	[correo] [varchar](50) NULL,
	[idEmpresa] [int] NOT NULL,
	[image] [image] NULL,
	[habilitado] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SOLICITUD]    Script Date: 11/17/2020 00:37:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SOLICITUD](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[noSecuencia] [varchar](12) NOT NULL,
	[fechaCreacion] [date] NOT NULL,
	[fechaInicio] [date] NULL,
	[horaInicio] [time](0) NULL,
	[fechaTermino] [date] NULL,
	[horaTermino] [time](0) NULL,
	[idUsuario] [int] NOT NULL,
	[idCliente] [int] NOT NULL,
	[tipoSolicitud] [varchar](20) NOT NULL,
	[tipoServicio] [varchar](20) NOT NULL,
	[estado] [varchar](20) NOT NULL,
	[idEmpresa] [int] NOT NULL,
	[titulo] [varchar](150) NOT NULL,
	[descripcion] [varchar](max) NULL,
	[aprobadoPor] [int] NULL,
	[habilitado] [bit] NULL,
 CONSTRAINT [PK__SOLICITU__3213E83F25869641] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[EMPRESA]    Script Date: 11/17/2020 00:37:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[EMPRESA](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[razon_social] [varchar](max) NOT NULL,
	[sector_comercial] [varchar](50) NULL,
	[rnc] [varchar](30) NULL,
	[telefono] [varchar](20) NULL,
	[correo] [varchar](50) NULL,
	[contrasena] [varchar](30) NULL,
	[url] [varchar](80) NULL,
	[host] [varchar](30) NULL,
	[port] [int] NULL,
	[direccion] [varchar](150) NULL,
	[image] [image] NULL,
	[noAutorizacion] [varchar](12) NULL,
	[secuenciaticket] [varchar](12) NOT NULL,
	[habilitado] [bit] NULL,
 CONSTRAINT [PK__EMPRESA__3213E83F07F6335A] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CLIENTE]    Script Date: 11/17/2020 00:37:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CLIENTE](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](100) NOT NULL,
	[contacto] [varchar](80) NULL,
	[telefono] [varchar](20) NULL,
	[extension] [varchar](10) NULL,
	[rnc] [varchar](30) NULL,
	[correo] [varchar](50) NULL,
	[departamento] [varchar](60) NULL,
	[direccion] [varchar](max) NULL,
	[idEmpresa] [int] NOT NULL,
	[habilitado] [bit] NULL,
 CONSTRAINT [PK__CLIENTE__3213E83F0F975522] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Default [DF__SOLICITUD__estad__276EDEB3]    Script Date: 11/17/2020 00:37:37 ******/
ALTER TABLE [dbo].[SOLICITUD] ADD  CONSTRAINT [DF__SOLICITUD__estad__276EDEB3]  DEFAULT ('Abierto') FOR [estado]
GO
