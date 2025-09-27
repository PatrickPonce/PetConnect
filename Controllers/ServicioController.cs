using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using PetConnect.Models;

namespace PetConnect.Controllers;

public class ServicioController : Controller
{
    // Datos inventados para las veterinarias
    private List<Servicio> GetVeterinarias()
    {
        return new List<Servicio>
        {
            new Servicio
            {
                Id = 1,
                Nombre = "Clínica Veterinaria VetAmigo",
                DescripcionCorta = "Atención integral para tus mascotas. Consultas, vacunas y cirugías con un equipo de profesionales apasionados.",
                DescripcionLarga = @"
                    <p>En <strong>Clínica Veterinaria VetAmigo</strong>, entendemos que tu mascota es parte de tu familia. Por eso, ofrecemos un servicio completo y cercano para garantizar su bienestar en cada etapa de su vida.</p>
                    <h2>Nuestros Servicios Principales</h2>
                    <ul>
                        <li>Consultas generales y de especialidad.</li>
                        <li>Planes de vacunación y desparasitación.</li>
                        <li>Cirugías menores y de alta complejidad.</li>
                        <li>Laboratorio y diagnóstico por imágenes.</li>
                        <li>Peluquería y spa canino y felino.</li>
                    </ul>
                    <p>Contamos con tecnología de punta y un equipo humano que ama a los animales. ¡Tu mejor amigo está en las mejores manos!</p>",
                UrlImagen = "/images/servicios/veterinaria-1.png", // Debes crear esta ruta y poner una imagen
                Direccion = "Av. Siempre Viva 123, Miraflores",
                Telefono = "(01) 555-1111",
                Horario = "Lunes a Sábado de 9:00 a 20:00",
                Resenas = new List<Resena>
                {
                    new Resena { Autor = "Ana López", Texto = "¡Excelente atención! El Dr. Pérez fue muy amable.", Puntuacion = 5, FechaResena = DateTime.Now.AddDays(-2) },
                    new Resena { Autor = "Carlos a", Texto = "Llevé a mi perrito por una emergencia y lo salvaron. Muy agradecido.", Puntuacion = 5, FechaResena = DateTime.Now.AddDays(-5) }
                }
            },
            new Servicio
            {
                Id = 2,
                Nombre = "Hospital Animalia 24h",
                DescripcionCorta = "Emergencias y cuidados intensivos las 24 horas del día, los 7 días de la semana. Siempre listos para atenderte.",
                DescripcionLarga = @"
                    <p>La salud de tu mascota no tiene horario. En <strong>Hospital Animalia 24h</strong>, estamos disponibles día y noche para atender cualquier emergencia. Nuestro centro cuenta con una unidad de cuidados intensivos (UCI) y especialistas de turno permanente.</p>
                    <h2>Especialidades y Emergencias</h2>
                    <ul>
                        <li>Atención de emergencias 24/7.</li>
                        <li>Hospitalización y monitoreo continuo.</li>
                        <li>Especialistas en cardiología, neurología y oncología.</li>
                        <li>Banco de sangre para perros y gatos.</li>
                    </ul>",
                UrlImagen = "/images/servicios/veterinaria-2.jpg",
                Direccion = "Calle La Urgencia 456, San Borja",
                Telefono = "(01) 555-2222",
                Horario = "Abierto 24 horas, todos los días",
            },
            new Servicio
            {
                Id = 3,
                Nombre = "Centro Felino CatCare",
                DescripcionCorta = "Un espacio exclusivo para el cuidado de tus gatos. Ambiente libre de estrés y especialistas en medicina felina.",
                DescripcionLarga = @"
                    <p>Sabemos que los gatos son especiales y merecen un trato único. <strong>CatCare</strong> es una clínica diseñada exclusivamente para ellos, eliminando el estrés que les provocan los perros y otros animales. Nuestro personal está altamente capacitado en comportamiento y medicina felina.</p>
                    <h2>Servicios Exclusivos para Gatos</h2>
                    <ul>
                        <li>Consultas en un ambiente tranquilo.</li>
                        <li>Cirugía y hospitalización en áreas separadas.</li>
                        <li>Especialista en comportamiento felino.</li>
                        <li>Peluquería especializada en gatos, sin sedación.</li>
                    </ul>",
                UrlImagen = "/images/servicios/veterinaria-3.jpg",
                Direccion = "Jr. Los Gatos 789, Surco",
                Telefono = "(01) 555-3333",
                Horario = "Lunes a Viernes de 10:00 a 19:00",
            }
        };
    }

    // Muestra la lista de todas las veterinarias (similar a tu Index de Noticias)
    public IActionResult Index()
    {
        var veterinarias = GetVeterinarias();
        return View(veterinarias);
    }

    // Muestra el detalle de una veterinaria específica (similar a tu Detalle de Noticias)
    public IActionResult Detalle(int? id)
    {
        if (id == null) return NotFound();

        var veterinaria = GetVeterinarias().FirstOrDefault(v => v.Id == id);

        if (veterinaria == null) return NotFound();

        return View(veterinaria);
    }
}