USE pa_db;
GO

/* =========================================================
   USUARIOS (autenticación y perfil)
   ========================================================= */

-- Devuelve el usuario (con su rol) a partir del correo, para login.
CREATE OR ALTER PROCEDURE sp_usuario_iniciar_sesion
(
    @CorreoElectronico NVARCHAR(200)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        u.Id,
        u.Identificacion,
        u.NombreCompleto,
        u.CorreoElectronico,
        u.Telefono,
        u.FechaNacimiento,
        u.PasswordHash,
        u.TemporaryPassword,
        u.FechaExpiracionPasswordTemporal,
        u.FechaRegistro,
        u.Estado,
        u.RolId,
        r.Nombre AS RolNombre,
        u.ProfesionalMedicoId
    FROM Usuario u
        INNER JOIN RolUsuario r
            ON r.Id = u.RolId
    WHERE u.CorreoElectronico = @CorreoElectronico;
END
GO

-- Registra un nuevo usuario público (siempre queda con rol Paciente = 3).
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

    IF EXISTS (
        SELECT 1
        FROM Usuario
        WHERE Identificacion = @Identificacion
    )
        THROW 50001, 'La identificación ya se encuentra registrada.', 1;

    IF EXISTS (
        SELECT 1
        FROM Usuario
        WHERE CorreoElectronico = @CorreoElectronico
    )
        THROW 50002, 'El correo electrónico ya se encuentra registrado.', 1;

    INSERT INTO Usuario
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
        @Identificacion,
        @NombreCompleto,
        @CorreoElectronico,
        @Telefono,
        @FechaNacimiento,
        @PasswordHash,
        0,
        NULL,
        SYSUTCDATETIME(),
        1,
        3,
        NULL
    );

    DECLARE @UsuarioId INT = CAST(SCOPE_IDENTITY() AS INT);

    SELECT
        u.Id,
        u.Identificacion,
        u.NombreCompleto,
        u.CorreoElectronico,
        u.Telefono,
        u.FechaNacimiento,
        u.RolId,
        r.Nombre AS RolNombre,
        u.ProfesionalMedicoId,
        pm.NombreCompleto AS ProfesionalNombre
    FROM Usuario u
        INNER JOIN RolUsuario r
            ON r.Id = u.RolId
        LEFT JOIN ProfesionalMedico pm
            ON pm.Id = u.ProfesionalMedicoId
    WHERE
        u.Id = @UsuarioId;
END
GO

