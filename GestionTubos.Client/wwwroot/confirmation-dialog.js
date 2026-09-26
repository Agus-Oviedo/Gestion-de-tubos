export function abrir(dialog) {
    dialog.dataset.busy = "false";
    dialog.oncancel = event => {
        if (dialog.dataset.busy === "true") event.preventDefault();
    };
    if (!dialog.open) dialog.showModal();
}
export function procesando(dialog) {
    dialog.dataset.busy = "true";
}
export function cerrar(dialog) {
    dialog.dataset.busy = "false";
    if (dialog.open) dialog.close();
}
