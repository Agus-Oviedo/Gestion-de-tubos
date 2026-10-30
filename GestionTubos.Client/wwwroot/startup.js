(function () {
    function mostrarError(error) {
        console.error('No se pudo iniciar Blazor.', error);
        var panel = document.getElementById('startup-error');
        var detalle = document.getElementById('startup-error-detail');
        // Blazor puede haber reemplazado la pantalla inicial antes de fallar.
        if (!panel || !detalle) return;
        detalle.textContent = error && error.message ? error.message : String(error);
        panel.hidden = false;
        document.querySelectorAll('.loading-progress, .loading-progress-text').forEach(function (elemento) {
            elemento.style.display = 'none';
        });
    }

    try {
        if (!window.Blazor) {
            throw new Error('No se pudo descargar el componente de inicio. Verificá la conexión y recargá la página.');
        }
        window.Blazor.start().catch(mostrarError);
    } catch (error) {
        mostrarError(error);
    }
})();
