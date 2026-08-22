
--==========================================================
-- SEED DE DATOS INICIALES
--=========================================================

-- =========================================================
-- MediCitasDB - Datos semilla
-- Ejecutar DESPUÉS de 00_CrearBaseDeDatos.sql
--
-- Cada bloque solo inserta si la tabla correspondiente está
-- vacía, así el script se puede volver a correr sin duplicar
-- datos ni chocar con SET IDENTITY_INSERT.
--
-- Contraseña de prueba para TODOS los usuarios seed:
-- Prueba123!
-- =========================================================

USE pa_db;
GO

-- =========================================================
-- Roles de usuario (3)
-- =========================================================

IF NOT EXISTS (SELECT 1 FROM dbo.RolUsuario WHERE Id = 1)
BEGIN
    INSERT INTO dbo.RolUsuario (Id, Nombre, Descripcion, Estado)
    VALUES (1, N'SuperAdmin', N'Administrador principal del sistema.', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.RolUsuario WHERE Id = 2)
BEGIN
    INSERT INTO dbo.RolUsuario (Id, Nombre, Descripcion, Estado)
    VALUES (2, N'Doctor', N'Profesional médico con acceso a sus citas.', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.RolUsuario WHERE Id = 3)
BEGIN
    INSERT INTO dbo.RolUsuario (Id, Nombre, Descripcion, Estado)
    VALUES (3, N'Paciente', N'Usuario paciente que agenda citas médicas.', 1);
END
GO

-- =========================================================
-- ESPECIALIDADES MÉDICAS (8)
-- =========================================================

IF NOT EXISTS (SELECT 1 FROM EspecialidadMedica)
BEGIN
    SET IDENTITY_INSERT EspecialidadMedica ON;

    INSERT INTO EspecialidadMedica
    (
        Id,
        Nombre,
        Descripcion,
        Estado
    )
    VALUES
        (1, N'Medicina General',
            N'Consulta médica general y valoración inicial.', 1),

        (2, N'Pediatría',
            N'Atención médica especializada en niños y adolescentes.', 1),

        (3, N'Dermatología',
            N'Diagnóstico y tratamiento de enfermedades de la piel.', 1),

        (4, N'Cardiología',
            N'Prevención, diagnóstico y tratamiento de enfermedades cardiovasculares.', 1),

        (5, N'Ginecología',
            N'Atención integral de la salud ginecológica de la mujer.', 1),

        (6, N'Ortopedia',
            N'Diagnóstico y tratamiento de lesiones y enfermedades del sistema musculoesquelético.', 1),

        (7, N'Neurología',
            N'Diagnóstico y tratamiento de enfermedades del sistema nervioso.', 1),

        (8, N'Endocrinología',
            N'Diagnóstico y tratamiento de trastornos hormonales y metabólicos.', 1);

    SET IDENTITY_INSERT EspecialidadMedica OFF;
END
GO


-- =========================================================
-- PROFESIONALES MÉDICOS (6)
-- =========================================================

IF NOT EXISTS (SELECT 1 FROM ProfesionalMedico)
BEGIN
    SET IDENTITY_INSERT ProfesionalMedico ON;

    INSERT INTO ProfesionalMedico
    (
        Id,
        NombreCompleto,
        CodigoMedico,
        Descripcion,
        PrecioConsulta,
        CorreoElectronico,
        Telefono,
        FechaNacimiento,
        Estado
    )
    VALUES

    -- 1
    (
        1,
        N'Dra. Ana Ramírez Solís',
        N'MED-1001',
        N'Médica general con más de 10 años de experiencia en atención primaria.',
        35000.00,
        N'ana.ramirez@medicitas.test',
        N'8888-1001',
        '1985-03-14',
        1
    ),

    -- 2
    (
        2,
        N'Dr. Luis Fernando Castro',
        N'MED-1002',
        N'Pediatra enfocado en control de niño sano y enfermedades infantiles.',
        42000.00,
        N'luis.castro@medicitas.test',
        N'8888-1002',
        '1979-11-02',
        1
    ),

    -- 3
    (
        3,
        N'Dra. Karla Jiménez Vargas',
        N'MED-1003',
        N'Dermatóloga especializada en acné, alergias y enfermedades de la piel.',
        48000.00,
        N'karla.jimenez@medicitas.test',
        N'8888-1003',
        '1988-07-21',
        1
    ),

    -- 4
    (
        4,
        N'Dr. Andrés Mora Rodríguez',
        N'MED-1004',
        N'Cardiólogo especializado en prevención cardiovascular y control de hipertensión.',
        55000.00,
        N'andres.mora@medicitas.test',
        N'8888-1004',
        '1976-05-18',
        1
    ),

    -- 5
    (
        5,
        N'Dra. Sofía Vargas Méndez',
        N'MED-1005',
        N'Ginecóloga con experiencia en salud reproductiva y controles preventivos.',
        50000.00,
        N'sofia.vargas@medicitas.test',
        N'8888-1005',
        '1983-09-09',
        1
    ),

    -- 6
    (
        6,
        N'Dr. Diego Hernández Rojas',
        N'MED-1006',
        N'Ortopedista especializado en lesiones deportivas y problemas articulares.',
        52000.00,
        N'diego.hernandez@medicitas.test',
        N'8888-1006',
        '1981-01-27',
        1
    );

    SET IDENTITY_INSERT ProfesionalMedico OFF;
