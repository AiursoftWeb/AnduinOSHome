document.addEventListener('DOMContentLoaded', () => {
    const dialog = document.querySelector('[data-hardware-dialog]');
    if (!(dialog instanceof HTMLDialogElement)) return;

    const title = dialog.querySelector('[data-hardware-dialog-title]');
    const status = dialog.querySelector('[data-hardware-dialog-status]');
    const detail = dialog.querySelector('[data-hardware-dialog-detail]');
    let trigger = null;

    document.querySelectorAll('[data-hardware-insight]:not(:disabled)').forEach(button => {
        button.addEventListener('click', () => {
            if (!button.dataset.hardwareDetail) return;
            trigger = button;
            title.textContent = button.dataset.hardwareTitle || '';
            status.textContent = button.dataset.hardwareStatus || '';
            detail.textContent = button.dataset.hardwareDetail;
            dialog.showModal();
        });
    });

    dialog.querySelectorAll('[data-hardware-dialog-close]').forEach(button => {
        button.addEventListener('click', () => dialog.close());
    });
    dialog.addEventListener('click', event => {
        if (event.target === dialog) dialog.close();
    });
    dialog.addEventListener('close', () => {
        trigger?.focus();
        trigger = null;
    });
});
