using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class LeaderboardCacheService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    //таблица  лидеров
    public static List<User> TopPlayers { get; private set; } = new();

    public LeaderboardCacheService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<AuthRepository>();

                    // сохраняем массив в память
                    TopPlayers = await repo.GetTop100FromDbAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления кэша рейтинга: {ex.Message}");
            }

            // Ждем 20 минут перед следующим перебором базы
            await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken);
        }
    }
}
