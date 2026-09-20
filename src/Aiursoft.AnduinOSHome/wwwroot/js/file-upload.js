import Uploader from '/scripts/uploader.js';

function initializeUploaders() {
    document.querySelectorAll('[data-file-upload]:not([data-initialized])').forEach(root => {
        root.dataset.initialized = 'true';
        const localized = key => root.querySelector(`.file-upload-loc-data [data-key="${key}"]`)?.textContent || '';
        const addressInput = Array.from(root.querySelectorAll('input'))
            .find(input => input.name === root.dataset.fieldName);
        new Uploader({
            fileInput: $(root.querySelector('input[type="file"]')),
            progress: $(root.querySelector('.progress')),
            progressbar: $(root.querySelector('.progress-bar')),
            addressInput: $(addressInput),
            sizeInMb: Number(root.dataset.sizeInMb),
            validExtensions: (root.dataset.validExtensions || '').split(' ').filter(Boolean),
            uploadUrl: root.dataset.uploadUrl,
            beforeUnloadMessage: localized('beforeUnload')
        }).init({
            messages: {
                default: localized('default'),
                replace: localized('replace'),
                remove: localized('remove'),
                error: localized('error')
            },
            error: {
                fileSize: localized('fileSize'),
                minWidth: localized('minWidth'),
                maxWidth: localized('maxWidth'),
                minHeight: localized('minHeight'),
                maxHeight: localized('maxHeight'),
                imageFormat: localized('imageFormat'),
                fileExtension: localized('fileExtension')
            }
        });
    });
}

if (document.readyState === 'complete') initializeUploaders();
else window.addEventListener('load', initializeUploaders);
