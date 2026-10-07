window.firmaRemito = (() => {
    const pads = new Map();

    function preparar(canvas) {
        const rect = canvas.getBoundingClientRect();
        const ratio = Math.max(window.devicePixelRatio || 1, 1);
        canvas.width = Math.round(rect.width * ratio);
        canvas.height = Math.round(rect.height * ratio);
        const ctx = canvas.getContext('2d');
        ctx.setTransform(ratio, 0, 0, ratio, 0, 0);
        ctx.lineWidth = 2.4;
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';
        ctx.strokeStyle = '#082f6b';
        ctx.fillStyle = '#fff';
        ctx.fillRect(0, 0, rect.width, rect.height);
        return ctx;
    }

    function inicializar(id) {
        const canvas = document.getElementById(id);
        if (!canvas) return;
        const ctx = preparar(canvas);
        const pad = { canvas, ctx, dibujando: false, firmada: false };
        pads.set(id, pad);

        const punto = e => {
            const r = canvas.getBoundingClientRect();
            return { x: e.clientX - r.left, y: e.clientY - r.top };
        };
        canvas.onpointerdown = e => {
            e.preventDefault();
            canvas.setPointerCapture(e.pointerId);
            const p = punto(e);
            pad.dibujando = true;
            pad.ctx.beginPath();
            pad.ctx.moveTo(p.x, p.y);
        };
        canvas.onpointermove = e => {
            if (!pad.dibujando) return;
            e.preventDefault();
            const p = punto(e);
            pad.ctx.lineTo(p.x, p.y);
            pad.ctx.stroke();
            pad.firmada = true;
        };
        const terminar = e => {
            if (pad.dibujando && canvas.hasPointerCapture(e.pointerId)) canvas.releasePointerCapture(e.pointerId);
            pad.dibujando = false;
        };
        canvas.onpointerup = terminar;
        canvas.onpointercancel = terminar;
    }

    function limpiar(id) {
        const pad = pads.get(id);
        if (!pad) return;
        pad.ctx = preparar(pad.canvas);
        pad.firmada = false;
    }

    function obtener(id) {
        const pad = pads.get(id);
        return pad && pad.firmada ? pad.canvas.toDataURL('image/png') : '';
    }

    return { inicializar, limpiar, obtener };
})();

let ventanaWhatsappPendiente = null;

window.prepararWhatsApp = () => {
    ventanaWhatsappPendiente = window.open('about:blank', 'whatsapp-remito');
};

window.abrirWhatsApp = (numero, mensaje) => {
    const destino = String(numero || '').replace(/\D/g, '');
    if (!destino) return;
    const url = `https://wa.me/${destino}?text=${encodeURIComponent(mensaje || '')}`;
    if (ventanaWhatsappPendiente && !ventanaWhatsappPendiente.closed) {
        ventanaWhatsappPendiente.location.href = url;
        ventanaWhatsappPendiente = null;
    } else {
        window.open(url, '_blank', 'noopener,noreferrer');
    }
};

window.guardarRemitoPdf = (nombreArchivo) => {
    const tituloAnterior = document.title;
    document.title = String(nombreArchivo || 'remito').replace(/[^a-zA-Z0-9_-]/g, '_');
    const restaurar = () => {
        document.title = tituloAnterior;
        window.removeEventListener('afterprint', restaurar);
    };
    window.addEventListener('afterprint', restaurar);
    window.print();
    window.setTimeout(restaurar, 30000);
};

window.descargarArchivoBase64 = (nombreArchivo, tipo, base64) => {
    const enlace = document.createElement('a');
    enlace.href = `data:${tipo};base64,${base64}`;
    enlace.download = nombreArchivo;
    document.body.appendChild(enlace);
    enlace.click();
    enlace.remove();
};
