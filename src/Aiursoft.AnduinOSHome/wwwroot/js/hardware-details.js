document.addEventListener('DOMContentLoaded', () => {
    const dialog = document.querySelector('[data-hardware-dialog]');
    if (!(dialog instanceof HTMLDialogElement)) return;

    const title = dialog.querySelector('[data-hardware-dialog-title]');
    const status = dialog.querySelector('[data-hardware-dialog-status]');
    const detail = dialog.querySelector('[data-hardware-dialog-detail]');
    let trigger = null;

    document.querySelectorAll('[data-hardware-insight]:not(:disabled)').forEach(button => {
        button.addEventListener('click', () => {
            const explanation = button.querySelector('[data-hardware-detail]')?.textContent?.trim();
            if (!explanation) return;
            trigger = button;
            title.textContent = button.querySelector('small')?.textContent?.trim()
                || button.querySelector('span:not([hidden])')?.textContent?.trim() || '';
            status.textContent = button.querySelector('strong')?.textContent?.trim() || '';
            detail.textContent = explanation;
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
