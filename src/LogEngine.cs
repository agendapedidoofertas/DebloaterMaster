using System;
using System.Collections.Generic;

namespace DebloaterPC
{
    public class LogEntry
    {
        public string Text { get; set; }
        public string Type { get; set; } // "", "info", "success", "warning", "danger"
        public DateTime Timestamp { get; set; }

        public LogEntry(string text, string type = "")
        {
            Text = text;
            Type = type;
            Timestamp = DateTime.Now;
        }
    }

    public static class LogEngine
    {
        private static readonly object _lock = new object();
        private static readonly List<LogEntry> _logs = new List<LogEntry>();
        public static int CurrentProgress { get; set; }
        public static string CurrentTask { get; set; }
        public static bool IsExecuting { get; set; }

        static LogEngine()
        {
            CurrentProgress = 0;
            CurrentTask = "Pronto";
            IsExecuting = false;
        }

        public static void Log(string text, string type = "")
        {
            lock (_lock)
            {
                var formatted = string.Format("[{0:HH:mm:ss}] {1}", DateTime.Now, text);
                _logs.Add(new LogEntry(formatted, type));
                Console.WriteLine(formatted);
            }
        }

        public static void SetProgress(int percent, string task = null)
        {
            CurrentProgress = Math.Min(100, Math.Max(0, percent));
            if (task != null)
            {
                CurrentTask = task;
                Log(task, "info");
            }
        }

        public static List<LogEntry> GetLogsFrom(int offset, out int nextOffset)
        {
            lock (_lock)
            {
                var result = new List<LogEntry>();
                if (offset < _logs.Count)
                {
                    for (int i = offset; i < _logs.Count; i++)
                    {
                        result.Add(_logs[i]);
                    }
                }
                nextOffset = _logs.Count;
                return result;
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _logs.Clear();
            }
        }
    }
}
