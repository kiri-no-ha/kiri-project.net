// ========================
// Отрисовка мероприятий в стиле info-with-media
// ========================
async function loadEventsGrid() {
    const grid = document.getElementById('eventsGrid');
    if (!grid) return;

    try {
        const res = await fetch(`${CONFIG.API}/events`);
        const items = await res.json();

        if (!items.length) {
            grid.innerHTML = '<p class="text-muted">Мероприятий пока нет</p>';
            return;
        }

        grid.innerHTML = items.map(e => {
            const img = e.imageUrl || 'https://placehold.co/800x450/37281f/ebe825?text=Мероприятие';
            return `
                <div class="info-with-media">
                    <div class="info-with-media__left">
                        <div class="info-box">
                            <div class="info-box__title">${escapeHtml(e.title)}</div>
                            <div class="info-box__content">${escapeHtml(e.description || '')}</div>
                        </div>

                        <div class="info-with-media__bottom">
                            <div class="info-with-media__media">
                                <div class="screenshot-item" style="height:100%;">
                                    <div class="screenshot-item__media" style="aspect-ratio:16/9;">
                                        <img src="${img}" alt="${escapeHtml(e.title)}" loading="lazy">
                                    </div>
                                    <div class="screenshot-item__footer">
                                        <span class="screenshot-item__caption">${escapeHtml(e.title)}</span>
                                        <span class="screenshot-item__tag">${escapeHtml(e.category || 'ФОТО')}</span>
                                    </div>
                                </div>
                            </div>

                            <div class="info-with-media__side">
                                <div class="info-box">
                                    <div class="info-box__title">${formatDate(e.eventDate)}</div>
                                    <div class="info-box__content">
                                        📍 ${escapeHtml(e.location || '—')}<br>
                                        🏆 +${e.pointsReward} опыта<br>
                                        👥 Мест: ${e.capacity}
                                    </div>
                                </div>
                                <div class="info-box">
                                    <div class="info-box__title">Участие</div>
                                    <div class="info-box__content">
                                        Запись открыта для всех студентов колледжа.
                                        После подтверждения организатором — начисляются опыт и жетоны.
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        }).join('');
    } catch (err) {
        console.error(err);
        grid.innerHTML = '<p class="text-error">Ошибка загрузки мероприятий</p>';
    }
}

function escapeHtml(s) {
    return String(s || '')
        .replace(/&/g, '&amp;').replace(/</g, '&lt;')
        .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

function formatDate(iso) {
    if (!iso) return '—';
    const d = new Date(iso);
    return d.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' });
}

document.addEventListener('DOMContentLoaded', loadEventsGrid);