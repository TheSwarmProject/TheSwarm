using System.Reflection;

namespace TheSwarmClient.Utils;

public static class AttributeHelper
{
    public static bool TypeContainsAttribute(Type type, Type attribute)
    {
        return type.GetCustomAttribute(attribute) != null;
    }
}