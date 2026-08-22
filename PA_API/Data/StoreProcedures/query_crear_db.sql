-- =========================================================
-- MediCitasDB
-- Sistema de Reserva de Citas Médicas
-- Proyecto de Programación Avanzada Web
-- =========================================================

CREATE DATABASE pa_db;
GO

USE pa_db;
GO

-- =========================================================
-- RolUsuario
-- =========================================================
CREATE TABLE RolUsuario (
    Id           TINYINT            NOT NULL,
    Nombre       NVARCHAR(50)       NOT NULL,
    Descripcion  NVARCHAR(200)      NULL,
    Estado       BIT                NOT NULL DEFAULT 1,

    CONSTRAINT PK_RolUsuario PRIMARY KEY (Id),
    CONSTRAINT UQ_RolUsuario_Nombre UNIQUE (Nombre)
);
GO


-- =========================================================
-- EspecialidadMedica
-- =========================================================
CREATE TABLE EspecialidadMedica (
    Id           INT IDENTITY(1,1)  NOT NULL,
    Nombre       NVARCHAR(150)      NOT NULL,
    Descripcion  NVARCHAR(500)      NULL,
    Estado       BIT                NOT NULL DEFAULT 1,

    CONSTRAINT PK_EspecialidadMedica PRIMARY KEY (Id),
    CONSTRAINT UQ_EspecialidadMedica_Nombre UNIQUE (Nombre)
);
GO

-- =========================================================
-- ProfesionalMedico
-- =========================================================
CREATE TABLE ProfesionalMedico (
    Id                 INT IDENTITY(1,1)   NOT NULL,
    NombreCompleto     NVARCHAR(200)       NOT NULL,
    CodigoMedico       NVARCHAR(8)         NOT NULL,
    Descripcion        NVARCHAR(1000)      NULL,
    PrecioConsulta     DECIMAL(10,2)       NOT NULL,
    CorreoElectronico  NVARCHAR(200)       NOT NULL,
    Telefono           NVARCHAR(20)        NOT NULL,
    FechaNacimiento    DATE                NOT NULL,
    Estado             BIT                 NOT NULL DEFAULT 1,

    CONSTRAINT PK_ProfesionalMedico PRIMARY KEY (Id),
    CONSTRAINT UQ_ProfesionalMedico_CodigoMedico UNIQUE (CodigoMedico),
    CONSTRAINT UQ_ProfesionalMedico_Correo UNIQUE (CorreoElectronico),
    CONSTRAINT CK_ProfesionalMedico_Precio CHECK (PrecioConsulta >= 0)
);
GO

-- =========================================================
-- ProfesionalEspecialidad (tabla puente N:N)
-- =========================================================
CREATE TABLE ProfesionalEspecialidad (
    Id                   INT IDENTITY(1,1)  NOT NULL,
    ProfesionalMedicoId  INT                NOT NULL,
    EspecialidadId       INT                NOT NULL,

    CONSTRAINT PK_ProfesionalEspecialidad PRIMARY KEY (Id),
    CONSTRAINT UQ_ProfesionalEspecialidad UNIQUE (ProfesionalMedicoId, EspecialidadId),
    CONSTRAINT FK_ProfesionalEspecialidad_Profesional FOREIGN KEY (ProfesionalMedicoId)
        REFERENCES ProfesionalMedico (Id),
    CONSTRAINT FK_ProfesionalEspecialidad_Especialidad FOREIGN KEY (EspecialidadId)
        REFERENCES EspecialidadMedica (Id)
);
GO

-- =========================================================
-- HorarioProfesional
-- Horario recurrente semanal (día + rango + duración de cita).
-- DiaSemana:     1=Lunes 2=Martes 3=Miercoles 4=Jueves 5=Viernes 6=Sabado 7=Domingo
-- TipoConsulta:  1=Presencial 2=Videoconsulta
-- =========================================================
CREATE TABLE HorarioProfesional (
    Id                    INT IDENTITY(1,1)  NOT NULL,
    ProfesionalMedicoId   INT                NOT NULL,
    DiaSemana             TINYINT            NOT NULL,
    HoraInicio            TIME               NOT NULL,
    HoraFin               TIME               NOT NULL,
    Estado                BIT                NOT NULL DEFAULT 1,

    CONSTRAINT PK_HorarioProfesional PRIMARY KEY (Id),
    CONSTRAINT FK_HorarioProfesional_Profesional FOREIGN KEY (ProfesionalMedicoId)
        REFERENCES ProfesionalMedico (Id),
    CONSTRAINT CK_HorarioProfesional_DiaSemana CHECK (DiaSemana BETWEEN 1 AND 7),
);
GO

