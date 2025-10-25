using PetConnect.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using PetConnect.Data;
using PetConnect.Services;
using static AspNet.Security.OAuth.GitHub.GitHubAuthenticationConstants;
using PetConnect.Models; // <-- 1. ASEGÚRATE DE TENER ESTE 'USING'
using Microsoft.Extensions.Logging; // <-- Añadido para ILogger
using Microsoft.Extensions.DependencyInjection; // <-- Añadido para GetService

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

// --- SECCIÓN MODIFICADA ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await CrearRolesYAdmin(userManager, roleManager);

        var context = services.GetRequiredService<ApplicationDbContext>();
        // V--- PASAMOS 'services' AQUÍ ---V
        await SeedNoticias(context, services); 
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al crear roles/admin o inicializar noticias.");
    }
}
// --- FIN SECCIÓN MODIFICADA ---

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

// --- 4. FUNCIÓN SEEDNOTICIAS AÑADIDA ---
async Task SeedNoticias(ApplicationDbContext context, IServiceProvider services)
{
    // Verifica si ya hay noticias en la base de datos
    if (!context.Noticias.Any())
    {
        // Si no hay, crea la lista de noticias (sacada de tu controlador original)
        var noticias = new List<Noticia>
        {
            new Noticia
            {
                //Id = 1, // El ID se genera automáticamente
                Titulo = "Gato persa: El rey de la elegancia felina",
                Contenido = @"
                    <p>El gato persa es, sin duda, uno de los gatos más emblemáticos y queridos del mundo. Su presencia imponente, su carácter sereno y su lujoso pelaje lo convierten en un compañero ideal para quienes buscan ternura, sofisticación y elegancia.</p>
                    <h2>Historia y personalidad del gato persa</h2>
                    <p>Originario de Persia (actual Irán), este felino fue introducido en Europa en los siglos XVII y XVIII, cautivando de inmediato a la nobleza. Incluso la reina Victoria sucumbió ante su encanto, ayudando a popularizar la raza en Inglaterra.</p>
                    <p>El gato persa es conocido por su carácter suave y calmado. No es de esos gatos que saltan por todos lados: prefiere acurrucarse y disfrutar de un ambiente tranquilo.</p>
                    <h2>Apariencia: un manto de belleza en cada color</h2>
                    <p>Una de las características más llamativas del gato persa es su pelaje largo, denso y abundante. Esta “melena” requiere cuidado diario para que se mantenga brillante, libre de nudos y saludable. Y cuando hablamos de los colores del gato persa, encontramos un despliegue realmente impresionante. La raza cuenta con una gran variedad de tonos y patrones:</p>
                    <ul><li>Colores sólidos: blanco, negro, azul (gris), chocolate, crema, rojo.</li><li>Silver y Golden: pelajes plateados o dorados.</li><li>Smoke y shaded: efecto visual que cambia con el movimiento.</li><li>Tabby, bicolor, tricolor y particolor.</li><li>Colorpoint: cuerpo claro y extremidades oscuras con ojos azules.</li></ul>
                    <h2>Cuidados que requiere un gato persa</h2>
                    <ul><li>Cepillado diario con peine y cepillo de cerdas suaves.</li><li>Baños ocasionales.</li><li>Limpieza facial (ojos).</li><li>Corte de uñas, limpieza dental y de orejas.</li></ul>
                    <h2>Salud y bienestar</h2>
                    <p>Como raza de cara aplanada o braquicéfalo, el gato persa puede enfrentar desafíos en su respiración. Además, es susceptible a enfermedades hereditarias como la poliquistosis renal, displasia de cadera y afecciones oculares. Una revisión veterinaria frecuente y el apoyo de criadores responsables son clave para su bienestar.</p>
                    <h2>Vida en casa: ¿para quién es ideal?</h2>
                    <p>El gato persa es perfecto para personas que buscan un compañero tranquilo  y de carácter apacible. Son afectuosos, toleran bien la soledad moderada y aprecian un entorno relajado. No son la mejor opción para hogares con niños muy enérgicos o ambientes caóticos. También son propensos al sobrepeso si no tienen una dieta controlada y algo de juego diario. Pocos minutos de interacción pueden asegurar su salud física y mental. El gato persa se distingue por su temperamento apacible, cariño y elegancia.</p>
                    ",
                UrlImagen = "https://gatos.plus/wp-content/uploads/2020/05/gato-persa.jpg",
                FechaPublicacion = new DateTime(2025, 8, 21).ToUniversalTime(),
                // No necesitas añadir Comentarios aquí, se cargarán después
            },
            new Noticia
            {
                //Id = 2,
                Titulo = "¿Por qué los gatos duermen mucho?",
                Contenido = @"
                    <h2>El ciclo felino del sueño</h2>
                    <p>Los gatos suelen dormir entre 12 y 16 horas al día, y algunos incluso llegan hasta las 20 horas. Sí, ¡prácticamente dos tercios de su vida la pasan durmiendo! Esta tendencia tiene raíces evolutivas: en la naturaleza, los felinos son depredadores que necesitan conservar energía para cazar.</p>
                    <p>El sueño de los gatos no es igual al de los humanos. Ellos alternan entre el sueño ligero, en el que se mantienen alerta ante cualquier ruido, y el sueño profundo.</p>
                    <h2>¿Qué influye en cuánto duerme un gato?</h2>
                    <ul><li><strong>Edad:</strong> Los gatitos y los gatos mayores duermen más.</li><li><strong>Nivel de actividad:</strong> Los gatos de interior suelen dormir más.</li><li><strong>Temperatura:</strong> Buscan un lugar cálido cuando hace frío o está nublado.</li><li><strong>Rutina del hogar:</strong> Ajustan sus horarios a los tuyos.</li></ul>
                    <h2>¿Dormilón o enfermo?</h2>
                    <p>Aunque los gatos duerman mucho, el exceso de sueño puede ser una señal de alerta si va acompañado de:</p>
                    <ul><li>Menos interés en jugar.</li><li>Cambios en el apetito.</li><li>Dificultad para moverse.</li><li>Escondites prolongados.</li></ul>
                    <p>En estos casos, la recomendación es clara: consulta al veterinario lo antes posible.</p>
                    <h2>Tu gato y el arte de dormir sobre ti</h2>
                    <p>Lo hacen porque buscan calor, seguridad y vínculo emocional. Si tu gato duerme sobre ti, considéralo un halago: eres su lugar favorito en el mundo.</p>
                    <p>La próxima vez que veas a tu gato dormir por quinta vez en el día, no te preocupes demasiado. Está recargando baterías para la gran misión que lo espera: ser adorable y conquistar tu corazón.</p>
                ",
                UrlImagen = "https://www.muyinteresante.com/wp-content/uploads/sites/5/2022/10/12/634616ee6f89a.jpeg",
                FechaPublicacion = new DateTime(2025, 8, 26).ToUniversalTime(),
            },
            new Noticia
            {
                //Id = 3,
                Titulo = "¡Perros surfeando olas! El evento más adorable",
                Contenido =  @"
                    <p>Las olas comienzan a formarse en el horizonte, y de repente, una tabla corta el agua con destreza. ¿El protagonista? No es un surfista profesional, sino un atleta de cuatro patas. Así se vive cada verano en California y otros rincones del planeta, donde los perros surfistas se han convertido en la sensación de un deporte único que mezcla diversión, ternura y solidaridad.</p>
                    <h2>Una ola que conquista el mundo</h2>
                    <p>Los campeonatos de surf canino no solo se celebran en California. En el Reino Unido, el <strong>Dog Surfing Championship</strong> organizado por Shaka Surf tuvo la mayor cantidad de perros surfeando en una misma ola y la carrera más rápida de paddleboard humano-perro en 50 metros. Aunque nació en San Diego hace más de una década, el surf para perros ya ha llegado a Florida, Australia y el Reino Unido, convirtiéndose en una tendencia internacional.</p>
                    <p>En estas competencias participan perros de todas las razas y tamaños: desde pequeños chihuahuas hasta imponentes golden retrievers. Lo más valioso es el vínculo entre los dueños y sus mascotas, que entrenan juntos y disfrutan cada momento dentro y fuera del agua.</p>
                    <h2>Surf canino: una experiencia que debes vivir</h2>
                    <p>Más allá del espectáculo, el campeonato mundial de surf canino tiene un objetivo solidario. <strong>Parte de los ingresos recaudados se destinan a asociaciones de bienestar animal</strong>, apoyando a refugios y programas comunitarios. Este espíritu benéfico hace que cada ola surfeada tenga un doble valor: diversión para el público y ayuda concreta para cientos de animales necesitados.</p>
                    <p>Si alguna vez visitas California en verano, no dudes en acercarte al World Dog Surfing Championships. Te llevarás la sonrisa más grande y la certeza de haber presenciado uno de los eventos más originales y adorables del mundo.</p>
                ",
                UrlImagen = "https://elbocon.pe/resizer/T-z98vZkTNeF7bkusKA3dSpFg6I=/980x0/smart/filters:format(jpeg):quality(75)/arc-anglerfish-arc2-prod-elcomercio.s3.amazonaws.com/public/FG3WVTPYP5BYJFYCG7QCBVAJYA.jpg",
                FechaPublicacion = new DateTime(2025, 8, 28).ToUniversalTime(),
            }
        };
        
        // Añade las noticias al contexto y guarda los cambios
        context.Noticias.AddRange(noticias);
        await context.SaveChangesAsync();
        
        // Usamos GetService para obtener el logger de forma segura
        var logger = services.GetService<ILogger<Program>>();
        logger?.LogInformation("Se inicializaron {Count} noticias en la base de datos.", noticias.Count);
    }
}
// --- FIN FUNCIÓN SEEDNOTICIAS ---


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

// ¡IMPORTANTE! Asegúrate de que UseAuthentication() esté antes de UseAuthorization()
app.UseAuthentication(); // <-- AÑADIR SI FALTA
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();