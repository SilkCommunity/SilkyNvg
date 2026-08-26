using System;
using System.Diagnostics;
using System.IO;

namespace SilkyNvg.Utils;

internal static class Log
{

    private enum Level
    {
        Debug,
        Info,
        Warn,
        Err
    }

    [Conditional("DEBUG")]
    internal static void Debug(string message, params object[] args)
        => Print(Level.Debug, message, args);
    
    [Conditional("DEBUG")]
    internal static void Info(string message, params object[] args)
        => Print(Level.Info, message, args);

    [Conditional("DEBUG")]
    internal static void Warn(string message, params object[] args)
        => Print(Level.Warn, message, args);
    
    [Conditional("DEBUG")]
    internal static void Error(string message, params object[] args)
        => Print(Level.Err, message, args);
    
    [Conditional("DEBUG")]
    private static void Print(Level level, string message, params object[] args)
    {
        message = string.Format(message, args);

        var c = level switch
        {
            Level.Debug => 'D',
            Level.Info => 'I',
            Level.Warn => 'W',
            Level.Err => 'E',
            _ => '?'
        };

        var trace = new StackTrace(fNeedFileInfo: true);
        var frame = trace.GetFrame(2);
        
        var fileName = frame?.GetFileName() ?? "<unknown>";
        if (fileName != "<unknown>")
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
        }
        var lineNumber = frame?.GetFileLineNumber() ?? -1;
        
        string logLine = $"[{c}] {fileName}:{lineNumber} {message}";

        Console.WriteLine(logLine);
    }
    
}