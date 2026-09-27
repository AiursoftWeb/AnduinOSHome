document.addEventListener('DOMContentLoaded', () => {
    const publish = document.getElementById('hardware-publish');
    const form = document.getElementById('hardware-edit-form');
    const publication = document.getElementById('Device_Publication');
    if (!publish || !form || !publication) return;
    const name = document.getElementById('hardware-preview-name');
    const price = document.getElementById('hardware-preview-price');
    const image = document.getElementById('hardware-preview-image');
    const unsaved = document.getElementById('hardware-preview-unsaved');
    const initial = new FormData(form);
    const initialSnapshot = JSON.stringify(Array.from(initial.entries()).filter(([key]) =>
        key !== '__RequestVerificationToken'));
    const renderPreview = () => {
        const values = new FormData(form);
        name.textContent = `${values.get('Device.Brand') || ''} ${values.get('Device.Model') || ''}`.trim();
        const amount = Number(values.get('Device.PriceUsd'));
        price.textContent = values.get('Device.PriceUsd') && Number.isFinite(amount) ?
            `US$ ${amount.toLocaleString()}` : '—';
        const imagePath = values.get('Device.ProductImagePath');
        const clearImage = values.get('ClearProductImage') === 'true';
        image.classList.toggle('d-none', !imagePath || clearImage);
        if (imagePath && !clearImage && image.dataset.logicalPath !== imagePath) {
            image.src = `/download/${String(imagePath).split('/').map(encodeURIComponent).join('/')}?w=600`;
            image.dataset.logicalPath = imagePath;
        }
        unsaved.classList.toggle('d-none', JSON.stringify(Array.from(values.entries()).filter(([key]) =>
            key !== '__RequestVerificationToken')) === initialSnapshot);
    };
    form.addEventListener('input', renderPreview);
    form.addEventListener('change', renderPreview);
    renderPreview();
    publish.addEventListener('click', () => {
        publication.value = '1';
        form.requestSubmit();
    });
});
