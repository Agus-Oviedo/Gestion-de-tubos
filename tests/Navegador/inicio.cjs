// Probar la publicación real: no basta con cambiar el agente de usuario.
const assert = require('node:assert/strict');
const { chromium } = require('playwright');
const fs = require('node:fs/promises');

(async () => {
    const browser = await chromium.launch({ headless: true });
    const errores = [];
    try {
        assert.match(browser.version(), /^109\./, 'La prueba debe usar Chromium 109');
        const page = await browser.newPage();
        page.on('pageerror', error => errores.push(error.message));
        page.on('console', message => {
            if (message.type() === 'error') errores.push(message.text());
        });
        const url = process.env.TEST_URL || 'http://127.0.0.1:8765/';
        await page.goto(url, { waitUntil: 'domcontentloaded' });
        await page.locator('input[name="username"]').waitFor({ state: 'visible', timeout: 120000 });
        await page.locator('input[name="password"]').waitFor({ state: 'visible' });
        assert.equal(await page.locator('.loading-progress').count(), 0);
        assert.deepEqual(errores, [], 'No debe haber errores de arranque');
        console.log(`Inicio correcto en Chromium ${browser.version()}: formulario de acceso visible.`);

        // Sesión y API simuladas exclusivamente contra la publicación local.
        // Permite probar MainLayout y NavMenu sin credenciales ni datos de producción.
        assert.equal(new URL(url).hostname, '127.0.0.1');
        const payload = Buffer.from(JSON.stringify({
            unique_name: 'Prueba de interfaz', role: 'Superadministrador',
            exp: Math.floor(Date.now() / 1000) + 3600
        })).toString('base64url');
        await page.addInitScript(token => localStorage.setItem('gestionCilindros.token', token), `e30.${payload}.prueba`);
        await page.route('**/api/**', route => route.fulfill({contentType:'application/json', body:'[]'}));
        await page.setViewportSize({width:1366, height:900});
        await page.goto(url);
        await page.getByRole('heading', {name:'Control de stock', exact:true}).waitFor({state:'visible'});
        const logo = page.locator('.navbar-brand img');
        const sidebar = page.locator('.sidebar');
        assert.ok((await logo.boundingBox()).width <= 210, 'El logo debe conservar su tamaño de escritorio');
        assert.equal(await sidebar.evaluate(element => getComputedStyle(element).width), '270px');
        assert.equal(await page.locator('.page').evaluate(element => getComputedStyle(element).flexDirection), 'row');
        await fs.mkdir('.build-check/capturas', {recursive:true});
        await page.screenshot({path:'.build-check/capturas/escritorio.png', fullPage:true});

        await page.setViewportSize({width:390, height:844});
        assert.ok((await logo.boundingBox()).width <= 110, 'El logo debe reducirse en móvil');
        await page.getByRole('button', {name:'Abrir menú de navegación'}).click();
        await page.getByRole('link', {name:'Resumen',exact:true}).waitFor({state:'visible'});
        assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), 'No debe haber desborde horizontal');
        await page.screenshot({path:'.build-check/capturas/movil.png', fullPage:true});
        assert.deepEqual(errores, [], 'El resumen de stock debe abrir sin errores');
        console.log('Diseño de escritorio y móvil verificado en Chromium 109.');
    } catch (error) {
        console.error('Errores del navegador:', errores);
        throw error;
    } finally {
        await browser.close();
    }
})().catch(error => {
    console.error(error);
    process.exitCode = 1;
});
