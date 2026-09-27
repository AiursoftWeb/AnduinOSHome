document.addEventListener('DOMContentLoaded', () => {
    const root = document.getElementById('hardware-localize');
    if (!root) return;

    const form = document.getElementById('hardware-translation-form');
    const fields = document.getElementById('hardware-translation-fields');
    const cultureInput = document.getElementById('hardware-translation-culture');
    const languageLabel = document.getElementById('hardware-editor-language');
    const status = document.getElementById('hardware-editor-status');
    const preview = document.getElementById('hardware-preview-link');
    const languages = Array.from(root.querySelectorAll('.hardware-language'));
    const names = [
        'Description', 'InstallationNotes', 'FirmwareNotes', 'KnownIssues',
        'ConfigurationText', 'ImageCreditText', 'InstallationDetail',
        'PerformanceDetail', 'SecureBootDetail', 'WifiDetail',
        'GraphicsDetail', 'VirtualizationDetail', 'DisplayDetail'
    ];
    const text = key => document.querySelector(`#hardware-localize-text [data-key="${key}"]`)?.textContent || key;
    const input = name => form.elements.namedItem(name);
    const sectionFor = name => {
        if (name === 'Description') return 'device-intro';
        if (['InstallationNotes', 'FirmwareNotes', 'KnownIssues'].includes(name)) return 'device-notes';
        if (['ConfigurationText', 'ImageCreditText'].includes(name)) return 'device-record';
        if (['InstallationDetail', 'PerformanceDetail', 'SecureBootDetail'].includes(name)) return 'device-overview';
        return 'device-compatibility';
    };
    names.forEach(name => {
        const field = input(name);
        const container = field.parentElement;
        container.classList.remove('col-md-6');
        container.classList.add('col-12');
        const grid = document.createElement('div');
        grid.className = 'row g-2';
        const reference = document.createElement('div');
        reference.className = 'col-md-6 hardware-source-reference';
        reference.dataset.sourceField = name;
        const original = document.createElement('span');
        original.dataset.sourceCopy = name;
        const editSource = document.createElement('a');
        editSource.href = `${root.dataset.editUrl}#${sectionFor(name)}`;
        editSource.textContent = text('edit-source');
        editSource.className = 'd-block mt-2';
        reference.append(original, editSource);
        const translation = document.createElement('div');
        translation.className = 'col-md-6';
        field.before(grid);
        grid.append(reference, translation);
        translation.append(field);
    });
    const snapshot = () => JSON.stringify(names.map(name => input(name).value));
    let currentCulture = null;
    let savedSnapshot = null;
    let loadingVersion = 0;
    let saving = false;

    const isDirty = () => currentCulture !== null && savedSnapshot !== null && snapshot() !== savedSnapshot;
    const showStatus = (message, kind = '') => {
        status.textContent = message;
        status.className = `small ${kind === 'error' ? 'text-danger' : kind === 'success' ? 'text-success' : 'text-muted'}`;
    };
    const select = button => {
        languages.forEach(item => {
            const active = item === button;
            item.classList.toggle('active', active);
            item.setAttribute('aria-pressed', String(active));
        });
    };

    async function load(button) {
        const culture = button.dataset.culture;
        if (saving || currentCulture === culture) return;
        if (isDirty() && !window.confirm(text('unsaved'))) return;

        const version = ++loadingVersion;
        currentCulture = null;
        savedSnapshot = null;
        fields.disabled = true;
        select(button);
        languageLabel.textContent = button.querySelector('strong').textContent + ` (${culture})`;
        if (preview) {
            const url = new URL(preview.href);
            url.searchParams.set('culture', culture);
            preview.href = url.toString();
        }
        showStatus(text('loading'));
        try {
            const url = new URL(root.dataset.loadUrl, window.location.origin);
            url.searchParams.set('id', root.dataset.deviceId);
            url.searchParams.set('culture', culture);
            const response = await fetch(url);
            if (!response.ok) throw new Error('load failed');
            const data = await response.json();
            if (version !== loadingVersion) return;
            names.forEach(name => {
                const key = name.charAt(0).toLowerCase() + name.slice(1);
                input(name).value = data[key] || '';
                const source = data.source?.[key];
                root.querySelector(`[data-source-copy="${name}"]`).textContent =
                    `${text('source-reference')}: ${source || text('no-source-text')}`;
            });
            cultureInput.value = culture;
            currentCulture = culture;
            savedSnapshot = snapshot();
            fields.disabled = false;
            showStatus('');
        } catch (error) {
            if (version === loadingVersion) showStatus(text('load-error'), 'error');
        }
    }

    form.addEventListener('submit', async event => {
        event.preventDefault();
        if (saving || currentCulture === null || !form.reportValidity()) return;
        const payload = new FormData(form);
        saving = true;
        fields.disabled = true;
        showStatus(text('saving'));
        try {
            const url = new URL(root.dataset.saveUrl, window.location.origin);
            url.searchParams.set('id', root.dataset.deviceId);
            const response = await fetch(url, { method: 'POST', body: payload });
            const data = await response.json();
            if (!response.ok || !data.success) throw new Error(data.error || text('save-error'));
            savedSnapshot = snapshot();
            const button = languages.find(item => item.dataset.culture === currentCulture);
            const check = button?.querySelector('.hardware-language-check');
            if (check) {
                check.textContent = text('saved');
                check.classList.remove('text-muted', 'text-warning');
                check.classList.add('text-success');
                check.setAttribute('aria-label', text('saved'));
                check.title = text('saved');
            }
            showStatus(text('translation-updated'), 'success');
        } catch (error) {
            showStatus(error.message || text('save-error'), 'error');
        } finally {
            saving = false;
            fields.disabled = false;
        }
    });

    languages.forEach(button => button.addEventListener('click', () => load(button)));
    window.addEventListener('beforeunload', event => {
        if (!isDirty()) return;
        event.preventDefault();
        event.returnValue = '';
    });
    if (languages.length) load(languages[0]);
});
