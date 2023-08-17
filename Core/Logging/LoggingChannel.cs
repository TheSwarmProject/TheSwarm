namespace TheSwarm.Common;

public class LoggingChannel
{
    private string          ChannelName     { get; set; }
    private LoggingLevel    LoggingLevel    { get; set; }

    public LoggingChannel(string channelName, LoggingLevel loggingLevel)
    {
        this.ChannelName = channelName;
        this.LoggingLevel = loggingLevel;
    }

    public void ChangeLevel(LoggingLevel level) => LoggingLevel = level;

    public void Error(string message)
    {
        if ((int)LoggingLevel >= 1)
        {
            Logger.ReportLog(CreateLogMessage(message, LoggingLevel.ERROR));
        }
    }

    public void Warning(string message)
    {
        if ((int)LoggingLevel >= 2)
        {
            Logger.ReportLog(CreateLogMessage(message, LoggingLevel.WARNING));
        }
    }

    public void Info(string message)
    {
        if ((int)LoggingLevel >= 3)
        {
            Logger.ReportLog(CreateLogMessage(message, LoggingLevel.INFO));
        }
    }

    public void Debug(string message)
    {
        if ((int)LoggingLevel >= 4)
        {
            Logger.ReportLog(CreateLogMessage(message, LoggingLevel.DEBUG));
        }
    }

    public void Trace(string message)
    {
        if ((int)LoggingLevel >= 5)
        {
            Logger.ReportLog(CreateLogMessage(message, LoggingLevel.TRACE));
        }
    }

    private LogMessage CreateLogMessage(string message, LoggingLevel level)
    {
        return new LogMessage(ChannelName, message, level);
    }
}