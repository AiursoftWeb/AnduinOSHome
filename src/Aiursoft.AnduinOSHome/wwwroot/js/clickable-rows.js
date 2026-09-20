document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.clickable-row').forEach(row => {
        row.addEventListener('click', event => {
            if (!event.target.closest('a, button') && row.dataset.href) {
                window.location.href = row.dataset.href;
            }
        });
    });
});
