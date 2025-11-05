using PetConnect.Data;
using PetConnect.Models;
using System.Linq;
using System.Collections.Generic;
using System;

namespace PetConnect.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // 1. Verificar si ya hay noticias para evitar duplicados
            if (context.Noticias.Any())
            {
                return; // La base de datos ya ha sido sembrada, salimos.
            }

            // 2. Definir los datos de las Noticias (Contenido en formato HTML)
            var noticiasIniciales = new List<Noticia>
            {
                // NOTICIA 1: GATO PERSA
                new Noticia
                {
                    Titulo = "Gato persa: El rey de la elegancia felina",
                    UrlImagen = "https://gatos.plus/wp-content/uploads/2020/05/gato-persa.jpg",
                    // ⬅️ CORRECCIÓN: Fechas estáticas convertidas a UTC
                    FechaPublicacion = new DateTime(2025, 8, 21, 0, 0, 0, DateTimeKind.Utc),
                    Tags = "Gatos, Salud, Comportamiento",
                    Contenido = @"
                        <p>El gato persa es, sin duda, uno de los gatos más emblemáticos y queridos del mundo. 
                        Su presencia imponente, su carácter sereno y su lujoso pelaje lo convierten en un compañero ideal para quienes buscan ternura, sofisticación y elegancia.</p>
                        <h2>Historia y personalidad del gato persa</h2>
                        <p>Originario de Persia (actual Irán), este felino fue introducido en Europa en los siglos XVII y XVIII, cautivando de inmediato a la nobleza. Incluso la reina Victoria sucumbió ante su encanto, ayudando a popularizar la raza en Inglaterra.</p>
                        <p>El gato persa es conocido por su carácter suave y calmado. No es de esos gatos que saltan por todos lados: prefiere acurrucarse y disfrutar de un ambiente tranquilo.</p>
                        <h2>Apariencia: un manto de belleza en cada color</h2>
                        <p>Una de las características más llamativas del gato persa es su pelaje largo, denso y abundante. Esta “melena” requiere cuidado diario para que se mantenga brillante, libre de nudos y saludable. Y cuando hablamos de los colores del gato persa, encontramos un despliegue realmente impresionante. La raza cuenta con una gran variedad de tonos y patrones:</p>
                        <ul>
                            <li>Colores sólidos: blanco, negro, azul (gris), chocolate, crema, rojo.</li>
                            <li>Silver y Golden: pelajes plateados o dorados.</li>
                            <li>Smoke y shaded: efecto visual que cambia con el movimiento.</li>
                            <li>Tabby, bicolor, tricolor y particolor.</li>
                            <li>Colorpoint: cuerpo claro y extremidades oscuras con ojos azules.</li>
                        </ul>
                        <h2>Cuidados que requiere un gato persa</h2>
                        <ul>
                            <li>Cepillado diario con peine y cepillo de cerdas suaves.</li>
                            <li>Baños ocasionales.</li>
                            <li>Limpieza facial (ojos).</li>
                            <li>Corte de uñas, limpieza dental y de orejas.</li>
                        </ul>
                        <h2>Salud y bienestar</h2>
                        <p>Como raza de cara aplanada o braquicéfalo, el gato persa puede enfrentar desafíos en su respiración. Además, es susceptible a enfermedades hereditarias como la poliquistosis renal, displasia de cadera y afecciones oculares. Una revisión veterinaria frecuente y el apoyo de criadores responsables son clave para su bienestar.</p>
                        <h2>Vida en casa: ¿para quién es ideal?</h2>
                        <p>El gato persa es perfecto para personas que buscan un compañero tranquilo y de carácter apacible. Son afectuosos, toleran bien la soledad moderada y aprecian un entorno relajado. No son la mejor opción para hogares con niños muy enérgicos o ambientes caóticos.</p>
                        <p>También son propensos al sobrepeso si no tienen una dieta controlada y algo de juego diario. Pocos minutos de interacción pueden asegurar su salud física y mental. El gato persa se distingue por su temperamento apacible, cariño y elegancia.</p>
                    "
                },
                // NOTICIA 2: GATOS DURMIENDO
                new Noticia
                {
                    Titulo = "¿Por qué los gatos duermen mucho?",
                    UrlImagen = "https://www.muyinteresante.com/wp-content/uploads/sites/5/2022/10/12/634616ee6f89a.jpeg",
                    // ⬅️ CORRECCIÓN: Fechas estáticas convertidas a UTC
                    FechaPublicacion = new DateTime(2025, 8, 26, 0, 0, 0, DateTimeKind.Utc),
                    Tags = "Gatos, Comportamiento, Salud",
                    Contenido = @"
                        <h2>El ciclo felino del sueño</h2>
                        <p>Los gatos suelen dormir entre 12 y 16 horas al día, y algunos incluso llegan hasta las 20 horas. Sí, ¡prácticamente dos tercios de su vida la pasan durmiendo! Esta tendencia tiene raíces evolutivas: en la naturaleza, los felinos son depredadores que necesitan conservar energía para cazar.</p>
                        <p>El sueño de los gatos no es igual al de los humanos. Ellos alternan entre el sueño ligero, en el que se mantienen alerta ante cualquier ruido, y el sueño profundo.</p>
                        <h2>¿Qué influye en cuánto duerme un gato?</h2>
                        <ul>
                            <li><strong>Edad:</strong> Los gatitos y los gatos mayores duermen más.</li>
                            <li><strong>Nivel de actividad:</strong> Los gatos de interior suelen dormir más.</li>
                            <li><strong>Temperatura:</strong> Buscan un lugar cálido cuando hace frío o está nublado.</li>
                            <li><strong>Rutina del hogar:</strong> Ajustan sus horarios a los tuyos.</li>
                        </ul>
                        <h2>¿Dormilón o enfermo?</h2>
                        <p>Aunque los gatos duerman mucho, el exceso de sueño puede ser una señal de alerta si va acompañado de otros cambios. En estos casos, la recomendación es clara: consulta al veterinario lo antes posible.</p> 
                        <h2>Tu gato y el arte de dormir sobre ti</h2>
                        <p>Lo hacen porque buscan calor, seguridad y vínculo emocional. Si tu gato duerme sobre ti, considéralo un halago: eres su lugar favorito en el mundo.</p>
                        <p>La próxima vez que veas a tu gato dormir por quinta vez en el día, no te preocupes demasiado. Está recargando baterías para la gran misión que lo espera: ser adorable y conquistar tu corazón.</p>
                    "
                },
                // NOTICIA 3: PERROS SURFISTAS
                new Noticia
                {
                    Titulo = "¡Perros surfeando olas! El evento más adorable",
                    UrlImagen = "https://elbocon.pe/resizer/T-z98vZkTNeF7bkusKA3dSpFg6I=/980x0/smart/filters:format(jpeg):quality(75)/arc-anglerfish-arc2-prod-elcomercio.s3.amazonaws.com/public/FG3WVTPYP5BYJFYCG7QCBVAJYA.jpg",
                    // ⬅️ CORRECCIÓN: Fechas estáticas convertidas a UTC
                    FechaPublicacion = new DateTime(2025, 8, 28, 0, 0, 0, DateTimeKind.Utc),
                    Tags = "Perros, Deporte, Eventos",
                    Contenido = @"
                        <p>Las olas comienzan a formarse en el horizonte, y de repente, una tabla corta el agua con destreza. ¿El protagonista? No es un surfista profesional, sino un atleta de cuatro patas. Así se vive cada verano en California y otros rincones del planeta, donde los perros surfistas se han convertido en la sensación de un deporte único que mezcla diversión, ternura y solidaridad.</p>
                        <h2>Una ola que conquista el mundo</h2>
                        <p>Los campeonatos de surf canino no solo se celebran en California. En el Reino Unido, el <strong>Dog Surfing Championship</strong> tuvo la mayor cantidad de perros surfeando en una misma ola y la carrera más rápida de paddleboard humano-perro en 50 metros. Aunque nació en San Diego hace más de una década, el surf para perros ya ha llegado a Florida, Australia y el Reino Unido, convirtiéndose en una tendencia internacional.</p>
                        <p>En estas competencias participan perros de todas las razas y tamaños: desde pequeños chihuahuas hasta imponentes golden retrievers. Lo más valioso es el vínculo entre los dueños y sus mascotas, que entrenan juntos y disfrutan cada momento dentro y fuera del agua.</p>
                        <h2>Surf canino: una experiencia que debes vivir</h2>
                        <p>Más allá del espectáculo, el campeonato mundial de surf canino tiene un objetivo solidario. <strong>Parte de los ingresos recaudados se destinan a asociaciones de bienestar animal</strong>, apoyando a refugios y programas comunitarios. Este espíritu benéfico hace que cada ola surfeada tenga un doble valor: diversión para el público y ayuda concreta para cientos de animales necesitados.</p>
                        <p>Si alguna vez visitas California en verano, no dudes en acercarte al World Dog Surfing Championships. Te llevarás la sonrisa más grande y la certeza de haber presenciado uno de los eventos más originales y adorables del mundo.</p>
                    "
                },
                // NOTICIA 4 : El gato bengala: un leopardo en miniatura que ronronea
                new Noticia
                {
                    Titulo = "El gato bengala: un leopardo en miniatura que ronronea",
                    UrlImagen = "https://www.webconsultas.com/sites/default/files/media/2019/05/13/gato_bengala_caracteristicas.jpg",
                    // ⬅️ CORRECCIÓN: Fechas estáticas convertidas a UTC
                    FechaPublicacion = new DateTime(2025, 8, 30, 0, 0, 0, DateTimeKind.Utc),
                    Tags = "Gatos,Razas de gatos, Comportamiento felino, Cuidado felino",
                    Contenido = @"
                        <p>El gato Bengala, también conocido como gato bengalí, es una de las razas más fascinantes del mundo felino. Su aspecto recuerda al de un pequeño leopardo, pero detrás de ese pelaje exótico se esconde un compañero cariñoso, juguetón y lleno de energía. Esta raza combina lo mejor de dos mundos: la belleza salvaje de sus antepasados y la dulzura de un gato doméstico moderno.</p>
                        <h2>Un felino con raíces salvajes</h2>
                        <p>El origen del gato Bengala es tan interesante como su apariencia. Nació del cruce entre un gato leopardo asiático (Prionailurus bengalensis), una especie silvestre, y gatos domésticos de razas como el abisinio, ocicat o mau egipcio. La criadora estadounidense Jean Mill fue quien, en la década de 1960, comenzó este ambicioso proyecto para crear un gato que luciera salvaje, pero con un carácter dócil y familiar.</p>
                        <p>El resultado: un gato de mirada intensa, musculoso, con un pelaje que parece pintado a mano. Desde entonces, el Bengala ha ganado admiradores en todo el mundo por su belleza única y su personalidad equilibrada, convirtiéndose en una de las razas más populares entre los amantes de los felinos exóticos.</p>
                        <h2>Un abrigo de leopardo, un corazón de gato doméstico</h2>
                        <p>El pelaje del gato Bengala es su sello más distintivo. Sus manchas (conocidas como rosetas) pueden variar en forma y tono, desde dorado y ámbar hasta plateado o incluso nieve. Al tacto, su pelo es suave, corto y brillante, con un efecto satinado que refleja la luz, lo que le da un aspecto casi hipnótico.No solo es hermoso a la vista: su patrón es el resultado de una cuidadosa selección genética que resalta los rasgos del gato leopardo asiático sin comprometer su salud o temperamento.</p>
                        <h2>El encanto del Bengala</h2>
                        <p>El gato Bengala combina lo mejor del mundo salvaje y el doméstico. Su belleza natural, energía incansable e inteligencia lo hacen una raza fascinante, perfecta para tutores activos que buscan un compañero tan vibrante como ellos.Si estás pensando en ampliar tu familia, este pequeño felino con alma de explorador puede ser el compañero ideal. Eso sí, prepárate para una vida llena de saltos, juegos y mucho amor con el gato más salvajemente encantador que podrás conocer.</p>"
                },
                // NOTICIA 5
                new Noticia
                {
                    Titulo = "La importancia de la vacunación anual en perros",
                    Contenido = "<h2>Prevención es salud</h2><p>Las vacunas protegen a tu perro de enfermedades graves y potencialmente mortales. La rabia, el moquillo y el parvovirus son solo algunas de las amenazas que una simple vacuna puede prevenir.</p>",
                    UrlImagen = "https://www.gardacan.com/la-importancia-de-la-vacunacion-anual-en-perros-protege-su-salud-todo-el-a%C3%B1o_img357343t1.jpg",
                    FechaPublicacion = new DateTime(2025, 8, 28, 0, 0, 0, DateTimeKind.Utc),
                    Tags = "Perros, Salud"
                },
                // NOTICIA 6
                new Noticia
                {
                    Titulo = "Cómo detectar la ansiedad por separación en tu mascota",
                    Contenido = "<h2>¿Se queda solo en casa?</h2><p>Ladridos excesivos, destrozos en muebles o hacer sus necesidades dentro de casa son señales clásicas de ansiedad por separación. Es un problema que se puede tratar.</p>",
                    UrlImagen = "https://www.amarededomicanes.com/wp-content/uploads/2022/12/Tiene-comportamientos-destructivos-2.jpg",
                    FechaPublicacion = new DateTime(2025, 8, 29, 0, 0, 0, DateTimeKind.Utc),
                    Tags = "Perros, Gatos, Comportamiento"
                },
                // NOTICIA 7
                new Noticia
                {
                    Titulo = "Peligros del chocolate para los gatos",
                    Contenido = "<h2>¡Nunca le des chocolate!</h2><p>El chocolate contiene teobromina, una sustancia que es tóxica para los gatos y perros. Puede causar vómitos, diarrea, temblores e incluso la muerte.</p>",
                    UrlImagen = "https://cdn0.uncomo.com/es/posts/0/8/4/que_pasa_si_un_gato_come_chocolate_51480_orig.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-5),
                    Tags = "Gatos, Salud, Nutrición"
                },
                // NOTICIA 8
                new Noticia
                {
                    Titulo = "¿Por qué adoptar una mascota adulta?",
                    Contenido = "<h2>Una segunda oportunidad</h2><p>Los cachorros son adorables, pero las mascotas adultas ya tienen una personalidad definida. Sabrás exactamente cómo es su temperamento y nivel de energía. ¡Además, muchos ya están entrenados!</p>",
                    UrlImagen = "https://www.tiendanimal.es/articulos/wp-content/uploads/2023/07/beneficios-adopcion-perro-mayor.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-6),
                    Tags = "Adopción, Perros, Gatos"
                },
                // NOTICIA 9
                new Noticia
                {
                    Titulo = "Preparando tu casa para un nuevo cachorro",
                    Contenido = "<h2>¡Bienvenido a casa!</h2><p>Antes de que llegue el nuevo miembro de la familia, asegúrate de tener una cama cómoda, platos para comida y agua, juguetes seguros y un área designada para que haga sus necesidades.</p>",
                    UrlImagen = "https://www.hillspet.com.pe/content/dam/cp-sites-aem/hills/hills-pet/articles/body/g/golden-retriever-puppy-running-in-field-sw.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-7),
                    Tags = "Adopción, Perros, Entrenamiento"
                },
                // NOTICIA 10
                new Noticia
                {
                    Titulo = "El proceso de adopción en un refugio: Lo que debes saber",
                    Contenido = "<h2>Encontrando a tu mejor amigo</h2><p>Adoptar en un refugio implica llenar una solicitud, una entrevista y, a veces, una visita a tu hogar. Están diseñados para asegurar que tanto tú como la mascota sean una pareja perfecta.</p>",
                    UrlImagen = "https://es.statefarm.com/content/dam/sf-library/en-us/secure/legacy/simple-insights/pet-adoption.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-8),
                    Tags = "Adopción, Refugios"
                },
                // NOTICIA 11
                new Noticia
                {
                    Titulo = "Gatos negros: Mitos y realidades",
                    Contenido = "<h2>Más allá de la superstición</h2><p>Lamentablemente, los gatos negros son los que menos se adoptan debido a mitos anticuados. En realidad, son tan cariñosos, juguetones y maravillosos como cualquier otro gato.</p>",
                    UrlImagen = "https://www.sdpnoticias.com/resizer/v2/AJFUVZMSWFBTNI5DHMGWVYG2RM.jpg?smart=true&auth=8ecea01375dcc110396249009054c7f29485e686f932738180e055ed3064781b&width=1440&height=810",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-9),
                    Tags = "Adopción, Gatos"
                },
                // NOTICIA 12
                new Noticia
                {
                    Titulo = "5 trucos fáciles para enseñarle a tu perro",
                    Contenido = "<h2>¡Buen chico!</h2><p>Empezar con comandos básicos como 'sentado', 'quieto' y 'ven' es fundamental. Usa refuerzo positivo (premios y caricias) para que el aprendizaje sea divertido.</p>",
                    UrlImagen = "https://www.hundeo.com/wp-content/uploads/2024/01/High-Five-1024x576.webp",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-10),
                    Tags = "Perros, Entrenamiento, Comportamiento"
                },
                // NOTICIA 13
                new Noticia
                {
                    Titulo = "Cómo acostumbrar a tu gato a usar el rascador (¡y no tus muebles!)",
                    Contenido = "<h2>¡Mis sofás!</h2><p>Coloca el rascador en un lugar visible, cerca de donde duerme o juega. Usa catnip (hierba gatera) para atraerlo y prémialo cuando lo use. Nunca lo regañes por rascar los muebles, redirígelo.</p>",
                    UrlImagen = "https://purina.cl/sites/default/files/2022-10/Purina-5-trucos-para-que-tu-gato-aprenda-a-usar-el-rascador.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-11),
                    Tags = "Gatos, Entrenamiento, Comportamiento"
                },
                // NOTICIA 14
                new Noticia
                {
                    Titulo = "El Clicker: Una herramienta poderosa de entrenamiento",
                    Contenido = "<h2>El poder del 'Click'</h2><p>El clicker es un dispositivo que hace un sonido 'click' para marcar el momento exacto en que tu perro hace algo bien. Se asocia el 'click' con un premio, creando una comunicación clara y rápida.</p>",
                    UrlImagen = "https://lazosdelagente.com/wp-content/uploads/2024/02/El-clicker-en-el-adiestramiento-canino.webp",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-12),
                    Tags = "Perros, Entrenamiento"
                },
                // NOTICIA 15
                new Noticia
                {
                    Titulo = "Alimentación BARF: ¿Es segura para tu mascota?",
                    Contenido = "<h2>Dieta cruda</h2><p>La dieta BARF (Biologically Appropriate Raw Food) consiste en alimentar a las mascotas con carne cruda, huesos y vegetales. Tiene defensores y detractores; es vital investigar y consultar a un veterinario.</p>",
                    UrlImagen = "https://imagescdn.estarbien.com.pe/blt3c6af4ceb9664f34/663aa4121fb2b0cbbe51f129/dieta_barf.jpg?format=auto&quality=85",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-13),
                    Tags = "Nutrición, Perros, Gatos"
                },
                // NOTICIA 16
                new Noticia
                {
                    Titulo = "La guía definitiva para leer etiquetas de comida para gatos",
                    Contenido = "<h2>¿Qué contiene realmente?</h2><p>Busca que el primer ingrediente sea una fuente de proteína animal específica (ej. 'Pollo', 'Salmón'). Evita subproductos no especificados y exceso de 'rellenos' como el maíz o el trigo.</p>",
                    UrlImagen = "https://verdecora.es/blog/wp-content/uploads/2017/11/comida-gatos-natural.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-14),
                    Tags = "Gatos, Nutrición"
                },
                // NOTICIA 17
                new Noticia
                {
                    Titulo = "Frutas y verduras seguras para tu perro",
                    Contenido = "<h2>Snacks saludables</h2><p>¡Sí! Muchas frutas son geniales como premios. Las manzanas (sin semillas), los arándanos y los plátanos son opciones seguras y nutritivas. ¡Pero cuidado! Las uvas son muy tóxicas.</p>",
                    UrlImagen = "https://www.google.com/url?sa=i&url=https%3A%2F%2Fwww.expertoanimal.com%2Ffrutas-y-verduras-recomendadas-para-perros-20130.html&psig=AOvVaw14roC2RjV0AW6QssSrWpRf&ust=1762417283224000&source=images&cd=vfe&opi=89978449&ved=0CBUQjRxqFwoTCMDJ7pPK2pADFQAAAAAdAAAAABAM",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-15),
                    Tags = "Perros, Nutrición, Salud"
                },
                // NOTICIA 18
                new Noticia
                {
                    Titulo = "Beneficios del aceite de salmón para la piel de tu perro",
                    Contenido = "<h2>Piel sana, perro feliz</h2><p>El aceite de salmón es rico en ácidos grasos Omega-3 y Omega-6, que ayudan a reducir la inflamación, aliviar la picazón y promover un pelaje brillante y saludable.</p>",
                    UrlImagen = "https://animalnatura.com/img/cms/beneficios-aceite-de-salmon-perros.jpg",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-4),
                    Tags = "Perros, Salud, Nutrición"
                },
                // NOTICIA 19
                new Noticia
                {
                    Titulo = "Entendiendo el lenguaje corporal de tu gato",
                    Contenido = "<h2>¿Qué te quiere decir?</h2><p>Una cola erizada significa miedo o agresión. Un ronroneo no siempre es felicidad, a veces es para calmarse. Aprender a leer sus orejas, ojos y cola es clave para entenderlo.</p>",
                    UrlImagen = "https://www.feliway.es/cdn/shop/articles/Understanding_20Kitten_20Body_20Language_1_20_281_29-1.jpg?v=1674124079&width=1024",
                    FechaPublicacion = DateTime.UtcNow.AddDays(-16),
                    Tags = "Gatos, Comportamiento"
                }

            };

            context.Noticias.AddRange(noticiasIniciales);
            context.SaveChanges(); 

            // 4. Agregar los comentarios a la primera noticia usando el ID generado.
            var noticiaGatoPersa = context.Noticias.FirstOrDefault(n => n.Titulo.Contains("Gato persa"));

            if (noticiaGatoPersa != null)
            {
                var comentariosIniciales = new List<Comentario>
                {
                    // ⬅️ CORRECCIÓN: Fechas dinámicas convertidas a UTC
                    new Comentario { Autor = "David Gor", Texto = "Holaa", FechaComentario = DateTime.UtcNow.AddMinutes(-5), NoticiaId = noticiaGatoPersa.Id },
                    new Comentario { Autor = "David Gor", Texto = "Los gatos persa son muy bonitos >.<", FechaComentario = DateTime.UtcNow.AddMinutes(-2), NoticiaId = noticiaGatoPersa.Id }
                };
                context.Comentarios.AddRange(comentariosIniciales);
                context.SaveChanges();
            }
        }
    }
}