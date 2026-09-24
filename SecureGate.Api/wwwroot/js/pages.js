const PLAN_OPTIONS = [
    { value: 0, label: "Free" },
    { value: 1, label: "Pro" },
    { value: 2, label: "Enterprise" }
];

// Seeded admin user; a real system would offer a user picker here.
const DEFAULT_USER_ID = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

/* ---------- Keys ---------- */

async function renderKeysPage(container) {
    container.innerHTML = `
        <div class="page-head"><h2>API Keys</h2></div>
        <form id="create-key" class="card-inline">
            <label>User ID <input id="new-user-id" value="${DEFAULT_USER_ID}" /></label>
            <label>Plan
                <select id="new-plan">
                    ${PLAN_OPTIONS.map(p => `<option value="${p.value}">${p.label}</option>`).join("")}
                </select>
            </label>
            <button type="submit">Create</button>
        </form>
        <div id="new-key-banner" class="banner" hidden></div>
        <table>
            <thead><tr><th>Key</th><th>Plan</th><th>Status</th><th>Actions</th></tr></thead>
            <tbody id="keys-body"><tr><td colspan="4" class="muted">Loading…</td></tr></tbody>
        </table>`;

    document.getElementById("create-key").addEventListener("submit", onCreateKey);
    await loadKeys();
}

async function loadKeys() {
    const body = document.getElementById("keys-body");
    try {
        const res = await apiFetch("/api/admin/keys");
        const keys = await res.json();
        if (!keys.length) {
            body.innerHTML = `<tr><td colspan="4" class="muted">No records.</td></tr>`;
            return;
        }
        body.innerHTML = keys.map(renderKeyRow).join("");
        body.querySelectorAll("[data-action]").forEach(el =>
            el.addEventListener(el.tagName === "SELECT" ? "change" : "click", onKeyAction));
    } catch {
        body.innerHTML = `<tr><td colspan="4" class="error">Failed to load list.</td></tr>`;
    }
}

function renderKeyRow(key) {
    const isActive = key.status === "Active";
    const planSelect = `<select data-action="plan" data-id="${key.id}">
        ${PLAN_OPTIONS.map(p => `<option value="${p.value}" ${p.label === key.planName ? "selected" : ""}>${p.label}</option>`).join("")}
    </select>`;
    const toggleBtn = isActive
        ? `<button class="small" data-action="suspend" data-id="${key.id}">Suspend</button>`
        : `<button class="small" data-action="activate" data-id="${key.id}">Activate</button>`;
    return `<tr>
        <td><code>${key.maskedKeyValue}</code></td>
        <td>${key.planName}</td>
        <td><span class="badge ${isActive ? "active" : "suspended"}">${key.status}</span></td>
        <td class="actions">${planSelect} ${toggleBtn}</td>
    </tr>`;
}

async function onCreateKey(e) {
    e.preventDefault();
    const userId = document.getElementById("new-user-id").value.trim();
    const planType = parseInt(document.getElementById("new-plan").value, 10);
    try {
        const res = await apiFetch("/api/keys", { method: "POST", body: JSON.stringify({ userId, planType }) });
        if (!res.ok) throw new Error();
        const created = await res.json();
        const banner = document.getElementById("new-key-banner");
        banner.innerHTML = `
            <span>New key (shown once):</span>
            <code>${created.keyValue}</code>
            <button type="button" class="small" id="copy-key">Copy</button>`;
        banner.hidden = false;
        document.getElementById("copy-key").addEventListener("click", () => copyToClipboard(created.keyValue));
        showToast("Key created.", "success");
        await loadKeys();
    } catch {
        showToast("Key creation failed — is the User ID valid?", "error");
    }
}

async function onKeyAction(e) {
    const el = e.currentTarget;
    const id = el.dataset.id;
    const action = el.dataset.action;
    try {
        if (action === "plan") {
            await apiFetch(`/api/admin/keys/${id}/plan`, {
                method: "PATCH",
                body: JSON.stringify({ planType: parseInt(el.value, 10) })
            });
        } else if (action === "suspend") {
            await apiFetch(`/api/admin/keys/${id}/suspend`, { method: "PATCH" });
        } else if (action === "activate") {
            await apiFetch(`/api/admin/keys/${id}/activate`, { method: "PATCH" });
        }
        showToast("Updated.", "success");
        await loadKeys();
    } catch {
        showToast("Action failed.", "error");
    }
}

/* ---------- Usage ---------- */

let usageChartInstances = [];

function destroyUsageCharts() {
    usageChartInstances.forEach(c => c.destroy());
    usageChartInstances = [];
}

function chartOptions() {
    return {
        responsive: true,
        plugins: { legend: { labels: { color: "#e2e8f0" } } },
        scales: {
            x: { ticks: { color: "#94a3b8" }, grid: { color: "#334155" } },
            y: { ticks: { color: "#94a3b8" }, grid: { color: "#334155" }, beginAtZero: true }
        }
    };
}

