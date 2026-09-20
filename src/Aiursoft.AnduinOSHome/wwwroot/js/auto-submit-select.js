document.addEventListener('change', event => {
    if (event.target.matches('.auto-submit-select')) event.target.form?.requestSubmit();
});
