const loginView = document.getElementById("login-view");
const dashboardView = document.getElementById("dashboard-view");
const content = document.getElementById("content");

function showLogin() {
    dashboardView.hidden = true;
    loginView.hidden = false;
}

function showDashboard() {
    loginView.hidden = true;
    dashboardView.hidden = false;
    navigate("keys");
}

function navigate(page) {
    document.querySelectorAll(".nav-btn").forEach(b =>
        b.classList.toggle("active", b.dataset.page === page));

    if (page === "keys") {
        renderKeysPage(content);
        return;
    }
    if (page === "usage") {
        renderUsagePage(content);
        return;
    }
    if (page === "anomalies") {
        renderAnomaliesPage(content);
        return;
    }
    if (page === "playground") {
        renderPlaygroundPage(content);
        return;
    }

    content.innerHTML = `<div class="placeholder">"${page}" sayfası yakında.</div>`;
}

document.getElementById("login-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const errorEl = document.getElementById("login-error");
    errorEl.hidden = true;

    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;

    try {
        const ok = await login(email, password);
        if (ok) {
            showDashboard();
        } else {
            errorEl.textContent = "Email veya parola hatalı.";
            errorEl.hidden = false;
        }
    } catch {
        errorEl.textContent = "Bağlantı hatası.";
        errorEl.hidden = false;
    }
});

document.getElementById("toggle-password").addEventListener("click", (e) => {
    const input = document.getElementById("password");
    const show = input.type === "password";
    input.type = show ? "text" : "password";
    e.currentTarget.textContent = show ? "🙈" : "👁";
    e.currentTarget.setAttribute("aria-label", show ? "Parolayı gizle" : "Parolayı göster");
});

document.getElementById("logout").addEventListener("click", () => {
    clearToken();
    showLogin();
});

document.querySelectorAll(".nav-btn").forEach(b =>
    b.addEventListener("click", () => navigate(b.dataset.page)));

if (getToken()) {
    showDashboard();
} else {
    showLogin();
}
