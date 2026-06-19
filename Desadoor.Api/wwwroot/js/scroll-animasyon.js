/* Ustam'in istedigi animasyon sistemi — SPA dahil tum sayfalarda */
(function(){
var animasyonAcikMi = function() { return localStorage.getItem('desadoor_animasyon') !== 'kapali'; };

var tumScrollTriggerlariTemizle = function() {
    if (typeof ScrollTrigger !== 'undefined') ScrollTrigger.getAll().forEach(function(st) { st.kill(); });
};

var animasyonKur = function() {
    if (!animasyonAcikMi()) { tumScrollTriggerlariTemizle(); return; }
    if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;
    
    tumScrollTriggerlariTemizle();
    gsap.registerPlugin(ScrollTrigger);
    
    // 1. SAYFA YUKLENINCE (sadece varsa)
    var els = document.querySelectorAll('.anim-yazi-sol'); if (els.length) gsap.fromTo(els, { x: -40, opacity: 0 }, { x: 0, opacity: 1, duration: 0.9, ease: "power3.out", delay: 0.1 });
    var els2 = document.querySelectorAll('.anim-yazi-sag'); if (els2.length) gsap.fromTo(els2, { x: 40, opacity: 0 }, { x: 0, opacity: 1, duration: 0.9, ease: "power3.out", delay: 0.3 });
    var els3 = document.querySelectorAll('.anim-resim'); if (els3.length) gsap.fromTo(els3, { opacity: 0, scale: 0.95 }, { opacity: 1, scale: 1, duration: 1.2, ease: "power2.out", delay: 0.5 });
    
    // 2. SCROLL — sadece gelir, kaybolmaz
    var yukariEls = document.querySelectorAll('.anim-scroll-yukari');
    var solEls = document.querySelectorAll('.anim-scroll-sol');
    var sagEls = document.querySelectorAll('.anim-scroll-sag');
    var resimEls = document.querySelectorAll('.anim-scroll-resim');
    yukariEls.forEach(function(el) {
        gsap.set(el, { visibility: 'visible' });
        gsap.fromTo(el, { y: 60, opacity: 0 }, { y: 0, opacity: 1, duration: 0.9, ease: "power2.out",
            scrollTrigger: { trigger: el, start: "top bottom-=30", toggleActions: "play none none none", once: true } });
    });
    solEls.forEach(function(el) {
        gsap.set(el, { visibility: 'visible' });
        gsap.fromTo(el, { x: -60, opacity: 0 }, { x: 0, opacity: 1, duration: 0.9, ease: "power2.out",
            scrollTrigger: { trigger: el, start: "top bottom-=30", toggleActions: "play none none none", once: true } });
    });
    sagEls.forEach(function(el) {
        gsap.set(el, { visibility: 'visible' });
        gsap.fromTo(el, { x: 60, opacity: 0 }, { x: 0, opacity: 1, duration: 0.9, ease: "power2.out",
            scrollTrigger: { trigger: el, start: "top bottom-=30", toggleActions: "play none none none", once: true } });
    });
    resimEls.forEach(function(el) {
        gsap.set(el, { visibility: 'visible' });
        gsap.fromTo(el, { opacity: 0, scale: 0.9 }, { opacity: 1, scale: 1, duration: 1.0, ease: "power2.out",
            scrollTrigger: { trigger: el, start: "top bottom-=30", toggleActions: "play none none none", once: true } });
    });
    gsap.utils.toArray('.anim-scroll-sol').forEach(function(el) {
        gsap.set(el, { visibility: 'visible' });
        gsap.fromTo(el, { x: -80, opacity: 0 }, { x: 0, opacity: 1, duration: 1.0, ease: "power2.out",
            scrollTrigger: { trigger: el, start: "top bottom-=40", toggleActions: "play reverse play reverse" } });
    });
    gsap.utils.toArray('.anim-scroll-sag').forEach(function(el) {
        gsap.set(el, { visibility: 'visible' });
        gsap.fromTo(el, { x: 80, opacity: 0 }, { x: 0, opacity: 1, duration: 1.0, ease: "power2.out",
            scrollTrigger: { trigger: el, start: "top bottom-=40", toggleActions: "play reverse play reverse" } });
    });
    gsap.utils.toArray('.anim-scroll-resim').forEach(function(el) {
        gsap.set(el, { visibility: 'visible' });
        gsap.fromTo(el, { opacity: 0, scale: 0.88 }, { opacity: 1, scale: 1, duration: 1.1, ease: "power2.out",
            scrollTrigger: { trigger: el, start: "top bottom-=40", toggleActions: "play reverse play reverse" } });
    });
    
    ScrollTrigger.refresh();
};

// Admin panelden cagrilabilir — tum animasyonlari kapat
window.desadoorAnimasyon = {
    tumunuKapat: function() {
        tumScrollTriggerlariTemizle();
        localStorage.setItem('desadoor_animasyon', 'kapali');
        document.querySelectorAll('[class*="anim-scroll"]').forEach(function(el) { el.style.visibility = 'visible'; el.style.opacity = '1'; });
    },
    tumunuAc: function() {
        localStorage.setItem('desadoor_animasyon', 'acik');
        animasyonKur();
    },
    baslatScrollAnimasyonlari: function() { animasyonKur(); },
    yenile: function() { if (typeof ScrollTrigger !== 'undefined') ScrollTrigger.refresh(); }
};

/* Sohbet widget'i — eval kullanmadan scroll islemi */
window.sohbetAlaniEnAltaKaydir = function() {
    var el = document.getElementById('sohbet-alani');
    if (el) el.scrollTop = el.scrollHeight;
};

// Ilk kurulum
var kuruldu = false;
var tara = setInterval(function() {
    if (!kuruldu && document.querySelector('.anim-scroll-yukari, .anim-scroll-sol')) {
        kuruldu = true; clearInterval(tara); setTimeout(animasyonKur, 300);
    }
}, 500);

// SPA sayfa gecislerinde yeniden kur
new MutationObserver(function() {
    setTimeout(function() { tumScrollTriggerlariTemizle(); animasyonKur(); }, 400);
}).observe(document.getElementById('app') || document.body, { childList: true, subtree: true });

setTimeout(function() { if (!kuruldu) { clearInterval(tara); animasyonKur(); } }, 12000);
})();
