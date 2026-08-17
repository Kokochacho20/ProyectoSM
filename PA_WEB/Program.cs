using Microsoft.AspNetCore.Mvc.Filters;
using PA_WEB.Filters;
using PA_WEB.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<ApiAuthorizationHandler>();

builder.Services
    .AddHttpClient("ApiClient", client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["Valores:UrlApi"]!);
    })
    .AddHttpMessageHandler<ApiAuthorizationHandler>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ApiAuthorizationFilter>();
});

builder.Services.AddTransient<ApiAuthorizationHandler>();
builder.Services.AddTransient<IProfesionalService, ProfesionalService>();
builder.Services.AddTransient<ICitasService, CitasService>();
builder.Services.AddTransient<IUsuarioService, UsuariosService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();