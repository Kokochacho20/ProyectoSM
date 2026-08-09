using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PA_API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<IUsuarioService, UsuarioService>();
builder.Services.AddTransient<ICitaMedicaService, CitaMedicaService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<IProfesionalService, ProfesionalService>();
builder.Services.AddTransient<IEspecialidadesService, EspecialidadesService>();
builder.Services.AddScoped<IUtilesService, UtilesService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();