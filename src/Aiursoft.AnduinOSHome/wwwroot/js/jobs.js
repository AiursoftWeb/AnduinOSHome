function calculateProgress(elapsedSeconds) {
    const k = 5;
    const progress = 100 * (elapsedSeconds - k * Math.sqrt(elapsedSeconds)) /
        (elapsedSeconds - k * k);
    return Math.max(0, Math.min(99.9, progress));
}

function updateProgressBars() {
    const now = new Date();
    document.querySelectorAll('.progress-bar[data-started-at]').forEach(progressBar => {
        const elapsedSeconds = (now - new Date(progressBar.dataset.startedAt)) / 1000;
        progressBar.style.width = `${calculateProgress(elapsedSeconds)}%`;
    });
}

updateProgressBars();
setInterval(updateProgressBars, 500);
setTimeout(() => window.location.reload(), 5000);
