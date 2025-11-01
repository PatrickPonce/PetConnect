// --- USINGs REQUERIDOS ---
using PetConnect.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Services;
using Microsoft.AspNetCore.HttpOverrides; 
using static AspNet.Security.OAuth.GitHub.GitHubAuthenticationConstants; // Este 'using' puede no ser estrictamente necesario, pero no hace daño

// ------------------------------------
// --- CONFIGURACIÓN DE SERVICIOS ---
// ------------------------------------

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Línea de depuración para ver la cadena original
Console.WriteLine($"--- DEBUG: Cadena de conexión ORIGINAL obtenida: '{connectionString}' ---");

// 3. Verifica si la cadena está en formato de URL (como en Render)
if (Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
{
    // Si es una URL, la "traducimos" al formato estándar
    var userInfo = uri.UserInfo.Split(':');
    var dbHost = uri.Host;
    var dbPort = uri.Port;
    var dbUser = userInfo[0];
    var dbPass = userInfo[1];
    var dbName = uri.LocalPath.TrimStart('/');

    // Construimos la nueva cadena de conexión en el formato que Npgsql espera
    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass};SSL Mode=Require;Trust Server Certificate=true;";
    
    Console.WriteLine($"--- DEBUG: Cadena de conexión TRADUCIDA para Npgsql: '{connectionString}' ---");
}
else if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("--- DEBUG: ¡¡ERROR!! La cadena de conexión está VACÍA o NULA.");
}

// 4. Usa la cadena de conexión (ya sea la original o la traducida) para configurar el DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configuración de Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// Configuración de Autenticación Externa (GitHub)
builder.Services.AddAuthentication()
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
        options.Scope.Add("user:email");
    });

// Otros servicios
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ConfiguracionSitioService>();
builder.Services.AddScoped<AnimalApiService>();

// Configuración de Google Maps API Key
var googleMapsApiKey = builder.Configuration["GoogleMaps:ApiKey"];
builder.Services.AddSingleton(new GoogleMapsConfig { ApiKey = googleMapsApiKey });


// ------------------------------------
// --- CONSTRUCCIÓN DE LA APP ---
// ------------------------------------

var app = builder.Build();


// ---------------------------------------------
// --- CONFIGURACIÓN DEL PIPELINE DE REQUEST ---
// ---------------------------------------------

// 1. Configuración para Proxies Inversos (Render) - DEBE IR PRIMERO
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// 2. Tareas de Inicialización (Migración, Roles, Datos Semilla) - Se ejecutan solo una vez al arrancar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Iniciando tareas de arranque: Migración y siembra de datos.");
        
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Tarea 2a: Aplicar migraciones pendientes
        context.Database.Migrate();
        logger.LogInformation("Migración de base de datos completada (si era necesaria).");
        
        // Tarea 2b: Crear Roles y Usuario Administrador
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await CrearRolesYAdmin(userManager, roleManager, logger);
        logger.LogInformation("Verificación de roles y admin completada.");
        
        // Tarea 2c: Sembrar datos iniciales (Noticias, etc.)
        DbInitializer.Initialize(context);
        logger.LogInformation("Siembra de datos iniciales completada.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error durante las tareas de inicialización.");
    }
}

// 3. Configuración del Pipeline para el Entorno
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 4. Middlewares estándar
app.UseHttpsRedirection();
// app.MapStaticAssets(); // MapStaticAssets es de una librería externa. Si no la usas, usa app.UseStaticFiles();
app.UseStaticFiles(); 
app.UseRouting();
app.UseAuthorization();

// 5. Mapeo de Endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
//   .WithStaticAssets(); // Quitar si no usas la librería de static assets

app.MapRazorPages();
//   .WithStaticAssets(); // Quitar si no usas la librería de static assets


// ------------------------------------
// --- EJECUCIÓN DE LA APP ---
// ------------------------------------

app.Run();


// ------------------------------------
// --- MÉTODOS AUXILIARES ---
// ------------------------------------

async Task CrearRolesYAdmin(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger)
{
    const string rolNombre = "Admin";
    if (!await roleManager.RoleExistsAsync(rolNombre))
    {
        await roleManager.CreateAsync(new IdentityRole(rolNombre));
        logger.LogInformation($"Rol '{rolNombre}' creado.");
    }

    const string adminEmail = "admin@petconnect.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if(result.Succeeded)
        {
            logger.LogInformation($"Usuario admin '{adminEmail}' creado.");
        }
    }

    if (!await userManager.IsInRoleAsync(adminUser, rolNombre))
    {
        await userManager.AddToRoleAsync(adminUser, rolNombre);
        logger.LogInformation($"Usuario admin '{adminEmail}' añadido al rol '{rolNombre}'.");
    }
}