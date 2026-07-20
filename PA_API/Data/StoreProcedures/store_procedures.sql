USE pa_db;
GO

----------------------------------------------------------
-- Iniciar sesion
----------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_usuario_iniciar_sesion
(
    @CorreoElectronico NVARCHAR(200)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        Id,
        Identificacion,
        NombreCompleto,
        CorreoElectronico,
        Telefono,
        FechaNacimiento,
        PasswordHash,
        TemporaryPassword,
        Estado
    FROM Usuario
    WHERE CorreoElectronico = @CorreoElectronico;
END
GO


----------------------------------------------------------
-- Registrar Usuario
----------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_usuario_registrar
(
    @Identificacion NVARCHAR(20),
    @NombreCompleto NVARCHAR(200),
    @CorreoElectronico NVARCHAR(200),
    @Telefono NVARCHAR(20),
    @FechaNacimiento DATE,
    @PasswordHash NVARCHAR(300)
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        IF EXISTS (
            SELECT 1
            FROM Usuario
            WHERE CorreoElectronico = @CorreoElectronico
        )
            THROW 50001, 'Ya existe un usuario con ese correo electrónico.', 1;

        IF EXISTS (
            SELECT 1
            FROM Usuario
            WHERE Identificacion = @Identificacion
        )
            THROW 50002, 'Ya existe un usuario con esa identificación.', 1;

        INSERT INTO Usuario
        (
            Identificacion,
            NombreCompleto,
            CorreoElectronico,
            Telefono,
            FechaNacimiento,
            PasswordHash,
            TemporaryPassword
        )
        VALUES
        (
            @Identificacion,
            @NombreCompleto,
            @CorreoElectronico,
            @Telefono,
            @FechaNacimiento,
            @PasswordHash,
            0
        );

        DECLARE @Id INT = CAST(SCOPE_IDENTITY() AS INT);

        SELECT
            Id,
            Identificacion,
            NombreCompleto,
            CorreoElectronico,
            Telefono,
            FechaNacimiento
        FROM Usuario
        WHERE Id = @Id;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

----------------------------------------------------------
-- Obtener Usuario por UsuarioId o Correo
----------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_usuario_obtener
(
    @Id INT = NULL,
    @CorreoElectronico NVARCHAR(200) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        Id,
        Identificacion,
        NombreCompleto,
        CorreoElectronico,
        Telefono,
        FechaNacimiento
    FROM Usuario
    WHERE (@Id IS NOT NULL AND Id = @Id)
END
GO


----------------------------------------------------------
-- Obtener lista de usuarios (filtrar por estado)
----------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_usuarios_lista
(
    @Activo BIT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Identificacion,
        NombreCompleto,
        CorreoElectronico,
        Telefono,
        FechaNacimiento
    FROM Usuario
    WHERE (@Activo IS NULL OR Estado = @Activo)
END
GO


----------------------------------------------------------
-- Buscar Profesionales por texto (nombre completo, descripcion, codigo medico)
-- Buscar Profesionales por especialidadId
----------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_profesional_buscar
(
    @Texto NVARCHAR(200) = NULL,
    @EspecialidadId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        pm.Id,
        pm.NombreCompleto,
        pm.CodigoMedico,
        pm.Descripcion,
        pm.PrecioConsulta,
        pm.CorreoElectronico,
        pm.Telefono,
        STRING_AGG(em.Nombre, ', ') AS Especialidades
    FROM ProfesionalMedico pm
        INNER JOIN ProfesionalEspecialidad pe
            ON pe.ProfesionalMedicoId = pm.Id
        INNER JOIN EspecialidadMedica em
            ON em.Id = pe.EspecialidadId
    WHERE
        pm.Estado = 1
        AND (
            @Texto IS NULL
            OR @Texto = ''
            OR pm.NombreCompleto LIKE '%' + @Texto + '%'
            OR pm.Descripcion LIKE '%' + @Texto + '%'
            OR pm.CodigoMedico LIKE '%' + @Texto + '%'
        )
        AND (
            @EspecialidadId IS NULL
            OR pe.EspecialidadId = @EspecialidadId
        )

    GROUP BY
        pm.Id,
        pm.NombreCompleto,
        pm.CodigoMedico,
        pm.Descripcion,
        pm.PrecioConsulta,
        pm.CorreoElectronico,
        pm.Telefono
    ORDER BY
        pm.NombreCompleto;
END
GO


----------------------------------------------------------
-- Buscar Profesional Por Id
----------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_obtener_profesional_por_id
(
    @ProfesionalId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        pm.Id,
        pm.NombreCompleto,
        pm.Descripcion,
        pm.PrecioConsulta,
        pm.CorreoElectronico,
        pm.Telefono,
        STRING_AGG(em.Nombre, ', ') AS Especialidades
    FROM ProfesionalMedico pm
        INNER JOIN ProfesionalEspecialidad pe
            ON pe.ProfesionalMedicoId = pm.Id
        INNER JOIN EspecialidadMedica em
            ON em.Id = pe.EspecialidadId
    WHERE
        pm.Estado = 1
        AND pm.Id = @ProfesionalId
    GROUP BY
        pm.Id,
        pm.NombreCompleto,
        pm.Descripcion,
        pm.PrecioConsulta,
        pm.CorreoElectronico,
        pm.Telefono
    ORDER BY
        pm.NombreCompleto;
END
GO


----------------------------------------------------------
-- Obtener especialidades medicas
----------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_obtener_especialidades
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Nombre,
        Descripcion
    FROM EspecialidadMedica
    WHERE Estado = 1
    ORDER BY Nombre;
END;
GO


----------------------------------------------------------
-- Obtener citas (filtrar por usuario - opcional)
----------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_obtener_citas
(
    @UsuarioId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.Id,
        c.UsuarioId,
        c.ProfesionalMedicoId,

        pm.NombreCompleto AS ProfesionalMedico,
        pm.CorreoElectronico AS CorreoProfesional,
        pm.Telefono AS TelefonoProfesional,

        c.FechaHoraInicio,
        c.FechaHoraFin,

        c.NombrePaciente,
        c.IdentificacionPaciente,
        c.CorreoPaciente,
        c.TelefonoPaciente,
        c.Motivo,

        c.EstadoCita,
        c.FechaCreacion
    FROM CitaMedica c
        INNER JOIN ProfesionalMedico pm
            ON pm.Id = c.ProfesionalMedicoId
    WHERE
        (
            @UsuarioId IS NULL
            OR c.UsuarioId = @UsuarioId
        )
    ORDER BY
        c.FechaHoraInicio DESC;
END
GO


----------------------------------------------------------
-- Obtener cita por citaId o UsuarioId
----------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_obtener_cita
(
    @Id INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.Id,
        c.UsuarioId,
        c.ProfesionalMedicoId,

        pm.NombreCompleto AS ProfesionalMedico,
        pm.CorreoElectronico AS CorreoProfesional,
        pm.Telefono AS TelefonoProfesional,

        c.FechaHoraInicio,
        c.FechaHoraFin,

        c.NombrePaciente,
        c.IdentificacionPaciente,
        c.CorreoPaciente,
        c.TelefonoPaciente,
        c.Motivo,

        c.EstadoCita,
        c.FechaCreacion
    FROM CitaMedica c
        INNER JOIN ProfesionalMedico pm
            ON pm.Id = c.ProfesionalMedicoId
    WHERE c.Id = @Id
END
GO


----------------------------------------------------------
-- Crear cita
----------------------------------------------------------


CREATE OR ALTER PROCEDURE sp_crear_cita
(
    @UsuarioId INT,
    @ProfesionalMedicoId INT,
    @FechaHoraInicio DATETIME2,
    @FechaHoraFin DATETIME2,
    @NombrePaciente NVARCHAR(200),
    @IdentificacionPaciente NVARCHAR(20),
    @FechaNacimientoPaciente DATE,
    @CorreoPaciente NVARCHAR(200),
    @TelefonoPaciente NVARCHAR(20),
    @Motivo NVARCHAR(500) = NULL,
    @EstadoCita TINYINT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        IF NOT EXISTS (
            SELECT 1
            FROM Usuario
            WHERE Id = @UsuarioId
        )
            THROW 50001, 'Usuario invalido.', 1;

    INSERT INTO CitaMedica
    (
        UsuarioId,
        ProfesionalMedicoId,
        FechaHoraInicio,
        FechaHoraFin,
        NombrePaciente,
        IdentificacionPaciente,
        FechaNacimientoPaciente,
        CorreoPaciente,
        TelefonoPaciente,
        Motivo,
        EstadoCita
    )
    VALUES
    (
        @UsuarioId,
        @ProfesionalMedicoId,
        @FechaHoraInicio,
        @FechaHoraFin,
        @NombrePaciente,
        @IdentificacionPaciente,
        @FechaNacimientoPaciente,
        @CorreoPaciente,
        @TelefonoPaciente,
        @Motivo,
        @EstadoCita
    );

    DECLARE @Id INT = SCOPE_IDENTITY();

    SELECT
        c.Id,
        c.UsuarioId,
        c.ProfesionalMedicoId,

        pm.NombreCompleto AS ProfesionalMedico,
        pm.CorreoElectronico AS CorreoProfesional,
        pm.Telefono AS TelefonoProfesional,

        c.FechaHoraInicio,
        c.FechaHoraFin,

        c.NombrePaciente,
        c.IdentificacionPaciente,
        c.CorreoPaciente,
        c.TelefonoPaciente,
        c.Motivo,

        c.EstadoCita,
        c.FechaCreacion
    FROM CitaMedica c
        INNER JOIN ProfesionalMedico pm
            ON pm.Id = c.ProfesionalMedicoId
    WHERE
        c.Id = @Id;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO


----------------------------------------------------------
-- Actualizar contrasenna
----------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_actualizar_contrasena
(
    @Id INT,
    @PasswordHash NVARCHAR(300),
    @TemporaryPassword BIT
)
AS
BEGIN
    UPDATE Usuario
    SET
        PasswordHash = @PasswordHash,
        TemporaryPassword = @TemporaryPassword
    WHERE
        Id = @Id
        AND Estado = 1;
END;
GO