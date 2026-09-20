document.addEventListener('DOMContentLoaded', () => {
    const platformTabs = document.querySelectorAll('#platform-tabs [data-install-platform]');
    platformTabs.forEach(button => button.addEventListener('click', () => {
        const name = button.dataset.installPlatform;
        document.querySelectorAll('.platform-panel').forEach(panel => {
            panel.classList.toggle('show', panel.id === `panel-${name}`);
        });
        platformTabs.forEach(tab => tab.classList.toggle('active', tab === button));
    }));

    const torrentTabs = document.querySelectorAll('#torrent-platform-tabs [data-torrent-platform]');
    function selectTorrentPlatform(button) {
        const name = button.dataset.torrentPlatform;
        document.querySelectorAll('.torrent-platform-panel').forEach(panel => {
            panel.classList.toggle('d-none', panel.id !== `torrent-panel-${name}`);
        });
        torrentTabs.forEach(tab => {
            const active = tab === button;
            tab.classList.toggle('active', active);
            tab.setAttribute('aria-pressed', String(active));
        });
    }
    torrentTabs.forEach(button => button.addEventListener('click', () => selectTorrentPlatform(button)));
    if (torrentTabs.length) {
        const detected = (navigator.userAgentData?.platform || navigator.platform || navigator.userAgent).toLowerCase();
        const platform = detected.includes('mac') ? 'macos' :
            detected.includes('linux') || detected.includes('x11') ? 'linux' : 'windows';
        const button = document.querySelector(`#torrent-platform-tabs [data-torrent-platform="${platform}"]`);
        if (button) selectTorrentPlatform(button);
    }

    const modal = document.getElementById('torrent-screenshot-modal');
    const preview = document.getElementById('torrent-screenshot-preview');
    if (modal && preview) {
        modal.addEventListener('show.bs.modal', event => {
            preview.src = event.relatedTarget?.dataset.torrentImage || '';
            preview.alt = event.relatedTarget?.dataset.torrentAlt || '';
        });
        modal.addEventListener('hidden.bs.modal', () => {
            preview.src = '';
            preview.alt = '';
        });
    }

    const downloadUrl = document.getElementById('thankyou-download-data')?.dataset.url;
    if (downloadUrl) setTimeout(() => { window.location.href = downloadUrl; }, 1000);
});
