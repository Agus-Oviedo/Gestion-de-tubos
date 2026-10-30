// Probar la publicación real: no basta con cambiar el agente de usuario.
const assert = require('node:assert/strict');
const { chromium } = require('playwright');

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
