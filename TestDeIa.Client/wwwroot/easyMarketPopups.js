(function () {
    const selectors = [
        '.alert-message',
        '.feedback-banner',
        '.dynamic-status-banner',
        '.login-error'
    ];

    const seenMessages = new WeakMap();
    let container = null;

    function ensureContainer() {
        if (container && document.body.contains(container)) {
            return container;
        }

        container = document.createElement('div');
        container.className = 'global-popup-stack';
        document.body.appendChild(container);
        document.body.classList.add('popup-messages-enabled');
        return container;
    }

    function getVariant(element) {
        if (element.classList.contains('alert-message') || element.classList.contains('login-error') || element.classList.contains('error')) {
            return 'error';
        }

        if (element.classList.contains('success')) {
            return 'success';
        }

        return 'info';
    }

    function normalizeMessage(text) {
        return (text || '').replace(/\s+/g, ' ').trim();
    }

    function isVisible(element) {
        if (!element || !element.isConnected) {
            return false;
        }

        const styles = window.getComputedStyle(element);
        return styles.display !== 'none' && styles.visibility !== 'hidden' && styles.opacity !== '0';
    }

    function createPopup(message, variant) {
        const host = ensureContainer();
        const popup = document.createElement('div');
        popup.className = `global-popup global-popup-${variant}`;
        popup.setAttribute('role', variant === 'error' ? 'alert' : 'status');
        popup.innerHTML = `
            <div class="global-popup-content">
                <strong>${variant === 'error' ? 'Atención' : variant === 'success' ? 'Listo' : 'Información'}</strong>
                <span>${message}</span>
            </div>
            <button type="button" class="global-popup-close" aria-label="Cerrar mensaje">×</button>
        `;

        const removePopup = () => {
            popup.classList.add('is-leaving');
            window.setTimeout(() => popup.remove(), 220);
        };

        popup.querySelector('.global-popup-close')?.addEventListener('click', removePopup);
        host.appendChild(popup);

        window.setTimeout(() => popup.classList.add('is-visible'), 10);
        window.setTimeout(removePopup, variant === 'error' ? 6500 : 4200);
    }

    function processElement(element) {
        if (!selectors.some(selector => element.matches(selector)) || !isVisible(element)) {
            return;
        }

        const message = normalizeMessage(element.textContent);
        if (!message) {
            return;
        }

        const previous = seenMessages.get(element);
        if (previous === message) {
            return;
        }

        seenMessages.set(element, message);
        createPopup(message, getVariant(element));
    }

    function processTree(root) {
        if (!(root instanceof Element)) {
            return;
        }

        if (selectors.some(selector => root.matches(selector))) {
            processElement(root);
        }

        root.querySelectorAll(selectors.join(',')).forEach(processElement);
    }

    const observer = new MutationObserver(mutations => {
        for (const mutation of mutations) {
            if (mutation.type === 'childList') {
                mutation.addedNodes.forEach(node => processTree(node));
                continue;
            }

            if (mutation.type === 'characterData') {
                const parent = mutation.target.parentElement;
                if (parent) {
                    processTree(parent);
                }
                continue;
            }

            if (mutation.type === 'attributes' && mutation.target instanceof Element) {
                processTree(mutation.target);
            }
        }
    });

    function start() {
        ensureContainer();
        processTree(document.body);
        observer.observe(document.body, {
            childList: true,
            subtree: true,
            characterData: true,
            attributes: true,
            attributeFilter: ['class', 'style']
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', start, { once: true });
    } else {
        start();
    }
})();
