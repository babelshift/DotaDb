namespace DotaDb.Models
{
    public class PlayerCountModel
    {
        public int InGamePlayerCount { get; internal set; }
        public int DailyPeakPlayerCount { get; internal set; }
        public int AllTimePeakPlayerCount { get; internal set; }
    }
}