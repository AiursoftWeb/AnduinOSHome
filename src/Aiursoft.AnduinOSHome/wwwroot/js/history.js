// =====================================
//  HistoryBuilds page — download links
// =====================================

let languageMap = {};

document.addEventListener('DOMContentLoaded', function () {
    // Read the language map from the hidden element (single source of truth)
    var langDataEl = document.getElementById('history-lang-data');
    if (langDataEl) {
        try {
            languageMap = JSON.parse(langDataEl.dataset.allLanguages || '{}');
        } catch (e) {
            console.error('Failed to parse history language data', e);
        }
    }

    // Lazy populate language selects when accordion body opens
    var historyAccordion = document.getElementById('historyAccordion');
    if (historyAccordion) {
        historyAccordion.addEventListener('shown.bs.collapse', function (event) {
            var collapseEl = event.target;
            var container = collapseEl.querySelector('.history-download-container');
            if (!container) return;

            var langSelect = container.querySelector('.history-lang-select');
            // Only populate if not already populated
            if (langSelect && langSelect.options.length === 0) {
                populateLanguageSelect(langSelect, container);
                // Auto-render for the first language
                langSelect.dispatchEvent(new Event('change'));
            }
        });
    }

    // Wire up change events on all language selects (even future ones)
    document.addEventListener('change', function (event) {
        if (event.target.classList.contains('history-lang-select')) {
            var select = event.target;
            var container = select.closest('.history-download-container');
            if (container) {
                renderDownloadLinks(container, select.value);
            }
        }
    });
});

function populateLanguageSelect(selectEl, container) {
    var allowedCodes = (container.dataset.languages || '').split(',').filter(Boolean);

    if (allowedCodes.length === 0) {
        // Fallback: show all languages
        allowedCodes = Object.keys(languageMap);
    }

    selectEl.innerHTML = '';
    allowedCodes.forEach(function (code) {
        var name = languageMap[code] || code;
        var opt = document.createElement('option');
        opt.value = code;
        opt.textContent = name;
        selectEl.appendChild(opt);
    });

    // Select the first language by default
    if (allowedCodes.length > 0) {
        selectEl.value = allowedCodes[0];
    }
}

function renderDownloadLinks(container, langCode) {
    var linksDiv = container.querySelector('.history-download-links');
    if (!linksDiv) return;

    var version = container.dataset.version;
    var latest  = container.dataset.latest;
    var singleIso = container.dataset.singleIso === 'true';

    linksDiv.innerHTML = '';

    var base;
    if (singleIso) {
        base = 'https://download.anduinos.com/' + version + '/' + latest + '/AnduinOS-' + latest;
    } else {
        base = 'https://download.anduinos.com/' + version + '/' + latest + '/AnduinOS-' + latest + '-' + langCode;
    }

    // Torrent button
    appendLink(linksDiv, base + '.torrent', 'Torrent', 'btn-primary');

    // Checksum button
    var checksumLabel = 'Checksum';
    if (!singleIso && languageMap[langCode]) {
        // For known languages we could show a localized label, but since
        // the language map only stores name, we keep a simple "Checksum" label.
        // The torrent + checksum labels match the pattern users expect.
    }
    appendLink(linksDiv, base + '.sha256', checksumLabel, 'btn-outline-primary');
}

function appendLink(container, href, label, btnClass) {
    var a = document.createElement('a');
    a.href      = href;
    a.target    = '_blank';
    a.className = 'btn btn-lg btn-pill ' + btnClass;
    a.textContent = label;
    a.addEventListener('click', function (e) {
        setTimeout(function () {
            top.location.href = '/thankyou.html';
        }, 500);
    });
    container.appendChild(a);
}
