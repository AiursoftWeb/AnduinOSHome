document.addEventListener('DOMContentLoaded', () => {
    let timeLeft = 10;
    const button = document.getElementById('delete-btn');
    const countdown = document.getElementById('countdown');
    const timer = setInterval(() => {
        timeLeft--;
        if (timeLeft <= 0) {
            clearInterval(timer);
            button.removeAttribute('disabled');
            document.getElementById('countdown-wrapper').remove();
        } else {
            countdown.textContent = String(timeLeft);
        }
    }, 1000);
});
