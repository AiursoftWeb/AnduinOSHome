(() => {
    'use strict';

    const root = document.querySelector('.pro-experience');
    if (!root) return;

    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const desktop = root.querySelector('.pro-desktop');
    const stage = root.querySelector('.pro-desktop-stage');
    const sections = root.querySelectorAll('[data-pro-reveal]');
    let observer;
    let framePending = false;

    function revealAll() {
        observer?.disconnect();
        sections.forEach(section => section.classList.remove('pro-awaiting-reveal'));
        root.classList.remove('pro-motion');
    }

    function configureMotion() {
        revealAll();
        if (!reducedMotion.matches && 'IntersectionObserver' in window) {
            observer = new IntersectionObserver(entries => {
                entries.forEach(entry => {
                    if (!entry.isIntersecting) return;
                    entry.target.classList.remove('pro-awaiting-reveal');
                    observer.unobserve(entry.target);
                });
            }, { threshold: 0, rootMargin: '0px 0px 24px 0px' });

            root.classList.add('pro-motion');
            sections.forEach(section => {
                // Already-visible content never flashes or waits for an animation.
                if (section.getBoundingClientRect().top > window.innerHeight) {
                    section.classList.add('pro-awaiting-reveal');
                    observer.observe(section);
                }
            });
        }
        queueFrame();
    }

    function updateScroll() {
        framePending = false;
        const maxScroll = document.documentElement.scrollHeight - window.innerHeight;
        const progress = maxScroll > 0 ? Math.min(1, Math.max(0, window.scrollY / maxScroll)) : 0;
        root.style.setProperty('--pro-reading-progress', progress.toFixed(4));

        if (reducedMotion.matches || window.innerWidth < 701) {
            desktop.style.removeProperty('--pro-desktop-y');
            desktop.style.removeProperty('--pro-desktop-tilt');
            return;
        }

        const rect = stage.getBoundingClientRect();
        if (rect.bottom < 0 || rect.top > window.innerHeight) return;
        const position = Math.min(1, Math.max(0, (window.innerHeight - rect.top) / (window.innerHeight + rect.height)));
        desktop.style.setProperty('--pro-desktop-tilt', `${(1 - position) * 5}deg`);
        desktop.style.setProperty('--pro-desktop-y', `${(1 - position) * 10}px`);
    }

    function queueFrame() {
        if (framePending) return;
        framePending = true;
        window.requestAnimationFrame(updateScroll);
    }

    // Native anchors keep navigation and keyboard behavior available without JS.
    root.querySelectorAll('a[href^="#pro-"]').forEach(link => {
        link.addEventListener('click', event => {
            if (event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
            const target = root.querySelector(link.getAttribute('href'));
            if (!target) return;
            target.classList.remove('pro-awaiting-reveal');
        });
    });

    window.addEventListener('scroll', queueFrame, { passive: true });
    window.addEventListener('resize', queueFrame, { passive: true });
    window.addEventListener('pageshow', queueFrame);
    reducedMotion.addEventListener('change', configureMotion);
    configureMotion();
})();
