// ========================
// Инициализация
// ========================
const currentUser = sessionStorage.getItem('username') || '';
const navUsername = document.getElementById('navUsername');
const leaderboardBody = document.getElementById('leaderboardBody');
const errorEl = document.getElementById('leaderboardError');

if (currentUser && navUsername) {
    navUsername.textContent = currentUser;
}

// ========================
// Медаль для топ-3
// ========================
function rankLabel(index) {
    if (index === 0) return '<span class="rank-1">🥇 1</span>';
    if (index === 1) return '<span class="rank-2">🥈 2</span>';
    if (index === 2) return '<span class="rank-3">🥉 3</span>';
    return index + 1;
}

// ========================
// Загрузка
// ========================
async function loadLeaderboard() {
    try {
        const res = await fetch(`${CONFIG.API}/leaderboard`);

        if (!res.ok) {
            throw new Error(await res.text());
        }

        const players = await res.json();
        renderTable(players);
    } catch (err) {
        leaderboardBody.innerHTML = '';
        errorEl.classList.remove('hidden');
        errorEl.textContent = 'Не удалось загрузить данные. Попробуй позже.';
        console.error(err);
    }
}

// ========================
// Отрисовка таблицы
// ========================
function renderTable(players) {
    if (!players.length) {
        leaderboardBody.innerHTML = `
            <tr>
                <td colspan="6" style="text-align:center; padding: var(--space-12); color: var(--text-muted);">
                    Пока нет студентов. Будь первым!
                </td>
            </tr>`;
        return;
    }

    leaderboardBody.innerHTML = players.map((p, i) => {
        const isMe = currentUser &&
            (p.username.toLowerCase() === currentUser.toLowerCase());

        return `
            <tr class="${isMe ? 'is-me' : ''}">
                <td>${rankLabel(i)}</td>
                <td>
                    <strong>${escapeHtml(p.username)}</strong>
                    ${isMe ? ' <span class="text-accent" style="font-size:12px">← ты</span>' : ''}
                </td>
                <td>${p.points}</td>
                <td>${p.attendance}</td>
                <td>${p.gaps}</td>
                <td>${p.events}</td>
            </tr>`;
    }).join('');
}

// ========================
// Защита от XSS
// ========================
function escapeHtml(str) {
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}

// Запуск
loadLeaderboard();