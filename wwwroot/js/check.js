/**
 * ASPNET PLAYGROUND - System Diagnostics Client (/check)
 * Provádí živé testování konektivity k PostgreSQL a Redis serverům.
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

    // Redis Elements
    const redisCard = document.getElementById('redisCard');
    const redisPill = document.getElementById('redisPill');
    const redisStatusText = document.getElementById('redisStatusText');
    const redisLatencyText = document.getElementById('redisLatencyText');
    const redisConnState = document.getElementById('redisConnState');
    const redisEndpoint = document.getElementById('redisEndpoint');
    const redisClient = document.getElementById('redisClient');
    const redisErrorBox = document.getElementById('redisErrorBox');
    const redisErrorMsg = document.getElementById('redisErrorMsg');

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
        const isPgOk = data.postgres.isConnected;
        const isRedisOk = data.redis.isConnected;

        // Overall
        if (overallStatusBadge) {
            overallStatusBadge.className = `stamp-badge ${isHealthy ? 'stamp-badge-black' : 'stamp-badge-yellow'}`;
        }
        if (overallStatusDot) {
            overallStatusDot.style.color = isHealthy ? '#30D158' : (isPgOk || isRedisOk ? '#FF9500' : '#FF3B30');
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
        if (pgLatencyText) pgLatencyText.textContent = `${data.postgres.latencyMs} ms`;
        if (pgConnState) pgConnState.textContent = isPgOk ? 'ÚSPĚŠNĚ PŘIPOJENO' : 'NEPŘIPOJENO';

        if (pgDbName) pgDbName.textContent = (data.postgres.details && data.postgres.details.Database) || '-';
        if (pgDataSource) pgDataSource.textContent = (data.postgres.details && data.postgres.details.DataSource) || '-';
        if (pgVersion) pgVersion.textContent = (data.postgres.details && data.postgres.details.ServerVersion) || '-';

        if (pgErrorBox) {
            if (data.postgres.errorMessage) {
                pgErrorBox.style.display = 'block';
                if (pgErrorMsg) pgErrorMsg.textContent = data.postgres.errorMessage;
            } else {
                pgErrorBox.style.display = 'none';
            }
        }

        // Redis
        if (redisCard) {
            redisCard.className = `service-diagnostic-card ${isRedisOk ? 'service-online' : 'service-offline'}`;
        }
        if (redisPill) {
            redisPill.className = `service-status-pill ${isRedisOk ? 'pill-online' : 'pill-offline'}`;
            const dot = redisPill.querySelector('.status-indicator-dot');
            if (dot) dot.style.background = isRedisOk ? '#30D158' : '#FF3B30';
        }
        if (redisStatusText) redisStatusText.textContent = isRedisOk ? 'ONLINE' : 'OFFLINE';
        if (redisLatencyText) redisLatencyText.textContent = `${data.redis.latencyMs} ms`;
        if (redisConnState) redisConnState.textContent = isRedisOk ? 'ÚSPĚŠNĚ PŘIPOJENO' : 'NEPŘIPOJENO';

        if (redisEndpoint) redisEndpoint.textContent = (data.redis.details && data.redis.details.Endpoint) || '-';
        if (redisClient) redisClient.textContent = (data.redis.details && data.redis.details.ClientName) || '-';

        if (redisErrorBox) {
            if (data.redis.errorMessage) {
                redisErrorBox.style.display = 'block';
                if (redisErrorMsg) redisErrorMsg.textContent = data.redis.errorMessage;
            } else {
                redisErrorBox.style.display = 'none';
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
