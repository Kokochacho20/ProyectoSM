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
        FechaExpiracionPasswordTemporal,
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
        c.EstadoCita ASC,
        c.FechaHoraInicio ASC;
END
GO


----------------------------------------------------------
-- Obtener cita por citaId
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
    SET DATEFIRST 1; -- 1 = Lunes ... 7 = Domingo (igual que el enum DiaSemana)

    BEGIN TRY

        IF NOT EXISTS (
            SELECT 1
            FROM Usuario
            WHERE Id = @UsuarioId
        )
            THROW 50001, 'Usuario invalido.', 1;

        DECLARE @DiaSemana TINYINT = DATEPART(WEEKDAY, @FechaHoraInicio);
        DECLARE @HoraInicioSolicitada TIME = CAST(@FechaHoraInicio AS TIME);
                -- La cita tiene duracion fija en esta version inicial
        DECLARE @FechaHoraFin DATETIME2 =
            DATEADD(MINUTE, 39, @FechaHoraInicio);

        DECLARE @HoraFinSolicitada TIME = CAST(@FechaHoraFin AS TIME);

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
    @UsuarioId INT,
    @PasswordHash NVARCHAR(300),
    @TemporaryPassword BIT,
    @FechaExpiracionPasswordTemporal DATETIME2 = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Usuario
    SET
        PasswordHash = @PasswordHash,
        TemporaryPassword = @TemporaryPassword,
        FechaExpiracionPasswordTemporal =
            CASE
                WHEN @TemporaryPassword = 1
                    THEN @FechaExpiracionPasswordTemporal
                ELSE NULL
            END
    WHERE
        Id = @UsuarioId
        AND Estado = 1
        AND
        (
            @TemporaryPassword = 1
            OR TemporaryPassword = 0
            OR
            (
                TemporaryPassword = 1
                AND FechaExpiracionPasswordTemporal IS NOT NULL
                AND FechaExpiracionPasswordTemporal >= SYSUTCDATETIME()
            )
        );

    SELECT @@ROWCOUNT;
END
GO

----------------------------------------------------------
-- Modificar cita (fecha/hora), validando disponibilidad
----------------------------------------------------------

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

    BEGIN TRY

        -- La cita tiene duracion fija en esta version inicial
        DECLARE @FechaHoraFin DATETIME2 =
            DATEADD(MINUTE, 39, @FechaHoraInicio);

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

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO


----------------------------------------------------------
-- Cancelar cita
----------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_cancelar_cita
(
    @Id INT,
    @UsuarioId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE CitaMedica
    SET EstadoCita = 3
    WHERE Id = @Id
      AND UsuarioId = @UsuarioId
      AND EstadoCita IN (1, 2);
END
GO


----------------------------------------------------------
-- Disponibilidad profesional medico 
----------------------------------------------------------

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

    /*
        Cargamos únicamente las citas del profesional
        dentro del rango solicitado.

        Así evitamos consultar CitaMedica una vez
        por cada bloque de 40 minutos.
    */
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

        /*
            MUY IMPORTANTE:
            Se limpian las variables antes de buscar el horario.

            Si este día no tiene horario, ambas quedan NULL
            y NO se reutiliza el horario del día anterior.
        */
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


        /*
            Solo generamos disponibilidad si existe
            un horario para ese día.
        */
        IF @HoraInicio IS NOT NULL
           AND @HoraFin IS NOT NULL
           AND @HoraInicio < @HoraFin
        BEGIN

            SET @HoraActual = @HoraInicio;

            WHILE DATEADD(MINUTE, 40, @HoraActual) <= @HoraFin
            BEGIN

                /*
                    Convertimos el bloque actual a DATETIME
                    para poder comprobar traslapes directamente
                    contra FechaHoraInicio / FechaHoraFin.
                */
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