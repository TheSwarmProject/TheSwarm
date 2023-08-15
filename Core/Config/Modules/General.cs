using System.Xml;

namespace TheSwarmClient.Configuration.Modules;

public class GeneralConfig
{
    public string ResultsLocation { get; init; } = "SwarmResults";

    public GeneralConfig(XmlDocument xdoc)
    {
        if (xdoc is null)
            return;

        XmlNode fileNode = xdoc.SelectSingleNode("//General/ResultsFolder");
        if (fileNode is null)
            return;
        ResultsLocation = fileNode.InnerText;
    }
}