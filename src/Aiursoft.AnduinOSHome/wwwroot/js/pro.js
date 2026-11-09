(() => {
    'use strict';

    const root = document.querySelector('.pro-experience');
    if (!root) return;

    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const sections = root.querySelectorAll('[data-pro-reveal]');
    let observer;

    function configureMotion() {
        observer?.disconnect();
        sections.forEach(section => section.classList.remove('pro-awaiting-reveal'));
        root.classList.remove('pro-motion');
        if (reducedMotion.matches || !('IntersectionObserver' in window)) return;

        observer = new IntersectionObserver(entries => {
            entries.forEach(entry => {
                if (!entry.isIntersecting) return;
                entry.target.classList.remove('pro-awaiting-reveal');
                observer.unobserve(entry.target);
            });
        }, { threshold: 0, rootMargin: '0px 0px 24px 0px' });

        root.classList.add('pro-motion');
        sections.forEach(section => {
            // Content already on screen never flashes or waits for an animation.
            if (section.getBoundingClientRect().top > window.innerHeight) {
                section.classList.add('pro-awaiting-reveal');
                observer.observe(section);
            }
        });
    }

    // Keep native anchors and details usable without JavaScript.
    root.querySelectorAll('a[href^="#pro-"]').forEach(link => {
        link.addEventListener('click', event => {
            if (event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
            root.querySelector(link.getAttribute('href'))?.classList.remove('pro-awaiting-reveal');
        });
    });

    reducedMotion.addEventListener('change', configureMotion);
    configureMotion();
})();
