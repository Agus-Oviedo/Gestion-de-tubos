(function () {
    'use strict';
    var clave = 'gestionTubos.respaldoLocal.v1';
    var almacen;

    function fechaArgentina() {
        var partes = new Intl.DateTimeFormat('en-CA', {
            timeZone: 'America/Argentina/Buenos_Aires', year: 'numeric', month: '2-digit', day: '2-digit'
        }).formatToParts(new Date());
        function valor(tipo) { return partes.find(function (p) { return p.type === tipo; }).value; }
        return valor('year') + '-' + valor('month') + '-' + valor('day');
    }

    function abrir() {
        if (almacen) return almacen;
        almacen = new Promise(function (resolve, reject) {
            var req = indexedDB.open('gestionTubos-respaldos', 1);
            req.onupgradeneeded = function () { req.result.createObjectStore('configuracion'); };
            req.onsuccess = function () { resolve(req.result); };
            req.onerror = function () { reject(new Error('No se pudo abrir la configuración local de las copias.')); };
        });
        return almacen;
    }

    async function carpetaGuardada() {
        var db = await abrir();
        return new Promise(function (resolve, reject) {
            var req = db.transaction('configuracion').objectStore('configuracion').get('carpeta');
            req.onsuccess = function () { resolve(req.result); };
            req.onerror = function () { reject(new Error('No se pudo leer la carpeta configurada.')); };
        });
    }

    async function recordar(carpeta) {
        var db = await abrir();
        return new Promise(function (resolve, reject) {
            var tx = db.transaction('configuracion', 'readwrite');
            tx.objectStore('configuracion').put(carpeta, 'carpeta');
            tx.oncomplete = resolve;
            tx.onerror = function () { reject(new Error('No se pudo recordar la carpeta.')); };
        });
    }

    function ultima() {
        try { return JSON.parse(localStorage.getItem(clave) || 'null'); } catch (_) { return null; }
    }

    function resultado(estado, mensaje, carpeta) {
        var copia = ultima();
        return { estado: estado, mensaje: mensaje, carpeta: carpeta ? carpeta.name : '',
            ultimaFecha: copia ? copia.fecha : '', ultimoArchivo: copia ? copia.archivo : '' };
    }

    function errorVisible(error) {
        if (error && error.name === 'AbortError') return 'Operación cancelada. La copia sigue pendiente.';
        if (error && (error.name === 'NotAllowedError' || error.name === 'SecurityError'))
            return 'Se necesita autorizar el acceso a la carpeta para guardar la copia.';
        return error && error.message ? error.message : 'No se pudo guardar la copia. Podés reintentar.';
    }

    async function guardar(carpeta, apiUrl, forzar) {
        // Evita copias duplicadas si hay dos pestañas abiertas en esta PC.
        return navigator.locks.request('gestionTubos-copia-diaria', async function () {
            var previa = ultima();
            var hoy = fechaArgentina();
            if (!forzar && previa && previa.fecha === hoy && previa.carpeta === carpeta.name)
                return resultado('ok', 'La copia de hoy ya está guardada en esta PC.', carpeta);
            if (await carpeta.queryPermission({ mode: 'readwrite' }) !== 'granted')
                return resultado('permiso', 'Autorizá la carpeta para realizar la copia de hoy.', carpeta);

            var token = localStorage.getItem('gestionCilindros.token');
            if (!token) throw new Error('Iniciá sesión nuevamente para realizar la copia.');
            var response = await fetch(apiUrl + 'api/respaldos', {
                method: 'POST', cache: 'no-store', headers: { Authorization: 'Bearer ' + token }
            });
            if (!response.ok) {
                if (response.status === 401) throw new Error('La sesión venció. Volvé a ingresar para realizar la copia.');
                if (response.status === 403) throw new Error('Solo un administrador puede generar la copia de seguridad.');
                if (response.status === 409) throw new Error('Hay otra copia en curso. Reintentá en unos minutos.');
                throw new Error('El servidor no pudo generar la copia. Sigue pendiente; podés reintentar.');
            }
            var expected = response.headers.get('X-Respaldo-Sha256');
            var data = await response.arrayBuffer();
            if (!expected || data.byteLength < 4) throw new Error('La respuesta del respaldo está incompleta.');
            var digest = await crypto.subtle.digest('SHA-256', data);
            var actual = Array.from(new Uint8Array(digest)).map(function (b) { return b.toString(16).padStart(2, '0'); }).join('');
            if (actual.toUpperCase() !== expected.toUpperCase()) throw new Error('La descarga está dañada. La copia sigue pendiente.');
            var cabecera = new Uint8Array(data, 0, 2);
            if (cabecera[0] !== 80 || cabecera[1] !== 75) throw new Error('El archivo recibido no es una copia válida.');
            var nombre = 'oxigeno-norte-' + hoy + '-' + new Date().toISOString().replace(/[-:.]/g, '') + '.zip';
            // No se reemplazan ni eliminan respaldos anteriores.
            var file = await carpeta.getFileHandle(nombre, { create: true });
            var writer = await file.createWritable();
            try {
                await writer.write(data);
                await writer.close();
            } catch (error) {
                try { await writer.abort(); } catch (_) { /* Puede estar cerrado. */ }
                throw error;
            }
            var guardado = await (await file.getFile()).arrayBuffer();
            var hashGuardado = await crypto.subtle.digest('SHA-256', guardado);
            var verificado = Array.from(new Uint8Array(hashGuardado)).map(function (b) { return b.toString(16).padStart(2, '0'); }).join('');
            if (verificado.toUpperCase() !== expected.toUpperCase()) throw new Error('El archivo guardado no pudo verificarse. La copia sigue pendiente.');
            localStorage.setItem(clave, JSON.stringify({ fecha: hoy, archivo: nombre, carpeta: carpeta.name, fechaUtc: new Date().toISOString() }));
            return resultado('ok', 'Copia de hoy guardada y descarga verificada.', carpeta);
        });
    }

    function compatible() { return !!(window.showDirectoryPicker && navigator.locks && window.isSecureContext); }

    window.respaldoLocal = {
        iniciar: async function (apiUrl) {
            try {
                if (!compatible()) return resultado('error', 'Este navegador no permite guardar en una carpeta. Usá Chrome 109 por HTTPS.');
                var carpeta = await carpetaGuardada();
                if (!carpeta) return resultado('configurar', 'Elegí una carpeta de esta PC para guardar una copia cada día al abrir el sistema.');
                return await guardar(carpeta, apiUrl, false);
            } catch (error) { return resultado('error', errorVisible(error)); }
        },
        elegir: async function (apiUrl) {
            try {
                // El selector se abre inmediatamente desde el clic, antes de cualquier espera.
                var carpeta = await window.showDirectoryPicker({ id: 'gestion-tubos-copias', mode: 'readwrite' });
                var anterior = await carpetaGuardada();
                var misma = anterior && await carpeta.isSameEntry(anterior);
                await recordar(carpeta);
                return await guardar(carpeta, apiUrl, !misma);
            } catch (error) { return resultado('error', errorVisible(error)); }
        },
        autorizar: async function (apiUrl) {
            try {
                var carpeta = await carpetaGuardada();
                if (!carpeta) return resultado('configurar', 'Elegí primero una carpeta para las copias.');
                if (await carpeta.requestPermission({ mode: 'readwrite' }) !== 'granted')
                    return resultado('permiso', 'La copia sigue pendiente. Autorizá la escritura en la carpeta.', carpeta);
                return await guardar(carpeta, apiUrl, false);
            } catch (error) { return resultado('error', errorVisible(error)); }
        }
    };
})();
