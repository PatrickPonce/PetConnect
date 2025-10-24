using PetConnect.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using PetConnect.Data;
using System; 
using PetConnect.Services;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------
// 1. AÑADIR SERVICIOS (builder.Services)
// -------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configuración de Identity (usa IdentityUser si NO tienes una clase personalizada)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

// Servicios personalizados
builder.Services.AddScoped<PetConnect.Services.ConfiguracionSitioService>();
var googleMapsApiKey = builder.Configuration["GoogleMaps:ApiKey"];
builder.Services.AddSingleton(new GoogleMapsConfig { ApiKey = googleMapsApiKey });

// -------------------------------------------------------------
// 2. CONSTRUIR LA APLICACIÓN
// -------------------------------------------------------------
var app = builder.Build();

// -------------------------------------------------------------
// 3. CONFIGURAR EL PIPELINE DE HTTP (app.Use...)
// -------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // <-- AÑADIDO: Habilita el uso de archivos en wwwroot (CSS, JS, imágenes)

app.UseRouting();

app.UseAuthorization(); // <-- Asegúrate de que esto esté después de UseRouting

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// -------------------------------------------------------------
// 4. SEMBRADO DE DATOS (Se ejecuta al arrancar)
// -------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // 1. Sembrar Noticias y Comentarios (si DbInitializer existe)
        var context = services.GetRequiredService<ApplicationDbContext>();
        // DbInitializer.Initialize(context); // Descomenta si tienes esta clase
        logger.LogInformation("DbInitializer ejecutado (si existía).");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error al sembrar la base de datos (DbInitializer).");
    }

    try
    {
        // 2. Crear Roles y Usuario Admin
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        // Llama a la función local CrearRolesYAdmin definida abajo
        await CrearRolesYAdmin(userManager, roleManager); 
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error al crear roles y el usuario admin.");
    }
}

// -------------------------------------------------------------
// 5. EJECUTAR LA APLICACIÓN
// -------------------------------------------------------------
app.Run();

// --- Función Local para crear el Admin (movida aquí abajo) ---
async Task CrearRolesYAdmin(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    string rolNombre = "Admin";
    // 1. Crear el Rol "Admin" si no existe
    if (!await roleManager.RoleExistsAsync(rolNombre))
    {
        await roleManager.CreateAsync(new IdentityRole(rolNombre));
    }

    // 2. Crear el Usuario Admin si no existe
    string adminEmail = "admin@petconnect.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true // Confirma el email automáticamente
        };
        // Crea el usuario con la contraseña
        await userManager.CreateAsync(adminUser, "Admin123!"); 
    }

    // 3. Asignar el Rol "Admin" al usuario si no lo tiene
    if (!await userManager.IsInRoleAsync(adminUser, rolNombre))
    {
        await userManager.AddToRoleAsync(adminUser, rolNombre);
    }
}