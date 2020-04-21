namespace DotaDb.Models
{
    public class PlayerCountModel
    {
        public int InGamePlayerCount { get; set; }
        public int DailyPeakPlayerCount { get; set; }
        public int AllTimePeakPlayerCount { get; set; }
    }
}