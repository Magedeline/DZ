using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Celeste;

/// <summary>Shared YAML deserializer for all metadata registries</summary>
internal static class RegistryDeserializer
{
    private static readonly IDeserializer _deserializer =
        new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

    public static IDeserializer GetDeserializer() => _deserializer;
}
