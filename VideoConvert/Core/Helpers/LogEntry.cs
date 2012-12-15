using System;

namespace VideoConvert.Core.Helpers
{
    public class LogEntry
    {
        public DateTime EntryTime { get; set; }
        public string JobName { get; set; }
        public string LogText { get; set; }
    }
}