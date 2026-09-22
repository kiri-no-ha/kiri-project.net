// ========================
// Корзина на localStorage
// ========================
const CART_KEY = 'cart';
const BALANCE_KEY = 'userBalance';
const PURCHASES_KEY = 'purchases';

function getCart() {
    return JSON.parse(localStorage.getItem(CART_KEY) || '[]');
}

function saveCart(cart) {
    localStorage.setItem(CART_KEY, JSON.stringify(cart));
    updateCartUI();
}

function getBalance() {
    const b = localStorage.getItem(BALANCE_KEY);
    return b === null ? 1000 : parseInt(b);  // стартовый баланс 1000 баллов
}

function setBalance(v) {
    localStorage.setItem(BALANCE_KEY, v);
}

function addToCart(item) {
    const cart = getCart();
    const existing = cart.find(i => i.id === item.id);
    if (existing) existing.qty++;
    else cart.push({ ...item, qty: 1 });
    saveCart(cart);

    const badge = document.getElementById('cartBadge');
    if (badge) {
        badge.animate(
            [{ transform: 'scale(1)' }, { transform: 'scale(1.4)' }, { transform: 'scale(1)' }],
            { duration: 300 }
        );
    }
}

function removeFromCart(id) {
    const cart = getCart().filter(i => i.id !== id);
    saveCart(cart);
}

function updateCartUI() {
    const cart = getCart();
    const count = cart.reduce((s, i) => s + i.qty, 0);
    const total = cart.reduce((s, i) => s + i.price * i.qty, 0);

    const badge = document.getElementById('cartBadge');
    if (badge) {
        badge.textContent = count;
        badge.classList.toggle('is-visible', count > 0);
    }

    const totalEl = document.getElementById('cartTotal');
    if (totalEl) totalEl.textContent = total.toLocaleString('ru-RU') + ' ₽';

    const itemsEl = document.getElementById('cartItems');
    const emptyEl = document.getElementById('cartEmpty');
    const footerEl = document.getElementById('cartFooter');
    const promoEl = document.getElementById('cartPromo');

    if (!itemsEl) return;

    if (cart.length === 0) {
        itemsEl.innerHTML = '';
        emptyEl.classList.remove('is-hidden');
        footerEl.classList.add('is-hidden');
        if (promoEl) promoEl.classList.add('is-hidden');
        return;
    }

    emptyEl.classList.add('is-hidden');
    footerEl.classList.remove('is-hidden');
    if (promoEl) promoEl.classList.remove('is-hidden');

    itemsEl.innerHTML = cart.map(item => `
    <div class="cart__item">
      <img class="cart__item-img" src="${item.image}" alt="${escapeHtml(item.name)}">
      <div class="cart__item-info">
        <div class="cart__item-name">${escapeHtml(item.name)}</div>
        <div class="cart__item-qty">${item.qty} × ${item.price.toLocaleString('ru-RU')} ₽</div>
      </div>
      <div style="display:flex;flex-direction:column;align-items:flex-end;gap:6px">
        <div class="cart__item-price">${(item.price * item.qty).toLocaleString('ru-RU')} ₽</div>
        <button class="cart__item-remove" onclick="removeFromCart(${item.id})" aria-label="Удалить">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="3 6 5 6 21 6"></polyline>
            <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"></path>
            <path d="M10 11v6M14 11v6"></path>
          </svg>
        </button>
      </div>
    </div>
  `).join('');
}

function escapeHtml(str) {
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}

// ========================
// Оформление заказа (фейковый бэкенд)
// ========================
function checkout() {
    const cart = getCart();
    if (!cart.length) return;

    const total = cart.reduce((s, i) => s + i.price * i.qty, 0);
    const balance = getBalance();

    if (balance < total) {
        alert(`Недостаточно баллов. У вас ${balance} ₽, нужно ${total} ₽.`);
        return;
    }

    // списываем
    setBalance(balance - total);

    // сохраняем покупки (для страницы «Мои покупки»)
    const purchases = JSON.parse(localStorage.getItem(PURCHASES_KEY) || '[]');
    cart.forEach(item => {
        purchases.push({
            ...item,
            boughtAt: new Date().toISOString(),
            total: item.price * item.qty
        });
    });
    localStorage.setItem(PURCHASES_KEY, JSON.stringify(purchases));

    // очищаем корзину
    localStorage.removeItem(CART_KEY);
    updateCartUI();

    // закрываем попап
    const popup = document.getElementById('cartPopup');
    if (popup) popup.classList.remove('is-open');

    // красивое уведомление
    showToast(`Заказ оформлен! Списано ${total.toLocaleString('ru-RU')} ₽`);
}

function showToast(msg) {
    const toast = document.createElement('div');
    toast.className = 'toast';
    toast.textContent = msg;
    document.body.appendChild(toast);
    requestAnimationFrame(() => toast.classList.add('toast--visible'));
    setTimeout(() => {
        toast.classList.remove('toast--visible');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}
function updateBalanceUI() {
    const el = document.getElementById('balanceValue');
    if (el) el.textContent = getBalance().toLocaleString('ru-RU');
}
// ========================
// Инициализация
// ========================
document.addEventListener('DOMContentLoaded', () => {
    updateCartUI();

    const toggle = document.getElementById('cartToggle');
    const popup = document.getElementById('cartPopup');

    if (toggle && popup) {
        toggle.addEventListener('click', (e) => {
            e.stopPropagation();
            popup.classList.toggle('is-open');
        });

        document.addEventListener('click', (e) => {
            if (!popup.contains(e.target) && !toggle.contains(e.target)) {
                popup.classList.remove('is-open');
            }
        });

        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') popup.classList.remove('is-open');
        });
    }

    const checkoutBtn = document.getElementById('cartCheckout');
    if (checkoutBtn) checkoutBtn.addEventListener('click', checkout);
});