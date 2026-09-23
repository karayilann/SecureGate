const PLAN_OPTIONS = [
    { value: 0, label: "Free" },
    { value: 1, label: "Pro" },
    { value: 2, label: "Enterprise" }
];

// Seeded admin user; a real system would offer a user picker here.
const DEFAULT_USER_ID = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

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
            <button type="submit">Oluştur</button>
        </form>
        <div id="new-key-banner" class="banner" hidden></div>
        <table>
            <thead><tr><th>Key</th><th>Plan</th><th>Durum</th><th>Aksiyonlar</th></tr></thead>
            <tbody id="keys-body"><tr><td colspan="4" class="muted">Yükleniyor…</td></tr></tbody>
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
            body.innerHTML = `<tr><td colspan="4" class="muted">Kayıt yok.</td></tr>`;
            return;
        }
        body.innerHTML = keys.map(renderKeyRow).join("");
        body.querySelectorAll("[data-action]").forEach(el =>
            el.addEventListener(el.tagName === "SELECT" ? "change" : "click", onKeyAction));
    } catch {
        body.innerHTML = `<tr><td colspan="4" class="error">Liste yüklenemedi.</td></tr>`;
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
            <span>Yeni key (bir daha gösterilmeyecek):</span>
            <code>${created.keyValue}</code>
            <button type="button" class="small" id="copy-key">Kopyala</button>`;
        banner.hidden = false;
        document.getElementById("copy-key").addEventListener("click", () => copyToClipboard(created.keyValue));
        showToast("Key oluşturuldu.", "success");
        await loadKeys();
    } catch {
        showToast("Key oluşturulamadı — User ID geçerli mi?", "error");
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
        showToast("Güncellendi.", "success");
        await loadKeys();
    } catch {
        showToast("İşlem başarısız.", "error");
    }
}
