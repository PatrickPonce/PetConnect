// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// --- Lógica para el Menú Desplegable de Usuario ---

document.addEventListener('DOMContentLoaded', function() {
    
    // Seleccionamos el botón y el menú
    const userMenuButton = document.getElementById('user-menu-button');
    const userMenuDropdown = document.getElementById('user-menu-dropdown');

    if (userMenuButton && userMenuDropdown) {
        
        // 1. Al hacer clic en el botón
        userMenuButton.addEventListener('click', function(event) {
            // Detenemos la propagación para que el clic en 'window' no se active
            event.stopPropagation();
            // Mostramos u ocultamos el menú
            userMenuDropdown.classList.toggle('show');
        });

        // 2. Al hacer clic en cualquier parte de la página
        window.addEventListener('click', function(event) {
            // Si el menú está abierto y no se hizo clic en el botón
            if (userMenuDropdown.classList.contains('show')) {
                // Ocultamos el menú
                userMenuDropdown.classList.remove('show');
            }
        });
        
        // 3. (Opcional) Evitar que el menú se cierre si haces clic dentro de él
        userMenuDropdown.addEventListener('click', function(event) {
            event.stopPropagation();
        });
    }
});