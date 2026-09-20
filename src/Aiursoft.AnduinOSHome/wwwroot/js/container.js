document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.container-copy-btn').forEach(button => {
        button.addEventListener('click', () => {
            navigator.clipboard.writeText('sudo docker run -it aiursoft/anduinos bash');
        });
    });
});
