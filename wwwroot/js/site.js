// Opcional: Script para manejar el estado activo al hacer clic
document.addEventListener('DOMContentLoaded', function () {
    const navLinks = document.querySelectorAll('.main-nav a');

    navLinks.forEach(link => {
        link.addEventListener('click', function(event) {
            // Si no es una SPA real, quita la siguiente línea para permitir la navegación
            // event.preventDefault(); 

            // Quita la clase 'active' de cualquier enlace que la tuviera
            document.querySelector('.main-nav a.active')?.classList.remove('active');
            
            // Añade la clase 'active' al enlace que fue clickeado
            this.classList.add('active');
        });
    });
});



document.addEventListener("DOMContentLoaded", function() {

    // --- SCRIPT DE ANIMACIÓN DE TARJETAS AL HACER SCROLL ---
    const animatedCards = document.querySelectorAll('.servicio-card, .noticia-card'); // Buscamos ambos tipos de tarjetas
    if (animatedCards.length > 0) {
        const options = { threshold: 0.1 };
        const observer = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, options);
        animatedCards.forEach(card => observer.observe(card));
    }

    // --- SCRIPT PARA EL BOTÓN DE FAVORITO ANIMADO ---
    const favIcons = document.querySelectorAll('.fav-icon');
    favIcons.forEach(icon => {
        icon.addEventListener('click', function() {
            this.classList.toggle('active');
            this.classList.add('is-animating');
        });

        icon.addEventListener('animationend', function() {
            this.classList.remove('is-animating');
        });
    });
});