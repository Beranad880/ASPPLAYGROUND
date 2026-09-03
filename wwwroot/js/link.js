/**
 * ASPNET PLAYGROUND - Redis Link Sharing Client
 * Spravuje okamžité sdílení textů a URL odkazů napříč zařízeními přes Redis REST API.
 */
document.addEventListener('DOMContentLoaded', () => {
    // DOM Elements
    const linkForm = document.getElementById('linkForm');
    const linkInput = document.getElementById('linkInput');
    const saveLinkBtn = document.getElementById('saveLinkBtn');
    const saveBtnText = document.getElementById('saveBtnText');
    const pasteFromClipboardBtn = document.getElementById('pasteFromClipboardBtn');
    const refreshLinksBtn = document.getElementById('refreshLinksBtn');
    const refreshIcon = document.getElementById('refreshIcon');
    const clearAllLinksBtn = document.getElementById('clearAllLinksBtn');

    const totalLinksCount = document.getElementById('totalLinksCount');
    const redisStatusBadge = document.getElementById('redisStatusBadge');
    const redisStatusDot = document.getElementById('redisStatusDot');
    const redisStatusText = document.getElementById('redisStatusText');

    const linksLoadingState = document.getElementById('linksLoadingState');
    const emptyLinksState = document.getElementById('emptyLinksState');
    const linksContainer = document.getElementById('linksContainer');

    const deleteLinkModal = document.getElementById('deleteLinkModal');
    const deleteItemPreview = document.getElementById('deleteItemPreview');
    const cancelDeleteLinkBtn = document.getElementById('cancelDeleteLinkBtn');
    const confirmDeleteLinkBtn = document.getElementById('confirmDeleteLinkBtn');

    const clearAllModal = document.getElementById('clearAllModal');
    const cancelClearAllBtn = document.getElementById('cancelClearAllBtn');
    const confirmClearAllBtn = document.getElementById('confirmClearAllBtn');

    const toast = document.getElementById('toastNotification');
    const toastMessage = document.getElementById('toastMessage');

    let cachedLinks = [];
    let linkIdToDelete = null;
    let pollTimer = null;
    let toastTimeout = null;

    // --- Toast Notification ---
    function showToast(message, isError = false) {
        if (!toast || !toastMessage) return;
        toastMessage.textContent = message;
        if (isError) {
            toast.style.background = '#FF3B30';
            toast.style.color = '#FFFFFF';
        } else {
            toast.style.background = '#FFE500';
            toast.style.color = '#000000';
        }
        toast.classList.add('show');
        clearTimeout(toastTimeout);
        toastTimeout = setTimeout(() => {
            toast.classList.remove('show');
        }, 3200);
    }

    // --- Format Date Helpers ---
    function formatTime(isoString) {
        if (!isoString) return '';
        try {
            const d = new Date(isoString);
            const now = new Date();
            const isToday = d.toDateString() === now.toDateString();

            const timeStr = d.toLocaleTimeString('cs-CZ', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
            if (isToday) {
                return `Dnes v ${timeStr}`;
            }
            return `${d.toLocaleDateString('cs-CZ', { day: '2-digit', month: '2-digit' })} ${timeStr}`;
        } catch {
            return isoString;
        }
    }

    function escapeHtml(text) {
        if (!text) return '';
        return text
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    function extractDomain(urlStr) {
        try {
            const u = new URL(urlStr);
            return u.hostname;
        } catch {
            return 'odkaz';
        }
    }

    // --- API Calls ---

    // 1. Fetch Status
    async function checkStatus() {
        try {
            const res = await fetch('/api/links/status');
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();

            if (data.isRedisConnected) {
                if (redisStatusBadge) {
                    redisStatusBadge.className = 'stamp-badge stamp-badge-black';
                }
                if (redisStatusDot) {
                    redisStatusDot.style.background = '#30D158';
                }
                if (redisStatusText) {
                    redisStatusText.textContent = '● REDIS ACTIVE';
                }
            } else {
                if (redisStatusBadge) {
                    redisStatusBadge.className = 'stamp-badge stamp-badge-yellow';
                }
                if (redisStatusDot) {
                    redisStatusDot.style.background = '#FF9500';
                }
                if (redisStatusText) {
                    redisStatusText.textContent = '● IN-MEMORY FALLBACK';
                }
            }
        } catch (err) {
            if (redisStatusBadge) {
                redisStatusBadge.className = 'stamp-badge';
            }
            if (redisStatusDot) {
                redisStatusDot.style.background = '#FF3B30';
            }
            if (redisStatusText) {
                redisStatusText.textContent = '● STATUS N/A';
            }
        }
    }

    // 2. Fetch All Links
    async function loadLinks(isBackground = false) {
        if (!isBackground && linksLoadingState && cachedLinks.length === 0) {
            linksLoadingState.style.display = 'flex';
            emptyLinksState.style.display = 'none';
            linksContainer.style.display = 'none';
        }

        try {
            const res = await fetch('/api/links');
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const links = await res.json();

            // Intelligent diff: render only if count or latest ID changed
            const hasChanged = JSON.stringify(links) !== JSON.stringify(cachedLinks);
            if (hasChanged || !isBackground) {
                cachedLinks = links;
                renderLinks(links);
            }
        } catch (err) {
            console.error('Chyba při načítání odkazů z API:', err);
            if (!isBackground) {
                showToast('Chyba při načítání dat z Redis serveru.', true);
            }
        } finally {
            if (linksLoadingState) linksLoadingState.style.display = 'none';
        }
    }

    // 3. Render Links List
    function renderLinks(links) {
        if (totalLinksCount) {
            totalLinksCount.textContent = links.length;
        }

        if (!links || links.length === 0) {
            if (emptyLinksState) emptyLinksState.style.display = 'block';
            if (linksContainer) linksContainer.style.display = 'none';
            return;
        }

        if (emptyLinksState) emptyLinksState.style.display = 'none';
        if (linksContainer) linksContainer.style.display = 'grid';

        linksContainer.innerHTML = '';

        links.forEach((item, index) => {
            const isUrl = item.isUrl || item.IsUrl;
            const content = item.content || item.Content || '';
            const createdAt = item.createdAt || item.CreatedAt;
            const id = item.id || item.Id || String(index);

            const card = document.createElement('article');
            card.className = `link-item-card ${isUrl ? 'is-url-item' : 'is-text-item'}`;
            card.setAttribute('data-id', id);

            const domain = isUrl ? extractDomain(content) : null;
            const formattedTime = formatTime(createdAt);

            card.innerHTML = `
                <div class="link-item-header">
                    <div class="link-meta-badges">
                        <span class="link-index-stamp">#${index + 1}</span>
                        ${isUrl ? `<span class="link-type-pill pill-url">🔗 URL [${escapeHtml(domain)}]</span>` : `<span class="link-type-pill pill-text">📝 TEXT</span>`}
                        <span class="link-time-stamp">${formattedTime}</span>
                    </div>
                    <div class="link-item-actions">
                        <button type="button" class="btn-item-action btn-copy-link" data-content="${escapeHtml(content)}" title="Zkopírovat obsah do schránky">
                            <span>📋 COPY</span>
                        </button>
                        ${isUrl ? `
                        <a href="${escapeHtml(content)}" target="_blank" rel="noopener noreferrer" class="btn-item-action btn-open-link" title="Otevřít odkaz v novém okně">
                            <span>↗ OTEVŘÍT</span>
                        </a>` : ''}
                        <button type="button" class="btn-item-action btn-delete-link" data-id="${escapeHtml(id)}" data-preview="${escapeHtml(content.substring(0, 80))}" title="Smazat tento záznam">
                            <span>🗑️ SMAZAT</span>
                        </button>
                    </div>
                </div>

                <div class="link-item-body">
                    ${isUrl ? `
                        <a href="${escapeHtml(content)}" target="_blank" rel="noopener noreferrer" class="link-content-anchor">
                            <span class="link-anchor-icon">🔗</span>
                            <span class="link-anchor-text">${escapeHtml(content)}</span>
                        </a>
                    ` : `
                        <pre class="link-content-text">${escapeHtml(content)}</pre>
                    `}
                </div>
            `;

            linksContainer.appendChild(card);
        });

        // Attach action listeners
        attachItemEventListeners();
    }

    function attachItemEventListeners() {
        // Copy buttons
        document.querySelectorAll('.btn-copy-link').forEach(btn => {
            btn.addEventListener('click', async (e) => {
                e.preventDefault();
                const content = btn.getAttribute('data-content');
                if (!content) return;

                try {
                    await navigator.clipboard.writeText(content);
                    showToast('✓ Zkopírováno do schránky!');
                    
                    const span = btn.querySelector('span');
                    if (span) {
                        const original = span.textContent;
                        span.textContent = '✓ COPIED';
                        setTimeout(() => { span.textContent = original; }, 1500);
                    }
                } catch (err) {
                    console.error('Chyba při kopírování:', err);
                    showToast('Nepodařilo se zkopírovat text.', true);
                }
            });
        });

        // Delete buttons
        document.querySelectorAll('.btn-delete-link').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                linkIdToDelete = btn.getAttribute('data-id');
                const preview = btn.getAttribute('data-preview') || '';
                if (deleteItemPreview) {
                    deleteItemPreview.textContent = `„${preview}${preview.length >= 80 ? '...' : ''}”`;
                }
                if (deleteLinkModal) {
                    deleteLinkModal.style.display = 'flex';
                }
            });
        });
    }

    // --- Form Submission (Create Link) ---
    async function submitLink(e) {
        if (e) e.preventDefault();
        const content = linkInput.value.trim();

        if (!content) {
            linkInput.focus();
            showToast('Zadejte text nebo URL odkaz!', true);
            return;
        }

        saveLinkBtn.disabled = true;
        if (saveBtnText) saveBtnText.textContent = 'UKLÁDÁM...';

        try {
            const res = await fetch('/api/links', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ content: content })
            });

            if (!res.ok) {
                const errData = await res.json().catch(() => ({}));
                throw new Error(errData.message || `HTTP ${res.status}`);
            }

            const created = await res.json();
            linkInput.value = '';
            showToast(created.isUrl ? '✓ URL odkaz byl uložen do Redis!' : '✓ Text byl uložen do Redis!');

            // Immediate re-fetch and render
            await loadLinks(false);
            await checkStatus();
        } catch (err) {
            console.error('Chyba při ukládání do Redis:', err);
            showToast(`Chyba: ${err.message}`, true);
        } finally {
            saveLinkBtn.disabled = false;
            if (saveBtnText) saveBtnText.textContent = '＋ ULOŽIT DO REDIS';
            linkInput.focus();
        }
    }

    if (linkForm) {
        linkForm.addEventListener('submit', submitLink);
    }

    // Keyboard shortcut: Ctrl + Enter / Cmd + Enter inside textarea
    if (linkInput) {
        linkInput.addEventListener('keydown', (e) => {
            if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
                e.preventDefault();
                submitLink();
            }
        });
    }

    // --- Paste from Clipboard ---
    if (pasteFromClipboardBtn) {
        pasteFromClipboardBtn.addEventListener('click', async () => {
            try {
                const text = await navigator.clipboard.readText();
                if (text) {
                    linkInput.value = text;
                    linkInput.focus();
                    showToast('Vloženo ze schránky!');
                } else {
                    showToast('Schránka je prázdná.', true);
                }
            } catch (err) {
                console.warn('Clipboard read error:', err);
                showToast('Povolte přístup ke schránce v prohlížeči.', true);
            }
        });
    }

    // --- Preset Chips ---
    document.querySelectorAll('.preset-chip').forEach(chip => {
        chip.addEventListener('click', () => {
            const text = chip.getAttribute('data-text');
            if (text && linkInput) {
                linkInput.value = text;
                linkInput.focus();
            }
        });
    });

    // --- Refresh Button ---
    if (refreshLinksBtn) {
        refreshLinksBtn.addEventListener('click', async () => {
            if (refreshIcon) refreshIcon.textContent = '⏳ ČEKÁM...';
            refreshLinksBtn.disabled = true;

            await loadLinks(false);
            await checkStatus();
            showToast('Data byla aktualizována!');

            setTimeout(() => {
                if (refreshIcon) refreshIcon.textContent = '🔄 OBNOVIT';
                refreshLinksBtn.disabled = false;
            }, 500);
        });
    }

    // --- Delete Single Link ---
    if (cancelDeleteLinkBtn) {
        cancelDeleteLinkBtn.addEventListener('click', () => {
            if (deleteLinkModal) deleteLinkModal.style.display = 'none';
            linkIdToDelete = null;
        });
    }

    if (confirmDeleteLinkBtn) {
        confirmDeleteLinkBtn.addEventListener('click', async () => {
            if (!linkIdToDelete) return;
            confirmDeleteLinkBtn.disabled = true;

            try {
                const res = await fetch(`/api/links/${encodeURIComponent(linkIdToDelete)}`, {
                    method: 'DELETE'
                });

                if (res.ok || res.status === 204 || res.status === 200) {
                    showToast('Záznam byl smazán z Redis.');
                    if (deleteLinkModal) deleteLinkModal.style.display = 'none';
                    linkIdToDelete = null;
                    await loadLinks(false);
                    await checkStatus();
                } else {
                    throw new Error(`HTTP ${res.status}`);
                }
            } catch (err) {
                console.error('Chyba při mazání záznamu:', err);
                showToast('Nepodařilo se smazat záznam.', true);
            } finally {
                confirmDeleteLinkBtn.disabled = false;
            }
        });
    }

    // --- Clear All Links ---
    if (clearAllLinksBtn) {
        clearAllLinksBtn.addEventListener('click', () => {
            if (clearAllModal) clearAllModal.style.display = 'flex';
        });
    }

    if (cancelClearAllBtn) {
        cancelClearAllBtn.addEventListener('click', () => {
            if (clearAllModal) clearAllModal.style.display = 'none';
        });
    }

    if (confirmClearAllBtn) {
        confirmClearAllBtn.addEventListener('click', async () => {
            confirmClearAllBtn.disabled = true;

            try {
                const res = await fetch('/api/links/clear', {
                    method: 'DELETE'
                });

                if (res.ok) {
                    showToast('Všechny odkazy byly úspěšně smazány z Redis.');
                    if (clearAllModal) clearAllModal.style.display = 'none';
                    await loadLinks(false);
                    await checkStatus();
                } else {
                    throw new Error(`HTTP ${res.status}`);
                }
            } catch (err) {
                console.error('Chyba při mazání všech odkazů:', err);
                showToast('Nepodařilo se vymazat historii odkazů.', true);
            } finally {
                confirmClearAllBtn.disabled = false;
            }
        });
    }

    // Close modals on overlay background click
    window.addEventListener('click', (e) => {
        if (e.target === deleteLinkModal) {
            deleteLinkModal.style.display = 'none';
            linkIdToDelete = null;
        }
        if (e.target === clearAllModal) {
            clearAllModal.style.display = 'none';
        }
    });

    // Close on Escape key
    window.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            if (deleteLinkModal) deleteLinkModal.style.display = 'none';
            if (clearAllModal) clearAllModal.style.display = 'none';
        }
    });

    // --- Initial Load ---
    checkStatus();
    loadLinks(false);

    // --- SignalR Real-Time Connection ---
    if (typeof signalR !== 'undefined') {
        const linkConnection = new signalR.HubConnectionBuilder()
            .withUrl("/linkHub")
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        linkConnection.on("LinksUpdated", () => {
            loadLinks(true);
            checkStatus();
        });

        linkConnection.start()
            .then(() => console.log("SignalR: Připojeno k LinkHub pro real-time aktualizace odkazů."))
            .catch(err => console.error("SignalR: Chyba připojení k LinkHub:", err));
    } else {
        console.warn("SignalR není dostupný. Fallback na polling.");
        pollTimer = setInterval(() => {
            if (document.visibilityState === 'visible') {
                loadLinks(true);
            }
        }, 5000);
    }
});