async function renderUsagePage(container) {
    destroyUsageCharts();
    container.innerHTML = `
        <div class="page-head"><h2>Usage (last 24 hours)</h2></div>
        <div class="charts">
            <div class="chart-card"><h3>Requests / Hour</h3><canvas id="timeline-chart"></canvas></div>
            <div class="chart-card"><h3>Requests per Key</h3><canvas id="perkey-chart"></canvas></div>
        </div>`;

    try {
        const res = await apiFetch("/api/admin/usage");
        const stats = await res.json();

        usageChartInstances.push(new Chart(document.getElementById("timeline-chart"), {
            type: "line",
            data: {
                labels: stats.timeline.map(p => new Date(p.bucket).toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit" })),
                datasets: [{ label: "Requests", data: stats.timeline.map(p => p.count), borderColor: "#38bdf8", backgroundColor: "rgba(56,189,248,.2)", fill: true, tension: .3 }]
            },
            options: chartOptions()
        }));

        usageChartInstances.push(new Chart(document.getElementById("perkey-chart"), {
            type: "bar",
            data: {
                labels: stats.perKey.map(k => k.maskedKeyValue),
                datasets: [{ label: "Requests", data: stats.perKey.map(k => k.count), backgroundColor: "#818cf8" }]
            },
            options: chartOptions()
        }));
    } catch {
        showToast("Failed to load usage data.", "error");
    }
}

/* ---------- Anomalies ---------- */

async function renderAnomaliesPage(container) {
    container.innerHTML = `
        <div class="page-head"><h2>Anomalies</h2></div>
        <table>
            <thead><tr><th>Key</th><th>Reason</th><th>Distinct IPs</th><th>Detected</th></tr></thead>
            <tbody id="anomalies-body"><tr><td colspan="4" class="muted">Loading…</td></tr></tbody>
        </table>`;

    const body = document.getElementById("anomalies-body");
    try {
        const res = await apiFetch("/api/admin/anomalies");
        const logs = await res.json();
        if (!logs.length) {
            body.innerHTML = `<tr><td colspan="4" class="muted">No anomaly records.</td></tr>`;
            return;
        }
        body.innerHTML = logs.map(l => `<tr>
            <td><code>${l.maskedKeyValue || l.apiKeyId}</code></td>
            <td>${l.reason}</td>
            <td>${l.distinctIpCount}</td>
            <td>${new Date(l.detectedAt).toLocaleString("en-US")}</td>
        </tr>`).join("");
    } catch {
        body.innerHTML = `<tr><td colspan="4" class="error">Failed to load.</td></tr>`;
    }
}

/* ---------- Health ---------- */

async function renderHealthPage(container) {
    container.innerHTML = `
        <div class="page-head"><h2>Health</h2><button id="health-refresh" class="ghost small">Refresh</button></div>
        <div id="health-body"><div class="muted">Loading…</div></div>`;
    document.getElementById("health-refresh").addEventListener("click", loadHealth);
    await loadHealth();
}

async function loadHealth() {
    const body = document.getElementById("health-body");
    try {
        const res = await fetch("/health");
        const data = await res.json();
        const overallOk = data.status === "Healthy";
        const overall = `<div class="health-overall ${overallOk ? "ok" : "bad"}">
            <span class="dot"></span> Overall: <strong>${data.status}</strong>
            <span class="muted">(${Math.round(data.totalDurationMs)} ms)</span>
        </div>`;
        const cards = data.checks.map(c => `
            <div class="health-card">
                <div class="health-name">${c.name.toUpperCase()}</div>
                <span class="badge ${c.status === "Healthy" ? "active" : "suspended"}">${c.status}</span>
                <div class="muted">${c.description || ""} · ${Math.round(c.durationMs)} ms</div>
            </div>`).join("");
        body.innerHTML = overall + `<div class="health-cards">${cards}</div>`;
    } catch {
        body.innerHTML = `<div class="error">Health endpoint unavailable.</div>`;
    }
}

/* ---------- Playground ---------- */

function renderPlaygroundPage(container) {
    container.innerHTML = `
        <div class="page-head"><h2>Proxy Playground</h2></div>
        <form id="pg-form" class="card-inline">
            <label>X-Api-Key <input id="pg-key" placeholder="key value" /></label>
            <label>resource <input id="pg-resource" value="report" /></label>
            <button type="submit">Send</button>
        </form>
        <div id="pg-result"></div>`;

    document.getElementById("pg-form").addEventListener("submit", onProxyPlaygroundSubmit);
}

async function onProxyPlaygroundSubmit(e) {
    e.preventDefault();
    const key = document.getElementById("pg-key").value.trim();
    const resource = document.getElementById("pg-resource").value.trim();
    const resultEl = document.getElementById("pg-result");
    resultEl.innerHTML = `<div class="muted">Sending…</div>`;

    try {
        const res = await fetch(`/proxy?resource=${encodeURIComponent(resource)}`, {
            headers: { "X-Api-Key": key }
        });

        const shown = ["X-Cache", "X-Response-Time-Ms", "X-RateLimit-Limit", "X-RateLimit-Remaining", "Retry-After"];
        const headerRows = shown
            .map(h => [h, res.headers.get(h.toLowerCase())])
            .filter(([, v]) => v !== null)
            .map(([h, v]) => `<tr><th>${h}</th><td><code>${escapeHtml(v)}</code></td></tr>`)
            .join("");

        let bodyText;
        try { bodyText = JSON.stringify(await res.clone().json(), null, 2); }
        catch { bodyText = await res.text(); }

        resultEl.innerHTML = `
            <div class="pg-status ${res.ok ? "ok" : "bad"}">HTTP ${res.status}</div>
            <table class="pg-headers">${headerRows || '<tr><td class="muted">No headers to show</td></tr>'}</table>
            <pre class="pg-body">${escapeHtml(bodyText)}</pre>`;
    } catch {
        resultEl.innerHTML = `<div class="error">Request failed.</div>`;
    }
}
