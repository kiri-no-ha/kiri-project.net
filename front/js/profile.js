// ========================
// Инициализация
// ========================
const currentUser    = sessionStorage.getItem('username') || '';
const navUsername    = document.getElementById('navUsername');
const profileSkeleton  = document.getElementById('profileSkeleton');
const profileContent   = document.getElementById('profileContent');
const profileNotFound  = document.getElementById('profileNotFound');

if (currentUser) navUsername.textContent = currentUser;

// ========================
// Форматирование времени
// ========================
function formatTime(minutes) {
  if (minutes < 60) return `${minutes}м`;
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  return m > 0 ? `${h}ч ${m}м` : `${h}ч`;
}

// ========================
// Отрисовка профиля
// ========================
function renderProfile(player, rank) {
  // Аватар — первая буква username
  document.getElementById('profileAvatar').textContent =
    player.username.charAt(0).toUpperCase();

  document.getElementById('profileName').textContent = player.username;

  // Ранг в топе
  const rankText = rank <= 3
    ? ['🥇 1 место', '🥈 2 место', '🥉 3 место'][rank - 1]
    : `# ${rank} в рейтинге`;
  document.getElementById('profileRank').textContent = rankText;

  document.getElementById('statWins').textContent   = player.wins;
  document.getElementById('statLosses').textContent = player.losses;
  document.getElementById('statTotal').textContent  = player.totalGames;
  document.getElementById('statTime').textContent   = formatTime(player.playtimeMinutes);

  // Винрейт
  const winrate = player.totalGames > 0
    ? Math.round((player.wins / player.totalGames) * 100)
    : 0;

  document.getElementById('winrateLabel').textContent = `${winrate}%`;

  // Анимируем полоску с задержкой
  setTimeout(() => {
    document.getElementById('winrateBar').style.width = `${winrate}%`;
  }, 100);

  // Показываем профиль
  profileSkeleton.classList.add('hidden');
  profileContent.classList.remove('hidden');
}

// ========================
// Загрузка
// ========================
async function loadProfile() {
  if (!currentUser) {
    profileSkeleton.classList.add('hidden');
    profileNotFound.classList.remove('hidden');
    return;
  }

  try {
    const res = await fetch(`${CONFIG.API}/leaderboard`);

    if (!res.ok) throw new Error();

    const players = await res.json();

    // Ищем игрока по username (без регистра)
    const index = players.findIndex(
      p => p.username.toLowerCase() === currentUser.toLowerCase()
    );

    if (index === -1) {
      // Игрок ещё не сыграл ни одной игры — показываем что статистики нет
      profileSkeleton.classList.add('hidden');
      profileNotFound.classList.remove('hidden');
      profileNotFound.querySelector('p').textContent =
        'Сыграй первую игру, чтобы появиться в рейтинге.';
      profileNotFound.querySelector('a').classList.add('hidden');
      return;
    }

    renderProfile(players[index], index + 1);

  } catch {
    profileSkeleton.classList.add('hidden');
    profileNotFound.classList.remove('hidden');
    profileNotFound.querySelector('p').textContent =
      'Ошибка загрузки. Попробуй позже.';
  }
}

loadProfile();