-- =========================================================
-- Usuario
-- Paciente / Doctor / SuperAdmin que se registra e inicia sesión.
-- RolId es obligatorio. ProfesionalMedicoId solo aplica cuando el
-- usuario es Doctor (Rol=2) y liga 1:1 con ProfesionalMedico.
-- =========================================================
CREATE TABLE Usuario (
    Id                              INT IDENTITY(1,1)  NOT NULL,
    Identificacion                  NVARCHAR(20)       NOT NULL,
    NombreCompleto                  NVARCHAR(200)      NOT NULL,
    CorreoElectronico               NVARCHAR(200)      NOT NULL,
    Telefono                        NVARCHAR(20)       NOT NULL,
    FechaNacimiento                 DATE               NOT NULL,
    PasswordHash                    NVARCHAR(300)      NOT NULL,
    TemporaryPassword               BIT                NOT NULL DEFAULT 0,
    FechaExpiracionPasswordTemporal DATETIME2          NULL,
    FechaRegistro                   DATETIME2          NOT NULL DEFAULT SYSUTCDATETIME(),
    Estado                          BIT                NOT NULL DEFAULT 1,
    RolId                           TINYINT            NOT NULL,
    ProfesionalMedicoId             INT                NULL,

    CONSTRAINT PK_Usuario PRIMARY KEY (Id),
    CONSTRAINT UQ_Usuario_Correo UNIQUE (CorreoElectronico),
    CONSTRAINT UQ_Usuario_Identificacion UNIQUE (Identificacion),
    CONSTRAINT FK_Usuario_RolUsuario FOREIGN KEY (RolId)
        REFERENCES RolUsuario (Id),
    CONSTRAINT FK_Usuario_ProfesionalMedico FOREIGN KEY (ProfesionalMedicoId)
        REFERENCES ProfesionalMedico (Id)
);
GO

-- Un ProfesionalMedico solo puede estar ligado a un único Usuario
CREATE UNIQUE INDEX UX_Usuario_ProfesionalMedicoId
    ON Usuario (ProfesionalMedicoId)
    WHERE ProfesionalMedicoId IS NOT NULL;
GO

-- =========================================================
-- CitaMedica
-- Los datos del paciente atendido se guardan aparte del Usuario logueado
-- para soportar "cita para otra persona" sin tocar la tabla Usuario.
-- EstadoCita: 1=Pendiente 2=Confirmada 3=Cancelada 4=Finalizada
-- =========================================================
CREATE TABLE CitaMedica (
    Id                        INT IDENTITY(1,1)     NOT NULL,
    UsuarioId                 INT                   NOT NULL,
    ProfesionalMedicoId       INT                   NOT NULL,

    FechaHoraInicio           DATETIME2             NOT NULL,
    FechaHoraFin              DATETIME2             NOT NULL,

    NombrePaciente            NVARCHAR(200)         NOT NULL,
    IdentificacionPaciente    NVARCHAR(20)          NOT NULL,
    FechaNacimientoPaciente   DATE                  NOT NULL,
    CorreoPaciente            NVARCHAR(200)         NOT NULL,
    TelefonoPaciente          NVARCHAR(20)          NOT NULL,
    Motivo                    NVARCHAR(500)         NULL,

    EstadoCita                TINYINT               NOT NULL DEFAULT 1,
    FechaCreacion             DATETIME2             NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_CitaMedica PRIMARY KEY (Id),
    CONSTRAINT FK_CitaMedica_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES Usuario (Id),
    CONSTRAINT FK_CitaMedica_Profesional FOREIGN KEY (ProfesionalMedicoId)
        REFERENCES ProfesionalMedico (Id),
    CONSTRAINT CK_CitaMedica_Rango CHECK (FechaHoraFin > FechaHoraInicio),
    CONSTRAINT CK_CitaMedica_Estado CHECK (EstadoCita BETWEEN 1 AND 4)
);
GO

-- Respaldo a nivel de base de datos del chequeo que ya hace sp_CitaMedica_ExisteParaProfesionalYFecha:
-- evita dos citas activas (Pendiente=1 o Confirmada=2) para el mismo profesional a la misma hora exacta,
-- incluso si dos requests concurrentes pasan la validación del SP al mismo tiempo.
CREATE UNIQUE INDEX UQ_CitaMedica_ProfesionalHorarioActivo
    ON CitaMedica (ProfesionalMedicoId, FechaHoraInicio)
    WHERE EstadoCita IN (1, 2);
GO

-- =========================================================
-- Notificacion
-- =========================================================
CREATE TABLE Notificacion (
    Id             INT IDENTITY(1,1)  NOT NULL,
    UsuarioId      INT                NOT NULL,
    CitaMedicaId   INT                NULL,
    Titulo         NVARCHAR(150)      NOT NULL,
    Mensaje        NVARCHAR(500)      NOT NULL,
    Leida          BIT                NOT NULL DEFAULT 0,
    FechaCreacion  DATETIME2          NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Notificacion PRIMARY KEY (Id),
    CONSTRAINT FK_Notificacion_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES Usuario (Id),
    CONSTRAINT FK_Notificacion_CitaMedica FOREIGN KEY (CitaMedicaId)
        REFERENCES CitaMedica (Id)
);
GO

CREATE INDEX IX_Notificacion_Usuario_Leida
    ON Notificacion (UsuarioId, Leida, FechaCreacion);
GO

