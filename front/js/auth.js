// ========================
// Элементы
// ========================
const regUsername  = document.getElementById('regUsername');
const regEmail     = document.getElementById('regEmail');
const registerBtn  = document.getElementById('registerBtn');
const registerMsg  = document.getElementById('registerMsg');

const loginInput   = document.getElementById('loginInput');
const sendCodeBtn  = document.getElementById('sendCodeBtn');
const loginMsg     = document.getElementById('loginMsg');

const codeSection  = document.getElementById('codeSection');
const codeInput    = document.getElementById('codeInput');
const verifyBtn    = document.getElementById('verifyBtn');
const verifyMsg    = document.getElementById('verifyMsg');

// ========================
// Утилиты
// ========================
function showMsg(el, text, type) {
  el.textContent = text;
  el.className = `msg msg--${type}`;
}

function clearMsg(el) {
  el.textContent = '';
  el.className = 'msg';
}

function setLoading(btn, loading, originalText) {
  btn.disabled = loading;
  btn.textContent = loading ? 'Загрузка...' : originalText;
}

// ========================
// Регистрация
// ========================
async function handleRegister() {
  clearMsg(registerMsg);

  const username = regUsername.value.trim();
  const email    = regEmail.value.trim();

  if (!username || !email) {
    showMsg(registerMsg, 'Заполни оба поля', 'error');
    return;
  }

  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    showMsg(registerMsg, 'Неверный формат email', 'error');
    return;
  }

  setLoading(registerBtn, true, 'Создать аккаунт');

  try {
    const res  = await fetch(`${CONFIG.API}/register`, {
      method:  'POST',
      headers: { 'Content-Type': 'application/json' },
      body:    JSON.stringify({ username, email })
    });
    const text = await res.text();

    if (res.ok) {
      showMsg(registerMsg, text, 'success');
      regUsername.value = '';
      regEmail.value    = '';
    } else {
      showMsg(registerMsg, text, 'error');
    }
  } catch {
    showMsg(registerMsg, 'Сервер недоступен', 'error');
  } finally {
    setLoading(registerBtn, false, 'Создать аккаунт');
  }
}

// ========================
// Запрос кода
// ========================
async function handleSendCode() {
  clearMsg(loginMsg);

  const login = loginInput.value.trim();

  if (!login) {
    showMsg(loginMsg, 'Введи логин или email', 'error');
    return;
  }

  setLoading(sendCodeBtn, true, 'Получить код');

  try {
    const res  = await fetch(`${CONFIG.API}/request-code`, {
      method:  'POST',
      headers: { 'Content-Type': 'application/json' },
      body:    JSON.stringify({ login })
    });
    const text = await res.text();

    if (res.ok) {
      showMsg(loginMsg, text, 'success');
      codeSection.classList.remove('hidden');
      codeInput.focus();
    } else {
      showMsg(loginMsg, text, 'error');
    }
  } catch {
    showMsg(loginMsg, 'Сервер недоступен', 'error');
  } finally {
    setLoading(sendCodeBtn, false, 'Получить код');
  }
}

// ========================
// Верификация кода
// ========================
async function handleVerify() {
  clearMsg(verifyMsg);

  const login = loginInput.value.trim();
  const code  = codeInput.value.trim();

  if (!code) {
    showMsg(verifyMsg, 'Введи код из письма', 'error');
    return;
  }

  setLoading(verifyBtn, true, 'Войти');

  try {
    const res  = await fetch(`${CONFIG.API}/verify-code`, {
      method:  'POST',
      headers: { 'Content-Type': 'application/json' },
      body:    JSON.stringify({ login, code })
    });
    const text = await res.text();

    if (res.ok) {
      // Сохраняем username и редиректим на лидерборд
      sessionStorage.setItem('username', login);
      showMsg(verifyMsg, 'Вход выполнен! Перенаправляем...', 'success');
      setTimeout(() => {
        window.location.href = './leaderboard.html';
      }, 800);
    } else {
      showMsg(verifyMsg, text, 'error');
    }
  } catch {
    showMsg(verifyMsg, 'Сервер недоступен', 'error');
  } finally {
    setLoading(verifyBtn, false, 'Войти');
  }
}

// ========================
// Обработчики событий
// ========================
registerBtn.addEventListener('click', handleRegister);
regEmail.addEventListener('keydown', e => e.key === 'Enter' && handleRegister());
regUsername.addEventListener('keydown', e => e.key === 'Enter' && handleRegister());

sendCodeBtn.addEventListener('click', handleSendCode);
loginInput.addEventListener('keydown', e => e.key === 'Enter' && handleSendCode());

verifyBtn.addEventListener('click', handleVerify);
codeInput.addEventListener('keydown', e => e.key === 'Enter' && handleVerify());