-- Obtiene un usuario por Id o por correo (uno de los dos, no ambos obligatorios).
CREATE OR ALTER PROCEDURE sp_usuario_obtener
(
    @Id INT = NULL,
    @CorreoElectronico NVARCHAR(200) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        u.Id,
        u.Identificacion,
        u.NombreCompleto,
        u.CorreoElectronico,
        u.Telefono,
        u.FechaNacimiento,
        u.Estado,
        u.RolId,
        r.Nombre AS RolNombre,
        u.ProfesionalMedicoId,
        pm.NombreCompleto AS ProfesionalNombre
    FROM Usuario u
        INNER JOIN RolUsuario r
            ON r.Id = u.RolId
        LEFT JOIN ProfesionalMedico pm
            ON pm.Id = u.ProfesionalMedicoId
    WHERE
        (@Id IS NOT NULL AND u.Id = @Id)
        OR
        (@CorreoElectronico IS NOT NULL AND u.CorreoElectronico = @CorreoElectronico);
END
GO

-- Lista usuarios filtrando por estado activo/inactivo.
CREATE OR ALTER PROCEDURE sp_usuarios_lista
(
    @Activo BIT = NULL,
    @Texto NVARCHAR(200) = NULL,
    @RolId TINYINT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.Id,
        u.Identificacion,
        u.NombreCompleto,
        u.CorreoElectronico,
        u.Telefono,
        u.FechaNacimiento,
        u.RolId,
        u.Estado,
        r.Nombre AS RolNombre,
        u.ProfesionalMedicoId,
        pm.NombreCompleto AS ProfesionalNombre
    FROM Usuario u
        INNER JOIN RolUsuario r
            ON r.Id = u.RolId
        LEFT JOIN ProfesionalMedico pm
            ON pm.Id = u.ProfesionalMedicoId
    WHERE
        (@Activo IS NULL OR u.Estado = @Activo)
        AND
        (
            @Texto IS NULL
            OR @Texto = ''
            OR u.NombreCompleto LIKE '%' + @Texto + '%'
            OR u.CorreoElectronico LIKE '%' + @Texto + '%'
            OR u.Identificacion LIKE '%' + @Texto + '%'
        )
        AND
        (
            @RolId IS NULL
            OR u.RolId = @RolId
        )
     ORDER BY
        r.Id,
        u.NombreCompleto;
END
GO

-- Actualiza el hash de contraseña de un usuario (login normal o reseteo).
CREATE OR ALTER PROCEDURE sp_actualizar_contrasena
(
    @UsuarioId INT,
    @PasswordHash NVARCHAR(300),
    @TemporaryPassword BIT,
    @FechaExpiracionPasswordTemporal DATETIME2 = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM Usuario
        WHERE Id = @UsuarioId
    )
    BEGIN
        SELECT 0 AS Resultado;
        RETURN;
    END

    UPDATE Usuario
    SET
        PasswordHash = @PasswordHash,
        TemporaryPassword = @TemporaryPassword,
        FechaExpiracionPasswordTemporal = @FechaExpiracionPasswordTemporal
    WHERE
        Id = @UsuarioId;

    SELECT 1 AS Resultado;
END
GO


/* =========================================================
   PROFESIONALES MÉDICOS
   ========================================================= */

-- Busca profesionales activos por texto libre y/o especialidad.
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

-- Obtiene el detalle de un profesional activo por Id.
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

-- Lista el catálogo de especialidades médicas activas.
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

-- Genera los bloques de disponibilidad de un profesional entre dos fechas.
-- NOTA: los bloques se generan en incrementos de 40 minutos, pero
-- sp_crear_cita/sp_modificar_cita reservan bloques de 39 minutos.
-- Conviene unificar ese número en un solo lugar.
CREATE OR ALTER PROCEDURE sp_disponibilidad_profesional
(
    @ProfesionalMedicoId INT,
    @FechaInicio DATE,
    @FechaFin DATE
)
AS
BEGIN
    SET NOCOUNT ON;
    SET DATEFIRST 1;

    CREATE TABLE #Disponibilidad
    (
        Fecha DATE NOT NULL,
        DiaSemana INT NOT NULL,
        HoraInicio TIME NOT NULL,
        HoraFin TIME NOT NULL,
        Disponible BIT NOT NULL
    );

    -- Carga las citas activas dentro del rango solicitado.
    CREATE TABLE #Citas
    (
        FechaHoraInicio DATETIME NOT NULL,
        FechaHoraFin DATETIME NOT NULL
    );

    INSERT INTO #Citas
    (
        FechaHoraInicio,
        FechaHoraFin
    )
    SELECT
        FechaHoraInicio,
        FechaHoraFin
    FROM CitaMedica
    WHERE ProfesionalMedicoId = @ProfesionalMedicoId
      AND EstadoCita IN (1, 2)
      AND FechaHoraInicio < DATEADD(DAY, 1, CAST(@FechaFin AS DATETIME))
      AND FechaHoraFin > CAST(@FechaInicio AS DATETIME);

    CREATE INDEX IX_Citas_FechaHora
        ON #Citas (FechaHoraInicio, FechaHoraFin);

    DECLARE @FechaActual DATE = @FechaInicio;
    DECLARE @HoraInicio TIME;
    DECLARE @HoraFin TIME;
    DECLARE @HoraActual TIME;
    DECLARE @SlotInicio DATETIME;
    DECLARE @SlotFin DATETIME;

    WHILE @FechaActual <= @FechaFin
    BEGIN

        -- Reinicia el horario para evitar reutilizar el del día anterior.
        SET @HoraInicio = NULL;
        SET @HoraFin = NULL;

        SELECT TOP 1
            @HoraInicio = HoraInicio,
            @HoraFin = HoraFin
        FROM HorarioProfesional
        WHERE ProfesionalMedicoId = @ProfesionalMedicoId
          AND DiaSemana = DATEPART(WEEKDAY, @FechaActual)
          AND Estado = 1
        ORDER BY HoraInicio;

        -- Genera bloques únicamente cuando el profesional tiene horario.
        IF @HoraInicio IS NOT NULL
           AND @HoraFin IS NOT NULL
           AND @HoraInicio < @HoraFin
        BEGIN

            SET @HoraActual = @HoraInicio;

            WHILE DATEADD(MINUTE, 40, @HoraActual) <= @HoraFin
            BEGIN

                -- Construye el rango del bloque para validar traslapes.
                SET @SlotInicio =
                    DATEADD(
                        MINUTE,
                        DATEDIFF(MINUTE, CAST('00:00:00' AS TIME), @HoraActual),
                        CAST(@FechaActual AS DATETIME)
                    );

                SET @SlotFin =
                    DATEADD(MINUTE, 40, @SlotInicio);

                INSERT INTO #Disponibilidad
                (
                    Fecha,
                    DiaSemana,
                    HoraInicio,
                    HoraFin,
                    Disponible
                )
                VALUES
                (
                    @FechaActual,
                    DATEPART(WEEKDAY, @FechaActual),
                    @HoraActual,
                    DATEADD(MINUTE, 40, @HoraActual),
                    CASE
                        WHEN EXISTS
                        (
                            SELECT 1
                            FROM #Citas C
                            WHERE C.FechaHoraInicio < @SlotFin
                              AND C.FechaHoraFin > @SlotInicio
                        )
                        THEN 0
                        ELSE 1
                    END
                );

                SET @HoraActual =
                    DATEADD(MINUTE, 40, @HoraActual);

            END
        END

        SET @FechaActual =
            DATEADD(DAY, 1, @FechaActual);

    END;

    SELECT
        Fecha,
        DiaSemana,
        HoraInicio,
        HoraFin,
        Disponible
    FROM #Disponibilidad
    ORDER BY
        Fecha,
        HoraInicio;

END
GO


/* =========================================================
   CITAS (paciente)
   ========================================================= */

-- Obtiene el detalle de una cita por Id.
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

-- Crea una cita y notifica al usuario-doctor asociado al profesional.
-- NOTA: NO valida contra HorarioProfesional (solo contra otras citas
-- activas), a diferencia de sp_modificar_cita que sí lo hace. Vale la
-- pena decidir si se agrega esa validación aquí también.
CREATE OR ALTER PROCEDURE sp_crear_cita
(
    @UsuarioId INT,
    @ProfesionalMedicoId INT,
    @FechaHoraInicio DATETIME2,
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
    SET DATEFIRST 1;

        IF NOT EXISTS (
            SELECT 1
            FROM Usuario
            WHERE Id = @UsuarioId
              AND Estado = 1
        )
            THROW 50001, 'Usuario invalido.', 1;

        DECLARE @FechaHoraFin DATETIME2 =
            DATEADD(MINUTE, 40, @FechaHoraInicio);

        IF EXISTS (
            SELECT 1
            FROM CitaMedica
            WHERE ProfesionalMedicoId = @ProfesionalMedicoId
              AND EstadoCita IN (1, 2)
              AND FechaHoraInicio < @FechaHoraFin
              AND FechaHoraFin > @FechaHoraInicio
        )
            THROW 50004, 'Ya existe una cita activa para ese profesional en ese horario.', 1;

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

        DECLARE @Id INT = CAST(SCOPE_IDENTITY() AS INT);

        DECLARE @UsuarioDoctorId INT;

        SELECT TOP (1)
            @UsuarioDoctorId = Id
        FROM Usuario
        WHERE
            ProfesionalMedicoId = @ProfesionalMedicoId
            AND RolId = 2
            AND Estado = 1;

        IF @UsuarioDoctorId IS NOT NULL
        BEGIN
            INSERT INTO Notificacion
            (
                UsuarioId,
                CitaMedicaId,
                Titulo,
                Mensaje,
                Leida
            )
            VALUES
            (
                @UsuarioDoctorId,
                @Id,
                N'Nueva cita pendiente',
                N'Tiene una nueva cita pendiente de revisar y aprobar.',
                0
            );
        END

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
            c.FechaNacimientoPaciente,
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
END
GO

-- Reprograma una cita existente, validando horario del profesional y disponibilidad.
CREATE OR ALTER PROCEDURE sp_modificar_cita
(
    @Id INT,
    @UsuarioId INT,
    @FechaHoraInicio DATETIME2
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET DATEFIRST 1;

        -- La cita tiene una duración fija de 40 minutos.
        DECLARE @FechaHoraFin DATETIME2 =
            DATEADD(MINUTE, 40, @FechaHoraInicio);

        DECLARE @ProfesionalMedicoId INT;

        SELECT @ProfesionalMedicoId = ProfesionalMedicoId
        FROM CitaMedica
        WHERE Id = @Id
          AND UsuarioId = @UsuarioId
          AND EstadoCita IN (1, 2);

        IF @ProfesionalMedicoId IS NULL
            THROW 50005, 'Cita no encontrada, no le pertenece, o ya no se puede modificar.', 1;

        DECLARE @DiaSemana TINYINT = DATEPART(WEEKDAY, @FechaHoraInicio);
        DECLARE @HoraInicioSolicitada TIME = CAST(@FechaHoraInicio AS TIME);
        DECLARE @HoraFinSolicitada TIME = CAST(@FechaHoraFin AS TIME);

        IF NOT EXISTS (
            SELECT 1
            FROM HorarioProfesional
            WHERE ProfesionalMedicoId = @ProfesionalMedicoId
              AND DiaSemana = @DiaSemana
              AND Estado = 1
              AND HoraInicio <= @HoraInicioSolicitada
        )
            THROW 50003, 'El profesional no atiende en ese día/horario.', 1;

        IF EXISTS (
            SELECT 1
            FROM CitaMedica
            WHERE ProfesionalMedicoId = @ProfesionalMedicoId
              AND EstadoCita IN (1, 2)
              AND Id <> @Id
              AND FechaHoraInicio < @FechaHoraFin
              AND FechaHoraFin > @FechaHoraInicio
        )
            THROW 50004, 'Ya existe una cita activa para ese profesional en ese horario.', 1;

        UPDATE CitaMedica
        SET FechaHoraInicio = @FechaHoraInicio,
            FechaHoraFin = @FechaHoraFin
        WHERE Id = @Id;

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
        WHERE c.Id = @Id;
END
GO

-- Cancela una cita del paciente y notifica al doctor asociado.
CREATE OR ALTER PROCEDURE sp_cancelar_cita
(
    @Id INT,
    @UsuarioId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Resultado INT = 0;
    DECLARE @ProfesionalMedicoId INT;
    DECLARE @UsuarioDoctorId INT;
    DECLARE @NombrePaciente NVARCHAR(200);
    DECLARE @FechaHoraInicio DATETIME2;

    UPDATE CitaMedica
    SET EstadoCita = 3
    WHERE Id = @Id
      AND UsuarioId = @UsuarioId
      AND EstadoCita IN (1, 2);

    SET @Resultado = @@ROWCOUNT;

    IF @Resultado > 0
    BEGIN
        SELECT
            @ProfesionalMedicoId = ProfesionalMedicoId,
            @NombrePaciente = NombrePaciente,
            @FechaHoraInicio = FechaHoraInicio
        FROM CitaMedica
        WHERE Id = @Id;

        SELECT TOP (1)
            @UsuarioDoctorId = Id
        FROM Usuario
        WHERE ProfesionalMedicoId = @ProfesionalMedicoId
          AND RolId = 2
          AND Estado = 1;

        IF @UsuarioDoctorId IS NOT NULL
        BEGIN
            INSERT INTO Notificacion
            (
                UsuarioId,
                CitaMedicaId,
                Titulo,
                Mensaje,
                Leida
            )
            VALUES
            (
                @UsuarioDoctorId,
                @Id,
                N'Cita cancelada por paciente',
                CONCAT(
                    N'El paciente ',
                    ISNULL(@NombrePaciente, N''),
                    N' canceló una cita programada para el ',
                    FORMAT(@FechaHoraInicio, 'dd/MM/yyyy HH:mm'),
                    N'.'
                ),
                0
            );
        END
    END
END
GO


/* =========================================================
   NOTIFICACIONES
   ========================================================= */

-- Lista notificaciones de un usuario (todas o solo no leídas).
CREATE OR ALTER PROCEDURE sp_notificaciones_usuario
(
    @UsuarioId INT,
    @SoloPendientes BIT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        UsuarioId,
        CitaMedicaId,
        Titulo,
        Mensaje,
        Leida,
        FechaCreacion
    FROM Notificacion
    WHERE
        UsuarioId = @UsuarioId
        AND
        (
            @SoloPendientes = 0
            OR Leida = 0
        )
    ORDER BY
        FechaCreacion DESC;
END
GO

-- Marca una notificación puntual como leída.
CREATE OR ALTER PROCEDURE sp_notificacion_marcar_leida
(
    @UsuarioId INT,
    @NotificacionId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Notificacion
    SET Leida = 1
    WHERE
        Id = @NotificacionId
        AND UsuarioId = @UsuarioId;

    SELECT @@ROWCOUNT AS Resultado;
END
GO


/* =========================================================
   DOCTOR
   ========================================================= */

-- Métricas de resumen para el dashboard del doctor autenticado.
CREATE OR ALTER PROCEDURE sp_medico_dashboard
(
    @UsuarioId INT,
    @ProfesionalMedicoId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(1)
         FROM CitaMedica
         WHERE ProfesionalMedicoId = @ProfesionalMedicoId
           AND EstadoCita = 1) AS CitasPendientesAprobar,

        (SELECT COUNT(1)
         FROM CitaMedica
         WHERE ProfesionalMedicoId = @ProfesionalMedicoId
           AND EstadoCita = 2) AS CitasAprobadas,

        (SELECT COUNT(1)
         FROM CitaMedica
         WHERE ProfesionalMedicoId = @ProfesionalMedicoId
           AND EstadoCita = 3) AS CitasCanceladas,

        (SELECT COUNT(1)
         FROM CitaMedica
         WHERE ProfesionalMedicoId = @ProfesionalMedicoId
           AND EstadoCita = 4) AS CitasFinalizadas,

        (SELECT COUNT(1)
         FROM CitaMedica
         WHERE ProfesionalMedicoId = @ProfesionalMedicoId
           AND CAST(FechaHoraInicio AS DATE) = CAST(SYSUTCDATETIME() AS DATE)
           AND EstadoCita IN (1,2)) AS CitasHoy,

        (SELECT COUNT(1)
         FROM Notificacion
         WHERE UsuarioId = @UsuarioId
           AND Leida = 0) AS NotificacionesPendientes;
END
GO

/* =========================================================
   ADMINISTRACIÓN
   ========================================================= */

-- Métricas globales para el dashboard del SuperAdmin.
CREATE OR ALTER PROCEDURE sp_admin_dashboard
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(1) FROM Usuario WHERE Estado = 1) AS UsuariosRegistrados,
        (SELECT COUNT(1) FROM Usuario WHERE Estado = 1 AND RolId = 1) AS Administradores,
        (SELECT COUNT(1) FROM Usuario WHERE Estado = 1 AND RolId = 2) AS UsuariosDoctores,
        (SELECT COUNT(1) FROM ProfesionalMedico WHERE Estado = 1) AS Doctores,
        (SELECT COUNT(1) FROM EspecialidadMedica WHERE Estado = 1) AS Especialidades,
        (SELECT COUNT(1) FROM CitaMedica) AS CitasAgendadas,
        (SELECT COUNT(1) FROM CitaMedica WHERE EstadoCita = 1) AS CitasPendientes,
        (SELECT COUNT(1) FROM CitaMedica WHERE EstadoCita = 2) AS CitasAprobadas,
        (SELECT COUNT(1) FROM CitaMedica WHERE EstadoCita = 3) AS CitasCanceladas,
        (SELECT COUNT(1) FROM CitaMedica WHERE EstadoCita = 4) AS CitasFinalizadas;
END
GO

-- Actualiza perfil completo + rol de un usuario (y sincroniza datos del
CREATE OR ALTER PROCEDURE sp_admin_actualizar_usuario
(
    @UsuarioId INT,
    @Identificacion NVARCHAR(20),
    @NombreCompleto NVARCHAR(200),
    @CorreoElectronico NVARCHAR(200),
    @Telefono NVARCHAR(20),
    @FechaNacimiento DATE,
    @RolId TINYINT,
    @ProfesionalMedicoId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM Usuario
        WHERE Id = @UsuarioId
    )
        THROW 55001, 'El usuario no existe.', 1;

    IF EXISTS (
        SELECT 1
        FROM Usuario
        WHERE Identificacion = @Identificacion
          AND Id <> @UsuarioId
    )
        THROW 55002, 'La identificación ya está registrada por otro usuario.', 1;

    IF EXISTS (
        SELECT 1
        FROM Usuario
        WHERE CorreoElectronico = @CorreoElectronico
          AND Id <> @UsuarioId
    )
        THROW 55003, 'El correo electrónico ya está registrado por otro usuario.', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM RolUsuario
        WHERE Id = @RolId
          AND Estado = 1
    )
        THROW 55004, 'El rol seleccionado no existe.', 1;

    IF @RolId = 2 AND @ProfesionalMedicoId IS NULL
        THROW 55005, 'Para asignar el rol Doctor debe seleccionar un profesional médico.', 1;

    IF @RolId <> 2
        SET @ProfesionalMedicoId = NULL;

    IF @ProfesionalMedicoId IS NOT NULL
       AND EXISTS (
            SELECT 1
            FROM Usuario
            WHERE ProfesionalMedicoId = @ProfesionalMedicoId
              AND Id <> @UsuarioId
       )
        THROW 55006, 'Ese profesional médico ya está asociado a otro usuario.', 1;

    UPDATE Usuario
    SET
        Identificacion = @Identificacion,
        NombreCompleto = @NombreCompleto,
        CorreoElectronico = @CorreoElectronico,
        Telefono = @Telefono,
        FechaNacimiento = @FechaNacimiento,
        RolId = @RolId,
        ProfesionalMedicoId = @ProfesionalMedicoId
    WHERE
        Id = @UsuarioId;

    IF @ProfesionalMedicoId IS NOT NULL
    BEGIN
        UPDATE ProfesionalMedico
        SET
            NombreCompleto = @NombreCompleto,
            CorreoElectronico = @CorreoElectronico,
            Telefono = @Telefono,
            FechaNacimiento = @FechaNacimiento
        WHERE
            Id = @ProfesionalMedicoId;
    END

    SELECT @@ROWCOUNT AS Resultado;
END
GO

-- Cambia solo el rol (y profesional asociado) de un usuario.
-- NOTA: subconjunto de sp_admin_actualizar_usuario; candidato a eliminarse
-- si la UI siempre puede llamar al SP completo.
CREATE OR ALTER PROCEDURE sp_admin_actualizar_rol_usuario
(
    @UsuarioId INT,
    @RolId TINYINT,
    @ProfesionalMedicoId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM RolUsuario
        WHERE Id = @RolId
          AND Estado = 1
    )
        THROW 51001, 'El rol seleccionado no existe.', 1;

    IF @RolId = 2 AND @ProfesionalMedicoId IS NULL
        THROW 51002, 'Para asignar el rol Doctor debe seleccionar un profesional médico.', 1;

    IF @RolId <> 2
        SET @ProfesionalMedicoId = NULL;

    IF @ProfesionalMedicoId IS NOT NULL
       AND EXISTS (
            SELECT 1
            FROM Usuario
            WHERE ProfesionalMedicoId = @ProfesionalMedicoId
              AND Id <> @UsuarioId
       )
        THROW 51003, 'Ese profesional médico ya está asociado a otro usuario.', 1;

    UPDATE Usuario
    SET
        RolId = @RolId,
        ProfesionalMedicoId = @ProfesionalMedicoId
    WHERE
        Id = @UsuarioId
        AND Estado = 1;

    SELECT @@ROWCOUNT AS Resultado;
END
GO

-- Activa o desactiva (soft-delete) un usuario.
CREATE OR ALTER PROCEDURE sp_admin_cambiar_estado_usuario
(
    @UsuarioId INT,
    @Estado BIT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM Usuario
        WHERE Id = @UsuarioId
    )
        THROW 55007, 'El usuario no existe.', 1;

    UPDATE Usuario
    SET Estado = @Estado
    WHERE Id = @UsuarioId;

    SELECT @@ROWCOUNT AS Resultado;
END
GO

-- Lista todos los profesionales médicos e indica si ya tienen usuario ligado.
CREATE OR ALTER PROCEDURE sp_admin_doctores_lista
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
        pm.Estado,
        u.Id AS UsuarioId,
        u.CorreoElectronico AS CorreoUsuario,
        CASE
            WHEN u.Id IS NULL THEN 0
            ELSE 1
        END AS TieneUsuario
    FROM ProfesionalMedico pm
        LEFT JOIN Usuario u
            ON u.ProfesionalMedicoId = pm.Id
    ORDER BY
        pm.NombreCompleto;
END
GO

-- Lista todas las citas del sistema para el panel admin (filtro por estado y/o texto).
CREATE OR ALTER PROCEDURE sp_citas_lista
(
    @UsuarioId INT = NULL,
    @ProfesionalMedicoId INT = NULL,
    @EstadoCita TINYINT = NULL,
    @Texto NVARCHAR(200) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
 
    SELECT
        c.Id,
        c.UsuarioId,
        u.NombreCompleto AS UsuarioRegistro,
        u.CorreoElectronico AS CorreoUsuarioRegistro,
        c.ProfesionalMedicoId,
        pm.NombreCompleto AS ProfesionalMedico,
        pm.CorreoElectronico AS CorreoProfesional,
        pm.Telefono AS TelefonoProfesional,
        c.FechaHoraInicio,
        c.FechaHoraFin,
        c.NombrePaciente,
        c.IdentificacionPaciente,
        c.FechaNacimientoPaciente,
        c.CorreoPaciente,
        c.TelefonoPaciente,
        c.Motivo,
        c.EstadoCita,
        c.FechaCreacion
    FROM CitaMedica c
        INNER JOIN Usuario u
            ON u.Id = c.UsuarioId
        INNER JOIN ProfesionalMedico pm
            ON pm.Id = c.ProfesionalMedicoId
    WHERE
        (@UsuarioId IS NULL OR c.UsuarioId = @UsuarioId)
        AND (@ProfesionalMedicoId IS NULL OR c.ProfesionalMedicoId = @ProfesionalMedicoId)
        AND (@EstadoCita IS NULL OR c.EstadoCita = @EstadoCita)
        AND (
            @Texto IS NULL
            OR @Texto = ''
            OR c.NombrePaciente LIKE '%' + @Texto + '%'
            OR c.IdentificacionPaciente LIKE '%' + @Texto + '%'
            OR c.CorreoPaciente LIKE '%' + @Texto + '%'
            OR pm.NombreCompleto LIKE '%' + @Texto + '%'
            OR u.NombreCompleto LIKE '%' + @Texto + '%'
        )
    ORDER BY
        c.EstadoCita ASC,
        c.FechaHoraInicio ASC;
END
GO

-- Permite fijar estado de cita (sin restricción de transición) y notifica al paciente.
CREATE OR ALTER PROCEDURE sp_actualizar_estado_cita
(
    @CitaId INT,
    @EstadoCita TINYINT,
    @ProfesionalMedicoId INT = NULL   -- NULL = llamada de admin; con valor = llamada de doctor (ownership + transición restringida)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @ProfesionalMedicoId IS NOT NULL AND @EstadoCita NOT IN (2, 3, 4)
        THROW 52001, 'Estado de cita no permitido para el doctor.', 1;

    IF @ProfesionalMedicoId IS NULL AND @EstadoCita NOT IN (1, 2, 3, 4)
        THROW 54001, 'Estado de cita no permitido.', 1;

    UPDATE CitaMedica
    SET EstadoCita = @EstadoCita
    WHERE
        Id = @CitaId
        AND (@ProfesionalMedicoId IS NULL OR ProfesionalMedicoId = @ProfesionalMedicoId)
        AND (
            @ProfesionalMedicoId IS NULL   -- admin: sin restricción de transición
            OR (@EstadoCita = 2 AND EstadoCita = 1)
            OR (@EstadoCita = 3 AND EstadoCita IN (1, 2))
            OR (@EstadoCita = 4 AND EstadoCita = 2)
        );

    DECLARE @Resultado INT = @@ROWCOUNT;

    IF @Resultado > 0
    BEGIN
        DECLARE @UsuarioPacienteId INT, @Titulo NVARCHAR(150), @Mensaje NVARCHAR(500);
        DECLARE @Origen NVARCHAR(20) = CASE WHEN @ProfesionalMedicoId IS NULL THEN N'administración' ELSE N'el profesional' END;

        SELECT @UsuarioPacienteId = UsuarioId FROM CitaMedica WHERE Id = @CitaId;

        SET @Titulo = CASE @EstadoCita
            WHEN 1 THEN N'Cita pendiente' WHEN 2 THEN N'Cita aprobada'
            WHEN 3 THEN N'Cita cancelada' WHEN 4 THEN N'Cita finalizada' END;

        SET @Mensaje = CONCAT(
            N'Su cita médica fue ',
            CASE @EstadoCita
                WHEN 1 THEN N'marcada como pendiente' WHEN 2 THEN N'aprobada'
                WHEN 3 THEN N'cancelada' WHEN 4 THEN N'marcada como finalizada' END,
            N' por ', @Origen, N'.'
        );

        IF @UsuarioPacienteId IS NOT NULL
            INSERT INTO Notificacion (UsuarioId, CitaMedicaId, Titulo, Mensaje, Leida)
            VALUES (@UsuarioPacienteId, @CitaId, @Titulo, @Mensaje, 0);
    END

    SELECT @Resultado AS Resultado;
END
GO

/* =========================================================
   SETUP / BOOTSTRAP
   ========================================================= */

-- Crea (o restaura) el usuario SuperAdmin inicial del sistema.
CREATE OR ALTER PROCEDURE sp_setup_crear_superadmin
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

    IF EXISTS (
        SELECT 1
        FROM Usuario
        WHERE CorreoElectronico = @CorreoElectronico
    )
    BEGIN
        UPDATE Usuario
        SET
            RolId = 1,
            ProfesionalMedicoId = NULL,
            PasswordHash = @PasswordHash,
            TemporaryPassword = 0,
            FechaExpiracionPasswordTemporal = NULL,
            Estado = 1
        WHERE CorreoElectronico = @CorreoElectronico;
    END
    ELSE
    BEGIN
        INSERT INTO Usuario
        (
            Identificacion,
            NombreCompleto,
            CorreoElectronico,
            Telefono,
            FechaNacimiento,
            PasswordHash,
            TemporaryPassword,
            FechaExpiracionPasswordTemporal,
            Estado,
            RolId,
            ProfesionalMedicoId
        )
        VALUES
        (
            @Identificacion,
            @NombreCompleto,
            @CorreoElectronico,
            @Telefono,
            @FechaNacimiento,
            @PasswordHash,
            0,
            NULL,
            1,
            1,
            NULL
        );
    END

    SELECT TOP (1)
        Id,
        Identificacion,
        NombreCompleto,
        CorreoElectronico,
        Telefono,
        FechaNacimiento,
        RolId,
        ProfesionalMedicoId
    FROM Usuario
    WHERE CorreoElectronico = @CorreoElectronico;
END
GO

-- Crea (o vincula) el usuario de acceso para un profesional médico existente.
CREATE OR ALTER PROCEDURE sp_setup_crear_usuario_doctor
(
    @ProfesionalMedicoId INT,
    @PasswordHash NVARCHAR(300)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NombreCompleto NVARCHAR(200);
    DECLARE @CorreoElectronico NVARCHAR(200);
    DECLARE @Telefono NVARCHAR(20);
    DECLARE @FechaNacimiento DATE;
    DECLARE @Identificacion NVARCHAR(20);

    SELECT
        @NombreCompleto = NombreCompleto,
        @CorreoElectronico = CorreoElectronico,
        @Telefono = Telefono,
        @FechaNacimiento = FechaNacimiento,
        @Identificacion = CONCAT('DOC', RIGHT(CONCAT('000000', Id), 6))
    FROM ProfesionalMedico
    WHERE
        Id = @ProfesionalMedicoId
        AND Estado = 1;

    IF @CorreoElectronico IS NULL
        THROW 53001, 'El profesional médico no existe o está inactivo.', 1;

    IF EXISTS (
        SELECT 1
        FROM Usuario
        WHERE CorreoElectronico = @CorreoElectronico
    )
    BEGIN
        UPDATE Usuario
        SET
            NombreCompleto = @NombreCompleto,
            Telefono = @Telefono,
            FechaNacimiento = @FechaNacimiento,
            PasswordHash = @PasswordHash,
            TemporaryPassword = 0,
            FechaExpiracionPasswordTemporal = NULL,
            Estado = 1,
            RolId = 2,
            ProfesionalMedicoId = @ProfesionalMedicoId
        WHERE CorreoElectronico = @CorreoElectronico;
    END
    ELSE
    BEGIN
        WHILE EXISTS (
            SELECT 1
            FROM Usuario
            WHERE Identificacion = @Identificacion
        )
        BEGIN
            SET @Identificacion = CONCAT('DOC', RIGHT(CONCAT('000000', ABS(CHECKSUM(NEWID()))), 6));
        END

        INSERT INTO Usuario
        (
            Identificacion,
            NombreCompleto,
            CorreoElectronico,
            Telefono,
            FechaNacimiento,
            PasswordHash,
            TemporaryPassword,
            FechaExpiracionPasswordTemporal,
            Estado,
            RolId,
            ProfesionalMedicoId
        )
        VALUES
        (
            @Identificacion,
            @NombreCompleto,
            @CorreoElectronico,
            @Telefono,
            @FechaNacimiento,
            @PasswordHash,
            0,
            NULL,
            1,
            2,
            @ProfesionalMedicoId
        );
    END

    SELECT TOP (1)
        Id,
        Identificacion,
        NombreCompleto,
        CorreoElectronico,
        Telefono,
        FechaNacimiento,
        RolId,
        ProfesionalMedicoId
    FROM Usuario
    WHERE CorreoElectronico = @CorreoElectronico;
END
GO