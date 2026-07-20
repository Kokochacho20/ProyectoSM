
using PA_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "MediCitas API",
        Version = "v1",
        Description = "API para la gestión de usuarios y citas médicas."
    });
});

builder.Services.AddTransient<IUsuarioService, UsuarioService>();
builder.Services.AddTransient<ICitaMedicaService, CitaMedicaService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<IProfesionalService, ProfesionalService>();
builder.Services.AddTransient<IEspecialidadesService, EspecialidadesService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.Run();