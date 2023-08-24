namespace TheSwarm.Common;

/// <summary>
/// A simple representation of a log message. Initialized by LoggingChannel and passed down to logger's queue for further processing.
/// </summary>
public class LogMessage
{
    public string           Source      { get; private set; } = "";
    public string           Message     { get; private set; } = "";
    public LoggingLevel     Level       { get; private set; } = LoggingLevel.ERROR;
    public string           Timestamp   { get; private set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffff");

    public LogMessage(string source, string message, LoggingLevel level)
    {
        this.Source = source;
        this.Message = message.Replace("\n\n", "\n");
        this.Level = level;
    }
}