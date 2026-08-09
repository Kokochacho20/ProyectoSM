document.addEventListener("DOMContentLoaded", function () {
    const checkOtraPersona = document.getElementById("esParaOtraPersona");

    if (!checkOtraPersona) {
        return;
    }

    const camposPaciente = [
        "NombrePaciente",
        "IdentificacionPaciente",
        "FechaNacimientoPaciente",
        "CorreoPaciente",
        "TelefonoPaciente"
    ];

    checkOtraPersona.addEventListener("change", function () {
        camposPaciente.forEach(function (id) {
            const campo = document.getElementById(id);

            if (!campo) {
                return;
            }

            if (checkOtraPersona.checked) {
                campo.value = "";
            } else {
                campo.value = campo.defaultValue;
            }
        });
    });
});