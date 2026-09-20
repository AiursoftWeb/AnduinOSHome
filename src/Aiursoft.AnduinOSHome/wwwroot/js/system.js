document.addEventListener('DOMContentLoaded', () => {
    const restartButton = document.querySelector('#restartConfirmModal .btn-restart');
    if (!restartButton) return;

    const getText = (key, fallback) =>
        document.querySelector(`#system-loc-data span[data-key="${key}"]`)?.textContent?.trim() || fallback;

    function startTimer(duration) {
        let remaining = duration;
        const updateLabel = () => {
            restartButton.textContent = getText('restarting-countdown', 'Restarting... (__SECONDS__ seconds)')
                .replace('__SECONDS__', String(remaining));
        };
        updateLabel();
        const interval = setInterval(() => {
            remaining--;
            if (remaining <= 0) {
                clearInterval(interval);
                restartButton.textContent = getText('reloading', 'Reloading page...');
            } else {
                updateLabel();
            }
        }, 1000);
    }

    restartButton.addEventListener('click', () => {
        fetch('/System/Shutdown', { method: 'POST' })
            .catch(error => console.error('Error sending shutdown request:', error));
        restartButton.disabled = true;
        const cancelButton = document.querySelector('#restartConfirmModal .btn-secondary');
        if (cancelButton) cancelButton.disabled = true;
        startTimer(10);
        setTimeout(() => window.location.reload(), 10000);
    });
});
