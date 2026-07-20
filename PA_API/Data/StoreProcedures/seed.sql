-- =========================================================
-- MediCitasDB - Datos semilla (seed) para pruebas
-- Ejecutar DESPUÉS de 00_CrearBaseDeDatos.sql, sobre tablas vacías.
--
-- Contraseña de prueba para TODOS los usuarios seed: Prueba123!
-- (el hash de abajo es un BCrypt real de esa contraseña, generado con
-- la misma librería que usa el API, así se puede probar login de una vez)
-- =========================================================

USE pa_db;
GO

DECLARE @PasswordHashPrueba NVARCHAR(300) = '$2b$12$2kNchGjd/rGmoLR6xpeArOShs9G3khoxzb55nrf40dZ8jaBYoskHO';

-- =========================================================
-- Especialidades (3)
-- Se fuerzan los Id explícitos para poder referenciarlos abajo sin ambigüedad.
-- =========================================================
SET IDENTITY_INSERT EspecialidadMedica ON;

INSERT INTO EspecialidadMedica (Id, Nombre, Descripcion, Estado) VALUES
    (1, N'Medicina General', N'Consulta médica general y valoración inicial.', 1),
    (2, N'Pediatría',        N'Atención médica especializada en niños y adolescentes.', 1),
    (3, N'Dermatología',     N'Diagnóstico y tratamiento de enfermedades de la piel.', 1);

SET IDENTITY_INSERT EspecialidadMedica OFF;
GO

-- =========================================================
-- Profesionales médicos (3)
-- =========================================================
SET IDENTITY_INSERT ProfesionalMedico ON;

INSERT INTO ProfesionalMedico (Id, NombreCompleto, CodigoMedico, Descripcion, PrecioConsulta, CorreoElectronico, Telefono, FechaNacimiento, Estado) VALUES
    (1, N'Dra. Ana Ramírez Solís',   N'MED-1001', N'Médico general con más de 10 años de experiencia en atención primaria.', 35000.00, N'ana.ramirez@medicitas.test',   N'8888-1001', '1985-03-14', 1),
    (2, N'Dr. Luis Fernando Castro', N'MED-1002', N'Pediatra enfocado en control de niño sano y enfermedades infantiles.',     42000.00, N'luis.castro@medicitas.test',   N'8888-1002', '1979-11-02', 1),
    (3, N'Dra. Karla Jiménez Vargas',N'MED-1003', N'Dermatóloga especializada en acné y enfermedades de la piel.',            48000.00, N'karla.jimenez@medicitas.test', N'8888-1003', '1988-07-21', 1);

SET IDENTITY_INSERT ProfesionalMedico OFF;
GO

-- =========================================================
-- Profesional <-> Especialidad (1 a 2 por médico)
-- Profesional 1: Medicina General + Pediatría (2)
-- Profesional 2: Pediatría (1)
-- Profesional 3: Dermatología + Medicina General (2)
-- =========================================================
INSERT INTO ProfesionalEspecialidad (ProfesionalMedicoId, EspecialidadId) VALUES
    (1, 1),
    (1, 2),
    (2, 2),
    (3, 3),
    (3, 1);
GO

-- =========================================================
-- Horarios (3 días por profesional)
-- DiaSemana: 1=Lunes 2=Martes 3=Miercoles 4=Jueves 5=Viernes 6=Sabado 7=Domingo
-- TipoConsulta: 1=Presencial 2=Videoconsulta
-- =========================================================
INSERT INTO HorarioProfesional (ProfesionalMedicoId, DiaSemana, HoraInicio, HoraFin, Estado) VALUES
    -- Profesional 1: Lunes, Miércoles, Viernes
    (1, 1, '08:00', '12:00', 1),
    (1, 3, '08:00', '12:00', 1),
    (1, 5, '13:00', '17:00', 1),

    -- Profesional 2: Martes, Jueves, Sábado
    (2, 2, '09:00', '13:00', 1),
    (2, 4, '09:00', '13:00', 1),
    (2, 6, '08:00', '11:00', 1),

    -- Profesional 3: Lunes, Miércoles, Viernes
    (3, 1, '10:00', '14:00', 1),
    (3, 3, '10:00', '14:00', 1),
    (3, 5, '08:00', '12:00', 1);
GO

-- =========================================================
-- Usuarios (3)
-- Todos con la misma contraseña de prueba: Prueba123!
-- =========================================================
DECLARE @PasswordHashPrueba NVARCHAR(300) = '$2b$12$2kNchGjd/rGmoLR6xpeArOShs9G3khoxzb55nrf40dZ8jaBYoskHO';

INSERT INTO Usuario (Identificacion, NombreCompleto, CorreoElectronico, Telefono, FechaNacimiento, PasswordHash, FechaRegistro, Estado) VALUES
    (N'1-1111-1111', N'María José Alvarado', N'maria.alvarado@correo.test', N'8700-0001', '1995-05-10', @PasswordHashPrueba, SYSUTCDATETIME(), 1),
    (N'2-2222-2222', N'Carlos Andrés Mora',  N'carlos.mora@correo.test',    N'8700-0002', '1990-09-23', @PasswordHashPrueba, SYSUTCDATETIME(), 1),
    (N'3-3333-3333', N'Fernanda Solano Ruiz',N'fernanda.solano@correo.test',N'8700-0003', '2000-01-30', @PasswordHashPrueba, SYSUTCDATETIME(), 1);
GO

-- =========================================================
-- Citas (1 por usuario)
-- Fechas a futuro respecto a hoy, alineadas con los horarios sembrados arriba.
-- EstadoCita: 1=Pendiente 2=Confirmada 3=Cancelada 4=Finalizada
-- =========================================================
INSERT INTO CitaMedica (
    UsuarioId, ProfesionalMedicoId, FechaHoraInicio, FechaHoraFin,
    NombrePaciente, IdentificacionPaciente, FechaNacimientoPaciente, CorreoPaciente, TelefonoPaciente, Motivo,
    EstadoCita, FechaCreacion
) VALUES
    -- María José agenda para sí misma con el Profesional 1 (Medicina General)
    (1, 1, '2026-07-27 08:00', '2026-07-27 08:30',
     N'María José Alvarado', N'1-1111-1111', '1995-05-10', N'maria.alvarado@correo.test', N'8700-0001', N'Chequeo general anual.',
     1, SYSUTCDATETIME()),

    -- Carlos agenda con el Profesional 2 (Pediatría) para su hijo -> es para otra persona
    (2, 2, '2026-07-28 09:00', '2026-07-28 09:20', 
     N'Mateo Mora Salas', N'4-4444-4444', '2018-02-14', N'carlos.mora@correo.test', N'8700-0002', N'Control de niño sano.',
     2, SYSUTCDATETIME()),

    -- Fernanda agenda para sí misma con el Profesional 3 (Dermatología)
    (3, 3, '2026-07-29 10:00', '2026-07-29 10:40',
     N'Fernanda Solano Ruiz', N'3-3333-3333', '2000-01-30', N'fernanda.solano@correo.test', N'8700-0003', N'Valoración de acné persistente.',
     1, SYSUTCDATETIME());
GO