(function () {
    const loginBtn = document.getElementById('loginBtn');
    const navUsername = document.getElementById('navUsername');
    if (!loginBtn || !navUsername) return;

    const username =
        sessionStorage.getItem('username') ||
        sessionStorage.getItem('staffName');

    if (username) {
        navUsername.textContent = username;
        loginBtn.textContent = 'Выйти';
        loginBtn.href = '#';
        loginBtn.addEventListener('click', (e) => {
            e.preventDefault();
            sessionStorage.clear();
            localStorage.removeItem('cart');
            window.location.href = './auth.html';
        });
    } else {
        navUsername.textContent = '—';
        loginBtn.textContent = 'Войти';
        loginBtn.href = './auth.html';
    }
})();