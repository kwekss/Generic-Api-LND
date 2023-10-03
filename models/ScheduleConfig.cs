namespace models
{
    public class ScheduleConfig
    {
        public string Type { get; set; }
        public int Interval { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public string ExcludeDays { get; set; }
        public bool Active { get; set; }
    }

    public class TaskCompletedResponse
    {
        public bool Success { get; set; }
        public dynamic Item { get; set; }
    }
}
