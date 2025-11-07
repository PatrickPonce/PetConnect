using PetConnect.Data;
using PetConnect.Models;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore; // Asegúrate de tener este 'using'

namespace PetConnect.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // --- LÓGICA DE SIEMBRA DE NOTICIAS (MEJORADA) ---

            // 1. Obtener los títulos de las noticias que YA existen en la BD.
            var titulosExistentes = context.Noticias
                .Select(n => n.Titulo)
                .ToHashSet(); // Usamos HashSet para búsquedas rápidas

            // 2. Definir la lista completa de noticias (18 noticias únicas).
            // (Eliminé la noticia duplicada y corregí las URLs)
            var noticiasSemilla = new List<Noticia>
            {
                // NOTICIA 1: GATO PERSA
                new Noticia {
                    Titulo = "Gato persa: El rey de la elegancia felina",
                    UrlImagen = "https://gatos.plus/wp-content/uploads/2020/05/gato-persa.jpg",
                    FechaPublicacion = new DateTime(2025, 8, 21, 0, 0, 0, DateTimeKind.Utc),
                    Tags = "Gatos, Salud, Comportamiento",
                    Contenido = @"<p>El gato persa es...</p>" // (Tu HTML largo)
                },
                // NOTICIA 2: GATOS DURMIENDO
                new Noticia {
                    Titulo = "¿Por qué los gatos duermen mucho?",
                    UrlImagen = "https://www.muyinteresante.com/wp-content/uploads/sites/5/2022/10/12/634616ee6f89a.jpeg",
                    FechaPublicacion = new DateTime(2025, 8, 26, 0, 0, 0, DateTimeKind.Utc),
                    Tags = "Gatos, Comportamiento, Salud",
                    Contenido = @"<h2>El ciclo felino del sueño</h2>..." // (Tu HTML largo)
                },
                // NOTICIA 3: PERROS SURFISTAS
                new Noticia {
                    Titulo = "¡Perros surfeando olas! El evento más adorable",
                    UrlImagen = "https://elbocon.pe/resizer/T-z98vZkTNeF7bkusKA3dSpFg6I=/980x0/smart/filters:format(jpeg):quality(75)/arc-anglerfish-arc2-prod-elcomercio.s3.amazonaws.com/public/FG3WVTPYP5BYJFYCG7QCBVAJYA.jpg",
                    FechaPublicacion = new DateTime(2025, 8, 28, 0, 0, 0, DateTimeKind.Utc),
                    Tags = "Perros, Deporte, Eventos", // (Etiqueta cambiada)
                    Contenido = @"<p>Las olas comienzan a formarse...</p>" // (Tu HTML largo)
                },
                // NOTICIA 4 : El gato bengala
                new Noticia {
                    Titulo = "El gato bengala: un leopardo en miniatura que ronronea",
                    UrlImagen = "https://www.webconsultas.com/sites/default/files/media/2019/05/13/gato_bengala_caracteristicas.jpg",
                    FechaPublicacion = new DateTime(2025, 8, 30, 0, 0, 0, DateTimeKind.Utc),
                    Tags = "Gatos, Comportamiento",
                    Contenido = @"<p>El gato Bengala, también conocido como gato bengalí...</p>" // (Tu HTML largo)
                },
                // NOTICIA 5 (La duplicada fue eliminada, esta es la única)
                new Noticia {
                    Titulo = "La importancia de la vacunación anual en perros",
                    Contenido = "<h2>Prevención es salud</h2><p>Las vacunas protegen a tu perro de enfermedades graves...</p>",
                    UrlImagen = "https://www.gardacan.com/la-importancia-de-la-vacunacion-anual-en-perros-protege-su-salud-todo-el-a%C3%B1o_img357343t1.jpg",
                    FechaPublicacion = new DateTime(2025, 8, 28, 0, 0, 0, DateTimeKind.Utc),
                    Tags = "Perros, Salud"
                },
                // NOTICIA 6
                new Noticia {
                    Titulo = "Cómo detectar la ansiedad por separación en tu mascota",
                    Contenido = "<h2>¿Se queda solo en casa?</h2><p>Ladridos excesivos, destrozos en muebles...</p>",
                    UrlImagen = "https://www.amarededomicanes.com/wp-content/uploads/2022/12/Tiene-comportamientos-destructivos-2.jpg",
                    FechaPublicacion = new DateTime(2025, 8, 29, 0, 0, 0, DateTimeKind.Utc),
                    Tags = "Perros, Gatos, Comportamiento"
                },
                // NOTICIA 7
                new Noticia {
                    Titulo = "Peligros del chocolate para los gatos",
                    Contenido = "<h2>¡Nunca le des chocolate!</h2><p>El chocolate contiene teobromina...</p>",
                    UrlImagen = "https://cdn0.uncomo.com/es/posts/0/8/4/que_pasa_si_un_gato_come_chocolate_51480_orig.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-5),
                    Tags = "Gatos, Salud, Nutrición"
                },
                // NOTICIA 8
                new Noticia {
                    Titulo = "¿Por qué adoptar una mascota adulta?",
                    Contenido = "<h2>Una segunda oportunidad</h2><p>Los cachorros son adorables, pero las mascotas adultas...</p>",
                    UrlImagen = "https://www.tiendanimal.es/articulos/wp-content/uploads/2023/07/beneficios-adopcion-perro-mayor.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-6),
                    Tags = "Adopción, Perros, Gatos"
                },
                // NOTICIA 9
                new Noticia {
                    Titulo = "Preparando tu casa para un nuevo cachorro",
                    Contenido = "<h2>¡Bienvenido a casa!</h2><p>Antes de que llegue el nuevo miembro de la familia...</p>",
                    UrlImagen = "https://www.hillspet.com.pe/content/dam/cp-sites-aem/hills/hills-pet/articles/body/g/golden-retriever-puppy-running-in-field-sw.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-7),
                    Tags = "Adopción, Perros, Entrenamiento"
                },
                // NOTICIA 10
                new Noticia {
                    Titulo = "El proceso de adopción en un refugio: Lo que debes saber",
                    Contenido = "<h2>Encontrando a tu mejor amigo</h2><p>Adoptar en un refugio implica llenar una solicitud...</p>",
                    UrlImagen = "https://es.statefarm.com/content/dam/sf-library/en-us/secure/legacy/simple-insights/pet-adoption.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-8),
                    Tags = "Adopción, Refugios"
                },
                // NOTICIA 11
                new Noticia {
                    Titulo = "Gatos negros: Mitos y realidades",
                    Contenido = "<h2>Más allá de la superstición</h2><p>Lamentablemente, los gatos negros son los que menos se adoptan...</p>",
                    UrlImagen = "https://www.sdpnoticias.com/resizer/v2/AJFUVZMSWFBTNI5DHMGWVYG2RM.jpg?smart=true&auth=8ecea01375dcc110396249009054c7f29485e686f932738180e055ed3064781b&width=1440&height=810",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-9),
                    Tags = "Adopción, Gatos"
                },
                // NOTICIA 12
                new Noticia {
                    Titulo = "5 trucos fáciles para enseñarle a tu perro",
                    Contenido = "<h2>¡Buen chico!</h2><p>Empezar con comandos básicos como 'sentado', 'quieto' y 'ven'...</p>",
                    UrlImagen = "https://www.hundeo.com/wp-content/uploads/2024/01/High-Five-1024x576.webp",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-10),
                    Tags = "Perros, Entrenamiento, Comportamiento"
                },
                // NOTICIA 13
                new Noticia {
                    Titulo = "Cómo acostumbrar a tu gato a usar el rascador (¡y no tus muebles!)",
                    Contenido = "<h2>¡Mis sofás!</h2><p>Coloca el rascador en un lugar visible...</p>",
                    UrlImagen = "https://purina.cl/sites/default/files/2022-10/Purina-5-trucos-para-que-tu-gato-aprenda-a-usar-el-rascador.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-11),
                    Tags = "Gatos, Entrenamiento, Comportamiento"
                },
                // NOTICIA 14
                new Noticia {
                    Titulo = "El Clicker: Una herramienta poderosa de entrenamiento",
                    Contenido = "<h2>El poder del 'Click'</h2><p>El clicker es un dispositivo que hace un sonido 'click'...</p>",
                    UrlImagen = "https://lazosdelagente.com/wp-content/uploads/2024/02/El-clicker-en-el-adiestramiento-canino.webp",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-12),
                    Tags = "Perros, Entrenamiento"
                },
                // NOTICIA 15
                new Noticia {
                    Titulo = "Alimentación BARF: ¿Es segura para tu mascota?",
                    Contenido = "<h2>Dieta cruda</h2><p>La dieta BARF (Biologically Appropriate Raw Food)...</p>",
                    UrlImagen = "https://imagescdn.estarbien.com.pe/blt3c6af4ceb9664f34/663aa4121fb2b0cbbe51f129/dieta_barf.jpg?format=auto&quality=85",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-13),
                    Tags = "Nutrición, Perros, Gatos"
                },
                // NOTICIA 16
                new Noticia {
                    Titulo = "La guía definitiva para leer etiquetas de comida para gatos",
                    Contenido = "<h2>¿Qué contiene realmente?</h2><p>Busca que el primer ingrediente sea una fuente de proteína...</p>",
                    UrlImagen = "https://verdecora.es/blog/wp-content/uploads/2017/11/comida-gatos-natural.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-14),
                    Tags = "Gatos, Nutrición"
                },
                // NOTICIA 17
                new Noticia {
                    Titulo = "Frutas y verduras seguras para tu perro",
                    Contenido = "<h2>Snacks saludables</h2><p>¡Sí! Muchas frutas son geniales como premios...</p>",
                    UrlImagen = "https://cdn0.expertoanimal.com/es/posts/0/3/1/beneficios_de_las_frutas_y_verduras_para_perros_20130_0_600.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-15),
                    Tags = "Perros, Nutrición, Salud"
                },
                // NOTICIA 18
                new Noticia {
                    Titulo = "Beneficios del aceite de salmón para la piel de tu perro",
                    Contenido = "<h2>Piel sana, perro feliz</h2><p>El aceite de salmón es rico en ácidos grasos Omega-3...</p>",
                    UrlImagen = "https://animalnatura.com/img/cms/beneficios-aceite-de-salmon-perros.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-4),
                    Tags = "Perros, Salud, Nutrición"
                },
                // NOTICIA 19
                new Noticia {
                    Titulo = "Entendiendo el lenguaje corporal de tu gato",
                    Contenido = "<h2>¿Qué te quiere decir?</h2><p>Una cola erizada significa miedo o agresión...</p>",
                    UrlImagen = "https://www.feliway.es/cdn/shop/articles/Understanding_20Kitten_20Body_20Language_1_20_281_29-1.jpg?v=1674124079&width=1024",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-16),
                    Tags = "Gatos, Comportamiento"
                }
            };

            // 3. Filtrar solo las noticias que FALTAN.
            var noticiasParaAgregar = new List<Noticia>();
            foreach (var noticia in noticiasSemilla)
            {
                if (!titulosExistentes.Contains(noticia.Titulo))
                {
                    noticiasParaAgregar.Add(noticia);
                }
            }

            // 4. Si hay noticias nuevas para agregar, las guardamos.
            if (noticiasParaAgregar.Any())
            {
                context.Noticias.AddRange(noticiasParaAgregar);
                context.SaveChanges(); // Guardar las noticias nuevas
            }

            // --- LÓGICA DE SIEMBRA DE COMENTARIOS (PROTEGIDA) ---
            
            // 5. Solo agregamos comentarios de ejemplo SI LA TABLA ESTÁ COMPLETAMENTE VACÍA.
            if (!context.Comentarios.Any())
            {
                var noticiaGatoPersa = context.Noticias.FirstOrDefault(n => n.Titulo.Contains("Gato persa"));
                if (noticiaGatoPersa != null)
                {
                    var comentariosIniciales = new List<Comentario>
                    {
                        // ✅ CORRECCIÓN: Añadidos los campos requeridos (AutorId y AutorFotoUrl)
                        new Comentario { 
                            Autor = "David Gor", 
                            Texto = "Holaa", 
                            FechaComentario = DateTime.UtcNow.AddMinutes(-5), 
                            NoticiaId = noticiaGatoPersa.Id,
                            AutorId = "default-user-id", // Valor de ejemplo
                            AutorFotoUrl = "/images/avatars/https://i.pinimg.com/736x/0f/ff/2b/0fff2bf9b912d785e8e370ff87b2706c.jpg" // Valor de ejemplo
                        },
                        new Comentario { 
                            Autor = "David Gor", 
                            Texto = "Los gatos persa son muy bonitos >.<", 
                            FechaComentario = DateTime.UtcNow.AddMinutes(-2), 
                            NoticiaId = noticiaGatoPersa.Id,
                            AutorId = "default-user-id", // Valor de ejemplo
                            AutorFotoUrl = "/images/avatars/https://i.pinimg.com/736x/0f/ff/2b/0fff2bf9b912d785e8e370ff87b2706c.jpg" // Valor de ejemplo
                        }
                    };
                    context.Comentarios.AddRange(comentariosIniciales);
                    context.SaveChanges(); // Guardar los comentarios
                }
            }
        }
    }
}