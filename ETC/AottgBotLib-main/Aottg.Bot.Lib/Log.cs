using System;

namespace AottgBotLib
{
    public static class Log
    {
        public enum LogLevel
        {
            Info,
            Warning,
            Error
        }

        public static Action<string, LogLevel> LogMethod = new Action<string, LogLevel>(test);

        public static void test(string log, LogLevel a)
        {
            Console.WriteLine(log);
        }

        public static void LogInfo(string message)
        {
            LogMethod?.Invoke(message, LogLevel.Info);
        }

        public static void LogWarning(string message)
        {
            LogMethod?.Invoke(message, LogLevel.Warning);
        }

        public static void LogError(string message)
        {
            LogMethod?.Invoke(message, LogLevel.Error);
        }
    }
}