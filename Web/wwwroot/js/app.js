// ── API Status Check ─────────────────────────────────────────────────────
async function checkApiStatus() {
    const statusEl = document.getElementById('apiStatus');
    if (!statusEl) return;

    try {
        const resp = await fetch('/api/proxy/health', { signal: AbortSignal.timeout(4000) });
        if (resp.ok) {
            statusEl.classList.add('online');
            statusEl.classList.remove('offline');
            statusEl.querySelector('.status-text').textContent = 'online';
        } else {
            throw new Error('not ok');
        }
    } catch {
        statusEl.classList.add('offline');
        statusEl.classList.remove('online');
        statusEl.querySelector('.status-text').textContent = 'offline';
    }
}

checkApiStatus();
setInterval(checkApiStatus, 15000);
