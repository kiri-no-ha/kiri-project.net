public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";

    // Новая статистика студента
    public int Points { get; set; }         // баллы
    public int Attendance { get; set; }     // посещения
    public int Gaps { get; set; }           // пропуски
    public int Events { get; set; }         // мероприятия
    public int Course { get; set; }         // курс
}