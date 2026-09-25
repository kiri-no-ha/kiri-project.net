// profile.js — демо-версия с fallback
document.addEventListener('DOMContentLoaded', async () => {
    const email = sessionStorage.getItem('username') || 
                  sessionStorage.getItem('staffName') || 
                  'student@test.ru';

    // Показываем блок профиля
    const content = document.getElementById('profileContent');
    if (content) content.classList.remove('hidden');

    // Заполняем шапку
    const name = email.split('@')[0];
    document.getElementById('profileAvatar').textContent = name.charAt(0).toUpperCase();
    document.getElementById('profileName').textContent = name;
    document.getElementById('profileRank').textContent = 'Студент';

    // Пробуем загрузить с API, если не выйдет — fallback
    let data = { points: 86, attended: 8, total: 8, events: 4, eventsMax: 10, gaps: 0 };

    try {
        const res = await fetch(`${CONFIG.API}/leaderboard`);
        if (res.ok) {
            const list = await res.json();
            const me = list.find(u => u.username === email || u.email === email);
            if (me) {
                data = {
                    points: me.points ?? me.totalPoints ?? 86,
                    attended: me.attended ?? 8,
                    total: me.total ?? 8,
                    events: me.events ?? 4,
                    eventsMax: 10,
                    gaps: me.gaps ?? 0
                };
            }
        }
    } catch (e) {
        console.warn('API недоступен, использую демо-данные', e);
    }

    // Заполняем карточки
    document.getElementById('statWins').textContent = data.attended;
    document.getElementById('statLosses').textContent = data.gaps;
    document.getElementById('statTotal').textContent = data.total;
    document.getElementById('statTime').textContent = data.points;

    // Прогресс-бар сверху
    const winrate = Math.min(100, Math.round((data.attended / Math.max(1, data.total)) * 100));
    document.getElementById('winrateLabel').textContent = winrate + '%';
    document.getElementById('winrateBar').style.width = winrate + '%';

    // Прогресс-бары снизу
    const pct = (a, b) => Math.min(100, Math.round((a / Math.max(1, b)) * 100));

    document.getElementById('pgPointsText').textContent = `${data.points} / 500`;
    document.getElementById('pgPoints').style.width = pct(data.points, 500) + '%';

    document.getElementById('pgAttText').textContent = `${data.attended} / ${data.total}`;
    document.getElementById('pgAtt').style.width = pct(data.attended, data.total) + '%';

    document.getElementById('pgEventsText').textContent = `${data.events} / ${data.eventsMax}`;
    document.getElementById('pgEvents').style.width = pct(data.events, data.eventsMax) + '%';

    document.getElementById('pgGapsText').textContent = data.gaps;
    document.getElementById('pgGaps').style.width = pct(data.gaps, 10) + '%';
});