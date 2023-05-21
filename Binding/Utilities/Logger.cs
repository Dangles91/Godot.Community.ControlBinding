using System.Reflection;

namespace Godot.Community.ControlBinding.Utilities
{
    public static class Logger
    {
        private static readonly Configuration _configuration = new();
        private static readonly string _messagePrefix = typeof(Logger).Assembly.GetName().Name;

        public static void Log(string message)
        {
            if(!_configuration.SuppressAllLogMessages && !_configuration.SuppressInfoLogMessages)
            {
                GD.Print($"INFO: {_messagePrefix}: {message}");
            }
        }

        public static void Warn(string message)
        {
            if(!_configuration.SuppressAllLogMessages && !_configuration.SuppressWarnLogMessage)
            {
                GD.PushWarning($"{_messagePrefix}: {message}");
            }
        }

        public static void Error(string message)
        {
            if(!_configuration.SuppressAllLogMessages && !_configuration.SuppressErrLogMessages)
            {
                GD.PrintErr($"{_messagePrefix}: {message}");
            }
        }
    }
}