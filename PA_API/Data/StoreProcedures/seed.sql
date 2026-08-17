-- =========================================================
-- MediCitasDB - Datos semilla ampliados
-- Ejecutar DESPUÉS de 00_CrearBaseDeDatos.sql, sobre tablas vacías.
--
-- Contraseña de prueba para TODOS los usuarios seed:
-- Prueba123!
-- =========================================================

USE pa_db;
GO

-- =========================================================
-- ESPECIALIDADES MÉDICAS (8)
-- =========================================================

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
GO


-- =========================================================
-- PROFESIONALES MÉDICOS (12)
-- =========================================================

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
),

-- 7
(
    7,
    N'Dra. Mariana Solano Pérez',
    N'MED-1007',
    N'Neuróloga especializada en migrañas, trastornos del sueño y enfermedades neurológicas.',
    58000.00,
    N'mariana.solano@medicitas.test',
    N'8888-1007',
    '1978-12-04',
    1
),

-- 8
(
    8,
    N'Dr. Esteban Quesada León',
    N'MED-1008',
    N'Endocrinólogo especializado en diabetes y trastornos metabólicos.',
    56000.00,
    N'esteban.quesada@medicitas.test',
    N'8888-1008',
    '1980-06-12',
    1
),

-- 9
(
    9,
    N'Dra. Laura Chaves Navarro',
    N'MED-1009',
    N'Médica general con énfasis en medicina preventiva y seguimiento de pacientes crónicos.',
    37000.00,
    N'laura.chaves@medicitas.test',
    N'8888-1009',
    '1987-04-30',
    1
),

-- 10
(
    10,
    N'Dr. Mauricio Salas Brenes',
    N'MED-1010',
    N'Pediatra especializado en enfermedades respiratorias infantiles.',
    44000.00,
    N'mauricio.salas@medicitas.test',
    N'8888-1010',
    '1975-10-15',
    1
),

-- 11
(
    11,
    N'Dra. Valeria Campos Arias',
    N'MED-1011',
    N'Dermatóloga especializada en dermatología clínica y estética.',
    51000.00,
    N'valeria.campos@medicitas.test',
    N'8888-1011',
    '1989-02-11',
    1
),

-- 12
(
    12,
    N'Dr. Ricardo Montero Ruiz',
    N'MED-1012',
    N'Médico general y especialista en atención preventiva del adulto.',
    36000.00,
    N'ricardo.montero@medicitas.test',
    N'8888-1012',
    '1984-08-25',
    1
);

SET IDENTITY_INSERT ProfesionalMedico OFF;
GO


-- =========================================================
-- PROFESIONAL <-> ESPECIALIDAD
--
-- Algunos profesionales tienen varias especialidades.
-- =========================================================

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
(6, 1),

-- Mariana
(7, 7),

-- Esteban
(8, 8),
(8, 1),

-- Laura
(9, 1),

-- Mauricio
(10, 2),
(10, 1),

-- Valeria
(11, 3),

-- Ricardo
(12, 1),
(12, 8);

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
(6, 6, '09:00', '13:00', 1),


-- =========================================================
-- 7. MARIANA SOLANO
-- Lunes y miércoles tarde
-- Jueves mañana
-- =========================================================

(7, 1, '14:00', '18:00', 1),
(7, 3, '14:00', '18:00', 1),
(7, 4, '08:00', '12:00', 1),


-- =========================================================
-- 8. ESTEBAN QUESADA
-- Martes mañana
-- Miércoles tarde
-- Viernes mañana
-- =========================================================

(8, 2, '08:00', '12:00', 1),
(8, 3, '14:00', '18:00', 1),
(8, 5, '08:00', '12:00', 1),


-- =========================================================
-- 9. LAURA CHAVES
-- Lunes, martes y viernes
-- Horarios mixtos
-- =========================================================

(9, 1, '08:00', '12:00', 1),
(9, 2, '13:00', '17:00', 1),
(9, 5, '09:00', '13:00', 1),


-- =========================================================
-- 10. MAURICIO SALAS
-- Martes y jueves tarde
-- Sábado mañana
-- =========================================================

(10, 2, '14:00', '18:00', 1),
(10, 4, '14:00', '18:00', 1),
(10, 6, '08:00', '12:00', 1),


-- =========================================================
-- 11. VALERIA CAMPOS
-- Lunes mañana
-- Miércoles tarde
-- Viernes tarde
-- =========================================================

(11, 1, '08:00', '12:00', 1),
(11, 3, '13:00', '17:00', 1),
(11, 5, '13:00', '17:00', 1),


-- =========================================================
-- 12. RICARDO MONTERO
-- Martes, jueves y sábado
-- Horarios variados
-- =========================================================

(12, 2, '07:00', '11:00', 1),
(12, 4, '13:00', '17:00', 1),
(12, 6, '09:00', '13:00', 1);

GO