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