using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Controllers/PetShopController.cs
using Microsoft.AspNetCore.Mvc;
using PetConnect.Models;

namespace PetConnect.Controllers;

public class PetShopController : Controller
{
    // Método privado para generar los datos de ejemplo de Pet Shops
    private List<Servicio> GetPetShops()
    {
        return new List<Servicio>
        {
            new Servicio
            {
                Id = 4, // Usamos IDs nuevos para no chocar con las veterinarias
                Nombre = "La Casa de Bigotes",
                DescripcionCorta = "Todo lo que tu perro o gato puede soñar: alimentos, juguetes y accesorios.",
                DescripcionLarga = @"
                    <p><strong>La Casa de Bigotes</strong> es más que una tienda, es un paraíso para tus mascotas. Nos especializamos en ofrecer una selección curada de los mejores productos del mercado.</p>
                    <h2>¿Qué encontrarás?</h2>
                    <ul>
                        <li>Alimentos premium y dietas especiales.</li>
                        <li>Juguetes interactivos para estimular su mente.</li>
                        <li>Camas cómodas y rascadores para gatos.</li>
                        <li>Correas, arneses y ropa de moda.</li>
                    </ul>",
                UrlImagen = "/images/servicios/petshop-1.jpg", // Debes añadir esta imagen
                Direccion = "Av. Los Mimosos 111, La Molina",
                Telefono = "(01) 555-4444",
                Horario = "Lunes a Domingo de 10:00 a 21:00",
                Resenas = new List<Resena>
                {
                    new Resena { Autor = "Lucía Ferreyros", Texto = "La mejor variedad de juguetes, a mi perro le encantó.", Puntuacion = 5, FechaResena = DateTime.Now.AddDays(-1) }
                }
            },
            new Servicio
            {
                Id = 5,
                Nombre = "Acuario & Exóticos Mundo Animal",
                DescripcionCorta = "El lugar para los amantes de los peces, reptiles y aves. Asesoría experta.",
                DescripcionLarga = @"
                    <p>¿Buscas algo más que perros y gatos? En <strong>Mundo Animal</strong> nos especializamos en el fascinante mundo de los animales exóticos y acuáticos. Contamos con todo lo necesario para crear el hábitat perfecto.</p>
                    <h2>Nuestras Secciones</h2>
                    <ul>
                        <li>Acuarios de agua dulce y salada.</li>
                        <li>Terrarios para reptiles y anfibios.</li>
                        <li>Alimento vivo y especializado.</li>
                        <li>Jaulas y accesorios para aves y roedores.</li>
                    </ul>",
                UrlImagen = "/images/servicios/petshop-2.jpg",
                Direccion = "Calle El Océano 222, San Isidro",
                Telefono = "(01) 555-5555",
                Horario = "Martes a Sábado de 11:00 a 19:00",
            },
            new Servicio
            {
                Id = 6,
                Nombre = "Patitas Boutique & Spa",
                DescripcionCorta = "Lujo y cuidado para tu mascota. Peluquería, spa y accesorios de alta gama.",
                DescripcionLarga = @"
                    <p>En <strong>Patitas Boutique & Spa</strong>, consentimos a tu mascota como se merece. Ofrecemos servicios de grooming de alta calidad y una selección de accesorios de diseñador.</p>
                    <h2>Servicios de Lujo</h2>
                    <ul>
                        <li>Peluquería canina con estilistas profesionales.</li>
                        <li>Tratamientos de spa: masajes, aromaterapia y baños relajantes.</li>
                        <li>Accesorios de lujo: collares de cuero, ropa de diseñador.</li>
                        <li>Golosinas y pastelería gourmet para mascotas.</li>
                    </ul>",
                UrlImagen = "/images/servicios/petshop-3.jpg",
                Direccion = "Boulevard del Lujo 333, Asia",
                Telefono = "(01) 555-6666",
                Horario = "Jueves a Domingo de 12:00 a 22:00",
            }
        };
    }

    // Muestra la lista de todas las tiendas
    public IActionResult Index()
    {
        var petShops = GetPetShops();
        return View(petShops);
    }

    // Muestra el detalle de una tienda específica
    public IActionResult Detalle(int? id)
    {
        if (id == null) return NotFound();
        var petShop = GetPetShops().FirstOrDefault(p => p.Id == id);
        if (petShop == null) return NotFound();
        return View(petShop);
    }
}