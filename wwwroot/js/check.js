/**
 * ASPNET PLAYGROUND - System Diagnostics Client (/check)
 * Provádí živé testování konektivity k PostgreSQL a IMemoryCache serverům.
 */
document.addEventListener('DOMContentLoaded', () => {
    const retestBtn = document.getElementById('retestBtn');
    const retestIcon = document.getElementById('retestIcon');
    const copyJsonBtn = document.getElementById('copyJsonBtn');
    const rawJsonBox = document.getElementById('rawJsonBox');

    const overallStatusBadge = document.getElementById('overallStatusBadge');
    const overallStatusDot = document.getElementById('overallStatusDot');
    const overallStatusText = document.getElementById('overallStatusText');
    const totalDurationText = document.getElementById('totalDurationText');
    const serverUtcTime = document.getElementById('serverUtcTime');

    // PG Elements
    const pgCard = document.getElementById('pgCard');
    const pgPill = document.getElementById('pgPill');
    const pgStatusText = document.getElementById('pgStatusText');
    const pgLatencyText = document.getElementById('pgLatencyText');
    const pgConnState = document.getElementById('pgConnState');
    const pgDbName = document.getElementById('pgDbName');
    const pgDataSource = document.getElementById('pgDataSource');
    const pgVersion = document.getElementById('pgVersion');
    const pgErrorBox = document.getElementById('pgErrorBox');
    const pgErrorMsg = document.getElementById('pgErrorMsg');

    // Cache Elements
    const cacheCard = document.getElementById('cacheCard');
    const cachePill = document.getElementById('cachePill');
    const cacheStatusText = document.getElementById('cacheStatusText');
    const cacheLatencyText = document.getElementById('cacheLatencyText');
    const cacheConnState = document.getElementById('cacheConnState');
    const cacheStorageType = document.getElementById('cacheStorageType');
    const cacheCachedItems = document.getElementById('cacheCachedItems');
    const cacheErrorBox = document.getElementById('cacheErrorBox');
    const cacheErrorMsg = document.getElementById('cacheErrorMsg');

    const toast = document.getElementById('toastNotification');
    const toastMessage = document.getElementById('toastMessage');
    let toastTimeout = null;

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
        }, 3000);
    }

    async function runDiagnostics() {
        if (retestBtn) retestBtn.disabled = true;
        if (retestIcon) retestIcon.textContent = '⏳ TESTUJI SPOJENÍ...';

        try {
            const res = await fetch('/api/check');
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();

            updateUI(data);
            showToast('✓ Diagnostický test dokončen!');
        } catch (err) {
            console.error('Chyba při spouštění diagnostiky:', err);
            showToast('Nepodařilo se spustit diagnostický test.', true);
        } finally {
            if (retestBtn) retestBtn.disabled = false;
            if (retestIcon) retestIcon.textContent = '🔄 OTESTOVAT SPOJENÍ';
        }
    }

    function updateUI(data) {
        const isHealthy = data.overallStatus === 'Healthy';
        const isPgOk = data.postgres && data.postgres.isConnected;
        const cacheData = data.cache || data.redis || {};
        const isCacheOk = cacheData.isConnected;

        // Overall
        if (overallStatusBadge) {
            overallStatusBadge.className = `stamp-badge ${isHealthy ? 'stamp-badge-black' : 'stamp-badge-yellow'}`;
        }
        if (overallStatusDot) {
            overallStatusDot.style.color = isHealthy ? '#30D158' : (isPgOk || isCacheOk ? '#FF9500' : '#FF3B30');
        }
        if (overallStatusText) {
            overallStatusText.innerHTML = `STAV: <strong>${data.overallStatus.toUpperCase()}</strong>`;
        }
        if (totalDurationText) {
            totalDurationText.textContent = `${data.totalCheckDurationMs} ms`;
        }
        if (serverUtcTime) {
            serverUtcTime.textContent = new Date(data.timestamp).toUTCString();
        }

        const memEl = document.getElementById('memoryUsage');
        const cpuEl = document.getElementById('cpuTime');
        const upEl = document.getElementById('uptime');
        if (memEl && data.environment.memoryUsageMb) memEl.textContent = data.environment.memoryUsageMb;
        if (cpuEl && data.environment.cpuTime) cpuEl.textContent = data.environment.cpuTime;
        if (upEl && data.environment.uptime) upEl.textContent = data.environment.uptime;

        // PostgreSQL
        if (pgCard) {
            pgCard.className = `service-diagnostic-card ${isPgOk ? 'service-online' : 'service-offline'}`;
        }
        if (pgPill) {
            pgPill.className = `service-status-pill ${isPgOk ? 'pill-online' : 'pill-offline'}`;
            const dot = pgPill.querySelector('.status-indicator-dot');
            if (dot) dot.style.background = isPgOk ? '#30D158' : '#FF3B30';
        }
        if (pgStatusText) pgStatusText.textContent = isPgOk ? 'ONLINE' : 'OFFLINE';
        if (pgLatencyText) pgLatencyText.textContent = `${data.postgres ? data.postgres.latencyMs : 0} ms`;
        if (pgConnState) pgConnState.textContent = isPgOk ? 'ÚSPĚŠNĚ PŘIPOJENO' : 'NEPŘIPOJENO';

        if (pgDbName) pgDbName.textContent = (data.postgres && data.postgres.details && data.postgres.details.Database) || '-';
        if (pgDataSource) pgDataSource.textContent = (data.postgres && data.postgres.details && data.postgres.details.DataSource) || '-';
        if (pgVersion) pgVersion.textContent = (data.postgres && data.postgres.details && data.postgres.details.ServerVersion) || '-';

        if (pgErrorBox) {
            if (data.postgres && data.postgres.errorMessage) {
                pgErrorBox.style.display = 'block';
                if (pgErrorMsg) pgErrorMsg.textContent = data.postgres.errorMessage;
            } else {
                pgErrorBox.style.display = 'none';
            }
        }

        // Cache / MemoryCache
        if (cacheCard) {
            cacheCard.className = `service-diagnostic-card ${isCacheOk ? 'service-online' : 'service-offline'}`;
        }
        if (cachePill) {
            cachePill.className = `service-status-pill ${isCacheOk ? 'pill-online' : 'pill-offline'}`;
            const dot = cachePill.querySelector('.status-indicator-dot');
            if (dot) dot.style.background = isCacheOk ? '#30D158' : '#FF3B30';
        }
        if (cacheStatusText) cacheStatusText.textContent = isCacheOk ? 'ONLINE' : 'OFFLINE';
        if (cacheLatencyText) cacheLatencyText.textContent = `${cacheData.latencyMs || 0} ms`;
        if (cacheConnState) cacheConnState.textContent = isCacheOk ? 'AKTIVNÍ' : 'NEAKTIVNÍ';

        if (cacheStorageType) cacheStorageType.textContent = (cacheData.details && cacheData.details.StorageType) || '-';
        if (cacheCachedItems) cacheCachedItems.textContent = (cacheData.details && cacheData.details.CachedItems) || '-';

        if (cacheErrorBox) {
            if (cacheData.errorMessage) {
                cacheErrorBox.style.display = 'block';
                if (cacheErrorMsg) cacheErrorMsg.textContent = cacheData.errorMessage;
            } else {
                cacheErrorBox.style.display = 'none';
            }
        }

        // Raw JSON Box
        if (rawJsonBox) {
            rawJsonBox.textContent = JSON.stringify(data, null, 2);
        }
    }

    if (retestBtn) {
        retestBtn.addEventListener('click', runDiagnostics);
    }

    if (copyJsonBtn && rawJsonBox) {
        copyJsonBtn.addEventListener('click', async () => {
            try {
                await navigator.clipboard.writeText(rawJsonBox.textContent);
                showToast('✓ JSON zkopírován do schránky!');
            } catch {
                showToast('Nepodařilo se zkopírovat JSON.', true);
            }
        });
    }
});
