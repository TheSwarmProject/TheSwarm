using System.Xml;
using TheSwarm.Common;

namespace TheSwarm.Configuration.Modules;

public class LoggingConfig
{
    public bool                                 LogToFile               { get; init; } = false;
    public string                               LogsDirectory           { get; init; } = "/SwarmLogs";
    public string                               LogFilePrefix           { get; init; } = "swarm_log";
    public int                                  MaximumLogFileSizeKb    { get; init; } = 4096;
    public int                                  GlobalLogLevelOverride  { get; init; } = 0;

    public Dictionary<string, LoggingLevel>     LoggingChannels         { get; init; } = new Dictionary<string, LoggingLevel>();

    public LoggingConfig(XmlDocument xdoc)
    {
        if (xdoc is null)
            return;

        XmlNode fileNode = xdoc.SelectSingleNode("//Logger/File");
        if (fileNode is null)
            return;
        if (fileNode.Attributes.GetNamedItem("WriteToFile") is not null)
            LogToFile = (Convert.ToBoolean(fileNode.Attributes.GetNamedItem("WriteToFile").Value));
        if (fileNode.Attributes.GetNamedItem("LogsDirectory") is not null)
            LogsDirectory = fileNode.Attributes.GetNamedItem("LogsDirectory").Value;
        if (fileNode.Attributes.GetNamedItem("LogfilePrefix") is not null)
            LogFilePrefix = fileNode.Attributes.GetNamedItem("LogfilePrefix").Value;
        if (fileNode.Attributes.GetNamedItem("MaximumLogSizeKB") is not null)
            MaximumLogFileSizeKb = Int32.Parse(fileNode.Attributes.GetNamedItem("MaximumLogSizeKB").Value);

        foreach (XmlNode node in xdoc.SelectNodes("//Logger/LoggingChannels/Channel"))
        {
            if (node.Attributes.GetNamedItem("LogLevel") is null)
                throw new Exception($"Config property for channel '{node.InnerText}' didn't have LogLevel attribute specified.");

            LoggingLevel level = GlobalLogLevelOverride > 0 ? (LoggingLevel)GlobalLogLevelOverride : (LoggingLevel)Int32.Parse(node.Attributes.GetNamedItem("LogLevel").Value);
            LoggingChannels[node.InnerText] = level;
        }
    }
}