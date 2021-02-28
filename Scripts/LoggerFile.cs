using Godot;
using System;

namespace NightFallAuthenticationServer.Scripts
{
    /// Wrapper around Godot.File class. Manages creation, rotation and deletion of files. Does cyclic writes of information to a disk.
    public sealed class LoggerFile
    {
        private const string FileName = "latest.log";

        private readonly File _fileHandle;

        private readonly string path;

        private int _writeCount;

        private const int MaxWriteCount = 100;
        private bool CanWrite;

        public LoggerFile(string path)
        {
            _writeCount = 0;
            _fileHandle = new File();
            this.path = path;

            CanWrite = ValidatePath();
            RotateFiles();
        }

        /// Opens a logger file handle in write-only mode.
        public void Open()
        {
            _fileHandle.Open(path.PlusFile(FileName), File.ModeFlags.Write);
        }

        /// Flushes gathered output from LoggerFile.Write.
        public void Flush()
        {
            _fileHandle.Flush();
        }

        /// Writes output to a file handle. Flushed the buffer when LoggerFile._writeCount is exceeded.
        public void Write(string output)
        {
            if (!CanWrite) return;
            _fileHandle.StoreLine(output);

            if (_writeCount++ >= MaxWriteCount)
            {
                Flush();
                _writeCount = 0;
            }
        }

        public void Close()
        {
            _fileHandle.Close();
        }

        /// Checks if LoggerFile.path is valid and creates directories recursively.
        /// Returns false if LoggerFile.path is invalid otherwise true.
        private bool ValidatePath()
        {
            if (!(path.IsAbsPath() || path.IsRelPath()))
            {
#if DEBUG
                GD.PushError($"Specified path '{path}' is not valid.");
#endif
                return false;
            }
            var dir = new Directory();
            if (dir.DirExists(path)) return true;
            var error = dir.MakeDirRecursive(path);
            if (error != Error.Ok)
            {
#if DEBUG
                GD.PushError($"Could not create directory {path}. Error code {error}.");
#endif
                return false;
            }
            return true;
        }

        /// Makes sure that LoggerFile always writes to new empty file called LoggerFile.FileName.
        /// If said file is not empty, a rotation is performed.
        /// Rotation is a process of renaming main logger file to `log-YYYY-MM-DD-n.log` where n is a unique unsigned integer.
        private void RotateFiles()
        {
            var fullFilePath = path.PlusFile(FileName);
            if (!_fileHandle.FileExists(fullFilePath)) return;
            var date = GetCurrentLocalDateTimeFromUnixTime(_fileHandle.GetModifiedTime(fullFilePath));

            var year = date.Year;
            var month = date.Month.ToString().PadLeft(2, '0');
            var day = date.Day.ToString().PadLeft(2, '0');

            var newFileBaseName = $"log-{year}-{month}-{day}";
            if (_fileHandle.FileExists(path.PlusFile(newFileBaseName + "-0.log")))
            {
                var index = GetHighestIndexOfLoggerFile(newFileBaseName);
                Rename(path.PlusFile(FileName), path.PlusFile(newFileBaseName + "-" + (index + 1).ToString() + ".log"));
                return;
            }
            Rename(path.PlusFile(FileName), path.PlusFile(newFileBaseName + "-0.log"));
        }

        private DateTime GetCurrentLocalDateTimeFromUnixTime(ulong unixTimestamp)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var newDate = date.AddSeconds(unixTimestamp);
            return newDate.ToLocalTime();
        }

        private void Rename(string from, string to)
        {
            var dir = new Directory();
            dir.Rename(from, to);
        }

        /// Finds the highest index `n` of logger file `log-YYYY-MM-DD-n.log` in directory LoggerFile.path
        private int GetHighestIndexOfLoggerFile(string loggerFileBaseName)
        {
            var dir = new Directory();

            dir.Open(path);
            dir.ListDirBegin(skipNavigational: true, skipHidden: true);
            var length = 0;
            var begin = 0;
            var max = 0;
            string fileName;
            while ((fileName = dir.GetNext()).Length != 0)
            {
                if (!fileName.Match(loggerFileBaseName + "-*.log")) continue;

                begin = loggerFileBaseName.Length + 1;
                length = fileName.Length - begin - 4;

                int index;
                if (!System.Int32.TryParse(fileName.Substr(begin, length), out index)) continue;
                if (index <= max) continue;
                max = index;
            }
            dir.ListDirEnd();

            return max;
        }
    }
}