END
GO


-- =========================================================
-- PROFESIONAL <-> ESPECIALIDAD
-- =========================================================

IF NOT EXISTS (SELECT 1 FROM ProfesionalEspecialidad)
BEGIN
    INSERT INTO ProfesionalEspecialidad
    (
        ProfesionalMedicoId,
        EspecialidadId
    )
    VALUES

    -- Ana
    (1, 1),
    (1, 2),

    -- Luis
    (2, 2),

    -- Karla
    (3, 3),
    (3, 1),

    -- Andrés
    (4, 4),
    (4, 1),

    -- Sofía
    (5, 5),

    -- Diego
    (6, 6),
    (6, 1);
END
GO


-- =========================================================
-- HORARIOS
--
-- DiaSemana:
-- 1 = Lunes
-- 2 = Martes
-- 3 = Miércoles
-- 4 = Jueves
-- 5 = Viernes
-- 6 = Sábado
-- 7 = Domingo
--
-- Todos los horarios tienen bloques suficientemente largos
-- para generar citas de 40 minutos.
-- =========================================================

IF NOT EXISTS (SELECT 1 FROM HorarioProfesional)
BEGIN
    INSERT INTO HorarioProfesional
    (
        ProfesionalMedicoId,
        DiaSemana,
        HoraInicio,
        HoraFin,
        Estado
    )
    VALUES

    -- =========================================================
    -- 1. ANA RAMÍREZ
    -- Lunes y miércoles mañana
    -- Viernes tarde
    -- =========================================================

    (1, 1, '08:00', '12:00', 1),
    (1, 3, '08:00', '12:00', 1),
    (1, 5, '13:00', '17:00', 1),


    -- =========================================================
    -- 2. LUIS CASTRO
    -- Martes y jueves mañana
    -- Sábado mañana
    -- =========================================================

    (2, 2, '09:00', '13:00', 1),
    (2, 4, '09:00', '13:00', 1),
    (2, 6, '08:00', '12:00', 1),


    -- =========================================================
    -- 3. KARLA JIMÉNEZ
    -- Lunes tarde
    -- Miércoles mañana
    -- Viernes mañana y tarde
    -- =========================================================

    (3, 1, '13:00', '17:00', 1),
    (3, 3, '09:00', '13:00', 1),
    (3, 5, '08:00', '12:00', 1),
    (3, 5, '14:00', '18:00', 1),


    -- =========================================================
    -- 4. ANDRÉS MORA
    -- Lunes, martes y jueves
    -- Horarios de tarde
    -- =========================================================

    (4, 1, '14:00', '18:00', 1),
    (4, 2, '14:00', '18:00', 1),
    (4, 4, '13:00', '17:00', 1),


    -- =========================================================
    -- 5. SOFÍA VARGAS
    -- Lunes, miércoles y viernes
    -- Mañana
    -- =========================================================

    (5, 1, '07:00', '11:00', 1),
    (5, 3, '07:00', '11:00', 1),
    (5, 5, '07:00', '11:00', 1),


    -- =========================================================
    -- 6. DIEGO HERNÁNDEZ
    -- Martes y jueves mañana
    -- Viernes tarde
    -- Sábado
    -- =========================================================

    (6, 2, '08:00', '12:00', 1),
    (6, 4, '08:00', '12:00', 1),
    (6, 5, '14:00', '18:00', 1),
    (6, 6, '09:00', '13:00', 1);
END
GO


-- =========================================================
-- Roles
-- Nota: si ya corriste 00_CrearBaseDeDatos.sql actualizado,
-- estos ya existen y este bloque no hace nada gracias al
-- IF NOT EXISTS por fila. Se deja aquí como respaldo por si
-- este seed se corre sobre una base creada con el script viejo.
-- =========================================================

IF NOT EXISTS (SELECT 1 FROM dbo.RolUsuario WHERE Id = 1)
BEGIN
    INSERT INTO dbo.RolUsuario (Id, Nombre, Descripcion, Estado)
    VALUES (1, N'SuperAdmin', N'Administrador principal del sistema.', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.RolUsuario WHERE Id = 2)
BEGIN
    INSERT INTO dbo.RolUsuario (Id, Nombre, Descripcion, Estado)
    VALUES (2, N'Doctor', N'Profesional médico con acceso a sus citas.', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.RolUsuario WHERE Id = 3)
BEGIN
    INSERT INTO dbo.RolUsuario (Id, Nombre, Descripcion, Estado)
    VALUES (3, N'Paciente', N'Usuario paciente que agenda citas médicas.', 1);
END
GO


