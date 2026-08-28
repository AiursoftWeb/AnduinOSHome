//=====================================
//    Distribution comparison
//=====================================
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-comparison-root]').forEach(initializeComparison);

    if (window.lucide) {
        window.lucide.createIcons();
    }
});

function initializeComparison(root) {
    const validCompetitors = new Set(['zorin', 'mint', 'ubuntu']);
    const selectorButtons = Array.from(root.querySelectorAll('[data-comparison-select]'));
    const competitorResults = Array.from(root.querySelectorAll('[data-comparison-result]'));
    const selectedLabel = root.querySelector('[data-comparison-selected-label]');
    const mobileSelectedLabel = root.querySelector('[data-comparison-mobile-selected-label]');
    const expandButton = root.querySelector('[data-comparison-expand]');
    const extraRows = Array.from(root.querySelectorAll('.comparison-row--extra'));
    const dialog = root.querySelector('[data-comparison-dialog]');
    const dialogProject = root.querySelector('[data-comparison-dialog-project]');
    const dialogTitle = root.querySelector('[data-comparison-dialog-title]');
    const dialogBody = root.querySelector('[data-comparison-dialog-body]');
    let dialogTrigger = null;

    let parameters;
    try {
        parameters = new URLSearchParams(window.location.search);
    } catch {
        parameters = new URLSearchParams();
    }

    const requestedCompetitor = parameters.get('compare');
    const initialCompetitor = validCompetitors.has(requestedCompetitor)
        ? requestedCompetitor
        : 'zorin';

    function updateUrl(key, value) {
        try {
            const url = new URL(window.location.href);
            if (value === null) {
                url.searchParams.delete(key);
            } else {
                url.searchParams.set(key, value);
            }
            window.history.replaceState({}, '', `${url.pathname}${url.search}${url.hash}`);
        } catch {
            // The comparison remains fully functional when URL state is unavailable.
        }
    }

    function selectCompetitor(competitor, persist = true) {
        if (!validCompetitors.has(competitor)) {
            return;
        }

        const selectedButton = selectorButtons.find(button => button.dataset.comparisonSelect === competitor);
        selectorButtons.forEach(button => {
            const isSelected = button === selectedButton;
            button.classList.toggle('is-active', isSelected);
            button.setAttribute('aria-checked', isSelected ? 'true' : 'false');
            button.tabIndex = isSelected ? 0 : -1;
        });

        competitorResults.forEach(result => {
            result.hidden = result.dataset.comparisonResult !== competitor;
        });

        if (selectedButton && selectedLabel) {
            selectedLabel.replaceChildren(
                document.createTextNode(selectedButton.dataset.projectName || ''),
                Object.assign(document.createElement('span'), {
                    textContent: selectedButton.dataset.projectVersion || ''
                })
            );
        }

        if (selectedButton && mobileSelectedLabel) {
            mobileSelectedLabel.textContent = `${selectedButton.dataset.projectName || ''} ${selectedButton.dataset.projectVersion || ''}`.trim();
        }

        if (persist) {
            updateUrl('compare', competitor === 'zorin' ? null : competitor);
        }
    }

    selectorButtons.forEach((button, index) => {
        button.addEventListener('click', () => {
            selectCompetitor(button.dataset.comparisonSelect);
        });

        button.addEventListener('keydown', event => {
            if (!['ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(event.key)) {
                return;
            }

            event.preventDefault();
            let nextIndex = index;
            if (event.key === 'ArrowLeft') {
                nextIndex = (index - 1 + selectorButtons.length) % selectorButtons.length;
            } else if (event.key === 'ArrowRight') {
                nextIndex = (index + 1) % selectorButtons.length;
            } else if (event.key === 'Home') {
                nextIndex = 0;
            } else if (event.key === 'End') {
                nextIndex = selectorButtons.length - 1;
            }

            const nextButton = selectorButtons[nextIndex];
            selectCompetitor(nextButton.dataset.comparisonSelect);
            nextButton.focus();
        });
    });

    function setExpanded(expanded, persist = true) {
        if (!expandButton) {
            return;
        }

        expandButton.setAttribute('aria-expanded', expanded ? 'true' : 'false');
        const title = expandButton.querySelector('[data-comparison-expand-title]');
        const subtitle = expandButton.querySelector('[data-comparison-expand-subtitle]');

        if (title) {
            title.textContent = expanded
                ? expandButton.dataset.collapseTitle
                : expandButton.dataset.expandTitle;
        }
        if (subtitle) {
            subtitle.textContent = expanded
                ? expandButton.dataset.collapseSubtitle
                : expandButton.dataset.expandSubtitle;
        }

        extraRows.forEach((row, index) => {
            row.style.setProperty('--comparison-row-index', index.toString());
            row.classList.remove('is-revealed');
            row.hidden = !expanded;
            if (expanded) {
                window.requestAnimationFrame(() => row.classList.add('is-revealed'));
            }
        });

        if (persist) {
            updateUrl('expanded', expanded ? 'true' : null);
        }
    }

    if (expandButton) {
        expandButton.addEventListener('click', () => {
            setExpanded(expandButton.getAttribute('aria-expanded') !== 'true');
        });
    }

    function openDialog(templateId, project, capability, trigger) {
        if (!dialog || !dialogProject || !dialogTitle || !dialogBody) {
            return;
        }

        const template = document.getElementById(templateId);
        if (!(template instanceof HTMLTemplateElement)) {
            return;
        }

        dialogTrigger = trigger;
        dialogProject.textContent = project || '';
        dialogTitle.textContent = capability || '';
        dialogBody.replaceChildren(template.content.cloneNode(true));
        dialogBody.scrollTop = 0;

        if (typeof dialog.showModal === 'function') {
            dialog.showModal();
        } else {
            dialog.setAttribute('open', '');
        }

        if (window.lucide) {
            window.lucide.createIcons();
        }
    }

    function closeDialog() {
        if (!dialog || !dialog.hasAttribute('open') || dialog.classList.contains('is-closing')) {
            return;
        }

        const finishClosing = () => {
            dialog.classList.remove('is-closing');
            if (typeof dialog.close === 'function') {
                dialog.close();
            } else {
                dialog.removeAttribute('open');
            }
            if (dialogTrigger instanceof HTMLElement) {
                dialogTrigger.focus();
            }
            dialogTrigger = null;
        };

        if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
            finishClosing();
            return;
        }

        dialog.classList.add('is-closing');
        window.setTimeout(finishClosing, 190);
    }

    root.querySelectorAll('[data-comparison-detail]').forEach(button => {
        button.addEventListener('click', () => {
            openDialog(
                button.dataset.comparisonDetail,
                button.dataset.comparisonProject,
                button.dataset.comparisonCapability,
                button
            );
        });
    });

    root.querySelectorAll('[data-comparison-dialog-close]').forEach(button => {
        button.addEventListener('click', closeDialog);
    });

    if (dialog) {
        dialog.addEventListener('cancel', event => {
            event.preventDefault();
            closeDialog();
        });
        dialog.addEventListener('click', event => {
            if (event.target === dialog) {
                closeDialog();
            }
        });
    }

    selectCompetitor(initialCompetitor, false);
    setExpanded(parameters.get('expanded') === 'true', false);
}
