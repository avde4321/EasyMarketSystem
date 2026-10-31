(function () {
    const selectors = [
        '.alert-message',
        '.success-message',
        '.feedback-banner',
        '.dynamic-status-banner',
        '.login-error'
    ];

    const seenMessages = new WeakMap();
    const recentMessages = new Map();
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
        if (element.classList.contains('alert-message') ||
            element.classList.contains('login-error') ||
            element.classList.contains('error')) {
            return 'error';
        }

        if (element.classList.contains('success') ||
            element.classList.contains('success-message')) {
            return 'success';
        }

        return 'info';
    }

    function normalizeMessage(text) {
        return (text || '').replace(/\s+/g, ' ').trim();
    }

    function createPopup(message, variant) {
        const safeVariant = ['error', 'success', 'info'].includes(variant) ? variant : 'info';
        const dedupeKey = `${safeVariant}:${normalizeMessage(message).toLowerCase()}`;
        const now = Date.now();
        const previousAt = recentMessages.get(dedupeKey);
        if (previousAt && now - previousAt < 1200) {
            return;
        }

        recentMessages.set(dedupeKey, now);
        window.setTimeout(() => recentMessages.delete(dedupeKey), 1500);

        const host = ensureContainer();
        const popup = document.createElement('div');
        popup.className = `global-popup global-popup-${safeVariant}`;
        popup.setAttribute('role', safeVariant === 'error' ? 'alert' : 'status');

        const content = document.createElement('div');
        content.className = 'global-popup-content';

        const title = document.createElement('strong');
        title.textContent = safeVariant === 'error'
            ? 'Atencion'
            : safeVariant === 'success'
                ? 'Listo'
                : 'Informacion';

        const text = document.createElement('span');
        text.textContent = message;

        const close = document.createElement('button');
        close.type = 'button';
        close.className = 'global-popup-close';
        close.setAttribute('aria-label', 'Cerrar mensaje');
        close.textContent = 'x';

        content.appendChild(title);
        content.appendChild(text);
        popup.appendChild(content);
        popup.appendChild(close);

        const removePopup = () => {
            popup.classList.add('is-leaving');
            window.setTimeout(() => popup.remove(), 220);
        };

        close.addEventListener('click', removePopup);
        host.appendChild(popup);

        window.setTimeout(() => popup.classList.add('is-visible'), 10);
        window.setTimeout(removePopup, safeVariant === 'error' ? 6500 : 4200);
    }

    function processElement(element) {
        if (!selectors.some(selector => element.matches(selector)) || !element.isConnected) {
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

    window.easyMarketPopups = window.easyMarketPopups || {};
    window.easyMarketPopups.show = function (message, variant) {
        const normalizedMessage = normalizeMessage(message);
        if (!normalizedMessage) {
            return;
        }

        createPopup(normalizedMessage, variant || 'info');
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', start, { once: true });
    } else {
        start();
    }
})();
