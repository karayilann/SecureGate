const TOKEN_KEY = "securegate_token";

function getToken() {
    try { return localStorage.getItem(TOKEN_KEY); } catch { return null; }
}

function setToken(token) {
    try { localStorage.setItem(TOKEN_KEY, token); } catch { }
}

function clearToken() {
    try { localStorage.removeItem(TOKEN_KEY); } catch { }
}

async function login(email, password) {
    const res = await fetch("/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password })
    });
    if (!res.ok) return false;
    const data = await res.json();
    setToken(data.token);
    return true;
}

async function apiFetch(path, options = {}) {
    const headers = Object.assign({ "Content-Type": "application/json" }, options.headers || {});
    const token = getToken();
    if (token) headers["Authorization"] = "Bearer " + token;

    const res = await fetch(path, Object.assign({}, options, { headers }));
    if (res.status === 401) {
        clearToken();
        showLogin();
        throw new Error("Unauthorized");
    }
    return res;
}
