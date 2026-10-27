// Añade etiquetas sin reemplazar los nodos que administra Blazor.
(() => {
    let pendiente = false;
    function adaptar() {
        pendiente = false;
        document.querySelectorAll('.table-responsive table.table').forEach(tabla => {
            const titulos = [];
            tabla.querySelectorAll('thead tr:first-child th').forEach(th => {
                for (let i = 0; i < th.colSpan; i++) titulos.push(th.textContent.trim());
            });
            if (!titulos.length) return;
            tabla.dataset.mobileCards = 'true';
            tabla.querySelectorAll('tbody tr').forEach(fila => {
                let indice = 0;
                fila.querySelectorAll(':scope > td').forEach(celda => {
                    if (celda.colSpan > 1) {
                        celda.removeAttribute('data-label');
                    } else {
                        const etiqueta = titulos[indice] || (celda.querySelector('input[type="checkbox"]') ? 'Seleccionar' : 'Acciones');
                        if (celda.dataset.label !== etiqueta) celda.dataset.label = etiqueta;
                    }
                    indice += celda.colSpan;
                });
            });
        });
    }
    function programar() { if (!pendiente) { pendiente = true; requestAnimationFrame(adaptar); } }
    new MutationObserver(programar).observe(document.body, { childList: true, subtree: true });
    programar();
})();
