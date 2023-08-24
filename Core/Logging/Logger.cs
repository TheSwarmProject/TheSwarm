using TheSwarm.Configuration;

namespace TheSwarm.Common;

public static class Logger
{
    private static Dictionary<string, LoggingChannel>   loggingChannels         = new Dictionary<string, LoggingChannel>();
    private static LoggingChannel                       log                     { get; set; }
    private static LoggingLevel                         defaultLoggingLevel     { get; set; }
    public static string                                LogsDirectory           { get; private set; } = "";
    private static string                               logFilePrefix           { get; set; } = "";
    private static int                                  logFileSizeLimit        { get; set; } = 1024;
    private static int                                  currentFileSize         { get; set; } = 0;
    private static bool                                 writeToFile             { get; set; } = false;
    private static bool                                 fileLocked              { get; set; } = false;

    private static string                               currentLogFileName      { get; set; } = "";

    // Since there was a notable loss of data saved to file, we use queue to keep things things uniform
    // Probably a bit of an overkill, but it doesn't seem to consume overly much resources - all processes are trivial
    private static Queue<LogMessage>                    messagesToProcess       { get; set; } = new Queue<LogMessage>();

    static Logger()
    {
        defaultLoggingLevel = LoggingLevel.INFO;
        log = new LoggingChannel("LoggingManager", defaultLoggingLevel);
        LogsDirectory = ConfigManager.Logging.LogsDirectory;
        logFilePrefix = ConfigManager.Logging.LogFilePrefix;
        logFileSizeLimit = ConfigManager.Logging.MaximumLogFileSizeKb;
        writeToFile = ConfigManager.Logging.LogToFile;
    }

    /// <summary>
    /// Creates a logging channel with given name and default logging level.
    /// If channel with this name already exist - we do not re-create it.
    /// </summary>
    /// <param name="channelName">Name of the channel to create</param>
    /// <returns>LoggingChannel instance</returns>
    public static LoggingChannel CreateChannel(string channelName)
    {
        if (loggingChannels.ContainsKey(channelName))
        {
            log.Warning($"Channel with name ${channelName} already exist. Ignoring");
            return loggingChannels[channelName];
        }
        else
        {
            LoggingChannel channel = new LoggingChannel(channelName, defaultLoggingLevel);
            loggingChannels[channelName] = channel;
            return channel;
        }
    }

    /// <summary>
    /// Overload for CreateChannel - takes logging level as a second argument.
    /// If said channel already exist - it finds it, sets logging level and returns as a ret.val.
    /// </summary>
    /// <param name="channelName">Name of the channel to create</param>
    /// <param name="loggingLevel">Logging level for channel</param>
    /// <returns>LoggingChannel instance</returns>
    public static LoggingChannel CreateChannel(string channelName, LoggingLevel loggingLevel)
    {
        if (loggingChannels.ContainsKey(channelName))
        {
            log.Warning($"Channel with name ${channelName} already exist. Ignoring");
            return loggingChannels[channelName];
        }
        else
        {
            LoggingLevel level = ConfigManager.Logging.LoggingChannels.ContainsKey(channelName) ? ConfigManager.Logging.LoggingChannels[channelName] : defaultLoggingLevel;
            LoggingChannel channel = new LoggingChannel(channelName, level);
            loggingChannels[channelName] = channel;
            return channel;
        }
    }

    /// <summary>
    /// Finds and returns a Logging channel instance. If it doesn't exist - it will create a new one with ERROR level
    /// </summary>
    /// <param name="name">Name of the channel</param>
    /// <returns>LoggingChannel instance</returns>
    public static LoggingChannel GetLogger(string name)
    {
        if (loggingChannels.ContainsKey(name))
        {
            return loggingChannels[name];
        }
        else
        {
            log.Debug($"No channel with name '{name}' was found. Creading default one with ERROR logging level");
            return new LoggingChannel(name, LoggingLevel.ERROR);
        }
    }

    /// <summary>
    /// Main external callable - used to receive LogMessage from log channels and put it into queue
    /// </summary>
    /// <param name="message">LogMessage to process</param>
    public static void ReportLog(LogMessage message)
    {
        messagesToProcess.Enqueue(message);
        if (!fileLocked)
        {
            fileLocked = true;
            while (messagesToProcess.Count > 0)
            {
                ProcessMessage(messagesToProcess.Dequeue());
            }
            fileLocked = false;
        }
    }

    /// <summary>
    /// Main workhorse - processes LogMessage instance, picked from the queue
    /// </summary>
    /// <param name="message">Message to process</param>
    private static void ProcessMessage(LogMessage message)
    {
        if (message is null)
            return;
        switch (message.Level)
        {
            case LoggingLevel.ERROR:
                PrintWithColor(message, ConsoleColor.Red);
                PrintIntoFile(message);
                break;
            case LoggingLevel.WARNING:
                PrintWithColor(message, ConsoleColor.Yellow);
                PrintIntoFile(message);
                break;
            case LoggingLevel.INFO:
                PrintWithColor(message, ConsoleColor.White);
                PrintIntoFile(message);
                break;
            case LoggingLevel.DEBUG:
                PrintWithColor(message, ConsoleColor.Gray);
                PrintIntoFile(message);
                break;
            case LoggingLevel.TRACE:
                PrintWithColor(message, ConsoleColor.Magenta);
                PrintIntoFile(message);
                break;
            default:
                PrintWithColor(message, ConsoleColor.White);
                PrintIntoFile(message);
                break;
        }
    }

    /// <summary>
    /// Formats and prints the log message into the console.
    /// </summary>
    /// <param name="message">Log message to process</param>
    /// <param name="color">ConsoleColor to be used for level, source and message itself. Differs depending on the level</param>
    private static void PrintWithColor(LogMessage message, ConsoleColor color)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("{0,-24} ", message.Timestamp);
        Console.ForegroundColor = color;
        Console.Write("{0,-7} {1,-25}: {2}\n", $"[{message.Level.ToString()}]", $"({message.Source})", message.Message);
        Console.ResetColor();
    }

    /// <summary>
    /// File processor - creates directory and log file (if it doesn't exist), puts log data into it and keeps track of
    /// file's size. If it begins to exceed the limit - it creates a new file.
    /// /// </summary>
    /// <param name="message">LogMessage to process</param>
    private static void PrintIntoFile(LogMessage message)
    {
        if (writeToFile)
        {
            if (currentLogFileName == "" || currentFileSize >= logFileSizeLimit * 1024)
            {
                Directory.CreateDirectory(LogsDirectory);
                currentLogFileName = $"{LogsDirectory}/{logFilePrefix}_{DateTime.UtcNow.ToString("MM-dd-yy_HH-mm-ss.ffff")}.log";
                File.Create(currentLogFileName).Close();
                currentFileSize = 0;
            }

            string msg = String.Format("{0,-24} {1,-7} {2,-25}: {3}\n", message.Timestamp, $"[{message.Level.ToString()}]", $"({message.Source})", message.Message);
            File.AppendAllText(currentLogFileName, msg);
            currentFileSize += msg.Length;
        }
    }
}

public enum LoggingLevel
{
    NONE = 0,
    ERROR = 1,
    WARNING = 2,
    INFO = 3,
    DEBUG = 4,
    TRACE = 5
}