/// <summary>
/// Interface dùng chung cho tất cả các Boss để các script hệ thống (như BossLanding) 
/// có thể điều khiển mà không cần biết cụ thể đó là Boss nào.
/// </summary>
public interface IBossController
{
    bool isBattleStarted { get; set; }
    bool enabled { get; set; }
}