-- =========================================================
-- USUARIOS PROFESIONALES MÉDICOS
-- RolId = 2
-- $2a$11$uq75mtIS2h2j4frwtVXWqehailPBy18igZljAHV0FKwDgWNHsmp0W = Test@123
-- =========================================================

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Usuario
    WHERE RolId = 2
)
BEGIN

    INSERT INTO dbo.Usuario
    (
        Identificacion,
        NombreCompleto,
        CorreoElectronico,
        Telefono,
        FechaNacimiento,
        PasswordHash,
        TemporaryPassword,
        FechaExpiracionPasswordTemporal,
        FechaRegistro,
        Estado,
        RolId,
        ProfesionalMedicoId
    )
    VALUES

    -- =====================================================
    -- 1. Dra. Ana Ramírez Solís
    -- =====================================================
    (
        N'101010101',
        N'Dra. Ana Ramírez Solís',
        N'ana.ramirez@medicitas.test',
        N'8888-1001',
        '1985-03-14',
        N'$2a$11$uq75mtIS2h2j4frwtVXWqehailPBy18igZljAHV0FKwDgWNHsmp0W',
        1,
        DATEADD(DAY, 30, GETDATE()),
        GETDATE(),
        1,
        2,
        1
    ),

    -- =====================================================
    -- 2. Dr. Luis Fernando Castro
    -- =====================================================
    (
        N'202020202',
        N'Dr. Luis Fernando Castro',
        N'luis.castro@medicitas.test',
        N'8888-1002',
        '1979-11-02',
        N'$2a$11$uq75mtIS2h2j4frwtVXWqehailPBy18igZljAHV0FKwDgWNHsmp0W',
        1,
        DATEADD(DAY, 30, GETDATE()),
        GETDATE(),
        1,
        2,
        2
    ),

    -- =====================================================
    -- 3. Dra. Karla Jiménez Vargas
    -- =====================================================
    (
        N'303030303',
        N'Dra. Karla Jiménez Vargas',
        N'karla.jimenez@medicitas.test',
        N'8888-1003',
        '1988-07-21',
        N'$2a$11$uq75mtIS2h2j4frwtVXWqehailPBy18igZljAHV0FKwDgWNHsmp0W',
        1,
        DATEADD(DAY, 30, GETDATE()),
        GETDATE(),
        1,
        2,
        3
    ),

    -- =====================================================
    -- 4. Dr. Andrés Mora Rodríguez
    -- =====================================================
    (
        N'404040404',
        N'Dr. Andrés Mora Rodríguez',
        N'andres.mora@medicitas.test',
        N'8888-1004',
        '1976-05-18',
        N'$2a$11$uq75mtIS2h2j4frwtVXWqehailPBy18igZljAHV0FKwDgWNHsmp0W',
        1,
        DATEADD(DAY, 30, GETDATE()),
        GETDATE(),
        1,
        2,
        4
    ),

    -- =====================================================
    -- 5. Dra. Sofía Vargas Méndez
    -- =====================================================
    (
        N'505050505',
        N'Dra. Sofía Vargas Méndez',
        N'sofia.vargas@medicitas.test',
        N'8888-1005',
        '1983-09-09',
        N'$2a$11$uq75mtIS2h2j4frwtVXWqehailPBy18igZljAHV0FKwDgWNHsmp0W',
        1,
        DATEADD(DAY, 30, GETDATE()),
        GETDATE(),
        1,
        2,
        5
    ),

    -- =====================================================
    -- 6. Dr. Diego Hernández Rojas
    -- =====================================================
    (
        N'606060606',
        N'Dr. Diego Hernández Rojas',
        N'diego.hernandez@medicitas.test',
        N'8888-1006',
        '1981-01-27',
        N'$2a$11$uq75mtIS2h2j4frwtVXWqehailPBy18igZljAHV0FKwDgWNHsmp0W',
        1,
        DATEADD(DAY, 30, GETDATE()),
        GETDATE(),
        1,
        2,
        6
    );

END
GO


-- =========================================================
-- SUPER ADMIN
-- RolId = 3
-- =========================================================

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Usuario
    WHERE CorreoElectronico = N'superadmin@sistemacitas.com'
)
BEGIN
    INSERT INTO dbo.Usuario
    (
        Identificacion,
        NombreCompleto,
        CorreoElectronico,
        Telefono,
        FechaNacimiento,
        PasswordHash,
        TemporaryPassword,
        FechaExpiracionPasswordTemporal,
        FechaRegistro,
        Estado,
        RolId,
        ProfesionalMedicoId
    )
    VALUES
    (
        N'000000001',
        N'Super Administrador',
        N'superadmin@sistemacitas.com',
        N'8888-0001',
        '2026-01-01',
        N'$2a$11$uq75mtIS2h2j4frwtVXWqehailPBy18igZljAHV0FKwDgWNHsmp0W',        1,
        DATEADD(DAY, 30, GETDATE()),
        GETDATE(),
        1,
        1,
        NULL
    );
END
GO