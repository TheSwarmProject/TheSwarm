using System.Xml;
using TheSwarm.Configuration.Modules;

namespace TheSwarm.Configuration;

public static class ConfigManager
{
    private const string FILENAME = "SwarmConfig.xml";
    public static GeneralConfig General { get; }
    public static LoggingConfig Logging { get; }

    static ConfigManager()
    {
        if (!File.Exists(FILENAME)) 
        {
            General = new GeneralConfig(null);
            Logging = new LoggingConfig(null);

            return;
        }

        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;
        using (XmlReader reader = XmlReader.Create(FILENAME, readerSettings))
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(reader);

            General = new GeneralConfig(xDoc);
            Logging = new LoggingConfig(xDoc);
        }
    }
}