namespace PA_API.Constants
{
    public static class StoreProceduresConstants
    {
        public const string sp_crear_cita = "sp_crear_cita";
        public const string sp_obtener_cita = "sp_obtener_cita";
        public const string sp_obtener_citas = "sp_obtener_citas";
        public const string sp_modificar_cita = "sp_modificar_cita";
        public const string sp_cancelar_cita = "sp_cancelar_cita";

        public const string sp_profesional_buscar = "sp_profesional_buscar";
        public const string sp_obtener_profesional_por_id = "sp_obtener_profesional_por_id";
        public const string sp_disponibilidad_profesional = "sp_disponibilidad_profesional";

        public const string sp_usuarios_lista = "sp_usuarios_lista";
        public const string sp_usuario_obtener = "sp_usuario_obtener";
        public const string sp_usuario_iniciar_sesion = "sp_usuario_iniciar_sesion";
        public const string sp_usuario_registrar = "sp_usuario_registrar";
        public const string sp_actualizar_contrasena = "sp_actualizar_contrasena";

        public const string sp_admin_dashboard = "sp_admin_dashboard";
        public const string sp_admin_usuarios_lista = "sp_admin_usuarios_lista";
        public const string sp_admin_doctores_lista = "sp_admin_doctores_lista";
        public const string sp_admin_actualizar_rol_usuario = "sp_admin_actualizar_rol_usuario";

        public const string sp_admin_usuario_obtener = "sp_admin_usuario_obtener";
        public const string sp_admin_actualizar_usuario = "sp_admin_actualizar_usuario";
        public const string sp_admin_cambiar_estado_usuario = "sp_admin_cambiar_estado_usuario";

        public const string sp_admin_citas_lista = "sp_admin_citas_lista";
        public const string sp_admin_actualizar_estado_cita = "sp_admin_actualizar_estado_cita";

        public const string sp_medico_dashboard = "sp_medico_dashboard";
        public const string sp_medico_citas = "sp_medico_citas";
        public const string sp_medico_actualizar_estado_cita = "sp_medico_actualizar_estado_cita";

        public const string sp_notificaciones_usuario = "sp_notificaciones_usuario";
        public const string sp_notificacion_marcar_leida = "sp_notificacion_marcar_leida";

        public const string sp_setup_crear_superadmin = "sp_setup_crear_superadmin";
        public const string sp_setup_crear_usuario_doctor = "sp_setup_crear_usuario_doctor";
    }
}