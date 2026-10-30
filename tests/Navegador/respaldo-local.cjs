const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const { webcrypto, createHash } = require('node:crypto');

const source = fs.readFileSync('GestionTubos.Client/wwwroot/respaldo-local.js', 'utf8');
const local = new Map([['gestionCilindros.token', 'token-de-prueba']]);
const records = new Map();
let ahora = '2026-10-30T15:00:00Z';
let permiso = 'granted', concede = true, fallaRed = false, fallaDisco = false, corrupta = false;
let descargas = 0, escrituras = 0;
const copia = Buffer.from('PK-respaldo-de-prueba');
const hash = createHash('sha256').update(copia).digest('hex');
const carpeta = {
    name: 'Copias de prueba',
    async queryPermission() { return permiso; },
    async requestPermission() { return concede ? (permiso = 'granted') : 'denied'; },
    async isSameEntry(other) { return other === carpeta; },
    async getFileHandle() {
        let contenido;
        return {
            async createWritable() {
                return {
                    async write(data) { if (fallaDisco) throw new Error('Disco lleno'); contenido = data; },
                    async close() { escrituras++; }, async abort() {}
                };
            },
            async getFile() { return { async arrayBuffer() { return contenido; } }; }
        };
    }
};
let cola = Promise.resolve();
const contexto = {
    console, Intl, crypto:webcrypto, Uint8Array, ArrayBuffer,
    Date: class extends Date { constructor(...args) { super(...(args.length ? args : [ahora])); } },
    localStorage: {getItem:key=>local.get(key)||null, setItem:(key,value)=>local.set(key,value)},
    window: {isSecureContext:true, async showDirectoryPicker() { return carpeta; }},
    navigator: {locks:{request(_name, operation) { const next=cola.then(operation); cola=next.catch(()=>{}); return next; }}},
    indexedDB: {
        open() {
            const db = {
                createObjectStore() {},
                transaction() {
                    const tx = {objectStore() {return {
                        get(key) { const req={}; queueMicrotask(()=>{req.result=records.get(key); req.onsuccess();}); return req; },
                        put(value,key) { records.set(key,value); queueMicrotask(()=>tx.oncomplete()); }
                    };}};
                    return tx;
                }
            };
            const req={result:db}; queueMicrotask(()=>{req.onupgradeneeded(); req.onsuccess();}); return req;
        }
    },
    async fetch() {
        descargas++;
        if (fallaRed) return {ok:false,status:503};
        return {ok:true,headers:{get:()=>corrupta?'00':hash},async arrayBuffer(){return copia.buffer.slice(copia.byteOffset,copia.byteOffset+copia.byteLength);}};
    }
};
vm.runInNewContext(source,contexto);
const api=contexto.window.respaldoLocal;
(async()=>{
    assert.equal((await api.iniciar('/')).estado,'configurar');
    assert.equal(descargas,0,'No descarga sin carpeta elegida');
    assert.equal((await api.elegir('/')).estado,'ok');
    assert.equal(escrituras,1);
    await Promise.all([api.iniciar('/'),api.iniciar('/')]);
    assert.equal(descargas,1,'No duplica la copia diaria ni con dos pestañas');
    ahora='2026-10-31T15:00:00Z'; permiso='prompt';
    assert.equal((await api.iniciar('/')).estado,'permiso');
    assert.equal(descargas,1,'No pide permiso automáticamente ni marca copia inexistente');
    concede=false;
    assert.equal((await api.autorizar('/')).estado,'permiso');
    concede=true;
    assert.equal((await api.autorizar('/')).estado,'ok');
    assert.equal(escrituras,2);
    const correcta=local.get('gestionTubos.respaldoLocal.v1');
    ahora='2026-11-01T15:00:00Z'; fallaRed=true;
    assert.equal((await api.iniciar('/')).estado,'error');
    assert.equal(local.get('gestionTubos.respaldoLocal.v1'),correcta,'Una falla de red no marca éxito');
    fallaRed=false; corrupta=true;
    assert.equal((await api.iniciar('/')).estado,'error');
    assert.equal(escrituras,2,'No escribe descargas corruptas');
    corrupta=false; fallaDisco=true;
    assert.equal((await api.iniciar('/')).estado,'error');
    assert.equal(local.get('gestionTubos.respaldoLocal.v1'),correcta,'Un fallo de disco no marca éxito');
    fallaDisco=false;
    assert.equal((await api.iniciar('/')).estado,'ok');
    assert.equal(escrituras,3);
    console.log('Copias locales: selección inicial, una por día, concurrencia, permisos, descarga corrupta y fallos de red/disco verificados.');
})().catch(error=>{console.error(error);process.exitCode=1;});
