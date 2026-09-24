# Campus Platform

Веб-платформа для управления студенческими мероприятиями и мерчем.  
Учебный проект, реализованный в рамках хакатона.

## Возможности

- Регистрация и вход студентов
- Личный кабинет со статистикой (баллы, посещаемость, мероприятия)
- Запись на мероприятия колледжа
- Магазин мерча с корзиной и оплатой баллами
- Панель организатора: создание и удаление мероприятий
- Панель администратора: управление мерчем и статистикой

## Стек

| Слой | Технологии |
|------|-----------|
| Backend | C# 12, .NET 8, ASP.NET Core Web API |
| База данных | MySQL 8.0 |
| Доступ к данным | ADO.NET (MySql.Data) |
| Email | MailKit (SMTP) |
| API-документация | Swagger / Swashbuckle |
| Frontend | HTML5, CSS3, vanilla JavaScript (ES6+) |
| Хранение на клиенте | localStorage, sessionStorage |

## Структура
├── back/ # Backend (.NET 8)
│ ├── Controllers/
│ ├── Models/
│ ├── Repositories/
│ └── Services/
├── front/ # Frontend
│ ├── css/
│ ├── js/
│ ├── img/
│ └── *.html
└── db/ # Дамп базы данных

## Запуск

### Требования
- .NET 8 SDK
- MySQL 8.0
- Современный браузер

### Backend

```bash
cd back
dotnet restore
dotnet run
