
using Godot;

namespace NightFallAuthenticationServer.Scripts.AutoLoad
{
    public sealed class Logger : Node
    {
        // one instance of LoggerFile for every Logger instance just in case
        private static LoggerFile _loggerFile = new LoggerFile("user://logs/");
        private static BasicLogger _main;
        private static BasicLogger _server;
        public static BasicLogger Server => _server;

        public Logger()
        {
            // _loggerFile passed by reference to always write to the same file
            _main = new BasicLogger(ref _loggerFile, "MAIN");
            _server = new BasicLogger(ref _loggerFile, "SERVER");
        }
        public override void _EnterTree()
        {
            _loggerFile.Open();
        }
        public static void Verbose(string output)
        {
            _main.Verbose(output);
        }
        public static void Debug(string output)
        {
            _main.Debug(output);
        }
        public static void Info(string output)
        {
            _main.Info(output);
        }
        public static void Warn(string output)
        {
            _main.Warn(output);
        }
        public static void Error(string output)
        {
            _main.Error(output);
        }
        public override void _ExitTree()
        {
            _loggerFile.Close();
        }

    }
}