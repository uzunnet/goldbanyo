/**
 * DesaDoor Tema Motoru
 * localStorage + CSS degiskenleri ile admin panel temasini canli degistirir.
 */
window.desadoorTema = {
    uygula: function (birincil, vurgu, arkaPlan, yuzey, koyuTemaMi) {
        var root = document.documentElement;
        root.style.setProperty('--admin-bg', arkaPlan);
        root.style.setProperty('--admin-surface', yuzey);
        root.style.setProperty('--admin-surface-hover', koyuTemaMi ? '#1a1a1a' : '#f0f0f0');
        root.style.setProperty('--admin-surface-raised', koyuTemaMi ? '#161616' : '#fafafa');
        root.style.setProperty('--admin-primary', birincil);
        root.style.setProperty('--admin-primary-light', koyuTemaMi ? '#151515' : '#f5f5f5');
        root.style.setProperty('--admin-accent', vurgu);
        root.style.setProperty('--admin-accent-light', vurgu + 'CC');
        root.style.setProperty('--admin-text', koyuTemaMi ? '#ffffff' : '#1a1a1a');
        root.style.setProperty('--admin-text-secondary', koyuTemaMi ? 'rgba(255,255,255,0.70)' : 'rgba(0,0,0,0.65)');
        root.style.setProperty('--admin-text-muted', koyuTemaMi ? 'rgba(255,255,255,0.40)' : 'rgba(0,0,0,0.40)');
        root.style.setProperty('--admin-text-dim', koyuTemaMi ? 'rgba(255,255,255,0.20)' : 'rgba(0,0,0,0.15)');
        root.style.setProperty('--admin-border', koyuTemaMi ? 'rgba(255,255,255,0.06)' : 'rgba(0,0,0,0.08)');
        root.style.setProperty('--admin-border-light', koyuTemaMi ? 'rgba(255,255,255,0.04)' : 'rgba(0,0,0,0.04)');
        root.style.setProperty('--admin-border-focus', 'rgba(' + this._hexToRgb(vurgu) + ',0.25)');
        root.style.setProperty('--admin-shadow', koyuTemaMi ? '0 4px 24px rgba(0,0,0,0.3)' : '0 4px 24px rgba(0,0,0,0.06)');
        root.style.setProperty('--admin-shadow-hover', koyuTemaMi ? '0 8px 32px rgba(0,0,0,0.5)' : '0 8px 32px rgba(0,0,0,0.1)');
        root.style.setProperty('--admin-shadow-glow', '0 0 20px rgba(' + this._hexToRgb(vurgu) + ',0.08)');

        console.log('[DesaDoor Tema] ' + (koyuTemaMi ? 'Karanlik' : 'Aydinlik') + ' tema uygulandi: ' + vurgu);
    },

    _hexToRgb: function (hex) {
        var h = hex.replace('#', '');
        if (h.length === 3) h = h[0] + h[0] + h[1] + h[1] + h[2] + h[2];
        var r = parseInt(h.substring(0, 2), 16);
        var g = parseInt(h.substring(2, 4), 16);
        var b = parseInt(h.substring(4, 6), 16);
        return r + ',' + g + ',' + b;
    }
};
