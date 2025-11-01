using PetConnect.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using PetConnect.Data;
using PetConnect.Services;
using static AspNet.Security.OAuth.GitHub.GitHubAuthenticationConstants;
using System; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication()
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
        options.Scope.Add("user:email");
    });

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<PetConnect.Services.ConfiguracionSitioService>();

var googleMapsApiKey = builder.Configuration["GoogleMaps:ApiKey"];


builder.Services.AddSingleton(new GoogleMapsConfig { ApiKey = googleMapsApiKey });

builder.Services.AddScoped<PetConnect.Services.AnimalApiService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Esto aplica cualquier migración pendiente a la base de datos.
        // Si la base de datos ya está actualizada, no hace nada.
        context.Database.Migrate(); 
        
        // El código para crear el admin ya lo tenías, lo mantenemos aquí
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await CrearRolesYAdmin(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al migrar la base de datos o crear roles.");
    }
}



using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 1. Obtener el contexto de la base de datos
        var context = services.GetRequiredService<ApplicationDbContext>();

        // 2. Ejecutar la inicialización de datos (inserta Noticias y Comentarios)
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        // 3. Si ocurre un error, loguearlo y continuar
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al sembrar la base de datos.");
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await CrearRolesYAdmin(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al crear roles y el usuario admin.");
    }
}

async Task CrearRolesYAdmin(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    string rolNombre = "Admin";
    if (!await roleManager.RoleExistsAsync(rolNombre))
    {
        await roleManager.CreateAsync(new IdentityRole(rolNombre));
    }

    string adminEmail = "admin@petconnect.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, "Admin123!");
    }

    if (!await userManager.IsInRoleAsync(adminUser, rolNombre))
    {
        await userManager.AddToRoleAsync(adminUser, rolNombre);
    }
}

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
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
